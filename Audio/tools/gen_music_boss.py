#!/usr/bin/env python3
"""Hellord BOSS-fight music loops (DRAFT). Procedural numpy/scipy only, fixed seeds.
Reuses the circular renderer from gen_music_options.py; kick on every beat, accented downbeat,
all layers in from bar 1 (sec_off) for boss intensity. Writes Music/boss/*.ogg, previews, AUDIO_MAP BOSS_MUSIC block."""
import os, re, subprocess, numpy as np, soundfile as sf
import gen_music_options as G
SR = G.SR; ROOT = G.ROOT; OUT = os.path.join(ROOT, "Music", "boss"); PREV = os.path.join(ROOT, "previews")
R = np.random.default_rng(77); K = lambda *a: G.kick(*a[:5], R, a[5])
# Hook riffs: (step 0-31 over 2 bars, len in 16ths, semitone offset from root). Phrygian: b2=1, b3=3, 4=5, tritone=6, 5=7, b6=8
HOOKS = {
 "oxhead": dict(
   call=[(0, 2, 0), (2, 1, 0), (3, 1, 0), (4, 2, 1), (6, 2, 0), (8, 3, 6), (11, 1, 5), (12, 2, 3), (14, 2, 1),
         (16, 2, 0), (18, 1, 0), (19, 1, 0), (20, 2, 1), (22, 2, 3), (24, 4, 1), (28, 4, 0)],
   resp=[(0, 2, 0), (2, 1, 0), (3, 1, 0), (4, 2, 1), (6, 2, 0), (8, 3, 6), (11, 1, 5), (12, 2, 3), (14, 2, 1),
         (16, 2, 7), (18, 2, 8), (20, 2, 7), (22, 2, 6), (24, 2, 5), (26, 2, 3), (28, 4, 1)],
   dev=[(0, 1, 0), (1, 1, 1), (2, 1, 0), (3, 1, 1), (4, 2, 6), (6, 2, 5), (8, 1, 0), (9, 1, 1), (10, 1, 0), (11, 1, 1), (12, 2, 7), (14, 2, 8),
        (16, 2, 12), (18, 2, 11), (20, 2, 8), (22, 2, 7), (24, 2, 6), (26, 2, 3), (28, 2, 1), (30, 2, 0)],
   trans=[0] * 4 + [1, 1, 0, 0] + [0] * 8 + [6, 6, 0, 0, 1, 1, 0, 0]),
 "horseface": dict(
   call=[(0, 3, 0), (3, 3, 0), (6, 2, 1), (8, 2, 0), (10, 2, -2), (12, 4, 0),
         (16, 3, 0), (19, 3, 0), (22, 2, 3), (24, 2, 1), (26, 2, 0), (28, 4, 6)],
   resp=[(0, 3, 0), (3, 3, 0), (6, 2, 1), (8, 2, 0), (10, 2, -2), (12, 4, 0),
         (16, 3, 0), (19, 3, 3), (22, 2, 5), (24, 4, 6), (28, 2, 7), (30, 2, 8)],
   dev=[(0, 2, 12), (2, 1, 13), (3, 1, 12), (4, 2, 8), (6, 2, 7), (8, 3, 6), (11, 3, 5), (14, 2, 3),
        (16, 2, 1), (18, 2, 0), (20, 2, 1), (22, 2, 3), (24, 3, 1), (27, 3, 0), (30, 2, -2)],
   trans=[0] * 4 + [1, 1, 0, 0] + [0] * 8 + [6, 6, 0, 0, 1, 1, 0, 0]),
}

HOOKS["horseface7"] = dict(  # v7: hammer riff locked to the pestle - 3 hammer blows, then a falling tritone howl
   call=[(0, 2, 0), (2, 2, 0), (4, 2, 0), (6, 2, 1), (8, 4, 6), (12, 2, 5), (14, 2, 1),
         (16, 2, 0), (18, 2, 0), (20, 2, 0), (22, 2, 3), (24, 6, 1), (30, 2, 0)],
   resp=[(0, 2, 0), (2, 2, 0), (4, 2, 0), (6, 2, 1), (8, 4, 6), (12, 2, 5), (14, 2, 1),
         (16, 2, 12), (18, 2, 12), (20, 2, 12), (22, 2, 13), (24, 4, 7), (28, 4, 6)],
   dev=[(0, 1, 0), (1, 1, 0), (2, 2, 1), (4, 1, 0), (5, 1, 0), (6, 2, 3), (8, 1, 0), (9, 1, 0), (10, 2, 6), (12, 4, 7),
        (16, 2, 13), (18, 2, 12), (20, 2, 8), (22, 2, 7), (24, 2, 6), (26, 2, 3), (28, 4, 1)],
   trans=[0] * 4 + [1, 1, 0, 0] + [0] * 8 + [6, 6, 1, 1, 0, 0, 1, 0])
