// t1567 Hellord AddGlow: plain additive sprite (light flash disc, pre-blurred v2 glow strips, overbright hit pass).
// out = tex.rgb * tex.a * vertexColor.rgb * vertexColor.a * _Tint.rgb * _Intensity, Blend One One (never darkens).
// _Intensity > 1 = overbright. _Bands > 0 quantises the result into that many steps with a 2x2 dither (pixel-art look; 0 = smooth).
// No LightMode tag: runs as SRPDefaultUnlit in URP (Unity 6) and as a normal pass in the built-in pipeline.
Shader "Hellord/AddGlow1567"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite / mask", 2D) = "white" {}
        _Tint ("Tint", Color) = (1, 1, 1, 1)
        _Intensity ("Intensity (overbright > 1)", Float) = 1
        _Bands ("Bands (0 = smooth)", Float) = 0
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
            sampler2D _MainTex; float4 _MainTex_TexelSize; float4 _Tint; float _Intensity, _Bands;
            struct appdata { float4 vertex : POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; float4 col : COLOR; float2 uv : TEXCOORD0; };
            v2f vert (appdata v) { v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.col = v.color; o.uv = v.uv; return o; }
            float4 frag (v2f i) : SV_Target
            {
                float4 t = tex2D(_MainTex, i.uv); float k = t.a * i.col.a;
                if (_Bands > 0.5)
                {
                    float2 px = floor(i.uv / max(_MainTex_TexelSize.xy, 1e-5)); float d = fmod(px.x + px.y, 2.0) < 0.5 ? 0.25 : 0.75;
                    k = floor(k * _Bands + d) / _Bands; if (k <= 0.0) discard;
                }
                return float4(t.rgb * i.col.rgb * _Tint.rgb * (k * _Intensity), 1);
            }
            ENDCG
        }
    }
}
