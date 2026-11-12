import export_t1246 as X
from export_t1246 import *
SCN.S4.put = X._put0   # original put for the reference build
def build_ref(sel=0):
    SCN.SEL_TAB[0] = sel; SCN.NOLABEL[0] = True; SCN.TABS_ON[0] = True; SCN.GLOW_ON[0] = True; SCN.BTN_ST.clear()
    fx84.LIGHT_TINT = B.NEUT_TINT; RC.VARIANT[0] = 'B'; parts = B.make_parts()
    S, mir = SCN.sins4(parts, red=True); return S
Sref = build_ref(0)
SCN.S4.put = X.put_p
Sb, parts = X.build_base(0)
rr = {e['name']: e['r'] for e in Sref.els}
print({k: rr[k] for k in ('tab0','tab1','tab2','tab3','equip','prev','pager','next','back','dicon','souls','sigils','slot0','slot1','slot2')})
c = Sb.c.copy()
PUT = lambda im, xy: c.alpha_composite(im, xy)
def cellimg(k):
    if k%9==4: st='locked'
    elif k==10: st='selected'
    elif k==0: st='equipped'
    elif k==3: st='hover'
    else: st='normal'
    return SCN.cell24(st) if st!='selected' else Image.fromarray(HP.cell(HP.PEAK), 'RGBA')
for k in range(40):
    x,y,w,h = rr[f'cell{k}']; PUT(cellimg(k), (x,y))
x,y,w,h = rr['dicon']; PUT(SCN.cell24('normal'), (x,y))
SX=(224-32-8,224,256+8); PUT(Sref.parts['lock_glyph'][0], (SX[2]+16-4, 18+16-5))
glowed=set()
tl=['ALL','MORTAL','DEMONIC','HEAVEN-DEFYING']
for i in range(4):
    x,y,w,h = rr[f'tab{i}']; st='selected' if i==0 else 'normal'
    PUT(SCN.painted('tab_'+st, w, h if False else (20 if i==0 else 17)), (x,y))
    if st=='selected':
        pimg=SCN.painted('tab_selected', w, 20); PUT(Image.fromarray(HP.glow_img(np.array(pimg)[...,3], HP.SEL_FRAMES[HP.SEL_PEAK][0]),'RGBA'), (x-1,y-1)); glowed.add(i)
def btn(cls, st, name, prefix=None):
    x,y,w,h = rr[name]; PUT(Image.fromarray(HP.compose(cls, st, w, h),'RGBA'), (x,y))
btn('primary','normal','equip'); btn('small','disabled','prev'); btn('small','normal','next'); btn('secondary','normal','back')
x,y,w,h = rr['pager']; PUT(SCN.painted('pager', w, h), (x,y))
# shadows
px = c.load(); ink = tuple(PC['INK'])+(255,)
for nm in ('equip','prev','next','back','pager','tab0','tab1','tab2','tab3'):
    if nm.startswith('tab') and int(nm[3]) in glowed: continue
    x_,y_,w_,h_ = rr[nm]; tab_=nm.startswith('tab')
    for yy in range(y_+1, y_+h_+(0 if tab_ else 1)): px[x_+w_, yy] = ink
    if not tab_:
        for xx in range(x_+1, x_+w_+1): px[xx, y_+h_] = ink
m = np.load(SCN.CLEAR_MASK)
A = np.array(c.convert('RGB')).astype(int); R = np.array(Sref.c.convert('RGB')).astype(int)
d = (abs(A-R).sum(2)>0); print('diff before mask applied', d.sum())
ys,xs=np.nonzero(d); print(sorted(set(zip(xs.tolist(),ys.tolist())))[:20])
print('mask px', m.sum(), 'mask inside diff', (m.T[:,:] if m.shape==(480,270) else m)[...].shape)
