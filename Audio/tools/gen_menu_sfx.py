"""Menu / altar UI SFX - mystic hell, quiet. 44.1 kHz 16-bit mono WAV -> SFX/ui/<hook>_NN.wav (+ end/ restart)."""
import os, subprocess, numpy as np, soundfile as sf, pyloudnorm as pyln
from scipy.signal import butter, sosfilt, fftconvolve
SR = 44100; ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
def T(d): return np.arange(int(d * SR)) / SR
def filt(x, k, fc): return sosfilt(butter(2, fc, k, fs=SR, output="sos"), x)
def mt(m): return 440 * 2 ** ((m - 69) / 12)
def bell(f, d=1.0, tau=0.4, det=1.004):
    t = T(d); return sum(a * np.sin(2 * np.pi * f * k * t) * np.exp(-t / (tau / k ** .5)) for k, a in ((1, 1), (2.76, .4), (5.4, .15))) + 0.4 * np.sin(2 * np.pi * f * det * t) * np.exp(-t / tau)
def noise(r, d, lo, hi, tau): t = T(d); return filt(filt(r.standard_normal(len(t)), "highpass", lo), "lowpass", hi) * np.exp(-t / tau)
def thump(f0, d=0.4, tau=0.12):
    t = T(d); return np.sin(2 * np.pi * np.cumsum(f0 * (1 + 0.8 * np.exp(-t / 0.02))) / SR) * np.exp(-t / tau)
def whoosh(r, d, f0, f1, up=True):
    t = T(d); n = r.standard_normal(len(t)); out = np.zeros_like(t); seg = 512
    for i in range(0, len(t), seg):
        fc = f0 + (f1 - f0) * (i / len(t)); out[i:i + seg] = filt(n[i:i + seg + 2000], "lowpass", fc)[:len(out[i:i + seg])]
    return out * np.sin(np.pi * t / t[-1]) ** (1.5 if up else 0.7)
def drone(f, d, att=0.05):
    t = T(d); x = sum(np.sin(2 * np.pi * f * k * t) / k for k in (1, 2, 3)) * np.minimum(1, t / att) * np.exp(-t / (d / 3)); return x
def place(total, *parts):
    y = np.zeros(int(total * SR))
    for o, x in parts: i = int(o * SR); n = min(len(x), len(y) - i); y[i:i + n] += x[:n]
    return y
def verb(r, x, rt=0.8, wet=0.3):
    m = int(rt * SR); ir = filt(r.standard_normal(m), "lowpass", 3500) * np.exp(-6.9 * T(rt) / rt); ir /= np.sqrt((ir ** 2).sum())
    y = np.concatenate([x, np.zeros(m)]); return y + wet * fftconvolve(y, ir)[:len(y)] * 0.5

# ---- hooks: fn(r, i) ----
def ui_hover(r, i):
    f = mt([86, 89, 84, 91][i]); return place(0.35, (0, 0.6 * bell(f, 0.3, 0.06)), (0, 0.5 * noise(r, 0.12, 2500, 7000, 0.03)))
def ui_confirm(r, i):
    m = [62, 65][i]; return place(1.0, (0, thump(70, 0.5)), (0, bell(mt(m + 12), 1.0, 0.35)), (0.07, 0.7 * bell(mt(m + 19), 0.9, 0.3)))
def ui_back(r, i):
    m = [74, 72][i]; return place(0.7, (0, 0.8 * bell(mt(m), 0.6, 0.18)), (0.08, 0.6 * bell(mt(m - 5), 0.6, 0.2)), (0, 0.6 * whoosh(r, 0.35, 2500, 500, False)))
def ui_denied(r, i):
    m = [50, 49][i]; t = T(0.45); buzz = np.sign(np.sin(2 * np.pi * mt(m) * t)) * np.exp(-t / 0.12)
    return place(0.6, (0, 0.5 * filt(buzz, "lowpass", 900)), (0.11, 0.4 * filt(buzz, "lowpass", 700)), (0, 0.6 * bell(mt(m + 25), 0.5, 0.12)))
def seal_press(r, i):
    return place(1.2, (0, 1.2 * thump(55 + 6 * i, 0.8, 0.2)), (0, 0.7 * noise(r, 0.25, 200, 1800, 0.06)), (0.02, 0.6 * bell(mt(57 + i), 1.1, 0.5)))
