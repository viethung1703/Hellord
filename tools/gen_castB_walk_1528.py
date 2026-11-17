"""t1528 v2: walk-based TAIL-cast frames (no new design, built in code from existing art).
For each walk frame w (hero_36_base frames 8-15) and each cast frame k (hero_castB_tail_body_36 f0-f4, plus the blink), the walk frame keeps its
legs + body bob; its tail (x >= TAIL_X, y >= TAIL_Y + bob) is swapped for the cast frame's tail (shifted by the bob); cast f1 gets the same 1-px squash
as HA's idle cast (row 21 removed, rows 0-20 moved down 1, body columns only); the blink uses HA's ramp remap (b0122e->ec2438, ec2438->ff6a58).
Output: Resources/HellordArt/Hero36/hero_castB_tail_walk_36.png, one row of 48 frames 36x36 (pivot 18,35): index = w * 6 + k, k 0-4 cast, k 5 blink."""
import os, numpy as np
from PIL import Image
D = os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))), "Assets/Hellord/Resources/HellordArt/Hero36")
TAIL_X, TAIL_Y, SQUASH_ROW, SQUASH_F, REL = 28, 25, 21, 1, 2
base = np.array(Image.open(f"{D}/hero_36_base.png").convert("RGBA")); cast = np.array(Image.open(f"{D}/hero_castB_tail_body_36.png").convert("RGBA"))
F = lambda a, i: a[:, 36 * i:36 * i + 36].copy()
idle0 = F(base, 0)
def shift(a, dy):
    o = np.zeros_like(a)
    if dy >= 0: o[dy:] = a[:36 - dy]
    else: o[:dy] = a[-dy:]
    return o
def bob(w):  # vertical body offset of a walk frame vs idle 0 (best match of the body columns above the feet)
    best = None
    for dy in range(-3, 4):
        s = shift(idle0, dy); m = np.any(s[:32, :TAIL_X] != w[:32, :TAIL_X], axis=2).sum()
        if best is None or m < best[0]: best = (m, dy)
    return best[1]
REMAP = {(0xb0, 0x12, 0x2e): (0xec, 0x24, 0x38), (0xec, 0x24, 0x38): (0xff, 0x6a, 0x58)}
out = np.zeros((36, 36 * 48, 4), np.uint8); rep = []
for wi in range(8):
    w = F(base, 8 + wi); dy = bob(w); rep.append(dy)
    for k in range(6):
        ck = F(cast, REL if k == 5 else k); f = w.copy()
        f[max(0, TAIL_Y + dy):, TAIL_X:] = 0                                   # drop the walk tail
        if k == SQUASH_F:                                                     # 1-px squash, body columns only (feet stay)
            r = SQUASH_ROW + dy; b = f[:, :TAIL_X].copy(); b[1:r + 1] = f[0:r, :TAIL_X]; b[0] = 0; f[:, :TAIL_X] = b
        ct = shift(ck, dy); reg = np.zeros((36, 36), bool); reg[max(0, TAIL_Y + dy):, TAIL_X:] = True
        f[reg & (ct[:, :, 3] > 0)] = ct[reg & (ct[:, :, 3] > 0)]               # paste the cast tail at the walk bob
        if k == 5:
            for y in range(36):
                for x in range(36):
                    if f[y, x, 3] and tuple(f[y, x, :3]) in REMAP: f[y, x, :3] = REMAP[tuple(f[y, x, :3])]
        out[:, 36 * (wi * 6 + k):36 * (wi * 6 + k + 1)] = f
Image.fromarray(out).save(f"{D}/hero_castB_tail_walk_36.png"); print("bob per walk frame", rep)
