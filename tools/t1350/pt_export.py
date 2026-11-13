"""t1350: exports the runtime assets of the menu-theme Necromancer PACT INSIDE screen (mockup t1345_B) using the t1287/t1345 engine (read-only imports).
Writes Assets/Hellord/Resources/HellordArt/PactMenu/{pm_bg,pm_scene,pm_atlas,pm_dots,pm_glow,pm_lavadata}.png and Assets/Hellord/Scripts/Runtime/HellordPactMenuData.cs (generated).
Run: python3 tools/t1350/pt_export.py   (needs the Art/previews engine + t1328_lava renders; layout comes from pacts18.py / routes1345.py)"""
import sys, os, math, shutil; sys.dont_write_bytecode = True
HERE = os.path.dirname(os.path.abspath(__file__)); sys.path.insert(0, HERE)
sys.path.insert(0, '/workspace/Hellord/Art/previews/t1287_redraw/work'); LAVA = '/workspace/Hellord/Art/previews/t1328_lava'; sys.path.insert(0, LAVA)
import numpy as np
from PIL import Image
from scipy import ndimage as ndi
import eng2 as E, d2_ui as S, pacts6 as P, pacts9 as Q
from kit import socket, rect_mask
import pacts18 as X, routes1345 as RT
REPO = os.path.abspath(os.path.join(HERE, '..', '..'))
OUT = REPO + '/Assets/Hellord/Resources/HellordArt/PactMenu/'; CS = REPO + '/Assets/Hellord/Scripts/Runtime/HellordPactMenuData.cs'
os.makedirs(OUT, exist_ok=True)
# ---- real game names (the mockup text had typos): label lines per node code
FIX = {'LE': ['LEGION OF', 'ONE HUNDRED'], 'BC': ['GRAVE', 'COLOSSUS'], 'BO': ['DRY', 'OSSUARY']}
for k, v in FIX.items(): X.NODE[k] = (v,) + X.NODE[k][1:]
CODE = list(X.V130.keys())                                   # order == game V130[] / icon_4_<i>
IDX = {c: i for i, c in enumerate(CODE)}
for c in CODE: assert X.NODE[c][1] == IDX[c], c
GAME = ['GRAVE MUSTER', 'IRON MARROW', 'HOLLOW HASTE', 'QUICK GRAVE', 'GRAVE EDGE', 'MARROW SHOT', 'OSSUARY ORDER', 'CRYPT KING', 'LEGION OF ONE HUNDRED', 'MARROW BITE', 'SPLINTERING', 'BRITTLE MARK', 'DRY OSSUARY', 'GRAVE REACH', 'CALCIFY', 'MARROW HARVEST', 'OSSUARY TYRANT', 'GRAVE COLOSSUS']
for c in CODE: assert ' '.join(X.NODE[c][0]) == GAME[IDX[c]], (c, X.NODE[c][0])
placed, tags = X.place_labels(); print('labels placed', len(placed))
# ---------------------------------------------------------------- keyed (alpha by two-key render) sprites
KA, KB = (255, 0, 255), (0, 255, 0)
def keyed(fn, w, h, m=0):
    ox = oy = 8; outs = []
    for k in (KA, KB):
        cv = E.Cv('INK'); cv.a[:] = k; fn(cv, ox, oy); outs.append(cv.a.copy())
    a, b = outs; x0, y0, x1, y1 = ox - m, oy - m, ox + w + m, oy + h + m
    d = (a != b).any(axis=2)[y0:y1, x0:x1]
    rgba = np.zeros((y1 - y0, x1 - x0, 4), np.uint8); rgba[..., :3] = a[y0:y1, x0:x1]; rgba[..., 3] = np.where(d, 0, 255); rgba[d, :3] = 0
    return rgba
SPR = {}
E.PAL.update({'G1': (99, 199, 77), 'G2': (62, 137, 72), 'G3': (38, 92, 66), 'G4': (25, 60, 62)})   # t1384: Endesga greens (SIGNED / OPEN TREE family)
def node_sprite(cv, x, y, ico, kind, big, green=False):
    lit, dark, fill = S.profiles('normal' if kind == 'open' else 'locked', 0.15)
    if green: lit, dark = ['G1'] + list(lit[1:]), ['G2'] + list(dark[1:])      # t1384: selected / signed node = its OWN frame (outer line) turns green, same size, crisp 1x
    socket(cv, x, y, rect_mask(X.CELL, X.CELL, 4 if big else 2), lit, dark, fill)
    cv.asset_blit(X.icon24(ico, kind != 'open'), x + 1, y + 1)
    if kind == 'lock': cv.asset_blit(E.LOCK, x + X.CELL - 10, y + 1)