HOOKS["qilin"] = dict(  # burning chariot riff: rolling root 16ths, b2 / tritone wheel spikes
   call=[(0, 1, 0), (1, 1, 0), (2, 2, 1), (4, 1, 0), (5, 1, 0), (6, 2, 3), (8, 1, 0), (9, 1, 0), (10, 2, 6), (12, 4, 1),
         (16, 1, 0), (17, 1, 0), (18, 2, 1), (20, 1, 0), (21, 1, 0), (22, 2, 3), (24, 4, 1), (28, 4, 0)],
   resp=[(0, 1, 0), (1, 1, 0), (2, 2, 1), (4, 1, 0), (5, 1, 0), (6, 2, 3), (8, 1, 0), (9, 1, 0), (10, 2, 6), (12, 4, 1),
         (16, 2, 7), (18, 2, 8), (20, 2, 7), (22, 2, 6), (24, 4, 3), (28, 4, 1)],
   dev=[(0, 2, 12), (2, 2, 13), (4, 2, 8), (6, 2, 7), (8, 2, 6), (10, 2, 3), (12, 4, 1),
        (16, 1, 0), (17, 1, 0), (18, 2, 1), (20, 2, 3), (22, 2, 6), (24, 4, 7), (28, 4, 1)],
   trans=[0] * 4 + [1, 1, 0, 0] + [0] * 8 + [6, 6, 0, 0, 1, 1, 0, 0])
HOOKS["icegorilla"] = dict(  # chest-beat riff: two pounding root hits, cold climb to the tritone, crashing back via b2
   call=[(0, 3, 0), (3, 3, 0), (6, 2, 3), (8, 2, 5), (10, 4, 6), (14, 2, 1),
         (16, 3, 0), (19, 3, 0), (22, 2, 1), (24, 2, 3), (26, 2, 1), (28, 4, -1)],
   resp=[(0, 3, 0), (3, 3, 0), (6, 2, 3), (8, 2, 5), (10, 4, 6), (14, 2, 1),
         (16, 3, 7), (19, 3, 8), (22, 2, 7), (24, 4, 6), (28, 2, 1), (30, 2, 0)],
   dev=[(0, 2, 12), (2, 2, 13), (4, 2, 12), (6, 2, 8), (8, 2, 7), (10, 2, 6), (12, 4, 3),
        (16, 2, 1), (18, 2, 0), (20, 2, 1), (22, 2, 3), (24, 3, 6), (27, 3, 5), (30, 2, 1)],
   trans=[0] * 4 + [1, 1, 0, 0] + [0] * 8 + [6, 6, 0, 0, 1, 1, 0, 0])

MOTIFS = {  # (step in 16ths, length, semitone offset from root), one bar
 "horseface": dict(oct=36, lp=3200, notes=[(0, 3, 1), (3, 3, 0), (6, 10, 6)]),          # Eb-D-Ab: "dum-dum-HOWL" (b2 falls to root, leaps to tritone)
 "oxhead": dict(oct=36, lp=3400, notes=[(0, 4, 0), (4, 4, 3), (8, 8, 1)]),   # A-C-Bb: climb, then crush down a half step
 "qilin": dict(oct=36, lp=3600, notes=[(0, 2, 7), (2, 2, 8), (4, 12, 0)]),    # B-C-E: half-step rise, plunge to root
 "icegorilla": dict(oct=24, lp=4200, ice=True, notes=[(0, 4, 12), (4, 4, 6), (8, 8, 1)]),  # B-F-C: high root, drop a tritone, sink to b2
}

