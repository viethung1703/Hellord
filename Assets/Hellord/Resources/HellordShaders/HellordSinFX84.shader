// Test compositor ONLY. Fire comes from ORIGINAL SpriteRenderer material via a Camera RT.
Shader "Hellord/SinFX84" {
 Properties { _MainTex("Historical static menu",2D)="black"{} _Backing("Verified clean atlas",2D)="black"{} _InkMask("Ink only",2D)="black"{} _FireL("Original renderer left",2D)="black"{} _FireR("Original renderer right",2D)="black"{} _PostL("Actual CRT left",2D)="black"{} _PostR("Actual CRT right",2D)="black"{} _Stone("Stone only mask",2D)="black"{} _Cracks("Authored crack paths",2D)="black"{} _Paper("Paper layer",2D)="black"{} _PaperBacking("Prepaper backing",2D)="black"{} _Energy("Measured fire energy",2D)="white"{} _T("Explicit seconds",Float)=0 _FireScale("Projection scale",Float)=0.5833333 }
 SubShader { Cull Off ZWrite Off ZTest Always Blend Off Pass { CGPROGRAM
 #pragma vertex vert_img
 #pragma fragment frag
 #include "UnityCG.cginc"
 sampler2D _MainTex,_Backing,_InkMask,_FireL,_FireR,_PostL,_PostR,_Stone,_Cracks,_Paper,_PaperBacking,_Energy;float _T,_FireScale;
 float3 Enc(float3 c){
 #if !defined(UNITY_COLORSPACE_GAMMA)
 return LinearToGammaSpace(c);
 #else
 return c;
 #endif
 }
 bool Box(float2 p,float4 r){return p.x>=r.x&&p.y>=r.y&&p.x<r.x+r.z&&p.y<r.y+r.w;}
 fixed4 frag(v2f_img i):SV_Target {
 float2 p=float2(i.uv.x,1-i.uv.y)*float2(480,270);
 float2 uv=(floor(p)+.5)/float2(480,270);uv.y=1-uv.y;
 float3 c=Enc(tex2D(_Backing,uv).rgb);
 float stone=tex2D(_Stone,uv).r;
 if(stone>.01){
  float el=tex2D(_Energy,float2(.25,.5)).r,er=tex2D(_Energy,float2(.75,.5)).r;
  float2 dl=(p-float2(30,215))/float2(60,150),dr=(p-float2(448,215))/float2(60,150);
  float light=(exp(-dot(dl,dl)*2)*el+exp(-dot(dr,dr)*2)*er)*stone*(.3+dot(c,float3(.3,.5,.2)));
  c=saturate(c+light*float3(.32,.12,.025));
  float4 cr=tex2D(_Cracks,uv);
  float a=.5+.5*sin(_T*2.1-cr.g*9+cr.b*6.283);
  float flow=.12+.55*pow(a,3);float hot=smoothstep(.90,1,a);
  c=saturate(c+cr.r*(flow*float3(.72,.045,.008)+hot*float3(.25,.23,.04)));
  c=saturate(c+cr.a*(1-cr.r)*(.3+.7*a)*float3(.09,.009,.002));
  // Selected red fragments lifting from stone; no fullscreen noise and masked off all controls.
  for(int j=0;j<12;j++){
   float side=j>=6?1:0;float k=fmod(j,6);float age=frac(_T/(2.3+k*.13)+j*.317);
   float px=side==0?18+k*5:461-k*5;
   float py=67+k*23-age*9;
   px+=round(sin(age*5+j)*age*2);
   if(age<.72&&abs(p.x-px)<.7&&abs(p.y-py)<1.0)c=lerp(c,float3(.94,.11,.035),.85*(1-age*.5));
  }
 }
 bool side=Box(p,float4(10,112,40,112))||Box(p,float4(428,112,40,112));
 int paper=-1;
 if(Box(p,float4(54,6,28,42)))paper=0;
 if(Box(p,float4(162,6,28,42)))paper=1;
 if(Box(p,float4(292,6,28,42)))paper=2;
 if(Box(p,float4(404,6,28,42)))paper=3;
 // Stone was already restored from verified backing; do not overwrite fire lighting here.
 if(paper>=0){
  c=Enc(tex2D(_PaperBacking,uv).rgb);
  // Continuous anchored flex, fixed y, no row floor, y remap or width compression.
  // Bilinear premultiplied RGBA interpolation preserves coherent strokes and clean edges.
  float bend=saturate((p.y-14)/31);float ph=_T*1.85+paper*1.43;
  float offset=1.7*bend*bend*sin(ph-bend*1.2)+.40*sin(bend*3.14159)*sin(ph*1.7);
  float2 puv=float2((p.x-offset)/480,1-p.y/270);
  float4 pp=tex2D(_Paper,puv);
  if(pp.a>.001){pp.rgb/=pp.a;float3 pc=Enc(pp.rgb);float ik=tex2D(_InkMask,puv).r;float pulse=.5-.5*cos(_T*6.2831853/3.6+paper*1.31);pc=saturate(pc+ik*pulse*float3(56,2,0)/255);c=lerp(c,pc,pp.a);}
 }
 for(int s=0;s<2;s++){
  float x=16+s*418;
  if(Box(p,float4(x,184,28,35))){
   float2 q=float2((p.x-(x+14))/_FireScale,(219-p.y)/_FireScale);
   float2 fuv=float2(q.x/48+.5,(q.y+12)/64);
   fixed4 f=s==0?tex2D(_FireL,fuv):tex2D(_FireR,fuv);
   // Actual 640x360 -> original halfres glow/CRT ->1280x720, cropped after post.
   // Post RT already contains display-encoded RGB; no second gamma encode.
   float3 post=s==0?tex2D(_PostL,fuv).rgb:tex2D(_PostR,fuv).rgb;
   c=saturate(c*(1-f.a)+post);
  }
  // Brighter, drifting original-colour opaque particles, enlarged to survive menu scale.
  if(Box(p,float4(10+s*418,112,40,107))){
   float2 np=floor(p);
   for(int j=0;j<6;j++){
    float f30=floor(_T*12)/12*30;
    float sp=.65+j*.095;float age=frac(f30*sp/40+j/6.0+s*.237);
    float lane=(j%2==0?-1:1)*(5+(j%3)*2);
    float px=floor(x+14+lane+sin(f30*.15+j*1.7+s)*3*age);
    float py=floor(211-(8+42*age));
    float w=j%3==0?2:1;float h=2;
    if(age<.90&&np.x>=px&&np.x<px+w&&np.y>=py&&np.y<py+h){c=age<.35?float3(254,231,97)/255:age<.65?float3(247,118,34)/255:float3(162,38,51)/255;}
   }
  }
 }
 return fixed4(c,1);
 }
 ENDCG } }
}
