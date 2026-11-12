#!/usr/bin/env python3
"""Hellord Batch 1 combat SFX - fully procedural (numpy/scipy), no samples, no AI audio.
Usage: python3 gen_batch1.py   (writes ../SFX/*, ../AUDIO_MAP.md, ../previews/batch1_preview.ogg)
Deterministic: every variant uses a fixed seed."""
import os, subprocess, numpy as np
from scipy.signal import butter, sosfilt
import soundfile as sf

SR = 44100
ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), ".."))
SFX = os.path.join(ROOT, "SFX")

# ---------- primitives ----------
def T(d): return np.arange(int(d * SR)) / SR
def TN(n): return np.arange(n) / SR
def noise(rng, n): return rng.uniform(-1, 1, n)
def lp(x, f, o=2): return sosfilt(butter(o, min(f, 20000), 'lp', fs=SR, output='sos'), x)
def hp(x, f, o=2): return sosfilt(butter(o, f, 'hp', fs=SR, output='sos'), x)
def bp(x, lo, hi, o=2): return sosfilt(butter(o, [lo, min(hi, 20000)], 'bp', fs=SR, output='sos'), x)
def exp_env(n, tau): return np.exp(-np.arange(n) / SR / tau)
def ad(n, a, tau):  # attack then exponential decay
    e = exp_env(n, tau); na = max(1, int(a * SR)); e[:na] *= np.linspace(0, 1, na); return e
def sweep(f0, f1, n, curve=4.0):  # exponential-ish glide, returns phase
    u = np.linspace(0, 1, n); f = f1 + (f0 - f1) * np.exp(-curve * u) if curve else np.linspace(f0, f1, n)
    return 2 * np.pi * np.cumsum(f) / SR
def sine_sweep(f0, f1, n, curve=4.0): return np.sin(sweep(f0, f1, n, curve))
def tone(f, n): return np.sin(2 * np.pi * f * np.arange(n) / SR)
def sat(x, k=2.0): return np.tanh(k * x) / np.tanh(k)
def vary_filt_sweep(x, f0, f1, q_lo=0.7, blocks=24):
    """time-varying lowpass by crossfading blocks (cheap sweep)"""
    n = len(x); out = np.zeros(n); fr = np.geomspace(f0, f1, blocks); L = n // blocks + 1
    for i, f in enumerate(fr):
        seg = lp(x, f, 2); w = np.zeros(n); a, b = i * L, min(n, (i + 1) * L); w[a:b] = 1
        w = np.convolve(w, np.hanning(401) / np.hanning(401).sum(), 'same'); out += seg * w
    return out
def pad(x, n):
    y = np.zeros(n); m = min(n, len(x)); y[:m] = x[:m]; return y
def mix(*parts):
    n = max(len(p) for p in parts); return sum(pad(p, n) for p in parts)
def delay(x, s): return np.concatenate([np.zeros(int(s * SR)), x])
def crackle(rng, n, density, tau):
    """random sparse impulses with decaying density"""
    p = density * exp_env(n, tau) / SR
    imp = (rng.random(n) < p) * rng.uniform(-1, 1, n)
    return imp
def rev_tail(rng, n, tau, f=1800):
    return lp(noise(rng, n), f) * exp_env(n, tau)

