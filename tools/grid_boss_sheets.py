"""v0.0.9: WebGL max texture size is 2048 on many GPUs/browsers. Re-pack horizontal strips wider than 2048 px into a
row-major grid (top row first) of at most 2048 px wide. HArt.Strip slices both layouts. Originals kept in tools/art_src/."""
import os, shutil, sys
from PIL import Image
ART = os.path.join(os.path.dirname(__file__), '..', 'Assets', 'Hellord', 'Resources', 'HellordArt')
SRC = os.path.join(os.path.dirname(__file__), 'art_src')
SHEETS = {'boss_oxhead_clubjailer': (72, 72, 80), 'boss_oxhead_clubjailer_bloodrage': (72, 72, 80)}
for name, (w, h, n) in SHEETS.items():
    src = os.path.join(SRC, name + '.png'); dst = os.path.join(ART, name + '.png')
    if not os.path.exists(src): shutil.copy(dst, src)
    im = Image.open(src).convert('RGBA'); assert im.size == (w * n, h), im.size
    cols = 2048 // w; rows = -(-n // cols)
    g = Image.new('RGBA', (cols * w, rows * h), (0, 0, 0, 0))
    for i in range(n): g.paste(im.crop((i * w, 0, i * w + w, h)), ((i % cols) * w, (i // cols) * h))
    g.save(dst); print(name, '->', g.size, f'{cols}x{rows}')
