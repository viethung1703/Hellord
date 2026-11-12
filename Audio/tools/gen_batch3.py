#!/usr/bin/env python3
"""Hellord Batch 3 - enemy + Ox-Head boss SFX (procedural numpy/scipy). Reuses helpers from gen_batch1/2.
Writes ../SFX/enemies/*.wav and ../SFX/boss/*.wav, a <!-- BATCH3 --> block in ../AUDIO_MAP.md, ../previews/batch3_preview.ogg."""
import os, sys, subprocess, numpy as np, soundfile as sf
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from gen_batch1 import (SR, ROOT, lp, hp, bp, ad, sat, tone, sine_sweep, noise, crackle, finish, pad, mix, delay, exp_env, TN,
                        vary_filt_sweep, lufs_or_rms, write)
from gen_batch2 import reverb, bell, chip, note_at, mtof

def saw_f(f, n):  # saw from a frequency contour (array) or constant
    f = np.broadcast_to(f, (n,)); ph = np.cumsum(f) / SR; return 2 * (ph % 1) - 1
def formants(x, bands):  # bands: [(lo, hi, amp)]
    return sum(a * bp(x, lo, hi) for lo, hi, a in bands)
def morph(x, v1, v2, curve):  # crossfade between two vowel filters over time
    return formants(x, v1) * (1 - curve) + formants(x, v2) * curve
def clink(r, f, tau, n=None):
    n = n or int(.12 * SR)
    return (tone(f, n) + .6 * tone(f * r.uniform(2.3, 2.9), n) + .3 * tone(f * r.uniform(4.1, 5.2), n)) * ad(n, .0002, tau)

# ---------- Hook Thrower ----------
def hook_windup_rattle(r):
    n = int(r.uniform(.6, .75) * SR); out = np.zeros(n); t = 0.0; k = 0
    while t < n / SR - .1:
        a = (.25 + .75 * t / (n / SR)) * r.uniform(.6, 1)
        c = pad(clink(r, r.uniform(1400, 3200), r.uniform(.012, .025)) * a, int(.12 * SR)); i = int(t * SR)
        out[i:i + len(c)] += c[:n - i]
        t += max(.032, .1 - .07 * t / (n / SR)) * r.uniform(.8, 1.2); k += 1
    rum = lp(noise(r, n), 220) * np.linspace(.3, 1, n) * .6
    return out * .8 + rum * .5
