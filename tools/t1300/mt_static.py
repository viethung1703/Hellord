"""t1300: static layer of the D2 Morta v6 (lava dimB) Sins screen, exported for the runtime. Uses the t1287 engine UNCHANGED (read-only import).
Static = arch, pillars, floor, sconce straps + rims, title, panels, rail, divider. Not drawn here: flame, coals, bowl front, embers, light, cells, niches, text, buttons, tabs."""
import sys; sys.dont_write_bytecode = True
W0 = '/workspace/Hellord/Art/previews/t1287_redraw/work'
sys.path.insert(0, W0)
import numpy as np
from PIL import Image
import eng2 as E
from eng2 import PAL
import backdrop as B, scene5 as SCN, d2_ui as S
import screen5 as SC
KEYA, KEYB = (255, 0, 255), (0, 255, 0)     # off-palette keys
def strap(cv, cx, rim_y):
    px0 = cx - 3; py0 = rim_y + 20
    for j in range(20):
        for i in range(7):
            top = j == 0; bot = j == 19; l = i == 0; r = i == 6
            if (top or bot) and (i < 1 or i > 5): continue
            if top or bot or l or r: c = 'DEEP'
            elif i == 1: c = 'B2'
            elif i == 5: c = 'N2'
            else: c = 'P2'
            if (j, i) in ((3, 3), (16, 3)): c = 'T2'
            cv.p(px0 + i, py0 + j, c)
def static_screen(key):
    cv = E.Cv('INK'); cv.a[:] = key
    B.ST.pop(id(cv), None); B.stone(cv); SCN.reset(cv); SCN.floor(cv); SCN.arch(cv); SCN.pillars(cv)
    S.title(cv, 0.15)
    SCN._track(cv)
    for cx in (21, 619):
        strap(cv, cx, 214); SCN.rim(cv, cx, 214)
    SCN._untrack(cv)
    GR, DE = SC.GR, SC.DE
    S.rail(cv, GR['x'], 121 + 15, DE['x'] + DE['w'] - GR['x'])
    gi = S.panel(cv, **GR); di = S.panel(cv, **DE)
    ix = di[0] + 10; iy = di[1] + 10; sy = iy + SC.CELL + 8
    S.divider(cv, ix, sy, di[2] - 20)
    return cv, dict(gi=gi, di=di, divider=(ix, sy, di[2] - 20))
def build_rgba():
    a, info = static_screen(KEYA); b, _ = static_screen(KEYB)
    diff = (a.a != b.a).any(axis=2)           # pixels that follow the key colour = untouched background
    rgba = np.zeros((360, 640, 4), np.uint8); rgba[..., :3] = a.a; rgba[..., 3] = np.where(diff, 0, 255)
    rgba[diff, :3] = 0
    return rgba, info
if __name__ == '__main__':
    rgba, info = build_rgba(); print(info, int((rgba[..., 3] == 0).sum()), 'transparent px')
