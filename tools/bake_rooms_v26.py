# v0.0.26: fight rooms in several sizes + props moved OUT of the baked floor.
# - Same offline stitch as bake_rooms_v08.py (cut the exported 1536x864 sample room at a 16px seam, close the gap), now for six sizes x two source rooms.
# - Props (oxpit_prop_* except braziers, oxpit_altar_*) are NOT baked into the floor any more. They go into <name>_data.json as
#   props:[{s,x,y,ax,ay,ox,oy,k}] and the game draws them as y-sorted sprites (sort by pivot y, HA's pivot_note), so the hero and
#   demons can walk in front of and behind them. A soft banded contact shadow (code-drawn, palette #181425) is baked into the floor
#   under every prop and brazier, so nothing floats.
# - k = light factor sampled from the lit preview (shadow pools darken props in the runtime too); the game also applies the floor grade.
import json,shutil,sys,os,numpy as np
from PIL import Image,ImageDraw
src=open('/workspace/Hellord/tools/bake_rooms_v08.py').read().split("stitch('oxpit_room_lanes',768,432,'v08_start')")[0]
exec(src)   # sheet(), crop(), light_tl(), fill_holes(), copy, E, OUT, R ... (v08 helpers; module-level bakes are cut off)
PROP_PREFIXES=('oxpit_prop_','oxpit_altar_')
INK=(24,20,37)  # HInk #181425
def is_prop(ref): return ref.startswith(PROP_PREFIXES) and not ref.startswith('oxpit_prop_brazier')
def shadow(c,cx,cy,w,h,rx_k=0.42):
    """two-band hard-edged ellipse (pixel-art), centred on the contact point"""
    rx=max(5,int(round(w*rx_k))); 
    if h>w*1.5: rx=max(4,int(round(w*0.36)))
    ry=max(2,int(round(rx*0.32)))
    lay=Image.new('RGBA',c.size,(0,0,0,0)); d=ImageDraw.Draw(lay)
    d.ellipse((cx-rx,cy-ry+1,cx+rx,cy+ry+1),fill=INK+(92,))
    ix,iy=max(2,int(rx*0.68)),max(1,int(ry*0.62)); d.ellipse((cx-ix,cy-iy+1,cx+ix,cy+iy+1),fill=INK+(150,))
    c.alpha_composite(lay)
