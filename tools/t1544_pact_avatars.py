#!/usr/bin/env python3
"""t1544: slice the t1540 god avatar strips (8 x 64x64, 180 ms) into Resources/HellordArt/Altar97/god_<g>{,_idle0..7,_lock}.png.
Static = <g>_static.png. Lock = the old avatar_*_lock luminance->slate map (L*0.539 + (24.7,30.7,48.7)), snapped to the old lock palette.
Metas are copies of avatar_0.png.meta (same importer settings) with a fresh GUID; existing metas are kept so GUIDs stay stable."""
import os, re, uuid, sys
import numpy as np
from PIL import Image
SRC = sys.argv[1] if len(sys.argv) > 1 else '/workspace/Hellord/Art/pact_avatars_t1540'
REPO = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
DST = os.path.join(REPO, 'Assets/Hellord/Resources/HellordArt/Altar97')
GODS = ['zhurong', 'gonggong', 'leizu', 'xingtian', 'zhongkui']
meta_tpl = open(os.path.join(DST, 'avatar_0.png.meta')).read()
pal = set()
for p in range(5):
    f = os.path.join(DST, 'avatar_%d_lock.png' % p)
    if os.path.exists(f):
        a = np.array(Image.open(f).convert('RGBA')).reshape(-1, 4); pal |= {tuple(c[:3]) for c in a if c[3] > 0}
pal = np.array(sorted(pal), float)
def lock(im):
    a = np.array(im.convert('RGBA')).astype(float); L = a[..., 0] * 0.299 + a[..., 1] * 0.587 + a[..., 2] * 0.114
    out = np.stack([L * 0.539 + 24.7, L * 0.539 + 30.7, L * 0.539 + 48.7], -1)
    if len(pal): d = ((out[..., None, :] - pal[None, None]) ** 2).sum(-1); out = pal[d.argmin(-1)]
    res = a.copy(); res[..., :3] = out; return Image.fromarray(np.clip(res, 0, 255).astype(np.uint8), 'RGBA')
def save(im, name):
    path = os.path.join(DST, name + '.png'); im.save(path)
    if not os.path.exists(path + '.meta'): open(path + '.meta', 'w').write(re.sub(r'guid: [0-9a-f]{32}', 'guid: ' + uuid.uuid4().hex, meta_tpl, 1))
for g in GODS:
    strip = Image.open(os.path.join(SRC, 'avatar_%s_64x64_8f.png' % g)).convert('RGBA'); assert strip.size == (512, 64)
    st = Image.open(os.path.join(SRC, 'avatar_%s_64x64_static.png' % g)).convert('RGBA')
    for k in range(8): save(strip.crop((k * 64, 0, k * 64 + 64, 64)), 'god_%s_idle%d' % (g, k))
    save(st, 'god_' + g); save(lock(st), 'god_%s_lock' % g)
print('ok', len(pal), 'lock colours')