for c in CODE:
    i = IDX[c]; big = X.NODE[c][5]
    for kind in ('open', 'dim', 'lock'): SPR[f'n{i}_{kind}'] = keyed(lambda cv, x, y, i=i, kind=kind, big=big: node_sprite(cv, x, y, i, kind, big), X.CELL, X.CELL)
    for kind in ('open', 'dim', 'lock'): SPR[f'n{i}_{kind}_g'] = keyed(lambda cv, x, y, i=i, kind=kind, big=big: node_sprite(cv, x, y, i, kind, big, True), X.CELL, X.CELL)
    pos, f, rr = placed[c][1], placed[c][2], placed[c][3]
    for nm, col in (('bone', 'BONE'), ('dim', 'DIM'), ('amb', 'AMBER')):
        def lab(cv, x, y, f=f, pos=pos, rr=rr, col=col):
            for j, ln in enumerate(f):
                lw = X.TINY.width(ln); lx = (rr[2] - lw) // 2 if pos in ('below', 'above') else (0 if pos == 'right' else rr[2] - lw)
                X.TINY.draw(cv, x + lx, y + j * 6, ln, col, role='nodelabel')
        SPR[f'lb{i}_{nm}'] = keyed(lab, rr[2], rr[3])
def hub_sprite(cv, x, y, fn):
    cx, cy = x + 19, y + 19; R = 16
    for j in range(-R, R + 1):
        for i in range(-R, R + 1):
            if math.hypot(i, j) <= R + 0.5: cv.p(cx + i, cy + j, 'DEEP' if math.hypot(i, j) > R - 2 else 'N1')
    im = Image.open(X.ALT + fn).convert('RGBA'); a = np.array(im).astype(float); al = a[..., 3:4] / 255.0
    pm = Image.fromarray(np.dstack([a[..., :3] * al, a[..., 3:4]]).astype(np.uint8), 'RGBA'); r = np.array(pm.resize((38, 38), Image.LANCZOS)).astype(float); aa = r[..., 3:4] / 255.0
    rgb = np.where(aa > 0.02, r[..., :3] / np.maximum(aa, 0.02), 0)
    cv.asset_blit(Image.fromarray(np.dstack([np.clip(rgb, 0, 255), np.where(aa > 0.5, 255, 0)]).astype(np.uint8), 'RGBA'), x, y)
