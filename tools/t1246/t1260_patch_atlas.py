# t1260 (VH t1260 feedback): patch ui_t1246_parts.png cell sprites. Idempotent; run AFTER export4.py if the atlas is re-exported.
#  - removes the 4 corner stud dots + the two lone lit-lip corner pixels from every cell sprite (clean dark cut + soft lit lip)
#  - selected cell: 1px outer ring only, body/lip identical in all 6 frames, ring breathes dark red -> medium red (no size change)
#  - adds cell_hr0..2: 1px amber hover ring overlays (outer ring + very soft inner ring), drawn over any cell state
import sys
from PIL import Image
P = sys.argv[1] if len(sys.argv) > 1 else '/workspace/Hellord/Assets/Hellord/Resources/HellordArt/ui_t1246_parts.png'
im = Image.open(P).convert('RGBA'); px = im.load()
INK = (24, 20, 37); LIP_D = (45, 44, 43); LIP_L = (84, 79, 78)
def put(x0, y0, x, y, c, a=255): px[x0 + x, y0 + y] = (c[0], c[1], c[2], a)
def clear(x0, y0, w=24, h=24):
    for y in range(h):
        for x in range(w): px[x0 + x, y0 + y] = (0, 0, 0, 0)
def cell(x0, body, ring):
    clear(x0, 0)
    for y in range(24):
        for x in range(24):
            corner = (x in (0, 23)) and (y in (0, 23))
            if corner: continue
            if x in (0, 23) or y in (0, 23): c = ring
            elif x == 1 or y == 1: c = LIP_D          # top/left lip: dark
            elif x == 22 or y == 22: c = LIP_L        # bottom/right lip: soft lit
            else: c = body
            put(x0, 0, x, y, c)
    put(x0, 0, 22, 1, LIP_D); put(x0, 0, 1, 22, LIP_D)  # no lone lit pixel where lit lip meets dark lip
BODY = (55, 54, 53); BODY_EQ = (76, 75, 74); BODY_SEL = (62, 61, 60)
cell(0, BODY, INK); cell(25, BODY, INK); cell(50, BODY_EQ, INK); cell(75, BODY, INK)
A = (92, 27, 38); B = (162, 38, 51)
import math
for k in range(6):
    t = 0.5 - 0.5 * math.cos(2 * math.pi * k / 6)
    ring = tuple(round(A[i] + (B[i] - A[i]) * t) for i in range(3))
    cell(100 + 25 * k, BODY_SEL, ring)
HR = [(150, 74, 34), (196, 96, 34), (232, 134, 42)]
for k in range(3):
    x0 = 25 * k; y0 = 300; clear(x0, y0)
    for y in range(24):
        for x in range(24):
            if (x in (0, 23)) and (y in (0, 23)): continue
            if x in (0, 23) or y in (0, 23): put(x0, y0, x, y, HR[k], 255)
            elif (x == 1 or x == 22 or y == 1 or y == 22) and not ((x in (1, 22)) and (y in (1, 22))): put(x0, y0, x, y, HR[k], 46)
im.save(P)
print('patched', P)
