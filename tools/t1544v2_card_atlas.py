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
W = 276; atlas = np.zeros((400, W, 4), np.uint8); rects = {}; col = {0: 0, 1: 0}   # column 0: cards (x 0), column 1: frames + plate (x 152)
for k, p in PIECES.items():
    h, w = p.shape[:2]; c = 0 if k.startswith('card') else 1; x = 0 if c == 0 else 152; y = col[c]
    atlas[y:y + h, x:x + w] = p; rects[k] = (x, y, w, h); col[c] = y + h + 2
atlas = atlas[:((max(col.values()) + 3) // 4) * 4]
out = os.path.join(DST, 'pact_cards_v2.png'); Image.fromarray(atlas, 'RGBA').save(out)
if not os.path.exists(out + '.meta'):
    tpl = open(os.path.join(DST, 'avatar_0.png.meta')).read(); open(out + '.meta', 'w').write(re.sub(r'guid: [0-9a-f]{32}', 'guid: ' + uuid.uuid4().hex, tpl, count=1))
print(atlas.shape, rects)
