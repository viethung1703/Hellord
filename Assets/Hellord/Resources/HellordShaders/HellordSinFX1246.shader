// t1259 v0.0.99: Sins compositor for the approved t1246b pass3e layout (480x270 native). Same math as HellordSinFX84.shader (and its offline port fx84.render_stage),
// only the layout numbers differ: pillars at x2/x438, fire boxes x6/x446 base y221, stone light centres (20,217)/(460,217) neutral tint .17, talismans at (8,112)/(444,112),
// ember box (2|438,114,40,107). Frame cracks (grid panel, detail panel, 2 plaques) live in the SAME _Cracks texture as the pillar cracks and use the SAME glow + clock,
// without the stone light tint. URP-safe: it is a plain fullscreen-quad fragment shader drawn with Graphics.DrawTexture like HellordSinFX84.
Shader "Hellord/SinFX1246" {
 Properties { _MainTex("unused",2D)="black"{} _Backing("Static scene (no dynamic UI)",2D)="black"{} _InkMask("Ink only",2D)="black"{} _FireL("Original renderer left",2D)="black"{} _FireR("Original renderer right",2D)="black"{} _PostL("Actual CRT left",2D)="black"{} _PostR("Actual CRT right",2D)="black"{} _Stone("Stone only mask",2D)="black"{} _Cracks("Pillar + frame crack paths",2D)="black"{} _Paper("Paper layer",2D)="black"{} _PaperBacking("unused",2D)="black"{} _Energy("Measured fire energy",2D)="white"{} _T("Explicit seconds",Float)=0 _FireScale("Projection scale",Float)=0.5833333 _TalFrames("t1263 talisman frames 176x32",2D)="black"{} _TalMasks("t1263 ink/paper/alpha masks",2D)="black"{} _Tal1263("1 = hand-pixelled sway frames, 0 = old row-shift",Float)=1 _Tal1265("1 = step84 smooth anchored flex on the 4x-expanded paper (default), 0 = t1260 row-shift flutter",Float)=1 _Emb1272("t1272: 1 = small embers that split into tiny sparks, 0 = t1271 embers",Float)=1 _Frame1273("t1273: 1 = brazier firelight also on the main frame stone (mask), 0 = off",Float)=1 _FrameMask("t1273 frame stone mask",2D)="black"{} _Emb1271("t1271: 1 = long-rising smooth-fading embers at screen resolution, 0 = t1262 embers",Float)=1 _Tal1270("t1270: 1 = talisman flex at SCREEN resolution (continuous bend, sharp-bilinear texels); 0 = t1265 native-pixel flex",Float)=1 _Tal1267("t1267 preview: 0 = step84 flex (default), 1 = A pendulum+rest, 2 = B still+hem gusts, 3 = C combined",Float)=0 _FireMode("1 = t1260 palette fire + banded fire light, 0 = v0.0.99 renderer fire",Float)=1 }
 SubShader { Cull Off ZWrite Off ZTest Always Blend Off Pass { CGPROGRAM
 #pragma target 3.0
 #pragma vertex vert_img
 #pragma fragment frag
 #include "UnityCG.cginc"
 sampler2D _FrameMask,_MainTex,_Backing,_InkMask,_FireL,_FireR,_PostL,_PostR,_Stone,_Cracks,_Paper,_PaperBacking,_Energy,_TalFrames,_TalMasks;float _T,_FireScale,_FireMode,_Tal1263,_Tal1265,_Tal1267,_Tal1270,_Emb1271,_Emb1272,_Frame1273;
// ---------------- t1260: palette fire, fire-driven light, embers (all native-pixel, stepped at 12 fps, pixel-crisp, no render targets) ----------------
float H2(float2 i){return frac(sin(dot(i,float2(12.9898,78.233)))*43758.5453);}
float VN(float2 q){float2 i=floor(q);float2 f=q-i;f=f*f*(3-2*f);return lerp(lerp(H2(i),H2(i+float2(1,0)),f.x),lerp(H2(i+float2(0,1)),H2(i+float2(1,1)),f.x),f.y);}
float TS(){return floor(_T*12)/12;}
// fire flicker 0.55..1: drives the flame height AND the light intensity
float Flick(float side){float ts=TS();return .55+.45*VN(float2(ts*5+side*17.3,1.7+side*5));}
float Bayer4(float2 np){int x=((int)np.x)&3;int y=((int)np.y)&3;int a=((x&1)^(y&1))*2+(y&1);int b=(((x>>1)&1)^((y>>1)&1))*2+((y>>1)&1);return (a*4+b+.5)/16.0;}
static const float3 PAL0=float3(254,231,97)/255;static const float3 PAL1=float3(254,174,52)/255;static const float3 PAL2=float3(247,118,34)/255;static const float3 PAL3=float3(228,59,68)/255;static const float3 PAL4=float3(162,38,51)/255;
// banded, ordered-dither light level (0..1, 5 steps) of one brazier at native pixel np (side 0 = left pillar, 1 = right)
float FireLight(float2 p,float side,float dith){
 float2 lp=float2(side<.5?20:460,203);float2 d2=(p-lp)/float2(60,112);float L=saturate(1-length(d2));L=pow(L,1.25)*1.12*(.35+.65*Flick(side));   // t1262: was 1.45
 return saturate(floor(L*4+(dith>.5?Bayer4(floor(p)):.5))/4);   // 5 bands; ordered dither between bands (paper: plain bands)
}
float3 Lit(float3 c,float2 p,float dith){
 float lv=max(FireLight(p,0,dith),FireLight(p,1,dith));
 // t1262: warm cast that keeps the stone's own brightness (luma-normalised), a mild lift, no additive floor: mortar stays dark, stone value steps stay readable
 float3 tint=float3(1+.46*lv,1+.10*lv,1-.34*lv);tint/=dot(tint,float3(.299,.587,.114));
 return saturate(c*tint*(1+.09*lv));
}
// t1273: extra banded firelight for the MAIN frame stone only (ui_t1273_frame_mask: outer ornate border grey pixels + the stone rings of the grid/detail panels). Wider reach than the pillar light (110x125 px around each fire), same 5 bands, Bayer dither, flicker clock and strength rule as Lit().
float FrameLvS(float2 p,float sd,float dith){float2 d=(p-float2(sd<.5?20:460,203))/float2(125,135);float L=saturate(1-length(d));L=pow(L,1.1)*1.2*(.35+.65*Flick(sd));return saturate(floor(L*4+dith)/4);}
float FrameLv(float2 p){float dith=Bayer4(floor(p));return max(FrameLvS(p,0,dith),FrameLvS(p,1,dith));}
// t1262: bowl cavity (must match tools/t1246/t1262_patch_brazier.py): ellipse centre (cx,219.9) radii (14.6,4.3). Flame and coals live INSIDE it; the front lip is the base art drawn on top.
static const float2 CAVR=float2(14.6,4.3);static const float CAVY=219.9;
float CavCx(float side){return side<.5?19.5:459.5;}
float CavE(float2 p,float side){float2 d=float2((p.x-CavCx(side))/CAVR.x,(p.y-CAVY)/CAVR.y);return dot(d,d);}
// y of the front lip's top edge at pixel column p.x: flame/embers are hidden at and below it (the front rim hides the flame base)
float FrontY(float2 p,float side){float u=(p.x-CavCx(side))/CAVR.x;return CAVY+CAVR.y*sqrt(max(0,1-u*u))-.5;}
// flame: 5 tongues rising from separate points across the bowl, each with its own height/sway/width; hottest at the base, no straight cut (the rim hides the base)
// t1263: hand-pixelled sway loop. 8 frames, ms: 190 160 170 210 190 160 170 210 (1460 ms loop) read off the unscaled clock _T; right talisman plays frame (i+4) mod 8.
float TalFrame(){float m=fmod(_T*1000,1460.0);return m<190?0:m<350?1:m<520?2:m<730?3:m<920?4:m<1080?5:m<1250?6:7;}
float4 Flame(float2 p,float side){
 float cx=CavCx(side);float ts=TS();float I=Flick(side);
 float H=225.4-p.y;float Hmax=38+8*I;
 if(H<-.5||H>=Hmax)return 0;
 float best=-1;
 for(int k=0;k<5;k++){
  float kk=k;
  float hs=VN(float2(kk*1.7+side*9.3+ts*4.5,3.3+kk));if(k==2)hs=max(hs,.72);
  float env=1-pow(abs(kk-2)/2.2,1.3)*.5;
  float hk=max(Hmax*(.34+.66*hs)*env,6.5);
  if(H>=hk)continue;
  float t=H/hk;
  float off=(kk-2)*3.9+(VN(float2(kk*3.1+side*5,ts*2.5))-.5)*2.4;
  float sway=sin(ts*5.3+side*2+kk*1.3+t*3.2)*.9*t+(VN(float2(t*2+ts*3+kk,side*3))-.5)*1.8*t;
  float w=(3.5+1.0*hs)*pow(1-t,.72)*(.5+.5*smoothstep(0,6,H));
  float dx=abs(p.x-(cx+off+sway));
  if(dx>=w)continue;
  float n=VN(float2(p.x*.75+3*side,p.y*.55+ts*17));
  float heat=(1-t)*(1-.5*dx/max(w,.5))+(n-.5)*.16*t+.09*(I-.7);
  best=max(best,heat);
 }
 if(best<.10)return 0;
 float3 col;
 if(best>.66)col=PAL0;else if(best>.47)col=PAL1;else if(best>.31)col=PAL2;else if(best>.19)col=PAL3;else col=PAL4;
 return float4(col,1);
}
// glowing coals in the dark cavity (static clumps that pulse at the 12 fps clock): deep red, red, orange, a few yellow pixels
float3 Coals(float3 c,float2 p,float side){
 float e=CavE(p,side);if(e>=1)return c;
 float ts=TS();float2 np=floor(p);float I=Flick(side);
 float u=(p.x-CavCx(side))/CAVR.x;float v=(p.y-CAVY)/CAVR.y;
 float st=VN(np*float2(.62,.9)+side*9.7)*.55+H2(np+side*31.7)*.45;
 float fl=(VN(float2(H2(np+4.2)*23+ts*3.1,np.x*.13+side))-.5)*.55+(I-.7)*.35;
 float lv=st+fl+.22*(1-abs(u))+.12*v-(v<-.55?.22:0);
 if(lv>1.02)return PAL0;if(lv>.9)return PAL2;if(lv>.74)return PAL3;if(lv>.58)return PAL4;
 return c;
}
 float3 Enc(float3 c){
 #if !defined(UNITY_COLORSPACE_GAMMA)
 return LinearToGammaSpace(c);
 #else
 return c;
 #endif
 }
 bool Box(float2 p,float4 r){return p.x>=r.x&&p.y>=r.y&&p.x<r.x+r.z&&p.y<r.y+r.w;}
 float3 CrackGlow(float3 c,float4 cr){
  if(cr.a<.01)return c; // transparent texels carry no crack (the importer may bleed colour into them)
  float a=.5+.5*sin(_T*2.1-cr.g*9+cr.b*6.283);
  float flow=.12+.55*pow(a,3);float hot=smoothstep(.90,1,a);
  c=saturate(c+cr.r*(flow*float3(.72,.045,.008)+hot*float3(.25,.23,.04)));
  return saturate(c+cr.a*(1-cr.r)*(.3+.7*a)*float3(.09,.009,.002));
 }
float Hh67(float k,float s){return frac(sin(dot(float2(k,s),float2(12.9898,78.233)))*43758.5453);}
// t1267: seeded, irregular talisman motion. tl = 0 left / 1 right (different seeds). src = where to read the 4x-expanded paper layer for output pixel p.
// pendulum: rigid rotation about the nail (top-centre of the paper), damped, events 2-5 s apart (jittered grid t_k=3.5k+1.5h_k => gap in [2,5]); hem gusts: events 3-7 s apart (t_k=5k+2h_k), 0.4-0.7 s flutter, damped overshoot, only the bottom 30% moves (continuous ramp, no tear)
void Tal67(float2 p,float tl,out float2 src,out float tilt,out float gust){
 int mode=(int)(_Tal1267+.5);float seed=3.7+tl*11.3;float T=_T;float pend=0;float gv=0;float dx=0,ly=0;
 float2 pivot=float2(tl<.5?22.0:458.0,120.0);
 float thA=mode==1?1.8:.55;float fr=mode==1?.85:.5;float dp=mode==1?1.5:.8;
 if(mode==1||mode==3){float k0=floor(T/3.5);
  for(int j=-2;j<=0;j++){float k=k0+j;float age=T-(3.5*k+1.5*Hh67(k,seed));
   if(age>0){float a=.45+.55*Hh67(k,seed+5);float sg=Hh67(k,seed+9)>.5?1:-1;pend+=sg*a*thA*exp(-dp*age)*sin(6.2831853*fr*age)*smoothstep(0,.3,age);}}}
 float v=saturate((p.y-120)/32);float m=smoothstep(.70,1.0,v);
 if(mode==2||mode==3){float k0=floor(T/5.0);
  for(int j=-1;j<=0;j++){float k=k0+j;float age=T-(5.0*k+2.0*Hh67(k,seed+40));
   if(age>0&&age<2.6){float dur=.4+.3*Hh67(k,seed+21);float u=age-dur;float am=.6+.4*Hh67(k,seed+31);float sg=Hh67(k,seed+33)>.5?1:-1;
    float flut=(1-smoothstep(dur-.12,dur,age))*.30*sin(6.2831853*7*age+p.x*.6);
    float R=smoothstep(0,.15,age)*(u<0?1+flut:exp(-3.5*u)*cos(6.2831853*1.6*u));
    dx+=sg*1.7*am*R*m;ly+=1.5*am*R*m;gv+=am*R;}}}
 float ang=pend*0.0174533;float cs=cos(ang),sn=sin(ang);float2 d=p-pivot;
 src=pivot+float2(d.x*cs+d.y*sn,-d.x*sn+d.y*cs);src.x-=dx;src.y+=ly;
 tilt=pend/thA;gust=gv*m;
}

// t1270: pixel-art AA ("sharp bilinear"): texels stay hard squares, only their edges are smoothed over ~1 SCREEN pixel (fw = art px per screen px). 4 fetches at exact texel centres of the 4x layer (=the art pixel), blended with a steep ramp.
float4 PaperS(float2 sp,float2 fw,out float ik){
 float2 t=sp-.5;float2 i0=floor(t);float2 f=t-i0;float2 w=saturate((f-.5)/fw+.5);
 float2 c0=(i0+.5)/float2(480,270),c1=(i0+1.5)/float2(480,270);
 float4 a00=tex2Dlod(_Paper,float4(c0.x,1-c0.y,0,0)),a10=tex2Dlod(_Paper,float4(c1.x,1-c0.y,0,0)),a01=tex2Dlod(_Paper,float4(c0.x,1-c1.y,0,0)),a11=tex2Dlod(_Paper,float4(c1.x,1-c1.y,0,0));
 float k00=tex2Dlod(_InkMask,float4(c0.x,1-c0.y,0,0)).r,k10=tex2Dlod(_InkMask,float4(c1.x,1-c0.y,0,0)).r,k01=tex2Dlod(_InkMask,float4(c0.x,1-c1.y,0,0)).r,k11=tex2Dlod(_InkMask,float4(c1.x,1-c1.y,0,0)).r;
 ik=lerp(lerp(k00,k10,w.x),lerp(k01,k11,w.x),w.y);
 return lerp(lerp(a00,a10,w.x),lerp(a01,a11,w.x),w.y);
}

 fixed4 frag(v2f_img i):SV_Target {
 // every fragment of one native pixel evaluates at that pixel's centre: integer-scale blocks stay uniform (pixel art), identical to the offline render
 float2 pf=float2(i.uv.x,1-i.uv.y)*float2(480,270);float2 fwp=max(fwidth(pf),float2(1e-4,1e-4));float2 p=floor(pf)+.5;float papA70=0;   // t1270: pf = the continuous art-pixel position of THIS screen pixel (p stays the native pixel centre for everything else)
 float2 uv=p/float2(480,270);uv.y=1-uv.y;
 float3 c=Enc(tex2Dlod(_Backing,float4(uv,0,0)).rgb);
 float stone=tex2Dlod(_Stone,float4(uv,0,0)).r;
 float4 cr=tex2Dlod(_Cracks,float4(uv,0,0));
 float lvF=0;if(_FireMode>.5&&_Frame1273>.5)lvF=FrameLv(p);
 if(lvF>0&&tex2Dlod(_FrameMask,float4(uv,0,0)).r>.5){float lvx=lvF;float3 tn=float3(1+.46*lvx,1+.10*lvx,1-.34*lvx);tn/=dot(tn,float3(.299,.587,.114));c=saturate(c*tn*(1+.09*lvx));}   // t1273: (applied on the unlit base, Lit() below adds the pillar light as before)
 if(_FireMode>.5)c=Lit(c,p,1);   // t1260: banded fire light over the whole environment (pillar stone, backdrop wall, talisman, frame edge)
if(stone>.01){
  if(_FireMode<.5){
  float el=tex2Dlod(_Energy,float4(float2(.25,.5),0,0)).r,er=tex2Dlod(_Energy,float4(float2(.75,.5),0,0)).r;
  float2 dl=(p-float2(20,217))/float2(60,150),dr=(p-float2(460,217))/float2(60,150);
  float light=(exp(-dot(dl,dl)*2)*el+exp(-dot(dr,dr)*2)*er)*stone*(.3+dot(c,float3(.3,.5,.2)));
  c=saturate(c+light*float3(.17,.17,.17));
  }
  c=CrackGlow(c,cr);
  // Selected red fragments lifting from the pillar stone (same maths as HellordSinFX84, side columns of the new layout).
  for(int j=0;j<12;j++){
   float side=j>=6?1:0;float k=fmod(j,6);float age=frac(_T/(2.3+k*.13)+j*.317);
   float px=side==0?10+k*5:469-k*5;
   float py=67+k*23-age*9;
   px+=round(sin(age*5+j)*age*2);
   if(age<.72&&abs(p.x-px)<.7&&abs(p.y-py)<1.0)c=lerp(c,float3(.94,.11,.035),.85*(1-age*.5));
  }
 } else if(cr.a>.01){
  // frame cracks: the same pillar crack glow on the same clock, no fire-light tint on the frame stone
  c=CrackGlow(c,float4(saturate(cr.r*(1+.7*lvF)),cr.g,cr.b,cr.a));   // t1273: glows up to 70% more near the fire (lvF = 0 when -framelight1273off)
 }
 // talismans (8,112) / (444,112), 28x42, idx 0 / 1, pinned top 8 rows (the static pinned top is part of the backing)
 if(_Tal1263>.5&&_FireMode>.5){
  // hand-pixelled frames: top-left at (11,120) / (447,120), 22x32, integer pixels, Point sampling; the clasp (rows 116..119) is static in the base
  for(int tl=0;tl<2;tl++){
   float2 org=float2(tl==0?11:447,120);float2 lp=p-org;
   if(lp.x>=0&&lp.y>=0&&lp.x<22&&lp.y<32){
    float fi=fmod(TalFrame()+tl*4,8);
    float2 tuv=float2((lp.x+fi*22)/176.0,1-lp.y/32.0);
    float4 fr=tex2Dlod(_TalFrames,float4(tuv,0,0));float3 mk=tex2Dlod(_TalMasks,float4(tuv,0,0)).rgb;
    if(mk.b>.5){
     float3 pc=Enc(fr.rgb);
     if(mk.g>.5)pc=Lit(pc,p,1);   // paper: the SAME banded fire light (Bayer-dithered 5 bands, brazier flicker clock) as the static clasp strip above row 120, so there is no seam; outline stays unlit
     if(mk.g>.5){float fb=floor(saturate((Flick(tl)-.55)/.45)*3.99)/3;float amp=.13*floor(lp.y/8.0)/3.0;pc=saturate(pc*(1+(fb-.5)*amp*float3(1,1.15,1.5)));}   // extra brazier-flicker band on the paper, 4 bands in time x 4 bands in height (0 at the clasp, strongest at the hem nearest the fire): no seam under the clasp
     if(mk.r>.5){   // sigil ink: glow pulse on the pillar crack clock (CrackGlow: sin(_T*2.1) with .12+.55 a^3 and a hot peak), scaled by the fire flicker
      float a=.5+.5*sin(_T*2.1-.9-tl*.8);float flow=.12+.55*a*a*a;float hot=smoothstep(.90,1,a);float fl=Flick(tl);
      pc=saturate(pc+(flow*(.50+.50*fl*fl)*float3(.30,.026,0)+hot*float3(.10,.07,0)));
     }
     c=pc;
    }
   }
  }
 }
 else{
 int paper=-1;
 if(Box(p,float4(8,112,28,42)))paper=0;
 if(Box(p,float4(444,112,28,42)))paper=1;
 if(paper>=0){
  float bend=saturate((p.y-112-8)/31);float ph=_T*1.85+paper*1.43;
  float offset=1.7*bend*bend*sin(ph-bend*1.2)+.40*sin(bend*3.14159)*sin(ph*1.7);
  float yoff=0;
  if(_FireMode>.5&&_Tal1265<.5){   // t1260: whole-pixel flutter per row (crisp), slow gust envelope, a second ripple, bottom edge lifts 1px on the swing
   float gust=.55+.7*(.5+.5*sin(_T*.45+paper*2.1));
   offset=3.4*pow(bend,1.4)*gust*(sin(ph-bend*2.1)*(.75+.25*sin(_T*.7+paper))+.4*sin(ph*2.3-bend*5+paper))+.35*sin(bend*3.14159)*sin(ph*1.7);
   offset=floor(offset+.5);yoff=floor(bend*bend*.99*(.5+.5*sin(ph*1.3+paper))+.5);
  }
  float2 srcp=float2(p.x-offset,p.y+yoff);float tilt67=0,gust67=0;
  if(_FireMode>.5&&_Tal1265>.5&&_Tal1267>.5)Tal67(p,paper,srcp,tilt67,gust67);
  float2 puv=float2(srcp.x/480,1-srcp.y/270);
  if(_Tal1267>.5)puv=float2((floor(srcp.x*4)+.5)/1920.0,1-(floor(srcp.y*4)+.5)/1080.0);   // t1267: NEAREST on the 4x-expanded layer (quarter-pixel steps): every pixel is exactly a paper/ink/outline texel, never a blend of two, so a rotated or lifted talisman cannot go translucent or blurry; the outline is part of the layer so it moves with the paper
  bool sm70=_Tal1270>.5&&_FireMode>.5&&_Tal1265>.5&&_Tal1267<.5;float ik70=0;float4 pp;
  if(sm70){   // t1270: the SAME step84 flex formula (amplitude, phase, timing), evaluated per SCREEN pixel row: the bend is continuous, the paper curves instead of shearing in stair-steps
   float bs=saturate((pf.y-120)/31);float of=1.7*bs*bs*sin(ph-bs*1.2)+.40*sin(bs*3.14159)*sin(ph*1.7);
   pp=PaperS(float2(pf.x-of,pf.y),fwp*.5,ik70);}
  else pp=tex2Dlod(_Paper,float4(puv,0,0));
  if(pp.a>.001){pp.rgb/=pp.a;float3 pc=Enc(pp.rgb);float ik=sm70?ik70:tex2Dlod(_InkMask,float4(puv,0,0)).r;float pulse=.5-.5*cos(_T*6.2831853/3.6+paper*1.31);
   if(_FireMode>.5&&_Tal1265>.5){   // t1265: step84 smooth flex + the t1263 firelight and crack-clock ink glow (colour only: no position change, so no pixel can break)
    float lv=max(FireLight(p,0,0),FireLight(p,1,0));float3 tint=float3(1+.40*lv,1+.08*lv,1-.30*lv);tint/=dot(tint,float3(.299,.587,.114));pc=saturate(pc*tint*(.97+.07*lv));
    float fb=floor(saturate((Flick(paper)-.55)/.45)*3.99)/3;float amp=.13*floor(max(p.y-120,0)/8.0)/3.0;pc=saturate(pc*(1+(fb-.5)*amp*float3(1,1.15,1.5)));
    if(_Tal1267>.5){   // t1267: light shifts with the tilt (banded), the hem catches the light on a gust, C also gets slow banded light sweeping over the folds
     float px=paper<.5?22.0:458.0;float md=_Tal1267>2.5?.5:1;float sh=tilt67*clamp((p.x-px)/9,-1,1)*.07*md+gust67*.10*smoothstep(.70,1.0,saturate((p.y-120)/32));
     if(_Tal1267>2.5)sh+=(floor((.5+.5*sin(p.x*.45+p.y*.33-_T*.8+paper*2.0))*3.0)/3.0-.5)*.06;
     sh=floor(sh/.02+.5)*.02;pc=saturate(pc*(1+sh*float3(1,1.1,1.4)));}
    float a=.5+.5*sin(_T*2.1-.9-paper*.8);float flow=.12+.55*a*a*a;float hot=smoothstep(.90,1,a);float fl=Flick(paper);
    pc=saturate(pc+ik*(flow*(.50+.50*fl*fl)*float3(.30,.026,0)+hot*float3(.10,.07,0)));}
   else if(_FireMode>.5){float fl=Flick(paper);pc=Lit(pc,p,0);pulse=saturate(.30+.70*pulse)*(.55+.95*fl*fl);pc=saturate(pc+ik*pulse*float3(70,6,0)/255);}   // ink glow reacts to the fire light
   else pc=saturate(pc+ik*pulse*float3(56,2,0)/255);
   papA70=_Tal1267>.5?step(.5,pp.a):pp.a;c=lerp(c,pc,papA70);}
 }
 }
 if(_FireMode>.5){
 for(int s=0;s<2;s++){
  {float ce=CavE(p,s);if(ce>=1&&ce<1.9&&p.y<227){float g=(1.9-ce)/.9;c=saturate(c*lerp(float3(1,1,1),float3(1.55,1.0,.55),g*(.35+.5*Flick(s))));}}   // rim catches the firelight
  c=Coals(c,p,s);   // t1262: coals in the cavity, then the flame over them; the front lip (base art) is never overwritten below FrontY
  float4 fl=Flame(p,s);if(fl.a>.5&&p.y<FrontY(p,s))c=fl.rgb;
  // embers: 9 per brazier, rise ~60 px with sway, colour by age, dither out at the end (12 fps steps, 1px, opaque)
  float cx=CavCx(s);float ts=TS();float2 np=floor(p);
  if(_Emb1271>.5){   // t1271: 48 embers per brazier (slot j, own period 4.8-8.6 s, own phase, re-rolled each cycle => no spawn pattern, L/R never sync), continuous time + screen-pixel coverage = glides in sub-art-pixel steps as crisp squares
   float x0=s==0?2:438;
   if(p.x>=x0&&p.x<x0+40&&p.y>=112&&p.y<FrontY(p,s)){
    [loop] for(int j=0;j<48;j++){
     float fj=j+s*61.0;float L=4.8+3.8*H2(float2(fj,3.1));float u=_T/L+H2(float2(fj,7.7));float k=floor(u);float a=u-k;
     float2 hk=float2(fj*1.37+k*7.13,s*3.3+k*.91);float r1=H2(hk),r2=H2(hk+17.3);
     float r3=frac(r1*91.7+r2*13.3),r4=frac(r2*57.3+r1*29.1),r5=frac(r1*37.9+r2*71.7),r6=frac(r2*23.3+r1*83.1);
     bool n72=_Emb1272>.5;float b=.7+.3*r5;float core=n72?0:step(r4,.2);
     float asp=.65+.2*frac(r6*47.3+r1*13.7);int ns=n72?2+(int)floor(frac(r3*29.1+r2*5.3)*3):0;   // t1272: the ember splits at 65-85% of its life into 2-4 tiny sparks
     [loop] for(int m=0;m<=ns;m++){
      float h1=frac(r1*(91.7+m*13.1)+r2*(37.3+m*7.7)+m*.618),h2=frac(h1*57.3+r6),h3=frac(h1*91.1+r5);
      float am=asp-((m>0&&h1<.25)?.15*h2:0);float aq=m==0?a:am;   // an occasional spark sheds early
      if(m==0||a>am){
       float rise=1-pow(1-aq,1.6);                                   // decelerates as it cools
       float ey=220-(62+34*r3)*rise;                                  // top y >= 124: stays inside the pillar, well below the lintel/frame
       float ex=cx+(r1-.5)*16+(1.2+2.3*r4)*(.3+.7*smoothstep(0,.5,aq))*sin(_T*(.8+.9*r5)+r2*6.283)+1.5*sin(_T*(.25+.2*r6)+r1*6.283)*aq+(r6-.5)*8*rise;
       float wv,cool,hot=0,al;
       if(m==0){
        wv=n72?(r4<.3?.75:.5)*(1-.35*smoothstep(asp-.15,asp,a)):lerp(1.0,lerp(2.0,1.0,smoothstep(.45,.62,a)),core);
        cool=n72?saturate((a-.3+(1-b)*.1)/.62):saturate((a-.6+(1-b)*.14)/.36);hot=max(smoothstep(.2,0,a)*.5,core*.55*(1-smoothstep(.2,.5,a)));
        al=smoothstep(0,.04,a)*(.75+.25*b)*(1-smoothstep(.9,1,a))*(a>.75?.7+.3*sin(_T*38+r1*40):1)*(n72?1-smoothstep(asp-.03,asp,a):1);
       }else{
        float tn=saturate((a-am)/max(1-am,.05));float ts=(a-am)*L;
        ex+=(h2-.5)*6*ts*(1-.25*tn)+sin(_T*(7+h1*6)+h3*6.283)*.45*saturate(ts*2);ey-=(5+9*h3)*ts*(1-.3*tn);   // faster, more erratic drift than the parent
        wv=.25;cool=saturate(.3+.75*tn+(h3-.5)*.1);
        al=smoothstep(0,.08,tn)*(1-smoothstep(.5,1,tn/(.55+.45*h2)))*(.7+.3*sin(_T*(20+h3*14)+h1*6.283))*(.8+.2*b);   // staggered smooth fade, flicker
       }
       al*=smoothstep(118,134,ey)*(1-papA70)*(m==0&&!n72?1:smoothstep(0,6,min(ex-x0,x0+40-ex)));
       float3 col=cool<.22?lerp(lerp(PAL0,float3(1,.97,.78),hot),PAL1,cool/.22):cool<.45?lerp(PAL1,PAL2,(cool-.22)/.23):cool<.68?lerp(PAL2,PAL3,(cool-.45)/.23):lerp(PAL3,PAL4,saturate((cool-.68)/.32));
       float2 q=pf-float2(ex,ey);
       if(abs(q.x)<3&&abs(q.y)<3){
        float cov;
        if(n72){float2 nn=max(float2(1,1),round(wv/fwp));float2 ci=floor(float2(ex,ey)/fwp-nn*.5+.5);float2 pi=floor(pf/fwp);cov=(all(pi>=ci)&&all(pi<ci+nn))?1:0;}   // t1272: whole-screen-pixel squares (never below 1 px), glide in screen-pixel steps, perfectly crisp
        else{float2 lo=max(pf-fwp*.5,float2(ex,ey)-wv*.5),hi=min(pf+fwp*.5,float2(ex,ey)+wv*.5);float2 ov=saturate(hi-lo)/fwp;cov=ov.x*ov.y;}   // t1271 box-filter coverage
        c=lerp(c,col,saturate(cov*al));
       }
      }
     }
    }
   }
  }
  else
  for(int j=0;j<9;j++){
   float age=frac(ts/(2.3+(j%4)*.55)+j*.137+s*.31);
   float lane=((j*5)%9-4)*2.1;
   float px=floor(cx+lane+sin(ts*2.3+j*1.9+s)*3.5*age+.5);
   float py=floor(220-age*(38+(j%3)*12));
   float w=(j%3==0)?2:1;
   float keep=step(H2(float2(floor(_T*12)+j*3.1,s*7.0+j)),1.7*(1-age));
   if(age<.93&&keep>.5&&p.y<FrontY(p,s)&&np.x>=px&&np.x<px+w&&np.y>=py&&np.y<py+1){c=age<.22?PAL0:age<.5?PAL1:age<.75?PAL2:PAL3;}
  }
 }
}
if(_FireMode<.5)for(int s=0;s<2;s++){
  float x=s==0?6:446;
  if(Box(p,float4(x,186,28,35))){
   float2 q=float2((p.x-(x+14))/_FireScale,(221-p.y)/_FireScale);
   float2 fuv=float2(q.x/48+.5,(q.y+12)/64);
   fixed4 f=s==0?tex2Dlod(_FireL,float4(fuv,0,0)):tex2Dlod(_FireR,float4(fuv,0,0));
   float3 post=s==0?tex2Dlod(_PostL,float4(fuv,0,0)).rgb:tex2Dlod(_PostR,float4(fuv,0,0)).rgb;
   c=saturate(c*(1-f.a)+post);
  }
  // opaque embers clipped to the ember box (2|438,114,40,107)
  if(Box(p,float4(s==0?2:438,114,40,107))){
   float2 np=floor(p);
   for(int j=0;j<6;j++){
    float f30=floor(_T*12)/12*30;
    float sp=.65+j*.095;float age=frac(f30*sp/40+j/6.0+s*.237);
    float lane=(j%2==0?-1:1)*(5+(j%3)*2);
    float px=floor(x+14+lane+sin(f30*.15+j*1.7+s)*3*age);
    float py=floor(213-(8+42*age));
    float w=j%3==0?2:1;float h=2;
    if(age<.90&&np.x>=px&&np.x<px+w&&np.y>=py&&np.y<py+h){c=age<.35?float3(254,231,97)/255:age<.65?float3(247,118,34)/255:float3(162,38,51)/255;}
   }
  }
 }
 return fixed4(c,1);
 }
 ENDCG } }
}
