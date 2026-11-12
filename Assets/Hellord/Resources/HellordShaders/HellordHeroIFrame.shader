// t969u/t970u Hellord HeroIFrame: hero sprite shader used ONLY while the hero is invulnerable (replaces the HurtFX ring).
// Built-in pipeline, CGPROGRAM, pixel-snapped in sprite texel space, Endesga-32 palette output. Draws the sprite only (no overlay).
// _Mode 0 = A soul shimmer (dark ghost ramp + flowing noise + 1px pulsing ember inner outline)
// _Mode 1 = B dither dissolve flicker (Bayer 4x4 breaks the sprite apart and reforms it, ember/dark edge)
// _Mode 2 = C scanline / heat wobble (1-texel row offsets, dark scanlines, faint dark-red tint)
Shader "Hellord/HeroIFrame"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _Mode ("Mode 0=A 1=B 2=C", Float) = 0
        _K ("I-frame remaining 0..1", Float) = 1
        _TimeStep ("Stepped fps", Float) = 12
        _Strength ("Effect strength 0..1", Float) = 1
        _TexelPx ("t981u: 1/texture size (xy), set per renderer from C#", Vector) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
        Cull Off Lighting Off ZWrite Off Blend One OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex; float4 _MainTex_TexelSize; float4 _TexelPx; float _Mode, _K, _TimeStep, _Strength;
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; float4 color : COLOR; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; float4 color : COLOR; };
            v2f vert(appdata v) { v2f o; o.pos = UnityObjectToClipPos(v.vertex); /* t979u v0.0.86: no UnityPixelSnap - it snaps to _ScreenParams, which under URP 2D / the low-res RT is not the target size, so the hero quad jumped scale (read as a zoom-out). Same geometry as Sprites-Default now. */ o.uv = v.uv; o.color = v.color; return o; }
            float h21(float2 p) { p = frac(p * float2(123.34, 456.21)); p += dot(p, p + 45.32); return frac(p.x * p.y); }
            float vn(float2 p) { float2 i = floor(p), f = frac(p); f = f * f * (3 - 2 * f);
                return lerp(lerp(h21(i), h21(i + float2(1,0)), f.x), lerp(h21(i + float2(0,1)), h21(i + 1), f.x), f.y); }
            float bayer4(float2 p) { int2 q = int2(fmod(p, 4)); int i = q.x + q.y * 4;
                float m[16] = { 0,8,2,10, 12,4,14,6, 3,11,1,9, 15,7,13,5 }; return (m[i] + 0.5) / 16.0; }
            // Endesga-32 (sRGB hex) -> linear for the Linear colour space project
            float3 E(float r, float g, float b) { return GammaToLinearSpace(float3(r, g, b) / 255.0); }
            fixed4 frag(v2f i) : SV_Target
            {
                float2 ts = _TexelPx.x > 0 ? _TexelPx.xy : _MainTex_TexelSize.xy; /* t981u v0.0.87: _MainTex_TexelSize is not always filled for SpriteRenderers (URP / SRP batcher) - a wrong size snapped the UVs to a few huge texels and the hero drew as a bigger block */ float2 tx = floor(i.uv / ts);           // texel coords (pixel snap)
                float t = floor(_Time.y * _TimeStep) / _TimeStep;
                float2 uv = (tx + 0.5) * ts; int mode = (int)round(_Mode);
                if (mode == 2) { float off = round(sin(tx.y * 0.9 + t * 14.0) * 1.2 * _Strength); uv.x += off * ts.x; }
                fixed4 c = tex2D(_MainTex, uv) * i.color; if (c.a < 0.01) discard;
                float3 col = c.rgb; float a = c.a;
                float lum = dot(LinearToGammaSpace(saturate(c.rgb)), float3(0.299, 0.587, 0.114));
                if (mode == 0)
                {   // A: soul shimmer. Quantise to a dark ghost ramp, noise flowing upward through the sprite.
                    float n = vn(tx * 0.35 + float2(0, -t * 6.0)) * 0.6 + vn(tx * 0.9 + float2(t * 3.0, -t * 9.0)) * 0.4;
                    float v = saturate(lum * 0.75 + (n - 0.5) * 0.55);
                    float b = bayer4(tx); float q = floor(v * 4.0 + b * 0.9);
                    float3 g = q < 1 ? E(24,20,37) : q < 2 ? E(38,43,68) : q < 3 ? E(58,68,102) : q < 4 ? E(90,105,136) : E(139,155,180);
                    // 1px inner outline: opaque texel with a transparent 4-neighbour -> pulsing ember
                    float e = 0; e += tex2D(_MainTex, uv + float2(ts.x, 0)).a < 0.01; e += tex2D(_MainTex, uv - float2(ts.x, 0)).a < 0.01;
                    e += tex2D(_MainTex, uv + float2(0, ts.y)).a < 0.01; e += tex2D(_MainTex, uv - float2(0, ts.y)).a < 0.01;
                    float pulse = 0.5 + 0.5 * sin(t * 18.0);
                    float3 ember = pulse > 0.66 ? E(247,118,34) : pulse > 0.33 ? E(228,59,68) : E(162,38,51);
                    col = lerp(col, g, _Strength); if (e > 0) col = ember;
                }
                else if (mode == 1)
                {   // B: dither dissolve. Dissolve amount rises and falls (~4 flickers per 0.7 s), noise-broken.
                    float d = (0.5 - 0.5 * cos(t * 36.0)) * 0.75 * _Strength;
                    float n = vn(tx * 0.5 + t * 2.0) * 0.5 + bayer4(tx) * 0.5;
                    if (n < d) discard;
                    if (n < d + 0.12) col = (fmod(tx.x + tx.y, 2) < 1) ? E(228,59,68) : E(62,39,49);
                }
                else
                {   // C: heat wobble. Every other row darker, faint dark-red multiply, rows quantised.
                    float3 red = E(162,38,51); col = lerp(col, col * red * 2.2, 0.35 * _Strength);
                    if (fmod(tx.y + floor(t * 12.0), 2) < 1) col *= lerp(1, 0.55, _Strength);
                }
                return fixed4(col * a, a);
            }
            ENDCG
        }
    }
}
