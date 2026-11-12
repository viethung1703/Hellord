// Pixel FX Pack - Sprite Uber shader (unlit). Works with URP 2D Renderer, URP Universal Renderer and Built-in.
// All effects are uniform-driven (no keywords), so nothing gets stripped from builds; every effect is off at default values.
// UNTESTED IN EDITOR at v1.0.0 - see Documentation/README.md "Known limits".
Shader "PixelFX/Sprite Uber"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [Header(Outline)]
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineSize ("Outline Size (px, 0=off)", Range(0,4)) = 0
        [Enum(Outer,0,Inner,1)] _OutlineInner ("Outline Mode", Float) = 0
        [Toggle] _OutlineDiagonal ("Outline Diagonals (8-way)", Float) = 0
        [Header(Hit Flash)]
        _FlashColor ("Flash Color", Color) = (1,1,1,1)
        _FlashAmount ("Flash Amount", Range(0,1)) = 0
        [Header(Dissolve)]
        _DissolveAmount ("Dissolve Amount", Range(0,1)) = 0
        _DissolvePixel ("Dissolve Cell (px)", Range(1,8)) = 1
        _DissolveEdgeColor ("Dissolve Edge Color", Color) = (1,0.5,0,1)
        _DissolveEdgeWidth ("Dissolve Edge Width", Range(0,0.2)) = 0.06
        _DissolveSeed ("Dissolve Seed", Float) = 0
        [Header(Palette Swap)]
        [NoScaleOffset] _PaletteTex ("Palette LUT (row0=source)", 2D) = "black" {}
        _PaletteRow ("Palette Row (0=off)", Float) = 0
        _PaletteRows ("Palette Rows In LUT", Float) = 2
        _PaletteColors ("Colors Per Row (<=16)", Float) = 4
        _PaletteTolerance ("Match Tolerance", Range(0,0.5)) = 0.02
        [Header(Silhouette)]
        _SilhouetteColor ("Silhouette Color", Color) = (0.2,0.6,1,0.6)
        _SilhouetteAmount ("Silhouette Amount", Range(0,1)) = 0
        [Header(Wave)]
        _WaveAmp ("Wave Amplitude (px)", Range(0,8)) = 0
        _WaveFreq ("Wave Frequency", Float) = 0.5
        _WaveSpeed ("Wave Speed", Float) = 4
        [Header(Shine)]
        _ShineColor ("Shine Color", Color) = (1,1,1,1)
        _ShinePos ("Shine Position (-0.5..1.5, off<-0.5)", Float) = -1
        _ShineWidth ("Shine Width", Range(0.01,0.5)) = 0.12
        _ShineSlant ("Shine Slant", Range(-2,2)) = 0.5
        [Header(Rim Light v1.2)]
        _RimColor ("Rim Color (a=strength)", Color) = (1,0.75,0.3,1)
        _RimAmount ("Rim Amount (0=off)", Range(0,1)) = 0
        _RimDir ("Rim Light Dir (texels; x=+1 light from right)", Vector) = (1,1,0,0)
        [Header(Emission v1.2)]
        _EmissionColor ("Emission Color", Color) = (1,1,1,1)
        _EmissionAmount ("Emission Amount (adds light; HDR-friendly)", Range(0,4)) = 0
        _DissolveEdgeGlow ("Dissolve Edge Glow (multiplier)", Range(1,8)) = 1
        [Header(v1.3 Wind Sway)]
        _WindAmount ("Wind Sway (world units at top, 0=off)", Range(0,0.5)) = 0
        _WindSpeed ("Wind Speed", Float) = 2
        [Header(v1.3 Glitch)]
        _GlitchAmount ("Glitch Amount", Range(0,1)) = 0
        [Header(v1.3 Motion Blur)]
        _MotionBlur ("Motion Blur (px, 0=off)", Range(0,8)) = 0
        _MotionBlurDir ("Motion Blur Dir", Vector) = (1,0,0,0)
        [Header(v1.3 Chromatic Aberration)]
        _ChromaAmount ("Chromatic Offset (px)", Range(0,4)) = 0
        [Header(v1.3 Hologram)]
        _HoloAmount ("Hologram Amount", Range(0,1)) = 0
        _HoloColor ("Hologram Color", Color) = (0.17,0.91,0.96,1)
        [Header(v1.3 Frozen)]
        _FrozenAmount ("Frozen Amount", Range(0,1)) = 0
        [Header(v1.3 Burn)]
        _BurnAmount ("Burn Amount (bottom-up)", Range(0,1)) = 0
        _BurnColor ("Burn Edge Color", Color) = (1,0.55,0.1,1)
        [Header(v1.3 Tint Pulse)]
        _PulseColor ("Pulse Color", Color) = (0.39,0.78,0.3,1)
        _PulseAmount ("Pulse Amount", Range(0,1)) = 0
        _PulseSpeed ("Pulse Speed", Float) = 6
        [Header(v1.3 Gradient Map)]
        _GradMapAmount ("Gradient Map Amount", Range(0,1)) = 0
        _GradA ("Gradient Dark", Color) = (0.1,0.08,0.15,1)
        _GradB ("Gradient Mid", Color) = (0.71,0.31,0.53,1)
        _GradC ("Gradient Light", Color) = (1,0.9,0.6,1)
        [Header(v1.3 Color Grades)]
        _GreyAmount ("Greyscale", Range(0,1)) = 0
        _SepiaAmount ("Sepia", Range(0,1)) = 0
        _NegativeAmount ("Negative", Range(0,1)) = 0
        _RainbowAmount ("Rainbow", Range(0,1)) = 0
        _RainbowSpeed ("Rainbow Speed", Float) = 1
        _HueShift ("Hue Shift (0..1)", Range(0,1)) = 0
        [Header(v1.3 Dither Fade)]
        _DitherFade ("Dither Fade (0=visible, 1=gone)", Range(0,1)) = 0
        [Header(Pixelate)]
        _PixelateSize ("Pixelate Block (px, 1=off)", Range(1,32)) = 1
        [Header(Drop Shadow)]
        _ShadowColor ("Shadow Color", Color) = (0,0,0,0.5)
        _ShadowOffset ("Shadow Offset (px; 0,0=off)", Vector) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.5
            #include "UnityCG.cginc"
            #include "PixelFXCommon.hlsl"

            struct appdata { float4 vertex : POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };

            sampler2D _MainTex; float4 _MainTex_TexelSize;
            float4 _Color;
            float4 _OutlineColor; float _OutlineSize; float _OutlineInner; float _OutlineDiagonal;
            float4 _FlashColor; float _FlashAmount;
            float _DissolveAmount, _DissolvePixel, _DissolveEdgeWidth, _DissolveSeed; float4 _DissolveEdgeColor;
            sampler2D _PaletteTex; float _PaletteRow, _PaletteRows, _PaletteColors, _PaletteTolerance;
            float4 _SilhouetteColor; float _SilhouetteAmount;
            float _WaveAmp, _WaveFreq, _WaveSpeed;
            float4 _ShineColor; float _ShinePos, _ShineWidth, _ShineSlant;
            float _PixelateSize;
            float4 _RimColor; float _RimAmount; float4 _RimDir;
            float _WindAmount, _WindSpeed, _GlitchAmount, _MotionBlur; float4 _MotionBlurDir; float _ChromaAmount;
            float _HoloAmount; float4 _HoloColor; float _FrozenAmount; float _BurnAmount; float4 _BurnColor;
            float4 _PulseColor; float _PulseAmount, _PulseSpeed; float _GradMapAmount; float4 _GradA, _GradB, _GradC;
            float _GreyAmount, _SepiaAmount, _NegativeAmount, _RainbowAmount, _RainbowSpeed, _HueShift, _DitherFade;
            float4 _EmissionColor; float _EmissionAmount; float _DissolveEdgeGlow;
            float4 _ShadowColor; float4 _ShadowOffset;

            v2f vert (appdata v)
            {
                v2f o;
                float4 vtx = v.vertex;
                if (_WindAmount > 0.0)
                {
                    float wx = mul(unity_ObjectToWorld, float4(0,0,0,1)).x;
                    vtx.x += sin(_Time.y * _WindSpeed + wx * 1.7) * _WindAmount * saturate(v.uv.y); // top moves, bottom pinned
                }
                o.pos = UnityObjectToClipPos(vtx);
                o.uv = v.uv;
                o.color = v.color * _Color;
                #ifdef PIXELSNAP_ON
                o.pos = UnityPixelSnap(o.pos);
                #endif
                return o;
            }

            // tex2Dlod: no gradients needed inside loops with break (avoids D3D "gradient in loop" errors); sprites have no mips.
            float4 Tex(float2 uv) { return tex2Dlod(_MainTex, float4(uv, 0, 0)); }

            float4 frag (v2f i) : SV_Target
            {
                float2 texel = _MainTex_TexelSize.xy;
                float2 uv = i.uv;

                // Pixelate: snap UV to blocks of N texels
                if (_PixelateSize > 1.0)
                    uv = PFX_SnapToTexel(uv, texel * floor(_PixelateSize));
                else
                    uv = PFX_SnapToTexel(uv, texel); // sample texel centers (pixel-perfect)

                // Wave: horizontal offset per texel row, quantised to whole texels
                if (_WaveAmp > 0.0)
                {
                    float row = floor(uv.y / texel.y);
                    float off = round(sin(row * _WaveFreq + _Time.y * _WaveSpeed) * _WaveAmp);
                    uv.x += off * texel.x;
                }

                // Glitch (v1.3): random horizontal row-block tears, re-rolled ~12x/s
                if (_GlitchAmount > 0.0)
                {
                    float blk = floor(uv.y / texel.y / 3.0), gt = floor(_Time.y * 12.0);
                    if (PFX_Hash21(float2(blk, gt)) < _GlitchAmount * 0.5)
                        uv.x += round((PFX_Hash21(float2(gt, blk + 7.0)) - 0.5) * 8.0 * _GlitchAmount) * texel.x;
                }

                float4 c = Tex(uv);
                // Motion blur (v1.3): 5 taps along a direction, whole-texel steps
                if (_MotionBlur > 0.0)
                {
                    float2 md = normalize(_MotionBlurDir.xy + 1e-5) * texel;
                    float4 acc = c;
                    [unroll] for (int mb = 1; mb <= 4; mb++) acc += Tex(uv - md * round(_MotionBlur * mb / 4.0)) * (1.0 - mb * 0.2);
                    c = acc / 3.0;
                }
                // Chromatic aberration (v1.3): split R and B by whole texels
                if (_ChromaAmount > 0.0)
                {
                    float2 co = float2(round(_ChromaAmount) * texel.x, 0);
                    float4 cr = Tex(uv + co), cb = Tex(uv - co);
                    c = float4(cr.r * cr.a > 0 ? cr.r : c.r * step(0.5, c.a), c.g, cb.a > 0 ? cb.b : c.b * step(0.5, c.a), max(c.a, max(cr.a, cb.a)));
                }

                // Palette swap (before tint so LUT matches authored colors)
                if (_PaletteRow > 0.5)
                    c.rgb = PFX_PaletteSwap(c.rgb, _PaletteTex, _PaletteRow, _PaletteRows, _PaletteColors, _PaletteTolerance);

                c *= i.color;
                float baseA = c.a;

                // ---- v1.3 colour effects ----
                float lum = dot(c.rgb, float3(0.299, 0.587, 0.114));
                if (_GreyAmount > 0.0) c.rgb = lerp(c.rgb, lum.xxx, _GreyAmount);
                if (_SepiaAmount > 0.0) c.rgb = lerp(c.rgb, float3(dot(c.rgb, float3(0.393,0.769,0.189)), dot(c.rgb, float3(0.349,0.686,0.168)), dot(c.rgb, float3(0.272,0.534,0.131))), _SepiaAmount);
                if (_GradMapAmount > 0.0)
                {
                    float3 gm = lum < 0.5 ? lerp(_GradA.rgb, _GradB.rgb, lum * 2) : lerp(_GradB.rgb, _GradC.rgb, lum * 2 - 1);
                    c.rgb = lerp(c.rgb, gm, _GradMapAmount);
                }
                if (_HueShift > 0.0) c.rgb = PFX_HueShift(c.rgb, _HueShift);
                if (_RainbowAmount > 0.0)
                {
                    float hue = frac((uv.x + uv.y) * 2.0 + _Time.y * _RainbowSpeed);
                    float3 rb = saturate(abs(frac(hue + float3(0, 2.0/3.0, 1.0/3.0)) * 6 - 3) - 1);
                    c.rgb = lerp(c.rgb, rb * (0.35 + lum), _RainbowAmount);
                }
                if (_NegativeAmount > 0.0) c.rgb = lerp(c.rgb, 1 - c.rgb, _NegativeAmount);
                if (_FrozenAmount > 0.0)
                {
                    float3 ice = lum < 0.5 ? lerp(float3(0.07,0.31,0.54), float3(0.17,0.91,0.96), lum * 2) : lerp(float3(0.17,0.91,0.96), float3(1,1,1), lum * 2 - 1);
                    float2 tp = floor(uv / texel);
                    float glint = step(0.985, PFX_Hash21(tp + floor(_Time.y * 3)));
                    c.rgb = lerp(c.rgb, ice + glint, _FrozenAmount);
                }
                if (_PulseAmount > 0.0) c.rgb = lerp(c.rgb, _PulseColor.rgb, _PulseAmount * (0.5 + 0.5 * sin(_Time.y * _PulseSpeed)));
                if (_HoloAmount > 0.0)
                {
                    float row = floor(uv.y / texel.y);
                    float scan = fmod(row + floor(_Time.y * 10), 4) < 2 ? 1.0 : 0.45;
                    float flick = 0.85 + 0.15 * step(0.5, PFX_Hash21(float2(floor(_Time.y * 20), 3)));
                    c.rgb = lerp(c.rgb, _HoloColor.rgb * (0.4 + lum * 1.2), _HoloAmount);
                    c.a *= lerp(1, scan * flick * 0.85, _HoloAmount);
                }

                // Outline
                if (_OutlineSize > 0.0)
                {
                    float n = 0.0;
                    int sz = (int)round(_OutlineSize);
                    [loop] for (int k = 1; k <= 4; k++)
                    {
                        if (k > sz) break;
                        float2 d = texel * k;
                        n = max(n, Tex(uv + float2(d.x, 0)).a);
                        n = max(n, Tex(uv - float2(d.x, 0)).a);
                        n = max(n, Tex(uv + float2(0, d.y)).a);
                        n = max(n, Tex(uv - float2(0, d.y)).a);
                        if (_OutlineDiagonal > 0.5)
                        {
                            n = max(n, Tex(uv + d).a);
                            n = max(n, Tex(uv - d).a);
                            n = max(n, Tex(uv + float2(d.x, -d.y)).a);
                            n = max(n, Tex(uv + float2(-d.x, d.y)).a);
                        }
                    }
                    if (_OutlineInner > 0.5)
                    {
                        // inner: opaque texel touching transparency
                        float m = 1.0;
                        [loop] for (int k2 = 1; k2 <= 4; k2++)
                        {
                            if (k2 > sz) break;
                            float2 d2 = texel * k2;
                            m = min(m, Tex(uv + float2(d2.x, 0)).a);
                            m = min(m, Tex(uv - float2(d2.x, 0)).a);
                            m = min(m, Tex(uv + float2(0, d2.y)).a);
                            m = min(m, Tex(uv - float2(0, d2.y)).a);
                        }
                        if (baseA > 0.5 && m < 0.5) c.rgb = _OutlineColor.rgb;
                    }
                    else if (baseA < 0.5 && n > 0.5)
                    {
                        c = _OutlineColor;
                        baseA = c.a;
                    }
                }

                // Drop shadow (behind sprite)
                if (abs(_ShadowOffset.x) + abs(_ShadowOffset.y) > 0.0 && baseA < 0.5)
                {
                    float sa = Tex(uv - round(_ShadowOffset.xy) * texel).a;
                    if (sa > 0.5) { c = _ShadowColor; baseA = c.a; }
                }

                // Hit flash
                c.rgb = lerp(c.rgb, _FlashColor.rgb, _FlashAmount * step(0.001, c.a));

                // Silhouette
                c.rgb = lerp(c.rgb, _SilhouetteColor.rgb, _SilhouetteAmount);
                c.a = lerp(c.a, c.a * _SilhouetteColor.a, _SilhouetteAmount);

                // Shine sweep (diagonal band in sprite UV space)
                if (_ShinePos > -0.5)
                {
                    float x = uv.x + uv.y * _ShineSlant * 0.5;
                    float band = step(abs(x - _ShinePos * (1 + abs(_ShineSlant) * 0.5)), _ShineWidth * 0.5);
                    c.rgb = lerp(c.rgb, _ShineColor.rgb, band * _ShineColor.a * step(0.5, baseA));
                }

                // Rim light (v1.2): opaque texel whose neighbour toward the light is empty gets lit
                if (_RimAmount > 0.0 && baseA > 0.5)
                {
                    float2 rd = round(_RimDir.xy);
                    float nx = abs(rd.x) > 0 ? Tex(uv + float2(rd.x * texel.x, 0)).a : 1;
                    float ny = abs(rd.y) > 0 ? Tex(uv + float2(0, rd.y * texel.y)).a : 1;
                    float rim = step(nx, 0.5) + step(ny, 0.5);
                    c.rgb = lerp(c.rgb, _RimColor.rgb, saturate(rim) * _RimAmount * _RimColor.a);
                }

                // Emission (v1.2): additive brighten; with HDR + Bloom (URP Volume) this makes sprites glow
                c.rgb += _EmissionColor.rgb * _EmissionAmount * step(0.001, c.a);

                // Dissolve (pixel cells)
                if (_DissolveAmount > 0.0)
                {
                    float2 cell = floor(uv / (texel * max(1.0, floor(_DissolvePixel))));
                    float h = PFX_Hash21(cell + _DissolveSeed);
                    if (h < _DissolveAmount) c = 0;
                    else if (h < _DissolveAmount + _DissolveEdgeWidth && _DissolveAmount < 1.0)
                        c.rgb = _DissolveEdgeColor.rgb * _DissolveEdgeGlow;
                }

                // Burn (v1.3): bottom-up, noisy front, charred band then glowing edge (assumes a non-atlased sprite, uv 0..1)
                if (_BurnAmount > 0.0)
                {
                    float2 cellB = floor(uv / texel);
                    float th = uv.y * 0.8 + PFX_Hash21(floor(cellB / 2)) * 0.2;
                    float f = _BurnAmount * 1.1;
                    if (th < f - 0.02) c = 0;
                    else if (th < f + 0.02) c.rgb = _BurnColor.rgb * 1.5;
                    else if (th < f + 0.1) c.rgb *= 0.3;
                }
                // Dither fade (v1.3): 4x4 Bayer pattern on texels, opaque pixels only (no blending)
                if (_DitherFade > 0.0)
                {
                    float2 tp2 = fmod(floor(uv / texel), 4);
                    if (PFX_Bayer4(tp2) < _DitherFade) c = 0;
                }

                // straight alpha -> premultiplied (Blend One OneMinusSrcAlpha)
                return float4(c.rgb * c.a, c.a);
            }
            ENDHLSL
        }
    }
    Fallback "Sprites/Default"
}
