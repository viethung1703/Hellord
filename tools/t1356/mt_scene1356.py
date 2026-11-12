"""t1356: re-lays-out the static layer of the menu-theme Sins screen (counter plates beside the title, tabs/slots/panels moved up, taller panels).
Uses the t1287 engine unchanged (read-only import) and writes ONLY Assets/Hellord/Resources/HellordArt/MenuTheme/mt_scene.png."""
import sys; sys.dont_write_bytecode = True
W0 = '/workspace/Hellord/Art/previews/t1287_redraw/work'
sys.path.insert(0, W0); sys.path.insert(0, '/workspace/Hellord/tools/t1300')
import numpy as np
from PIL import Image
import eng2 as E
import backdrop as B, scene5 as SCN, d2_ui as S, screen5 as SC
import mt_static as MS
OUT = '/workspace/Hellord/Assets/Hellord/Resources/HellordArt/MenuTheme/mt_scene.png'
TITLE_Y = 53                      # was 50
PLATE_W, PLATE_H = 88, 18; PLATE_L, PLATE_R = 150, 640 - 150 - 88; PLATE_Y = 49   # >= 7 px clear of the arch curve (checked by arch_clearance.py), 10 px from the title
TY = 116                          # tabs row (was 121)
RAIL_Y = TY + 15
PY = RAIL_Y + 6; PB = 312         # panel top (was 142) / bottom (unchanged)
GH = PB - PY
GR = dict(x=SC.GR['x'], y=PY, w=SC.GR['w'], h=GH); DE = dict(x=SC.DE['x'], y=PY, w=SC.DE['w'], h=GH)
def static_screen(key):
    cv = E.Cv('INK'); cv.a[:] = key
    B.ST.pop(id(cv), None); B.stone(cv); SCN.reset(cv); SCN.floor(cv); SCN.arch(cv); SCN.pillars(cv)
    t = 'INFERNAL SINS'; w = S.TITLE.width(t)
    S.TITLE.draw(cv, 320 - w // 2, TITLE_Y, t, 'BONE', leg={'+': 'BONE', '=': 'TERRA'}, role='title', outline='DEEP', rowcols=['BONE'] * 6 + ['CLAY'] * 3 + ['TERRA'] * 2)
    S.counter(cv, PLATE_L, PLATE_Y, PLATE_W, PLATE_H, ''); S.counter(cv, PLATE_R, PLATE_Y, PLATE_W, PLATE_H, '')
    SCN._track(cv)
    for cx in (21, 619):
        MS.strap(cv, cx, 214); SCN.rim(cv, cx, 214)
    SCN._untrack(cv)
    S.rail(cv, GR['x'], RAIL_Y, DE['x'] + DE['w'] - GR['x'])
    gi = S.panel(cv, **GR); di = S.panel(cv, **DE)
    ix = di[0] + 10; iy = di[1] + 10; sy = iy + SC.CELL + 8
    S.divider(cv, ix, sy, di[2] - 20)
    return cv, dict(gi=gi, di=di, divider=(ix, sy, di[2] - 20))
TINT_A = 140     # 55 % black
def tint_interiors(rgba, seeds):
    """Opaque dark panel/plate interiors -> 55 % black (0,0,0,140): 4-connected flood from each seed over the interior dark colours (INK, N1, N2 and the DEEP specks);
    a DEEP pixel is only converted when none of its 8 neighbours is a bright / frame colour, so frame lines and the divider stay."""
    from collections import deque
    H, W = rgba.shape[:2]; dark = {(24, 20, 37), (31, 24, 41), (42, 32, 51)}; deep = (14, 10, 20)
    col = lambda y, x: tuple(int(v) for v in rgba[y, x, :3])
    def isdark(y, x): return rgba[y, x, 3] == 255 and col(y, x) in dark
    def conv(y, x): return isdark(y, x) or (rgba[y, x, 3] == 255 and col(y, x) == deep and all(0 <= y + dy < H and 0 <= x + dx < W and (isdark(y + dy, x + dx) or col(y + dy, x + dx) == deep or rgba[y + dy, x + dx, 3] == 0) for dy in (-1, 0, 1) for dx in (-1, 0, 1)))
    seen = np.zeros((H, W), bool); q = deque(); n = 0
    for (x, y) in seeds:
        if isdark(y, x): q.append((y, x)); seen[y, x] = True
    keep = []
    while q:
        y, x = q.popleft(); keep.append((y, x))
        for dy, dx in ((1, 0), (-1, 0), (0, 1), (0, -1)):
            yy, xx = y + dy, x + dx
            if 0 <= yy < H and 0 <= xx < W and not seen[yy, xx] and (isdark(yy, xx) or col(yy, xx) == deep):
                seen[yy, xx] = True; q.append((yy, xx))
    for (y, x) in keep:
        if conv(y, x): rgba[y, x] = (0, 0, 0, TINT_A); n += 1
    return n
def build():
    a, info = static_screen(MS.KEYA); b, _ = static_screen(MS.KEYB)
    diff = (a.a != b.a).any(axis=2); rgba = np.zeros((360, 640, 4), np.uint8); rgba[..., :3] = a.a; rgba[..., 3] = np.where(diff, 0, 255); rgba[diff, :3] = 0
    seeds = [(info['gi'][0] + info['gi'][2] // 2, info['gi'][1] + 20), (info['di'][0] + info['di'][2] // 2, info['di'][1] + 100), (info['di'][0] + 20, info['di'][1] + 100),
             (PLATE_L + PLATE_W // 2, PLATE_Y + 10), (PLATE_R + PLATE_W // 2, PLATE_Y + 10)]
    print('tinted px', tint_interiors(rgba, seeds))
    return rgba, info
if __name__ == '__main__':
    rgba, info = build(); print(info, 'PY', PY, 'GH', GH)
    Image.fromarray(rgba).save(OUT)
    R = '/workspace/HellordRepo/Assets/Hellord/Resources/HellordArt/MenuTheme/'
    Image.fromarray(rgba).save(R + 'mt_scene.png')
    bg = Image.open(R + 'mt_bg.png').convert('RGBA'); bg.alpha_composite(Image.fromarray(rgba)); bg.resize((1280, 720), 0).save('/tmp/scene_new.png')
