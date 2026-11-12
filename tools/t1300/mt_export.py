"""t1300: exports the runtime assets of the menu-theme Sins screen (D2 Morta v6, lava bg dimB) using the t1287 engine + the main-menu fire60 flame, unchanged (read-only imports).
Writes Assets/Hellord/Resources/HellordArt/MenuTheme/{mt_bg,mt_scene,mt_atlas}.png and Assets/Hellord/Scripts/Runtime/HellordMenuThemeData.cs (generated)."""
import sys, os, json, math, random; sys.dont_write_bytecode = True
sys.path.insert(0, '/workspace/Hellord/tools/t1300')
import mt_static as MS
import numpy as np
from PIL import Image
import eng2 as E
from eng2 import PAL
import scene5 as SCN, d2_ui as S, backdrop as B, glyphs as G, screen5 as SC
sys.path.insert(0, '/workspace/Hellord/Art/previews/step36_main_menu/v69_pact_seal_hover_simple/scripts')
import fire60
R6 = '/workspace/Hellord/Art/previews/t1287_redraw/D2_MORTA_v6/'
OUT = '/workspace/Hellord/Assets/Hellord/Resources/HellordArt/MenuTheme/'
CS = '/workspace/Hellord/Assets/Hellord/Scripts/Runtime/HellordMenuThemeData.cs'
KA, KB = MS.KEYA, MS.KEYB
SPR = {}   # name -> RGBA uint8 array (tight)
def keyed(fn, box):
    """render fn(cv) on two key colours; box=(x0,y0,x1,y1) crop; alpha = pixels that changed from the key"""
    outs = []
    for k in (KA, KB):
        cv = E.Cv('INK'); cv.a[:] = k; B.ST.pop(id(cv), None); fn(cv); outs.append(cv.a.copy())
    a, b = outs; x0, y0, x1, y1 = box; d = (a != b).any(axis=2)[y0:y1, x0:x1]
    rgba = np.zeros((y1 - y0, x1 - x0, 4), np.uint8); rgba[..., :3] = a[y0:y1, x0:x1]; rgba[..., 3] = np.where(d, 0, 255); rgba[d, :3] = 0
    # d == pixels equal to key on BOTH?  (a!=b means follows the key -> background)
    return rgba
def add(name, rgba): SPR[name] = rgba
# ---------------- widgets (no text; text is drawn at runtime from the same glyph sets)
def cells():
    for nm, kind in (('cell_normal', 'normal'), ('cell_equipped', 'equipped'), ('cell_locked', 'locked'), ('cell_empty', 'empty')):
        add(nm, keyed(lambda cv: S.cell(cv, 10, 10, kind, 0, 0.0, False), (10, 10, 38, 38)))
    # selected = breath colour BLOOD -> DRED -> RED (E.breath ramp), 3 pre-built steps
    for i, nm in enumerate(['cell_sel0', 'cell_sel1', 'cell_sel2']):
        col = ['BLOOD', 'DRED', 'RED'][i]
        def f(cv, col=col):
            m = S.rect_mask(28, 28, 3); S.socket(cv, 10, 10, m, [col, 'R0', 'R0'], [col, 'R0', 'R0'], lambda j, ii, d: 'DEEP' if j < 6 else 'INK')
        add(nm, keyed(f, (10, 10, 38, 38)))
    for nm, kind in (('niche_normal', 'normal'), ('niche_locked', 'locked')):
        add(nm, keyed(lambda cv: S.niche(cv, 10, 10, kind, None, 0.0, False), (10, 10, 38, 42)))
def buttons():
    def btn(cv, w, h, st, prim): S.button(cv, 10, 10, w, h, '', st, 0.0, prim)
    for (nm, w, h) in (('s', 60, 20), ('b', 136, 20)):
        for st in ('normal', 'hover', 'disabled'): add(f'btn_{nm}_{st}', keyed(lambda cv: btn(cv, w, h, st, False), (10, 10, 10 + w, 10 + h)))
    for st in ('normal', 'disabled'): add(f'btn_ps_{st}', keyed(lambda cv: btn(cv, 60, 20, st, True), (10, 10, 70, 30)))
    w = 236; h = 24
    for st in ('normal', 'disabled'): add(f'btn_p_{st}', keyed(lambda cv: btn(cv, w, h, st, True), (10, 10, 10 + w, 10 + h)))
    add('counter', keyed(lambda cv: S.counter(cv, 10, 10, 44, 20, ''), (10, 10, 54, 30)))