BOSSES = [
 dict(dark=True, floor="icegorilla", name="Ice Gorilla", hook="Ice Gorilla boss fight (Frozen Hell boss room)", bpm=150, bars=24, seed=8301, hype=True, riff="icegorilla", motif="icegorilla", root=35, frozen=True, lp_dark=8000,
      vibe="HOOK v8: 3-note icy horn B-F-C (high, tritone drop, b2 sink) with glass ping, solo bars 1-2, returns every 4 bars louder; under it B-phrygian chest-beat riff call/response (2 pounding hits -> tritone climb -> b2 crash), evolves per 4 bars; cold hell: frozen drone, dissonant glass shards (b2/tritone), icy shatter swells, chug, 8-bar surge/impact"),
 dict(dark=True, floor="qilin", name="Qilin Chariot", hook="Qilin Chariot boss fight (Burning Hell boss room)", bpm=160, bars=24, seed=8401, hype=True, riff="qilin", motif="qilin", root=28, p2=True,
      vibe="HOOK v8: 3-note top-line horn B-C-E (half-step rise, then plunge to root) solo bars 1-2, returns every 4 bars louder (b2 shift, octave, tritone harmony, double-time, power chords); burning hell: E-phrygian 16th fire chug, distorted drone, ghost snares, 8-bar surge/impact"),
    # v3 DARK remakes (VH: v2 too fun). Rendered by render_dark(); v2 kept as *_fun_backup.ogg, v1 as *_v1_backup.ogg
    dict(dark=True, floor="oxhead", name="Ox-Head", hook="Ox-Head boss fight (Ox Pit boss room)", bpm=180, bars=24, seed=8501, hype=True, riff="oxhead", motif="oxhead", root=33,
      vibe="HOOK v8: 3-note top-line horn A-C-Bb (climb a minor third, crush down a half step) solo bars 1-2, returns every 4 bars louder (b2 shift, octave, tritone harmony, double-time, power chords); under it A-phrygian riff call/response (b2+tritone), evolves per 4 bars; hype surge: distorted b2/tritone drone, chugging low 8th bass, war drums, low dissonant horn swells"),
    dict(dark=True, floor="horseface", name="Horse-Face (P1)", hook="Horse-Face phase 1 (Pounding Mortar boss room P1)", bpm=140, bars=24, seed=8201, hype=True, riff="horseface7", motif="horseface", root=26, pestle4=True,
      vibe="HOOK v8: 3-note top-line horn Eb-D-Ab (dum-dum-HOWL) solo bars 1-2, returns every 4 bars louder (octave, tritone harmony, double-time); under it D-phrygian hammer riff (3 blows on the root -> tritone howl, answer an octave up), evolves per 4 bars; crushing: grinding drone, low chug, pestle thud beat 4, iron clangs, dissonant choir-like pad"),
    dict(dark=True, floor="horseface_p2", name="Horse-Face (P2)", hook="Horse-Face phase 2 (Pounding Mortar boss room P2)", bpm=155, bars=24, seed=8202, hype=True, riff="horseface7", motif="horseface", root=26, pestle4=True, p2=True,
      vibe="P2 HOOK v8: same Eb-D-Ab horn hook + hammer riff, harder from bar 1, 16th chug, denser drums, rising tritone horn, more distortion"),
]

