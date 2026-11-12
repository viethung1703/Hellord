// Pixel FX Pack v1.3 - screen-space distortion effects for URP "Full Screen Pass Renderer Feature" (Unity 6 / URP 17).
// Effects: Screen Shockwave (radial distortion ring), Screen Chromatic Aberration, Heat Haze (in a screen rect).
// Driven by GLOBAL shader values set from C# (PixelScreenFX component), so one material serves the whole game.
// Setup: Renderer asset > Add Renderer Feature > Full Screen Pass Renderer Feature > Pass Material = PixelFX_ScreenFX.mat,
//        Requirements = Color. Injection Point = Before/After Rendering Post Processing.
// UNTESTED IN THE UNITY EDITOR at v1.3.0 (written against the documented URP 17 Blit.hlsl API, same as the CRT shader).
Shader "PixelFX/Screen FX (URP)"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        ZWrite Off Cull Off ZTest Always
        Pass
        {
            Name "PixelFX ScreenFX"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float4 _PFX_Shock;      // xy = centre (0..1 viewport), z = radius (0..~1.5, aspect-corrected), w = strength
            float  _PFX_ShockWidth; // ring thickness
            float  _PFX_Chroma;     // chromatic offset in screen pixels
            float4 _PFX_HeatRect;   // xy = min, zw = max (viewport 0..1)
            float  _PFX_Heat;       // heat haze amplitude in screen pixels
            float  _PFX_PixelSnap;  // >0: quantise offsets to this many screen pixels (keeps the pixel look)

            float4 Frag (Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;
                float2 px = _ScreenParams.zw - 1;
                float2 off = 0;

                if (_PFX_Shock.w > 0)
                {
                    float aspect = _ScreenParams.x / _ScreenParams.y;
                    float2 d = (uv - _PFX_Shock.xy) * float2(aspect, 1);
                    float dist = length(d);
                    float x = (dist - _PFX_Shock.z) / max(_PFX_ShockWidth, 1e-4);
                    float ring = saturate(1 - abs(x)) ;
                    off += normalize(d + 1e-5) / float2(aspect, 1) * ring * ring * _PFX_Shock.w * 0.03 * sign(-x + 1e-5);
                }
                if (_PFX_Heat > 0 && all(uv >= _PFX_HeatRect.xy) && all(uv <= _PFX_HeatRect.zw))
                {
                    float edge = saturate(min(uv.y - _PFX_HeatRect.y, _PFX_HeatRect.w - uv.y) * 20);
                    off.x += sin(uv.y * 90 + _Time.y * 8) * _PFX_Heat * px.x * edge;
                }
                if (_PFX_PixelSnap > 0) off = round(off / (px * _PFX_PixelSnap)) * px * _PFX_PixelSnap;
                uv += off;

                float3 col;
                if (_PFX_Chroma > 0)
                {
                    float2 dir = (uv - 0.5) * 2;
                    float2 co = dir * px * _PFX_Chroma;
                    col.r = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_PointClamp, uv + co).r;
                    col.g = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_PointClamp, uv).g;
                    col.b = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_PointClamp, uv - co).b;
                }
                else col = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_PointClamp, uv).rgb;
                return float4(col, 1);
            }
            ENDHLSL
        }
    }
}
