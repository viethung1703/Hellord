#!/usr/bin/env python3
"""Hellord Batch 2 event SFX - procedural (numpy/scipy). Reuses primitives from gen_batch1.py.
Writes ../SFX/events/*.wav, a <!-- BATCH2 --> block in ../AUDIO_MAP.md, ../previews/batch2_preview.ogg.
Tuned to the A phrygian music (A Bb C D E F G)."""
import os, sys, subprocess, numpy as np, soundfile as sf
from scipy.signal import fftconvolve
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from gen_batch1 import *   # primitives: lp hp bp ad sat tone sine_sweep sweep noise crackle finish pad mix delay exp_env TN, SR, ROOT, lufs_or_rms, write

def mtof(m): return 440.0 * 2 ** ((m - 69) / 12)
def reverb(x, tau, f=2500, mix=.5, seed=3):
    r = np.random.default_rng(seed); n = int(tau * 2.2 * SR)
    ir = lp(noise(r, n), f) * exp_env(n, tau); ir[:int(.01 * SR)] = 0
    ir = ir / np.sqrt(np.sum(ir ** 2)) * .6
    return mix_(x, fftconvolve(x, ir)[:len(x) + n] * mix)
def mix_(a, b): return mix(a, b)
def bell(f, n, rr, ratios=(1, 2.76, 5.4, 8.93), amps=(1, .5, .25, .12), tau=.5, att=.002, detune=0.0):
    out = np.zeros(n)
    for k, (r, a) in enumerate(zip(ratios, amps)):
        fr = f * r
        if fr > 12000: continue
        out += a * (tone(fr, n) + (tone(fr * (1 + detune), n) if detune else 0)) * ad(n, att, tau / (1 + .6 * k))
    return out
def chip(f, n, duty=.25, tri_mix=.5):  # music-style pulse/tri voice
    ph = f * np.arange(n) / SR
    return (1 - tri_mix) * np.where((ph % 1) < duty, 1., -1.) + tri_mix * (2 * np.abs(2 * (ph % 1) - 1) - 1)
def note_at(x, sig, t): 
    i = int(t * SR); y = np.zeros(max(len(x), i + len(sig))); y[:len(x)] = x; y[i:i + len(sig)] += sig; return y

# ---------- designs ----------
def levelup_sting(r):
    out = np.zeros(1); seq = [57, 60, 64, 69, 72]  # A3 C4 E4 A4 C5 rising arp
    for k, m in enumerate(seq):
        n = int(.5 * SR); s = lp(chip(mtof(m), n, .25, .5), 3000) * ad(n, .003, .12)
        out = note_at(out, s * .6 + bell(mtof(m) * 2, n, r, tau=.25) * .12, .06 * k)
    n = int(1.1 * SR)   # final dark-bright chord: Am with Bb colour, bell shimmer + sub
    ch = sum(bell(mtof(m), n, r, tau=.55, detune=.003) * a for m, a in [(69, 1), (76, .7), (72, .6), (70, .22)])
    sub = tone(55, n) * ad(n, .02, .5) * .8 + tone(110, n) * ad(n, .02, .3) * .3
    out = note_at(out, ch * .35 + sub * .5, .3)
    return reverb(out, .45, 2200, .5)
def ui_card_select(r):
    n = int(r.uniform(.05, .07) * SR); f = r.uniform(700, 1100)
    x = lp(chip(f, n, .5, .3), 2800) * ad(n, .0005, .012) + hp(noise(r, n), 2500) * ad(n, .0002, .004) * .25
    return x + tone(f * .5, n) * ad(n, .001, .02) * .3
