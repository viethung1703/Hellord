# t1267: compose the A|B|C talisman comparison (both talismans at 5x, 30 fps) from the tiny native crops, plus an 8x still. usage: t1267_board.py <capdir> <outdir>
import sys,glob,os
import numpy as np
from PIL import Image,ImageDraw,ImageFont
cap,out=sys.argv[1],sys.argv[2];os.makedirs(out+'/f',exist_ok=True)
Z=5;font=ImageFont.truetype('/usr/share/fonts/truetype/dejavu/DejaVuSansMono-Bold.ttf',15);small=ImageFont.truetype('/usr/share/fonts/truetype/dejavu/DejaVuSansMono.ttf',12)
names=['A  PENDULUM + REST','B  STILL + HEM GUSTS','C  COMBINED']
N=len(glob.glob(cap+'/m1/*.png'));cw,ch=72*Z,50*Z;gap=14;W=3*cw+4*gap;H=ch+70
for i in range(N):
    im=Image.new('RGB',(W,H),(14,12,20));d=ImageDraw.Draw(im)
    d.text((gap,6),'IN-GAME CAPTURE (preview, not packaged)   t=%5.2fs  tal L / tal R: different seeds'%(40+i/30.0),font=small,fill=(200,200,210))
    for m in range(3):
        c=Image.open(f'{cap}/m{m+1}/{i:04d}.png').convert('RGB').resize((cw,ch),Image.NEAREST);x=gap+m*(cw+gap);im.paste(c,(x,52))
        d.text((x,26),names[m],font=font,fill=(254,174,52))
    im.save(f'{out}/f/{i:04d}.png')
# 8x still: option C, both talismans, at the frame of strongest hem lift
