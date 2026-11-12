# t511: bake Sacrifice Altar v97 art (PixelLab portraits + node icons) into Resources/HellordArt/Altar97
from PIL import Image
import os
P='/workspace/Hellord/Art/previews/step36_main_menu/'
OUT='/workspace/Hellord/Assets/Hellord/Resources/HellordArt/Altar97/'
os.makedirs(OUT,exist_ok=True)
pre=['pyro','bone','storm','blood']
names=[['KINDLING','CINDER SPLIT','EMBER ORBIT','OVERHEAT','METEOR PACT','SCORCHED TRAIL','HELLFIRE LANCE','PYRE FEAST','ASH COVENANT','INFERNO HEART'],
 None,None,None]
import re
src=open('/workspace/Hellord/Assets/Hellord/Scripts/Runtime/HellordPact.cs').read()
rows=re.findall(r'new\[\] \{(.*?)\},?\s*\n\s*(?=new\[\]|\};)',src,re.S)
allnames=re.findall(r'N\("(\w+)","([^"]+)"',src)
assert len(allnames)>=40, len(allnames)
for p in range(4):
    for i in range(10):
        nid,nm=allnames[p*10+i]
        key=nm.replace(' ','_').replace('.','')
        f=P+'v97_sacrifice_all4_paths_review/icons/%s_%s.png'%(pre[p],key)
        # weak-spot fixes: better candidates from v94 sheet
        if (p,key)==(2,'THUNDER_STEP'): f=P+'v94_sacrifice_nodes_all4_paths/candidates/storm_THUNDER_STEP_9.png'
        if (p,key)==(3,'WHIP_REACH'): f=P+'v94_sacrifice_nodes_all4_paths/candidates/blood_WHIP_REACH_4.png'
        im=Image.open(f).convert('RGBA')
        if im.size!=(48,48): im=im.resize((48,48),Image.NEAREST)
        im.save(OUT+'icon_%d_%d.png'%(p,i))
        # dim variant for locked nodes
        px=im.load(); d=im.copy(); dp=d.load()
        for y in range(48):
            for x in range(48):
                r,g,b,a=px[x,y]; l=int((r*.3+g*.59+b*.11)*.45)
                dp[x,y]=(l+20,l+22,l+34,a)
        d.save(OUT+'icon_%d_%d_dim.png'%(p,i))
        print(p,i,nid,nm,os.path.basename(f))
av={0:'v86_sacrifice_carousel_evil_avatars/avatars/pyro_pixellab_64.png',1:'v86_sacrifice_carousel_evil_avatars/avatars/bone_pixellab_64.png',
    2:'v86_sacrifice_carousel_evil_avatars/pixellab_raw/storm_9.png',3:'v86_sacrifice_carousel_evil_avatars/avatars/blood_pixellab_64.png'}
for p,f in av.items():
    im=Image.open(P+f).convert('RGBA')
    if im.size!=(64,64): im=im.resize((64,64),Image.NEAREST)
    a=im.split()[3].point(lambda v:255 if v>127 else 0); im.putalpha(a)
    im.save(OUT+'avatar_%d.png'%p)
    px=im.load(); d=im.copy(); dp=d.load()
    for y in range(64):
        for x in range(64):
            r,g,b,al=px[x,y]; l=int((r*.3+g*.59+b*.11)*.55)
            dp[x,y]=(l+24,l+30,l+48,al)
    d.save(OUT+'avatar_%d_lock.png'%p)

# t525u: Necromancer (path 4) PLACEHOLDER art = Bone Warden art tinted grave-green. Replace with PixelLab art once VH picks.
from PIL import ImageOps
def green(im):
    r,g,b,a=im.split(); l=ImageOps.grayscale(im.convert('RGB'))
    return Image.merge('RGBA',(l.point(lambda v:int(v*.55)),l.point(lambda v:min(255,int(v*1.05+18))),l.point(lambda v:int(v*.5)),a))
# t536u: 18 nodes. 0..8 group A SUMMON = green-tinted Bone icons, 9..17 group B BONE SKILLS = untinted Bone icons. PLACEHOLDER (not HA v111 art).
# t544: Necromancer node icons = v112 PixelLab placeholders (VH: placeholders OK). idx 0..6 A1..A7, 7 AK, 8 AK2, 9..15 B1..B7, 16 BK, 17 BK2.
import glob as _g
NI='/workspace/Hellord/Art/previews/step36_main_menu/v112_necromancer_node_icons/icons/'
for i,k in enumerate(['A1','A2','A3','A4','A5','A6','A7','AK','AK2','B1','B2','B3','B4','B5','B6','B7','BK','BK2']):
    im=[Image.open(x).convert('RGBA') for x in _g.glob(NI+k+'_*_48.png')][0]
    if im.size!=(48,48): im=im.resize((48,48),Image.NEAREST)
    im.save(OUT+'icon_4_%d.png'%i); px=im.load(); d=im.copy(); dp=d.load()
    for y in range(48):
        for x in range(48):
            r,g,b,a=px[x,y]; l=int((r*.3+g*.59+b*.11)*.45); dp[x,y]=(l+20,l+22,l+34,a)
    d.save(OUT+'icon_4_%d_dim.png'%i)
green(Image.open(OUT+'avatar_1.png').convert('RGBA')).save(OUT+'avatar_4.png')
Image.open(OUT+'avatar_1_lock.png').save(OUT+'avatar_4_lock.png')

# t526: VH-approved v107 Bone Warden whole-bust portrait (static base + 8-frame PixelLab idle, jaw locked shut)
V='/workspace/Hellord/Art/previews/step36_main_menu/v107_bone_warden_whole_bust_idle/'
b=Image.open(V+'base/bone_body_closed_64.png').convert('RGBA'); b.save(OUT+'avatar_1.png')
px=b.load(); d=b.copy(); dp=d.load()
for y in range(64):
    for x in range(64):
        r,g,bb,al=px[x,y]; l=int((r*.3+g*.59+bb*.11)*.55); dp[x,y]=(l+24,l+30,l+48,al)
d.save(OUT+'avatar_1_lock.png')
st=Image.open(V+'portrait_anim/bone_idle_strip_64.png').convert('RGBA')
for f in range(st.width//64): st.crop((f*64,0,f*64+64,64)).save(OUT+'avatar_1_idle%d.png'%f)

# t527: Pyro/Storm/Blood portraits = v102 closed-mouth bases + v103 idle strips (6 frames). Bone stays v107 (above).
V2='/workspace/Hellord/Art/previews/step36_main_menu/v102_portrait_closed_mouth_anim/base/'
V3='/workspace/Hellord/Art/previews/step36_main_menu/v103_portrait_idle_only_select/portrait_anim/'
for p,k in [(0,'pyro'),(2,'storm'),(3,'blood')]:
    b=Image.open(V2+'%s_closed_64.png'%k).convert('RGBA'); b.save(OUT+'avatar_%d.png'%p)
    px=b.load(); d=b.copy(); dp=d.load()
    for y in range(64):
        for x in range(64):
            r,g,bb,al=px[x,y]; l=int((r*.3+g*.59+bb*.11)*.55); dp[x,y]=(l+24,l+30,l+48,al)
    d.save(OUT+'avatar_%d_lock.png'%p)
    st=Image.open(V3+'%s_idle_strip_64.png'%k).convert('RGBA')
    for f in range(st.width//64): st.crop((f*64,0,f*64+64,64)).save(OUT+'avatar_%d_idle%d.png'%(p,f))