def altar_carousel(r, i):
    lo, hi = [(400, 2600), (2600, 400), (600, 3200)][i]
    return place(0.6, (0, 0.8 * whoosh(r, 0.45, lo, hi)), (0.3, 0.4 * bell(mt([81, 79, 84][i]), 0.3, 0.1)), (0.28, 0.3 * noise(r, 0.08, 800, 3000, 0.03)))
def pact_unlock(r, i):
    return place(1.6, (0, 0.6 * noise(r, 0.08, 1000, 6000, 0.02)), (0.05, thump(60, 0.6)), *[(0.12 + 0.12 * k, 0.6 * bell(mt(m), 1.2, 0.4)) for k, m in enumerate([62 + i, 69 + i, 74 + i])])
def pact_sign(r, i):  # quill scratch + blood-ink seal
    t = T(0.7); sc = noise(r, 0.7, 2000, 7000, 10) * (0.5 + 0.5 * np.sign(np.sin(2 * np.pi * (9 + 3 * i) * t))) * np.exp(-t / 0.4)
    return place(1.4, (0, 0.35 * sc), (0.65, thump(65, 0.5)), (0.65, 0.7 * bell(mt(55 + 7 * i), 0.8, 0.35)), (0.65, 0.5 * drone(mt(43), 0.7)))
def pact_remove(r, i):
    return place(1.0, (0, 0.6 * bell(mt(69), 0.5, 0.15)), (0.1, 0.5 * bell(mt(63), 0.6, 0.2)), (0, 0.6 * whoosh(r, 0.6, 3000, 300, False)))
def path_switch(r, i):
    return place(1.4, (0, 0.8 * whoosh(r, 0.5, 300, 2000)), (0.4, thump(60, 0.6)), (0.42, bell(mt(62), 1.0, 0.4)), (0.42, 0.6 * bell(mt(68 + i), 1.0, 0.4)))
def pact_seal_descend(r, i):
    t = T(2.0); fall = np.sin(2 * np.pi * np.cumsum(mt(50) * (1 - 0.5 * t / t[-1])) / SR) * np.exp(-t / 0.9)
    return place(2.4, (0, 1.2 * thump(50, 1.0, 0.3)), (0, 0.4 * fall), (0, 0.9 * bell(mt(50), 2.0, 0.9)), (0.1, 0.6 * bell(mt(56 + i), 2.0, 0.8)), (0.2, 0.5 * whoosh(r, 1.8, 2500, 150, False)))
def card_reroll(r, i):
    return place(0.7, *[(0.06 * k, 0.4 * noise(r, 0.05, 1500, 6000, 0.015)) for k in range(5)], (0.32, 0.6 * bell(mt(79 + 2 * i), 0.4, 0.12)), (0, 0.4 * whoosh(r, 0.4, 600, 3000)))
def card_skip(r, i):
    return place(0.6, (0, 0.6 * whoosh(r, 0.35, 2500, 400, False)), (0.05, 0.4 * bell(mt(70 - i), 0.4, 0.1)))
def restart(r, i):
    return place(1.6, (0, 0.7 * whoosh(r, 1.0, 200, 3000)), (0.85, thump(55, 0.6)), (0.85, bell(mt(62 + 5 * i), 0.8, 0.35)), (0.9, 0.5 * bell(mt(69 + 5 * i), 0.7, 0.3)))

# ---- v2 soft versions (VH: hover + node clicks too harsh) ----
def soft_tok(f0=180, d=0.25):  # rounded stone/wood tap, no sharp transient
    t = T(d); x = np.sin(2 * np.pi * f0 * t) * np.exp(-t / 0.05) + 0.3 * np.sin(2 * np.pi * f0 * 2.3 * t) * np.exp(-t / 0.025)
    return filt(x * np.minimum(1, t / 0.003), "lowpass", 1800)
def soft_bell(f, d=0.8, tau=0.3): return filt(bell(f, d, tau), "lowpass", 2500) * np.minimum(1, T(d) / 0.006)
def ui_hover(r, i):  # breath puff + faint low glint
    t = T(0.18); puff = filt(filt(r.standard_normal(len(t)), "highpass", 700 + 150 * i), "lowpass", 2200) * np.minimum(1, t / 0.02) * np.exp(-t / 0.045)
    return place(0.22, (0, puff), (0.005, 0.25 * soft_bell(mt([62, 65, 60, 67][i]) * 2, 0.2, 0.05)))
def ui_confirm(r, i):
    m = [50, 53][i]; return place(0.8, (0, 0.9 * soft_tok(150 + 20 * i)), (0.01, 0.5 * soft_bell(mt(m + 12), 0.8, 0.3)), (0.06, 0.25 * soft_bell(mt(m + 19), 0.7, 0.25)))
