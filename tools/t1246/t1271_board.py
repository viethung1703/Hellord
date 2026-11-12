# t1271: old (t1262) vs new embers, left brazier column captured at the REAL 4x output, 60 fps, shown 1:1. usage: t1271_board.py <capdir> <outdir>
import sys,glob,os
from PIL import Image,ImageDraw,ImageFont
cap,out=sys.argv[1],sys.argv[2];os.makedirs(out+'/f',exist_ok=True)
font=ImageFont.truetype('/usr/share/fonts/truetype/dejavu/DejaVuSansMono-Bold.ttf',13);small=ImageFont.truetype('/usr/share/fonts/truetype/dejavu/DejaVuSansMono.ttf',11)
N=len(glob.glob(cap+'/old/*.png'));w,h=Image.open(cap+'/old/0000.png').size;g=16;W=2*w+3*g;H=h+62
for i in range(N):
    im=Image.new('RGB',(W,H),(14,12,20));d=ImageDraw.Draw(im)
    d.text((g,5),'IN-GAME CAPTURE (preview, not packaged)',font=small,fill=(200,200,210));d.text((g,19),'real 4x, 60 fps  t=%5.2fs'%(40+i/60.0),font=small,fill=(200,200,210))
    for k,(v,n) in enumerate((('old','t1262 EMBERS'),('new','t1271 EMBERS'))):
        x=g+k*(w+g);d.text((x,36),n,font=font,fill=(254,174,52));im.paste(Image.open(f'{cap}/{v}/{i:04d}.png').convert('RGB'),(x,58))
    im.save(f'{out}/f/{i:04d}.png')
