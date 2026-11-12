#!/usr/bin/env python3
"""Hellord floor 1 combat music - procedural chiptune/tracker-style synth (numpy/scipy only).
A phrygian, 128 BPM, 40 bars = 75.000 s = 3,307,500 samples (exact: 40 bars * 4 beats * 60/128 * 44100).
Loop is seamless by construction: every note/effect is rendered *circularly* (events wrap modulo the loop length,
delay = np.roll, reverb = circular FFT convolution, filters run on a 3x tiled copy), so tails wrap over the start.
Output: ../Music/music_floor1_combat.ogg (+ ../previews/music_floor1_preview.ogg, AUDIO_MAP.md block)."""
import os, subprocess, numpy as np, soundfile as sf
from scipy.signal import butter, sosfilt

SR = 44100; BPM = 128; BARS = 40
BEAT = SR * 60 / BPM; STEP = BEAT / 4                      # 16th
N = int(round(BARS * 4 * BEAT)); assert N == 3307500
ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), ".."))
rng = np.random.default_rng(1313)

def mtof(m): return 440.0 * 2 ** ((m - 69) / 12)
def sos(kind, f, o=2): return butter(o, f, kind, fs=SR, output='sos')
def cfilt(x, s):  # circular filter: tile x3, filter, take the middle copy (x: (N,) or (N,2))
    t = np.concatenate([x, x, x]); return sosfilt(s, t, axis=0)[len(x):2 * len(x)]
def add(buf, start, sig, gl=1.0, gr=1.0):  # wrap-around add into stereo buf
    s = int(round(start)) % N; n = len(sig); a = min(n, N - s)
    buf[s:s + a, 0] += sig[:a] * gl; buf[s:s + a, 1] += sig[:a] * gr
    if a < n: b = n - a; buf[:b, 0] += sig[a:] * gl; buf[:b, 1] += sig[a:] * gr
