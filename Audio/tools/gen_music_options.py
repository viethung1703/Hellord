#!/usr/bin/env python3
"""Hellord floor music OPTIONS (DRAFT) - procedural numpy/scipy only, fixed seeds.
Every option: kick on every quarter note with downbeat accent, seamless circular render,
-19 LUFS, peak < -3 dBFS, highs rolled off. Writes Music/options/*.ogg, previews and the
AUDIO_MAP MUSIC_OPTIONS block."""
import os, re, subprocess, numpy as np, soundfile as sf, pyloudnorm as pyln
from scipy.signal import butter, sosfilt
SR = 44100
ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUT = os.path.join(ROOT, "Music", "options"); PREV = os.path.join(ROOT, "previews")
SCALES = {"phrygian": [0,1,3,5,7,8,10], "minor": [0,2,3,5,7,8,10],
          "harmonic_minor": [0,2,3,5,7,8,11], "phrygian_dominant": [0,1,4,5,7,8,10]}
mtof = lambda m: 440.0 * 2 ** ((m - 69) / 12)

def sos(kind, f, o=2):
    return butter(o, f, btype=kind, fs=SR, output="sos")
def filt(x, kind, f, o=2): return sosfilt(sos(kind, f, o), x)
def cfilt(x, kind, f, o=2):  # circular filter (loop-safe)
    n = len(x); y = sosfilt(sos(kind, f, o), np.concatenate([x, x, x])); return y[n:2*n]

class Song:
    def __init__(s, bpm, bars, seed):
        s.bpm, s.bars = bpm, bars
        s.beat = 60.0 / bpm * SR; s.step = s.beat / 4
        s.N = int(round(bars * 4 * s.beat)); s.rng = np.random.default_rng(seed)
        s.bus = {}
    def add(s, bus, t_samp, x, pan=0.0):
        b = s.bus.setdefault(bus, np.zeros((2, s.N)))
        i = int(round(t_samp)) % s.N; n = len(x)
        idx = (np.arange(n) + i) % s.N
        l, r = np.cos((pan + 1) * np.pi / 4), np.sin((pan + 1) * np.pi / 4)
        np.add.at(b[0], idx, x * l * 1.414); np.add.at(b[1], idx, x * r * 1.414)
    def at(s, bar, st): return (bar * 16 + st) * s.step

def tn(n): return np.arange(int(n)) / SR
def env(n, a, d, sus=0.0, rel=0.01):
    t = tn(n); e = np.minimum(1, t / max(a, 1e-4)) * (sus + (1 - sus) * np.exp(-t / d))
    r = int(rel * SR); 
    if r and r < n: e[-r:] *= np.linspace(1, 0, r)
    return e

# ---------- instruments ----------
def kick(f0, f1, tau, dur, click=0.3, rng=None, drive=1.5):
    t = tn(dur * SR); f = f1 + (f0 - f1) * np.exp(-t / 0.03)
    x = np.sin(2 * np.pi * np.cumsum(f) / SR) * np.exp(-t / tau)
    if click: x += click * filt(rng.standard_normal(len(t)), "bandpass", [800, 3000]) * np.exp(-t / 0.004)
    x = np.tanh(drive * x) / np.tanh(drive); x[-200:] *= np.linspace(1, 0, 200); return x
def pestle(rng, dur=0.6):  # heavy stone/wood thump: sub + knock + gritty grind
    t = tn(dur * SR)
    sub = np.sin(2*np.pi*np.cumsum(38 + 50*np.exp(-t/0.05))/SR) * np.exp(-t/0.28)
    knock = filt(rng.standard_normal(len(t)), "bandpass", [180, 700]) * np.exp(-t/0.03)
    grit = filt(rng.standard_normal(len(t)), "bandpass", [900, 2500]) * np.exp(-t/0.012)
    x = np.tanh(1.8*(sub + 0.6*knock + 0.15*grit)); x[-300:] *= np.linspace(1,0,300); return x
def snare(rng, dur=0.22, tone=190, tau=0.07, bright=4500):
    t = tn(dur * SR); n = filt(rng.standard_normal(len(t)), "bandpass", [700, bright])
    x = 0.8*n*np.exp(-t/tau) + 0.5*np.sin(2*np.pi*tone*t)*np.exp(-t/0.04)
    x[-150:] *= np.linspace(1,0,150); return x
