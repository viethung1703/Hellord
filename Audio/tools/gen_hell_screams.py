"""Procedural distant hell screams (numpy/scipy only). Glottal source + pitch contour/jitter + formant bank + rasp,
then distance (lowpass, early-reflection-free diffuse reverb), trimmed tail and fades. Writes SFX/ambient/hell_scream_NN.wav."""
import os, subprocess, numpy as np, soundfile as sf, pyloudnorm as pyln
from scipy.signal import butter, sosfilt, fftconvolve
SR = 44100; ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUT = os.path.join(ROOT, "SFX", "ambient"); PRE = os.path.join(ROOT, "previews")
VOW = {"a": [(800, 80, 1), (1150, 90, .5), (2900, 120, .25)], "e": [(400, 60, 1), (2000, 100, .45), (2800, 120, .3)],
       "i": [(300, 50, 1), (2300, 100, .5), (3100, 140, .3)], "o": [(450, 70, 1), (800, 80, .5), (2800, 120, .15)],
       "u": [(330, 60, 1), (700, 80, .3), (2400, 120, .1)]}
def bp(x, f, bw):
    lo, hi = max(30, f - bw), min(SR / 2 - 100, f + bw); return sosfilt(butter(2, [lo, hi], "band", fs=SR, output="sos"), x)
def lp(x, f): return sosfilt(butter(2, f, "low", fs=SR, output="sos"), x)
def hp(x, f): return sosfilt(butter(2, f, "high", fs=SR, output="sos"), x)
def voice(rng, dur, f0pts, vowels, rasp=0.3, jit=0.02, vib=(5.5, 0.015), formshift=1.0, breath=0.15):
    n = int(dur * SR); t = np.arange(n) / SR
    tp, fp = zip(*f0pts); f0 = np.interp(t, np.array(tp) * dur, fp)
    jn = lp(rng.standard_normal(n), 30); jn /= np.abs(jn).max() + 1e-9
    f0 = f0 * (1 + jit * jn + vib[1] * np.sin(2 * np.pi * vib[0] * t + rng.uniform(0, 6)))
    ph = np.cumsum(f0) / SR; src = (np.mod(ph, 1) * 2 - 1) ** 3 + 0.4 * (np.mod(ph, 1) * 2 - 1)
    src += rasp * lp(rng.standard_normal(n), 3000) * (0.5 + 0.5 * np.sin(2 * np.pi * ph * 0.5))  # subharmonic rasp/growl
    y = np.zeros(n); segs = np.linspace(0, n, len(vowels) + 1).astype(int)
    for i, v in enumerate(vowels):  # crossfaded vowel morph
        w = np.clip(1 - np.abs(np.arange(n) - (segs[i] + segs[i + 1]) / 2) / ((segs[i + 1] - segs[i]) * 0.9), 0, 1)
        y += w * sum(a * bp(src, f * formshift, bw * formshift) for f, bw, a in VOW[v])
    y += breath * bp(rng.standard_normal(n), 1800 * formshift, 1500)
    a = np.minimum(1, t / 0.12) * np.minimum(1, (dur - t) / (0.35 * dur)) ** 1.5
    return y * a
def distance(rng, x, cut, rt, wet):
    x = lp(hp(x, 120), cut); m = int(rt * SR); t = np.arange(m) / SR
    ir = rng.standard_normal(m) * np.exp(-6.9 * t / rt); ir = lp(ir, cut * 0.7); ir /= np.sqrt((ir ** 2).sum())
    y = np.concatenate([x, np.zeros(m)]); w = fftconvolve(y, ir)[:len(y)]
    return (1 - wet) * y + wet * w * 0.6
def finish(x, peak_db, maxlen):
    x = x[:int(maxlen * SR)]; e = np.abs(x); thr = e.max() * 10 ** (-60 / 20)
    end = min(len(x), np.nonzero(e > thr)[0][-1] + 1 + int(0.02 * SR)); x = x[:end]
    f = int(0.12 * SR); x[-f:] *= np.linspace(1, 0, f) ** 2; x[:64] *= np.linspace(0, 1, 64)
    x -= np.linspace(x[0], x[-1], len(x)) * 0  # endpoints already 0 by fades
    return x / np.abs(x).max() * 10 ** (peak_db / 20)
