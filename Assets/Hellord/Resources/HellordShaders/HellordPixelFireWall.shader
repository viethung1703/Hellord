// t948 Hellord PixelFireWall: 1:1 port of /workspace/fx_audit/shader_batch7/fw.py new_fx() (VH-approved Fire Wall mock-up).
// One quad per wall segment (HellordFireWall948 already builds one SpriteRenderer + material per segment).
// Opaque palette output + clip(), no blending; same conventions as Hellord/PixelAoE (CGPROGRAM, pipeline-agnostic).
// t957u: wired in v0.0.81 (HellordFireWall948.cs).
// Pixel space: world px at 16 PPU, snapped floor()+0.5, then y flipped to the prototype's y-down image space.
Shader "Hellord/PixelFireWall"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite (unused, quad only)", 2D) = "white" {}
        _NoiseTex ("fbm R 128 (Repeat, Bilinear, no mips) = fx928_noise128", 2D) = "gray" {}
        _RampTex ("Ramp 7x1 (Point, Clamp): ink,dred,dorg,org,yel,white,scorch", 2D) = "white" {}
        _RampCount ("Ramp colours", Float) = 7
        _Center ("Segment centre world xy (u)", Vector) = (0,0,0,0)
        _Dir ("Segment dir world xy (unit)", Vector) = (1,0,0,0)
        _RadiusPx ("Segment half length px", Float) = 40
        _BandW ("Footprint half thickness px (thick*0.5*PPU)", Float) = 4.8
        _FlameH ("Tongue base height px (fw.py 22)", Float) = 22
        _Gain ("Tongue extra height, x18 px (fw.py 16 px => 0.889)", Float) = 0.889
        _Grow ("Envelope g 0..1 (0.2s in, 0.35s out; set by code)", Float) = 1
        _Scorch ("Scorch fill 0..1 = min(1, age/0.133s); set by code, stays 1 in fade", Float) = 1
        _Seed ("Seed", Float) = 0
        _T0 ("Time offset s", Float) = 0
        _TimeStep ("Stepped fps", Float) = 12
        _PPU ("PPU", Float) = 16
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="TransparentCutout" "IgnoreProjector"="True" "PreviewType"="Plane" }
        Cull Off ZWrite Off ZTest Always Blend Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            sampler2D _NoiseTex, _RampTex;
            float4 _Center, _Dir;
            float _RampCount, _RadiusPx, _BandW, _FlameH, _Gain, _Grow, _Scorch, _Seed, _T0, _TimeStep, _PPU;
            struct appdata { float4 vertex : POSITION; };
            struct v2f { float4 pos : SV_POSITION; float2 wpx : TEXCOORD0; };
            v2f vert (appdata v) { v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.wpx = mul(unity_ObjectToWorld, v.vertex).xy * _PPU; return o; }
            float N1(float u, float v) { return tex2Dlod(_NoiseTex, float4(u, v, 0, 0)).r; }      // == S.tex2D (bilinear wrap)
            float Bay(float2 p)                                                                 // B2.bayer: BAYER[y%4,x%4] - 0.5
            {
                int x = (int)fmod(fmod(floor(p.x), 4) + 4, 4), y = (int)fmod(fmod(floor(p.y), 4) + 4, 4);
                float4 r0 = float4(0,8,2,10), r1 = float4(12,4,14,6), r2 = float4(3,11,1,9), r3 = float4(15,7,13,5);
                float4 r = y == 0 ? r0 : y == 1 ? r1 : y == 2 ? r2 : r3;
                return (x == 0 ? r.x : x == 1 ? r.y : x == 2 ? r.z : r.w) / 16.0 - 0.5;
            }
            float SegD(float2 q, float2 a, float2 b) { float2 ab = b - a; float t = saturate(dot(q - a, ab) / max(1e-4, dot(ab, ab))); return length(q - a - t * ab); }
            half4 Ramp(int i) { return tex2Dlod(_RampTex, float4((i + 0.5) / _RampCount, 0.5, 0, 0)); }
            half4 frag (v2f i) : SV_Target
            {
                float2 w = floor(i.wpx) + 0.5;
                float2 p = float2(w.x, -w.y);                                                   // image space (y down), like fw.py xx,yy
                float2 c = float2(_Center.x, -_Center.y) * _PPU, dn = normalize(float2(_Dir.x, -_Dir.y));
                float2 a = c - dn * _RadiusPx, b = c + dn * _RadiusPx;
                float t = floor((_Time.y + _T0) * _TimeStep) / _TimeStep;                       // S.tstep
                float g = _Grow, th2 = _BandW, u = p.x / 128.0, v = p.y / 128.0, bd = Bay(p);
                int idx = -1;
                // --- scorched ground band (fw.py: 3 dithered rings of the noisy distance; sg = min(1,k/4) -> _Scorch)
                float d0 = SegD(p, a, b);
                float sc = d0 + (N1(u * 9 + _Seed, v * 9) - 0.5) * 6;
                float sg = _Scorch;
                if (sc < th2 + 3 && bd + 0.5 < 0.55 * sg + 0.15) idx = 0;
                if (sc < th2 + 1 && bd + 0.5 < 0.80 * sg + 0.15) idx = 6;
                if (sc < th2 - 1 && bd + 0.5 < 1.00 * sg + 0.15) idx = 0;
                // --- flame columns standing on the footprint kk px below (fw.py heat loop)
                float colh = _FlameH + 18 * _Gain * smoothstep(0.25, 0.75, N1(p.x / 70.0 + _Seed, t * 0.55)) * (0.75 + 0.25 * sin(p.x * 0.9 + t * 11));
                float hc = colh * g + 1e-3, heat = 0;
                int hmax = (int)ceil(_FlameH + 18 * _Gain);
                [loop] for (int kk = 0; kk < 48; kk++)
                {
                    if (kk >= hmax || kk > hc) break;                                         // later terms are 0
                    float foot = saturate(1 - SegD(p + float2(0, kk), a, b) / (th2 + 0.5));
                    heat = max(heat, foot * pow(saturate(1 - kk / hc), 0.55));
                }
                if (heat > 0)
                {
                    float warp = (N1(u * 2 + t * 0.2, v * 2 - t * 0.6) - 0.5) * 0.5;
                    float n = N1(u * 5 + warp, v * 3 + t * 1.5);                                // rising flow noise
                    float lick = smoothstep(0.3, 0.72, N1(u * 14 + warp * 0.6 + _Seed, v * 2.2 + t * 2.3));   // narrow tongues
                    float val = heat * (0.5 + (n - 0.3)) * (0.45 + 0.9 * lick) + bd * 0.12;
                    if (val > 0.16) idx = val < 0.2 ? 0 : clamp((int)floor((val - 0.16) / 0.15) + 1, 1, 5);   // banded Endesga ramp
                }
                clip((float)idx);                                                                    // nothing here: floor shows through
                return Ramp(idx);
            }
            ENDCG
        }
    }
}