def ui_upgrade_confirm(r):
    n = int(.35 * SR); f = mtof(69) * r.uniform(.99, 1.01)
    a = lp(chip(f, n, .25, .5), 2800) * ad(n, .002, .09)
    b = delay(lp(chip(f * 1.5, n, .25, .5), 3000) * ad(n, .002, .14), .075)[:n + 4000]
    thump = sine_sweep(150, 55, n, 9) * ad(n, .002, .07)
    sp = bell(f * 3, n, r, ratios=(1, 2.76), amps=(1, .4), tau=.12) * .12
    x = mix(a * .7, b * .7, thump * .6, delay(sp, .075)[:n + 4000])
    return reverb(x, .2, 2500, .3)
def room_cleared_sting(r):
    n = int(1.2 * SR)
    bb = lp(chip(mtof(58), n, .5, .6), 1800) * ad(n, .004, .12)           # Bb ... resolves to A (phrygian cadence)
    a1 = delay(bell(mtof(69), n, r, tau=.5, detune=.002), .22)[:n + 20000]
    a2 = delay(bell(mtof(64), n, r, tau=.5), .22)[:n + 20000] * .6
    sub = delay(tone(55, n) * ad(n, .02, .5) + .4 * tone(110, n) * ad(n, .02, .3), .22)[:n + 20000]
    thud = sine_sweep(120, 45, n, 8) * ad(n, .001, .1)
    pad = lp(sat(sum(chip(mtof(m) * (1 + d), n, .5, .7) for m, d in [(57, .004), (60, -.004), (64, .003)]) * .2, 2), 1400) * ad(n, .2, .5)
    x = mix(bb * .22, a1 * .3, a2 * .2, sub * .45, thud * .4, delay(pad, .22)[:n + 20000] * .35)
    return reverb(x, .5, 2000, .5)
def door_unlock(r):
    n = int(r.uniform(.85, 1.0) * SR)
    def clack(t0):
        m = int(.12 * SR); c = bp(noise(r, m), 1200, 5000) * ad(m, .0002, .008)
        c += (tone(r.uniform(900, 1400), m) + .5 * tone(r.uniform(2400, 3200), m)) * ad(m, .0003, .03) * .4
        c += sine_sweep(180, 70, m, 10) * ad(m, .001, .03) * .8
        return delay(c, t0)
    gm = int(.62 * SR)  # stone grind: low noise with rough AM, falling cutoff
    gr = vary_filt_sweep(noise(r, gm), 1400, 250) * (.55 + .45 * np.sign(np.sin(2 * np.pi * r.uniform(28, 45) * TN(gm)))) * np.sin(np.pi * np.linspace(0, 1, gm)) ** .7
    gr += lp(noise(r, gm), 160) * ad(gm, .05, .4) * 1.2
    x = mix(clack(0) * .9, clack(.11) * .6, delay(gr, .2) * .8)
    x = mix(x, delay(sine_sweep(100, 38, int(.4 * SR), 5) * ad(int(.4 * SR), .002, .12), .78) * .8)  # final thud
    return reverb(x, .25, 1800, .3)
def banner_floor(r):
    n = int(1.0 * SR); u = np.linspace(0, 1, n)
    sw = np.zeros(n); B = 20; L = n // B + 1; fc = np.geomspace(250, 2200, B)
    x = noise(r, n)
    for i, f in enumerate(fc):
        seg = bp(x, f * .6, f * 1.5); w = np.zeros(n); w[i * L:(i + 1) * L] = 1
        w = np.convolve(w, np.hanning(601) / np.hanning(601).sum(), 'same'); sw += seg * w
    env = np.sin(np.pi * u ** .8) ** 2
    swell = tone(55, n) * env + .5 * tone(82.4, n) * env  # A1 + E2
    boom = delay(sine_sweep(110, 36, int(.5 * SR), 5) * ad(int(.5 * SR), .003, .18), .55)[:n]
    return reverb(sw * env * .9 + swell * .5 + boom * .7, .6, 2200, .4)
