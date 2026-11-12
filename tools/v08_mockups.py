import re
# Builds/v08_preview/*.png: labelled MOCK-UP composites from exported art only (not Unity screenshots).
from PIL import Image
R='/workspace/Hellord/Assets/Hellord/Resources/HellordArt/'; O='/workspace/Hellord/Builds/v08_preview/'
L=lambda n: Image.open(R+n+'.png').convert('RGBA')
FONT=L('ui_font_5x7'); MAP="0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ:/+-%!.x'? "
def tint(im,c):
    t=Image.new('RGBA',im.size,c+(255,)); t.putalpha(im.getchannel('A')); return t
def text(img,s,x,y,c=(255,255,255),sc=1):
    for p,col in ((1,(0x18,0x14,0x25)),(0,c)):
        for i,ch in enumerate(s.upper()):
            if ch==' ': continue
            k=MAP.find(ch); k=MAP.find('?') if k<0 else k
            g=tint(FONT.crop((k*5,0,k*5+5,7)),col)
            if sc>1: g=g.resize((5*sc,7*sc),Image.NEAREST)
            img.alpha_composite(g,(x+i*6*sc,y+p*sc))
def tw(s): return len(s)*6-1
def frame(n,i,w,h): return L(n).crop((i*w,0,i*w+w,h))
def nine(img,src,fx,fw,fh,l,r,t,b,x,y,w,h):
    s=src.crop((fx,0,fx+fw,fh))
    cols=[(0,l,0,l),(l,fw-r,l,w-r),(fw-r,fw,w-r,w)]; rows=[(0,t,0,t),(t,fh-b,t,h-b),(fh-b,fh,h-b,h)]
    for sx0,sx1,dx0,dx1 in cols:
        for sy0,sy1,dy0,dy1 in rows:
            if dx1>dx0 and dy1>dy0: img.alpha_composite(s.crop((sx0,sy0,sx1,sy1)).resize((dx1-dx0,dy1-dy0),Image.NEAREST),(x+dx0,y+dy0))
def put(img,spr,px,py,pivot,cam,rot=0):
    if rot: spr=spr.rotate(rot,expand=True)
    img.alpha_composite(spr,(px-pivot[0]-cam[0],py-pivot[1]-cam[1]))
def base(room,cam):
    return L(room+'_base').crop((cam[0],cam[1],cam[0]+640,cam[1]+360))
def hero(img,x,y): img.alpha_composite(frame('hero_base',0,32,32),(x-16,y-30))
def hud(img,goal=None,gf=0,mm=None,bones=34,blood=212):
    img.alpha_composite(frame('ui_hud_hp_widget',0,86,16),(4,4)); img.alpha_composite(L('ui_hud_hp_bar_fill').crop((0,0,52,6)),(18,9)); text(img,'74',92,9)
    xp=L('ui_hud_xp_bar'); img.alpha_composite(xp.crop((0,0,98,9)),(17,21)); img.alpha_composite(xp.crop((98,0,98+25+40,9)),(17,21)); text(img,'LV7',18,22,(0x2c,0xe8,0xf5))
    if goal:
        w=max(44,13+tw(goal)+4); gx=320-w//2; nine(img,L('ui_hud_goal_box'),gf*44,44,13,12,2,3,2,gx,4,w,13); text(img,goal,gx+13,7)
    if mm: minimap(img,*mm)
    x=4
    for icon,v in ((0,blood),(1,bones)):
        s=str(v); w=14+tw(s); img.alpha_composite(L('ui_hud_counter_bg').resize((w,11),Image.NEAREST),(x,346)); img.alpha_composite(frame('ui_hud_counter_icons',icon,8,8),(x+1,347)); text(img,s,x+11,348); x=max(x+40,x+w+4)
def minimap(img,layout,cells):
    rows=len(layout); cols=len(layout[0]); mw=cols*10+9; mh=rows*8+9; mx=636-mw; my=4
    nine(img,L('ui_minimap_panel'),0,48,40,5,5,4,4,mx,my,mw,mh)
    T={'S':0,'1':1,'2':3,'3':2,'4':1,'F':3,'E':4,'H':5,'C':6,'T':7,'X':8,'B':9}
    for (gx,gy),cell in cells.items():
        cx=mx+5+gx*10; cy=my+5+gy*8; img.alpha_composite(frame('ui_minimap_cells',cell,9,7),(cx,cy))
        if cell!=3: img.alpha_composite(frame('ui_minimap_icons_5',T[layout[gy][gx]],5,5),(cx+2,cy+1))
        for (dx,dy,ox,oy) in ((1,0,9,3),(0,1,4,7)):
            if (gx+dx,gy+dy) in cells and layout[gy][gx]!='X' or (layout[gy][gx]=='X' and dy==1):
                if (gx+dx,gy+dy) in cells: img.alpha_composite(L('ui_minimap_corridor').crop((0,0,1,1)),(cx+ox,cy+oy))
def label(img,title,notes):
    title=re.sub('[()_=,]',' ',title); notes=[re.sub('[()_=,]',' ',n) for n in notes]
    out=img.resize((1280,720),Image.NEAREST); bar=Image.new('RGBA',(1280,40+16*len(notes)),(24,20,37,255))
    text(bar,'MOCK-UP  '+title,8,6,(0xfe,0xe7,0x61),2)
    for i,n in enumerate(notes): text(bar,n,8,28+16*i,(0xc0,0xcb,0xdc))
    full=Image.new('RGBA',(1280,720+bar.height)); full.alpha_composite(out,(0,0)); full.alpha_composite(bar,(0,720)); return full
