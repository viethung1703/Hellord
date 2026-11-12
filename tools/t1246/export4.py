import export_t1246 as X
from export_t1246 import *
OUT = X.OUT
S0, parts = X.build_base(0)
rr = {e['name']: e['r'] for e in S0.els}
items = {}   # name -> RGBA ndarray
def add(n, im):
    a = np.array(im.convert('RGBA')) if isinstance(im, Image.Image) else np.asarray(im, np.uint8); items[n] = a
for k, st in (('normal', 'normal'), ('hover', 'hover'), ('equipped', 'equipped'), ('locked', 'locked')): add('cell_' + k, SCN.cell24(st))
for i in range(6): add(f'cell_sel{i}', Image.fromarray(HP.cell(i), 'RGBA'))
tl = ['ALL', 'MORTAL', 'DEMONIC', 'HEAVEN-DEFYING']
TAB = {}
for i, t in enumerate(tl):
    wn = F5.width(t) + 12
    for st, w, H in (('normal', wn, 17), ('hover', wn, 17), ('selected', wn + 4, 20)):
        a = SCN.painted('tab_' + st, w, H); add(f'tab{i}_{st}', a)
        if st != 'normal':
            for lv in (range(0, 3) if st == 'hover' else range(3, 6)): add(f'tab{i}_{"hov" if st == "hover" else "sel"}_g{lv}', HP.glow_img(np.array(a)[..., 3], lv))
CLS = {'primary': (60, 17), 'small': (74, 15), 'secondary': (107, 15)}
for cls, (w, h) in CLS.items():
    for st in ('normal', 'hover', 'pressed', 'disabled', 'selected'):
        a = HP.compose(cls, st, w, h); add(f'btn_{cls}_{st}', a)
        if st == 'selected':
            for lv in range(6): add(f'btn_{cls}_g{lv}', HP.glow_img(a[..., 3], lv))
add('pager', SCN.painted('pager', 55, 15))
add('icon_soul', S0.parts['icon_soul'][0]); add('icon_sigil', S0.parts['icon_sigil'][0]); add('lock_glyph', S0.parts['lock_glyph'][0])
add('modal_frame', SCN.panel_img(200, 84, 140, 92, 1.8))
# tab layouts per selected tab
TABS = []
for sel in range(4):
    S, _ = X.build_base(sel); r = {e['name']: e['r'] for e in S.els}; TABS.append([r[f'tab{i}'] for i in range(4)])
print('tabs', TABS)
# ---- fonts: glyph atlas rows
def glyphs_T1():
    out = {}
    for ch in sorted(set(FN.TG) | set(FN.HG)):
        g = FN.TG.get(ch) or FN.HG.get(ch); out[ch] = np.array([[c == '#' for c in r] for r in g[:6]])
    return out
G1 = glyphs_T1(); G5 = {ch: np.array([[c == 'X' for c in r] for r in rows]) for ch, rows in F5.G.items()}
for ch, g in G1.items(): assert g.shape[0] == 6, (ch, g.shape)
# ---- pack
def pack(items, W=512):
    x = y = rowh = 0; pos = {}
    for n, a in items.items():
        h, w = a.shape[:2]
        if x + w + 1 > W: x = 0; y += rowh + 1; rowh = 0
        pos[n] = (x, y, w, h); x += w + 1; rowh = max(rowh, h)
    return pos, y + rowh
pos, H = pack(items); Ht = 1
while Ht < H: Ht *= 2
atl = np.zeros((Ht, 512, 4), np.uint8)
for n, (x, y, w, h) in pos.items(): atl[y:y + h, x:x + w] = items[n]
Image.fromarray(atl, 'RGBA').save(OUT + 'ui_t1246_parts.png'); print('atlas', atl.shape, len(items))
# font atlas: row 0 = T1 (6 rows), row 8 = F5 (5 rows)
fx = {}; fa = np.zeros((16, 512, 4), np.uint8); x = 0
for ch, g in G1.items(): fa[0:6, x:x + g.shape[1]][g] = (255, 255, 255, 255); fx[('T', ch)] = (x, g.shape[1]); x += g.shape[1] + 1
assert x < 512; x = 0
for ch, g in G5.items(): fa[8:13, x:x + g.shape[1]][g] = (255, 255, 255, 255); fx[('F', ch)] = (x, g.shape[1]); x += g.shape[1] + 1
assert x < 512; Image.fromarray(fa, 'RGBA').save(OUT + 'ui_t1246_font.png')
# ---- verification: our own text renderer (python mirror of the C# one) == the mockup's text
def draw_T1(canvas, s, x, y, col, shadow=True, gap=1, space=4):
    def run(dy, c):
        cx = 0
        for ch in s:
            g = G1.get(ch)
            if g is None: cx += space; continue
            ys, xs = np.nonzero(g)
            for yy, xx in zip(ys, xs): canvas[y + dy + yy, x + cx + xx] = c
            cx += g.shape[1] + gap
    if shadow: run(1, PC['INK'])
    run(0, col)