def banner_boss(r):
    n = int(1.2 * SR); u = np.linspace(0, 1, n); t = TN(n)
    drone = sum(a * tone(f, n) for f, a in [(55, 1), (55 * 1.0594 * 1.01, .7), (110.5, .4), (164.8, .25)])  # A1 vs Bb: menace
    drone = lp(sat(drone * .5, 2.5), 600)
    env = np.clip(u * 3, 0, 1) * np.clip((1 - u) * 2.2, 0, 1)
    x = noise(r, n); sw = np.zeros(n); B = 20; L = n // B + 1; fc = np.geomspace(1800, 180, B)  # falling = dread
    for i, f in enumerate(fc):
        seg = bp(x, f * .6, f * 1.5); w = np.zeros(n); w[i * L:(i + 1) * L] = 1
        w = np.convolve(w, np.hanning(601) / np.hanning(601).sum(), 'same'); sw += seg * w
    hit = delay(sine_sweep(95, 30, int(.6 * SR), 4) * ad(int(.6 * SR), .003, .22) + lp(noise(r, int(.6 * SR)), 500) * ad(int(.6 * SR), .001, .1) * .6, .45)
    hit = pad(hit, n)
    return reverb(drone * env * .8 + sw * np.sin(np.pi * u) ** 1.5 * .7 + hit * .9, .7, 1800, .45)
def chest_open(r):
    n = int(.85 * SR); t = TN(n)
    f = 95 * (1 + .4 * np.clip((t - .05) / .35, 0, 1)) * (1 + .03 * np.sin(2 * np.pi * 14 * t))  # wood creak
    ph = 2 * np.pi * np.cumsum(f) / SR; saw_ = 2 * ((ph / (2 * np.pi)) % 1) - 1
    creak = bp(saw_, 350, 1400) * np.clip((t - .02) / .05, 0, 1) * np.exp(-np.clip(t - .38, 0, None) * 12) * (.7 + .3 * noise(r, n))
    latch = delay(bp(noise(r, int(.1 * SR)), 1000, 4500) * ad(int(.1 * SR), .0002, .01) + sine_sweep(200, 80, int(.1 * SR), 9) * ad(int(.1 * SR), .001, .03), 0)
    glim = np.zeros(n)
    for k, m in enumerate([81, 85, 88, 93, 76][:4] if r.random() < .5 else [76, 81, 84, 88]):  # glimmer: E A C-ish rising chimes
        s = bell(mtof(m), int(.5 * SR), r, ratios=(1, 2.76, 4.1), amps=(1, .4, .2), tau=.2) * .12
        glim = note_at(glim, s, .34 + .06 * k)
    low = lp(chip(mtof(45), n, .5, .8), 400) * ad(n, .1, .35) * .4
    x = mix(creak * .55, delay(latch, .32) * .8, glim[:n + 30000], low)
    return reverb(x, .4, 2200, .4)
def shrine_use(r):
    n = int(1.7 * SR); f = mtof(r.choice([81, 76])) * r.uniform(.995, 1.005)  # E5 / A5-ish: eerie
    b1 = bell(f, n, r, tau=.9, detune=.004)
    b2 = delay(bell(f * 1.5 * 1.012, n, r, ratios=(1, 2.76, 5.4), amps=(1, .4, .2), tau=.8, detune=.005), .18)[:n] * .6   # slightly sour fifth
    wh = bp(noise(r, n), 1800, 4200) * np.sin(np.pi * np.linspace(0, 1, n)) ** 2 * (.5 + .5 * np.sin(2 * np.pi * 5 * TN(n))) * .35
    drone = lp(chip(mtof(45), n, .5, .8), 300) * ad(n, .3, .6) * .5
    x = mix(b1 * .4, b2 * .3, wh * .4, drone * .5)
    return reverb(x, .9, 2600, .6)