# 1 minimap + HUD in a survive room (layout 2, 11 rooms)
L2=["B4...", ".32X.", ".C1E.", ".HSF."]
cam=(256,148); im=base('v08_survive',cam); hero(im,640-cam[0],420-cam[1])
cells={(2,3):1,(1,3):1,(2,2):0,(1,2):3,(3,2):3,(2,1):3,(3,3):3}
hud(im,'0:42',0,(L2,cells)); label(im,'01 HUD + MINIMAP (SURVIVE ROOM)',['MINIMAP: CURRENT CELL WHITE. VISITED/CLEARED DARK. NEXT-DOOR ROOMS AS UNKNOWN CELLS WITHOUT ICON. SECRET STAYS HIDDEN.','GOAL BOX FRAME 0 = SURVIVE TIMER 0:42. DOORS LOCKED UNTIL 0:00.']).save(O+'01_hud_minimap.png')
# 2 shrine shop
cam=(16,12); im=base('rooms_shrine_672x384',cam); hero(im,336-cam[0],300-cam[1])
names=[('PIERCE',25,'BUY'),('MULTIPLE PROJECTILES',40,'SWAP'),('BURN',20,'NO FIT')]
ws=[max(56,16+tw(n)+5,26+tw(str(pp))+8+tw(t)+5) for n,pp,t in names]; x=336-(sum(ws)+12)//2-cam[0]; y=244-cam[1]
for (n,p,t),w in zip(names,ws):
    nine(im,L('ui_shop_box'),0,56,30,5,5,4,4,x,y,w,30); im.alpha_composite(frame('ui_icons_room_8',5,8,8),(x+5,y+6)); text(im,n,x+16,y+6)
    im.alpha_composite(frame('ui_hud_counter_icons',1,8,8),(x+16,y+17)); text(im,str(p),x+26,y+18,(255,255,255) if p<=34 else (0xe4,0x3b,0x44))
    text(im,t,x+w-5-tw(t),y+18,{'BUY':(0x2c,0xe8,0xf5),'SWAP':(0xfe,0xe7,0x61),'NO FIT':(0xe4,0x3b,0x44)}[t]); text(im,str(names.index((n,p,t))+1),x+w//2-2,y-10,(0x2c,0xe8,0xf5)); x+=w+6
h='PRESS 1-3 TO BUY. SWAP REPLACES THE LAST SUPPORT'; text(im,h,336-cam[0]-tw(h)//2,278-cam[1],(0x8b,0x9b,0xb4))
hud(im,'SHOP',1,(L2,{(1,3):0,(2,3):1,(2,2):1,(1,2):3,(3,2):3,(3,3):3,(2,1):3}))
label(im,'02 BONE SHRINE SHOP',['3 OFFERS IN CODE-DRAWN SHOP_BOX (9-SLICE). 1-3 BUYS WITHIN ~110PX OF THE SHRINE. PRICE RED IF TOO FEW BONES.','BUY = OPEN SLOT. SWAP = REPLACES THE LAST SUPPORT OF A SKILL IT FITS. ICON = SHRINE ROOM ICON (NO SUPPORT ICON ART).']).save(O+'02_shrine_shop.png')
# 3 secret room
import json
cam=(0,0); im=base('rooms_secret_640x360',cam); D=json.load(open(R+'rooms_secret_640x360_data.json'))
for c in D.get('chests',[]): put(im,frame('rooms_chest_large',2 if c['elite'] else 0,30,24),c['x'],c['y'],(15,23),cam)
put(im,frame('ui_door',0,32,24),320,360-24,(16,23),cam,180) if False else None
d=frame('ui_door',0,32,24).rotate(180); im.alpha_composite(d,(320-16,360-24-1))
hero(im,320,296); hud(im,None,0,(L2,{(3,1):0,(3,2):1,(2,2):1,(2,3):1,(1,2):3,(2,1):3}),bones=58)
label(im,'03 SECRET ROOM',['REACHED BY BUMPING THE CRACKED WALL_SECRET ON THE NORTH WALL OF THE ROOM BELOW (AFTER IT IS CLEARED): CRACKED - BROKEN_OPEN - WALK IN.','2 CLOSED 30X24 CHESTS = 15-25 BONES EACH. ELITE CHEST = PICK 1 OF 3 SUPPORTS. EXIT = SOUTH DOOR.']).save(O+'03_secret_room.png')
# 4 locked fight room (hunt)
cam=(64,0); im=base('v08_hunt',cam); D=json.load(open(R+'v08_hunt_data.json'))
for spr,px,py,pv,rot in ((frame('ui_door',4,32,24),D['doorX'],D['wallN'],(16,23),0),):
    put(im,spr,px,py,pv,cam)
e=frame('ui_door',4,32,24).rotate(90,expand=True); im.alpha_composite(e,(0-cam[0]+64,D['doorY']-16-cam[1]))
e=frame('ui_door',4,32,24).rotate(-90,expand=True); im.alpha_composite(e,(768-24-cam[0],D['doorY']-16-cam[1]))
imp=L('enemy_imp') if False else None
hero(im,380-cam[0],250-cam[1]); hud(im,'37/100',1,(L2,{(2,3):1,(2,2):1,(1,2):0,(1,3):1,(1,1):3,(2,1):3,(3,2):3,(3,3):3}))
label(im,'04 LOCKED FIGHT ROOM (HUNT)',['ON ENTRY ALL DOORS PLAY CLOSE 0-3 AT 12FPS THEN STAY ON LOCKED FRAME 4 (BOSS DOOR: DOOR_BOSS LOCKED/OPEN).','GOAL BOX FRAME 1 (SKULL) = KILLS 37/100. AT 100 THE REST OF THE HORDE DIES AND THE DOORS OPEN. ENEMIES NOT DRAWN IN THIS MOCK-UP.']).save(O+'04_locked_fight_room.png')
print('ok')
