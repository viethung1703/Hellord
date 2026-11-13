"""t1380: exports the runtime assets of the MENU-THEME Sacrifice Altar overview (path select) = mockup t1379 (Art/previews/t1379_altar/mk_altar_t1379.py, same coordinates / palette / bevels).
Writes Assets/Hellord/Resources/HellordArt/AltarMenu/{am_scene,am_atlas,am_glow}.png (+ .meta: point filter, uncompressed) and Assets/Hellord/Scripts/Runtime/HellordAltarMenuData.cs (generated).
The lava cavern background, lava data and shader are the PactMenu ones (pm_bg / pm_lavadata / HellordLava1332).
Run: python3 tools/t1380/altar_export.py   (needs the Art/previews engine: eng2 / d2_ui / pacts6 / kit, and tools/t1350/pacts18.py)"""
import sys, os, math, uuid, shutil; sys.dont_write_bytecode = True
HERE = os.path.dirname(os.path.abspath(__file__)); sys.path.insert(0, os.path.join(HERE, '..', 't1350'))
sys.path.insert(0, '/workspace/Hellord/Art/previews/t1287_redraw/work'); LAVA = '/workspace/Hellord/Art/previews/t1328_lava'; sys.path.insert(0, LAVA)
import numpy as np
from PIL import Image
from collections import deque
from scipy import ndimage as ndi
import eng2 as E, d2_ui as S, pacts6 as P
from kit import socket, rect_mask
REPO = os.path.abspath(os.path.join(HERE, '..', '..'))
RES = REPO + '/Assets/Hellord/Resources/HellordArt/'; OUT = RES + 'AltarMenu/'; CS = REPO + '/Assets/Hellord/Scripts/Runtime/HellordAltarMenuData.cs'
ALT = RES + 'Altar97/'; os.makedirs(OUT, exist_ok=True)
SM = S.SMALL
E.PAL.update({'G1': (99, 199, 77), 'G2': (62, 137, 72), 'G3': (38, 92, 66), 'G4': (25, 60, 62)})   # Endesga-32 greens (muted ramp) for OPEN TREE
bg = np.array(Image.open(RES + 'PactMenu/pm_bg.png').convert('RGB'))
# ------------------------------------------------------------------ geometry (identical to mk_altar_t1379.py)
CARD_Y, CARD_H, SY0, FACE_Y, BTN_Y = 89, 157, 10, 257, 290
SIDE_W, SIDE_H = 104, 113; SIDE_Y = CARD_Y + (CARD_H - SIDE_H) // 2; SIDE_CX = (160, 480)
CAV_Y, CAV_H, SOCK, PITCH, WALL = 39, 30, 24, 28, 7
PATHS = ['PYROMANCER', 'NECROMANCER', 'STORMCALLER', 'BLOOD REAVER']; NSOCK = [6, 8, 6, 6]; NNODES = [10, 18, 10, 10]; ICONSET = [0, 4, 2, 3]   # Necromancer icons = icon_4_<i> (V130 order)
# ------------------------------------------------------------------ helpers
def gbb(t, F=SM, **kw):
    sc = E.Cv('INK'); F.draw(sc, 20, 20, t, 'BONE', log=False, **{k: v for k, v in kw.items() if k in ('leg', 'outline', 'rowcols')})
    m = (sc.a != np.array(E.PAL['INK'], np.uint8)).any(2); ys, xs = np.where(m); return xs.min() - 20, xs.max() - 20, ys.min() - 20, ys.max() - 20
def put(cv, rect, t, col, F=SM, h=True, v=True, x=None, y=None, **kw):
    rx, ry, rw, rh = rect; x0, x1, y0, y1 = gbb(t, F, **kw); bw, bh = x1 - x0 + 1, y1 - y0 + 1
    ox = rx + (rw - bw) // 2 - x0 if h else x - x0; oy = ry + (rh - bh) // 2 - y0 if v else y - y0
    F.draw(cv, ox, oy, t, col, **kw); return ox, oy
def bevel(cv, x, y, w, h, lit, dark, fill=None):
    if fill: cv.rect(x, y, w, h, fill)
    for j in range(h):
        for i in range(w):
            d = min(i, w - 1 - i, j, h - 1 - j)
            if d >= len(lit): continue
            tl = min(i, j) <= min(w - 1 - i, h - 1 - j); cv.p(x + i, y + j, (lit if tl else dark)[d])
