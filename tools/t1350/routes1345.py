"""t1345 orthogonal connector routes (pixel coordinates, 1px lines) + rasterisers for styles A (solid), B (dots), C (double rail)."""
import numpy as np
def build(POS, JOINT, HUB, JY=159):
    R = []   # (name, [points], tstart)   tstart=True: first point sits just beside a through-line (T junction)
    L = lambda k: POS[k][0] - 12; Rr = lambda k: POS[k][0] + 11; T = lambda k: POS[k][1] - 12; Bt = lambda k: POS[k][1] + 11
    ge, im, sp, gr = POS['GE'], POS['IM'], POS['SP'], POS['GR']
    j1 = JOINT[('OO', 'CK')]; j2 = JOINT[('QG', 'GM')]; j3 = JOINT[('MB', 'BO')]
    R.append(('GE-j1', [(j1[0], Bt('GE')), (j1[0], JY - 4)], False))
    R.append(('GE-IM', [(j1[0] + 1, im[1]), (L('IM'), im[1])], True))
    R.append(('IM-j2', [(j2[0], Bt('IM')), (j2[0], JY - 4)], False))
    hx, hy = HUB
    R.append(('hub', [(hx, hy - 20), (hx, JY)], False))
    R.append(('hub-j2', [(hx - 1, JY), (j2[0] + 4, JY)], True))
    R.append(('hub-j3', [(hx + 1, JY), (j3[0] - 4, JY)], True))
    for pair, j in JOINT.items():
        a, b = pair; mx = j[0]; xa, xb = POS[a][0], POS[b][0]; yb = JY + 10; yt = T(a) + 1 - 1
        R.append(('stem', [(mx, JY + 4), (mx, yb - 1)], False))
        R.append(('fk-' + a, [(mx - 1, yb), (xa, yb), (xa, T(a))], False))
        R.append(('fk-' + b, [(mx + 1, yb), (xb, yb), (xb, T(b))], False))
    qg, hh, ms, mb, bm, ca, bo, mh, ot = (POS[k] for k in ('QG', 'HH', 'MS', 'MB', 'BM', 'CA', 'BO', 'MH', 'OT'))
    R.append(('QG-HH', [(L('QG'), qg[1]), (224, qg[1]), (224, 216), (hh[0], 216), (hh[0], T('HH'))], False))
    R.append(('HH-MS', [(L('HH'), hh[1]), (ms[0], hh[1]), (ms[0], T('MS'))], False))
    R.append(('MB-BM', [(Rr('MB'), mb[1]), (407, mb[1]), (407, 216), (bm[0], 216), (bm[0], T('BM'))], False))
    R.append(('BM-CA', [(Rr('BM'), bm[1]), (ca[0], bm[1]), (ca[0], T('CA'))], False))
    R.append(('BO-OT', [(Rr('BO'), bo[1]), (L('OT'), bo[1])], False))
    R.append(('BO-MH', [(455, bo[1] + 1), (455, mh[1]), (L('MH'), mh[1])], True))
    R.append(('SP-j3', [(j3[0], Bt('SP')), (j3[0], JY - 4)], False))
    R.append(('SP-GR', [(Rr('SP'), sp[1]), (445, sp[1]), (445, gr[1]), (L('GR'), gr[1])], False))
    return R
def pix(pts):
    out = []
    for (x0, y0), (x1, y1) in zip(pts, pts[1:]):
        assert x0 == x1 or y0 == y1, (pts)
        n = max(abs(x1 - x0), abs(y1 - y0)); sx = np.sign(x1 - x0); sy = np.sign(y1 - y0)
        for i in range(n + 1):
            p = (x0 + sx * i, y0 + sy * i)
            if not out or out[-1] != p: out.append(p)
    return out
def mask_A(R, shape=(360, 640)):
    m = np.zeros(shape, bool)
    for _, pts, _t in R:
        for x, y in pix(pts): m[y, x] = True
    return m
def mask_B(R, shape=(360, 640)):
    m = np.zeros(shape, bool)
    for _, pts, _t in R:
        P = pix(pts); n = len(P); off = (n - 1) % 3 // 2
        for i in range(off, n, 3): m[P[i][1], P[i][0]] = True
    return m
def mask_C(R, shape=(360, 640)):
    m = np.zeros(shape, bool)
    for _, pts, tst in R:
        P = pix(pts)
        for idx, (x, y) in enumerate(P):
            if tst and idx == 0: continue                      # rails start beside the through line
        segs = list(zip(pts, pts[1:]))
        for si, ((x0, y0), (x1, y1)) in enumerate(segs):
            horiz = (y0 == y1); n = max(abs(x1 - x0), abs(y1 - y0)); sx = np.sign(x1 - x0); sy = np.sign(y1 - y0)
            for i in range(n + 1):
                x = x0 + sx * i; y = y0 + sy * i
                if tst and si == 0 and i == 0: continue
                if horiz: m[y - 1, x] = True; m[y + 1, x] = True
                else: m[y, x - 1] = True; m[y, x + 1] = True
        for si in range(1, len(pts) - 1):                            # corner: fill the outer rail pixel
            (xa, ya), (xc, yc), (xb, yb) = pts[si - 1], pts[si], pts[si + 1]
            dx1 = np.sign(xc - xa); dy1 = np.sign(yc - ya); dx2 = np.sign(xb - xc); dy2 = np.sign(yb - yc)
            ox = -(dx1 + dx2) if dx1 == 0 or dx2 == 0 else 0
            # incoming direction (dx1,dy1), outgoing (dx2,dy2): outer corner lies opposite to the turn
            ox = -(dx1 if dx1 else dx2); oy = -(dy1 if dy1 else dy2)
            m[yc + oy, xc + ox] = True
    return m