def icons():
    for nm, im in (('icon_soul', E.ICON_SOUL), ('icon_sigil', E.ICON_SIGIL), ('lock_glyph', E.LOCK)): add(nm, np.array(im.convert('RGBA')))
# ---------------- sconce fire
FLAME_W, FLAME_H = 13, 19            # art px (x2 = 26 px wide, same width as the approved hand-set flame; menu flame is 17 wide)
SEEDS = {'L': 1, 'R': 2}
def flames():
    for sd, seed in SEEDS.items():
        for f in range(fire60.NF):
            im = fire60.flame(f, FLAME_W, FLAME_H, seed)          # unchanged main-menu flame generator
            im = im.resize((im.width * 2, im.height * 2), Image.NEAREST); a = np.array(im.convert('RGBA'))
            # the menu's ember bed colour 7a1020 is not in the menu palette family -> nearest family colour R0 (6e1c2a)
            m = (a[..., 0] == 0x7a) & (a[..., 1] == 0x10) & (a[..., 2] == 0x20); a[m, :3] = PAL['R0']
            add(f'flame{sd}{f:02d}', a)
def heap_and_bowl():
    def heap(fr):
        def f(cv):
            for j, row in enumerate(B.HEAP):
                for i, ch in enumerate(row):
                    if ch == '.': continue
                    if ch in 'OAY' and (i + fr) % 3 == 0: ch = {'O': 'A', 'A': 'O', 'Y': 'A'}[ch]
                    cv.p(10 + i, 10 + j, SCN.COAL[ch])
        return f
    for fr in range(3): add(f'coal{fr}', keyed(heap(fr), (10, 10, 37, 13)))
    def bowl(cv):
        SCN.bowl_draw(cv, 30, 20)          # cx = 30, rim_y = 20
    add('bowl', keyed(bowl, (30 - 16, 20, 30 + 16, 20 + 24)))
def halo():
    # pixel-art firelight on the background: binary ordered-dither of a radial falloff (white mask; tinted + alpha-flickered at runtime)
    rx, ry = 40, 56; w, h = rx * 2 + 1, ry * 2 + 1; bay = np.array([[0, 8, 2, 10], [12, 4, 14, 6], [3, 11, 1, 9], [15, 7, 13, 5]]) / 16.0 + 1 / 32
    a = np.zeros((h, w, 4), np.uint8)
    for y in range(h):
        for x in range(w):
            d = math.hypot((x - rx) / rx, (y - ry) / ry)
            if d >= 1: continue
            v = (1 - d) ** 1.6
            if v > bay[y % 4, x % 4] * 0.9: a[y, x] = (255, 255, 255, 255)
    add('halo', a)
LIGHT_F = [0.70, 0.85, 1.00, 1.12, 1.26]
def lights():
    for k, f in enumerate(LIGHT_F):
        cv0, _ = MS.static_screen(MS.KEYA); base = cv0.a.copy()
        cv, _ = MS.static_screen(MS.KEYA)
        for (cx, cy) in ((21, 196), (619, 196)): SCN.light(cv, cx, cy, 58, 84, gain=3.1 * f, rows=(0, SCN.FLOOR_Y))
        d = (cv.a != base).any(axis=2)
        for side, (xa, xb) in (('L', (0, 320)), ('R', (320, 640))):
            dd = np.zeros_like(d); dd[:, xa:xb] = d[:, xa:xb]; ys, xs = np.nonzero(dd)
            x0, x1, y0, y1 = xs.min(), xs.max() + 1, ys.min(), ys.max() + 1
            a = np.zeros((y1 - y0, x1 - x0, 4), np.uint8); sub = dd[y0:y1, x0:x1]; a[..., :3] = cv.a[y0:y1, x0:x1]; a[..., 3] = np.where(sub, 255, 0)
            add(f'light{side}{k}', a); LIGHT_POS[f'light{side}{k}'] = (int(x0), int(y0))
