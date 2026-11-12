"""t1300 parity: sconce fire (runtime 1080p frames, 3x) vs main-menu torches (measure_fire.menu_stats). Run: python3 -B measure_parity.py <frames_dir> [n]"""
import sys, glob; sys.dont_write_bytecode = True
sys.path.insert(0, '/workspace/Hellord/Art/previews/t1287_redraw/work')
import numpy as np
from PIL import Image
import measure_fire as mf
d = sys.argv[1]; fr = sorted(glob.glob(d + '/*.png'))[::int(sys.argv[2]) if len(sys.argv) > 2 else 4]
for k, v in mf.menu_stats().items(): print(k, v)
for nm, box in (('sconce L', (24, 500, 102, 640)), ('sconce R', (1816, 500, 1894, 640))):
    st = []
    for p in fr:
        a = np.array(Image.open(p).convert('RGB'))
        try: st.append(mf.blob_stats(a, box))
        except Exception: pass
    if st: print(nm, 'frames', len(st), 'mean w %.0f h %.0f warm %.0f hot %.0f core %.0f' % tuple(np.mean([s[k] for s in st]) for k in ('w', 'h', 'warm_px', 'hot_px', 'core_px')), 'max h', max(s['h'] for s in st))
