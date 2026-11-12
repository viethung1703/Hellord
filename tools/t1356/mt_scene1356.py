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
TITLE_Y = 44                      # was 50
PLATE_W, PLATE_H = 92, 20; PLATE_L, PLATE_R = 140, 640 - 140 - 92; PLATE_Y = TITLE_Y - 4
TY = 107                          # tabs row (was 121)
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
def build():
    a, info = static_screen(MS.KEYA); b, _ = static_screen(MS.KEYB)
    diff = (a.a != b.a).any(axis=2); rgba = np.zeros((360, 640, 4), np.uint8); rgba[..., :3] = a.a; rgba[..., 3] = np.where(diff, 0, 255); rgba[diff, :3] = 0
    return rgba, info
if __name__ == '__main__':
    rgba, info = build(); print(info, 'PY', PY, 'GH', GH)
    Image.fromarray(rgba).save(OUT)
    R = '/workspace/HellordRepo/Assets/Hellord/Resources/HellordArt/MenuTheme/'
    Image.fromarray(rgba).save(R + 'mt_scene.png')
    bg = Image.open(R + 'mt_bg.png').convert('RGBA'); bg.alpha_composite(Image.fromarray(rgba)); bg.resize((1280, 720), 0).save('/tmp/scene_new.png')
