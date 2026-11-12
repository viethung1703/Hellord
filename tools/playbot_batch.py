#!/usr/bin/env python3
"""Batch runner for tools/playbot.py: N bot runs of a Linux build on a private Xvfb display, per-run stats + summary.
Usage: playbot_batch.py --build linux_v0.0.20 --seeds 1 2 3 --out DIR [--display :12] [--jobs 1] [--maxt 900] [--rec] [--nocrt]
Each run: own -botstate file and -logFile; window screenshots on anomalies; optional x11grab clip cropped to the window."""
import argparse, csv, json, os, subprocess, sys, time, threading
B = '/workspace/Hellord/Builds'; BOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), 'playbot.py')
ap = argparse.ArgumentParser()
ap.add_argument('--build', default='linux_v0.0.20'); ap.add_argument('--seeds', type=int, nargs='+', default=[1, 2, 3])
ap.add_argument('--out', required=True); ap.add_argument('--display', default=':12'); ap.add_argument('--jobs', type=int, default=1)
ap.add_argument('--maxt', type=float, default=900); ap.add_argument('--rec', action='store_true'); ap.add_argument('--nocrt', action='store_true')
ap.add_argument('--boss-wait-max', type=float, default=90)
A = ap.parse_args(); os.makedirs(A.out, exist_ok=True)

def ensure_xvfb(disp):
    n = disp.lstrip(':')
    if os.path.exists(f'/tmp/.X11-unix/X{n}'): return None
    p = subprocess.Popen(['Xvfb', disp, '-screen', '0', '1600x900x24', '-ac', '+extension', 'GLX', '+render', '-noreset'], stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
    for _ in range(50):
        if os.path.exists(f'/tmp/.X11-unix/X{n}'): break
        time.sleep(0.1)
    return p

def one(seed, disp):
    tag = f's{seed}'; od = os.path.abspath(os.path.join(A.out, tag)); os.makedirs(od, exist_ok=True)
    env = {**os.environ, 'DISPLAY': disp}; sf = f'/tmp/pbv2_{os.getpid()}_{tag}.json'
    exe = os.path.join(B, A.build, 'Hellord.x86_64')
    args = [exe, '-screen-fullscreen', '0', '-screen-width', '1280', '-screen-height', '720', '-window-mode', 'windowed', '-volume', '0',
            '-logFile', os.path.join(od, 'player.log'), '-botstate', sf] + (['-nocrt'] if A.nocrt else [])
    g = subprocess.Popen(args, cwd=os.path.dirname(exe), env=env, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
    W = None
    for _ in range(80):
        r = subprocess.run(['xdotool', 'search', '--pid', str(g.pid)], env=env, capture_output=True, text=True).stdout.split()
        if r: W = r[-1]; break
        time.sleep(0.5)
    rec = None
    try:
        if not W: raise RuntimeError('no window')
        time.sleep(4); subprocess.run(['xdotool', 'windowmove', W, '0', '0'], env=env); subprocess.run(['xdotool', 'windowfocus', W], env=env); time.sleep(1)
        if A.rec:
            geo = dict(l.split('=', 1) for l in subprocess.run(['xdotool', 'getwindowgeometry', '--shell', W], env=env, capture_output=True, text=True).stdout.split() if '=' in l)
            rec = subprocess.Popen(['ffmpeg', '-loglevel', 'error', '-y', '-f', 'x11grab', '-framerate', '15', '-video_size', f"{geo['WIDTH']}x{geo['HEIGHT']}",
                                    '-i', f"{disp}+{geo['X']},{geo['Y']}", '-c:v', 'libx264', '-preset', 'ultrafast', '-crf', '28', os.path.join(od, 'clip.mkv')], stdin=subprocess.PIPE)
        subprocess.run([sys.executable, BOT, sf, W, str(A.maxt), os.path.join(od, 'bot.log'), '--seed', str(seed), '--stats', os.path.join(od, 'stats.json'),
                        '--shots', os.path.join(od, 'shots'), '--display', disp, '--boss-wait-max', str(A.boss_wait_max)], env=env, stdout=subprocess.DEVNULL, timeout=A.maxt + 120)
    except Exception as ex:
        json.dump({'seed': seed, 'result': 'harness_error: ' + str(ex)}, open(os.path.join(od, 'stats.json'), 'w'))
    finally:
        if rec:
            try: rec.communicate(b'q', timeout=15)
            except Exception: rec.kill()
        g.terminate()
        try: g.wait(10)
        except Exception: g.kill()
    try: return json.load(open(os.path.join(od, 'stats.json')))
    except Exception: return {'seed': seed, 'result': 'no_stats'}

xs = []; disps = [A.display] if A.jobs == 1 else [f':{int(A.display.lstrip(":")) + k}' for k in range(A.jobs)]
for d in disps: xs.append(ensure_xvfb(d))
results = {}; queue = list(A.seeds); lock = threading.Lock()
def worker(d):
    while True:
        with lock:
            if not queue: return
            sd = queue.pop(0)
        print(time.strftime('%H:%M:%S'), 'start seed', sd, 'on', d, flush=True); r = one(sd, d); results[sd] = r
        print(time.strftime('%H:%M:%S'), 'seed', sd, '->', r.get('result'), flush=True)
ts = [threading.Thread(target=worker, args=(d,)) for d in disps]
for t in ts: t.start()
for t in ts: t.join()
for x in xs:
    if x: x.terminate()

rows = []
for sd in A.seeds:
    r = results.get(sd, {}); hpr = r.get('hits_per_room') or []
    rows.append({'seed': sd, 'result': r.get('result'), 'run_s': r.get('run_s'), 'rooms_cleared': r.get('n_rooms_cleared'),
                 'cleared_list': ' '.join(r.get('rooms_cleared') or []), 'death_cause': r.get('death_cause'), 'death_room': r.get('death_room'),
                 'hits_total': r.get('hits_total'), 'hits_per_room': ' '.join(f"{v['type']}:{v['hits']}" for v in hpr),
                 'boss_spawn_latency_s': r.get('boss_spawn_latency_s'), 'boss_ttk_s': r.get('boss_ttk_s'), 'boss_hits': r.get('boss_hits'),
                 'stuck_events': r.get('stuck_events'), 'level': r.get('level'), 'kills': r.get('kills'), 'anomalies': ' '.join(r.get('anomalies') or [])})
json.dump({'build': A.build, 'runs': [results.get(sd) for sd in A.seeds]}, open(os.path.join(A.out, 'results.json'), 'w'), indent=1)
with open(os.path.join(A.out, 'results.csv'), 'w', newline='') as f:
    w = csv.DictWriter(f, fieldnames=list(rows[0].keys())); w.writeheader(); w.writerows(rows)
cols = ['seed', 'result', 'run_s', 'rooms_cleared', 'death_cause', 'hits_total', 'boss_spawn_latency_s', 'boss_ttk_s', 'boss_hits', 'stuck_events', 'level', 'anomalies']
md = [f'# playbot batch {A.build} ({time.strftime("%Y-%m-%d %H:%M")})', '', '| ' + ' | '.join(cols) + ' |', '|' + '---|' * len(cols)]
md += ['| ' + ' | '.join(str(r[c]) for c in cols) + ' |' for r in rows]
md += ['', 'Hits per room visit:'] + [f"- s{r['seed']}: {r['hits_per_room']}" for r in rows]
open(os.path.join(A.out, 'summary.md'), 'w').write('\n'.join(md) + '\n'); print('\n'.join(md))