SPR['hub_off'] = keyed(lambda cv, x, y: hub_sprite(cv, x, y, 'necro130_hub_off.png'), 38, 38)
SPR['hub_on'] = keyed(lambda cv, x, y: hub_sprite(cv, x, y, 'necro130_hub_on.png'), 38, 38)
SPR['fork'] = keyed(lambda cv, x, y: X.fork(cv, x + 3, y + 3), 7, 7)
def tag(cv, x, y, l1, col):
    P.box(cv, x, y, 56, 17, ('B2', 'DEEP', 'INK')); cx_ = x + 28
    X.TINY.draw(cv, cx_ - X.TINY.width(l1) // 2, y + 3, l1, col, role='tag', cont=(x, y, 56, 17)); X.TINY.draw(cv, cx_ - X.TINY.width('NOTABLES') // 2, y + 9, 'NOTABLES', col, role='tag', cont=(x, y, 56, 17))
for nm, l1 in (('le', 'ANY 2 SUMMON'), ('bc', 'ANY 2 BONE')):
    for cn, col in (('amb', 'AMBER'), ('dim', 'DIM')): SPR[f'tag_{nm}_{cn}'] = keyed(lambda cv, x, y, l1=l1, col=col: tag(cv, x, y, l1, col), 56, 17)
def keybtn(cv, x, y, w, label, key, state, primary):
    S.button(cv, x, y, w, 20, label, state, 0.15, primary)
    if state == 'disabled': S.SMALL  # key box drawn in DIM either way
    X.SM.draw(cv, x + 7, y + 6, key, 'DIM', role='key', cont=(x, y, w, 20)); cv.vl(x + 17, y + 4, 12, 'R0' if primary else 'P2')
for lab, prim in (('UNLOCK', True), ('SIGN', True), ('REMOVE', False)):
    for st in ('normal', 'hover', 'disabled'): SPR[f'btn_{lab.lower()}_{st}'] = keyed(lambda cv, x, y, lab=lab, prim=prim, st=st: keybtn(cv, x, y, 80, lab, 'A', st, prim and st != 'disabled'), 80, 20, 2)
for st in ('normal', 'hover'): SPR[f'btn_back_{st}'] = keyed(lambda cv, x, y, st=st: P.keyed(cv, x, y, 87, 'BACK', 'B', st), 87, 20, 2)
SPR['ks_on'] = keyed(lambda cv, x, y: cv.blit(['...d...', '..dAd..', '.dAAAd.', 'dAAAAAd', '.dAAAd.', '..dAd..', '...d...'], dict(d='DEEP', A='AMBER'), x, y), 7, 7)
# ---------------------------------------------------------------- atlas pack (shelf)
names = list(SPR.keys()); AW = 512; x = y = rowh = 0; RECT = {}
for n in sorted(names, key=lambda n: -SPR[n].shape[0]):
    h, w = SPR[n].shape[:2]
    if x + w > AW: x = 0; y += rowh + 1; rowh = 0
    RECT[n] = (x, y, w, h); x += w + 1; rowh = max(rowh, h)
AH = y + rowh; AH2 = 1
while AH2 < AH: AH2 *= 2
atlas = np.zeros((AH2, AW, 4), np.uint8)
for n, (ax, ay, w, h) in RECT.items(): atlas[ay:ay + h, ax:ax + w] = SPR[n]
Image.fromarray(atlas, 'RGBA').save(OUT + 'pm_atlas.png'); print('atlas', AW, AH2, len(RECT))
# ---------------------------------------------------------------- scene (chrome only) + bg + dots + glow
bg = np.array(Image.open(LAVA + '/t1345_ui/bgonly_t1345_1x.png').convert('RGB'))      # approved v5 lava cavern at 55% dim + cavern decor (mockup t1345)
Image.fromarray(bg).save(OUT + 'pm_bg.png')
def static_scene(bgarr):
    cv = E.Cv('INK'); cv.a[:] = bgarr; X.decor(cv)
    w = P.TITLE.width('NECROMANCER NODES'); P.TITLE.draw(cv, 320 - w // 2, 14, 'NECROMANCER NODES', 'BONE', leg={'+': 'BONE', '=': 'TERRA'}, role='title', outline='DEEP', rowcols=['BONE'] * 6 + ['CLAY'] * 3 + ['TERRA'] * 2)
    cv.asset_blit(E.ICON_SOUL, 132, 17); ix = 508 - E.ICON_SIGIL.width; cv.asset_blit(E.ICON_SIGIL, ix, 17)
    S.panel(cv, 126, 34, 388, 287); S.panel(cv, 7, 34, 109, 287); S.panel(cv, 524, 34, 109, 287)
    lit, dark, fill = S.profiles('selected', 0.15); socket(cv, 28, 52, rect_mask(66, 66, 3), lit, dark, fill)
    P.box(cv, 18, 128, 87, 15); P.ct(cv, 61, 132, 'NECROMANCER', 'BONE', 'plate', (18, 128, 87, 15))
    for i, t in enumerate(('COMMAND THE', 'DEAD AND BONE')): P.ct(cv, 61, 156 + i * 11, t, 'DIM', 'desc', (16, 150, 91, 30))
    for i in range(8):
        sx = 17 + (i % 4) * 23; sy = 212 + (i // 4) * 25; socket(cv, sx, sy, rect_mask(20, 20, 3), ['P2', 'DEEP', 'INK'], ['P2', 'DEEP', 'INK'], lambda j, i_, d: 'INK'); cv.rect(sx + 8, sy + 9, 4, 1, 'N2')
    P.ct(cv, 578, 52, 'KEYSTONE SLOT', 'BONE', 'ks')
    for k in range(2): cv.blit(['...d...', '..dBd..', '.dB.Bd.', 'dB...Bd', '.dB.Bd.', '..dBd..', '...d...'], dict(d='DEEP', B='P2'), 581 + k * 10, 66)
    P.ct(cv, 578, 83, 'MAX 2', 'DIM', 'ks')
    P.box(cv, 534, 104, 89, 136); P.box(cv, 534, 250, 89, 54)   # t1384: crest title + lines are runtime text now (HellordPactMenu.cs)
    X.fork(cv, 302, 302); X.TINY.draw(cv, 308, 300, 'PICK ONE OF A PAIR', 'DIM', role='legend')
    return cv
s = static_scene(bg).a.copy()
same = (s == bg).all(2)
fill0 = (s == (24, 20, 37)).all(2) | (s == (31, 24, 41)).all(2) | (s == (42, 32, 51)).all(2)
lab, nl = ndi.label(fill0); sz = ndi.sum(fill0, lab, range(1, nl + 1)); objs = ndi.find_objects(lab); keep = []
for i, v in enumerate(sz):
    sl = objs[i]; bw = sl[1].stop - sl[1].start; bh = sl[0].stop - sl[0].start
    if v > 2500 or (v >= 120 and v / (bw * bh) < 0.8): keep.append(i + 1)
fill = np.isin(lab, keep)
scene = np.zeros((360, 640, 4), np.uint8); ui = ~same
scene[ui, :3] = s[ui]; scene[ui, 3] = 255
scene[fill] = (0, 0, 0, 140)                                   # panel interior: 55% black over the lava cavern (see-through)
Image.fromarray(scene, 'RGBA').save(OUT + 'pm_scene.png')
dots = RT.mask_B(RT.build(X.POS, X.JOINT, X.HUBPOS)); da = np.zeros((360, 640, 4), np.uint8); da[dots] = (244, 196, 120, 128)
Image.fromarray(da, 'RGBA').save(OUT + 'pm_dots.png')
_cwd = os.getcwd(); os.chdir(LAVA)
import render_lava_1332 as R
os.chdir(_cwd)
STRIP = R.STRIP; dist = ndi.distance_transform_edt(~STRIP); YY = np.mgrid[0:360, 0:640][0]
g = np.clip(1 - dist / 36.0, 0, 1) ** 1.5 * 0.85; lvl = np.clip(np.floor(g * 4 + R.BAY * 0.9 + 0.5), 0, 3).astype(int)
K = np.array([0, 0.16, 0.30, 0.42]); ga = np.zeros((360, 640, 4), np.uint8); m = (scene[..., 3] > 0) & (~STRIP) & (YY < 340)
ga[m, 0], ga[m, 1], ga[m, 2] = 247, 118, 34; ga[m, 3] = (K[lvl[m]] * 255).round().astype(np.uint8)
Image.fromarray(ga, 'RGBA').save(OUT + 'pm_glow.png')
shutil.copy(LAVA + '/LavaData1332.png', OUT + 'pm_lavadata.png')
ytop = [int(np.where(STRIP[:, xx])[0].min()) for xx in range(640)]
# ---------------------------------------------------------------- generated C#
def arr(a): return '{ ' + ', '.join(str(int(v)) for v in a) + ' }'
L = ['// GENERATED by tools/t1350/pt_export.py (t1350: menu-theme Necromancer PACT INSIDE, mockup t1345_B). Do not edit by hand.', 'using System.Collections.Generic;', 'namespace Hellord', '{', '    public static class PactMenuData', '    {']
L.append('        public static readonly Dictionary<string, int[]> Atlas = new Dictionary<string, int[]> {')
for n, r in sorted(RECT.items()): L.append('            { "%s", new[] %s },' % (n, arr(r)))
L.append('        };')
L.append('        /// <summary>node centre (640x360 px) by game node index (V130 order A1..A7,AK,AK2,B1..B7,BK,BK2)</summary>')
L.append('        public static readonly int[][] Pos = {'); [L.append('            new[] %s, // %s %s' % (arr(X.POS[c]), c, GAME[IDX[c]])) for c in CODE]; L.append('        };')
L.append('        /// <summary>label rect (x, y, w, h) by game node index</summary>')
L.append('        public static readonly int[][] Label = {'); [L.append('            new[] %s,' % arr(placed[c][3])) for c in CODE]; L.append('        };')
L.append('        public const int Cell = %d, HubX = %d, HubY = %d, SignX = %d, SignY = 264, BackX = 18, BackY = 264, SigilIx = %d;' % (X.CELL, X.HUBPOS[0], X.HUBPOS[1], X.HUBPOS[0] - 40, 508 - E.ICON_SIGIL.width))
L.append('        public static readonly int[][] Joints = { new[] %s, new[] %s, new[] %s };' % tuple(arr(X.JOINT[p]) for p in X.PAIRS))
L.append('        public static readonly int[] TagLE = %s, TagBC = %s;' % (arr(tags['LE']), arr(tags['BC'])))
L.append('        /// <summary>top row of the lava strip per screen column</summary>')
L.append('        public static readonly int[] YTop = %s;' % arr(ytop))
L.append('    }'); L.append('}')
open(CS, 'w').write('\n'.join(L) + '\n'); print('wrote', CS)
