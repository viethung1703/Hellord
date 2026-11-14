// t1430 Hellord FrostGlow (Frostbolt, D2R Ice Bolt look): t1432: Derived from Hellord/NovaGlow1419 (HellordNovaGlow.shader), BUILT-IN pipeline, Blend One One.
// Differences: (1) pale/white ramp instead of the 4 ice blues (so the near-white comet is not tinted blue), with a cyan-white option for the hit strip (_Tint);
// (2) the mask carries GRADED alpha (0/40/80/130/190/255 with R2 dither) so the 3-ring tap sum already gives a soft falloff; (3) no centre-flash disc (hit strip has its own flash frames).
// Mask format: alpha channel only, same frame size and same pivot as the sprite strip, Point filtered, no mipmaps, sRGB off is fine (alpha only). Intensity per frame = SpriteRenderer.color.a.
Shader "Hellord/FrostGlow1430"
{
    Properties
    {
        [PerRendererData] _MainTex ("Glow mask (alpha)", 2D) = "white" {}
        _Gain ("Gain (tap-sum multiplier)", Float) = 1.6
        _Add ("Additive strength", Float) = 0.7
        _Tint ("Tint (flight/cast c0cbdc, hit aae8f0)", Color) = (0.7529, 0.7961, 0.8627, 1)
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
            sampler2D _MainTex; float4 _MainTex_TexelSize; float _Gain, _Add; fixed4 _Tint;
            struct appdata { float4 vertex : POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; float4 col : COLOR; float2 uv : TEXCOORD0; };
            v2f vert (appdata v) { v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.col = v.color; o.uv = v.uv; return o; }
            float A(float2 uv, float2 o) { return tex2D(_MainTex, uv + o * _MainTex_TexelSize.xy).a; }
            fixed4 frag (v2f i) : SV_Target
            {
                float s = A(i.uv, float2(0, 0)) * 3.0;
                const float2 d1[8] = { float2(1,0), float2(-1,0), float2(0,1), float2(0,-1), float2(1,1), float2(-1,1), float2(1,-1), float2(-1,-1) };
                [unroll] for (int k = 0; k < 8; k++) s += A(i.uv, d1[k]) * 1.5 + A(i.uv, d1[k] * 2.0) * 1.0 + A(i.uv, d1[k] * 3.0) * 0.5;
                float g = saturate(s / 27.0 * _Gain) * i.col.a;
                float2 px = floor(i.uv / _MainTex_TexelSize.xy); float dith = fmod(px.x + px.y, 2.0) < 0.5 ? 0.12 : 0.62;
                // 4 pale steps (levels 1..4 = 0.25/0.5/0.75/1.0 of the tint), 2x2 checker dither between them
                float lvl = floor(g * 4.0 + dith);
                if (lvl < 1.0) discard;
                return fixed4(_Tint.rgb * (min(lvl, 4.0) * 0.25) * _Add, 1);
            }
            ENDCG
        }
    }
}