def hat(rng, dur=0.05, lo=5000, hi=8500, tau=0.012):
    t = tn(dur*SR); x = filt(rng.standard_normal(len(t)), "bandpass", [lo, hi]) * np.exp(-t/tau)
    x[-80:] *= np.linspace(1,0,80); return x
def glass(f, dur=0.5, tau=0.18):  # glassy inharmonic tick
    t = tn(dur*SR); x = np.zeros_like(t)
    for k, a in [(1, 1), (2.76, .5), (5.4, .25), (8.9, .1)]:
        if f*k < 7500: x += a*np.sin(2*np.pi*f*k*t)*np.exp(-t/(tau/k**0.5))
    x *= np.minimum(1, t/0.002); x[-100:] *= np.linspace(1,0,100); return x
def tom(f, dur=0.3):
    t = tn(dur*SR); x = np.sin(2*np.pi*np.cumsum(f*(1+0.6*np.exp(-t/0.03)))/SR)*np.exp(-t/0.12)
    x[-100:] *= np.linspace(1,0,100); return x
def pulse(f, n, duty=0.5, vib=0.0, vr=5.5):
    t = tn(n); ph = np.cumsum(f*(1 + vib*np.sin(2*np.pi*vr*t)*np.minimum(1, t/0.25)))/SR
    return np.where((ph % 1) < duty, 1.0, -1.0)
def saw(f, n, det=0.0):
    t = tn(n); x = 2*((f*t) % 1) - 1
    if det: x = 0.5*(x + 2*((f*(1+det)*t + 0.3) % 1) - 1)
    return x
def tri(f, n):
    t = tn(n); return 2*np.abs(2*((f*t) % 1) - 1) - 1
def note(kind, f, n, a=0.005, d=0.3, sus=0.6, lp=None, drive=0, **kw):
    n = int(n); x = {"pulse": lambda: pulse(f, n, **kw), "saw": lambda: saw(f, n, **kw),
                     "tri": lambda: tri(f, n), "sine": lambda: np.sin(2*np.pi*f*tn(n))}[kind]()
    if drive: x = np.tanh(drive*x)
    if lp: x = filt(x, "lowpass", lp)
    return x * env(n, a, d, sus, rel=min(0.03, n/SR/3))

