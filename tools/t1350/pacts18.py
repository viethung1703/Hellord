"""t1350: copy of work/pacts17.py (t1345 mockup layout) used by pt_export.py; no main. Original: t1321 v5: PACT_INSIDE from v4 A (lava cavern). No arch/capitals. UI re-laid out, shorter spaced text, labels auto-placed (below/above/right/left) so no connector line runs behind a label. Fonts: d2_ui SMALL/DISP exactly as Sins v6. MOCKUP NOT RUNTIME"""
import sys, os, math, random; sys.dont_write_bytecode = True
sys.path.insert(0, '/workspace/Hellord/Art/previews/t1287_redraw/work'); sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import numpy as np
import eng2 as E, d2_ui as S, mk, bgproc5 as BP, bgproc6 as BQ
import pacts6 as P, pacts9 as Q
from kit import socket, rect_mask
OUT = '/workspace/Hellord/Art/previews/t1328_lava/t1345_ui/'; BGD = '/workspace/Hellord/Art/previews/t1321_pacts_menu_theme/v5/background/'
RAW = '/workspace/Hellord/Art/previews/t1321_pacts_menu_theme/v4/background/PIXELLAB_RAW_A_lava_cavern_640x360_AI_GENERATED_MOCKUP_NOT_RUNTIME.png'
SM, DS = S.SMALL, S.DISP
Q.CELL = 22; Q.Y0 = 48; Q.ROWY = {'A': 0, 'B': 46, 'C': 104, 'D': 160, 'E': 214}
NODE_X = {'HH': 140, 'SP': 330}
DROP = {('MS', 'OO'), ('HH', 'GM'), ('BO', 'BM'), ('MB', 'SP')}
Q.EDGES = [e for e in Q.EDGES if e not in DROP]
X0, Y0, CELL, ROWY, NODE = Q.X0, Q.Y0, Q.CELL, Q.ROWY, Q.NODE
for _k, _x in NODE_X.items(): NODE[_k] = NODE[_k][:2] + (_x,) + NODE[_k][3:]
hh = Q.hh

import eng2 as _E2
_TG = {
 'A':'.#./#.#/###/#.#/#.#','B':'##./#.#/##./#.#/##.','C':'.##/#../#../#../.##','D':'##./#.#/#.#/#.#/##.','E':'###/#../##./#../###','F':'###/#../##./#../#..',
 'G':'.##/#../#.#/#.#/.##','H':'#.#/#.#/###/#.#/#.#','I':'###/.#./.#./.#./###','J':'..#/..#/..#/#.#/.#.','K':'#.#/#.#/##./#.#/#.#','L':'#../#../#../#../###',
 'M':'#...#/##.##/#.#.#/#...#/#...#','N':'#..#/##.#/#.##/#..#/#..#','O':'.#./#.#/#.#/#.#/.#.','P':'##./#.#/##./#../#..','Q':'.#./#.#/#.#/###/..#','R':'##./#.#/##./#.#/#.#',
 'S':'.##/#../.#./..#/##.','T':'###/.#./.#./.#./.#.','U':'#.#/#.#/#.#/#.#/###','V':'#.#/#.#/#.#/#.#/.#.','W':'#...#/#...#/#.#.#/##.##/#...#','X':'#.#/#.#/.#./#.#/#.#',
 'Y':'#.#/#.#/.#./.#./.#.','Z':'###/..#/.#./#../###',
 '0':'###/#.#/#.#/#.#/###','1':'.#./##./.#./.#./###','2':'##./..#/.#./#../###','3':'##./..#/.##/..#/##.','4':'#.#/#.#/###/..#/..#','5':'###/#../##./..#/##.',
 '6':'.##/#../###/#.#/###','7':'###/..#/.#./.#./.#.','8':'###/#.#/###/#.#/###','9':'###/#.#/###/..#/##.','/':'..#/..#/.#./#../#..','-':'.../.../###/.../...',
}
TINY = _E2.Glyphs('tiny', {k: v.split('/') for k, v in _TG.items()}, space=2, gap=1, hgt=5)

