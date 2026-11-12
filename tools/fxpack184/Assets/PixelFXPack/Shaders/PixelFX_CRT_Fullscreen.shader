// Pixel FX Pack - CRT / scanline full-screen effect for URP "Full Screen Pass Renderer Feature" (URP 14+ / Unity 6).
// Setup: Renderer asset > Add Renderer Feature > Full Screen Pass Renderer Feature > Pass Material = a material using this shader.
// UNTESTED IN EDITOR at v1.0.0.
Shader "PixelFX/CRT Fullscreen (URP)"
{
    Properties
    {
        _ScanlineStrength ("Scanline Strength", Range(0,1)) = 0.35
        _ScanlineCount ("Scanline Count", Float) = 240
        _Curvature ("Curvature", Range(0,0.3)) = 0.08
        _Vignette ("Vignette", Range(0,1)) = 0.35
        _Chromatic ("Chromatic Offset (px)", Range(0,4)) = 1
        _MaskStrength ("RGB Mask Strength", Range(0,1)) = 0.15
        _Flicker ("Flicker", Range(0,0.1)) = 0.015
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        ZWrite Off Cull Off ZTest Always
        Pass
        {
            Name "PixelFX CRT"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float _ScanlineStrength, _ScanlineCount, _Curvature, _Vignette, _Chromatic, _MaskStrength, _Flicker;

            float4 Frag (Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;
                float2 cc = uv * 2 - 1;
                cc *= 1 + _Curvature * dot(cc, cc) * float2(0.25, 0.35);
                uv = cc * 0.5 + 0.5;
                if (any(uv < 0) || any(uv > 1)) return float4(0,0,0,1);

                float2 px = _ScreenParams.zw - 1; // 1/width, 1/height
                float r = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_PointClamp, uv + float2(px.x * _Chromatic, 0)).r;
                float g = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_PointClamp, uv).g;
                float b = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_PointClamp, uv - float2(px.x * _Chromatic, 0)).b;
                float3 col = float3(r, g, b);

                float scan = 0.5 + 0.5 * sin(uv.y * _ScanlineCount * 6.2831853);
                col *= lerp(1, scan, _ScanlineStrength);

                int m = (int)fmod(input.positionCS.x, 3);
                float3 mask = m == 0 ? float3(1, 0.7, 0.7) : (m == 1 ? float3(0.7, 1, 0.7) : float3(0.7, 0.7, 1));
                col *= lerp(1, mask, _MaskStrength);

                float vig = saturate(1 - dot(cc, cc) * 0.5 * _Vignette * 2);
                col *= vig;
                col *= 1 - _Flicker * (0.5 + 0.5 * sin(_Time.y * 60));
                return float4(col, 1);
            }
            ENDHLSL
        }
    }
}