def shop_purchase(r):
    n = int(.5 * SR); out = np.zeros(1)
    for k, (t0, f, a) in enumerate([(0, r.uniform(2300, 2800), 1), (.07, r.uniform(3000, 3600), .8)]):
        s = bell(f, int(.4 * SR), r, ratios=(1, 1.47, 2.09, 2.56), amps=(1, .6, .4, .25), tau=.12, att=.0005) * a
        out = note_at(out, s, t0)
    drop = delay(sine_sweep(380, 190, int(.15 * SR), 4) * ad(int(.15 * SR), .002, .05), .12)  # dark blood 'plip'
    tick = hp(noise(r, int(.02 * SR)), 3000) * ad(int(.02 * SR), .0001, .003)
    x = mix(out * .35, drop * .6, tick * .4)
    return reverb(x, .15, 3000, .25)

# (stem, generator(rng), peak dBFS, hook, fade_out)
SOUNDS = [
 ("levelup_sting", levelup_sting, 1, -9, "Player levels up (before card choice)", .08),
 ("ui_card_select", ui_card_select, 3, -17, "Hover/select a level-up card", .01),
 ("ui_upgrade_confirm", ui_upgrade_confirm, 2, -12, "Confirm the chosen upgrade card", .04),
 ("room_cleared_sting", room_cleared_sting, 1, -9, "Last enemy of a room dies", .08),
 ("door_unlock", door_unlock, 2, -10, "Room doors unlock after clear", .05),
 ("banner_whoosh_floor", banner_floor, 1, -10, "Floor-change banner appears", .1),
 ("banner_whoosh_boss", banner_boss, 1, -9, "Boss-incoming banner appears", .1),
 ("chest_open", chest_open, 2, -11, "Chest opened", .06),
 ("shrine_use", shrine_use, 2, -11, "Shrine activated", .1),
 ("shop_purchase", shop_purchase, 2, -13, "Item bought in shop", .04),
]
CAP = {"levelup_sting": 1.5, "ui_card_select": .1, "ui_upgrade_confirm": .6, "room_cleared_sting": 1.5, "door_unlock": 1.2,
       "banner_whoosh_floor": 1.2, "banner_whoosh_boss": 1.2, "chest_open": 1.2, "shrine_use": 2.0, "shop_purchase": .5}  # max seconds (reverb tails are cut with a fade)
FADE = {"levelup_sting": .3, "room_cleared_sting": .3, "door_unlock": .25, "banner_whoosh_floor": .3, "banner_whoosh_boss": .35,
        "chest_open": .25, "shrine_use": .5, "shop_purchase": .15, "ui_upgrade_confirm": .15}
JIT = {"levelup_sting": "vol 0.8, no jitter, duck music ~4 dB for 1.5 s", "ui_card_select": "vol 0.5, pitch 0.95-1.05 (random variant on each hover)",
       "ui_upgrade_confirm": "vol 0.7, pitch 0.97-1.03", "room_cleared_sting": "vol 0.7, no jitter; play at the same time as door_unlock (delay unlock 0.2 s)",
       "door_unlock": "vol 0.7, pitch 0.95-1.05", "banner_whoosh_floor": "vol 0.6, no jitter", "banner_whoosh_boss": "vol 0.7, no jitter; start with the banner slide-in",
       "chest_open": "vol 0.7, pitch 0.95-1.05", "shrine_use": "vol 0.7, pitch 0.97-1.03", "shop_purchase": "vol 0.6, pitch 0.95-1.05"}

