import sys; sys.dont_write_bytecode = True
sys.path.insert(0,'/workspace/Hellord/tools/t1300'); import mt_static as M
import numpy as np, run5
from PIL import Image
from scipy import ndimage as ndi
R='/workspace/Hellord/Art/previews/t1287_redraw/D2_MORTA_v6/'
bg=np.array(Image.open(R+'background/V6_lava_dimB_background_processed_640x360_AI_BG_MOCKUP_NOT_RUNTIME.png').convert('RGB'))
cl=np.array(Image.open(R+'clean/V6_lava_dimB_screen_1x_640x360_clean_MOCKUP_NOT_RUNTIME.png').convert('RGB'))
rgba,info=M.build_rgba()
comp=np.where(rgba[...,3:4]>0,rgba[...,:3],bg)
# reference with no light
cv=run5.build(opt='b',bg=bg,info={},light_on=False); ref=cv.a
print('clean vs light-off ref differ px', int((cl!=ref).any(2).sum()))
d=(comp!=ref).any(2); lab,n=ndi.label(ndi.binary_dilation(d,iterations=3)); print('components',n,'diff px',int(d.sum()))
for i,sl in enumerate(ndi.find_objects(lab)): print(i,'x',sl[1].start,sl[1].stop,'y',sl[0].start,sl[0].stop,'px',int(d[sl].sum()))
