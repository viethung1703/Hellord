# v0.8: bake every Ox Pit floor room used by the prototype into <name>_base.png + <name>_data.json (Resources/HellordArt).
# - oxpit_* fight rooms: re-arranged from the exported sample-room json (floor/oxpit_room_lanes|clip.json) by cutting the
#   tile grid at a 16px seam and closing the gap (entries + lights shifted, entries straddling the seam dropped). Only exported tiles/props.
# - rooms_shrine_672x384 / rooms_secret_640x360: from export/rooms json (interactive chests are left out and placed at runtime).
import json,shutil,sys,os,numpy as np
from PIL import Image
sys.path.insert(0,'/workspace/Hellord/tools'); import oxpit_room as R
E='/workspace/Hellord/Art/export/'; OUT='/workspace/Hellord/Assets/Hellord/Resources/HellordArt/'
SH={}
def sheet(n):
    if n in SH: return SH[n]
    for d in ['floor/','rooms/','ui/','hazards/']:
        if os.path.exists(E+d+n+'.json'):
            SH[n]=(Image.open(E+d+n+'.png').convert('RGBA'),json.load(open(E+d+n+'.json'))); return SH[n]
    raise KeyError(n)
def crop(e):
    n,_,k=e['ref'].partition(':'); im,j=sheet(n)
    if k: x,y,w,h=j['rects_xywh'][k]; return im.crop((x,y,x+w,y+h))
    fw,fh=j.get('frame_w',im.width),j.get('frame_h',im.height); f=e.get('frame',0)
    return im.crop((f*fw,0,f*fw+fw,fh))
copy=set()
def light_tl(L):
    im,j=sheet(L['ref']); copy.add(L['ref'])
    if L['anchor']=='centre': return L['x']-im.width//2 if 'pivot' not in j else L['x']-j['pivot'][0], L['y']-im.height//2 if 'pivot' not in j else L['y']-j['pivot'][1]
    return L['x'],L['y']
def fill_holes(c):
    a=np.array(c); H,W=a.shape[:2]
    for _ in range(8):
        m=(a[...,:3].sum(-1)==0)
        if not m.any(): break
        ys,xs=np.nonzero(m)
        for y,x in zip(ys,xs):
            sx=x-16 if x>=16 else x+16
            a[y,x]=a[y,sx]
    return Image.fromarray(a)
