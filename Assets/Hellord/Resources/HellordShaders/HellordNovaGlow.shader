// t1419 Hellord NovaGlow (Ice Nova burst): additive soft glow drawn from the rim-only glow-mask strip. BUILT-IN pipeline (the game is not URP).
// A few px of bloom: 3 rings of integer-texel taps around each mask pixel (so the glow stays on the pixel grid), quantised to 4 Endesga ice-blue
// steps (navy 124e89 / blue 0099db / cyan 2ce8f5 / pale c0cbdc) with a 2x2 checker dither between steps. Intensity per frame = SpriteRenderer.color.a (x _Gain); SpriteRenderer.color.r = centre-flash strength.
// Plain CGPROGRAM, sampler2D, no URP includes. Blend One One (additive). The mask sprite must be Point filtered.
Shader "Hellord/NovaGlow1419"
{
    Properties
    {
        [PerRendererData] _MainTex ("Glow mask (alpha = rim)", 2D) = "white" {}
        _Gain ("Gain", Float) = 1.6
        _Add ("Additive strength (keeps it out of white)", Float) = 0.5
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
            sampler2D _MainTex; float4 _MainTex_TexelSize; float _Gain, _Add;
            struct appdata { float4 vertex : POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; float4 col : COLOR; float2 uv : TEXCOORD0; float2 lp : TEXCOORD1; };
            // lp = pixel offset from the strip origin (sprite pivot = origin (56,46)): vertex units x 16 PPU
            v2f vert (appdata v) { v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.col = v.color; o.uv = v.uv; o.lp = v.vertex.xy * 16.0; return o; }
            float A(float2 uv, float2 o) { return tex2D(_MainTex, uv + o * _MainTex_TexelSize.xy).a; }
            fixed4 frag (v2f i) : SV_Target
            {
                float s = A(i.uv, float2(0, 0)) * 3.0;
                const float2 d1[8] = { float2(1,0), float2(-1,0), float2(0,1), float2(0,-1), float2(1,1), float2(-1,1), float2(1,-1), float2(-1,-1) };
                [unroll] for (int k = 0; k < 8; k++) s += A(i.uv, d1[k]) * 1.5 + A(i.uv, d1[k] * 2.0) * 1.0 + A(i.uv, d1[k] * 3.0) * 0.5;
                float g = saturate(s / 27.0 * _Gain) * i.col.a;
                // centre flash (frames 0-1; the mask has none): an ellipse disc on the ground origin, strength = vertex colour r (0..1), radius 8 + 10*r px
                float fl = i.col.r; if (fl > 0.01) { float rr = 8.0 + 10.0 * fl; float dd = length(float2(i.lp.x, i.lp.y / 0.6)); g = max(g, saturate(1.15 - dd / rr) * fl); }
                float2 px = floor(i.uv / _MainTex_TexelSize.xy); float dith = fmod(px.x + px.y, 2.0) < 0.5 ? 0.12 : 0.62;
                float lvl = floor(g * 4.0 + dith);
                if (lvl < 1.0) discard;
                float3 c = lvl < 2.0 ? float3(0.0706, 0.3059, 0.5373) : lvl < 3.0 ? float3(0.0, 0.6, 0.8588) : lvl < 4.0 ? float3(0.1725, 0.9098, 0.9608) : float3(0.7529, 0.7961, 0.8627);
                return fixed4(c * _Add, 1);
            }
            ENDCG
        }
    }
}