CARD_SEL_UN = (['DEEP', 'ORANGE', 'TERRA', 'DEEP', 'AMBER'], ['DEEP', 'TERRA', 'BROWN', 'DEEP', 'ORANGE'])
CARD_SEL_SE = (['DEEP', 'YELLOW', 'AMBER', 'DEEP', 'YELLOW'], ['DEEP', 'ORANGE', 'TERRA', 'DEEP', 'AMBER'])
CARD_NEU = (['DEEP', 'B2', 'P2', 'DEEP', 'P2'], ['DEEP', 'P2', 'PLUM', 'DEEP', 'PLUM'])
AVF_SEL = (['DEEP', 'ORANGE', 'AMBER'], ['DEEP', 'TERRA', 'ORANGE']); AVF_NEU = (['DEEP', 'P2', 'PLUM'], ['DEEP', 'PLUM', 'N2'])
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
# ------------------------------------------------------------------ static scene (chrome) composed on the lava bg, then interiors -> 55 % black
cv = E.Cv('INK'); cv.a[:] = bg
BAND = (0, 0, 640, 30)
put(cv, BAND, 'SACRIFICE ALTAR', 'BONE', P.TITLE, leg={'+': 'BONE', '=': 'TERRA'}, role='title', outline='DEEP', rowcols=['BONE'] * 6 + ['CLAY'] * 3 + ['TERRA'] * 2)
cv.asset_blit(E.ICON_SOUL, 132, (30 - E.ICON_SOUL.height) // 2); SIGIX = 508 - E.ICON_SIGIL.width; cv.asset_blit(E.ICON_SIGIL, SIGIX, (30 - E.ICON_SIGIL.height) // 2)
S.panel(cv, 14, 30, 612, 294)
for cx in SIDE_CX:
    bevel(cv, cx - SIDE_W // 2, SIDE_Y, SIDE_W, SIDE_H, *CARD_NEU, fill='INK')
    bevel(cv, cx - 35, SIDE_Y + 10, 70, 70, *AVF_NEU, fill='INK')
    px = cx - SIDE_W // 2 + (SIDE_W - 89) // 2; P.box(cv, px, SIDE_Y - 7, 89, 15, ('B2', 'DEEP', 'INK'))
pre = cv.a.copy()                                                   # pre-tint chrome (panel fill colours are needed for the glow ring)
out = cv.a.copy()
dark = {(24, 20, 37), (31, 24, 41), (42, 32, 51)}; deep = (14, 10, 20)
isd = lambda y, x: tuple(int(v) for v in out[y, x]) in dark
def tint_mask(seeds):
    q = deque(); seen = np.zeros(out.shape[:2], bool); keep = []
    for s in seeds:
        if isd(s[1], s[0]): q.append((s[1], s[0])); seen[s[1], s[0]] = True
    while q:
        y, x = q.popleft(); keep.append((y, x))
        for dy, dx in ((1, 0), (-1, 0), (0, 1), (0, -1)):
            yy, xx = y + dy, x + dx
            if 0 <= yy < 360 and 0 <= xx < 640 and not seen[yy, xx] and isd(yy, xx): seen[yy, xx] = True; q.append((yy, xx))
    for (y, x) in list(keep):
        for dy in (-1, 0, 1):
            for dx in (-1, 0, 1):
                yy, xx = y + dy, x + dx
                if 0 <= yy < 360 and 0 <= xx < 640 and not seen[yy, xx] and tuple(int(v) for v in out[yy, xx]) == deep: seen[yy, xx] = True; keep.append((yy, xx))
    m = np.zeros(out.shape[:2], bool)
    for (y, x) in keep:
        c = tuple(int(v) for v in out[y, x])
        ok = c in dark or all(0 <= y + dy < 360 and 0 <= x + dx < 640 and (tuple(int(v) for v in out[y + dy, x + dx]) in dark or tuple(int(v) for v in out[y + dy, x + dx]) == deep) for dy in (-1, 0, 1) for dx in (-1, 0, 1))
        if ok: m[y, x] = True
    return m
tm = tint_mask([(30, 200), (60, 70), (40, 300), (590, 300), (SIDE_CX[0] - 38, SIDE_Y), (SIDE_CX[1] - 38, SIDE_Y)])
same = (cv.a == bg).all(2); scene = np.zeros((360, 640, 4), np.uint8); ui = ~same
scene[ui, :3] = cv.a[ui]; scene[ui, 3] = 255; scene[tm] = (0, 0, 0, 140)
Image.fromarray(scene, 'RGBA').save(OUT + 'am_scene.png'); print('scene tinted px', int(tm.sum()))
# ------------------------------------------------------------------ atlas sprites
# centre card (frame + avatar frame + straddling name plate): local origin = plate top-left of the card column (x 240, y 82)
def card_sprite(cv, x, y, lit, dark_, avf):
    bevel(cv, x, y + 7, 160, CARD_H, *lit, fill='INK'); bevel(cv, x + 13, y + 7 + SY0, 134, 134, *avf, fill='INK'); P.box(cv, x + 9, y, 141, 15, ('B2', 'DEEP', 'INK'))
SPR['card_sel_un'] = keyed(lambda cv, x, y: card_sprite(cv, x, y, CARD_SEL_UN, None, AVF_SEL), 160, 7 + CARD_H)
SPR['card_sel_se'] = keyed(lambda cv, x, y: card_sprite(cv, x, y, CARD_SEL_SE, None, AVF_SEL), 160, 7 + CARD_H)
SPR['card_neu'] = keyed(lambda cv, x, y: card_sprite(cv, x, y, CARD_NEU, None, AVF_NEU), 160, 7 + CARD_H)
def glow_sprite(g1, g2):
    og = np.array(E.PAL['ORANGE'], float); w, h = 160, CARD_H; spr = np.zeros((h + 4, w + 4, 4), np.uint8)   # local origin = card origin - 2
    for j in range(-2, h + 2):
        for i in range(-2, w + 2):
            dx = max(-i, i - (w - 1), 0); dy = max(-j, j - (h - 1), 0); d = max(dx, dy)
            if d not in (1, 2) or (d == 2 and (i + j) % 2): continue
            X0, Y0 = 240 + i, CARD_Y + j; g = g1 if d == 1 else g2
            spr[j + 2, i + 2, :3] = np.rint(pre[Y0, X0] * (1 - g) + og * g).astype(np.uint8); spr[j + 2, i + 2, 3] = 255
    return spr
SPR['glow_un'] = glow_sprite(0.40, 0.22); SPR['glow_se'] = glow_sprite(0.55, 0.30)
SPR['chip'] = keyed(lambda cv, x, y: cv.blit(['...oooo...', '..oRRRRo..', '.oRRaaRRo.', 'oRRaRRaRRo', 'oRaRRRRaRo', 'oRaRRRRaRo', 'oRRaRRaRRo', '.oRRaaRRo.', '..oRRRRo..', '...oooo...'], dict(o='DEEP', R='RED', a='YELLOW'), x, y), 10, 10)
SPR['lock'] = keyed(lambda cv, x, y: cv.asset_blit(E.LOCK, x, y), E.LOCK.width, E.LOCK.height)
CREST = ['..###..', '.#####.', '#######', '#######', '.#####.', '..###..', '...#...']
for nm, col in (('off', 'P2'), ('on', 'AMBER')): SPR['crest_' + nm] = keyed(lambda cv, x, y, col=col: cv.blit([r.replace('#', 'c') for r in CREST], dict(c=col), x, y), 7, 7)
SPR['sock_on'] = keyed(lambda cv, x, y: socket(cv, x, y, rect_mask(SOCK, SOCK, 3), *S.profiles('normal', 0.0)), SOCK, SOCK)
SPR['sock_off'] = keyed(lambda cv, x, y: socket(cv, x, y, rect_mask(SOCK, SOCK, 3), ['P2', 'PLUM', 'N2'], ['PLUM', 'N2', 'N1'], lambda j, i, d: 'INK'), SOCK, SOCK)
def icon20(iconset, i):
    im = Image.open(ALT + f'icon_{iconset}_{i}.png').convert('RGBA'); a = np.array(im).astype(float); al = a[..., 3:4] / 255.0
    pm = Image.fromarray(np.dstack([a[..., :3] * al, a[..., 3:4]]).astype(np.uint8), 'RGBA')
    r = np.array(pm.resize((20, 20), Image.LANCZOS)).astype(float); aa = r[..., 3:4] / 255.0; rgb = np.where(aa > 0.02, r[..., :3] / np.maximum(aa, 0.02), 0)
    return np.dstack([np.clip(rgb, 0, 255), np.where(aa > 0.5, 255, 0)]).astype(np.uint8)
for p in range(4):
    for i in range(NNODES[p]): SPR[f'ic_{p}_{i}'] = icon20(ICONSET[p], i)
# ---- hanging tab variants (frame walls + cavity + path name + NODES + divider; sockets / count / crest are runtime)
TABINFO = []
WALLSEQ = ['DEEP', 'P2', 'DEEP', None, 'DEEP', 'P2', 'DEEP']
for p in range(4):
    name = PATHS[p]; nodes_w = NSOCK[p] * PITCH - (PITCH - SOCK); crest_w = 7 + 4 + SM.width('CREST 4+'); g1 = 21
    cav_w = 12 + SM.width(name) + g1 + SM.width('NODES') + 7 + nodes_w + 7 + SM.width('3/8') + 9 + 1 + 9 + crest_w + 12
    if (cav_w + 2 * WALL) % 2: g1 += 1; cav_w += 1
    tw = cav_w + 2 * WALL; th = CAV_H + WALL; tx0 = 320 - tw // 2
    def tab(cv, x, y, name=name, cav_w=cav_w, g1=g1, tw=tw, th=th, nodes_w=nodes_w):
        for yy in range(th):
            for xx in range(tw):
                kL, kR, kB = xx, tw - 1 - xx, th - 1 - yy; k = min(kL, kR, kB)
                if k >= WALL: cv.p(x + xx, y + yy, 'INK'); continue
                cv.p(x + xx, y + yy, WALLSEQ[k] or ('T2' if kL == k else 'B2'))
        cavx = x + WALL; cx_ = cavx + 12; cavr = (cavx, y, cav_w, CAV_H)
        put(cv, cavr, name, 'BONE', h=False, x=cx_, role='strip'); cx_ += SM.width(name) + g1
        put(cv, cavr, 'NODES', 'DIM', h=False, x=cx_, role='strip'); cx_ += SM.width('NODES') + 7
        cx_ += nodes_w + 7 + SM.width('3/8') + 9; cv.rect(cx_, y + 5, 1, CAV_H - 10, 'P2')
    SPR[f'tab_{p}'] = keyed(tab, tw, th)
    sockx = tx0 + WALL + 12 + SM.width(name) + g1 + SM.width('NODES') + 7; cntx = sockx + nodes_w + 7; divx = cntx + SM.width('3/8') + 9; crestx = divx + 1 + 9
    TABINFO.append((tx0, CAV_Y, tw, th, sockx, cntx, crestx)); print(name, 'tab', tx0, tw, 'sock', sockx, 'cnt', cntx, 'crest', crestx, 'right pad', tx0 + tw - WALL - (crestx + crest_w))
# ---- buttons (105x19 OPEN TREE green / BACK, 89x15 SEAL red; hover = lighter bands + orange corner brackets)
def brackets(cv, x, y, w, h):
    for (cx, cy, sx, sy) in ((x - 2, y - 2, 1, 1), (x + w + 1, y - 2, -1, 1), (x - 2, y + h + 1, 1, -1), (x + w + 1, y + h + 1, -1, -1)):
        for i in range(3): cv.p(cx + sx * i, cy, 'ORANGE'); cv.p(cx, cy + sy * i, 'ORANGE')
def band_fill(bands, h):
    nb = len(bands)
    def fill(j, i, d):
        k = (j - 4) * nb / max(1, h - 8); b = min(nb - 1, max(0, int(k)))
        if b > 0 and k - int(k) < 0.34 and (i + j) % 2 == 0: b -= 1
        return bands[b]
    return fill
def keybtn(cv, x, y, w, h, label, key, lit, dark_, bands, tcol, kcol, dcol, hover, shadow='DEEP'):
    S.raised(cv, x, y, rect_mask(w, h, 3), lit, dark_, band_fill(bands, h))
    put(cv, (x, y, w, h), label, tcol, role='button', cont=(x, y, w, h), shadow=shadow)
    put(cv, (x + 4, y, 13, h), key, kcol, role='key', cont=(x, y, w, h)); cv.vl(x + 17, y + 4, h - 8, dcol)
    if hover: brackets(cv, x, y, w, h)
for hv, nm in ((False, 'n'), (True, 'h')):
    gb = ['G1', 'G2', 'G2', 'G3', 'G3', 'G4'] if hv else ['G2', 'G2', 'G3', 'G3', 'G3', 'G4']
    SPR['open_' + nm] = keyed(lambda cv, x, y, gb=gb, hv=hv: keybtn(cv, x, y, 105, 19, 'OPEN TREE', 'A', ['DEEP', 'G1', 'G2', 'G3'], ['DEEP', 'G4', 'G3', 'G4'], gb, 'BONE', 'DIM', 'G4', hv), 105, 19, 2)
    def back(cv, x, y, hv=hv):
        S.button(cv, x, y, 105, 19, '', 'hover' if hv else 'normal', 0.15, False); put(cv, (x, y, 105, 19), 'BACK', 'BONE', role='button', cont=(x, y, 105, 19), shadow='DEEP')
        put(cv, (x + 4, y, 13, 19), 'B', 'DIM', role='key', cont=(x, y, 105, 19)); cv.vl(x + 17, y + 4, 11, 'P2')
    SPR['back_' + nm] = keyed(back, 105, 19, 2)
    SPR['seal_un_' + nm] = keyed(lambda cv, x, y, hv=hv: keybtn(cv, x, y, 89, 15, 'SEAL', 'X', ['DEEP', 'PINK', 'RED', 'DRED'], ['DEEP', 'R0', 'DRED', 'BLOOD'], ['RED', 'DRED', 'DRED', 'BLOOD', 'BLOOD', 'BLOOD', 'R0'] if not hv else ['PINK', 'RED', 'RED', 'DRED', 'DRED', 'BLOOD', 'BLOOD'], 'BONE', 'DIM', 'R0', hv), 89, 15, 2)
    SPR['seal_se_' + nm] = keyed(lambda cv, x, y, hv=hv: keybtn(cv, x, y, 89, 15, 'UNSEAL', 'X', ['DEEP', 'PINK', 'RED', 'DRED'], ['DEEP', 'DRED', 'BLOOD', 'R0'], ['BLOOD', 'DRED', 'DRED', 'R0', 'R0'] if not hv else ['DRED', 'RED', 'DRED', 'BLOOD', 'BLOOD'], 'YELLOW', 'CLAY', 'R0', hv), 89, 15, 2)
for d_, nm in ((1, 'l'), (-1, 'r')):
    for hv, st in ((False, 'n'), (True, 'h')):
        def arrow(cv, x, y, d_=d_, hv=hv):
            S.button(cv, x, y, 17, 23, '', 'hover' if hv else 'normal', 0.15, False)
            rows_ = ['..#', '.##', '###', '.##', '..#'] if d_ > 0 else ['#..', '##.', '###', '##.', '#..']
            cv.blit([r.replace('#', 'B') for r in rows_], dict(B='BONE'), x + 7, y + 9)
        SPR[f'arr_{nm}_{st}'] = keyed(arrow, 17, 23, 2)
# ------------------------------------------------------------------ atlas pack (shelf)
names = list(SPR.keys()); AW = 512; x = y = rowh = 0; RECT = {}
for n in sorted(names, key=lambda n: -SPR[n].shape[0]):
    h, w = SPR[n].shape[:2]
    if x + w > AW: x = 0; y += rowh + 1; rowh = 0
    RECT[n] = (x, y, w, h); x += w + 1; rowh = max(rowh, h)
AH = y + rowh; AH2 = 1
while AH2 < AH: AH2 *= 2
atlas = np.zeros((AH2, AW, 4), np.uint8)
for n, (ax, ay, w, h) in RECT.items(): atlas[ay:ay + h, ax:ax + w] = SPR[n]
Image.fromarray(atlas, 'RGBA').save(OUT + 'am_atlas.png'); print('atlas', AW, AH2, len(RECT))
# ------------------------------------------------------------------ warm glow of the lava strip onto the lower UI (same maths as pm_glow, mask = this scene)
_cwd = os.getcwd(); os.chdir(LAVA)
import render_lava_1332 as R
os.chdir(_cwd)
STRIP = R.STRIP; dist = ndi.distance_transform_edt(~STRIP); YY = np.mgrid[0:360, 0:640][0]
g = np.clip(1 - dist / 36.0, 0, 1) ** 1.5 * 0.85; lvl = np.clip(np.floor(g * 4 + R.BAY * 0.9 + 0.5), 0, 3).astype(int)
K = np.array([0, 0.16, 0.30, 0.42]); ga = np.zeros((360, 640, 4), np.uint8); m = (scene[..., 3] > 0) & (~STRIP) & (YY < 340)
ga[m, 0], ga[m, 1], ga[m, 2] = 247, 118, 34; ga[m, 3] = (K[lvl[m]] * 255).round().astype(np.uint8)
Image.fromarray(ga, 'RGBA').save(OUT + 'am_glow.png')
# ------------------------------------------------------------------ .meta files (same importer settings as PactMenu: point filter, uncompressed RGBA32, no mips)
tmpl = open(RES + 'PactMenu/pm_scene.png.meta').read()
for f in ('am_scene.png', 'am_atlas.png', 'am_glow.png'):
    mp = OUT + f + '.meta'
    if not os.path.exists(mp): open(mp, 'w').write(tmpl.replace('guid: 30e9d84a2a054d0fa81df49dbf1b24e5', 'guid: ' + uuid.uuid4().hex))
fm = RES + 'AltarMenu.meta'
if not os.path.exists(fm): open(fm, 'w').write(open(RES + 'PactMenu.meta').read().replace('74085e36824e4d87ae11b48bfab26f1f', uuid.uuid4().hex))
# ------------------------------------------------------------------ generated C#
def arr(a): return '{ ' + ', '.join(str(int(v)) for v in a) + ' }'
L = ['// GENERATED by tools/t1380/altar_export.py (t1380: menu-theme Sacrifice Altar overview, mockup t1379). Do not edit by hand.', 'using System.Collections.Generic;', 'namespace Hellord', '{', '    public static class AltarMenuData', '    {']
L.append('        public static readonly Dictionary<string, int[]> Atlas = new Dictionary<string, int[]> {')
for n, r in sorted(RECT.items()): L.append('            { "%s", new[] %s },' % (n, arr(r)))
L.append('        };')
L.append('        /// <summary>hanging top tab per Path: x, y, w, h, first socket x, count text x, crest glyph x (crest text = glyph x + 11)</summary>')
L.append('        public static readonly int[][] Tab = {'); [L.append('            new[] %s, // %s' % (arr(t), PATHS[i])) for i, t in enumerate(TABINFO)]; L.append('        };')
L.append('        public static readonly int[] TabSockets = %s, NodeCount = %s;' % (arr(NSOCK), arr(NNODES)))
L.append('        public const int CardX = 240, CardPlateY = 82, CardY = %d, CardH = %d, SideY = %d, SideW = %d, SideH = %d, SigilIx = %d, FaceY = %d, BtnY = %d, SealX = 275, SealY = %d, OpenX = 205, BackX = 330, ArrL = 74, ArrR = 549, ArrY = %d;' % (CARD_Y, CARD_H, SIDE_Y, SIDE_W, SIDE_H, SIGIX, FACE_Y, BTN_Y, CARD_Y + CARD_H - 3 - 7, CARD_Y + (CARD_H - 23) // 2))
L.append('        public static readonly int[] SideCx = %s;' % arr(SIDE_CX))
L.append('        /// <summary>head crop centre (in the 64px game avatar) per Path for the path-face row</summary>')
L.append('        public static readonly int[][] FaceCrop = { new[] { 38, 19 }, new[] { 28, 25 }, new[] { 37, 31 }, new[] { 30, 29 } };')
L.append('    }'); L.append('}')
open(CS, 'w').write('\n'.join(L) + '\n'); print('wrote', CS)
