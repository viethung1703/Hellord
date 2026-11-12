"""Menu music (main + altar) - slow, scary, mystic. Circular rendering -> seamless whole-file loops. Uses gen_music_options as G."""
import os, numpy as np, soundfile as sf
import gen_music_options as G
SR = G.SR; f = G.mtof; ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
PHRY = [0, 1, 3, 5, 7, 8, 10]

def formant(x, fs=(500, 900, 2600)):
    return sum(a * G.cfilt(G.cfilt(x, "highpass", q * 0.8), "lowpass", q * 1.25) for q, a in zip(fs, (1, .5, .2)))

def render(c):  # v2: ominous drones, bowed-metal, low chant, sparse low temple bells, irregular ritual drums, tension swells
    s = G.Song(c["bpm"], c["bars"], c["seed"]); N, rng, r = s.N, s.rng, c["root"]; ph = np.arange(N) / N; t = np.arange(N) / SR
    # 1) ominous drone cluster: root, b2, tritone, oct; slow beating; integer-cycle filter breathing
    d = np.zeros(N)
    for m, a in ((r - 12, 1.0), (r, .8), (r + 1, c["b2"]), (r + 6, c["tri"]), (r + 12, .35)):
        for det in (-0.003, 0.0, 0.0025):
            d += a * (np.mod(t * f(m) * (1 + det) + rng.uniform(), 1) * 2 - 1)
    br = 0.5 - 0.5 * np.cos(2 * np.pi * c["breaths"] * ph)
    d = np.tanh(c.get("drive", 1.5) * d / 4) * 4
    d = (1 - br) * G.cfilt(d, "lowpass", 140) + br * G.cfilt(d, "lowpass", c["dlp"])
    s.add("drone", 0, d * (0.7 + 0.3 * br))
    # 2) bowed metal: inharmonic sustained partials, slow swells at random spots
    for k in range(c["bowed"]):
        n = int(rng.uniform(6, 10) * SR); tt = np.arange(n) / SR; f0 = f(r + 24 + rng.choice(c.get("bowednotes", [0, 1, 6, 7])))
        x = sum(a * np.sin(2 * np.pi * f0 * q * tt * (1 + 0.002 * np.sin(2 * np.pi * 0.3 * tt))) for q, a in ((1, 1), (2.32, .5), (3.86, .3), (5.1, .15)))
        s.add("metal", rng.integers(N), x * np.sin(np.pi * tt / tt[-1]) ** 3, pan=rng.uniform(-.8, .8))
    # 3) low male chant "oh" on root / tritone, every 4 bars, long
    for i, (deg, iv) in enumerate(c["chant"]):
        n = int(4.6 * 4 * s.beat); tt = np.arange(n) / SR; x = np.zeros(n)
        for m in (r + 12 + deg, r + 12 + deg + iv):
            for det in (-0.004, 0.0, 0.004):
                fr = f(m) * (1 + det) * (1 + 0.003 * np.sin(2 * np.pi * 4.5 * tt + rng.uniform(0, 6)))
                x += np.mod(np.cumsum(fr) / SR, 1) * 2 - 1
        x = formant(x, (380, 700, 2400)) * np.sin(np.pi * tt / tt[-1]) ** 2
        s.add("chant", s.at(i * 4, 0), x, pan=0.15 * (-1) ** i)
    # 4) sparse low temple bells (mystic), not every bar
    for bar in c["bells"]:
        m = r + 36 + rng.choice(c["bellnotes"]); n = int(5 * SR); tt = np.arange(n) / SR
        x = sum(a * np.sin(2 * np.pi * f(m) * q * tt) * np.exp(-tt / (2.2 / q ** .6)) for q, a in ((1, 1), (2.01, .4), (2.76, .5), (4.07, .25), (5.43, .12)))
        s.add("bell", s.at(bar, rng.choice([0, 4, 8])), x * np.minimum(1, tt / 0.003), pan=rng.uniform(-.6, .6))
    # 5) irregular ritual drums (low frame drum / taiko), sparse
    for bar in range(c["bars"]):
        for st, a in c["drums"][bar % len(c["drums"])]:
            n = int(1.0 * SR); tt = np.arange(n) / SR
            x = np.sin(2 * np.pi * np.cumsum(48 * (1 + 0.5 * np.exp(-tt / 0.04))) / SR) * np.exp(-tt / 0.35)
            x += 0.15 * G.filt(rng.standard_normal(n), "lowpass", 600) * np.exp(-tt / 0.05)
            s.add("drum", s.at(bar, st), a * x * np.minimum(1, tt / 0.002))
    # 6) tension swell (reverse-noise + rising tone) into every 8-bar phrase start (wraps into bar 1)
    for bar in range(8, c["bars"] + 1, 8):
        n = int(2 * 4 * s.beat); tt = np.arange(n) / SR; e = (tt / tt[-1]) ** 3
        x = G.filt(rng.standard_normal(n), "bandpass" if False else "lowpass", 1800) * e + 0.5 * np.sin(2 * np.pi * np.cumsum(f(r + 12) * (1 + 0.06 * tt / tt[-1])) / SR) * e
        s.add("swell", s.at(bar - 2, 0), x)
    if c.get("gong"):
        for bar in range(0, c["bars"], 4):
            n = int(6 * SR); tt = np.arange(n) / SR
            x = sum(a * np.sin(2 * np.pi * f(r - 12) * k * tt) * np.exp(-tt / (3 / k ** .4)) for k, a in ((1, 1), (1.47, .7), (2.09, .5), (2.76, .4), (3.9, .25), (5.2, .1)))
            s.add("bell", s.at(bar, 0), 1.8 * x * np.minimum(1, tt / 0.02))
    gains = dict(drone=0.08, metal=0.03, chant=0.05, bell=0.05, drum=0.5, swell=0.05); gains.update(c.get("g", {}))
    mix = sum(gains[k] * v for k, v in s.bus.items())
    mix = G.reverb_c(mix, 2.6, 0.6, rng, 2800)
    mix = np.stack([G.cfilt(G.cfilt(x, "lowpass", 6500), "highpass", 28) for x in mix])
    return s, mix

