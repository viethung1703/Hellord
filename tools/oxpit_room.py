# Render an Ox Pit sample room json exactly as the art handoff describes.
import json,sys
from PIL import Image
import numpy as np
F='/workspace/Hellord/Art/export/floor/'
_c={}
def sheet(n):
    if n not in _c: _c[n]=(Image.open(F+n+'.png').convert('RGBA'),json.load(open(F+n+'.json')))
    return _c[n]
def crop(e):
    n,_,k=e['ref'].partition(':'); im,j=sheet(n)
    if k: x,y,w,h=j['rects_xywh'][k]; return im.crop((x,y,x+w,y+h))
    fw,fh=j.get('frame_w',im.width),j.get('frame_h',im.height); f=e.get('frame',0)
    return im.crop((f*fw,0,f*fw+fw,fh))
def render(room,lights=True,props=True):
    d=json.load(open(F+room+'.json')); W,H=d['size_px']
    c=Image.new('RGBA',(W,H),(0,0,0,255))
    for e in d['entries']:
        if not props and 'note' in e: continue
        c.alpha_composite(crop(e),(e['x'],e['y']))
    a=np.asarray(c).astype(np.float32)[...,:3]
    if lights:
        for L in d['lights']:
            im,j=sheet(L['ref']); l=np.asarray(im).astype(np.float32)
            if L['anchor']=='centre': x0,y0=L['x']-j['pivot'][0],L['y']-j['pivot'][1]
            else: x0,y0=L['x'],L['y']
            h,w=l.shape[:2]; X0,Y0=max(0,x0),max(0,y0); X1,Y1=min(W,x0+w),min(H,y0+h)
            s=l[Y0-y0:Y1-y0,X0-x0:X1-x0]; al=s[...,3:]/255*L['strength']
            if L['blend']=='mul': a[Y0:Y1,X0:X1]*=1-al*0.55
            else: a[Y0:Y1,X0:X1]+=s[...,:3]*al
    return Image.fromarray(np.clip(np.round(a),0,255).astype(np.uint8)),d
if __name__=='__main__':
    for r in ['oxpit_room_clip','oxpit_room_lanes']:
        for lt in (True,False):
            im,d=render(r,lt); ref=np.asarray(Image.open(F+r+'_1536x864.png').convert('RGB')).astype(int)
            diff=np.abs(np.asarray(im).astype(int)-ref); print(r,'lights',lt,'maxdiff',diff.max(),'px>2',(diff.max(2)>2).sum())
