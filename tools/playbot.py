#!/usr/bin/env python3
"""Hellord play bot (v2, hardened for linux_v0.0.19+). Plays the REAL Linux player through xdotool key holds
(no debug keys) and reads the read-only '-botstate' JSON snapshot the game writes (~20 Hz).
See tools/PLAYBOT_README.md.  Back-compatible CLI:
  playbot.py STATEFILE WINDOWID [maxSeconds] [logfile] [--seed N] [--stats out.json] [--shots DIR]
             [--boss-wait-max S] [--display :N]
DISPLAY: --display, else $DISPLAY, else :10.
"""
import argparse, json, math, os, random, subprocess, sys, time

ap = argparse.ArgumentParser()
ap.add_argument('state'); ap.add_argument('win'); ap.add_argument('maxt', nargs='?', type=float, default=1800)
ap.add_argument('log', nargs='?', default='/tmp/playbot.log')
ap.add_argument('--seed', type=int, default=None); ap.add_argument('--stats', default=None)
ap.add_argument('--shots', default=None, help='dir for anomaly screenshots (boss wait, stuck)')
ap.add_argument('--boss-wait-max', type=float, default=90.0, help='abort as boss_no_spawn after this many s in an uncleared boss room with no boss')
ap.add_argument('--display', default=None)
A = ap.parse_args()
STATE, WIN, MAXT = A.state, A.win, A.maxt
DISPLAY = A.display or os.environ.get('DISPLAY') or ':10'
LOG = open(A.log, 'a')
SEED = A.seed if A.seed is not None else int(time.time()) % 100000
random.seed(SEED)
T0 = time.time()
def log(*a):
    s = time.strftime('%H:%M:%S') + f' [{time.time()-T0:7.1f}] ' + ' '.join(str(x) for x in a)
    print(s, flush=True); LOG.write(s + '\n'); LOG.flush()