V130={'GM':(753,372),'IM':(664,246),'HH':(664,498),'QG':(636,372),'GE':(542,131),'MS':(542,613),'OO':(394,372),'CK':(511,372),'LE':(768,124),'MB':(925,372),'SP':(1023,246),'BM':(1023,498),'BO':(1055,372),'GR':(1158,131),'CA':(1158,613),'MH':(1206,543),'OT':(1191,372),'BC':(940,128)}
_GX=[394,511,636,753,835,925,1055,1191]; _MX=[154,201,249,296,344,391,439,486]
def mapx(v):
    if v >= _GX[-1]: return min(486, _MX[-1] + (v - _GX[-1]) * 0.35)
    for i in range(len(_GX) - 1):
        if _GX[i] <= v <= _GX[i + 1]: return _MX[i] + (v - _GX[i]) / (_GX[i + 1] - _GX[i]) * (_MX[i + 1] - _MX[i])
POS={'GE':(170,62),'LE':(296,62),'BC':(392,62),'GR':(478,62),'IM':(272,119),'SP':(410,119),'OO':(152,187),'CK':(200,187),'QG':(248,187),'GM':(296,187),'MB':(386,187),'BO':(434,187),'OT':(482,187),'HH':(248,234),'BM':(386,234),'MH':(482,234),'MS':(190,281),'CA':(430,281)}
HUBPOS=(341,187)
PAIRS=(('OO','CK'),('QG','GM'),('MB','BO'))
Q.center=lambda k: POS[k]

from PIL import Image
_IC24 = {}
ALT='/workspace/HellordRepo/Assets/Hellord/Resources/HellordArt/Altar97/'
def icon24(i, dim=False):
    k = (i, dim)
    if k not in _IC24:
        im = Image.open(ALT + f'icon_4_{i}{"_dim" if dim else ""}.png').convert('RGBA')
        a = np.array(im).astype(float); al = a[..., 3:4] / 255.0
        pm = Image.fromarray(np.dstack([a[..., :3] * al, a[..., 3:4]]).astype(np.uint8), 'RGBA')   # premultiplied resize, no halo
        r = np.array(pm.resize((20, 20), Image.LANCZOS)).astype(float); aa = r[..., 3:4] / 255.0
        rgb = np.where(aa > 0.02, r[..., :3] / np.maximum(aa, 0.02), 0)
        out = np.dstack([np.clip(rgb, 0, 255), np.where(aa > 0.5, 255, 0)]).astype(np.uint8)
        _IC24[k] = Image.fromarray(out, 'RGBA')
    return _IC24[k]
def node24(cv, x, y, ico, state, big, sel):
    lit, dark, fill = S.profiles('locked' if state == 'locked' else 'normal', 0.15)
    socket(cv, x, y, rect_mask(CELL, CELL, 4 if big else 2), lit, dark, fill)
    cv.asset_blit(icon24(ico, state == 'locked'), x + 1, y + 1)
    if state == 'locked': cv.asset_blit(E.LOCK, x + CELL - 10, y + 1)
    if sel:
        for (cx_, cy_, sx, sy) in ((x - 2, y - 2, 1, 1), (x + CELL + 1, y - 2, -1, 1), (x - 2, y + CELL + 1, 1, -1), (x + CELL + 1, y + CELL + 1, -1, -1)):
            for i in range(4): cv.p(cx_ + sx * i, cy_, 'ORANGE'); cv.p(cx_, cy_ + sy * i, 'ORANGE')
_HUB = None
def fork(cv, cx, cy):
    D = ['...d...', '..dAd..', '.dAAAd.', 'dAAAAAd', '.dAAAd.', '..dAd..', '...d...']
    cv.blit(D, dict(d='DEEP', A='AMBER'), cx - 3, cy - 3)
    for k in range(-2, 3): cv.p(cx + k, cy, 'DEEP') if abs(k) < 3 else None       # split line through the diamond
    cv.p(cx - 1, cy - 1, 'ORANGE'); cv.p(cx + 1, cy + 1, 'ORANGE'); cv.p(cx, cy - 2, 'YELLOW')
