import export_t1246 as X
from export_t1246 import *
import scene_v6d as SC, comps_v6d as C
from scipy import ndimage as ndi
OUT = X.OUT
Sb, parts = X.build_base(0)
D = SC.Dyn(Sb)
def l2s(x):
    x = np.clip(np.asarray(x, float), 0, 1); return np.where(x <= 0.0031308, x * 12.92, 1.055 * x ** (1 / 2.4) - 0.055)
# ---------------- textures
stone = D.stone; cr = D.cracks.copy(); fc = D.fcracks
fm = (fc[..., 0] > 0) | (fc[..., 3] > 0); print('frame crack px', fm.sum(), 'overlap with pillar cracks', (fm & ((cr[...,0]>0)|(cr[...,3]>0))).sum(), 'overlap with stone', (fm & (stone>0.01)).sum())
cr = np.where(fm[..., None], fc, cr)
def enc8(a): return np.clip(np.round(a * 255), 0, 255).astype(np.uint8)
Image.fromarray(enc8(l2s(stone)), 'L').save(OUT + 'ui_t1246_stone_mask.png')
crt = np.zeros((270, 480, 4), np.uint8); crt[..., :3] = enc8(l2s(cr[..., :3])); crt[..., 3] = enc8(cr[..., 3]); Image.fromarray(crt, 'RGBA').save(OUT + 'ui_t1246_cracks.png')
# paper layer / ink (two talismans at (8,112), (444,112); both from the same 28x42 art)
pl = np.zeros((270, 480, 4), np.uint8); ik = np.zeros((270, 480), np.uint8)
for px in (C.LEFT_PX if hasattr(C, 'LEFT_PX') else 2, 438):
    x0 = px + 6; pl[112:154, x0:x0 + 28] = enc8(C.PAPER_LAYER); ik[112:154, x0:x0 + 28] = enc8(C.PAPER_INK)
Image.fromarray(pl, 'RGBA').save(OUT + 'ui_t1246_paper_layer.png'); Image.fromarray(ik, 'L').save(OUT + 'ui_t1246_ink_mask.png')
print('paper alpha unique', np.unique(pl[...,3])[:10], 'paper layer dtype', C.PAPER_LAYER.dtype, C.PAPER_LAYER[...,3].max())
# ---- round-trip check: decode textures like the GPU and compare to Dyn's float arrays
def s2l(x): return fx84.s2l(x)
st_back = s2l(np.array(Image.open(OUT + 'ui_t1246_stone_mask.png')).astype(float) / 255)
cr_back = np.array(Image.open(OUT + 'ui_t1246_cracks.png')).astype(float) / 255; cr_back = np.concatenate([s2l(cr_back[..., :3]), cr_back[..., 3:]], 2)
print('stone max err', abs(st_back - stone).max(), ' cracks max err', abs(cr_back - cr).max(), 'G err', abs(cr_back[...,1]-cr[...,1]).max(), 'B err', abs(cr_back[...,2]-cr[...,2]).max())
D2 = SC.Dyn(Sb); D2.stone = st_back; D2.cracks = cr_back; D2.fcracks = None
# python render with decoded textures (frame cracks merged into the cracks array, applied w/o light tint -> render_stage handles pillars via stone; frames via fcracks)
D2.fcracks = np.where(fm[..., None], cr_back, 0) 
D2.cracks = np.where(fm[..., None], 0, cr_back)
for t in (0.0, 7.3, 11.1):
    a = np.array(SC.to_img(D.render(t, 480, 270)).convert('RGB')).astype(int); b = np.array(SC.to_img(D2.render(t, 480, 270)).convert('RGB')).astype(int)
    d = abs(a - b).max(2); print('t', t, 'px differing', (d > 0).sum(), 'max', d.max(), 'px >2:', (d > 2).sum())
