"""t1259 v0.0.99: export of the approved t1246b pass3e Sins scene (cellstone) for Unity. Everything from the SAME objects that rendered the mockup."""
import sys, os, json, inspect, re
os.environ.update(TAKE='3plus', CELLSTONE='1', BACK_INSET='8', OUTNAME='cellstone_pass3e')
sys.path.insert(0, '.')
import build_t1246b as B
from build_t1246b import *
import scene_d19 as SCN, d19 as HP, fnt as FN, font5 as F5, fx84, recolor as RC
import numpy as np
from PIL import Image
OUT = '/workspace/t1246x/out/'
ALLOW = [False]
# ---------- patches: static base = the mockup scene without any dynamic content
_text0 = SCN.S4.text
def text_p(self, s, x, y, role='BONE', **k):
    if ALLOW[0]: return _text0(self, s, x, y, role, **k)
    self.texts.append((s, (x, y, 0, 7), 0.0))
SCN.S4.text = text_p
SCN.S4.icon = lambda self, cx, cy, i, dim=False: self.icon_log.append((cx - 10, cy - 10, i)) if hasattr(self, 'icon_log') else None
_put0 = SCN.S4.put
SKIP = ('button_', 'tab_', 'cell_')
def put_p(self, part, x, y, w=None, h=None, name=None, kind='part', parent=None):
    if (part.startswith(SKIP) or part == 'well') and w is not None:
        if name: self.rect(name, kind, x, y, w, h, parent, 1)
        return (w, h)
    return _put0(self, part, x, y, w, h, name, kind, parent)
SCN.S4.put = put_p
lt0 = SCN.lintel_title
def lintel_p(S):
    ALLOW[0] = True
    try: return lt0(S)
    finally: ALLOW[0] = False
SCN.lintel_title = lintel_p
src = inspect.getsource(SCN.sins4)
def rep(a, b):
    global src
    assert a in src, a; src = src.replace(a, b)
rep("S.c.alpha_composite(S.parts['lock_glyph'][0], (SX[2] + 16 - 4, ys + 16 - 5))", "pass")
rep("S.c.alpha_composite(ic, (cx, cy)); S.text('1200'", "S.text('1200'")
rep("S.c.alpha_composite(ic2, (x2, cy)); S.text('10'", "S.text('10'")
rep("for nm_ in ('souls', 'sigils', 'equip', 'prev', 'next', 'back', 'pager', 'tab0', 'tab1', 'tab2', 'tab3'):", "for nm_ in ('souls', 'sigils'):")
ns = dict(SCN.__dict__); exec(src, ns); sins4_base = ns['sins4']
# the original (reference) builders for the verification
def build_base(sel=0):
    SCN.SEL_TAB[0] = sel; SCN.NOLABEL[0] = True; SCN.TABS_ON[0] = False; SCN.GLOW_ON[0] = False
    fx84.LIGHT_TINT = B.NEUT_TINT; RC.VARIANT[0] = 'B'; parts = B.make_parts()
    S, mir = sins4_base(parts, red=True); return S, parts
if __name__ == '__main__':
    S, parts = build_base(0)
    base = S.c.convert('RGB'); base.save(OUT + 'ui_t1246_base.png'); print('base', base.size)