# ---------- music helpers ----------
def chord(scale, root, deg, oct=0):
    sc = SCALES[scale]; out = []
    for k in (deg, deg+2, deg+4):
        out.append(root + 12*oct + sc[k % 7] + 12*(k // 7))
    return out
def degnote(scale, root, d):
    sc = SCALES[scale]; return root + sc[d % 7] + 12*(d // 7)

def gen_motif(rng, rhythm, deg0, span=(-2, 9)):
    """rhythm = list of (step,len) over 2 bars; returns list of (step,len,deg)."""
    out, d = [], deg0
    for i, (st, ln, *_) in enumerate(rhythm):
        if i: d = int(np.clip(d + rng.choice([-2, -1, -1, 1, 1, 2, 0, 3]), *span))
        out.append((st, ln, d))
    return out

def reverb_c(x, tau, mix, rng, lpf=3500):
    n = int(2.2*tau*SR); t = tn(n); out = np.zeros_like(x)
    for c in range(2):
        ir = filt(rng.standard_normal(n), "lowpass", lpf) * np.exp(-t/tau); ir /= np.sqrt(np.sum(ir**2))
        L = x.shape[1]; h = np.zeros(L); h[:min(n, L)] = ir[:L]
        out[c] = np.real(np.fft.ifft(np.fft.fft(x[c]) * np.fft.fft(h)))
    return x + mix*out

# ---------- floor renderers ----------
def render(cfg):
    s = Song(cfg["bpm"], cfg["bars"], cfg["seed"]); rng = s.rng
    sc, root, prog, style = cfg["scale"], cfg["root"], cfg["prog"], cfg["style"]
    B = s.bars; sec = lambda bar: min(3, (bar * 4) // B + cfg.get("sec_off", 0))  # 4 sections
    st = s.step
    # --- kick on EVERY beat, downbeat accent ---
    k_acc, k_norm = cfg["kick"]
    for bar in range(B):
        for b in range(4):
            v = 1.0 if b == 0 else cfg.get("knorm", 0.72)
            s.add("kick", s.at(bar, b*4), v * (k_acc if b == 0 else k_norm))
    # --- style percussion ---
    if style == "mortar":
        for bar in range(B):
            # pestle slam thud layered on beat 4 (design doc: accent on beat 4 = slam)
            s.add("kick", s.at(bar, 12), 0.3*pestle(rng, 0.5))
            if cfg.get("snare"):
                for b in (4, 12): s.add("snare", s.at(bar, b), 0.5*snare(rng, 0.3, 150, 0.09, 2500))
            for k in range(0, 16, 2):  # rice-grain shaker, soft 8ths
                if sec(bar) >= 1: s.add("hat", s.at(bar, k), (0.18 if k % 4 else 0.1)*hat(rng, 0.07, 2500, 5000, 0.02), pan=0.3)
            if sec(bar) >= 2 and bar % 2: s.add("perc", s.at(bar, 14), 0.35*tom(70, 0.35)); s.add("perc", s.at(bar, 15), 0.3*tom(60, 0.35))
    elif style == "frozen":
        for bar in range(B):
            for b in (4, 12):
                s.add("snare", s.at(bar, b), 0.35*snare(rng, 0.25, 230, 0.05, 3500) + 0.4*glass(mtof(degnote(sc, root+48, 4)), 0.25, 0.08))
            for k in range(2, 16, 4):  # glass ticks on offbeats
                f = mtof(degnote(sc, root + 60, int(rng.choice([0, 2, 4, 7]))))
                s.add("glass", s.at(bar, k), 0.22*glass(f, 0.4, 0.12), pan=rng.uniform(-.6, .6))
            if sec(bar) >= 1:
                for k in range(16):
                    if k % 2 or rng.random() < .3: s.add("hat", s.at(bar, k), (0.06 + 0.05*(k % 4 == 2))*hat(rng, 0.03, 4500, 7000, 0.006), pan=-0.3)
    elif style in ("burning", "oxpit"):
        for bar in range(B):
            for b in (4, 12): s.add("snare", s.at(bar, b), 0.6*snare(rng, 0.22, 200, 0.06, 4500))
            if style == "burning" and sec(bar) >= 1:
                for k in (7, 15):
                    if rng.random() < .5: s.add("snare", s.at(bar, k), 0.15*snare(rng, 0.1, 210, 0.03, 4000))
            for k in range(16):
                if style == "oxpit" and k % 2: continue
                v = 0.12 if k % 4 == 2 else 0.06
                s.add("hat", s.at(bar, k), v*hat(rng, 0.04, 5000, 8000, 0.01), pan=0.25)
            if cfg.get("pestle4"): s.add("kick", s.at(bar, 12), 0.3*pestle(rng, 0.45))
            if bar % 4 == 3:  # tom fill into next phrase
                for i, k in enumerate((12, 13, 14, 15)): s.add("perc", s.at(bar, k), 0.35*tom(150 - 22*i, 0.25), pan=-0.4+0.25*i)
    # --- bass ---
    bkind, bpat, blp, bdrive = cfg["bass"]
    for bar in range(B):
        deg = prog[bar % len(prog)]
        for (k, ln, o, v) in bpat:
            f = mtof(degnote(sc, root, deg) + 12*o)
            s.add("bass", s.at(bar, k), v*note(bkind, f, ln*st, 0.003, 0.15*ln*st/SR+0.05, 0.5, blp, bdrive))
    # --- pad ---
    pkind, pvol, plp = cfg["pad"]
    for bar in range(B):
        for i, m in enumerate(chord(sc, root + 24, prog[bar % len(prog)])):
            x = note(pkind, mtof(m), 16*st + 0.02*SR, 0.25, 1.0, 0.8, plp, det=0.004) if pkind == "saw" else note(pkind, mtof(m), 16*st + 0.02*SR, 0.3, 1.0, 0.8, plp)
            s.add("pad", s.at(bar, 0), pvol*x, pan=(-0.5, 0, 0.5)[i])
    # --- arp (sections 1+) ---
    akind, avol, apat, alp = cfg["arp"]
    for bar in range(B):
        if sec(bar) < 1: continue
        ch = chord(sc, root + 36, prog[bar % len(prog)]) ; ch = ch + [ch[0] + 12]
        for j, k in enumerate(apat):
            m = ch[j % len(ch)]
            x = note(akind, mtof(m), 1.6*st, 0.002, 0.08, 0.2, alp, duty=0.25) if akind == "pulse" else note(akind, mtof(m), 1.6*st, 0.002, 0.08, 0.2, alp)
            s.add("arp", s.at(bar, k), avol*x, pan=0.35 if j % 2 else -0.35)
    # --- lead (sections 2,3) ---
    lkind, lvol, lrhy, llp, loct = cfg["lead"]
    m1 = gen_motif(rng, lrhy, 4); m2 = gen_motif(rng, lrhy, 2)
    for bar in range(0, B, 2):
        if sec(bar) < 2: continue
        mot = m1 if (bar // 2) % 2 == 0 else m2
        for (k, ln, d) in mot:
            m = degnote(sc, root + 12*loct, d)
            x = note(lkind, mtof(m), ln*st, 0.01, 0.4, 0.55, llp, duty=0.3, vib=0.004) if lkind == "pulse" else note(lkind, mtof(m), ln*st, 0.01, 0.4, 0.55, llp)
            s.add("lead", s.at(bar, 0) + k*st, lvol*x)
    # ---- mix ----
    G = cfg["gains"]; mixb = np.zeros((2, s.N))
    # sidechain duck of bass/pad on every beat so the kick reads clearly (circular)
    ph = (np.arange(s.N) % s.beat) / SR; sc_env = 1 - cfg.get("duck", 0.65) * np.exp(-ph / 0.09)
    for nm in ("bass", "pad"): s.bus[nm] *= sc_env
    for name, b in s.bus.items():
        if name == "arp":  # ping-pong delay (circular)
            d = int(round(3*st)); b = b + 0.35*np.stack([np.roll(b[1], d), np.roll(b[0], 2*d)])
        mixb += G.get(name, 1.0) * b
    wet = np.stack([cfilt(c, "highpass", 200) for c in (mixb - G.get("kick", 1)*s.bus["kick"] - G.get("bass", 1)*s.bus["bass"])])
    mixb = reverb_c(mixb, cfg["verb"][0], 0, rng) + cfg["verb"][1]*(reverb_c(wet, cfg["verb"][0], 1, rng) - wet)
    mixb = np.stack([cfilt(cfilt(c, "lowpass", cfg["lp"], 2), "highpass", 28, 2) for c in mixb])
    return s, mixb

def master(x, target=-19.0):
    meter = pyln.Meter(SR); x = x / np.max(np.abs(x))
    x = x * 10 ** ((target - meter.integrated_loudness(x.T)) / 20)
    for _ in range(4):
        pk = np.max(np.abs(x))
        if 20*np.log10(pk) <= -3.6: break
        th = 10 ** (-4.2/20); a = np.abs(x)
        x = np.where(a > th*0.8, np.sign(x)*(th*0.8 + th*0.2*np.tanh((a - th*0.8)/(th*0.2))), x)
        x = x * 10 ** ((target - meter.integrated_loudness(x.T)) / 20)
    return x

def enc(x, path):
    tmp = path + ".tmp.wav"; sf.write(tmp, x.T, SR, subtype="PCM_16")
    subprocess.run(["ffmpeg", "-y", "-loglevel", "error", "-i", tmp, "-c:a", "libvorbis", "-q:a", "6", path], check=True); os.remove(tmp)

def verify(path, s):
    y, sr = sf.read(path); y = y.T; meter = pyln.Meter(SR)
    d = np.abs(np.diff(y, axis=1)).max(); seam = np.abs(y[:, 0] - y[:, -1]).max()
    w = int(0.05*SR); r_end = np.sqrt(np.mean(y[:, -w:]**2)); r_st = np.sqrt(np.mean(y[:, :w]**2))
    mono = y.mean(0); lo = filt(mono, "lowpass", 150, 4); e = filt(np.abs(lo), "lowpass", 40)
    on = [e[int(b*s.beat + 0.015*SR) % len(e)] for b in range(s.bars*4)]
    off = [e[int(b*s.beat - 0.03*SR) % len(e)] for b in range(s.bars*4)]
    down = np.mean(on[0::4]); other = np.mean([v for i, v in enumerate(on) if i % 4])
    return dict(frames=y.shape[1], expect=s.N, lufs=meter.integrated_loudness(y.T), peak=20*np.log10(np.abs(y).max()),
                seam=seam, maxstep=d, rms_end=20*np.log10(r_end+1e-9), rms_start=20*np.log10(r_st+1e-9),
                beat_db=20*np.log10(np.mean(on)/np.mean(off)), beats_ok=int(np.sum(np.array(on) > 2*np.array(off))),
                nbeats=s.bars*4, accent_db=20*np.log10(down/other))

# ---------- option configs ----------
def K(rng, f0, f1, tau, dur, click, drv): return kick(f0, f1, tau, dur, click, rng, drv)
R = np.random.default_rng(7)
OPTS = [
 dict(floor="mortar", label="A", bpm=80, bars=16, seed=5101, scale="phrygian", root=28, style="mortar",
      vibe="Slow, crushing E-phrygian grind; stone pestle thump on beat 4, kick on every beat",
      prog=[0, 0, 1, 0, 5, 5, 1, 0], kick=(pestle(R, 0.7), K(R, 110, 42, 0.16, 0.5, 0.25, 2.0)),
      bass=("saw", [(0, 3, 0, 1), (4, 2, 0, .7), (8, 3, 0, .9), (12, 2, 1, .6), (14, 2, 0, .6)], 420, 2.0),
      pad=("saw", 0.16, 900), arp=("tri", 0.10, [0, 4, 8, 12], 1800),
      lead=("pulse", 0.13, [(0, 8, 0), (8, 4, 0), (12, 4, 0), (16, 12, 0), (28, 4, 0)], 1600, 2),
      gains=dict(kick=1.0, bass=0.35, pad=0.45, arp=0.6, lead=0.6, hat=0.6, perc=0.8, snare=0.6), verb=(0.7, 0.25), lp=6000),
 dict(floor="mortar", label="B", bpm=90, bars=16, seed=5102, scale="phrygian", root=26, style="mortar", snare=True,
      vibe="Boss-P1 tempo D-phrygian march; driving 8th bass, clap 2/4, pestle thud beat 4",
      prog=[0, 1, 0, 6, 0, 1, 5, 6], kick=(pestle(R, 0.6), K(R, 120, 45, 0.14, 0.45, 0.3, 2.0)),
      bass=("pulse", [(k, 2, 1 if k in (6, 14) else 0, .9 if k % 4 == 0 else .6) for k in range(0, 16, 2)], 500, 1.5),
      pad=("saw", 0.14, 1000), arp=("pulse", 0.07, [0, 3, 6, 8, 11, 14], 2200),
      lead=("saw", 0.10, [(0, 6, 0), (6, 2, 0), (8, 8, 0), (16, 4, 0), (20, 4, 0), (24, 8, 0)], 1800, 2),
      gains=dict(kick=1.0, bass=0.35, pad=0.45, arp=0.6, lead=0.6, hat=0.6, perc=0.8, snare=0.55), verb=(0.6, 0.25), lp=6000),
 dict(floor="frozen", label="A", bpm=100, bars=16, seed=5201, scale="harmonic_minor", root=35, style="frozen",
      vibe="Cold B harmonic-minor; glassy offbeat ticks, hollow kick, slow icy pad",
      prog=[0, 0, 5, 4, 0, 3, 5, 4], kick=(K(R, 150, 48, 0.12, 0.35, 0.35, 1.3), K(R, 130, 50, 0.09, 0.3, 0.25, 1.2)),
      bass=("tri", [(0, 4, 0, 1), (6, 2, 0, .5), (8, 4, 0, .8), (14, 2, 1, .4)], 700, 0),
      pad=("tri", 0.2, 2500), arp=("sine", 0.12, [0, 3, 6, 10, 12], 4000),
      lead=("sine", 0.14, [(0, 6, 0), (6, 2, 0), (8, 8, 0), (16, 6, 0), (22, 10, 0)], 3500, 3),
      gains=dict(kick=1.0, bass=0.6, pad=0.55, arp=0.6, lead=0.6, hat=0.6, glass=0.7, snare=0.6), verb=(1.4, 0.45), lp=7000),
 dict(floor="frozen", label="B", bpm=110, bars=16, seed=5202, scale="phrygian", root=30, style="frozen",
      vibe="Eerie F#-phrygian pulse; brittle ice hats, glass bells, music-box arp",
      prog=[0, 1, 0, 6, 5, 1, 0, 0], kick=(K(R, 160, 50, 0.11, 0.3, 0.4, 1.4), K(R, 140, 52, 0.08, 0.28, 0.3, 1.2)),
      bass=("pulse", [(0, 2, 0, 1), (3, 1, 0, .4), (8, 2, 0, .8), (11, 1, 0, .4), (14, 2, 1, .5)], 600, 0),
      pad=("tri", 0.18, 2200), arp=("tri", 0.11, list(range(0, 16, 2)), 3800),
      lead=("pulse", 0.08, [(0, 4, 0), (4, 4, 0), (8, 8, 0), (16, 2, 0), (18, 2, 0), (20, 12, 0)], 2600, 3),
      gains=dict(kick=1.0, bass=0.5, pad=0.55, arp=0.55, lead=0.6, hat=0.7, glass=0.7, snare=0.6), verb=(1.2, 0.4), lp=7000),
 dict(floor="burning", label="A", bpm=140, bars=20, seed=5301, scale="phrygian_dominant", root=38, style="burning",
      vibe="Hot D phrygian-dominant drive; rolling 16th saw bass, snare 2/4, tom fills",
      prog=[0, 0, 1, 0, 5, 6, 1, 0, 0, 1], kick=(K(R, 180, 50, 0.13, 0.35, 0.4, 2.2), K(R, 160, 52, 0.1, 0.3, 0.3, 2.0)),
      bass=("saw", [(k, 1, 1 if k % 4 == 2 else 0, .9 if k % 4 == 0 else .55) for k in range(16)], 700, 2.0),
      pad=("saw", 0.09, 1400), arp=("pulse", 0.07, [0, 2, 4, 6, 8, 10, 12, 14], 2500),
      lead=("pulse", 0.11, [(0, 3, 0), (3, 3, 0), (6, 2, 0), (8, 4, 0), (12, 4, 0), (16, 3, 0), (19, 3, 0), (22, 2, 0), (24, 8, 0)], 2600, 2),
      gains=dict(kick=1.0, bass=0.45, pad=0.5, arp=0.5, lead=0.6, hat=0.6, perc=0.7, snare=0.6), verb=(0.6, 0.2), lp=6500),
 dict(floor="burning", label="B", bpm=150, bars=24, seed=5302, scale="phrygian", root=37, style="burning",
      vibe="Aggressive C#-phrygian gallop; distorted bass gallop, ghost snares, fiery riff",
      prog=[0, 0, 1, 1, 0, 0, 6, 5], kick=(K(R, 190, 52, 0.12, 0.32, 0.45, 2.4), K(R, 170, 54, 0.1, 0.28, 0.35, 2.2)),
      bass=("pulse", [(k + o, 1, 0, .9 if o == 0 else .6) for k in range(0, 16, 4) for o in (0, 2, 3)], 650, 2.5),
      pad=("saw", 0.08, 1300), arp=("saw", 0.05, [0, 3, 6, 8, 11, 14], 2300),
      lead=("saw", 0.08, [(0, 2, 0), (2, 2, 0), (4, 4, 0), (8, 2, 0), (10, 2, 0), (12, 4, 0), (16, 6, 0), (22, 2, 0), (24, 8, 0)], 2400, 2),
      gains=dict(kick=1.0, bass=0.4, pad=0.5, arp=0.5, lead=0.6, hat=0.6, perc=0.7, snare=0.6), verb=(0.55, 0.2), lp=6500),
 dict(floor="oxpit", label="A", bpm=128, bars=16, seed=5401, scale="phrygian", root=33, style="oxpit",
      vibe="Ox Pit A-phrygian, same palette as current track but four-on-the-floor throughout",
      prog=[0, 0, 1, 0, 5, 6, 1, 0], kick=(K(R, 170, 48, 0.14, 0.35, 0.35, 2.0), K(R, 150, 50, 0.11, 0.3, 0.3, 1.8)),
      bass=("pulse", [(k, 2, 1 if k in (6, 14) else 0, .9 if k % 4 == 0 else .6) for k in range(0, 16, 2)], 650, 1.2),
      pad=("saw", 0.12, 1500), arp=("pulse", 0.07, [0, 2, 3, 6, 8, 10, 11, 14], 2600),
      lead=("pulse", 0.11, [(0, 4, 0), (4, 2, 0), (6, 2, 0), (8, 6, 0), (14, 2, 0), (16, 4, 0), (20, 4, 0), (24, 8, 0)], 2600, 2),
      gains=dict(kick=1.0, bass=0.5, pad=0.5, arp=0.55, lead=0.6, hat=0.6, perc=0.7, snare=0.6), verb=(0.7, 0.25), lp=6500),
]

def main():
    os.makedirs(OUT, exist_ok=True); res = []
    for c in OPTS:
        s, x = render(c); x = master(x)
        p = os.path.join(OUT, f"music_{c['floor']}_opt{c['label']}_{c['bpm']}bpm.ogg"); enc(x, p)
        v = verify(p, s); v.update(cfg=c, path=p, song=s); res.append(v)
        print(f"{os.path.basename(p)}: {s.N} smp {s.N/SR:.3f}s frames={v['frames']} LUFS={v['lufs']:.2f} pk={v['peak']:.2f} "
              f"seam={v['seam']:.4f}/max{v['maxstep']:.3f} rms end/start={v['rms_end']:.1f}/{v['rms_start']:.1f} "
              f"beat={v['beat_db']:.1f}dB ok {v['beats_ok']}/{v['nbeats']} accent={v['accent_db']:+.1f}dB")
    # previews: A, 1 s gap, B (Ox Pit: optA, gap, current track first 30 s for comparison)
    gap = np.zeros((SR, 2))
    for fl in ("mortar", "frozen", "burning", "oxpit"):
        parts = [sf.read(r["path"])[0] for r in res if r["cfg"]["floor"] == fl]
        if fl == "oxpit": parts.append(sf.read(os.path.join(ROOT, "Music", "music_floor1_combat.ogg"))[0][:30*SR])
        y = np.concatenate([np.concatenate([p, gap]) for p in parts][:-1] if False else sum(([p, gap] for p in parts), [])[:-1])
        tmp = os.path.join(PREV, "tmp.wav"); sf.write(tmp, y, SR, subtype="PCM_16")
        subprocess.run(["ffmpeg", "-y", "-loglevel", "error", "-i", tmp, "-c:a", "libvorbis", "-q:a", "6",
                        os.path.join(PREV, f"music_options_{fl}_preview.ogg")], check=True); os.remove(tmp)
        print("preview", fl, f"{len(y)/SR:.1f}s")
    write_map(res)

NAMES = {"mortar": "Pounding Mortar", "frozen": "Frozen Hell", "burning": "Burning Hell", "oxpit": "Ox Pit"}
def write_map(res):
    L = ["<!-- MUSIC_OPTIONS:START -->", "## Floor music OPTIONS — DRAFT (not locked, not heard in game)", "",
         "Generator: `tools/gen_music_options.py` (fixed seeds). All: OGG 44.1 kHz stereo q6, whole file loops (loop start 0, end = file length), "
         "kick on every quarter note with downbeat accent, lowpassed 6–7 kHz. Hazard sync: beat = quarter note, beat 1 of bar = sample 0 + n·bar.", "",
         "| Floor | Opt | File | BPM | Bars | Length | Loop end (smp) | LUFS | Peak dBFS | Beat period | Bar period | Vibe |",
         "|---|---|---|---|---|---|---|---|---|---|---|---|"]
    for r in res:
        c, s = r["cfg"], r["song"]
        L.append(f"| {NAMES[c['floor']]} | {c['label']} | `Music/options/{os.path.basename(r['path'])}` | {c['bpm']} | {c['bars']} | {s.N/SR:.3f} s | {s.N} | "
                 f"{r['lufs']:.1f} | {r['peak']:.1f} | {60000/c['bpm']:.2f} ms / {s.beat:g} smp | {240000/c['bpm']:.1f} ms / {4*s.beat:g} smp | {c['vibe']} |")
    L += ["", "Notes:", "- Pounding Mortar: extra pestle-thud layer on beat 4 of every bar (design doc: accented 4th beat = slam), kick accent on beat 1. "
          "Survive ramp 80→110 needs runtime pitch (shifts key ~+5.5 st) or a separate ramped render; Frenzy 72 BPM = optA at pitch 0.9.",
          "- Ox Pit: current `music_floor1_combat.ogg` has a kick on every beat only in bars 25–32 (+37–40 rebuild); optA is a four-on-the-floor alternative.",
          "- Previews: `previews/music_options_<floor>_preview.ogg` = optA, 1 s gap, optB (Ox Pit: optA, gap, first 30 s of current track).",
          "<!-- MUSIC_OPTIONS:END -->"]
    mp = os.path.join(ROOT, "AUDIO_MAP.md"); txt = open(mp).read()
    txt = re.sub(r"\n?<!-- MUSIC_OPTIONS:START -->.*?<!-- MUSIC_OPTIONS:END -->\n?", "\n", txt, flags=re.S).rstrip() + "\n\n" + "\n".join(L) + "\n"
    open(mp, "w").write(txt)

if __name__ == "__main__": main()
