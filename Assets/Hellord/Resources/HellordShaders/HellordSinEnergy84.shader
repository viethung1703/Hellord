Shader "Hidden/HellordSinEnergy84" { Properties{_MainTex("Left",2D)="black"{} _Right("Right",2D)="black"{}} SubShader{Cull Off ZWrite Off ZTest Always Blend Off Pass{CGPROGRAM
#pragma vertex vert_img
#pragma fragment frag
#include "UnityCG.cginc"
sampler2D _MainTex,_Right;
fixed4 frag(v2f_img i):SV_Target{float e=0;for(int y=0;y<6;y++)for(int x=0;x<8;x++){float2 uv=float2((x+1)/9.0,.20+(y+.5)/6*.55);float4 c=i.uv.x<.5?tex2D(_MainTex,uv):tex2D(_Right,uv);e+=dot(c.rgb,float3(.3,.5,.2))*c.a;}e=clamp(.45+e/48*2.4,.45,1.5);return fixed4(e,e,e,1);}
ENDCG}}}
