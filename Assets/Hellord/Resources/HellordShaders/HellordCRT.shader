// v0.0.19 CRT pass. Pipeline-agnostic (no RenderPipeline tag, plain UnityCG), applied when the low-res world
// texture is upscaled in OnGUI (Graphics.DrawTexture), so it works in Built-in, URP and URP 2D alike and needs no renderer feature.
Shader "Hellord/CRT"
{
    Properties
    {
        _MainTex ("World", 2D) = "black" {}
        _SrcSize ("Source size (art px)", Vector) = (640,360,0,0)
        _Scan ("Scanline strength", Range(0,1)) = 0.32
        _Mask ("RGB mask strength", Range(0,1)) = 0.12
        _Curve ("Curvature", Range(0,0.3)) = 0.06
        _Vig ("Vignette", Range(0,1)) = 0.45
        _Chroma ("Chromatic offset (art px)", Range(0,1)) = 0.35
        _GlowAmt ("Bloom amount", Range(0,1)) = 0.3
        _Flicker ("Flicker", Range(0,0.1)) = 0.012
    }
    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Opaque" "IgnoreProjector"="True" "PreviewType"="Plane" }
        Cull Off ZWrite Off ZTest Always Blend Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            float _GlowAmt; sampler2D _MainTex, _GlowTex; float4 _MainTex_TexelSize;  float4 _SrcSize; float _Scan, _Mask, _Curve, _Vig, _Chroma, _Glow, _Flicker; float4 _FogCol, _PulseCol; float _FogAmt, _FogInner, _Haze, _AtmosT, _PulseAmt;
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; float4 sp : TEXCOORD1; };
            v2f vert (appdata v) { v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.uv = v.uv; o.sp = ComputeScreenPos(o.pos); return o; }
            float3 S(float2 uv) { return tex2D(_MainTex, uv).rgb; }
            float3 G(float2 uv) { return tex2D(_GlowTex, uv).rgb; }
            // v0.0.20b: exact port of HA's preview crt() (Art/previews/hero_styles/shortlist_14_15/art.py), in sRGB:
            // a = clip(a + 0.7*gauss(a, sigma 1.6 art px)); a *= 1.25; last of every 3 rows *0.5 (mean-matched at 2x); R +1px, B -1px; vignette.
            fixed4 frag (v2f i) : SV_Target
            {
                float2 k = 1.0 / max(fwidth(i.uv * _SrcSize.xy), 1e-4);
                float dx = fwidth(i.uv.x);
                float2 uvR = i.uv - float2(dx, 0), uvB = i.uv + float2(dx, 0);
                float2 sn = _SrcSize.xy;
                float3 c = float3(S((floor(uvR*sn)+0.5)/sn).r, S((floor(i.uv*sn)+0.5)/sn).g, S((floor(uvB*sn)+0.5)/sn).b);
                #if !defined(UNITY_COLORSPACE_GAMMA)
                c = LinearToGammaSpace(max(c, 0));
                #endif
                // v0.0.22 heat haze: slow (<0.5 Hz) sub-pixel wobble of the bilinear GLOW layer only, edges only (sharp pixels never move)
                float2 cc0 = i.uv * 2 - 1; float edgeE = smoothstep(_FogInner, 1.25, length(cc0));
                float2 hz = float2(sin(i.uv.y * sn.y * 0.11 + _AtmosT * 2.3), sin(i.uv.x * sn.x * 0.07 + _AtmosT * 1.7)) * (_Haze * edgeE / sn);
                float3 g = float3(G(uvR + hz).r, G(i.uv + hz).g, G(uvB + hz).b);   // glow RT already holds sRGB values
                float3 col = saturate(c + g * _GlowAmt) * 1.25;
                // v0.0.22 red edge fog (+ low-HP pulse, folded into _FogAmt by code). Dark ember red, screen edges only, centre untouched.
                col = lerp(col, _FogCol.rgb, saturate(_FogAmt) * edgeE * edgeE);
                // v0.0.23 low-HP pulse: own ember red, same edge-only mask (centre untouched), slow cosine set by code (<= 1.5 Hz)
                col = lerp(col, _PulseCol.rgb, saturate(_PulseAmt) * edgeE);   // mask edgeE (not squared): reads along the sides too
                float rowIn = frac(i.uv.y * sn.y) * k.y;
                float kr = max(1, round(k.y));
                float dark = saturate(1 - 0.5 * kr / 3.0);            // k=3 -> 0.5 exactly; k=2 -> 0.667 (same mean)
                if (rowIn < 1.0) col *= dark;
                float2 cc = i.uv * 2 - 1;
                col *= 1 - 0.25 * dot(cc, cc);
                return fixed4(saturate(col), 1);
            }
            ENDCG
        }
        Pass  // 1: separable Gaussian (sigma 1.6 art px, 9 taps), pass 1 also converts linear->sRGB
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment fb
            #include "UnityCG.cginc"
            sampler2D _MainTex; float4 _MainTex_TexelSize; float4 _BlurDir;
            fixed4 fb (v2f_img i) : SV_Target
            {
                float3 s = 0; float wsum = 0;
                for (int t = -4; t <= 4; t++) {
                    float w = exp(-t * t / (2.0 * 1.6 * 1.6));
                    float3 c = tex2D(_MainTex, i.uv + _BlurDir.xy * _MainTex_TexelSize.xy * t).rgb;
                    if (_BlurDir.z > 0.5) c = LinearToGammaSpace(max(c, 0));
                    s += c * w; wsum += w;
                }
                return fixed4(s / wsum, 1);
            }
            ENDCG
        }
    }
}
