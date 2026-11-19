// t1567 Hellord HitFlash: overbright hit pass drawn as a second sprite over the hit enemy (same sprite, same flip, one order above).
// out = _FlashColor.rgb * (tex.a * vertexColor.a * _Flash), Blend One One, so the demon reads WHITE-HOT for a few frames without swapping its material.
// _Overbright scales the flash (> 1 clips to white sooner). Works in URP (SRPDefaultUnlit) and the built-in pipeline.
Shader "Hellord/HitFlash1567"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _FlashColor ("Flash colour", Color) = (1, 1, 1, 1)
        _Flash ("Flash amount", Range(0, 1)) = 1
        _Overbright ("Overbright", Float) = 1.5
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
        Cull Off ZWrite Off ZTest Always Blend One One
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            sampler2D _MainTex; float4 _FlashColor; float _Flash, _Overbright;
            struct appdata { float4 vertex : POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; float4 col : COLOR; float2 uv : TEXCOORD0; };
            v2f vert (appdata v) { v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.col = v.color; o.uv = v.uv; return o; }
            float4 frag (v2f i) : SV_Target
            {
                float a = tex2D(_MainTex, i.uv).a; if (a < 0.5) discard;
                return float4(_FlashColor.rgb * (_Flash * i.col.a * _Overbright), 1);
            }
            ENDCG
        }
    }
}