def hub13(cv, cx, cy):
    R = 16
    for j in range(-R, R + 1):
        for i in range(-R, R + 1):
            if math.hypot(i, j) <= R + 0.5: cv.p(cx + i, cy + j, 'DEEP' if math.hypot(i, j) > R - 2 else 'N1')
    im = Image.open(ALT + 'necro130_hub_off.png').convert('RGBA'); a = np.array(im).astype(float); al = a[..., 3:4] / 255.0
    pm = Image.fromarray(np.dstack([a[..., :3] * al, a[..., 3:4]]).astype(np.uint8), 'RGBA'); r = np.array(pm.resize((38, 38), Image.LANCZOS)).astype(float); aa = r[..., 3:4] / 255.0
    rgb = np.where(aa > 0.02, r[..., :3] / np.maximum(aa, 0.02), 0)
    cv.asset_blit(Image.fromarray(np.dstack([np.clip(rgb, 0, 255), np.where(aa > 0.5, 255, 0)]).astype(np.uint8), 'RGBA'), cx - 19, cy - 19)
def decor(cv):
    for x in range(640):                                          # cavern ceiling, jagged lower edge with lava-lit tips
        d = 4 + hh(x // 6) % 5 + int(2 * math.sin(x * 0.05))
        if x < 46 or x > 593: d += int(26 * (1 - (x if x < 46 else 639 - x) / 46.0)) * (1 if hh(x // 5) % 3 else 0) // 1
        d = min(d, 30)
        for y in range(d + 1):
            c = 'DEEP' if y < d - 6 else 'N2' if y < d - 3 else 'PLUM' if (x + y) % 3 else 'N2'
            if y == d: c = 'P2'
            cv.p(x, y, c)
        if hh(x) % 4 == 0: cv.p(x, d + 1, 'R0')
        if hh(x) % 11 == 0: cv.p(x, d + 2, 'RED')
    for x in range(640):                                          # lava river + rock lip along the bottom
        lip = 329 + (hh(x // 5) % 3) + int(1.5 * math.sin(x * 0.04))
        for y in range(lip, 360):
            if y < lip + 4: c = ('P2', 'PLUM', 'N2', 'DEEP')[y - lip] if (x + y) % 4 else 'N2'
            else:
                t = math.sin(x * 0.11 + y * 0.8) + math.sin(x * 0.037 - y * 0.35) + 0.6 * math.sin(x * 0.3 + y * 0.2)
                c = 'DRED' if y > lip + 20 else 'RED'
                if t > 1.2: c = 'YELLOW' if (t > 1.9 and y < lip + 14) else 'AMBER'
                elif t > 0.5: c = 'ORANGE'
                elif t < -0.9: c = 'DRED' if y > lip + 9 else 'R0'
                if y == lip + 4: c = 'ORANGE'
            cv.p(x, y, c)
    for bx, bh in ((70, 15), (190, 9), (452, 11), (566, 16)):      # rock spires standing in the lava
        for j in range(bh):
            hw = int(5 * (1 - j / bh)) + 1
            for i in range(-hw, hw + 1): cv.p(bx + i, 355 - j, 'DEEP' if i > 1 else 'P2' if i < -1 else 'PLUM')
        cv.p(bx, 355 - bh, 'R0')
    rr = random.Random(5)
    for _ in range(40): cv.p(rr.randrange(0, 640), rr.randrange(322, 330), 'ORANGE' if rr.random() < .5 else 'AMBER')
JY = 159
JOINT = {('OO', 'CK'): ((POS['OO'][0] + POS['CK'][0]) // 2, JY), ('QG', 'GM'): ((POS['QG'][0] + POS['GM'][0]) // 2, JY), ('MB', 'BO'): ((POS['MB'][0] + POS['BO'][0]) // 2, JY)}
PAIRNODES = {n for p_ in PAIRS for n in p_}
import routes1345 as RT
def segs():
    out = []
    for nm, pts, _t in RT.build(POS, JOINT, HUBPOS):
        for p, q in zip(pts, pts[1:]): out.append((p, q, nm))
    return out
def cell_rect(k): return (POS[k][0] - CELL // 2, POS[k][1] - CELL // 2, CELL, CELL)
def hit(r, r2, m=1): return not (r[0] + r[2] + m <= r2[0] or r2[0] + r2[2] + m <= r[0] or r[1] + r[3] + m <= r2[1] or r2[1] + r2[3] + m <= r[1])
def place_labels():
    BX = (136, 46, 368, 266)                                        # tree bounds (abs)
    hx, hy = HUBPOS
    fixed = [(hx - 20, hy - 20, 40, 40), (hx - 40, 264, 80, 20), (296, 297, 96, 12)]
    tags = {}
    for k in ('LE', 'BC'):
        cx = POS[k][0] + (-6 if k == 'LE' else 6); tags[k] = (cx - 28, POS[k][1] + 17, 56, 17); fixed.append(tags[k])
    for pair in (('OO', 'CK'), ('QG', 'GM'), ('MB', 'BO')):
        (x1, y1), (x2, y2) = Q.center(pair[0]), Q.center(pair[1]); mx, y1 = JOINT[pair]; fixed.append((mx - 5, y1 - 5, 11, 11))
    cells = {k: cell_rect(k) for k in NODE}; S_ = segs(); placed = {}; rects = []
    def seg_hits(r):
        n = 0
        for (p, q, ab) in S_:
            L = int(max(abs(q[0] - p[0]), abs(q[1] - p[1]))) + 1
            for t in range(L + 1):
                x = p[0] + (q[0] - p[0]) * t / L; y = p[1] + (q[1] - p[1]) * t / L
                if r[0] - 3 <= x < r[0] + r[2] + 3 and r[1] - 3 <= y < r[1] + r[3] + 3: n += 1; break
        return n
    valid = {}
    for k in NODE:
        lab, _, cxn, row, st, big = NODE[k]; c = cells[k]; cx = c[0] + CELL // 2; cy = c[1] + CELL // 2
        forms = [lab] + ([[' '.join(lab)]] if len(lab) > 1 else [])
        cands = []
        for f in forms:
            w = max(TINY.width(l) for l in f); h = 6 * len(f) - 1
            for dx in (0, 1, -1, 2, -2, 3, -3, 4, -4):
                cands += [('below', f, (cx - w // 2 + dx * 6, c[1] + CELL + 2, w, h)), ('above', f, (cx - w // 2 + dx * 6, c[1] - 3 - h, w, h))]
            cands += [('below', f, (c[0] + CELL - w, c[1] + CELL + 2, w, h)), ('below', f, (c[0], c[1] + CELL + 2, w, h)), ('above', f, (c[0] + CELL - w, c[1] - 3 - h, w, h)), ('above', f, (c[0], c[1] - 3 - h, w, h))]
            for dy in (0, -8, 8, -14, 14, 22, -22, 28, -28):
                cands += [('right', f, (c[0] + CELL + 4, cy - h // 2 + dy, w, h)), ('left', f, (c[0] - 4 - w, cy - h // 2 + dy, w, h))]
        if k == 'LE': cands = [c_ for c_ in cands if c_[0] in ('left', 'above')]
        if k == 'BC': cands = [c_ for c_ in cands if c_[0] in ('right', 'above')]
        v = []
        for (pos, f, r) in cands:
            if not (BX[0] <= r[0] and r[0] + r[2] <= BX[0] + BX[2] and BX[1] <= r[1] and r[1] + r[3] <= BX[1] + BX[3]): continue
            if any(hit(r, cells[o], 12) for o in cells if o != k) or any(hit(r, o, 12) for o in fixed) or seg_hits(r): continue
            v.append((pos, f, r))
        if not v:                                                      # fall back: allow a label to sit over a connector line (never over a node/label)
            for (pos, f, r) in cands:
                if not (BX[0] <= r[0] and r[0] + r[2] <= BX[0] + BX[2] and BX[1] <= r[1] and r[1] + r[3] <= BX[1] + BX[3]): continue
                if any(hit(r, cells[o], 12) for o in cells if o != k) or any(hit(r, o, 12) for o in fixed): continue
                v.append((pos, f, r))
        valid[k] = v
        if not v: print('NO STATIC CANDIDATE', k)
    order = sorted(NODE, key=lambda k: len(valid[k])); sol = {}
    def dfs(i):
        if i == len(order): return True
        k = order[i]
        for cnd in valid[k]:
            if all(not hit(cnd[2], sol[o][2], 12) for o in sol):
                sol[k] = cnd
                if dfs(i + 1): return True
                del sol[k]
        return False
    ok = dfs(0); print('label layout solved:', ok)
    placed = {k: (0,) + sol[k] for k in sol} if ok else {}
    return placed, tags
def wrap(t, w):
    L = []; cur = ''
    for wd in t.split(' '):
        c = (cur + ' ' + wd).strip()
        if SM.width(c) <= w: cur = c
        else: L.append(cur); cur = wd
    L.append(cur); return L
def inside(bg):
    cv = E.Cv('INK'); cv.a[:] = bg; decor(cv)
    w = P.TITLE.width('NECROMANCER NODES'); P.TITLE.draw(cv, 320 - w // 2, 14, 'NECROMANCER NODES', 'BONE', leg={'+': 'BONE', '=': 'TERRA'}, role='title', outline='DEEP', rowcols=['BONE'] * 6 + ['CLAY'] * 3 + ['TERRA'] * 2)
    cv.asset_blit(E.ICON_SOUL, 132, 17); SM.draw(cv, 144, 16, 'SOULS 1670', 'YELLOW', role='currency', grp='cs')
    ix = 508 - E.ICON_SIGIL.width; SM.draw(cv, ix - 4 - SM.width('SIGILS 15'), 16, 'SIGILS 15', 'PINK', role='currency', grp='cg'); cv.asset_blit(E.ICON_SIGIL, ix, 17)
    S.panel(cv, 126, 34, 388, 287); S.panel(cv, 7, 34, 109, 287); S.panel(cv, 524, 34, 109, 287)
    # left column
    lit, dark, fill = S.profiles('selected', 0.15); socket(cv, 28, 52, rect_mask(66, 66, 3), lit, dark, fill); cv.asset_blit(P.avatar(1, False), 29, 53)
    P.box(cv, 18, 128, 87, 15); P.ct(cv, 61, 132, 'NECROMANCER', 'BONE', 'plate', (18, 128, 87, 15))
    for i, t in enumerate(('COMMAND THE', 'DEAD AND BONE')): P.ct(cv, 61, 156 + i * 11, t, 'DIM', 'desc', (16, 150, 91, 30))
    P.ct(cv, 61, 196, 'YOUR PACT 0/8', 'BONE', 'pick', (16, 190, 91, 12))
    for i in range(8):
        sx = 17 + (i % 4) * 23; sy = 212 + (i // 4) * 25; socket(cv, sx, sy, rect_mask(20, 20, 3), ['P2', 'DEEP', 'INK'], ['P2', 'DEEP', 'INK'], lambda j, i_, d: 'INK'); cv.rect(sx + 8, sy + 9, 4, 1, 'N2')
    P.keyed(cv, 18, 264, 87, 'BACK', 'B')
    # right column
    P.ct(cv, 578, 52, 'KEYSTONE SLOT', 'BONE', 'ks'); SM.draw(cv, 559, 68, '0/2', 'AMBER', role='ks')
    for k in range(2): cv.blit(['...d...', '..dBd..', '.dB.Bd.', 'dB...Bd', '.dB.Bd.', '..dBd..', '...d...'], dict(d='DEEP', B='P2'), 581 + k * 10, 66)
    P.ct(cv, 578, 83, 'MAX 2', 'DIM', 'ks')
    P.box(cv, 534, 104, 89, 112); r = (534, 104, 89, 112)
    DS.draw(cv, 539, 112, 'GRAVE MUSTER', 'BONE', role='dname', cont=r, rowcols=['BONE'] * 6 + ['CLAY'] * 3, outline='DEEP')
    for y, t, c in ((130, 'NOTABLE', 'DIM'), (146, 'READY TO SIGN', 'AMBER'), (162, 'PICK 0/5', 'BONE'), (182, '+1 TO EACH', 'DIM'), (194, 'MINION TYPE', 'DIM')): SM.draw(cv, 539, y, t, c, role='dtext', cont=r)
    P.box(cv, 534, 228, 89, 54); r = (534, 228, 89, 54)
    for y, t, c in ((236, 'GRAVECALLER', 'BONE'), (249, 'LOCKED 0/4', 'DIM'), (262, '+15% MINIONS', 'CLAY')): SM.draw(cv, 539, y, t, c, role='crest', cont=r)
    # tree
    hx, hy = HUBPOS
    placed, tags = place_labels()
    hub13(cv, hx, hy)
    for pair in (('OO', 'CK'), ('QG', 'GM'), ('MB', 'BO')):
        (x1, y1), (x2, y2) = Q.center(pair[0]), Q.center(pair[1]); mx, y1 = JOINT[pair]; fork(cv, mx, y1)
    for k, (lab, ico, cx_, rw, st, big) in NODE.items():
        c = cell_rect(k); node24(cv, c[0], c[1], ico, st, big, k == 'GM')
        bad, pos, f, rr = placed[k]
        for j, ln in enumerate(f):
            lw = TINY.width(ln); lx = rr[0] + (rr[2] - lw) // 2 if pos in ('below', 'above') else (rr[0] if pos == 'right' else rr[0] + rr[2] - lw)
            TINY.draw(cv, lx, rr[1] + j * 6, ln, 'BONE' if st == 'open' else 'DIM', role='nodelabel')
    for k, l1 in (('LE', 'ANY 2 SUMMON'), ('BC', 'ANY 2 BONE')):
        t = tags[k]; cx_ = t[0] + 28; P.box(cv, *t, ('B2', 'DEEP', 'INK')); col = 'AMBER' if k == 'LE' else 'DIM'
        TINY.draw(cv, cx_ - TINY.width(l1) // 2, t[1] + 3, l1, col, role='tag', cont=t); TINY.draw(cv, cx_ - TINY.width('NOTABLES') // 2, t[1] + 9, 'NOTABLES', col, role='tag', cont=t)
    sx, sy = HUBPOS[0] - 40, 264; fork(cv, 302, 302); TINY.draw(cv, 308, 300, 'PICK ONE OF A PAIR', 'DIM', role='legend'); S.button(cv, sx, sy, 80, 20, 'SIGN', 'normal', 0.15, True); SM.draw(cv, sx + 7, sy + 6, 'A', 'DIM', role='key', cont=(sx, sy, 80, 20)); cv.vl(sx + 17, sy + 4, 12, 'R0')
    return cv

def _clear_report(placed, tags):
    import itertools
    items = {}
    for k in NODE:
        c = cell_rect(k); r = placed[k][3]
        x0 = min(c[0], r[0]); y0 = min(c[1], r[1]); x1 = max(c[0]+c[2], r[0]+r[2]); y1 = max(c[1]+c[3], r[1]+r[3]); items[k] = (x0, y0, x1-x0, y1-y0)
    hx, hy = HUBPOS; items['HUB'] = (hx-19, hy-19, 38, 38)
    for k, t in tags.items(): items['TAG'+k] = t
    for i, (pr, (mx, jy)) in enumerate(JOINT.items()): items['J%d' % i] = (mx-5, jy-5, 11, 11)
    items['SIGN'] = (hx-40, 264, 80, 20); items['LEGEND'] = (296, 297, 96, 12)
    worst = 999
    for (a, ra), (b, rb) in itertools.combinations(items.items(), 2):
        gx = max(ra[0]-(rb[0]+rb[2]), rb[0]-(ra[0]+ra[2])); gy = max(ra[1]-(rb[1]+rb[3]), rb[1]-(ra[1]+ra[3])); g = max(gx, gy)
        own = lambda n, j: n in PAIRNODES and any(n in p for p in PAIRS) and j.startswith('J')
        if g < 12: print('  tight', a, b, g)
        worst = min(worst, g)
    print('min pair gap', worst)
    for k, r in items.items():
        m = min(r[0]-126, 514-(r[0]+r[2]), r[1]-34, 321-(r[1]+r[3]))
        if m < 10: print('  edge', k, m)