CFG = [  # v3 (VH: v2 too scary): consonant drones (root/5th), no b2/tritone, soft rare drums, gentle bells
 dict(name="main", bpm=56, bars=16, seed=9701, root=26, b2=0.0, tri=0.0, dlp=900, breaths=2, bowed=3, drive=0.6, bowednotes=[0, 7, 12],
      chant=[(0, 7), (-2, 7), (-4, 7), (-5, 7)], bells=[1, 3, 5, 7, 9, 11, 13, 15], bellnotes=[0, 3, 7, 10, 12, 14],
      drums=[[(0, 0.35)], [], [], []], g=dict(drone=0.06, metal=0.02, chant=0.045, bell=0.07, drum=0.35, swell=0.02)),
 dict(name="altar", bpm=50, bars=12, seed=9801, root=26, b2=0.0, tri=0.08, dlp=750, breaths=3, bowed=4, gong=True, drive=0.8, bowednotes=[0, 7, 10],
      chant=[(0, 7), (-2, 7), (0, 5)], bells=[1, 3, 5, 7, 9, 11], bellnotes=[0, 3, 5, 7, 10],
      drums=[[(0, 0.5)], [], [(0, 0.4), (8, 0.25)], []], g=dict(drone=0.065, metal=0.025, chant=0.05, bell=0.06, drum=0.4, swell=0.025)),
]

def main():
    res = []
    for c in CFG:
        s, x = render(c); x = G.master(x)
        p = os.path.join(ROOT, "Music", f"music_menu_{c['name']}.ogg"); G.enc(x, p); v = G.verify(p, s)
        k = int(0.3 * s.N); fd = int(SR); pv = np.concatenate([x, x[:, :k]], 1); pv[:, -fd:] *= np.linspace(1, 0, fd)
        G.enc(pv, os.path.join(ROOT, "previews", f"music_menu_{c['name']}_preview.ogg"))
        print(p, f"bpm={c['bpm']} {s.N/SR:.2f}s frames={v['frames']}/{v['expect']} LUFS={v['lufs']:.2f} pk={v['peak']:.2f} seam={v['seam']:.4f}/max{v['maxstep']:.4f} rms end/start={v['rms_end']:.1f}/{v['rms_start']:.1f}")
        res.append((c, s, v))
    # stingers (stereo OGG, not loops)
    rng = np.random.default_rng(9301)
    for nm, notes, fall in (("dead", [62, 61, 56, 50], True), ("won", [50, 57, 62, 65, 69], False)):
        n = int(4.5 * SR); tt = np.arange(n) / SR; x = np.zeros(n)
        for i, m in enumerate(notes):
            o = int(i * 0.35 * SR); g = G.glass(f(m + 12), 3.0, 1.2); x[o:o + len(g)] += g[:n - o]
        sw = sum(np.mod(np.cumsum(np.full(n, f(m) * d)) / SR, 1) * 2 - 1 for m in ([38, 45, 51] if fall else [38, 45, 50, 53]) for d in (0.997, 1.003))
        x += 0.25 * formant(sw, (450, 800, 2500)) * np.sin(np.pi * tt / tt[-1]) ** 2 * (np.exp(-tt / 2) if fall else 1)
        x += 0.8 * np.sin(2 * np.pi * np.cumsum(50 * (1 + np.exp(-tt / .05))) / SR) * np.exp(-tt / 0.6)
        st = np.stack([x, x]); st = G.reverb_c(st, 1.8, 0.5, rng, 3000)
        st *= np.minimum(1, (tt[-1] - tt) / 0.8); st[:, :64] *= np.linspace(0, 1, 64)
        st = st / np.abs(st).max() * 10 ** (-6 / 20); st = G.master(st, -20.0)
        p = os.path.join(ROOT, "Music", f"music_stinger_{nm}.ogg"); G.enc(st, p); print(p, f"{n/SR:.1f}s")
    return res
if __name__ == "__main__": main()