def finish(x, peak_db, fade_in=0.002, fade_out=0.02, hp_f=25):
    x = hp(x, hp_f, 1)
    # trim silent tail (below -50 dB of peak), then fade
    a = np.abs(x); thr = a.max() * 10 ** (-50 / 20); idx = np.where(a > thr)[0]
    x = x[:idx[-1] + 1] if len(idx) else x
    fo = min(int(fade_out * SR), len(x) // 2); fi = int(fade_in * SR)
    x = x.copy(); x[:fi] *= np.linspace(0, 1, fi); x[-fo:] *= np.linspace(1, 0, fo) ** 1.5
    x = x / np.max(np.abs(x)) * 10 ** (peak_db / 20)
    return x

# ---------- sound designs: each f(rng) -> raw float array ----------
def enemy_hit(rng):
    d = rng.uniform(0.09, 0.13); n = int(d * SR)
    f0 = rng.uniform(150, 230); thump = sine_sweep(f0, f0 * 0.4, n, 8) * ad(n, .001, .035)
    body = lp(noise(rng, n), rng.uniform(1200, 2400)) * ad(n, .0005, rng.uniform(.018, .03))
    tick = bp(noise(rng, n), 2500, 5000) * ad(n, .0003, .006) * rng.uniform(.15, .35)
    return thump * .8 + body * .7 + tick

def enemy_death(rng):
    n = int(rng.uniform(.2, .26) * SR)
    pop = sine_sweep(rng.uniform(420, 560), 70, n, 9) * ad(n, .001, .05)
    splat = vary_filt_sweep(noise(rng, n), rng.uniform(3500, 5000), 350) * ad(n, .002, rng.uniform(.05, .08))
    # bubbly AM on the wet part
    bub = 0.6 + 0.4 * np.sin(2 * np.pi * rng.uniform(28, 45) * TN(n) + rng.uniform(0, 6))
    return pop * .6 + splat * bub * .9

def elite_death(rng):
    n = int(rng.uniform(.65, .8) * SR)
    boom = sine_sweep(rng.uniform(150, 190), 38, n, 5) * ad(n, .002, .22)
    splat = vary_filt_sweep(noise(rng, n), 5000, 250) * ad(n, .002, .16)
    bub = 0.55 + 0.45 * np.sin(2 * np.pi * rng.uniform(20, 30) * TN(n))
    pop = sine_sweep(rng.uniform(500, 650), 80, n, 10) * ad(n, .001, .07)
    growl = lp(sat(sine_sweep(110, 55, n, 3) + .5 * sine_sweep(165, 82, n, 3), 3), 700) * ad(n, .01, .25)
    tail = rev_tail(rng, n, .25, 900) * .25
    return boom * .9 + splat * bub * .7 + pop * .45 + growl * .5 + delay(tail, .03)[:n]

def fireball_cast(rng):  # v2: whoosh + low-mid roar/thump + fire crackle
    n = int(rng.uniform(.22, .29) * SR); u = np.linspace(0, 1, n)
    x = noise(rng, n); sw = np.zeros(n); B = 16; L = n // B + 1
    fc = np.geomspace(rng.uniform(450, 650), rng.uniform(2200, 3200), B)  # rising fiery whoosh
    for i, f in enumerate(fc):
        seg = bp(x, f * .55, f * 1.6); w = np.zeros(n); w[i * L:(i + 1) * L] = 1
        w = np.convolve(w, np.hanning(301) / np.hanning(301).sum(), 'same'); sw += seg * w
    env = ad(n, .012, rng.uniform(.075, .1)) * (0.6 + .4 * np.sin(np.pi * np.clip(u * 1.6, 0, 1)))
    roar = lp(sat(noise(rng, n), 2.5), rng.uniform(500, 800)) * ad(n, .008, rng.uniform(.09, .12))
    thump = sine_sweep(rng.uniform(190, 240), rng.uniform(60, 75), n, 10) * ad(n, .003, .06)
    crk = hp(crackle(rng, n, 2600, .14), 1400) * 2.2
    crk = crk + hp(noise(rng, n), 3500) * (rng.random(n) < .1) * ad(n, .001, .1) * .5  # fine fizz
    x = sw * env * 1.1 + roar * .9 + thump * .85 + crk * ad(n, .002, .12)
    return sat(x * .8, 1.6)

def fireball_impact(rng):
    n = int(rng.uniform(.38, .48) * SR)
    boom = sine_sweep(rng.uniform(110, 150), 35, n, 6) * ad(n, .002, .13)
    blast = vary_filt_sweep(noise(rng, n), rng.uniform(4000, 6000), 250) * ad(n, .001, .09)
    crk = hp(crackle(rng, n, 1200, .12), 1200) * .6
    tail = rev_tail(rng, n, .13, 700) * .3
    return boom * .9 + blast * .8 + crk + tail

def frost_cleave(rng):
    n = int(rng.uniform(.17, .23) * SR); u = np.linspace(0, 1, n)
    x = noise(rng, n); out = np.zeros(n); B = 14; L = n // B + 1
    fc = np.geomspace(rng.uniform(7000, 9000), rng.uniform(3000, 4200), B)  # descending slash
    for i, f in enumerate(fc):
        seg = bp(x, f * .75, f * 1.3); w = np.zeros(n); w[i * L:(i + 1) * L] = 1
        w = np.convolve(w, np.hanning(201) / np.hanning(201).sum(), 'same'); out += seg * w
    env = np.sin(np.pi * np.clip(u * 1.25, 0, 1) ** .6) ** 2 * np.exp(-u * 2.5)
    glass = np.zeros(n)
    for fr in rng.uniform(2200, 6500, 4):
        glass += tone(fr, n) * ad(n, .001, rng.uniform(.04, .09)) * rng.uniform(.4, 1)
    glass = delay(glass, 0.03)[:n] * .18
    return out * env * 1.4 + glass

def storm_ring(rng):  # v2: sharp zap - buzzy saw/square arcs with rapid pitch jumps + snaps
    n = int(rng.uniform(.13, .19) * SR); segs = rng.integers(6, 10); L = n // segs + 1
    f = np.zeros(n); f0 = rng.uniform(2200, 3200)
    for i in range(segs):  # hopping, falling-trend pitch
        f[i * L:(i + 1) * L] = f0 * (.75 ** i) * rng.uniform(.7, 1.5) + rng.uniform(80, 250)
    ph = np.cumsum(f) / SR; saw = 2 * (ph % 1) - 1; sq = np.sign(np.sin(2 * np.pi * ph * 1.003))
    arc = bp(.6 * saw + .4 * sq, 350, 7000) * ad(n, .0008, rng.uniform(.045, .07))
    arc *= .7 + .3 * np.sign(noise(rng, n))  # rough, unstable arc
    snaps = np.zeros(n); k = 0
    for i in range(segs):  # tiny noise burst at each pitch jump
        a = i * L; m = min(int(.006 * SR), n - a); snaps[a:a + m] += hp(noise(rng, m), 2500, 1) * np.linspace(1, 0, m) ** 2 * rng.uniform(.5, 1) * (.9 ** i)
    click = hp(noise(rng, n), 1800) * ad(n, .0002, .004) * 1.2  # sharp attack
    crk = hp(crackle(rng, n, 3500, .09), 1000) * 2.5 * ad(n, .001, .1)
    return arc * 1.0 + snaps * .9 + click + crk * .7

def bone_orbit_hit(rng):
    n = int(rng.uniform(.09, .12) * SR)
    f1 = rng.uniform(520, 760); out = np.zeros(n)
    for r, a, t in [(1, 1, .03), (2.76, .6, .02), (5.4, .3, .01)]:
        out += tone(f1 * r * rng.uniform(.98, 1.02), n) * a * ad(n, .0003, t)
    click = bp(noise(rng, n), 1800, 4500) * ad(n, .0002, .005)
    hollow = sine_sweep(rng.uniform(220, 300), 120, n, 10) * ad(n, .001, .02) * .5
    return out * .6 + click * .9 + hollow

def curse_rune_arm(rng):  # v2: detuned sub drone + reversed swell + eerie whisper
    n = int(rng.uniform(1.15, 1.3) * SR); t = TN(n); u = t / t[-1]
    f = rng.uniform(52, 58) * (1 + .08 * u)
    drone = np.zeros(n)
    for det, a in [(1.0, 1), (1.017, .8), (.983, .8), (1.5, .35), (2.03, .25)]:  # beating, dissonant fifth
        drone += a * np.sin(2 * np.pi * np.cumsum(f * det) / SR + rng.uniform(0, 6))
    drone = lp(sat(drone * .45, 2.5), 700) * (.75 + .25 * np.sin(2 * np.pi * 4.5 * t))
    # reversed swell: a dark hit played backwards, landing right at the end
    m = int(.8 * SR); hit = (lp(noise(rng, m), 1500) * ad(m, .001, .18)
                             + sine_sweep(300, 50, m, 3) * ad(m, .001, .25))
    rev = pad(np.concatenate([np.zeros(n - m), hit[::-1]])[:n], n) * .7
    # eerie whispers: band noise with slow random AM + two drifting formant bands
    base = noise(rng, n); whisp = np.zeros(n)
    for lo, hi in [(900, 1500), (2200, 3600)]:
        am = lp(np.abs(noise(rng, n)), 12, 1); am = am / am.max()
        whisp += bp(base, lo, hi) * am ** 1.5
    whisp *= .55 * np.sin(np.pi * u ** .8) ** 2
    env = np.sin(np.pi * np.clip(u ** 1.2, 0, 1)) ** .8 * .6 + .4 * np.clip(u * 4, 0, 1) * np.clip((1 - u) * 6, 0, 1)
    return drone * env * 1.0 + rev * (u ** 2) + whisp

def curse_rune_burst(rng):  # v2: sub boom + distorted crunch + dark cursed reverb tail
    from scipy.signal import fftconvolve
    nd = int(.38 * SR); tail_len = int(rng.uniform(.55, .68) * SR)
    sub = sine_sweep(rng.uniform(170, 210), rng.uniform(32, 38), nd, 6) * ad(nd, .002, .17)
    crunch = lp(sat(bp(noise(rng, nd), 90, 2200) * 3, 5), 2600) * ad(nd, .001, .075)
    ring = np.zeros(nd)
    for fr in (rng.uniform(260, 330) * np.array([1, 1.414, 2.1, 3.17])):  # tritone-ish ring
        ring += tone(fr, nd) * ad(nd, .001, .1)
    ring = lp(ring, 1400) * .18
    grit = lp(sat(noise(rng, nd), 4), 1000) * ad(nd, .004, .12) * .35
    dry = sub * .95 + crunch * .85 + ring + grit
    # cursed reverb: dark, decaying noise IR (a faint 'whisper' modulation) convolved with the dry hit
    ir = lp(noise(rng, tail_len), 2200, 2) * exp_env(tail_len, .2)
    ir = lp(ir, 900, 1) * .5 + ir * .5
    wet = fftconvolve(dry, ir)[:nd + tail_len]; wet = wet / (np.abs(wet).max() + 1e-9) * np.abs(dry).max() * .55
    return mix(dry, wet)

def hero_hurt(rng):
    n = int(rng.uniform(.2, .26) * SR); t = TN(n)
    f0 = rng.uniform(130, 170); f = f0 * (1 - .35 * np.linspace(0, 1, n) ** .7)
    ph = 2 * np.pi * np.cumsum(f) / SR
    saw = 2 * ((ph / (2 * np.pi)) % 1) - 1  # glottal-ish saw
    vowel = bp(saw, 400, 650) + .7 * bp(saw, 900, 1300) + .25 * bp(saw, 2200, 2800)  # "uh/oh" formants
    vowel *= ad(n, .008, .09) * (1 + .15 * noise(rng, n))
    thump = sine_sweep(rng.uniform(140, 180), 50, n, 9) * ad(n, .001, .04)
    breath = bp(noise(rng, n), 800, 3000) * ad(n, .002, .03) * .4
    return vowel * .9 + thump * .7 + breath

def heartbeat_loop(rng=None):
    n = int(1.6 * SR); out = np.zeros(n * 2)  # 2x for wrap-around of tails
    def beat(at, f0, amp):
        m = int(.28 * SR)
        b = sine_sweep(f0, f0 * .6, m, 6) * ad(m, .006, .06) + .35 * lp(sat(tone(f0 * 2, m)), 300) * ad(m, .006, .035)
        i = int(at * SR); out[i:i + m] += b * amp
    beat(0.00, 62, 1.0)   # lub
    beat(0.27, 52, .7)    # dub
    x = out[:n] + out[n:]  # fold tail -> seamless loop
    return x

def hero_death(rng):
    n = int(1.7 * SR)
    grunt = hero_hurt(np.random.default_rng(5))
    g = pad(sine_sweep(150, 60, int(.7 * SR), 2.5) * 0, n)
    f = 150 * np.exp(-np.linspace(0, 1, int(.8 * SR)) * 1.1)
    ph = 2 * np.pi * np.cumsum(f) / SR; saw = 2 * ((ph / (2 * np.pi)) % 1) - 1
    m = len(f); voice = (bp(saw, 350, 600) + .6 * bp(saw, 800, 1100)) * ad(m, .01, .3) * (1 + .1 * np.sin(2 * np.pi * 6 * TN(m)))
    drop = sine_sweep(180, 28, n, 2.2) * ad(n, .003, .55)
    thump = sine_sweep(130, 40, n, 10) * ad(n, .001, .1)
    tail = rev_tail(rng, n, .6, 600) * .3
    return pad(voice, n) * .5 + drop * .9 + thump * .8 + pad(grunt, n) * .5 + delay(tail, .05)[:n]

def xp_pickup(rng):
    n = int(rng.uniform(.065, .09) * SR); f0 = rng.uniform(520, 700)
    ph = sweep(f0, f0 * rng.uniform(1.6, 1.9), n, 0)  # rising "bloop"
    x = np.sin(ph) + .25 * np.sin(2 * ph)
    return x * ad(n, .002, .03)

# (file stem, category dir, generator, #variants, peak dBFS, game hook)
SOUNDS = [
 ("enemy_hit", "enemy", enemy_hit, 4, -12, "Enemy takes damage (every projectile/melee hit)"),
 ("enemy_death", "enemy", enemy_death, 4, -10, "Normal enemy dies"),
 ("elite_death", "enemy", elite_death, 2, -7, "Elite enemy dies"),
 ("fireball_cast", "fireball", fireball_cast, 4, -12, "Fireball fired"),
 ("fireball_impact", "fireball", fireball_impact, 4, -9, "Fireball hits / explodes"),
 ("frost_cleave", "skills", frost_cleave, 4, -12, "Frost Cleave swing"),
 ("storm_ring", "skills", storm_ring, 4, -13, "Storm Ring pulse / zap on enemy"),
 ("bone_orbit_hit", "skills", bone_orbit_hit, 4, -13, "Bone Orbit bone strikes an enemy"),
 ("curse_rune_arm", "skills", curse_rune_arm, 2, -12, "Curse Rune placed / arming (before burst)"),
 ("curse_rune_burst", "skills", curse_rune_burst, 3, -9, "Curse Rune detonates"),
 ("hero_hurt", "hero", hero_hurt, 3, -9, "Hero takes damage"),
 ("hero_death", "hero", hero_death, 1, -6, "Hero dies"),
 ("xp_pickup", "pickups", xp_pickup, 4, -14, "XP blood-drop collected"),
]

def lufs_or_rms(x):
    try:
        import pyloudnorm as pyln
        y = pad(x, max(len(x), SR))  # BS.1770 needs >=0.4 s; zero-pad (silence is gated out)
        return pyln.Meter(SR).integrated_loudness(y), "LUFS"
    except Exception:
        return 20 * np.log10(np.sqrt(np.mean(x ** 2)) + 1e-12), "RMS dBFS"

def write(path, x):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    sf.write(path, x.astype(np.float32), SR, subtype="PCM_16")

def main():
    rows, first = [], {}
    seed = 1000
    for stem, cat, gen, nv, peak, hook in SOUNDS:
        for v in range(1, nv + 1):
            seed += 1; rng = np.random.default_rng(seed)
            x = gen(rng)
            if stem == "low_hp_heartbeat_loop": pass
            x = finish(x, peak, fade_out=.012 if stem in ("enemy_hit", "bone_orbit_hit", "xp_pickup") else .03,
                       fade_in=.004 if stem == "curse_rune_arm" else .001)
            x = np.round(x * 32767) / 32767
            rel = f"SFX/{cat}/{stem}_{v:02d}.wav"; write(os.path.join(ROOT, rel), x)
            first.setdefault(stem, []).append(x)
            l, unit = lufs_or_rms(x)
            rows.append((rel, hook, v, len(x) / SR, 20 * np.log10(np.abs(x).max()), l, unit, stem))
    # heartbeat loop (special: exact loop length, no trim/fade)
    x = heartbeat_loop(); x = hp(x, 18, 1); x = x / np.abs(x).max() * 10 ** (-14 / 20)
    x = np.round(x * 32767) / 32767; rel = "SFX/hero/low_hp_heartbeat_loop.wav"
    write(os.path.join(ROOT, rel), x); first["low_hp_heartbeat_loop"] = [x]
    l, unit = lufs_or_rms(np.tile(x, 3))
    rows.append((rel, "Hero HP below ~25%: loop while low; fade out on heal", 1, len(x) / SR,
                 20 * np.log10(np.abs(x).max()), l, unit, "low_hp_heartbeat_loop"))
    # wrap-seam check
    seam = abs(x[0] - x[-1])

    # ---- preview: heartbeat after hurt, in game-ish order ----
    order = [("enemy_hit", 0), ("enemy_hit", 1), ("enemy_death", 0), ("enemy_death", 2), ("elite_death", 0),
             ("fireball_cast", 0), ("fireball_cast", 2), ("fireball_impact", 0), ("fireball_impact", 1),
             ("frost_cleave", 0), ("frost_cleave", 1), ("storm_ring", 0), ("storm_ring", 1),
             ("bone_orbit_hit", 0), ("bone_orbit_hit", 2), ("curse_rune_arm", 0), ("curse_rune_burst", 0),
             ("hero_hurt", 0), ("hero_hurt", 1), ("low_hp_heartbeat_loop", 0), ("hero_death", 0)]
    pv = []; gap = np.zeros(int(.4 * SR))
    for s, i in order:
        w = first[s][i]
        if s == "low_hp_heartbeat_loop": w = np.tile(w, 3)
        pv += [w, gap if s != "curse_rune_arm" else np.zeros(int(.05 * SR))]
    # xp pickups with rising pitch (+1 semitone each) to demo the pitch-step suggestion
    base = first["xp_pickup"][0]
    from scipy.signal import resample
    pv.append(gap)
    for k in range(6):
        r = 2 ** (k / 12); y = np.interp(np.arange(0, len(base), r), np.arange(len(base)), base)
        pv += [y, np.zeros(int(.12 * SR))]
    pv += [gap] + [first["xp_pickup"][j] for j in (1, 2, 3)]
    pre = np.concatenate(pv); pre *= 1.0
    os.makedirs(os.path.join(ROOT, "previews"), exist_ok=True)
    wavp = os.path.join(ROOT, "previews", "_tmp_preview.wav"); write(wavp, pre)
    subprocess.run(["ffmpeg", "-y", "-loglevel", "error", "-i", wavp, "-c:a", "libvorbis", "-q:a", "5",
                    os.path.join(ROOT, "previews", "batch1_preview.ogg")], check=True)
    os.remove(wavp)

    # ---- redo preview (v2 sounds only): every variant of the three reworked sounds ----
    rp = []
    for s_ in ("fireball_cast", "curse_rune_arm", "curse_rune_burst", "storm_ring"):
        for w in first[s_]:
            rp += [w, np.zeros(int((.9 if s_.startswith("curse") else .45) * SR))]
        rp.append(np.zeros(int(.5 * SR)))
    wavp = os.path.join(ROOT, "previews", "_tmp_preview2.wav"); write(wavp, np.concatenate(rp))
    subprocess.run(["ffmpeg", "-y", "-loglevel", "error", "-i", wavp, "-c:a", "libvorbis", "-q:a", "5",
                    os.path.join(ROOT, "previews", "batch1_redo_preview.ogg")], check=True)
    os.remove(wavp)

    # ---- map ----
    jit = {"enemy_hit": "vol 0.55-0.7, pitch 0.92-1.08, random variant, no repeat",
           "enemy_death": "vol 0.7-0.85, pitch 0.9-1.1; rate-limit ~20/s (voice cap 6)",
           "elite_death": "vol 1.0, pitch 0.95-1.05",
           "fireball_cast": "vol 0.55-0.7, pitch 0.93-1.07; rate-limit with fire rate (voice cap 4)",
           "fireball_impact": "vol 0.7-0.85, pitch 0.9-1.1; cap 4 simultaneous",
           "frost_cleave": "vol 0.6-0.75, pitch 0.95-1.05",
           "storm_ring": "vol 0.5-0.65, pitch 0.92-1.08; rate-limit per ring pulse (voice cap 3)",
           "bone_orbit_hit": "vol 0.5-0.65, pitch 0.9-1.1; rate-limit ~12/s",
           "curse_rune_arm": "vol 0.7, pitch 0.97-1.03, play once on placement",
           "curse_rune_burst": "vol 0.8-0.9, pitch 0.93-1.07; has ~0.6 s reverb tail, do not cut it off",
           "hero_hurt": "vol 0.85-1.0, pitch 0.95-1.05; 0.25 s cooldown (i-frames)",
           "low_hp_heartbeat_loop": "vol 0.5, no jitter; loop; scale speed 1.0-1.25 with lower HP if wanted",
           "hero_death": "vol 1.0, no jitter; duck other SFX",
           "xp_pickup": "vol 0.45-0.55, pitch base 0.97-1.03 then combo step (see below); voice cap 4"}
    L = ["# Hellord Audio Map - Batch 1 (combat SFX)", "",
         "All files: 44.1 kHz, 16-bit, mono WAV, generated procedurally by `tools/gen_batch1.py` (numpy/scipy; no samples, no AI audio). "
         "**Nothing has been listened to in game yet** - levels are by design (peak target), not by ear.", "",
         "Loudness: short sounds are zero-padded to 1 s before the BS.1770 measurement (pyloudnorm), so LUFS values are "
         "*relative* indicators (lower = quieter), not strict program loudness. Frequent sounds peak ~-12..-14 dBFS; "
         "rare/big ones -6..-9 dBFS. Set mixer group volumes on top (SFX ~0.8).", "",
         "| File | Game event / hook | Variants | Length (s) | Peak dBFS | Loudness | Suggested in-game volume / pitch jitter |",
         "|---|---|---|---|---|---|---|"]
    for stem, cat, gen, nv, peak, hook in SOUNDS:
        rs = [r for r in rows if r[7] == stem]
        f = f"`SFX/{cat}/{stem}_01..{nv:02d}.wav`" if nv > 1 else f"`SFX/{cat}/{stem}_01.wav`"
        ln = [r[3] for r in rs]; L.append(
            f"| {f} | {hook} | {nv} | {min(ln):.2f}-{max(ln):.2f} | {max(r[4] for r in rs):.1f} | "
            f"{np.mean([r[5] for r in rs]):.1f} {rs[0][6]} | {jit[stem]} |")
        if stem == "hero_hurt" or stem == "hero_death": pass
        if stem == "hero_hurt":
            r = [r for r in rows if r[7] == "low_hp_heartbeat_loop"][0]
            L.append(f"| `{r[0]}` | {r[1]} | loop | {r[3]:.2f} | {r[4]:.1f} | {r[5]:.1f} {r[6]} (3 loops) | {jit['low_hp_heartbeat_loop']} |")
    L += ["", "## Loop points", "",
          f"- `SFX/hero/low_hp_heartbeat_loop.wav`: loop start = sample 0, loop end = sample {int(1.6*SR)} (1.600 s, whole file; "
          f"file length {len(x)} samples). Beats are lub at 0.00 s and dub at 0.27 s (one lub-dub pair per 1.6 s = 37.5 pairs/min, slow and ominous); tails are "
          f"folded around the end so it is seamless (wrap sample step {seam*32767:.0f}/32767). In Unity: `AudioSource.loop = true`, no extra trimming.",
          "- Other sounds are one-shots (`curse_rune_arm` is a ~1 s one-shot meant to be followed by `curse_rune_burst`).", "",
          "## XP pickup combo pitch", "",
          "Raise pitch on fast consecutive pickups: if the previous pickup was < 0.5 s ago, `step = min(step + 1, 12)`, "
          "else `step = 0`. `AudioSource.pitch = 2^(step/12) * random(0.97, 1.03)` (+1 semitone per pickup, cap +12). "
          "The preview ends with a 6-step ascending demo. Because pitch also shortens the clip, keep a voice cap of ~4.", "",
          "## Notes", "",
          "- Never play the same variant twice in a row; pick randomly from the variants.",
          "- Suggested voice caps/rate limits are in the table; hits and pickups are the most frequent sounds - keep them low.",
          "- Preview: `previews/batch1_preview.ogg` (order: enemy_hit x2, enemy_death x2, elite_death, fireball_cast x2, fireball_impact x2, "
          "frost_cleave x2, storm_ring x2, bone_orbit_hit x2, curse_rune_arm -> burst, hero_hurt x2, heartbeat x3 loops, hero_death, xp pickup ascending then 3 variants). "
          "It is rendered at the files' native (quiet) levels; turn the volume up.",
          "- v2 (after first in-game test): `fireball_cast`, `curse_rune_arm`, `curse_rune_burst`, `storm_ring` were redesigned (same names/variant counts); `previews/batch1_redo_preview.ogg` has only these. All other files are unchanged (deterministic seeds).",
          "- Regenerate: `python3 tools/gen_batch1.py` (deterministic seeds)."]
    mp = os.path.join(ROOT, "AUDIO_MAP.md"); keep = ""
    if os.path.exists(mp):  # preserve the music block written by gen_music_floor1.py
        old = open(mp).read()
        idx = [old.index(m) for m in ("<!-- MUSIC:START -->", "<!-- BATCH2:START -->", "<!-- BATCH3:START -->", "<!-- MUSIC_OPTIONS:START -->", "<!-- BOSS_MUSIC:START -->", "<!-- HELL_SCREAMS:START -->", "<!-- MENU_AUDIO:START -->") if m in old]
        if idx: keep = "\n" + old[min(idx):]  # preserve every later-added block
    open(mp, "w").write("\n".join(L) + "\n" + keep)
    print("preview s:", len(pre) / SR)

if __name__ == "__main__": main()
