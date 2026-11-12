import sys; sys.dont_write_bytecode = True
sys.path.insert(0,'/workspace/Hellord/tools/t1300')
import numpy as np
from PIL import Image
OUT='/workspace/Hellord/Assets/Hellord/Resources/HellordArt/MenuTheme/'
import re
cs=open('/workspace/Hellord/Assets/Hellord/Scripts/Runtime/HellordMenuThemeData.cs').read()
atl={m.group(1):list(map(int,m.group(2).split(','))) for m in re.finditer(r'\{ "(\w+)", new\[\] \{ (\d+, \d+, \d+, \d+) \} \}',cs)}
AT=Image.open(OUT+'mt_atlas.png').convert('RGBA'); bg=Image.open(OUT+'mt_bg.png').convert('RGBA'); sc=Image.open(OUT+'mt_scene.png').convert('RGBA')
def spr(n): x,y,w,h=atl[n]; return AT.crop((x,y,x+w,y+h))
def frame(f, lvl=2):
    im=bg.copy(); im.alpha_composite(sc)
    for side,cx in (('L',21),('R',619)):
        x,y=atl['light%s%d'%(side,lvl)][:2]
        # light pos entries are in LightPos (second regex group has 2 ints): handle
    return im
LP={m.group(1):list(map(int,m.group(2).split(','))) for m in re.finditer(r'\{ "(light\w+)", new\[\] \{ (\d+), (\d+) \} \}',cs)}
LP={}
for m in re.finditer(r'\{ "(light\w+)", new\[\] \{ (\d+), (\d+) \} \}',cs): LP[m.group(1)]=(int(m.group(2)),int(m.group(3)))
def frame(f, lvl=2, ph=(0,5)):
    im=bg.copy(); im.alpha_composite(sc)
    for side,cx,off in (('L',21,ph[0]),('R',619,ph[1])):
        im.alpha_composite(spr('light%s%d'%(side,lvl)),LP['light%s%d'%(side,lvl)])
        rim=214; fl=spr('flame%s%02d'%(side,(f+off)%16))
        im.alpha_composite(fl,(cx-13,rim+1-fl.height))
        im.alpha_composite(spr('coal%d'%(f%3)),(cx-13,rim-3)); im.alpha_composite(spr('bowl'),(cx-16,rim))
    return im
if __name__=='__main__':
    fs=[frame(f).crop((0,150,48,250)).resize((48*6,100*6),Image.NEAREST) for f in range(0,16,2)]
    W=sum(i.width for i in fs[:4]); sheet=Image.new('RGBA',(W,fs[0].height*2))
    for k,i in enumerate(fs): sheet.paste(i,((k%4)*i.width,(k//4)*i.height))
    sheet.convert('RGB').save('/tmp/fire_sheet.png'); frame(0).convert('RGB').save('/tmp/mt_frame0.png'); print('ok')