def render_dark(c):
    s = G.Song(c["bpm"], c["bars"], c["seed"]); rng = s.rng; N, st, B = s.N, s.step, s.bars
    r = c["root"]; p2 = c.get("p2", False); f = G.mtof; ph = np.arange(N) / N
    # drone: root + octave + b2 + tritone cluster, detuned saws, slow circular filter sweep (integer LFO cycles)
    dr = np.zeros(N)
    for m, a in [(r, 1), (r + 12, .6), (r + 13, .35), (r + 6, .3 if p2 else .2), (r + 7, .4)]:
        for d in (-0.004, 0.004):
            ph0 = rng.random(); dr += a * (2 * ((f(m) * (1 + d) * np.arange(N) / SR + ph0) % 1) - 1)
    dr = np.tanh(1.5 * dr / 3)
    lfo = 0.5 + 0.5 * np.sin(2 * np.pi * 2 * ph)  # 2 cycles per loop
    lo, hi = G.cfilt(dr, "lowpass", 250), G.cfilt(dr, "lowpass", 900)
    s.bus["drone"] = np.stack([lo + lfo * (hi - lo)] * 2) * 0.25
    # kick on every beat, downbeat accent
    bs = 60 / c["bpm"]; kA = G.kick(150, 38, min(0.17, 0.3 * bs), min(0.38, 0.8 * bs), 0.4, rng, 3.0); kN = G.kick(130, 40, min(0.12, 0.25 * bs), min(0.3, 0.7 * bs), 0.3, rng, 2.6)
    knorm = 0.55
    prog = [0, 0, 1, 0, 0, 6, 1, 0]  # semitone offsets: root, b2, tritone
    prog = [{0: 0, 1: 1, 6: 6}[x] for x in prog]
    if c.get("riff"): prog = HOOKS[c["riff"]]["trans"]  # per-bar transposition plan (24 bars)
    hk = HOOKS.get(c.get("riff"))
    for bar in range(B):
        off = prog[bar % len(prog)]
        for b in range(4): s.add("kick", s.at(bar, b * 4), kA if b == 0 else knorm * kN)
        if c.get("pestle4"): s.add("kick", s.at(bar, 12), 0.35 * G.pestle(rng, 0.35))
        for b in (4, 12): s.add("snare", s.at(bar, b), 0.7 * G.snare(rng, 0.3, 150, 0.09, 2800))
        if p2:
            for k in (7, 15): s.add("snare", s.at(bar, k), 0.12 * G.snare(rng, 0.1, 150, 0.03, 2500))
        for k in range(0, 16, 2): s.add("hat", s.at(bar, k), (0.08 if k % 4 else 0.04) * G.hat(rng, 0.05, 2500, 4500, 0.015), pan=0.3)
        if bar % 4 == 3:  # low war-tom fill
            for i, k in enumerate((8, 10, 12, 13, 14, 15)): s.add("perc", s.at(bar, k), 0.45 * G.tom(95 - 8 * i, 0.3), pan=-0.3 + 0.12 * i)
        if bar % 2 == 0:  # iron clang on downbeat: low inharmonic, dark
            t = G.tn(1.2 * SR); x = sum(a * np.sin(2 * np.pi * f(r + 36) * k * t) * np.exp(-t / (0.5 / k ** .5)) for k, a in [(1, 1), (2.41, .6), (3.77, .4), (5.2, .2)])
            s.add("perc", s.at(bar, 0), 0.12 * G.filt(x, "lowpass", 3000), pan=0.2)
        # chug bass: straight 8ths (16ths in P2), palm-muted distorted saw, sidechained by kick
        steps = range(16) if p2 else range(0, 16, 2)
        for k in steps:
            m = r - 12 + off + (1 if (k == 14 and bar % 4 == 1) else 0)
            n = int((1 if p2 else 2) * st * 0.85)
            x = np.tanh(3 * G.saw(f(m), n, 0.006)) * G.env(n, 0.002, 0.06, 0.3, 0.01)
            s.add("bass", s.at(bar, k), (1.0 if k % 4 == 0 else 0.7) * G.filt(x, "lowpass", 450))
    if c.get("hype"):  # v5 battle-surge layer: 8-bar build -> impact, double-kick bursts, 16th hats, power stabs
        for bar in range(B):
            pb = bar % 8
            for k in range(16):  # driving dark 16th hats with accents
                if k % 2: s.add("hat", s.at(bar, k), (0.09 if k % 4 == 3 else 0.05) * G.hat(rng, 0.03, 3000, 5000, 0.008), pan=-0.25)
            for k in (3, 6, 10, 14):  # ghost snares
                if rng.random() < 0.6: s.add("snare", s.at(bar, k), 0.13 * G.snare(rng, 0.1, 160, 0.025, 2600))
            if pb in (3, 7):  # double-kick burst on last beat(s) of bars 4 / 8
                for k in ((13, 14) if pb == 3 else (9, 10, 13, 14)): s.add("kick", s.at(bar, k), 0.3 * kN)
            # syncopated low power-chord stabs (root+5th, distorted, dark) - urgency
            off = prog[bar % len(prog)]
            for k in (() if (hk and bar < 4) else (0, 3, 6, 10) if pb % 2 == 0 else (0, 3, 6, 8, 11, 14)):
                n = int(1.5 * st)
                for m in (r + off, r + off + 7):
                    x = np.tanh(4 * G.saw(f(m), n, 0.008)) * G.env(n, 0.002, 0.08, 0.25, 0.01)
                    s.add("stab", s.at(bar, k), 0.12 * G.filt(x, "lowpass", 1200), pan=-0.4 if m == r + off else 0.4)
            if pb == 7:  # snare roll crescendo + noise riser into the next phrase
                for k in range(8, 16): s.add("snare", s.at(bar, k), (0.15 + 0.06 * (k - 8)) * G.snare(rng, 0.12, 170, 0.04, 3000))
                n = int(32 * st); t = G.tn(n); nz = rng.standard_normal(n)
                blk = 1024; y = np.zeros(n)
                for i in range(0, n, blk):
                    fc = 300 * (10 ** (i / n)); y[i:i + blk] = G.filt(nz[i:i + blk], "bandpass", [fc, fc * 2.5])[:len(y[i:i + blk])]
                s.add("perc", s.at(bar - 1, 0), 0.10 * y * (t / t[-1]) ** 2)
            if pb == 0:  # impact: sub drop + dark crash on phrase downbeat
                n = int(1.5 * SR); t = G.tn(n)
                sub = np.sin(2 * np.pi * np.cumsum(30 + 60 * np.exp(-t / 0.15)) / SR) * np.exp(-t / 0.5)
                cr = G.filt(rng.standard_normal(n), "bandpass", [1500, 4500]) * np.exp(-t / 0.4)
                s.add("perc", s.at(bar, 0), 0.35 * sub + 0.10 * cr)
    if c.get("frozen"):  # cold hell: dissonant glass shards (b2 / tritone clusters) answering the riff
        for bar in range(B):
            for k, iv, a in ((6, 13, 0.5), (10, 18, 0.4), (14, 25, 0.35)) if bar % 2 else ((3, 18, 0.4), (11, 13, 0.5)):
                s.add("glass", s.at(bar, k), a * 0.09 * (G.glass(f(r + 48 + iv), 0.6, 0.2) + 0.6 * G.glass(f(r + 49 + iv), 0.6, 0.2)), pan=0.6 if k % 4 == 2 else -0.6)
            if bar % 8 == 4:  # icy shatter swell
                n = int(4 * s.beat); x = rng.standard_normal(n) * np.linspace(0, 1, n) ** 2
                s.add("glass", s.at(bar, 0), 0.05 * G.filt(x, "highpass", 4000))
    mo = MOTIFS.get(c.get("motif"))
    if mo:  # v8 TOP-LINE HOOK: one short 3-note motif above the chug, stated solo in bars 1-2, returns every 4 bars, louder
        plan = {0: ("plain", 1.0, []), 1: ("plain", 1.0, []), 4: ("plain", 0.9, []), 5: ("echo", 0.8, []),
                8: ("plain", 1.15, [12]), 9: ("plain", 1.15, [12]), 12: ("plain", 1.2, [12, -6]), 13: ("echo", 1.1, [12]),
                16: ("double", 1.15, [12]), 17: ("double", 1.15, [12]), 20: ("plain", 1.3, [12, 7]), 21: ("plain", 1.3, [12, 7]),
                22: ("double", 1.3, [12, 7]), 23: ("double", 1.35, [12, 7])}
        for bar, (kind, lv, harm) in plan.items():
            T = prog[bar % len(prog)]
            notes = mo["notes"]
            if kind == "double": notes = [(k // 2 + h, max(1, ln // 2), o) for h in (0, 8) for (k, ln, o) in notes]
            if kind == "echo": notes = [(k, ln, o) for (k, ln, o) in notes[:2]] + [(notes[2][0], notes[2][1], notes[2][2] + 1)]  # answer bends the howl up a b2
            for (k, ln, o) in notes:
                n = int(ln * st * 0.95)
                for hv in [0] + harm:
                    m = r + mo["oct"] + o + T + hv; t_ = G.tn(n)
                    fr = f(m) * 2 ** (-np.exp(-t_ / 0.03) / 12)  # dark scoop into each note
                    phs = np.cumsum(fr) / SR
                    x = sum(np.mod(phs * d + i * 0.3, 1) * 2 - 1 for i, d in enumerate((1, 1.006, 0.994)))
                    x = np.tanh(2.2 * x) + 0.4 * np.sign(np.sin(np.pi * phs))
                    x = G.filt(G.filt(x, "lowpass", mo["lp"]), "highpass", 250) * G.env(n, 0.008, 0.2, 0.75, 0.04)
                    if mo.get("ice"): x[:len(g := G.glass(f(m) * 2, 0.4, 0.15))] += 0.8 * g[:n] if n >= len(g) else 0
                    s.add("motif", s.at(bar, 0) + k * st, lv * (0.17 if hv == 0 else 0.08) * x, pan=0.0 if hv == 0 else (0.35 if hv > 0 else -0.35))
    if hk:  # v6 HOOK riff: 2-bar call / 2-bar response, transposed + thickened per section
        for bar in range(0, B, 2):
            sec = bar // 4 if B == 24 else bar * 4 // B
            phr = hk["call"] if (bar // 2) % 2 == 0 else hk["resp"]
            T = prog[bar % len(prog)]
            if sec == 3 and (bar // 2) % 2 == 1: phr = hk["dev"]  # development answer in bars 17-24
            voices = [(12, 1.0), (24, 0.5)] if (p2 and bar < 8) else [(12, 1.0)] if bar < 4 else [(12, 1.0), (24, 0.45)] if bar < 8 else [(12, 1.0), (24, 0.6), (19, 0.5)]
            for (k, ln, o) in phr:
                n = int(ln * st * 0.92)
                for (vo, va) in voices:
                    m = r + vo + o + T
                    x = G.saw(f(m), n, 0.007) + 0.5 * G.pulse(f(m) * 0.5, n, 0.3)
                    x = G.filt(np.tanh(4 * x), "lowpass", 1700 if bar >= 8 else 1300) * G.env(n, 0.003, 0.25, 0.55, 0.015)
                    s.add("riff", s.at(bar, 0) + k * st, va * (0.13 if not mo else 0.04 if bar < 2 else 0.08) * x, pan=0.0 if vo == 12 else (-0.45 if vo == 24 else 0.45))
    # dissonant low horn swells (no melody): long notes, b2 / tritone against root
    horn = [(0, 0), (2, 1), (4, 0), (6, 6)] if not p2 else [(0, 0), (2, 1), (4, 6), (6, 7)]
    for bar in range(0, B, 8):
        for (bb, o) in horn:
            if bar + bb >= B: continue
            n = int(32 * st)
            for m in (r + 12 + o, r + 19 + o):
                x = G.saw(f(m), n, 0.005); x = G.filt(np.tanh(1.5 * x), "lowpass", 800) * G.env(n, 0.5, 2.0, 0.7, 0.4)
                s.add("horn", s.at(bar + bb, 0), 0.16 * x, pan=-0.3 if m % 2 else 0.3)
    # choir-like pad: formant-filtered noise+saw cluster (root, b3, 5, b2 up top)
    pad = np.zeros(N)
    for m in (r + 24, r + 27, r + 31, r + 37):
        pad += G.saw(f(m), N, 0.003)
    pad = G.cfilt(pad, "bandpass", [450, 1100]) * (0.6 + 0.4 * np.sin(2 * np.pi * 4 * ph + 1))
    s.bus["pad"] = np.stack([pad, np.roll(pad, 441)]) * 0.05
    gains = dict(kick=1.3, snare=0.7, hat=0.6, perc=0.8, bass=0.35, drone=1.0 if not c.get("hype") else 0.85, horn=0.8, pad=1.0, stab=1.0, riff=1.0, glass=1.0, motif=1.0)
    ph_b = (np.arange(N) % s.beat) / SR; duck = 1 - 0.85 * np.exp(-ph_b / 0.09)
    mixb = np.zeros((2, N))
    for nm, b in s.bus.items():
        if nm in ("bass", "drone", "stab", "riff"): b = b * (duck if nm != "riff" else 1 - 0.4 * (1 - duck))
        mixb += gains.get(nm, 1) * b
    wet = np.stack([G.cfilt(x, "highpass", 200) for x in (mixb - gains["kick"] * s.bus["kick"] - gains["bass"] * s.bus["bass"] * duck)])
    mixb = mixb + 0.3 * (G.reverb_c(wet, 1.2, 1, rng, 2500) - wet)
    mixb = np.stack([G.cfilt(G.cfilt(x, "lowpass", c.get("lp_dark", 5500)), "highpass", 28) for x in mixb])
    return s, mixb

def main():
    os.makedirs(OUT, exist_ok=True); res = []
    for c in BOSSES:
        s, x = (render_dark(c) if c.get("dark") else G.render(c)); x = G.master(x)
        p = os.path.join(OUT, f"music_boss_{c['floor']}_{c['bpm']}bpm.ogg"); G.enc(x, p)
        v = G.verify(p, s); v.update(cfg=c, path=p, song=s); res.append(v)
        print(f"{os.path.basename(p)}: {s.N} smp {s.N/SR:.3f}s frames={v['frames']} LUFS={v['lufs']:.2f} pk={v['peak']:.2f} "
              f"seam={v['seam']:.4f}/max{v['maxstep']:.3f} rms end/start={v['rms_end']:.1f}/{v['rms_start']:.1f} "
              f"beat={v['beat_db']:.1f}dB ok {v['beats_ok']}/{v['nbeats']} accent={v['accent_db']:+.1f}dB")
        y = sf.read(p)[0]; y = np.concatenate([y, y[:int(0.3*len(y))]])
        fo = np.linspace(1, 0, SR)[:, None]; y[-SR:] *= fo
        tmp = os.path.join(PREV, "tmpb.wav"); sf.write(tmp, y, SR, subtype="PCM_16")
        subprocess.run(["ffmpeg", "-y", "-loglevel", "error", "-i", tmp, "-c:a", "libvorbis", "-q:a", "6",
                        os.path.join(PREV, f"music_boss_{c.get('pkey', c['floor'])}_preview.ogg")], check=True); os.remove(tmp)
    write_map(res)

def write_map(res):
    L = ["<!-- BOSS_MUSIC:START -->", "## Boss-fight music — DRAFT (not locked, not heard in game)", "",
         "Generator: `tools/gen_music_boss.py` (fixed seeds, renderer from gen_music_options.py). OGG 44.1 kHz stereo q6; whole file loops "
         "(loop start 0, end = file length). Kick on every quarter note, accented downbeat; beat 1 of bar k = k·bar samples. "
         "Previews: `previews/music_boss_<name>_preview.ogg` (Horse-Face P2: `music_boss_horseface_p2_preview.ogg`). v1 files kept as `*_v1_backup.ogg` (oxhead 140, horseface 90/100); v2 fun versions as `*_fun_backup.ogg`; v3 dark 160/112/126 BPM as `*_dark_backup.ogg`; v4 180/140/155 as `*_pre_hype_backup.ogg`; v5 no-hook hype as `*_nohook_backup.ogg`; Horse-Face v6 + Ice Gorilla 120 BPM as `*_prehook_backup.ogg`; Horse-Face/Ice Gorilla v7 as `*_weakhook_backup.ogg`; Ox-Head v6 + Qilin v2 as `*_prehook_backup.ogg`. All five bosses = v8 top-line 3-note horn hook remake (riff call/response bars 1-4, b2 variation 5-8, doubled hook 9-16, tritone development 17-24 -> surge into wrap; drone, dissonance, chug + 8-bar build/riser/impact, double-kick bursts, power stabs; no lead/arp). = loop + 30% (wrap audible), 1 s fade.", "",
         "| Boss | File | BPM | Bars | Length | Loop end (smp) | LUFS | Peak dBFS | Beat period | Bar period | Event hook | Vibe |",
         "|---|---|---|---|---|---|---|---|---|---|---|---|"]
    for r in res:
        c, s = r["cfg"], r["song"]
        L.append(f"| {c['name']} | `Music/boss/{os.path.basename(r['path'])}` | {c['bpm']} | {c['bars']} | {s.N/SR:.3f} s | {s.N} | {r['lufs']:.1f} | {r['peak']:.1f} | "
                 f"{60000/c['bpm']:.2f} ms / {s.beat:g} smp | {240000/c['bpm']:.1f} ms / {4*s.beat:g} smp | {c['hook']} | {c['vibe']} |")
    L += ["", "Not covered: Frost Ox-Head (legacy, replaced by Ice Gorilla; debug-only), King Yama / Ten Kings (design doc only, not in code).",
          "<!-- BOSS_MUSIC:END -->"]
    mp = os.path.join(ROOT, "AUDIO_MAP.md"); txt = open(mp).read()
    txt = re.sub(r"\n?<!-- BOSS_MUSIC:START -->.*?<!-- BOSS_MUSIC:END -->\n?", "\n", txt, flags=re.S).rstrip() + "\n\n" + "\n".join(L) + "\n"
    open(mp, "w").write(txt)

if __name__ == "__main__": main()
