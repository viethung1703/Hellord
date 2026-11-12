# t503u: bake the VH-locked v69 main menu (step36/v69_pact_seal_hover_simple) into Resources/HellordArt/MainMenu69.
# Uses the v69 scripts unchanged: v49 title (title60_v49), fixed-right-fire bg (bg60/fire60), v49 skull+blood select rows (menu60 'S').
# Live data (currency, PACT n/6, socket fills, tooltip, Pact panel) is drawn by the game, so it is NOT baked here.
import sys,os,json,math
S='/workspace/Hellord/Art/previews/step36_main_menu/v69_pact_seal_hover_simple/scripts'; sys.path.insert(0,S)
OUT='/workspace/Hellord/Assets/Hellord/Resources/HellordArt/MainMenu69/'; os.makedirs(OUT,exist_ok=True)
G=dict(__file__=S+'/mm69_base.py',__name__='mm69'); exec(open(S+'/mm69_base.py').read(),G)
from PIL import Image,ImageDraw,ImageFilter,ImageChops,ImageEnhance
import menu60 as M
P,F,BG,T39,bg41,V8=G['P'],G['F'],G['BG'],G['T39'],G['bg41'],G['V8']
NL=48; sx0,sy0=549,96
# static UI without live text (copied from mm69_base.base_menu minus currency / PACT text / socket fills)
ui=Image.new('RGBA',(700,392),(0,0,0,0)); d=ImageDraw.Draw(ui); d.fontmode='1'
for x in range(300): d.line((x,0,x,392),fill=P('181425',int(200*(1-x/300)**1.5)))
glow=Image.new('RGBA',(700,392),(0,0,0,0)); g=ImageDraw.Draw(glow)
for r in range(110,0,-4): g.ellipse((sx0+64-r,sy0+70-r,sx0+64+r,sy0+70+r),fill=P('181425',int(150*(1-r/110)**0.6)))
ui.alpha_composite(glow); d=ImageDraw.Draw(ui); d.fontmode='1'
t='DAMNATION PACT'; w=d.textlength(t,font=F(11)); d.text((sx0+64-w/2+1,sy0-3),t,font=F(11),fill=P('181425')); d.text((sx0+64-w/2,sy0-4),t,font=F(11),fill=P('feae34'))
seal=Image.open(V8+'pixellab_raw/seal.png').convert('RGBA'); ui.alpha_composite(seal,(sx0,sy0+12)); d=ImageDraw.Draw(ui)
bx,by=sx0+64,sy0+12+109
d.polygon([(bx-30,by-8),(bx+30,by-8),(bx+36,by),(bx+30,by+8),(bx-30,by+8),(bx-36,by)],fill=P('181425'),outline=P('ead4aa'))
for t in range(NL):
    im=bg41.frame(BG,t,NL); im.alpha_composite(ui)
    L=T39.frame(t,NL,None); im.alpha_composite(L,(14-T39.PAD,26-T39.PAD)); T39.motes(ImageDraw.Draw(im),t,NL)
    im.convert('RGB').save(OUT+f'mm69_base_{t:02d}.png',optimize=True)
# menu rows (v49 'S' style) per selected item: atlas of NL cells
box=None
for sel in range(5):
    for t in (0,12,24,36):
        b=M.layer('S',t,NL,sel).getbbox(); box=b if box is None else (min(box[0],b[0]),min(box[1],b[1]),max(box[2],b[2]),max(box[3],b[3]))
box=(box[0]-2,box[1]-4,box[2]+2,box[3]+6); cw,ch=box[2]-box[0],box[3]-box[1]; cols=max(1,2040//cw); rows=-(-NL//cols)
for sel in range(5):
    A=Image.new('RGBA',(cols*cw,rows*ch),(0,0,0,0))
    for t in range(NL): A.alpha_composite(M.layer('S',t,NL,sel).crop(box),((t%cols)*cw,(t//cols)*ch))
    A.save(OUT+f'mm69_rows_{sel}.png',optimize=True)
rowy=[[y for (i,tx,m,x,y) in M.rows(sel)] for sel in range(5)]; rowh=[[m.size[1] for (i,tx,m,x,y) in M.rows(sel)] for sel in range(5)]; roww=[m.size[0] for (i,tx,m,x,y) in M.rows(0)]
# seal hover (clip69.hover): brightened seal silhouette + soft red halo outside it
SEALA=seal.getchannel('A').point(lambda v:255 if v>128 else 0)
SM=Image.new('L',(700,392),0); SM.paste(SEALA,(sx0,sy0+12)); ImageDraw.Draw(SM).rectangle((540,212,690,392),fill=0)  # t507u: register on the drawn seal (y108), keep the plate out
GL=ImageChops.subtract(SM.filter(ImageFilter.GaussianBlur(4)),SM); ImageDraw.Draw(GL).rectangle((0,212,700,392),fill=0)
SR=(525,96,700,215)
ref=bg41.frame(BG,0,NL); ref.alpha_composite(ui)
hi=ImageEnhance.Brightness(ref.convert('RGB')).enhance(1.19).convert('RGBA'); hi.putalpha(SM); hi.crop(SR).save(OUT+'mm69_seal_hi.png')
gl=Image.new('RGBA',(700,392),P('a22633')); gl.putalpha(GL.point(lambda v:int(v*0.75))); gl.crop(SR).save(OUT+'mm69_seal_glow.png')
# sockets (filled = mockup look; empty = dark) 24x24, centre (12,12)
def sock(on,icon):
    s=Image.new('RGBA',(24,24),(0,0,0,0)); d=ImageDraw.Draw(s); c=12
    d.ellipse((c-8,c-8,c+8,c+8),outline=P('e49b49',160))
    if on:
        d.ellipse((c-6,c-6,c+6,c+6),fill=P('a22633'),outline=P('ff0049'))
        if icon==0: d.polygon([(c,c-5),(c+3,c),(c+3,c+2),(c+1,c+4),(c-1,c+4),(c-3,c+2),(c-3,c)],fill=P('feae34'))
        else: d.polygon([(c-5,c+3),(c-5,c-3),(c-2,c),(c,c-4),(c+2,c),(c+5,c-3),(c+5,c+3)],fill=P('feae34'))
    else: d.ellipse((c-6,c-6,c+6,c+6),fill=P('181425'),outline=P('733e39'))
    return s
for i in range(2): sock(True,i).save(OUT+f'mm69_sock_on_{i}.png')
sock(False,0).save(OUT+'mm69_sock_off.png')
json.dump(dict(source='step36_main_menu/v69_pact_seal_hover_simple (VH locked)',frames=NL,frame_ms=90,canvas=[700,392],rows_box=box,rows_cell=[cw,ch],rows_cols=cols,
  row_y=rowy,row_h=rowh,row_w=roww,row_x=M.X0,seal_region=SR,seal_hit=[549,108,128,140],sockets=[[578,135],[648,135]],seal_centre=[613,172],plate_centre=[bx,by]),open(OUT+'mm69.json','w'),indent=1)
print(box,cw,ch,cols,rows,rowy,rowh)