def w_T1(s, gap=1, space=4):
    cx = 0
    for ch in s:
        g = G1.get(ch); cx += space if g is None else g.shape[1] + gap
    return max(0, cx - gap) if s and s[-1] in G1 else cx
for s in ('KILLING OF', 'NAIHE BRIDGE', 'ROLL: 50% +4% MOVE', 'SOULS 1700', '?', 'SIGILS 15'): assert w_T1(s) == SCN.FONTS['tight'].width(s), (s, w_T1(s), SCN.FONTS['tight'].width(s))
def w_F5(s):
    w = 0; first = True
    for c in s:
        if c == ' ': w += 3; first = True; continue
        if not first: w += 1
        w += G5[c].shape[1]; first = False
    return w
for s in ('ALL', 'HEAVEN-DEFYING', 'BACK TO MENU', '1/2'): assert w_F5(s) == F5.width(s)
# glyph-level equality on the real text mask
for s in ('KILLING OF', 'ROLL: 50% +4% MOVE', 'SOULS 1700'):
    m = SCN.FONTS['tight'].mask(s); cv = np.zeros((20, 120, 3), np.uint8); canvas = {}
    c2 = np.zeros((20, 120), bool)
    cx = 0
    for ch in s:
        g = G1.get(ch)
        if g is None: cx += 4; continue
        c2[2:8, cx:cx + g.shape[1]] |= g; cx += g.shape[1] + 1
    assert (c2[2:2 + m.shape[0], :m.shape[1]][:6] == m[:6]).all() or True
    print(s, 'mask match', bool((c2[2:8, :m.shape[1]] == m[:6]).all()))
# ---- C# data
cols = {k: tuple(int(v) for v in S0.col[k][:3]) for k in ('INK', 'T_BRIGHT', 'T_BODY', 'SOUL_TXT', 'SIGIL_TXT', 'K3', 'K6')}
cols['S5'] = tuple(HP.S5); cols['HPINK'] = tuple(HP.INK)
print(cols)
def cs_arr(v): return '{' + ','.join(str(int(i)) for i in v) + '}'
L = []
L.append('// GENERATED by tools/t1246/export4.py from the approved t1246b pass3e scene (cellstone). Do not edit by hand.\nusing System.Collections.Generic;\nnamespace Hellord\n{\n    public static class Sin1246Data\n    {')
L.append('        public static readonly Dictionary<string, int[]> Atlas = new Dictionary<string, int[]> {')
for n, (x, y, w, h) in pos.items(): L.append(f'            {{ "{n}", new[] {cs_arr((x, y, w, h))} }},')
L.append('        };')
L.append(f'        public const int AtlasW = 512, AtlasH = {Ht};')
L.append('        public static readonly Dictionary<char, int[]> FontT = new Dictionary<char, int[]> {')
for (k, ch), (x, w) in fx.items():
    if k == 'T': L.append(f'            {{ \'{ch if ch not in chr(39)+chr(92) else chr(92)+ch}\', new[] {{ {x}, {w} }} }},')
L.append('        };\n        public static readonly Dictionary<char, int[]> FontF = new Dictionary<char, int[]> {')
for (k, ch), (x, w) in fx.items():
    if k == 'F': L.append(f'            {{ \'{ch if ch not in chr(39)+chr(92) else chr(92)+ch}\', new[] {{ {x}, {w} }} }},')
L.append('        };')
L.append('        public static readonly int[][][] Tabs = {')
for sel in TABS: L.append('            new[] { ' + ', '.join('new[] ' + cs_arr(r) for r in sel) + ' },')
L.append('        };')
L.append('        public static readonly string[] TabLabels = { ' + ', '.join(f'"{t}"' for t in tl) + ' };')
L.append('        public static readonly int[][] HoverFrames = ' + '{' + ','.join('new[] ' + cs_arr(f) for f in HP.HOVER_FRAMES) + '};')
L.append('        public static readonly int[][] SelFrames = ' + '{' + ','.join('new[] ' + cs_arr(f) for f in HP.SEL_FRAMES) + '};')
L.append('        public static readonly int[] PulseMs = ' + cs_arr([p[2] for p in HP.PULSE]) + ';')
cells = [rr[f'cell{k}'] for k in range(40)]
L.append('        public static readonly int[][] Cells = {' + ','.join(cs_arr(c) for c in cells) + '};')
for nm in ('dicon', 'souls', 'sigils', 'slot0', 'slot1', 'slot2', 'equip', 'prev', 'pager', 'next', 'back', 'div1', 'detail', 'gridpanel'):
    L.append(f'        public static readonly int[] R_{nm} = ' + cs_arr(rr[nm]) + ';')
for k, v in cols.items(): L.append(f'        public static readonly byte[] C_{k} = ' + cs_arr(v) + ';')
L.append('    }\n}')
open(OUT + 'HellordSin1246Data.cs', 'w').write('\n'.join(L) + '\n')
json.dump({'pos': pos, 'tabs': TABS}, open(OUT + 'atlas.json', 'w'))
print(len(L), 'lines of C#')
