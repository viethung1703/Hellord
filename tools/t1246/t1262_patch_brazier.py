# t1262: repaints both pillar braziers in ui_t1246_base.png (bowl = back rim + dark cavity + lit front lip + dark underside + shadow on the pillar).
# Idempotent: always reads t1262_base_pristine.png (the t1261 base) and writes the base png. If export4.py re-exports the base, refresh the pristine copy first.
# The cavity ellipse constants below MUST match HellordSinFX1246.shader (CavC/CavR). usage: t1262_patch_brazier.py <base.png out>
import sys, math, os
from PIL import Image
here = os.path.dirname(os.path.abspath(__file__))
out = sys.argv[1]
src = Image.open(os.path.join(here, "t1262_base_pristine.png")).convert("RGBA"); im = src.copy(); px = im.load(); o = src.load()
INK=(0x18,0x14,0x25,255); G0=(0x38,0x37,0x36,255); G1=(0x4c,0x4b,0x4a,255); G2=(0x63,0x62,0x61,255); G3=(0x9d,0x9c,0x9b,255); G4=(0xd3,0xd2,0xd1,255)
CY=221.0; OX=17.7; OY=6.0      # outer rim ellipse (centre pixel index 19 / 459, world centre x+.5)
IY=219.9; IX=14.6; IYR=4.3     # inner cavity ellipse
def paint(cxi, mirror):
    # work in local coordinates u = pixel x - cxi (symmetric); write to world x = cxi + u
    def get(u,y): return o[cxi+u,y]
    def ell(u,y,cy,rx,ry): return (u/rx)**2 + ((y+.5-cy)/ry)**2
    for y in range(212, 238):
        for u in range(-17, 18):
            x = cxi + u
            e_out = ell(u,y,CY,OX,OY); e_in = ell(u,y,IY,IX,IYR)
            ee_out = ell(u,y,CY,OX+1.25,OY+1.25)
            if y <= 226:
                if e_in < 1:   # dark cavity: ink at the back, a faint grey texture toward the front
                    t = (y+.5-IY)/IYR
                    c = INK
                    if t > -.15 and ((u + y) % 3 == 0) : c = G0
                    if t < -.55 and abs(u) < 11 and (u + 2*y) % 4 == 0: c = G0   # lit inner wall of the back rim
                    px[x,y] = c
                elif e_out < 1:   # rim ring
                    ring_front = (y + .5) > IY
                    topn = (y + .5) < IY - IYR*.35
                    if ring_front:   # front lip: lit top edge with clustered catches, mid face, dark lower edge
                        if y == 224: c = G3
                        elif y == 225: c = G2
                        else: c = G0
                        if y == 224 and (abs(u) >= 13 or (u in (-8,-7,-6,-2,-1,4,5,9,10))): c = G4
                        if y == 225 and (u % 6 == 0 or abs(u) >= 15): c = G3 if abs(u) >= 15 else G1
                        if y == 223 and abs(u) > 11: c = G3
                    elif topn: c = G2 if (u + y) % 5 else G3     # back rim top: mid grey, a few lit pixels
                    else:
                        c = G3 if abs(u) < 16 else G4          # shoulders
                    px[x,y] = c
                elif ee_out < 1 and y <= 225:   # ink outline on sides and back
                    px[x,y] = INK
            elif y <= 231 and y >= 227:    # underside of the bowl: a crescent of light on the belly, dark toward the bottom and the edges
                k = y - 227; half = 17 - (0, 1, 3, 5, 7)[k]
                if abs(u) <= half:
                    sh = k + 2.6*(abs(u)/17.0)**2
                    if sh < 1.2: c = G2 if (u + k) % 3 else G1
                    elif sh < 2.6: c = G1
                    elif sh < 3.7: c = G0
                    else: c = INK
                    if k == 0 and abs(u) < 12 and (u % 4 == 0): c = G2     # crisp sheen dots just under the lip
                    if abs(u) >= half - 1 and 0 <= k <= 3 and sh < 3.6: c = G2       # side rim light
                    if abs(u) == half and k >= 2: c = INK
                    px[x,y] = c
                elif abs(u) == half + 1 and k >= 3: px[x,y] = INK
            # y 232+: pillar with the bowl's shadow below
    # shadow on the pillar under the bowl (rows 233..236), dithered, stronger under the middle
    dark = {G4:G2, G3:G1, G2:G0, G1:G0}
    for y in range(232, 239):
        for u in range(-15, 16):
            x = cxi + u; c = o[x,y]
            if c in dark:
                strength = (1.0 if y <= 234 else .66 if y == 235 else .4 if y==236 else .2) * (1 - (abs(u)/16)**2)
                if (((x + y) & 1) == 0 and strength > .25) or strength > .75: px[x,y] = dark[c]
paint(19, False); paint(459, True)
im.save(out); print("brazier art patched ->", out)