def pan(p): return np.cos((p + 1) * np.pi / 4), np.sin((p + 1) * np.pi / 4)
def adsr(n, a, d, s, r):  # n total samples incl. release
    e = np.ones(n); na = max(1, int(a * SR)); nd = int(d * SR); nr = min(int(r * SR), n // 2)
    e[:na] = np.linspace(0, 1, na); e[na:na + nd] = np.linspace(1, s, min(nd, max(0, n - na)))[:max(0, min(nd, n - na))]
    e[na + nd:] = s; e[n - nr:] *= np.linspace(1, 0, nr); return e
def pulse(ph, duty): return np.where((ph % 1) < duty, 1.0, -1.0)
def tri(ph): return 2 * np.abs(2 * (ph % 1) - 1) - 1
def saw(ph): return 2 * (ph % 1) - 1

# ---------- instruments (return mono signal) ----------
def bass(m, steps, vel):
    n = int(steps * STEP); f = mtof(m); t = np.arange(n) / SR; ph = f * t
    x = .7 * pulse(ph, .35) + .5 * np.sin(2 * np.pi * ph) + .25 * saw(ph * 1.003)
    return x * adsr(n, .004, .12, .5, .04) * vel
def arp_note(m, steps, vel):
    n = int(steps * STEP); t = np.arange(n) / SR; ph = mtof(m) * t
    x = .6 * pulse(ph, .25) + .4 * tri(ph * 2.0)
    return x * adsr(n, .002, .06, .25, .03) * vel
def lead_note(m, steps, vel):
    n = int(steps * STEP * 1.0); t = np.arange(n) / SR
    vib = 1 + .006 * np.sin(2 * np.pi * 5.2 * t) * np.clip(t * 4, 0, 1)
    ph = np.cumsum(mtof(m) * vib) / SR
    x = .5 * pulse(ph, .5) + .35 * pulse(ph * 1.006, .25) + .3 * tri(ph * .5 + .13)
    return x * adsr(n, .012, .15, .6, .12) * vel
def pad_note(ms, steps, vel):
    n = int(steps * STEP); t = np.arange(n) / SR; L = np.zeros(n); R = np.zeros(n)
    for m in ms:
        f = mtof(m)
        L += saw(f * 1.004 * t + rng.random()) + .5 * tri(f * t)
        R += saw(f * .996 * t + rng.random()) + .5 * tri(f * t)
    e = adsr(n, .5, .2, .8, .6) * vel / len(ms)
    return L * e, R * e
def kick(vel):
    n = int(.28 * SR); t = np.arange(n) / SR; f = 45 + 95 * np.exp(-t * 28)
    return np.sin(2 * np.pi * np.cumsum(f) / SR) * np.exp(-t * 11) * vel + .08 * vel * np.random.default_rng(1).uniform(-1, 1, n) * np.exp(-t * 400)
def snare(vel, r):
    n = int(.22 * SR); t = np.arange(n) / SR
    body = np.sin(2 * np.pi * np.cumsum(190 - 70 * np.clip(t * 12, 0, 1)) / SR) * np.exp(-t * 28)
    nz = sosfilt(sos('bp', [900, 5500]), r.uniform(-1, 1, n)) * np.exp(-t * 20)
    return (body * .5 + nz * .7) * vel
def hat(vel, r, openh=False):
    n = int((.12 if openh else .045) * SR); t = np.arange(n) / SR
    return sosfilt(sos('bp', [5000, 9000]), r.uniform(-1, 1, n)) * np.exp(-t * (22 if openh else 90)) * vel

# ---------- song data (A phrygian) ----------
CH = {  # name: (bass root midi, pad triad)
 "Am": (33, [57, 60, 64]), "Bb": (34, [58, 62, 65]), "Gm": (31, [55, 58, 62]),
 "F": (29, [53, 57, 60]), "E": (28, [52, 56, 59])}
P1 = ["Am", "Bb", "Am", "Gm"]; P2 = ["Am", "F", "Bb", "E"]
MEL1 = [[(0, 69, 6), (6, 72, 2), (8, 70, 4), (12, 69, 4)], [(0, 70, 6), (6, 74, 2), (8, 72, 4), (12, 70, 4)],
        [(0, 69, 4), (4, 72, 4), (8, 76, 6), (14, 74, 2)], [(0, 74, 6), (6, 72, 2), (8, 70, 4), (12, 67, 4)]]
MEL2 = [[(0, 76, 6), (6, 74, 2), (8, 72, 4), (12, 69, 4)], [(0, 72, 6), (6, 69, 2), (8, 65, 4), (12, 69, 4)],
        [(0, 74, 6), (6, 72, 2), (8, 70, 4), (12, 65, 4)], [(0, 71, 6), (6, 68, 2), (8, 71, 4), (12, 76, 4)]]
# per 8-bar section: progression, layers
SECTIONS = [  # (prog, bass, pad, arp, lead(melody or None, octave), kick pattern, snare, hats)
 dict(p=P1, bass=1, pad=1, arp=0, lead=None, kick=[0, 8], snare=0, hats=1, v=.8),
 dict(p=P1, bass=1, pad=1, arp=1, lead=None, kick=[0, 6, 8, 14], snare=1, hats=1, v=.9),
 dict(p=P2, bass=1, pad=1, arp=1, lead=(MEL2, 0), kick=[0, 6, 8, 14], snare=1, hats=2, v=1.0),
 dict(p=P1, bass=1, pad=1, arp=1, lead=(MEL1, 0), kick=[0, 4, 8, 12], snare=1, hats=2, v=1.0),
 dict(p=P2, bass=1, pad=1, arp=1, lead=(MEL2, 0), kick=[], snare=0, hats=0, v=.7),  # breakdown, drums return via fill
]
BASSPAT = [(0, 0, 1.6, 1), (2, 0, 1.6, .8), (4, 0, 1.6, .9), (6, 12, 1.4, .8), (8, 0, 1.6, 1), (10, 0, 1.6, .8),
           (12, 0, 1.6, .9), (14, 7, 1.4, .7)]  # (step, semitone offset, len, vel)
ARPPAT = [0, 1, 2, 1, 0, 2, 3, 2, 0, 1, 2, 3, 2, 1, 3, 1]

def render():
    dr = np.zeros((N, 2)); bs = np.zeros((N, 2)); ar = np.zeros((N, 2)); ld = np.zeros((N, 2)); pd = np.zeros((N, 2))
    r = np.random.default_rng(77)
    for si, S in enumerate(SECTIONS):
        for b in range(8):
            bar = si * 8 + b; t0 = bar * 4 * BEAT; ch = S["p"][b % 4]; root, tri_ = CH[ch]
            last = (b == 7); v = S["v"]
            if S["bass"] and not (si == 4 and b >= 4 and False):
                for st, off, ln, vel in BASSPAT:
                    add(bs, t0 + st * STEP, bass(root + off, ln, vel * .9 * v))
            if S["pad"]:
                L, R = pad_note(tri_, 16, .6 * v); add(pd, t0, L, 1, 0); add(pd, t0, R, 0, 1)
            if S["arp"]:
                notes = [tri_[0] + 12, tri_[1] + 12, tri_[2] + 12, tri_[0] + 24]
                for st in range(16):
                    gl, gr = pan(-.35 if st % 2 == 0 else .35)
                    add(ar, t0 + st * STEP, arp_note(notes[ARPPAT[st]], 1.3, (.55 if st % 4 == 0 else .4) * v), gl, gr)
            if S["lead"]:
                mel, oc = S["lead"]
                for st, m, ln in mel[b % 4]:
                    add(ld, t0 + st * STEP, lead_note(m + 12 * oc, ln * 1.0, .75 * v))
            for st in S["kick"]: add(dr, t0 + st * STEP, kick(.9 * v))
            if si == 4 and b < 4 or si == 0 and b >= 0 and False: pass
            if S["snare"]:
                for st in (4, 12): add(dr, t0 + st * STEP, snare(.5 * v, r))
            if S["hats"]:
                for st in (2, 6, 10, 14): add(dr, t0 + st * STEP, hat(.35 * v, r, st == 14 and b % 2 == 1), *pan(.4))
                if S["hats"] == 2:
                    for st in (0, 4, 8, 12): add(dr, t0 + st * STEP, hat(.18 * v, r), *pan(.4))
            # end-of-section fill (last bar): soft snare roll; section 4's fill leads back to the start
            if last:
                for k, st in enumerate((10, 12, 13, 14, 15)): add(dr, t0 + st * STEP, snare((.3 + .08 * k) * v, r))
                if si == 4:
                    for st in (0, 4, 8): add(dr, t0 + st * STEP, kick(.6))
        if si == 4:  # breakdown bars 32-35: lighter, then rebuild bars 36-39 with kick + snare + hats
            for b in range(4, 8):
                t0 = (si * 8 + b) * 4 * BEAT
                for st in (0, 8) if b < 7 else (0, 4, 8, 12): add(dr, t0 + st * STEP, kick(.75))
                for st in (4, 12): add(dr, t0 + st * STEP, snare(.4, r))
                for st in (2, 6, 10, 14): add(dr, t0 + st * STEP, hat(.3, r), *pan(.4))
    return dr, bs, ar, ld, pd

def reverb(x, tau, f, mix, pre=.012, seed=5):  # circular (FFT) convolution -> tail wraps over the start
    rr = np.random.default_rng(seed); n = int(tau * 4 * SR); t = np.arange(n) / SR
    out = np.zeros_like(x)
    for c in range(2):
        ir = sosfilt(sos('lp', f), rr.uniform(-1, 1, n)) * np.exp(-t / tau); ir[:int(pre * SR)] = 0
        ir = ir / np.sqrt(np.sum(ir ** 2)) * .35
        ipad = np.zeros(len(x)); ipad[:n] = ir
        out[:, c] = np.fft.irfft(np.fft.rfft(x[:, c]) * np.fft.rfft(ipad), len(x))
    return out * mix
def pingpong(x, delay_steps, fb, mix, taps=7):
    d = int(round(delay_steps * STEP)); out = np.zeros_like(x); cur = x.copy()
    for i in range(taps):
        cur = np.roll(cur, d, axis=0)[:, ::-1] * fb; out += cur
    return out * mix

def build():
    dr, bs, ar, ld, pd = render()
    bs = cfilt(bs, sos('lp', 700)); bs = np.tanh(bs * 1.4)
    ar = cfilt(ar, sos('lp', 3200)); ld = cfilt(ld, sos('lp', 2600)); pd = cfilt(pd, sos('lp', 900))
    ld = np.tanh(ld * 1.2)
    dr = cfilt(dr, sos('lp', 9000))
    mixd = bs * .85 + dr * .9 + ar * .5 + ld * .55 + pd * .5
    fx = reverb(ld * .9 + ar * .5 + pd * .6 + dr * .25, 1.1, 2800, .8) + pingpong(ar * .7 + ld * .5, 3, .5, .6)
    m = mixd + fx
    m = cfilt(m, sos('hp', 28)); m = cfilt(m, sos('lp', 11000, 1))
    # stereo-safe soft limit
    return m

def main():
    m = build()
    import pyloudnorm as pyln
    meter = pyln.Meter(SR); target = -19.0
    g = 10 ** ((target - meter.integrated_loudness(m)) / 20); m *= g
    for _ in range(4):  # gentle tanh limiting, keep peak < -3 dBFS
        pk = np.abs(m).max()
        if pk > 10 ** (-3.5 / 20):
            c = 10 ** (-4.5 / 20); m = np.tanh(m / c) * c
        m *= 10 ** ((target - meter.integrated_loudness(m)) / 20)
    m = np.clip(m, -1, 1)
    os.makedirs(os.path.join(ROOT, "Music"), exist_ok=True); os.makedirs(os.path.join(ROOT, "previews"), exist_ok=True)
    tmp = os.path.join(ROOT, "Music", "_tmp.wav"); sf.write(tmp, m.astype(np.float32), SR, subtype="FLOAT")
    out = os.path.join(ROOT, "Music", "music_floor1_combat.ogg")
    subprocess.run(["ffmpeg", "-y", "-loglevel", "error", "-i", tmp, "-ar", "44100", "-c:a", "libvorbis", "-q:a", "6", out], check=True)
    # verify decoded file
    dec, sr = sf.read(out, dtype="float32"); assert sr == SR
    lufs = meter.integrated_loudness(dec); peak = 20 * np.log10(np.abs(dec).max())
    seam = np.abs(dec[0] - dec[-1]).max(); typ = np.abs(np.diff(dec, axis=0)).mean()
    print(f"decoded frames {len(dec)} (expected {N}), LUFS {lufs:.2f}, peak {peak:.2f} dBFS, seam step {seam:.4f}, mean step {typ:.4f}")
    # preview: ~1.3 loops of the decoded file (wrap audible)
    pv = np.concatenate([dec, dec[:int(.3 * len(dec))]]); sf.write(tmp, pv, SR, subtype="FLOAT")
    subprocess.run(["ffmpeg", "-y", "-loglevel", "error", "-i", tmp, "-c:a", "libvorbis", "-q:a", "6",
                    os.path.join(ROOT, "previews", "music_floor1_preview.ogg")], check=True); os.remove(tmp)
    # AUDIO_MAP block (kept between markers; gen_batch1.py preserves it)
    blk = f"""<!-- MUSIC:START -->
## Music

| File | Game event / hook | BPM | Length | Loudness | Peak | Loop points |
|---|---|---|---|---|---|---|
| `Music/music_floor1_combat.ogg` | Floor 1 room combat music (loop while in combat rooms on floor 1) | {BPM} | {N/SR:.3f} s ({BARS} bars, 4/4) | {lufs:.1f} LUFS integrated | {peak:.1f} dBFS | start = sample 0, end = sample {N} (whole file loops) |

- A phrygian, 44.1 kHz stereo Ogg Vorbis (q6). Structure (8-bar sections): 1 bass+pad+soft drums, 2 + arp & snare, 3 + lead, 4 lead variation + 4-on-the-floor kick, 5 breakdown (bars 33-36) then rebuild into the loop start.
- Seamless: the music is rendered circularly (notes, delay and reverb tails wrap over the start), so the file is one exact loop of {N} samples. Decoded length: {len(dec)} samples. In Unity set `AudioSource.loop = true` and import with *Compression Format: Vorbis*, *Load Type: Streaming* (or Compressed In Memory); do **not** add a gap or fade between repeats. Use `AudioClip.samples == {N}` as a sanity check.
- Suggested in-game: volume 0.5-0.6 on a Music mixer group (already quiet at about -19 LUFS), duck by ~3-6 dB on level-up/banner stingers. Crossfade into other tracks, not by trimming the loop.
- Preview: `previews/music_floor1_preview.ogg` (the loop plus 30% - the wrap happens at {N/SR:.1f} s).
- Regenerate: `python3 tools/gen_music_floor1.py`.
<!-- MUSIC:END -->
"""
    mp = os.path.join(ROOT, "AUDIO_MAP.md"); s = open(mp).read()
    if "<!-- MUSIC:START -->" in s:
        a = s.index("<!-- MUSIC:START -->"); b = s.index("<!-- MUSIC:END -->") + len("<!-- MUSIC:END -->\n"); s = s[:a] + s[b:]
    open(mp, "w").write(s.rstrip("\n") + "\n\n" + blk)

if __name__ == "__main__": main()
