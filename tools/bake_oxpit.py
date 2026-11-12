# v0.7: bake Ox Pit rooms for Unity. Base PNG = every entry except animated braziers (floor, decals, lanes,
# walls, AO, gate frames, static props in json order). Braziers + lights are runtime overlays (data json).
import json,shutil,sys,numpy as np
from PIL import Image
sys.path.insert(0,'/workspace/Hellord/tools'); import oxpit_room as R
F=R.F; OUT='/workspace/Hellord/Assets/Hellord/Resources/HellordArt/'
copy=set(['oxpit_prop_brazier','oxpit_gate_open_glow'])
for r in ['oxpit_room_lanes','oxpit_room_clip']:
    d=json.load(open(F+r+'.json')); W,H=d['size_px']
    c=Image.new('RGBA',(W,H),(0,0,0,255)); braz=[]
    for e in d['entries']:
        if e['ref'].startswith('oxpit_prop_brazier'):
            j=R.sheet('oxpit_prop_brazier')[1]; braz.append(dict(x=e['x'],y=e['y'],f=e.get('frame',0))); continue
        c.alpha_composite(R.crop(e),(e['x'],e['y']))
    c.save(OUT+r+'_base.png')
    lights=[]
    for L in d['lights']:
        im,j=R.sheet(L['ref']); copy.add(L['ref'])
        x0,y0=(L['x']-j['pivot'][0],L['y']-j['pivot'][1]) if L['anchor']=='centre' else (L['x'],L['y'])
        lights.append(dict(s=L['ref'],x=x0,y=y0,mul=1 if L['blend']=='mul' else 0,k=L['strength']))
    gates=[dict(x=e['x'],open=1 if e['ref'].endswith('gate_open') else 0) for e in d['entries'] if e['ref'].startswith('oxpit_wall_gate_frame:gate_') and e['y']==0]
    data=dict(w=W,h=H,wallN=48,wallS=16,wallSide=24,braziers=braz,lights=lights,gates=gates,clipCam=d.get('clip_camera_640x360'))
    json.dump(data,open(OUT+r+'_data.json','w'),indent=0)
    # verify base+braziers+lights == reference
    for b in braz: c.alpha_composite(R.crop(dict(ref='oxpit_prop_brazier',frame=b['f'])),(b['x'],b['y']))
    a=np.asarray(c).astype(np.float32)[...,:3]
    for L in lights:
        l=np.asarray(R.sheet(L['s'])[0]).astype(np.float32); h,w=l.shape[:2]; x0,y0=L['x'],L['y']
        X0,Y0=max(0,x0),max(0,y0); X1,Y1=min(W,x0+w),min(H,y0+h); s=l[Y0-y0:Y1-y0,X0-x0:X1-x0]; al=s[...,3:]/255*L['k']
        if L['mul']: a[Y0:Y1,X0:X1]*=1-al*0.55
        else: a[Y0:Y1,X0:X1]+=s[...,:3]*al
    ref=np.asarray(Image.open(F+r+'_1536x864.png').convert('RGB')).astype(int)
    df=np.abs(np.clip(np.round(a),0,255).astype(int)-ref); print(r,len(braz),'braziers',len(lights),'lights maxdiff',df.max(),'px>2',(df.max(2)>2).sum())
for n in copy: shutil.copy(F+n+'.png',OUT+n+'.png')
U='/workspace/Hellord/Art/export/ui/'
for n in ['hud_hp_widget','hud_hp_bar_fill','hud_xp_bar','hud_goal_box','minimap_panel','minimap_cells','minimap_icons_5','minimap_corridor','hud_counter_icons','hud_counter_bg','font_5x7']:
    shutil.copy(U+n+'.png',OUT+'ui_'+n+'.png')
shutil.copy('/workspace/Hellord/Art/export/'+'gate_bone_dropgate.png',OUT+'gate_bone_dropgate.png') if False else None
print(sorted(copy))