def bake26(name,W,H,entries,lights):
    c=Image.new('RGBA',(W,H),(0,0,0,255)); braz=[]; props=[]
    for e in entries:
        if e['ref'].startswith('oxpit_prop_brazier'): braz.append(dict(x=e['x'],y=e['y'],f=e.get('frame',0))); continue
        if is_prop(e['ref']): props.append(e); continue
        c.alpha_composite(crop(e),(e['x'],e['y']))
    c=fill_holes(c)
    # contact shadows (brazier foot = bottom centre of the 26x38 frame)
    for b in braz: shadow(c,b['x']+13,b['y']+35,26,38,0.40)
    pj=[]
    for e in sorted(props,key=lambda e:e['y']):
        im=crop(e); n=e['ref']; _,j=sheet(n); ox,oy=j.get('pivot',[im.width//2,im.height-2]); ax,ay=e['x']+ox,e['y']+oy
        shadow(c,ax,ay,im.width,im.height)
        pj.append(dict(s=n,x=e['x'],y=e['y'],ax=ax,ay=ay,ox=ox,oy=oy,k=1.0)); copy.add(n)
    c.save(OUT+name+'_base.png'); copy.add('oxpit_prop_brazier')
    Ls=[]
    for L in lights:
        x0,y0=light_tl(L); k=L['strength']; mul=L['blend']=='mul'
        Ls.append(dict(s=L['ref'],x=int(x0),y=int(y0),mul=1 if mul else 0,k=round(k,4)))
    gates=[dict(x=e['x'],open=1 if e['ref'].endswith('gate_open') else 0) for e in entries if e['ref'].startswith('oxpit_wall_gate_frame:gate_') and e['y']==0]
    dx=W//2
    for off in [0,64,-64,128,-128,192,-192]:
        x=W//2+off
        if all(x+16<=g['x'] or x-16>=g['x']+48 for g in gates): dx=x; break
    # lit preview without props (to sample the light factor for each prop), then the full preview
    def lit(img):
        a=np.asarray(img).astype(np.float32)[...,:3].copy()
        for L in Ls:
            l=np.asarray(sheet(L['s'])[0]).astype(np.float32); h,w=l.shape[:2]; x0,y0=L['x'],L['y']
            X0,Y0=max(0,x0),max(0,y0); X1,Y1=min(W,x0+w),min(H,y0+h)
            if X1<=X0 or Y1<=Y0: continue
            s=l[Y0-y0:Y1-y0,X0-x0:X1-x0]; al=s[...,3:]/255*L['k']
            if L['mul']: a[Y0:Y1,X0:X1]*=1-np.minimum(al*0.55,1)
            else: a[Y0:Y1,X0:X1]+=s[...,:3]*al
        return a
    base=c.copy()
    for b in braz: base.alpha_composite(crop(dict(ref='oxpit_prop_brazier',frame=b['f'])),(b['x'],b['y']))
    lo=lit(base); un=np.asarray(base).astype(np.float32)[...,:3]
    for p in pj:
        x0,y0=max(0,p['ax']-14),max(0,p['ay']-8); x1,y1=min(W,p['ax']+14),min(H,p['ay']+6)
        u=un[y0:y1,x0:x1].sum(); l=lo[y0:y1,x0:x1].sum(); p['k']=round(float(np.clip(l/max(u,1),0.45,1.0)),3)
    d=dict(w=W,h=H,wallN=48,wallS=16,wallSide=24,braziers=braz,lights=Ls,gates=gates,doorX=dx,doorY=(H+48-16)//2,props=pj)
    json.dump(d,open(OUT+name+'_data.json','w'),indent=0)
    prev=base.copy()
    for p in pj:
        e=dict(ref=p['s']); prev.alpha_composite(crop(e),(p['x'],p['y']))
    a=lit(prev); Image.fromarray(np.clip(np.round(a),0,255).astype(np.uint8)).save('/workspace/Hellord/tools/_preview_'+name+'.png')
    print(name,W,H,len(pj),'props',len(braz),'braz',len(Ls),'lights door',dx)
def stitch26(srcn,W,H,name):
    d=json.load(open(E+'floor/'+srcn+'.json')); SW,SHh=d['size_px']; sx,sy=SW-W,SHh-H; mx,my=(W//2)//16*16,(H//2)//16*16
    assert sx%16==0 and sy%16==0,(W,H)
    def strad(m,s,ax):
        n=0
        for e in d['entries']:
            v=e[ax]; w=crop(e).width if ax=='x' else crop(e).height
            if not (v+w<=m or v>=m+s): n+= 1000 if (ax=='x' and e['y']<48) or (ax=='y' and e['x']<24) else 1
        return n
    if sx: mx=min(range(mx-160,mx+161,16),key=lambda m:(strad(m,sx,'x'),abs(m-W//2)))
    if sy: my=min(range(my-96,my+97,16),key=lambda m:(strad(m,sy,'y'),abs(m-H//2)))
    def keep(x,w,m,s):
        if x+w<=m: return x
        if x>=m+s: return x-s
        return None
    out=[]
    for e in d['entries']:
        if e['ref'].startswith('oxpit_wall_gate_frame:'): continue
        im=crop(e); x=keep(e['x'],im.width,mx,sx); y=keep(e['y'],im.height,my,sy)
        if x is None or y is None: continue
        out.append(dict(e,x=x,y=y))
    Ls=[]
    for L in d['lights']:
        if L['ref']=='oxpit_gate_open_glow': continue
        cx,cy=L['x'],L['y']
        x=cx if cx<mx else (cx-sx if cx>=mx+sx else None); y=cy if cy<my else (cy-sy if cy>=my+sy else None)
        if x is None or y is None: continue
        Ls.append(dict(L,x=x,y=y))
    return bake26(name,W,H,out,Ls)
SIZES=[(640,368),(768,432),(896,512),(1024,576),(1152,656),(1280,736)]
for (W,H) in SIZES:
    for s,tag in (('oxpit_room_lanes','a'),('oxpit_room_clip','b')): stitch26(s,W,H,f'v26_{W}x{H}_{tag}')
# the boss room keeps the v0.7 1536x864 bake (oxpit_room_clip) - re-baked without baked props so it matches the rest
_d=json.load(open(E+'floor/oxpit_room_clip.json')); bake26('v26_boss',1536,864,_d['entries'],_d['lights']); copy.add('oxpit_gate_open_glow')
for n in copy:
    for dd in ['floor/','rooms/']:
        if os.path.exists(E+dd+n+'.png'): shutil.copy(E+dd+n+'.png',OUT+n+'.png')
print(sorted(copy))
