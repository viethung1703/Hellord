#!/usr/bin/env python3
"""t1544 v2: cut HA's t1540 v2 mockup card art (pact_select_v2, work/mock2.py base layer = t1321 Morta v6 PACT SELECT with
portraits cleared and labels erased) into Resources/HellordArt/Altar97/pact_cards_v2.png for the live Sacrifice Altar.
Pieces (atlas x, y, w, h -> HellordPactCardsV2.cs): card_c (YOUR PATH red card + header bar), card_n (NOT CHOSEN card), card_l (dim locked card),
av_c / av_n / av_l (70x70 avatar frames), plate_c (120x15 centre name plate). Baked header text is erased (drawn by code in the SMALL font);
pixels outside the chamfered outer border are made transparent."""
import os, sys, re, uuid, numpy as np
from PIL import Image
WORK = sys.argv[1] if len(sys.argv) > 1 else '/workspace/Hellord/Art/pact_avatars_t1540/work'
sys.path.insert(0, WORK); sys.argv = sys.argv[:1]; _cwd = os.getcwd(); os.chdir(WORK)
import mock2 as M2
os.chdir(_cwd)
REPO = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
DST = os.path.join(REPO, 'Assets/Hellord/Resources/HellordArt/Altar97')
B = np.dstack([M2.B0, np.full(M2.B0.shape[:2], 255, np.uint8)])
H = lambda s: np.array([int(s[i:i + 2], 16) for i in (0, 2, 4)] + [255], np.uint8)
def cut(x0, y0, x1, y1): return B[y0:y1 + 1, x0:x1 + 1].copy()
def erase(p, col, fill, box):
    x0, y0, x1, y1 = box; r = p[y0:y1 + 1, x0:x1 + 1]; m = (r == H(col)).all(-1); r[m] = H(fill)
def outside(p, edges):  # transparent outside the outer border, scanning each row from both ends
    E = [H(e) for e in edges]
    for y in range(p.shape[0]):
        for xs in (range(p.shape[1]), range(p.shape[1] - 1, -1, -1)):
            for x in xs:
                if any((p[y, x] == e).all() for e in E): break
                p[y, x, 3] = 0
PIECES = {}
c = cut(245, 135, 394, 271); erase(c, 'ead4aa', 'a22633', (5, 6, 145, 16)); outside(c, ['e43b44', '6e1c2a']); PIECES['card_c'] = c
n = cut(105, 142, 234, 264); erase(n, 'a08f7c', '1f1829', (2, 3, 127, 13)); outside(n, ['8f4f43', 'cf8d62', '0e0a14']); PIECES['card_n'] = n
l = cut(405, 142, 534, 264); outside(l, ['3e2731', '55323a', '0e0a14', '2a2033']); PIECES['card_l'] = l
PIECES['av_c'] = cut(285, 158, 354, 227); PIECES['av_n'] = cut(135, 160, 204, 229); PIECES['av_l'] = cut(435, 160, 504, 229)
PIECES['plate_c'] = cut(260, 232, 379, 246)
# t1544 v3: side cards without the name plate (name moves to the header, title sits below the avatar)
def noplate(card):
    q = card.copy(); fill = q[100, 5].copy(); q[92:109, 9:121] = fill; return q
PIECES['card_n2'] = noplate(PIECES['card_n']); PIECES['card_l2'] = noplate(PIECES['card_l'])
# t1544 v3: SEALED button = the altar's own seal_se (UNSEAL) sprites with the label repainted as SEALED (same colours, SMALL font, 1px drop shadow)
AM = np.array(Image.open(os.path.join(REPO, 'Assets/Hellord/Resources/HellordArt/AltarMenu/am_atlas.png')).convert('RGBA'))
SEAL_SE = {'h_2_0': (0, 640, 91, 15), 'h_4_2': (0, 606, 93, 17), 'h_6_2': (94, 606, 95, 17), 'n_0_0': (358, 624, 89, 15), 'n_m2_0': (270, 624, 87, 15), 'n_m4_m2': (264, 640, 85, 13)}
def glyph_row_text(q, text, x0, y0, col, sh):
    cx = 0
    for n, ch in enumerate(text):
        if n: cx += 1
        if ch == ' ': cx += 3; continue
        g = M2.SMALL[ch]; gw = max(len(r) for r in g)
        for yy, row in enumerate(g):
            for xx, c in enumerate(row):
                if c == '#':
                    if q[y0 + yy + 1, x0 + cx + xx + 1, 3]: q[y0 + yy + 1, x0 + cx + xx + 1, :3] = sh
        for yy, row in enumerate(g):
            for xx, c in enumerate(row):
                if c == '#': q[y0 + yy, x0 + cx + xx, :3] = col
        cx += gw
SEALED = {}
for k, (x, y, w, h) in SEAL_SE.items():
    q = AM[y:y + h, x:x + w].copy(); sx = w - 9; tx0, tx1 = 22, w - 8
    bgx = lambda xx: sx if (sx - xx) % 2 == 0 else sx - 1   # the button fill has a 2px dither: compare / repaint with the same-parity column
    diff = [(yy, xx) for yy in range(3, h - 3) for xx in range(tx0, tx1) if (q[yy, xx, :3] != q[yy, bgx(xx), :3]).any()]
    ys = [a for a, b in diff]; xs = [b for a, b in diff]; ty0, ty1, lx, rx = min(ys), max(ys), min(xs), max(xs)
    vals = {}
    for yy, xx in diff: c = tuple(int(v) for v in q[yy, xx, :3]); vals[c] = vals.get(c, 0) + 1
    col = max(vals, key=lambda c: sum(c)); sh = np.array([0x0e, 0x0a, 0x14], np.uint8)
    for yy in range(ty0, ty1 + 1):
        for xx in range(lx, rx + 1): q[yy, xx, :3] = q[yy, bgx(xx), :3]
    l, r = M2.bb('SEALED'); tw = r - l + 1; cxm = (lx + rx - 1) // 2  # label centre (glyph box, shadow excluded)
    glyph_row_text(q, 'SEALED', cxm - tw // 2 - l, ty0, np.array(col, np.uint8), sh)
    PIECES['seal_sd_' + k] = q
W = 410; atlas = np.zeros((400, W, 4), np.uint8); rects = {}; col = {0: 0, 1: 0, 2: 0}   # column 0: cards (x 0), 1: frames, plate, SEALED buttons (x 152), 2: v3 cards (x 278)
for k, p in PIECES.items():
    h, w = p.shape[:2]; c = 2 if k.endswith('2') and k.startswith('card') else 0 if k.startswith('card') else 1; x = (0, 152, 278)[c]; y = col[c]
    atlas[y:y + h, x:x + w] = p; rects[k] = (x, y, w, h); col[c] = y + h + 2
atlas = atlas[:((max(col.values()) + 3) // 4) * 4]
out = os.path.join(DST, 'pact_cards_v2.png'); Image.fromarray(atlas, 'RGBA').save(out)
if not os.path.exists(out + '.meta'):
    tpl = open(os.path.join(DST, 'avatar_0.png.meta')).read(); open(out + '.meta', 'w').write(re.sub(r'guid: [0-9a-f]{32}', 'guid: ' + uuid.uuid4().hex, tpl, count=1))
print(atlas.shape, rects)