def hook_chain_throw(r):
    n = int(r.uniform(.32, .4) * SR); u = np.linspace(0, 1, n); x = noise(r, n); sw = np.zeros(n); B = 16; L = n // B + 1
    fc = 500 + 3200 * np.sin(np.pi * np.clip(u * 1.3, 0, 1)) ** 1  # rises then falls (pass-by)
    fcs = np.geomspace(600, 3600, B // 2).tolist() + np.geomspace(3600, 900, B - B // 2).tolist()
    for i, f in enumerate(fcs):
        seg = bp(x, f * .6, f * 1.5); w = np.zeros(n); w[i * L:(i + 1) * L] = 1
        w = np.convolve(w, np.hanning(401) / np.hanning(401).sum(), 'same'); sw += seg * w
    env = np.sin(np.pi * u ** .8) ** 1.5
    crack = hp(noise(r, n), 3000) * ad(n, .0002, .006) * .6
    jing = np.zeros(n)
    for k in range(4): jing = note_at(jing, clink(r, r.uniform(1800, 3500), .02, int(.1 * SR)) * (.45 * .7 ** k), .1 + .07 * k + r.uniform(0, .02))
    return sw * env * .9 + crack + pad(jing, n)
def hook_hit(r):
    n = int(r.uniform(.28, .34) * SR)
    f = r.uniform(380, 520); clank = sum(a * tone(f * q * r.uniform(.99, 1.01), n) * ad(n, .0003, t) for q, a, t in [(1, 1, .07), (2.4, .6, .05), (3.7, .4, .03), (5.6, .25, .02)])
    scrape = bp(noise(r, n), 1500, 5000) * ad(n, .005, .05) * (.6 + .4 * np.sign(np.sin(2 * np.pi * r.uniform(60, 90) * TN(n)))) * .4
    thud = sine_sweep(r.uniform(130, 170), 48, n, 9) * ad(n, .001, .07)
    snag = delay(clink(r, r.uniform(2000, 3000), .015) * .5, .06)
    return mix(clank * .5, scrape, thud * .9, snag)

# ---------- Gossip Imp ----------
def gossip_shout_warning(r):
    n = int(r.uniform(.48, .55) * SR); syl = int(r.integers(3, 5)); out = np.zeros(n); L = n // syl
    for i in range(syl):  # shrill chattering syllables, each with its own pitch jump
        m = int(L * .8); f0 = r.uniform(620, 950) * (1 + .12 * i * r.choice([-1, 1]))
        f = f0 * (1 + .25 * np.linspace(0, 1, m) ** 2) * (1 + .02 * np.sin(2 * np.pi * 38 * np.arange(m) / SR))
        g = saw_f(f, m); v1 = [(700, 1000, 1), (2200, 2800, .8), (3300, 3900, .3)]; v2 = [(450, 650, 1), (1800, 2300, .8), (3100, 3600, .3)]
        s = morph(g, v1, v2, np.linspace(0, 1, m)) * ad(m, .006, .07) * np.sin(np.pi * np.linspace(0, 1, m)) ** .3
        s += hp(noise(r, m), 4500) * ad(m, .002, .02) * .12  # sibilant 'ss'
        i0 = i * L; out[i0:i0 + m] += s
    return sat(out * 1.5, 1.5)
def gossip_shout_blast(r):
    n = int(r.uniform(.38, .45) * SR); t = TN(n)
    f = (r.uniform(300, 380) * (1 - .45 * np.clip(t / .3, 0, 1) ** .7)); g = saw_f(f, n)
    v = morph(g, [(600, 900, 1), (1000, 1500, .8), (2300, 2900, .5)], [(400, 600, 1), (800, 1200, .6), (2000, 2500, .3)], np.clip(t / .35, 0, 1))
    v *= ad(n, .004, .1) * (1 + .2 * np.sin(2 * np.pi * 45 * t))
    burst = bp(noise(r, n), 1200, 6000) * ad(n, .0005, .05) * .8
    thump = sine_sweep(190, 55, n, 10) * ad(n, .001, .07) * .9
    ring = sine_sweep(2800, 700, n, 8) * ad(n, .001, .06) * .25
    return sat(mix(v * 1.1, burst, thump, ring), 1.8)

# ---------- Ox-Head boss ----------
def roar_core(r, n, f0, drive=3.0, growl=40):
    t = TN(n); u = t / t[-1]
    f = f0 * (1 + .35 * np.sin(np.pi * np.clip(u * 1.2, 0, 1)) - .25 * u) * (1 + .04 * lp(noise(r, n), 30, 1) * 8)  # rise-fall + jitter
    g = saw_f(f, n); g = g * (.65 + .35 * np.sign(np.sin(2 * np.pi * growl * t))) * (.8 + .2 * lp(noise(r, n), 120, 1) * 6)  # growl AM
    v = morph(g, [(250, 450, 1), (600, 900, .7), (1000, 1500, .3)], [(450, 750, 1), (900, 1400, .8), (1800, 2400, .3)], np.sin(np.pi * u) ** 1.2)
    v = lp(sat(v * drive, 2.5), 3500) + .2 * lp(sat(g, 3), 300)
    return v * np.sin(np.pi * np.clip(u, 0, 1) ** .6) ** .8
def oxhead_roar(r):
    n = int(r.uniform(1.4, 1.55) * SR)
    x = roar_core(r, n, r.uniform(68, 85)) + bp(noise(r, n), 300, 1500) * .25 * np.sin(np.pi * np.linspace(0, 1, n))
    return reverb(x, .5, 1800, .25)
def oxhead_horn_windup(r):
    n = int(r.uniform(.75, .9) * SR); u = np.linspace(0, 1, n)
    f = 110 * (1 + .5 * u ** 1.5) * r.uniform(.97, 1.03); g = saw_f(f, n) + .6 * saw_f(f * 1.012, n)
    sweep = vary_filt_sweep(g, 300, 2600)  # brass-like opening filter
    scr = bp(noise(r, n), 400, 1800) * (.5 + .5 * np.sign(np.sin(2 * np.pi * 55 * TN(n)))) * u * .3
    rum = lp(noise(r, n), 120) * u * .8
    return sat(sweep * u ** .8 * .9 + scr + rum, 1.5) * np.clip((1 - u) * 12, 0, 1)
def oxhead_horn_strike(r):
    n = int(r.uniform(.45, .55) * SR)
    clang = sum(a * tone(f * r.uniform(.99, 1.01), n) * ad(n, .0003, t) for f, a, t in
                [(r.uniform(300, 380), 1, .2), (r.uniform(780, 900), .7, .12), (r.uniform(1600, 2100), .45, .07), (r.uniform(3000, 3800), .25, .04)])
    thud = sine_sweep(r.uniform(120, 150), 38, n, 7) * ad(n, .001, .14)
    crunch = lp(sat(bp(noise(r, n), 150, 3000) * 3, 4), 3500) * ad(n, .0005, .05)
    return reverb(mix(clang * .35, thud * 1.0, crunch * .6), .3, 2000, .3)
def oxhead_stomp_windup(r):
    n = int(r.uniform(.65, .75) * SR); u = np.linspace(0, 1, n)
    rum = lp(noise(r, n), 180) * u ** 1.5 * 1.3
    sub = np.sin(2 * np.pi * np.cumsum(32 + 28 * u ** 1.5) / SR) * u ** 1.3
    deb = hp(crackle(r, n, 1500, 1.0), 800) * u ** 2 * 2.0
    deb = lp(deb, 5000)
    return (rum + sub * .9 + deb * .6) * np.clip((1 - u) * 15, 0, 1)
def oxhead_stomp_strike(r):
    n = int(r.uniform(.85, 1.0) * SR)
    sub = sine_sweep(r.uniform(85, 100), 27, n, 4) * ad(n, .001, .3)
    body = sine_sweep(r.uniform(150, 180), 55, n, 9) * ad(n, .001, .09) * .7
    crack = lp(noise(r, n), 2500) * ad(n, .0005, .03)
    deb = np.zeros(n)
    for k in range(r.integers(10, 16)):  # falling debris: little noisy ticks scattered over 0.1-0.6 s
        t0 = r.uniform(.04, .6); m = int(r.uniform(.01, .03) * SR); i = int(t0 * SR)
        if i + m < n: deb[i:i + m] += bp(noise(r, m), r.uniform(800, 3500), r.uniform(4000, 7000)) * ad(m, .0003, .006) * r.uniform(.15, .5) * (1 - t0)
    return reverb(mix(sub * 1.1, body, crack * .6, deb), .45, 1500, .35)
def oxhead_lunge_dash(r):
    n = int(r.uniform(.6, .72) * SR); u = np.linspace(0, 1, n)
    x = noise(r, n); sw = vary_filt_sweep(x, 250, 1800)[:n] * (np.sin(np.pi * np.clip(u * 1.4, 0, 1)) ** 1.5)
    sw2 = vary_filt_sweep(x, 1800, 250)
    mixed = np.concatenate([sw[:n // 2], pad(sw2[n // 2:], n - n // 2) * np.linspace(1, 0, n - n // 2) ** 1.2])
    grow = lp(sat(saw_f(70 + 55 * np.sin(np.pi * u * .9), n) * .5, 2.5), 500) * np.sin(np.pi * u) ** 1.2
    sub = np.sin(2 * np.pi * np.cumsum(38 + 30 * u) / SR) * np.sin(np.pi * u) * .8
    return mixed * 1.1 + grow * .7 + sub * .6
def oxhead_phase_change(r):
    n = int(2.2 * SR); u = np.linspace(0, 1, n); t = TN(n)
    f = 55 * np.exp(-u * .35); dr_ = sum(a * saw_f(f * d, n) for d, a in [(1, 1), (1.0595 * 1.01, .6), (2.0, .3)])  # A1 vs Bb beating, sinking
    dr_ = lp(sat(dr_ * .35, 3), 600) * ad(n, .15, .9)
    m = int(1.0 * SR); hit = lp(noise(r, m), 1800) * ad(m, .001, .12) + sine_sweep(300, 45, m, 3) * ad(m, .001, .3)
    swell = pad(np.concatenate([np.zeros(n - m), hit[::-1]])[:int(1.0 * SR)], n) * 0
    rs = int(.9 * SR); rev = np.zeros(n); rev[int(.2 * SR):int(.2 * SR) + rs] = (lp(noise(r, rs), 1500) * ad(rs, .001, .2) + sine_sweep(350, 50, rs, 3) * ad(rs, .001, .25))[::-1] * .6
    rn = int(1.0 * SR); roar = roar_core(r, rn, 62, 3.5, 35) * np.sin(np.pi * np.linspace(0, 1, rn)) ** .5
    big = mix(sine_sweep(120, 30, int(1.4 * SR), 3.5) * ad(int(1.4 * SR), .002, .4), lp(sat(noise(r, int(.5 * SR)), 3), 1200) * ad(int(.5 * SR), .001, .15) * .6)
    bellx = bell(mtof(58), int(1.2 * SR), r, ratios=(1, 2.76, 5.4), amps=(1, .5, .25), tau=.8, detune=.006) * .12
    x = mix(dr_ * .8, rev * .8, delay(roar, .9) * .9, delay(big, .9) * 1.0, delay(bellx, 1.0))
    return reverb(x, .8, 1800, .4)

# (stem, folder, gen, variants, peak, hook, fade_out, cap seconds)
SOUNDS = [
 ("hook_windup_rattle", "enemies", hook_windup_rattle, 2, -13, "Hook Thrower begins wind-up (telegraph)", .06, .8),
 ("hook_chain_throw", "enemies", hook_chain_throw, 3, -12, "Hook Thrower throws the hook/chain", .06, .45),
 ("hook_hit", "enemies", hook_hit, 3, -10, "Hook connects with the hero", .06, .4),
 ("gossip_shout_warning", "enemies", gossip_shout_warning, 2, -11, "Gossip Imp starts shout telegraph", .06, .6),
 ("gossip_shout_blast", "enemies", gossip_shout_blast, 2, -9, "Gossip Imp shout blast fires", .06, .5),
 ("oxhead_roar", "boss", oxhead_roar, 2, -7, "Ox-Head intro / enrage roar", .2, 1.7),
 ("oxhead_horn_windup", "boss", oxhead_horn_windup, 2, -12, "Ox-Head horn attack telegraph", .05, .9),
 ("oxhead_horn_strike", "boss", oxhead_horn_strike, 3, -7, "Ox-Head horn attack lands", .15, .75),
 ("oxhead_stomp_windup", "boss", oxhead_stomp_windup, 2, -10, "Ox-Head stomp telegraph", .05, .75),
 ("oxhead_stomp_strike", "boss", oxhead_stomp_strike, 3, -6, "Ox-Head stomp lands (shockwave)", .25, 1.4),
 ("oxhead_lunge_dash", "boss", oxhead_lunge_dash, 2, -9, "Ox-Head lunge dash", .1, .8),
 ("oxhead_phase_change", "boss", oxhead_phase_change, 1, -6, "Ox-Head phase change (HP threshold)", .4, 2.2),
]
JIT = {"hook_windup_rattle": "vol 0.6, pitch 0.95-1.05", "hook_chain_throw": "vol 0.6, pitch 0.93-1.07", "hook_hit": "vol 0.8, pitch 0.95-1.05; play with hero_hurt",
       "gossip_shout_warning": "vol 0.6, pitch 0.9-1.1 (per imp)", "gossip_shout_blast": "vol 0.75, pitch 0.95-1.05; cap 2 simultaneous",
       "oxhead_roar": "vol 0.9, pitch 0.97-1.03", "oxhead_horn_windup": "vol 0.8, no jitter; length ~ attack telegraph, trim/fade by code if shorter",
       "oxhead_horn_strike": "vol 0.9, pitch 0.96-1.04", "oxhead_stomp_windup": "vol 0.8, pitch 0.97-1.03", "oxhead_stomp_strike": "vol 1.0, pitch 0.95-1.05; shake camera with it",
       "oxhead_lunge_dash": "vol 0.8, pitch 0.95-1.05", "oxhead_phase_change": "vol 1.0, no jitter; duck music ~6 dB, other SFX ~3 dB"}

def main():
    rows, first, seed = [], {}, 3000
    for stem, folder, gen, nv, peak, hook, fo, cap in SOUNDS:
        d = os.path.join(ROOT, "SFX", folder); os.makedirs(d, exist_ok=True)
        for v in range(1, nv + 1):
            seed += 1; r = np.random.default_rng(seed)
            x = gen(r)[:int(cap * SR)]; x = finish(x, peak, fade_in=.001, fade_out=fo)
            x = np.round(x * 32767) / 32767
            fn = f"{stem}_{v:02d}.wav" if nv > 1 else f"{stem}.wav"; write(os.path.join(d, fn), x)
            l, unit = lufs_or_rms(x); first.setdefault(stem, []).append(x)
            rows.append((stem, folder, fn, len(x) / SR, 20 * np.log10(np.abs(x).max()), l, unit))
    L = ["<!-- BATCH3:START -->", "## Batch 3 - enemy and boss SFX", "",
         "Same format/rules as earlier batches (44.1 kHz, 16-bit, mono WAV; procedural via `tools/gen_batch3.py`; nothing heard in game yet). "
         "Boss peaks <= -6 dBFS. LUFS are zero-padded to 1 s (relative values).", "",
         "| File | Game event / hook | Variants | Length (s) | Peak dBFS | Loudness | Suggested in-game volume / pitch jitter |", "|---|---|---|---|---|---|---|"]
    for stem, folder, gen, nv, peak, hook, fo, cap in SOUNDS:
        rs = [x for x in rows if x[0] == stem]
        f = f"`SFX/{folder}/{stem}_01..{nv:02d}.wav`" if nv > 1 else f"`SFX/{folder}/{stem}.wav`"
        L.append(f"| {f} | {hook} | {nv} | {min(x[3] for x in rs):.2f}-{max(x[3] for x in rs):.2f} | {max(x[4] for x in rs):.1f} | "
                 f"{np.mean([x[5] for x in rs]):.1f} {rs[0][6]} | {JIT[stem]} |")
    L += ["", "Notes: pick a random variant each time, never the same twice in a row. Reverb tails on boss sounds are cut with fades at the length shown. "
          "Preview: `previews/batch3_preview.ogg`.", "<!-- BATCH3:END -->"]
    mp = os.path.join(ROOT, "AUDIO_MAP.md"); s = open(mp).read()
    if "<!-- BATCH3:START -->" in s:
        a = s.index("<!-- BATCH3:START -->"); b = s.index("<!-- BATCH3:END -->") + len("<!-- BATCH3:END -->\n"); s = s[:a] + s[b:]
    open(mp, "w").write(s.rstrip("\n") + "\n\n" + "\n".join(L) + "\n")
    pv = []
    for stem, folder, gen, nv, *_ in SOUNDS:
        for w in first[stem]: pv += [w, np.zeros(int((.9 if len(w) > .6 * SR else .45) * SR))]
        pv.append(np.zeros(int(.3 * SR)))
    pre = np.concatenate(pv); tmp = os.path.join(ROOT, "previews", "_t3.wav"); write(tmp, pre)
    subprocess.run(["ffmpeg", "-y", "-loglevel", "error", "-i", tmp, "-c:a", "libvorbis", "-q:a", "5", os.path.join(ROOT, "previews", "batch3_preview.ogg")], check=True)
    os.remove(tmp)
    for x in rows: print(f"{x[1]}/{x[2]:32s} {x[3]:.2f}s peak {x[4]:.1f} {x[5]:.1f} {x[6]}")
    print("preview s", len(pre) / SR)

if __name__ == "__main__": main()
