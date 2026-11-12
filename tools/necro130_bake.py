# t493u: bake v130b Necromancer Altar static art (frame/header/sidebar/tree panel + rings, silent hub) into Altar97 resources.
import math, os, shutil, uuid
from PIL import Image, ImageDraw
OUT='/workspace/HellordProject/Assets/Hellord/Resources/HellordArt/Altar97/'
C=lambda h:tuple(int(h[i:i+2],16) for i in (0,2,4))
INK=C('181425');NAVY=C('262b44');LEA=C('3e2731');LEA2=C('733e39')
im=Image.new('RGB',(1280,720),INK); d=ImageDraw.Draw(im)
for y in range(0,720,8):
    for x in range((y//8)%2*16,1280,32): d.rectangle((x,y,x+29,y+5),fill=(28,24,42))
d.rectangle((8,8,1271,711),fill=LEA,outline=LEA2,width=4); d.rectangle((14,14,1265,705),outline=INK,width=2)
d.rectangle((18,18,1261,58),fill=NAVY,outline=INK,width=2)
d.rectangle((22,68,316,700),fill=(46,30,40),outline=INK,width=2)
d.rectangle((330,68,1265,700),fill=(52,33,44),outline=INK,width=2)
HX,HY=835,372
for r in (110,215,320,420): d.ellipse((HX-r,HY-r*0.62,HX+r,HY+r*0.62),outline=(62,40,52),width=2)
d.rectangle((330,68,1265,700),outline=INK,width=2)
im.resize((700,394),Image.LANCZOS).save(OUT+'necro130_bg.png')
def hub(lit):
    h=Image.new('RGBA',(100,100),(0,0,0,0)); d=ImageDraw.Draw(h); hx=hy=50
    K=(24,20,37);N2=(38,43,68);WN=(62,39,49);RB=(115,62,57);BN=(184,111,80);BO=(228,166,114);PU=(104,56,108);RD=(162,38,51);RL=(228,59,68)
    for i in range(12):
        a=math.radians(i*30+15); c,s_=math.cos(a),math.sin(a); L=45 if i%3==0 else 41
        pts=[(hx+c*33-s_*4,hy+s_*33+c*4),(hx+c*L,hy+s_*L),(hx+c*33+s_*4,hy+s_*33-c*4)]
        d.polygon([(round(x),round(y)) for x,y in pts],fill=BN if lit else (90,70,70),outline=K)
    d.ellipse((hx-36,hy-36,hx+36,hy+36),fill=K,outline=WN,width=2); d.ellipse((hx-31,hy-31,hx+31,hy+31),outline=RB if lit else (70,50,60),width=1)
    for i in range(16):
        a=math.radians(i*22.5); c,s_=math.cos(a),math.sin(a)
        d.line((round(hx+c*24),round(hy+s_*24),round(hx+c*29),round(hy+s_*29)),fill=(PU if lit else (70,50,80)),width=3 if i%4==0 else 1)
    d.ellipse((hx-22,hy-22,hx+22,hy+22),fill=N2,outline=PU,width=1)
    t=[(hx+round(20*math.cos(math.radians(90+120*i))),hy+round(20*math.sin(math.radians(90+120*i)))) for i in range(3)]; d.polygon(t,outline=WN)
    B=BO if lit else (150,130,120); u=2; ox,oy=hx-12,hy-13
    rows=["..XXXXXXXX..",".XXXXXXXXXX.","XXXXXXXXXXXX","XXXXXXXXXXXX","XEEEXXXXEEEX","XEEEXXXXEEEX","XXXXXNNXXXXX",".XXXXNNXXXX.","..XXXXXXXX..","..X.X..X.X..","..XXXXXXXX..","...X.X.X.X.."]
    for r,row in enumerate(rows):
        for q,ch in enumerate(row):
            if ch!='.': d.rectangle((ox+q*u,oy+r*u,ox+q*u+u-1,oy+r*u+u-1),fill={'X':B,'E':(RL if lit else RD),'N':K}[ch])
    return h.resize((55,55),Image.LANCZOS)
hub(True).save(OUT+'necro130_hub_on.png'); hub(False).save(OUT+'necro130_hub_off.png')
tmpl=open(OUT+'avatar_0.png.meta').read()
import re
for n in ('necro130_bg','necro130_hub_on','necro130_hub_off'):
    if not os.path.exists(OUT+n+'.png.meta'):
        open(OUT+n+'.png.meta','w').write(re.sub(r'guid: [0-9a-f]+','guid: '+uuid.uuid4().hex,tmpl,1))
print('ok')
