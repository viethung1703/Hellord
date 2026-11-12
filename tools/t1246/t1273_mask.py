# t1273: frame-stone mask for the brazier firelight on the MAIN outer frame: the grey ornament pixels of the left/right/bottom outer frame strips (navy ink, gold line, text, panels, cells: NOT in the mask)
from PIL import Image
import numpy as np,uuid
A='Assets/Hellord/Resources/HellordArt/'
b=np.array(Image.open(A+'ui_t1246_base.png').convert('RGB')).astype(int)
r,g,bl=b[...,0],b[...,1],b[...,2]
grey=((b.max(-1)-b.min(-1))<=12)&(r>=45)
reg=np.zeros(r.shape,bool)
reg[:,47:59]=True; reg[:,421:433]=True; reg[256:268,47:433]=True
m=grey&reg
# the chunky stone borders of the grid panel and the detail panel (ring of 8 px only: cells, text, buttons and icons are inside / dynamic and never in the mask); dark block outlines stay unlit
lum=(b*[.299,.587,.114]).sum(-1);sat=b.max(-1)-b.min(-1);stone=(lum>78)&(sat<=34)
ring=np.zeros(r.shape,bool)
for x0,x1,y0,y1 in ((64,288,75,229),(292,415,75,229)):
    ring[y0:y1+1,x0:x1+1]=True;ring[y0+8:y1-7,x0+8:x1-7]=False
m=m|(stone&ring)
Image.fromarray((m*255).astype(np.uint8),'L').save(A+'ui_t1273_frame_mask.png')
print('mask px',int(m.sum()))
meta=open(A+'ui_t1246_stone_mask.png.meta').read().replace('3567d78f2d281cbd72690c2c6dbf98b0',uuid.uuid4().hex)
open(A+'ui_t1273_frame_mask.png.meta','w').write(meta)