V = [  # name, hook text, seed, voice args, distance (cut, rt, wet), peak, maxlen
 ("damned soul wail", 11, dict(dur=1.6, f0pts=[(0, 380), (0.25, 620), (0.7, 560), (1, 300)], vowels="aao", rasp=0.15), (3500, 1.6, 0.55), -16, 2.4),
 ("demon howl (low, growly)", 12, dict(dur=1.7, f0pts=[(0, 95), (0.3, 150), (0.8, 130), (1, 70)], vowels="oau", rasp=0.9, jit=0.05, formshift=0.72, vib=(3, 0.03)), (2500, 1.8, 0.5), -15, 2.5),
 ("distant female-like wail, very far", 13, dict(dur=1.4, f0pts=[(0, 520), (0.4, 880), (0.75, 820), (1, 600)], vowels="aee", rasp=0.08, formshift=1.18, vib=(6, 0.02)), (1800, 2.0, 0.75), -18, 2.5),
 ("tortured shriek (short, harsh)", 14, dict(dur=0.75, f0pts=[(0, 700), (0.15, 1150), (0.6, 1050), (1, 650)], vowels="ii", rasp=0.5, jit=0.04, formshift=1.1), (4500, 1.0, 0.4), -17, 1.3),
 ("low moan rising to scream", 15, dict(dur=2.0, f0pts=[(0, 130), (0.5, 180), (0.8, 470), (1, 300)], vowels="uoaa", rasp=0.35), (3000, 1.4, 0.5), -15, 2.5),
 ("chorus of the damned (3 far voices)", 16, None, (2200, 2.2, 0.7), -17, 2.5),
]
def main():
    os.makedirs(OUT, exist_ok=True); meter = pyln.Meter(SR); rows = []; pre = []
    for i, (nm, seed, va, (cut, rt, wet), pk, ml) in enumerate(V, 1):
        rng = np.random.default_rng(seed)
        if va is None:
            x = sum(np.pad(voice(rng, 1.5, [(0, f), (0.4, f * 1.4), (1, f * 0.8)], vw, rasp=0.25, formshift=fs), (int(o * SR), int(0.6 * SR) - int(o * SR)))
                    for f, vw, fs, o in ((260, "aao", 0.9, 0.0), (410, "ooa", 1.05, 0.25), (180, "uao", 0.8, 0.45)))
        else: x = voice(rng, **va)
        x = finish(distance(rng, x, cut, rt, wet), pk, ml)
        p = os.path.join(OUT, f"hell_scream_{i:02d}.wav"); sf.write(p, x.astype(np.float32), SR, subtype="PCM_16")
        y, sr = sf.read(p); L = meter.integrated_loudness(y); P = 20 * np.log10(np.abs(y).max())
        click = max(abs(y[0]), abs(y[-1])); step = np.abs(np.diff(y)).max()
        rows.append((i, nm, len(y) / SR, P, L)); print(f"{p}: sr={sr} {len(y)/SR:.2f}s pk={P:.1f} LUFS={L:.1f} edge={click:.4f} maxstep={step:.4f}")
        pre += [y, np.zeros(int(1.2 * SR))]
    pa = os.path.join(PRE, "hell_screams_preview.ogg"); tmp = "/tmp/hs.wav"; sf.write(tmp, np.concatenate(pre), SR)
    subprocess.run(["ffmpeg", "-y", "-loglevel", "error", "-i", tmp, "-ac", "2", "-c:a", "libvorbis", "-q:a", "6", pa], check=True)
    mp = os.path.join(ROOT, "AUDIO_MAP.md"); old = open(mp).read()
    B = ["<!-- HELL_SCREAMS:START -->", "## Ambient hell screams (distant damned) - DRAFT, not heard/wired", "",
         "Event hook: **ambient hell scream / distant damned** (rooms & corridors). Procedural (`tools/gen_hell_screams.py`, deterministic seeds). 44.1 kHz 16-bit mono WAV in `SFX/ambient/`. Preview: `previews/hell_screams_preview.ogg` (all variants, 1.2 s gaps).", "",
         "| File | Variant | Length | Peak dBFS | LUFS |", "|---|---|---|---|---|"]
    B += [f"| `SFX/ambient/hell_scream_{i:02d}.wav` | {nm} | {d:.2f} s | {P:.1f} | {L:.1f} |" for i, nm, d, P, L in rows]
    B += ["", "Suggested use: pick a random variant (no immediate repeat), play as 2D/3D ambient at **0.3-0.5 volume** of the SFX bus (well under combat SFX), random pitch 0.92-1.08, random pan/position off-screen. "
          "Chance ~15-25% when entering a room, plus a global cooldown of 20-40 s (never more than 1 at a time; suppress during boss fights or duck under combat).", "<!-- HELL_SCREAMS:END -->"]
    if "<!-- HELL_SCREAMS:START -->" in old:
        a = old.index("<!-- HELL_SCREAMS:START -->"); b = old.index("<!-- HELL_SCREAMS:END -->") + len("<!-- HELL_SCREAMS:END -->"); old = old[:a] + "\n".join(B) + old[b:]
    else: old = old.rstrip("\n") + "\n\n" + "\n".join(B) + "\n"
    open(mp, "w").write(old)
if __name__ == "__main__": main()