def xdo(*a): subprocess.run(['xdotool', *a], env={**os.environ, 'DISPLAY': DISPLAY}, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
held = set()
def focus(): xdo('windowactivate', '--sync', WIN)
def set_keys(want):
    global held
    up, down = held - want, want - held
    if not up and not down: return
    args = []
    for k in up: args += ['keyup', k]
    for k in down: args += ['keydown', k]
    xdo(*args); held = set(want)
def tap(k):
    set_keys(set()); xdo('key', '--delay', '60', k); time.sleep(0.12)

def screenshot(tag):
    """Grab the game window only (x11grab cropped to the window geometry)."""
    if not A.shots: return None
    try:
        os.makedirs(A.shots, exist_ok=True)
        g = subprocess.run(['xdotool', 'getwindowgeometry', '--shell', WIN], env={**os.environ, 'DISPLAY': DISPLAY}, capture_output=True, text=True).stdout
        d = dict(l.split('=', 1) for l in g.split() if '=' in l)
        out = os.path.join(A.shots, f'{time.time()-T0:07.1f}_{tag}.png')
        subprocess.run(['ffmpeg', '-loglevel', 'error', '-y', '-f', 'x11grab', '-video_size', f"{d['WIDTH']}x{d['HEIGHT']}", '-i', f"{DISPLAY}+{d['X']},{d['Y']}", '-frames:v', '1', out], timeout=15)
        log('SHOT', out); return out
    except Exception as ex:
        log('shot failed', ex); return None

def read():
    try:
        with open(STATE) as f: return json.load(f)
    except Exception: return None

def v(x, y): return [x, y]
def ln(a): return math.hypot(a[0], a[1])
def nrm(a):
    l = ln(a); return [a[0] / l, a[1] / l] if l > 1e-6 else [0.0, 0.0]

SKILL_PREF = ['Fireball', 'Storm Ring', 'Bone Orbit', 'Frost Cleave', 'Curse Rune']
SUP_PREF = ['Added Damage', 'Faster Casting', 'Multi Projectiles', 'More Orbitals', 'Pierce', 'Bigger Area', 'Echo', 'Homing', 'Burn', 'Shock', 'Fork', 'Bounce', 'Chill', 'Bleed']
STAT_PREF = ['Vitality', 'Hide', 'Swiftness', 'Malice', 'Hunger']
def card_score(c, s):
    kind, name = c
    if kind == 'NewSkill': return 90 - SKILL_PREF.index(name) * 3 if name in SKILL_PREF else 60
    if kind == 'SkillLevel':
        return 85 if (s.get('skills') and name == s['skills'][0][0]) else 72
    if kind == 'Support': return 65 - (SUP_PREF.index(name) if name in SUP_PREF else 20)
    if kind == 'Stat':
        hpf = s['hp'] / max(1, s['maxHp'])
        b = 50 - STAT_PREF.index(name) * 4 if name in STAT_PREF else 30
        if name == 'Vitality' and hpf < 0.6: b += 30
        return b
    return 0

def bfs_path(floor, a, goal):
    prev = {a: None}; q = [a]
    while q:
        c = q.pop(0)
        if c == goal: break
        for n in floor[c][5]:
            if n >= 0 and n not in prev: prev[n] = c; q.append(n)
    if goal not in prev: return None
    p = [goal]
    while prev[p[-1]] is not None: p.append(prev[p[-1]])
    return p[::-1]

def bfs_path(floor, a, goal):
    prev = {a: None}; q = [a]
    while q:
        c = q.pop(0)
        if c == goal: break
        for n in floor[c][5]:
            if n >= 0 and n not in prev: prev[n] = c; q.append(n)
    if goal not in prev: return None
    p = [goal]
    while prev[p[-1]] is not None: p.append(prev[p[-1]])
    return p[::-1]

INWARD = {0: (0, -1), 1: (-1, 0), 2: (0, 1), 3: (1, 0)}   # door dir N,E,S,W -> step into the room (y up)
MAIN = ('Survive', 'Hunt', 'Nests')
class Bot:
    def __init__(s):
        s.orbit = 1; s.flip_t = 0; s.last_mode = None; s.stats = {}; s.shop_done = set(); s.last_room = None
        s.hp_min = 999; s.stuck = 0; s.lastpos = None; s.wander = random.random() * 6.28; s.target_room = None
        s.last_card_t = 0; s.quiet_since = time.time(); s.hist = []; s.esc_until = 0; s.esc_n = 0; s.esc_dir = [0, 0]
        s.unstick_times = []; s.wp_until = 0; s.wp = None; s.shop_enter_t = None
        s.boss_room_t = None; s.boss_seen_t = None; s.boss_wait_shots = set(); s.prev_hp = None; s.last_hit = None
        s.visits = []; s.cur_visit = None; s.run0 = None
        s.R = {'seed': SEED, 'result': None, 'run_s': None, 'wall_s': None, 'layout': None, 'rooms_cleared': [], 'n_rooms_cleared': 0,
               'death_cause': None, 'death_room': None, 'hits_total': 0, 'hp_lost_total': 0.0, 'hits_per_room': [],
               'boss_enter_run_s': None, 'boss_spawn_latency_s': None, 'boss_ttk_s': None, 'boss_hits': 0, 'stuck_events': 0,
               'stuck_log': [], 'shop_buys': [], 'level': None, 'kills': None, 'anomalies': []}

    # ---------------- routing ----------------
    def choose_target(s, st):
        fl = st['floor']; cur = st['room']['i']
        unc = [i for i, f in enumerate(fl) if f[2] in MAIN and not f[4]]
        if unc:
            return min(unc, key=lambda i: len(bfs_path(fl, cur, i) or [0] * 99))
        shr = [i for i, f in enumerate(fl) if f[2] == 'Shrine' and i not in s.shop_done]
        if shr and st['bones'] >= 25: return shr[0]
        boss = [i for i, f in enumerate(fl) if f[2] == 'Boss']
        return boss[0] if boss else None

    def door_force(s, st):
        h = st['hero']; room = st['room']
        if room['locked'] or room['active']: return None
        lo, hi = st['lo'], st['hi']
        if room['type'] == 'Boss' and room['cleared'] and not st.get('boss'):
            c = [(lo[0] + hi[0]) / 2, (lo[1] + hi[1]) / 2]     # stairs = room centre (D.w/2, D.h/2+16 px)
            if s.target_room != 'stairs': s.target_room = 'stairs'; log('route -> stairs', [round(c[0], 2), round(c[1], 2)])
            return [c[0] - h[0], c[1] - h[1]]
        tgt = s.choose_target(st)
        if tgt is None: return None
        if room['type'] == 'Shrine' and room['i'] not in s.shop_done: return None
        path = bfs_path(st['floor'], room['i'], tgt)
        if not path or len(path) < 2: return None
        nxt = path[1]; door = next((d for d in st['doors'] if d['to'] == nxt), None)
        if not door: return None
        if s.target_room != tgt: s.target_room = tgt; log('route ->', st['floor'][tgt][2], 'via', door['type'], 'dir', 'NESW'[door['dir']])
        # two-stage approach: (1) a waypoint 2.5 u inside the door on its axis, (2) push straight through the doorway
        iw = INWARD[door['dir']]
        dx, dy = door['x'] - h[0], door['y'] - h[1]
        wp = [min(max(door['x'] + iw[0] * 2.5, lo[0] + 0.3), hi[0] - 0.3), min(max(door['y'] + iw[1] * 2.5, lo[1] + 0.3), hi[1] - 0.3)]
        off_axis = abs(dx) if door['dir'] in (0, 2) else abs(dy)
        if off_axis > 0.5:
            F = [wp[0] - h[0], wp[1] - h[1]]
        else:
            F = [-iw[0] * 1.0 + (dx * 1.5 if door['dir'] in (0, 2) else 0), -iw[1] * 1.0 + (dy * 1.5 if door['dir'] in (1, 3) else 0)]
        sh = st.get('shop')
        if sh:  # walk around the shrine statue (solid) instead of into it
            sx, sy = sh['x'], sh['y'] + 1.0
            rx, ry = h[0] - sx, h[1] - sy; rd = math.hypot(rx, ry)
            if rd < 3.2:
                k = (3.2 - rd) * 2.5; F = [F[0] + rx / max(rd, .1) * k, F[1] + ry / max(rd, .1) * k + 1.5]
        return F

    def boss_wait(s, st):
        """Uncleared boss room, no boss in the snapshot: hold the v0.7 arena start spot (south centre) and record evidence."""
        h = st['hero']; lo, hi = st['lo'], st['hi']; now = time.time()
        if s.boss_room_t is None: return None
        w = now - s.boss_room_t
        for mark in (2, 10, 30, 60):
            if w >= mark and mark not in s.boss_wait_shots:
                s.boss_wait_shots.add(mark); log(f'BOSS-WAIT {mark}s: no boss in botstate; hero {h} room {st["room"]} en {len(st["en"])}'); screenshot(f'bosswait_{mark}s')
        spot = [(lo[0] + hi[0]) / 2, -580 / 16.0]
        d = [spot[0] - h[0], spot[1] - h[1]]
        if ln(d) < 0.6: s.wander += 0.4; return [math.cos(s.wander) * 0.3, math.sin(s.wander) * 0.3]
        return d

    def shop(s, st):
        sh = st.get('shop'); room = st['room']
        if not sh or room['i'] in s.shop_done: return None
        h = st['hero']
        if s.shop_enter_t is None: s.shop_enter_t = time.time()
        if time.time() - s.shop_enter_t > 25:
            log('SHOP give-up: could not reach shrine in 25 s'); s.R['anomalies'].append('shop_unreachable'); s.shop_done.add(room['i']); return None
        if not sh['near']:
            # shop.y is already 40 px south of the statue
            tgt = [sh['x'], sh['y']]
            return [tgt[0] - h[0], tgt[1] - h[1]]
        for i, it in enumerate(sh['items']):
            name, price, sold, tag = it
            if not sold and tag != 'NO FIT' and st['bones'] >= price:
                log('SHOP buy', name, price, 'bones', st['bones']); s.R['shop_buys'].append([name, price]); tap(str(i + 1)); return [0, 0]
        log('SHOP done; bones', st['bones'], 'items', sh['items']); s.shop_done.add(room['i']); return None

    def keys_for(s, F, push=False):
        n = nrm(F); k = set()
        th = 0.38
        if n[0] > th: k.add('d')
        if n[0] < -th: k.add('a')
        if n[1] > th: k.add('w')
        if n[1] < -th: k.add('s')
        return k

    def steer(s, st):
        h = st['hero']; lo, hi = st['lo'], st['hi']
        room = st['room']; F = [0.0, 0.0]; danger = 0.0
        ens = st['en']; boss = st.get('boss')
        # --- swarm repulsion + gap finding
        near = []
        for e in ens:
            dx, dy = h[0] - e[0], h[1] - e[1]; d = math.hypot(dx, dy) - e[2]
            if d < 9: near.append((d, dx, dy, e))
        cx = cy = wsum = 0.0
        for d, dx, dy, e in near:
            w = (3.0 if e[4] else 1.0) * (0.0 if e[5] else 1.0)   # nests do not chase
            if w == 0: continue
            dd = max(0.15, d - 0.35)
            rep = w * 2.2 / (dd * dd)
            n = nrm([dx, dy]); F[0] += n[0] * rep; F[1] += n[1] * rep
            k = w / (1 + d); cx += e[0] * k; cy += e[1] * k; wsum += k
            if d < 2.5: danger += w * (2.5 - d)
        # --- circle-kite: tangent around the weighted swarm centre at ~4.5u
        if wsum > 0:
            c = [cx / wsum, cy / wsum]; r = [h[0] - c[0], h[1] - c[1]]; rd = ln(r); rn = nrm(r)
            t = [-rn[1] * s.orbit, rn[0] * s.orbit]
            want = 5.5 if not boss else 6.0
            F[0] += t[0] * 1.6 + rn[0] * (want - rd) * 0.35; F[1] += t[1] * 1.6 + rn[1] * (want - rd) * 0.35
            # flip orbit when the tangent runs into a wall soon
            ahead = [h[0] + t[0] * 2.2, h[1] + t[1] * 2.2]
            if (ahead[0] < lo[0] + 0.6 or ahead[0] > hi[0] - 0.6 or ahead[1] < lo[1] + 0.6 or ahead[1] > hi[1] - 0.6) and time.time() > s.flip_t:
                # instead of reversing into the pack, prefer to keep turning: flip only if the open side is the other way
                s.orbit = -s.orbit; s.flip_t = time.time() + 1.2
        # --- walls / corners (open space is safety)
        for ax in (0, 1):
            a = h[ax] - lo[ax]; b = hi[ax] - h[ax]
            if a < 3.0: F[ax] += 1.8 * (3.0 - a) ** 2 / 3
            if b < 3.0: F[ax] -= 1.8 * (3.0 - b) ** 2 / 3
        mid = [(lo[0] + hi[0]) / 2, (lo[1] + hi[1]) / 2]
        cdist = [mid[0] - h[0], mid[1] - h[1]]
        F[0] += cdist[0] * 0.04 * (1 + danger); F[1] += cdist[1] * 0.04 * (1 + danger)
        # --- boss
        if boss:
            s.boss_terms(st, boss, h, F)
        # --- boss bullets: sidestep perpendicular to their path
        for b in st.get('bb', []):
            px, py, vx, vy = b; rx, ry = h[0] - px, h[1] + 0.6 - py; sp2 = vx * vx + vy * vy
            if sp2 < 1e-4: continue
            tca = (rx * vx + ry * vy) / sp2
            if tca < 0 or tca > 1.0: continue
            mx, my = rx - vx * tca, ry - vy * tca; md = math.hypot(mx, my)
            if md < 1.3:
                n = nrm([mx, my]) if md > 0.05 else nrm([-vy, vx])
                k = 3.5 * (1.3 - md) / (0.25 + tca)
                F[0] += n[0] * k; F[1] += n[1] * k
        # --- v0.0.24 hook throwers: step out of a winding / telegraphed / flying hook lane; when hooked, pull away (struggle snaps it)
        for hk in st.get('hooks', []):
            ax, ay, dx, dy = hk['x'], hk['y'], hk['dx'], hk['dy']
            rx, ry = h[0] - ax, h[1] + 0.8 - ay
            if hk['ph'] == 'Drag':
                n = nrm([rx, ry]); F[0] += n[0] * 9.0; F[1] += n[1] * 9.0; danger += 2; continue
            along = rx * dx + ry * dy
            if along < -0.5 or along > hk['len'] + 0.5: continue
            px, py = rx - dx * along, ry - dy * along; pd = math.hypot(px, py); half = hk['w'] / 2 + 0.4 + 0.9
            if pd < half:
                n = nrm([px, py]) if pd > 0.05 else nrm([-dy, dx])
                k = (6.0 if hk['ph'] == 'Windup' else 10.0) * (half - pd + 0.3)
                F[0] += n[0] * k; F[1] += n[1] * k; danger += 1
        # --- v0.0.25 gossip imps: walk straight out of a telegraphed shout ring (centre is locked); keep a little distance from an imp walking in to shout
        for sh in st.get('shouts', []):
            rx, ry = h[0] - sh['x'], h[1] - sh['y']; d = math.hypot(rx, ry); n = nrm([rx, ry]) if d > 0.05 else nrm([-rx - 0.3, 1.0])
            if sh['ph'] == 'Tell' and d < sh['r'] + 0.9:
                k = 12.0 * (sh['r'] + 0.9 - d + 0.4); F[0] += n[0] * k; F[1] += n[1] * k; danger += 2
            elif sh['ph'] == 'Ready' and d < sh['r']:
                k = 1.5 * (sh['r'] - d); F[0] += n[0] * k; F[1] += n[1] * k
        # --- pickups (only when the path is safe-ish) / chest
        pkw = 0.0 if danger > 2.5 else 1.0
        best = None
        tgts = [(p[0], p[1], 1.4 if p[2] else 1.0) for p in st.get('pk', [])]
        if st.get('chest'): tgts.append((st['chest'][0], st['chest'][1], 3.0))
        for px, py, w in tgts:
            d = math.hypot(px - h[0], py - h[1])
            if d < 0.1: continue
            risk = 0
            for e in ens:
                if e[5]: continue
                if math.hypot(e[0] - px, e[1] - py) < 1.6 + e[2]: risk += 1
            score = w / (1 + d) - 0.3 * risk
            if best is None or score > best[0]: best = (score, px, py, d)
        if best and best[0] > 0 and pkw:
            n = nrm([best[1] - h[0], best[2] - h[1]]); k = 1.3 if not room['active'] else 0.9
            F[0] += n[0] * k; F[1] += n[1] * k
        # hunt/nests: approach nests when safe
        nests = sorted((math.hypot(e[0] - h[0], e[1] - h[1]), e) for e in ens if e[5])
        if nests:
            d, e = nests[0]
            if d > 1.8:
                k = 3.0 if danger < 3 else 1.6
                n = nrm([e[0] - h[0], e[1] - h[1]]); F[0] += n[0] * k; F[1] += n[1] * k
        return F, danger

    def boss_terms(s, st, b, h, F):
        st_ = b['st']; bx, by = b['x'], b['y']; d = math.hypot(h[0] - bx, h[1] - by); n = nrm([h[0] - bx, h[1] - by])
        # body: keep range 5-7, orbit
        keep = 6.0
        F[0] += n[0] * (keep - d) * (0.9 if d < keep else 0.25); F[1] += n[1] * (keep - d) * (0.9 if d < keep else 0.25)
        t = [-n[1] * s.orbit, n[0] * s.orbit]; F[0] += t[0] * 1.2; F[1] += t[1] * 1.2
        if st_ in ('ChargeTell', 'Charge'):
            cd = [b['cdx'], b['cdy']]; rx, ry = h[0] - bx, h[1] - by - 0.5
            along = rx * cd[0] + ry * cd[1]; perp = [rx - cd[0] * along, ry - cd[1] * along]; pd = ln(perp)
            half = b['lane'] / 2 + 1.2
            if along > -1.5 and pd < half + 0.8:
                pn = nrm(perp) if pd > 0.05 else [-cd[1], cd[0]]
                k = 9.0 * (half + 0.8 - pd)
                F[0] += pn[0] * k; F[1] += pn[1] * k
        elif st_ == 'Sweep':
            cx, cy = b['bcx'], b['bcy']; rx, ry = h[0] - cx, h[1] - cy - 0.5; dd = math.hypot(rx, ry)
            if dd < b['hornR'] + 1.8:
                a = math.degrees(math.atan2(ry, rx)); da = (a - b['sweep'] + 180) % 360 - 180
                if abs(da) < 85:
                    out = nrm([rx, ry]); side = [-out[1], out[0]] if da > 0 else [out[1], -out[0]]
                    F[0] += out[0] * 6 + side[0] * 4; F[1] += out[1] * 6 + side[1] * 4
        elif st_ == 'Stomp':
            rx, ry = h[0] - b['sx'], (h[1] - b['sy']) / 0.6; de = math.hypot(rx, ry)
            if de < 8.5:
                o = nrm([h[0] - b['sx'], h[1] - b['sy']]); k = 6.0 * (8.5 - de) / 8.5 + 2
                F[0] += o[0] * k; F[1] += o[1] * k
        elif st_ == 'Herd':
            dx = h[0] - b['herdX']
            if abs(dx) < 3.2: F[0] += (1 if dx >= 0 else -1) * 8.0
        if b.get('rage'):
            F[0] += n[0] * 0.6; F[1] += n[1] * 0.6

    def bullet_plan(s, st, F):
        """Sample the 8 key directions (+ the field direction) and simulate 0.7 s: bullets fly straight, hero moves at its speed.
        Hit test matches the game: bullet within 0.67 of hero centre (feet + 0.6 up). Pick the safest heading closest to the field."""
        h = st['hero']; sp = st['speed']; lo, hi = st['lo'], st['hi']; bbs = st['bb']
        cands = [(math.cos(a * math.pi / 4), math.sin(a * math.pi / 4)) for a in range(8)]
        fn = nrm(F)
        best = None
        for c in cands:
            cl = 99.0
            for k in range(1, 15):
                t = k * 0.05
                hx = min(max(h[0] + c[0] * sp * t, lo[0]), hi[0]); hy = min(max(h[1] + c[1] * sp * t, lo[1]), hi[1]) + 0.6
                for b in bbs:
                    d = math.hypot(b[0] + b[2] * t - hx, b[1] + b[3] * t - hy) - 0.67
                    w = d + t * 0.6     # nearer-in-time threats matter more
                    if w < cl: cl = w
            danger = 0 if cl > 0.9 else (0.9 - cl) * 6 + (4 if cl < 0.25 else 0)
            score = danger - (c[0] * fn[0] + c[1] * fn[1])
            if best is None or score < best[0]: best = (score, c, cl)
        return [best[1][0], best[1][1]]

    # ---------------- stats helpers ----------------
    def hit_cause(s, st, dmg):
        h = st['hero']; b = st.get('boss')
        nb = min([math.hypot(x[0] - h[0], x[1] + 0 - (h[1] + 0.6)) for x in st.get('bb', [])] or [99])
        ens = [(math.hypot(e[0] - h[0], e[1] - h[1]) - e[2], e) for e in st['en']]
        ne = min(ens, key=lambda x: x[0]) if ens else (99, None)
        if b:
            db = math.hypot(b['x'] - h[0], b['y'] - h[1])
            if nb < 1.2: return 'boss_bullet', nb, db
            if b['st'] in ('Charge', 'Sweep', 'Stomp', 'Herd') or db < 3.5: return 'boss_' + b['st'], nb, db
        if ne[1] is not None and ne[0] < 1.5:
            e = ne[1]; return ('elite_' if e[4] else '') + str(e[3]), nb, ne[0]
        if nb < 1.5: return 'bullet', nb, ne[0]
        return 'unknown', nb, ne[0]

    def new_visit(s, st):
        r = st['room']; s.cur_visit = {'i': r['i'], 'type': r['type'], 'enter_run': round(st['run'], 1), 'hits': 0, 'hp_lost': 0.0,
                                       'cleared_run': None, 'hp_enter': round(st['hp'], 1)}
        s.visits.append(s.cur_visit)

    def unstick(s, st, F, why):
        now = time.time(); s.esc_n += 1; n = nrm(F) if ln(F) > 0 else [1, 0]; side = 1 if s.esc_n % 2 else -1
        s.unstick_times = [t for t in s.unstick_times if now - t < 12] + [now]
        lo, hi = st['lo'], st['hi']; h = st['hero']
        if len(s.unstick_times) >= 3:   # escalate: repeated failures -> walk to the room centre, then retry
            c = [(lo[0] + hi[0]) / 2, (lo[1] + hi[1]) / 2]; s.esc_dir = [c[0] - h[0], c[1] - h[1]]; s.esc_until = now + 2.5; s.unstick_times = []; kind = 'centre'
        else:
            s.esc_dir = [-n[0] * 0.6 - n[1] * side, -n[1] * 0.6 + n[0] * side]; s.esc_until = now + 0.9; kind = 'side'
        s.hist.clear(); s.R['stuck_events'] += 1
        ev = {'t': round(now - T0, 1), 'run': round(st['run'], 1), 'room': st['room']['type'], 'pos': h, 'why': why, 'recovery': kind}
        s.R['stuck_log'].append(ev); log('UNSTICK', s.esc_n, ev)
        if s.R['stuck_events'] in (1, 5, 10): screenshot(f'stuck{s.R["stuck_events"]}_{st["room"]["type"]}')
        return s.esc_dir

    @staticmethod
    def wall_slide(st, F):
        """Do not push into a wall in combat: drop the outward component and slide along it."""
        h = st['hero']; lo, hi = st['lo'], st['hi']; F = list(F)
        for ax in (0, 1):
            if h[ax] - lo[ax] < 0.25 and F[ax] < 0: F[1 - ax] += (1 if F[1 - ax] >= 0 else -1) * abs(F[ax]); F[ax] = 0.2
            if hi[ax] - h[ax] < 0.25 and F[ax] > 0: F[1 - ax] += (1 if F[1 - ax] >= 0 else -1) * abs(F[ax]); F[ax] = -0.2
        return F

    def finish(s, st, result):
        s.R['result'] = result; s.R['wall_s'] = round(time.time() - T0, 1)
        if st:
            s.R['run_s'] = round(st.get('run', 0), 1); s.R['level'] = st.get('lvl'); s.R['kills'] = st.get('kills')
            if st.get('floor'): s.R['layout'] = [[f[0], f[1], f[2]] for f in st['floor']]
        s.R['hits_per_room'] = s.visits
        s.R['n_rooms_cleared'] = len(s.R['rooms_cleared'])
        if A.stats:
            with open(A.stats, 'w') as f: json.dump(s.R, f, indent=1)
        return result

    # ---------------- main loop ----------------
    def run(s):
        focus(); last_seq = -1; st = None; last_new = time.time()
        while time.time() - T0 < MAXT:
            nst = read()
            if not nst or nst['seq'] == last_seq:
                if time.time() - last_new > 20: log('botstate stale 20 s -> abort'); s.R['anomalies'].append('botstate_stale'); return s.finish(st, 'stale')
                time.sleep(0.01); continue
            st = nst; last_new = time.time()
            last_seq = st['seq']; m = st['mode']
            if m != s.last_mode: log('mode', m, 'hp', round(st['hp']), '/', round(st['maxHp']), 'lvl', st['lvl'], 'bones', st['bones'], 'room', st.get('room', {}).get('type')); s.last_mode = m
            if m in ('StartPick', 'LevelUp', 'Chest'):
                if time.time() - s.last_card_t < 0.6: continue
                cs = st['cards']
                if cs:
                    i = max(range(len(cs)), key=lambda j: card_score(cs[j], st)); log('pick', cs[i]); tap(str(i + 1)); s.last_card_t = time.time()
                continue
            if m == 'Dead':
                set_keys(set()); lh = s.last_hit
                s.R['death_cause'] = lh['cause'] if lh and st['run'] - lh['run'] < 2.0 else 'unknown'; s.R['death_room'] = st['room']['type']
                log('DEAD', 'room', st['room']['type'], 'lvl', st['lvl'], 'kills', st['kills'], 'cause', s.R['death_cause'], 'boss', st.get('boss'))
                screenshot('dead'); return s.finish(st, 'dead')
            if m == 'Won':
                set_keys(set()); log('WON'); return s.finish(st, 'won')
            if m != 'Play': set_keys(set()); continue
            if not st['aim']: tap('Tab')
            room = st['room']; now = time.time()
            if room['i'] != s.last_room:
                s.last_room = room['i']; s.new_visit(st); s.shop_enter_t = None; s.hist.clear()
                log('ENTER', room['type'], 'hp', round(st['hp']), 'lvl', st['lvl'], 'bones', st['bones'], 'skills', st['skills'])
                if room['type'] == 'Boss' and not room['cleared']:
                    s.boss_room_t = now; s.R['boss_enter_run_s'] = round(st['run'], 1)
                    log('BOSS-ROOM enter; boss in botstate:', bool(st.get('boss')), 'doors', [(d['dir'], d['type']) for d in st['doors']], 'hero', st['hero'])
                    screenshot('boss_enter')
            if st.get('boss') and s.boss_seen_t is None:
                s.boss_seen_t = st['run']
                if s.R['boss_enter_run_s'] is not None: s.R['boss_spawn_latency_s'] = round(st['run'] - s.R['boss_enter_run_s'], 2)
                log('BOSS seen at run', st['run'], 'latency', s.R['boss_spawn_latency_s'], 'boss', {k: st['boss'][k] for k in ('x', 'y', 'hp', 'st')})
            if room.get('cleared') and s.stats.get(room['i']) is None:
                s.stats[room['i']] = 1
                if room['type'] not in ('Start', 'Shrine', 'Secret'): s.R['rooms_cleared'].append(room['type'])
                if s.cur_visit: s.cur_visit['cleared_run'] = round(st['run'], 1)
                if room['type'] == 'Boss' and s.boss_seen_t is not None: s.R['boss_ttk_s'] = round(st['run'] - s.boss_seen_t, 1); screenshot('boss_killed')
                log('CLEARED', room['type'], 'hp', round(st['hp']), 'lvl', st['lvl'])
            F, danger = s.steer(st)
            if st['en'] or room['active']: s.quiet_since = now
            nav = None; mode_tag = 'fight'
            boss_wait = room['type'] == 'Boss' and not room['cleared'] and not st.get('boss')
            if boss_wait and s.boss_room_t is not None:
                if now - s.boss_room_t > A.boss_wait_max:
                    log(f'BOSS-NO-SPAWN: {A.boss_wait_max:.0f} s in uncleared boss room, botstate never had a boss'); screenshot('boss_nospawn_final')
                    s.R['anomalies'].append('boss_no_spawn'); return s.finish(st, 'boss_no_spawn')
                nav = s.boss_wait(st); mode_tag = 'bosswait'
            elif not (danger > 1.0 or st['en']):
                nav = s.door_force(st); mode_tag = 'door'
                if nav is None and not room['active']:
                    nav = s.shop(st); mode_tag = 'shop'
            if nav is None: mode_tag = 'fight'
            if nav is not None:
                pks = st.get('pk', [])
                if pks and mode_tag == 'door' and now - s.quiet_since < 10:   # sweep the leftover blood/bones first (max 10 s)
                    h = st['hero']; p = min(pks, key=lambda q: math.hypot(q[0]-h[0], q[1]-h[1])); nav = [p[0]-h[0], p[1]-h[1]]
                F = nav if ln(nav) > 0 else F
            else:
                F = s.wall_slide(st, F)
            if ln(F) < 0.15 and mode_tag != 'bosswait':  # never stand still
                s.wander += 0.15; F = [math.cos(s.wander), math.sin(s.wander)]
            # progress-based unstick: <0.8 u net travel over 2 s while keys are held
            s.hist.append((now, st['hero'][0], st['hero'][1]))
            while s.hist and now - s.hist[0][0] > 2.0: s.hist.pop(0)
            shop_near = bool(st.get('shop') and st['shop'].get('near'))
            if now < s.esc_until: F = s.esc_dir
            elif not shop_near and mode_tag != 'bosswait':
                if len(s.hist) > 10 and now - s.hist[0][0] > 1.8 and math.hypot(s.hist[-1][1]-s.hist[0][1], s.hist[-1][2]-s.hist[0][2]) < 0.8 and ln(F) > 0.15:
                    F = s.unstick(st, F, 'no_progress_' + mode_tag)
            if st.get('bb'): F = s.bullet_plan(st, F)
            want = s.keys_for(F); set_keys(want)
            if want and s.lastpos and abs(st['hero'][0]-s.lastpos[0]) + abs(st['hero'][1]-s.lastpos[1]) < 1e-3:
                s.stuck += 1
                if s.stuck in (15, 60, 150): log('no movement with keys', sorted(held), '-> refocus'); focus(); set_keys(set()); set_keys(want)
                if s.stuck > 200: s.stuck = 0
            else: s.stuck = 0
            s.lastpos = st['hero']
            if st['hp'] < s.hp_min: s.hp_min = st['hp']
            ph = s.prev_hp if s.prev_hp is not None else st['hp']
            if st['hp'] < ph - 0.5:
                dmg = ph - st['hp']; cause, nb, nd = s.hit_cause(st, dmg)
                s.last_hit = {'cause': cause, 'run': st['run']}; s.R['hits_total'] += 1; s.R['hp_lost_total'] = round(s.R['hp_lost_total'] + dmg, 1)
                if s.cur_visit: s.cur_visit['hits'] += 1; s.cur_visit['hp_lost'] = round(s.cur_visit['hp_lost'] + dmg, 1)
                if room['type'] == 'Boss': s.R['boss_hits'] += 1
                b = st.get('boss')
                log(f"HIT -{dmg:.0f} hp {st['hp']:.0f} room {room['type']} cause {cause} boss {b['st'] if b else '-'} nearBullet {nb:.1f} near {nd:.1f} keys {sorted(held)}")
            s.prev_hp = st['hp']
            if now - s.__dict__.get('lastlog', 0) > 5:
                s.lastlog = now; r0 = s.__dict__.get('seq0', (now - 5, st['seq'])); s.seq0 = (now, st['seq'])
                log('fps~', round((st['seq']-r0[1])/max(now-r0[0], .1)), 'pos', st['hero'], 'room', room['type'], 'en', len(st['en']), 'pk', len(st.get('pk', [])),
                    'boss', (st['boss']['st'], round(st['boss']['hp'])) if st.get('boss') else '-', 'mode', mode_tag, 'keys', sorted(held))
        set_keys(set()); return s.finish(st, 'timeout')

if __name__ == '__main__':
    log('playbot v2 seed', SEED, 'display', DISPLAY, 'win', WIN, 'state', STATE)
    b = Bot()
    try: r = b.run()
    except Exception as ex:
        import traceback; log('CRASH', traceback.format_exc()); b.R['anomalies'].append('bot_crash'); r = b.finish(None, 'bot_crash')
    finally: set_keys(set())
    log('RESULT', r)