def main():
    out_dir = os.path.join(ROOT, "SFX", "events"); os.makedirs(out_dir, exist_ok=True)
    rows, first, seed = [], {}, 2000
    for stem, gen, nv, peak, hook, fo in SOUNDS:
        for v in range(1, nv + 1):
            seed += 1; r = np.random.default_rng(seed)
            x = gen(r)[:int(CAP[stem] * SR)]; x = finish(x, peak, fade_in=.001, fade_out=max(fo, FADE.get(stem, 0)))
            x = np.round(x * 32767) / 32767
            fn = f"{stem}_{v:02d}.wav" if nv > 1 else f"{stem}.wav"; write(os.path.join(out_dir, fn), x)
            l, unit = lufs_or_rms(x); first.setdefault(stem, []).append(x)
            rows.append((stem, fn, hook, nv, len(x) / SR, 20 * np.log10(np.abs(x).max()), l, unit))
    # map block
    L = ["<!-- BATCH2:START -->", "## Batch 2 - event SFX", "",
         "Same format/rules as Batch 1 (44.1 kHz, 16-bit, mono WAV; procedural via `tools/gen_batch2.py`; nothing heard in game yet). "
         "Tuned to A phrygian (A Bb C D E F G) to sit with the floor 1 music. LUFS are zero-padded to 1 s (relative values).", "",
         "| File | Game event / hook | Variants | Length (s) | Peak dBFS | Loudness | Suggested in-game volume / pitch jitter |", "|---|---|---|---|---|---|---|"]
    for stem, gen, nv, peak, hook, fo in SOUNDS:
        rs = [x for x in rows if x[0] == stem]
        f = f"`SFX/events/{stem}_01..{nv:02d}.wav`" if nv > 1 else f"`SFX/events/{stem}.wav`"
        L.append(f"| {f} | {hook} | {nv} | {min(x[4] for x in rs):.2f}-{max(x[4] for x in rs):.2f} | {max(x[5] for x in rs):.1f} | "
                 f"{np.mean([x[6] for x in rs]):.1f} {rs[0][7]} | {JIT[stem]} |")
    L += ["", "Notes: `ui_card_select_01..03` are tiny variants - pick randomly; `door_unlock_01..02` differ in grind roughness; "
          "`chest_open_01..02` have different glimmer notes; `shrine_use_01..02` use different chime pitches; `shop_purchase_01..02` differ in coin pitch. "
          "`banner_whoosh_floor` = floor change, `banner_whoosh_boss` = boss incoming. Preview: `previews/batch2_preview.ogg`.",
          "<!-- BATCH2:END -->"]
    mp = os.path.join(ROOT, "AUDIO_MAP.md"); s = open(mp).read()
    if "<!-- BATCH2:START -->" in s:
        a = s.index("<!-- BATCH2:START -->"); b = s.index("<!-- BATCH2:END -->") + len("<!-- BATCH2:END -->\n"); s = s[:a] + s[b:]
    s = s.rstrip("\n") + "\n\n" + "\n".join(L) + "\n"
    open(mp, "w").write(s)
    # preview: every sound in order, 0.6 s gaps
    order = [("levelup_sting", 0), ("ui_card_select", 0), ("ui_card_select", 1), ("ui_card_select", 2), ("ui_upgrade_confirm", 0), ("ui_upgrade_confirm", 1),
             ("room_cleared_sting", 0), ("door_unlock", 0), ("door_unlock", 1), ("banner_whoosh_floor", 0), ("banner_whoosh_boss", 0),
             ("chest_open", 0), ("chest_open", 1), ("shrine_use", 0), ("shrine_use", 1), ("shop_purchase", 0), ("shop_purchase", 1)]
    pv = []
    for s_, i in order: pv += [first[s_][i], np.zeros(int((.9 if first[s_][i].size > SR * .6 else .45) * SR))]
    pre = np.concatenate(pv); tmp = os.path.join(ROOT, "previews", "_t2.wav"); write(tmp, pre)
    subprocess.run(["ffmpeg", "-y", "-loglevel", "error", "-i", tmp, "-c:a", "libvorbis", "-q:a", "5", os.path.join(ROOT, "previews", "batch2_preview.ogg")], check=True)
    os.remove(tmp)
    for x in rows: print(f"{x[1]:32s} {x[4]:.2f}s peak {x[5]:.1f} {x[6]:.1f} {x[7]}")
    print("preview s", len(pre) / SR)

if __name__ == "__main__": main()
