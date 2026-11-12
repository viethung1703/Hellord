import sys, os
os.environ.update(TAKE='3plus', CELLSTONE='1', BACK_INSET='8', OUTNAME='cellstone_pass3e')
sys.path.insert(0,'.')
import build_t1246b as B
from build_t1246b import *
import numpy as np
parts, S, Sx, mir = B.scenes()
ref = np.array(Image.open('/workspace/Hellord/Art/previews/t1246b/pass3e/cellstone_pass3e/raw_native_nocaption.png').convert('RGB')).astype(int)
t=STILL_T; print('STILL_T',t)
r = np.array(to_img(SC.Dyn(S).render(t,480,270)).convert('RGB')).astype(int)
d=(abs(r-ref).sum(2)>0); print('diff px',d.sum())