def bake(name,W,H,entries,lights,extra,mulscale):
    c=Image.new('RGBA',(W,H),(0,0,0,255)); braz=[]
    for e in entries:
        if e['ref'].startswith('oxpit_prop_brazier'): braz.append(dict(x=e['x'],y=e['y'],f=e.get('frame',0))); continue
        c.alpha_composite(crop(e),(e['x'],e['y']))
    c=fill_holes(c)  # v0.0.9: the seam cut can drop a 48px wall piece -> black 16px column; copy the tile column on its left
    c.save(OUT+name+'_base.png'); copy.add('oxpit_prop_brazier')
    Ls=[]
    for L in lights:
        x0,y0=light_tl(L); k=L['strength']/mulscale
        mul=L['blend']=='mul'
        if mul and mulscale==100: k=k/0.55   # rooms/ json: out = base*(1-a); runtime shader multiplies by 0.55
        Ls.append(dict(s=L['ref'],x=int(x0),y=int(y0),mul=1 if mul else 0,k=round(k,4)))
    gates=[dict(x=e['x'],open=1 if e['ref'].endswith('gate_open') else 0) for e in entries if e['ref'].startswith('oxpit_wall_gate_frame:gate_') and e['y']==0]
    # north/south door x: centre, moved off any gate frame (48px) so the 32px door never sits on a gate
    dx=W//2
    for off in [0,64,-64,128,-128,192,-192]:
        x=W//2+off
        if all(x+16<=g['x'] or x-16>=g['x']+48 for g in gates): dx=x; break
    d=dict(w=W,h=H,wallN=48,wallS=16,wallSide=24,braziers=braz,lights=Ls,gates=gates,doorX=dx,doorY=(H+48-16)//2)
    d.update(extra); json.dump(d,open(OUT+name+'_data.json','w'),indent=0)
    # preview with braziers + lights (numpy, same math as runtime)
    for b in braz: c.alpha_composite(crop(dict(ref='oxpit_prop_brazier',frame=b['f'])),(b['x'],b['y']))
    a=np.asarray(c).astype(np.float32)[...,:3]
    for L in Ls:
        l=np.asarray(sheet(L['s'])[0]).astype(np.float32); h,w=l.shape[:2]; x0,y0=L['x'],L['y']
        X0,Y0=max(0,x0),max(0,y0); X1,Y1=min(W,x0+w),min(H,y0+h)
        if X1<=X0 or Y1<=Y0: continue
        s=l[Y0-y0:Y1-y0,X0-x0:X1-x0]; al=s[...,3:]/255*L['k']
        if L['mul']: a[Y0:Y1,X0:X1]*=1-np.minimum(al*0.55,1)
        else: a[Y0:Y1,X0:X1]+=s[...,:3]*al
    Image.fromarray(np.clip(np.round(a),0,255).astype(np.uint8)).save('/workspace/Hellord/tools/_preview_'+name+'.png')
    print(name,W,H,len(entries),'entries',len(braz),'braz',len(Ls),'lights door',dx)
    return d
def stitch(src,W,H,name):
    d=json.load(open(E+'floor/'+src+'.json')); SW,SHh=d['size_px']; sx,sy=SW-W,SHh-H; mx,my=(W//2)//16*16,(H//2)//16*16
    if H<SHh and (sy%16): raise SystemExit('bad H')
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
        # v0.0.9: wall gate frames read as exits; fight rooms only get the runtime door sprites where a neighbour exists
        if e['ref'].startswith('oxpit_wall_gate_frame:'): continue
        im=crop(e); x=keep(e['x'],im.width,mx,sx); y=keep(e['y'],im.height,my,sy)
        if x is None or y is None: continue
        out.append(dict(e,x=x,y=y))
    Ls=[]
    for L in d['lights']:
        if L['ref']=='oxpit_gate_open_glow': continue  # v0.0.9: glow belonged to the dropped gate frame
        cx,cy=L['x'],L['y']
        x=cx if cx<mx else (cx-sx if cx>=mx+sx else None); y=cy if cy<my else (cy-sy if cy>=my+sy else None)
        if x is None or y is None: continue
        Ls.append(dict(L,x=x,y=y))
    return bake(name,W,H,out,Ls,{},1.0)
stitch('oxpit_room_lanes',768,432,'v08_start')
stitch('oxpit_room_lanes',1152,656,'v08_survive')
stitch('oxpit_room_clip',768,432,'v08_hunt')
stitch('oxpit_room_lanes',1536,864,'v08_nests')
stitch('oxpit_room_clip',1152,656,'v08_curse')
stitch('oxpit_room_clip',768,432,'v08_treasure')
stitch('oxpit_room_lanes',1536,864,'v08_elite')
for n in ['oxpit_room_clip','oxpit_room_lanes']: pass  # boss room keeps v0.7's oxpit_room_clip bake
def special(fn,name,skip):
    d=json.load(open(E+'rooms/'+fn+'.json')); W,H=d['size_px']; ents=[]; chests=[]
    for e in d['floor']+d['decals']+d['walls']+d['props']:
        if skip(e,chests): continue
        ents.append(e)
    st=[p for p in d['props'] if p['ref']=='stairs']
    extra=dict(spawnX=d['spawn_hero'][0],spawnY=d['spawn_hero'][1],chests=chests)
    if 'shop_box_ui_at' in d: extra.update(shopX=d['shop_box_ui_at'][0],shopY=d['shop_box_ui_at'][1])
    if st: extra.update(stairsX=st[0]['x'],stairsY=st[0]['y'])
    sh=[p for p in d['props'] if p['ref']=='rooms_shrine_bone_large']
    if sh: extra.update(shrineX=sh[0]['x']+26,shrineY=sh[0]['y']+76)
    return bake(name,W,H,ents,d['lights'],extra,100)
def secret_skip(e,chests):
    if e['ref']=='rooms_chest_large' and e.get('frame',0)!=1:
        chests.append(dict(x=e['x']+15,y=e['y']+23,elite=1 if e.get('frame',0)==2 else 0)); return True
    return False
special('rooms_shrine_672x384','rooms_shrine_672x384',lambda e,c:False)
special('rooms_secret_640x360','rooms_secret_640x360',secret_skip)
for n in copy:
    for d in ['floor/','rooms/']:
        if os.path.exists(E+d+n+'.png'): shutil.copy(E+d+n+'.png',OUT+n+'.png')
U=E+'ui/'
for n in ['door','door_boss','stairs','wall_secret','shop_box','chest','altar_curse','fade_room_change','icons_room_8','levelup_card','levelup_level_dots','levelup_new_badge','icons_skill_16','nest','pause_box','pause_cursor','minimap_cells','minimap_icons_5','minimap_panel','minimap_corridor']:
    shutil.copy(U+n+'.png',OUT+'ui_'+n+'.png')
shutil.copy(E+'rooms/rooms_chest_large.png',OUT+'rooms_chest_large.png')
print(sorted(copy))