LIGHT_POS = {}
# ---------------- atlas
def pack(names, W=1024):
    x = y = rowh = 0; pos = {}
    for n in names:
        h, w = SPR[n].shape[:2]
        if x + w + 1 > W: x = 0; y += rowh + 1; rowh = 0
        pos[n] = (x, y, w, h); x += w + 1; rowh = max(rowh, h)
    H = y + rowh
    at = np.zeros((H, W, 4), np.uint8)
    for n, (px, py, w, h) in pos.items(): at[py:py + h, px:px + w] = SPR[n]
    return at, pos
def glyph_cs(name, g):
    lines = []
    for ch in sorted(g):
        key = {"'": "\\'", '\\': '\\\\'}.get(ch, ch)
        lines.append(f"            {{ '{key}', new[] {{ " + ', '.join('"%s"' % r for r in g[ch]) + " }, " + "}".replace(", }", " }") if False else f"            {{ '{key}', new[] {{ " + ', '.join('"%s"' % r for r in g[ch]) + " } },")
    return '\n'.join(lines)
def extra_glyphs():
    sm = dict(G.D2_SMALL); dp = dict(G.D2_DISP)
    sm['!'] = ['#', '#', '#', '#', '#', '.', '#']; dp['!'] = ['##', '##', '##', '##', '##', '..', '##', '##', '..'][:9]
    return sm, dp
def main():
    os.makedirs(OUT, exist_ok=True)
    cells(); buttons(); icons(); flames(); heap_and_bowl(); halo(); lights()
    names = sorted(SPR); at, pos = pack(names)
    Image.fromarray(at).save(OUT + 'mt_atlas.png')
    bg = Image.open(R6 + 'background/V6_lava_dimB_background_processed_640x360_AI_BG_MOCKUP_NOT_RUNTIME.png').convert('RGB'); bg.save(OUT + 'mt_bg.png')
    rgba, info = MS.build_rgba(); Image.fromarray(rgba).save(OUT + 'mt_scene.png')
    sm, dp = extra_glyphs()
    pal = ''.join(f"            {{ \"{k}\", new Color32({v[0]}, {v[1]}, {v[2]}, 255) }},\n" for k, v in PAL.items())
    at_cs = ''.join(f"            {{ \"{n}\", new[] {{ {x}, {y}, {w}, {h} }} }},\n" for n, (x, y, w, h) in pos.items())
    lp = ''.join(f"            {{ \"{n}\", new[] {{ {x}, {y} }} }},\n" for n, (x, y) in LIGHT_POS.items())
    code = f'''// GENERATED by tools/t1300/mt_export.py (t1300). Menu-theme Sins screen data: atlas rects, palette, hand-pixelled glyph sets (t1287 D2 Morta). Do not edit by hand.
using UnityEngine;
using System.Collections.Generic;
namespace Hellord
{{
    public static class MenuThemeData
    {{
        public const int FlameW = {FLAME_W * 2}, FlameH = {(FLAME_H + 2) * 2}, FlameFrames = {fire60.NF}, LightLevels = {len(LIGHT_F)};
        public static readonly int[] GridRect = {{ {info['gi'][0]}, {info['gi'][1]}, {info['gi'][2]}, {info['gi'][3]} }}, DetailRect = {{ {info['di'][0]}, {info['di'][1]}, {info['di'][2]}, {info['di'][3]} }}, Divider = {{ {info['divider'][0]}, {info['divider'][1]}, {info['divider'][2]} }};
        public static readonly Dictionary<string, Color32> Pal = new Dictionary<string, Color32> {{
{pal}        }};
        public static readonly Dictionary<string, int[]> Atlas = new Dictionary<string, int[]> {{
{at_cs}        }};
        public static readonly Dictionary<string, int[]> LightPos = new Dictionary<string, int[]> {{
{lp}        }};
        public static readonly Dictionary<char, string[]> Small = new Dictionary<char, string[]> {{
{glyph_cs('s', sm)}
        }};
        public static readonly Dictionary<char, string[]> Disp = new Dictionary<char, string[]> {{
{glyph_cs('d', dp)}
        }};
    }}
}}
'''
    open(CS, 'w').write(code)
    print('atlas', at.shape, len(names), 'sprites; scene', rgba.shape)
if __name__ == '__main__': main()