def pact_unlock(r, i):  # node unlock: soft tap, low bloom of two bells
    return place(1.3, (0, 0.8 * soft_tok(130)), (0.02, 0.6 * soft_bell(mt(50 + i), 1.2, 0.45)), (0.14, 0.4 * soft_bell(mt(57 + i), 1.1, 0.4)), (0.02, 0.4 * drone(mt(38 + i), 1.0, 0.08)))
def pact_sign(r, i):  # node sign: soft quill brush + muffled seal
    t = T(0.35); br = filt(filt(r.standard_normal(len(t)), "highpass", 1200), "lowpass", 3500) * np.sin(np.pi * t / t[-1]) ** 2
    return place(1.2, (0, 0.18 * br), (0.3, 0.9 * soft_tok(110)), (0.31, 0.5 * soft_bell(mt(53 + 2 * i), 0.9, 0.35)))
def ui_card_select(r, i):  # card/node focus click
    return place(0.4, (0, 0.8 * soft_tok([200, 220, 190][i], 0.15)), (0.0, 0.3 * soft_bell(mt([69, 72, 67][i]), 0.35, 0.09)))

H = [  # hook, fn, variants, peak dBFS, event, folder, suggested volume
 ("ui_hover", ui_hover, 4, -26, "menu item hover/focus (soft v2)", "ui", 0.35), ("ui_confirm", ui_confirm, 2, -21, "menu confirm / open submenu", "ui", 0.6),
 ("ui_card_select", ui_card_select, 3, -22, "card / altar node focus-select click (soft v2; overrides batch2 file)", "events", 0.45),
 ("ui_back", ui_back, 2, -19, "back / close submenu", "ui", 0.55), ("ui_denied", ui_denied, 2, -19, "locked / COMING SOON (Msg51)", "ui", 0.55),
 ("seal_press", seal_press, 2, -17, "pact seal press", "ui", 0.65), ("altar_carousel", altar_carousel, 3, -20, "altar path carousel L/R", "ui", 0.5),
 ("pact_unlock", pact_unlock, 2, -20, "pact unlocked", "ui", 0.65), ("pact_sign", pact_sign, 2, -20, "pact signed", "ui", 0.65),
 ("pact_remove", pact_remove, 2, -19, "pact removed", "ui", 0.55), ("path_switch", path_switch, 2, -17, "path switch confirm YES", "ui", 0.65),
 ("pact_seal_descend", pact_seal_descend, 2, -15, "SEAL PACT / Descend into game", "ui", 0.75), ("card_reroll", card_reroll, 2, -19, "level-up card reroll", "ui", 0.55),
 ("card_skip", card_skip, 2, -20, "level-up card skip", "ui", 0.5), ("restart", restart, 2, -17, "Dead/Won R restart", "end", 0.65),
]
def main():
    meter = pyln.Meter(SR); rows = []; pre = []
    for hk, fn, nv, pk, ev, fo, vol in H:
        od = os.path.join(ROOT, "SFX", fo); os.makedirs(od, exist_ok=True)
        for i in range(nv):
            r = np.random.default_rng(abs(hash(hk)) % 1000 + 100 * i) if False else np.random.default_rng(sum(map(ord, hk)) * 10 + i)
            x = verb(r, fn(r, i), 0.3 if hk in ("ui_hover", "ui_card_select") else 1.0, 0.12 if hk in ("ui_hover", "ui_card_select") else 0.25)
            x = filt(x, "highpass", 40); e = np.abs(x); end = np.nonzero(e > e.max() * 10 ** (-55 / 20))[0][-1] + 1; x = x[:end]
            fd = min(len(x) // 4, int(0.08 * SR)); x[-fd:] *= np.linspace(1, 0, fd) ** 2; x[:48] *= np.linspace(0, 1, 48)
            x = x / np.abs(x).max() * 10 ** (pk / 20)
            p = os.path.join(od, f"{hk}_{i + 1:02d}.wav"); sf.write(p, x, SR, subtype="PCM_16")
            y, _ = sf.read(p); L = meter.integrated_loudness(y) if len(y) > 0.4 * SR else float("nan")
            rows.append((hk, fo, f"{hk}_{i + 1:02d}.wav", len(y) / SR, 20 * np.log10(np.abs(y).max()), L, ev, vol, max(abs(y[0]), abs(y[-1]))))
            pre += [y, np.zeros(int(0.5 * SR))]
    for r_ in rows: print(f"{r_[1]}/{r_[2]} {r_[3]:.2f}s pk={r_[4]:.1f} LUFS={r_[5]:.1f} edge={r_[8]:.4f}")
    tmp = "/tmp/menusfx.wav"; sf.write(tmp, np.concatenate(pre), SR)
    subprocess.run(["ffmpeg", "-y", "-loglevel", "error", "-i", tmp, "-ac", "2", "-c:a", "libvorbis", "-q:a", "6", os.path.join(ROOT, "previews", "menu_sfx_preview.ogg")], check=True)
    return rows
def write_map(rows):
    meter = pyln.Meter(SR); mus = []
    for nm, bpm, bars, note in (("music_menu_main", 56, 16, "v3 main menu (calmer): consonant D drone (root/5th) gently breathing, soft bowed metal, low chant in 5ths, gentle minor-mode temple bells every 2 bars, one soft drum per 4 bars; no horror dissonance"),
                                ("music_menu_altar", 50, 12, "v3 altar (calmer, slightly more ritual): D drone with faint tritone shade, low chant, soft gong every 4 bars, sparse soft frame drum, minor-mode bells"),
                                ("music_stinger_dead", None, None, "Dead stinger (one-shot, before menu track): falling bells + low choir"),
                                ("music_stinger_won", None, None, "Won stinger (one-shot): rising bells + dark D-minor swell")):
        y, _ = sf.read(os.path.join(ROOT, "Music", nm + ".ogg"))
        mus.append(f"| `Music/{nm}.ogg` | {bpm or '-'} | {bars or '-'} | {len(y)/SR:.2f} s | {'loop' if bpm else 'one-shot'} | {meter.integrated_loudness(y):.1f} | {20*np.log10(np.abs(y).max()):.1f} | {note} |")
    B = ["<!-- MENU_AUDIO:START -->", "## Menu music + menu/altar UI SFX - DRAFT, not heard/wired", "",
         "### MUSIC menu (`tools/gen_menu_music.py`)", "Seamless whole-file loops (circular render), OGG 44.1 kHz stereo q6. Previews: `previews/music_menu_main_preview.ogg`, `previews/music_menu_altar_preview.ogg` (loop + 30%).", "",
         "| File | BPM | Bars | Length | Type | LUFS | Peak dBFS | Use |", "|---|---|---|---|---|---|---|---|"] + mus + [
         "", "Suggested: music bus 0.6-0.7 for loops; crossfade main <-> altar over 1.5 s; stingers at 0.7, then fade the menu loop in after ~3 s.", "",
         "### UI menu hooks (`tools/gen_menu_sfx.py`)", "44.1 kHz 16-bit mono WAV, for `HellordAudio.Play(\"<hook>\")` -> `Resources/Audio/SFX/<folder>/<hook>_NN`. Preview: `previews/menu_sfx_preview.ogg` (all, in order, 0.5 s gaps).", "",
         "| Hook | Folder | Files | Event | Lengths | Peak dBFS | Suggested vol |", "|---|---|---|---|---|---|---|"]
    from collections import OrderedDict
    g = OrderedDict()
    for hk, fo, fn, d, P, L, ev, vol, _ in rows: g.setdefault(hk, []).append((fo, fn, d, P, ev, vol))
    for hk, v in g.items():
        B.append(f"| `{hk}` | {v[0][0]} | {len(v)} (`_01`-`_{len(v):02d}`) | {v[0][4]} | {min(x[2] for x in v):.2f}-{max(x[2] for x in v):.2f} s | {v[0][3]:.0f} | {v[0][5]} |")
    B += ["", "Random variant per play (no immediate repeat). Hover: rate-limit ~80 ms. v2 (VH): new menu loops; softer ui_hover, ui_confirm, ui_card_select (node/card click), pact_unlock, pact_sign. v1 kept as `*_v1_backup.wav/.ogg`; v2 scary menu loops as `*_scary_backup.ogg`, loops now v3 calmer. Note: rerunning `gen_batch2.py` would restore the old harsh `SFX/events/ui_card_select_*`.", "<!-- MENU_AUDIO:END -->"]
    mp = os.path.join(ROOT, "AUDIO_MAP.md"); old = open(mp).read(); a, b = "<!-- MENU_AUDIO:START -->", "<!-- MENU_AUDIO:END -->"
    old = old[:old.index(a)] + "\n".join(B) + old[old.index(b) + len(b):] if a in old else old.rstrip("\n") + "\n\n" + "\n".join(B) + "\n"
    open(mp, "w").write(old)
if __name__ == "__main__": write_map(main())
