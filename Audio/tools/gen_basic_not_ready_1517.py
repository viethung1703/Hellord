"""t1517 PLACEHOLDER (code-synth, no third-party audio): short dull 'not ready' click for a basic attack clicked during its cooldown.
Low muffled thump (110 Hz, 30 ms decay) + a lowpassed noise tick, 70 ms, peak -12 dBFS. Writes Resources/Audio/SFX/ui/basic_not_ready_0{1,2}.wav."""
import os, numpy as np, wave
SR = 44100; OUT = os.path.join(os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__)))), "Assets/Hellord/Resources/Audio/SFX/ui")
def lp(x, a):
    y = np.zeros_like(x); acc = 0.0
    for i, v in enumerate(x): acc += a * (v - acc); y[i] = acc
    return y
for k, (f0, seed) in enumerate(((110.0, 1), (98.0, 2)), 1):
    n = int(0.07 * SR); t = np.arange(n) / SR; r = np.random.default_rng(seed)
    thump = np.sin(2 * np.pi * np.cumsum(f0 * (1 + 0.6 * np.exp(-t / 0.006))) / SR) * np.exp(-t / 0.03)
    tick = lp(r.standard_normal(n), 0.12) * np.exp(-t / 0.006) * 0.8
    x = thump + tick; x *= np.minimum(1, t / 0.001); x = x / np.max(np.abs(x)) * 10 ** (-12 / 20)
    with wave.open(os.path.join(OUT, f"basic_not_ready_{k:02d}.wav"), "wb") as w:
        w.setnchannels(1); w.setsampwidth(2); w.setframerate(SR); w.writeframes((x * 32767).astype("<i2").tobytes())
print("ok")
