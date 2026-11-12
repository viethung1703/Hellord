// t934u Hellord PixelFX934: VH-approved shader batch 5/6 modes, ported from /workspace/fx_audit/shader_batch6/PixelAoE.shader (URP draft)
// to the game's plain UnityCG style (game runs the Built-in pipeline). Palettes are hardcoded per mode (no ramp assets).
// 10 pillar tell(_Phase 0)/crust(_Phase 2)  11 Nezha wheel (_Side 0 back,1 front)  21 Nezha burn trail (capsule _A->_B)
// 13 split node  14 Tribulation scorch  15 Golden Crow v2 sun  17 gore burst  19 impact shards.  Opaque + clip, no blend.
Shader "Hellord/PixelFX934"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite (unused)", 2D) = "white" {}
        _NoiseTex ("fbm R 128", 2D) = "gray" {}
        _CellTex ("Voronoi F2-F1 128", 2D) = "white" {}
        _Mode ("Mode", Float) = 11
        _Center ("Centre world xy", Vector) = (0,0,0,0)
        _A ("Trail start world xy", Vector) = (0,0,0,0)
        _B ("Trail end world xy", Vector) = (0,0,0,0)
        _RadiusPx ("Radius px", Float) = 16
        _Squash ("Squash", Float) = 0.55
        _Life ("Life 0..1", Float) = 0
        _Grow ("Grow / sk", Float) = 1
        _Spin ("Spin rad", Float) = 0
        _Bias ("Bias", Float) = 0
        _Tick ("Tick / tier", Float) = 0
        _Phase ("Phase", Float) = 0
        _Side ("Side", Float) = 0
        _Seed ("Seed", Float) = 0
        _TW ("Trail width px", Float) = 9.6
        _ColH ("Column height px", Float) = 110
        _HW0 ("Column half width px", Float) = 10
        _WSc ("Column width scale", Float) = 1
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
            sampler2D _NoiseTex, _CellTex;
            float _Mode, _RadiusPx, _Squash, _Life, _Grow, _Spin, _Bias, _Tick, _Phase, _Side, _Seed, _TW, _TimeStep, _PPU, _ColH, _HW0, _WSc;
            float4 _Center, _A, _B;
            struct appdata { float4 vertex : POSITION; };
            struct v2f { float4 pos : SV_POSITION; float2 wpx : TEXCOORD0; };
            v2f vert (appdata v) { v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.wpx = mul(unity_ObjectToWorld, v.vertex).xy * _PPU; return o; }
            float N1(float2 uv) { return tex2Dlod(_NoiseTex, float4(uv, 0, 0)).r; }
            float Cl(float2 uv) { return tex2Dlod(_CellTex, float4(uv, 0, 0)).r; }
            float Fbm(float2 uv) { return N1(uv) * 0.65 + N1(uv * 2.03 + float2(0.37, 0.11)) * 0.35; }
            float Bay(float2 p)
            {
                int x = (int)fmod(fmod(p.x, 4) + 4, 4), y = (int)fmod(fmod(p.y, 4) + 4, 4);
                float4 r0 = float4(0,8,2,10), r1 = float4(12,4,14,6), r2 = float4(3,11,1,9), r3 = float4(15,7,13,5);
                float4 r = y == 0 ? r0 : y == 1 ? r1 : y == 2 ? r2 : r3;
                return (x == 0 ? r.x : x == 1 ? r.y : x == 2 ? r.z : r.w) / 16.0;
            }
            float3 H3(float h) { return float3(floor(h / 65536.0), fmod(floor(h / 256.0), 256.0), fmod(h, 256.0)) / 255.0; }
            float3 Pal(int m, int i)
            {
                // Ko 181425 Ck 2b1d24 Dk 3f2832 Fr be4a2f Or f77622 Am feae34 Gd fee761 Wh ffffff
                if (m == 10 || m == 15) { float3 r[7] = { H3(0x2b1d24), H3(0x3f2832), H3(0xbe4a2f), H3(0xf77622), H3(0xfeae34), H3(0xfee761), H3(0xffffff) }; return r[clamp(i,0,6)]; }
                if (m == 11) { float3 r[5] = { H3(0x3f2832), H3(0xbe4a2f), H3(0xf77622), H3(0xfeae34), H3(0xfee761) }; return r[clamp(i,0,4)]; }
                if (m == 21) { float3 r[6] = { H3(0x2b1d24), H3(0x3a3046), H3(0xa22633), H3(0xe43b44), H3(0xfee761), H3(0x181425) }; return r[clamp(i,0,5)]; }
                if (m == 13) { float3 r[4] = { H3(0x3f2832), H3(0xbe4a2f), H3(0xf77622), H3(0xffffff) }; return r[clamp(i,0,3)]; }
                if (m == 14) { float3 r[5] = { H3(0x2b1d24), H3(0x3f2832), H3(0x8b9bb4), H3(0x2ce8f5), H3(0xffffff) }; return r[clamp(i,0,4)]; }
                if (m == 17) { float3 r[3] = { H3(0xa22633), H3(0xe43b44), H3(0xead4aa) }; return r[clamp(i,0,2)]; }
                float3 s[4] = { H3(0xffffff), H3(0xfee761), H3(0xfeae34), H3(0xf77622) }; return s[clamp(i,0,3)];
            }
            fixed4 frag (v2f i) : SV_Target
            {
                float2 p = floor(i.wpx) + 0.5; float2 c = _Center.xy * _PPU;
                float t = floor(_Time.y * _TimeStep) / _TimeStep;
                float2 d = p - c; d.y = -d.y;                       // y-down like the prototypes
                float sq = max(0.05, _Squash); float dyS = d.y / sq; float q = length(float2(d.x, dyS)) / max(0.5, _RadiusPx);
                float dth = Bay(p) - 0.5; int m = (int)round(_Mode); float idx = -1;
                if (m == 21)                                         // NEZHA burn trail along the dash path (batch5 area(back))
                {
                    float2 a = _A.xy * _PPU, b = _B.xy * _PPU; float2 ab = b - a; float L = length(ab) + 1; float2 dn = ab / L; float2 pa = p - a;
                    float s = saturate(dot(pa, dn) / L); float dist = length(pa - dn * s * L);
                    float n = N1(p / 128.0 * 3 + float2(0, t * 0.15)); float cool = saturate(_Life);
                    float w = _TW * 0.5 * (1 - 0.35 * cool) * (0.75 + 0.5 * n); float bd = dth + 0.5;
                    if (!(dist < w && (n + bd * 0.25) > 0.2 + cool * 0.75)) discard;
                    float heat = (1 - dist / (_TW * 0.5)) * (1 - cool) * 1.6 + (n - 0.5) * 0.8 + sin(s * 30 - t * 9) * 0.15;
                    idx = heat > 1.05 ? 4 : heat > 0.75 ? 3 : heat > 0.45 ? 2 : heat > 0.2 ? 1 : 0;
                    if (dist > w - 1.2 && bd > 0.5) idx = 5;
                }
                else if (m == 11)                                    // NEZHA flat wheel (batch6 mode 11, approved). back half before actors, front after
                {
                    bool front = dyS >= 0; if (front != (_Side > 0.5)) discard;
                    float r = q; float ang = atan2(dyS, d.x) + _Spin;
                    float spoke = frac(ang / 6.2832 * 6); float nz = Fbm(float2(ang * 1.2, r * 3 - t));
                    bool body = (r > 0.7 && r < 1.0) || (r < 0.7 && spoke < 0.12 && r > 0.2) || r < 0.2;
                    if (!body || nz < 0.2) discard;
                    idx = clamp(floor(nz * 4 + (r > 0.9 ? -1 : 0) + dth * 0.3), 0, 4);
                }
                else if (m == 10)                                    // HELLFIRE PILLAR tell / crust (impact ring removed: PixelShock does the impact)
                {
                    float cell = Cl(float2(d.x, dyS) / 128.0 * 2.2 + float2(0.3, 0.1)); float nn = N1(p / 50.0);
                    if (_Phase < 0.5)
                    {
                        float u = _Life; float front = (1 - pow(1 - saturate(u / 0.7), 3)) * 1.05;
                        if (q + (nn - 0.5) * 0.3 < 0.5 + 0.45 * u && dth + 0.5 < 0.35 + 0.5 * u) idx = 0;
                        if (cell < 0.05 + 0.04 * (1 - q) && q < front) idx = 2 + floor(saturate(u * 1.3 - q * 0.7 + (nn - 0.5) * 0.4 + dth * 0.2) * 3.99);
                        if (u > 0.86 && q < 0.12) idx = 6;
                    }
                    else if (_Phase < 1.5)                           // COLUMN quad above centre: d.y = height px (world up)
                    {
                        float hy = -d.y, f = hy / max(1, _ColH); float w1 = N1(float2(p.x / 60.0 + 0.2, (hy - t * 140) / 90.0)) - 0.5;
                        float cx = 4.5 * sin(t * 4.1) * pow(saturate(f), 1.6) + w1 * 9 * f; float base = _HW0 * _WSc * pow(saturate(1 - f), 0.62) * (1 + 0.12 * (1 - f));
                        float dd = abs(d.x - cx) / max(0.8, base);
                        float a1 = N1(float2(p.x / 50.0, (hy - t * 160) / 70.0)), a2 = N1(float2(p.x / 26.0 + 0.4, (hy - t * 230) / 34.0));
                        float edge = dd + (a2 - 0.5) * 0.9 * (0.3 + f);
                        if (hy < 0 || f >= 1 || edge >= 1 || (f > 0.4 && a1 > 0.78 - 0.3 * (f - 0.4))) discard;
                        float heat = (1 - pow(saturate(dd), 1.5)) * (1 - 0.8 * f) + (a1 - 0.5) * 0.9 + (a2 - 0.5) * 0.35 + _Bias + dth * 0.16;
                        if (hy < 3 && dd < 0.35 && _Bias > -0.2) heat = 1;
                        idx = 1 + step(0.27, heat) + step(0.46, heat) + step(0.66, heat) + step(0.92, heat); if (heat < 0.1 || edge > 0.86) idx = 1;
                    }
                    else
                    {
                        if (!(q + (nn - 0.5) * 0.35 < 0.82 && dth + 0.5 < _Grow * 1.1)) discard;
                        idx = (cell < 0.06 + 0.05 * (1 - q)) ? clamp(floor(4 - _Life * 4 + (nn - 0.5) * 1.2), 0, 4) : ((nn > 0.6) ? 1 : 0);
                    }
                }
                else if (m == 13)                                    // SPLIT NODE (Nine-Forked Lie)
                {
                    float r = length(d) / max(0.5, _RadiusPx); float nz = Fbm(d / 8 + t * 0.8);
                    if (r + (nz - 0.5) * 0.6 > 1 - _Life * 0.6) discard;
                    idx = clamp(floor((1 - r) * 3 + nz + dth * 0.4), 0, 3);
                }
                else if (m == 14)                                    // TRIBULATION scorch + crawl (no ring; shock = PixelShock934)
                {
                    float cell = Cl(float2(d.x, dyS) / 96.0 + float2(0.17, 0.4)); float nn = N1(p / 40.0); float u = _Life;
                    bool crack = cell < 0.045 + 0.03 * (1 - q) && q < 0.95 && nn > 0.3;
                    if (!(crack || q < 0.18 * (1 - u))) discard;
                    idx = q < 0.18 * (1 - u) ? 4 : clamp(3 - floor(u * 3 + dth * 0.6), 0, 3);
                    if (crack && frac(cell * 40 - t * 3) < 0.2 && u < 0.6) idx = 3;
                }
                else if (m == 15)                                    // GOLDEN CROW v2 solar disc (no crow). _Bias>0 = pierce: outer disc fill whitens
                {
                    float2 dd = d; float2 dir = normalize(_A.xy + float2(1e-4, 0)); dd = float2(d.x * dir.x - d.y * dir.y, d.x * dir.y + d.y * dir.x); // travel -> +x
                    float r = length(dd) / max(0.5, _RadiusPx); float ang = atan2(dd.y, dd.x);
                    float2 pol = float2(ang / 6.2832 * 7 + r * 0.8 - t * 0.9, r * 2 - t * 1.6);
                    float corona = Fbm(pol) + Fbm(pol * 2.1 + 0.3) * 0.5;
                    float reach = 1 + 0.9 * corona * saturate(1.2 - abs(dd.y) / (_RadiusPx * 3)) + 0.6 * saturate(-dd.x / _RadiusPx) * corona;
                    if (r > reach) discard;
                    if (r < 1)
                    {
                        float gran = Fbm(p / 6 + t * 0.5);
                        idx = r > 0.86 ? 3 : (gran < 0.3 ? 3 : (gran > 0.7 || r < 0.4 ? 6 : 5));
                        if (_Bias > 0 && r > 0.75) idx = 6;
                    }
                    else { float h = 1 - (r - 1) / max(0.01, reach - 1) + dth * 0.25; idx = h > 0.8 ? 4 : h > 0.55 ? 3 : h > 0.3 ? 2 : h > 0.12 ? 1 : 0; }
                }
                else if (m == 17)                                    // GORE BURST: thrown Voronoi chunks (cells, no band)
                {
                    float2 cuv = float2(d.x, dyS) / (_RadiusPx * 0.6) - normalize(d + 1e-4) * _Life * 1.5;
                    float cell = Cl(cuv * 0.25 + _Seed); float spread = 0.25 + _Life * 0.9; float keep = N1(floor(cuv * 3) / 7 + _Seed);
                    if (q > spread || cell < 0.08 || keep < 0.35 + _Life * 0.5 + dth * 0.2) discard;
                    idx = keep > 0.85 ? 2 : (cell > 0.3 ? 1 : 0);
                }
                else                                                 // 19 IMPACT SHARDS (hit); _Tick = tier (1 crit). NO ring
                {
                    float ang = atan2(d.y, d.x); float nk = 6 + 4 * _Tick; float k = floor((ang + 3.1416) / 6.2832 * nk);
                    float h = frac(sin(k * 12.9898 + _Seed) * 43758.5); float r = length(d);
                    float len = (3 + 3 * _Tick + h * 3) * (1 - _Life); float off = _Life * (4 + 4 * _Tick);
                    float a0 = (k + 0.5) / nk * 6.2832 - 3.1416; float perp = abs(r * sin(ang - a0));
                    if (!(perp < 0.6 && r > off && r < off + len)) discard;
                    idx = min(3, floor(_Life * 4 + (r - off) / max(1, len) * 1.5));
                }
                clip(idx);
                return fixed4(Pal(m, (int)idx), 1);
            }
            ENDCG
        }
    }
}
