// HellordLava1332.shader (t1350): steady-flow pixel lava for the Pacts INSIDE screen strip (mockup t1332/t1345). Plain CG vert/frag (no URP includes) so it runs under IMGUI via Graphics.Blit.
// One continuous layer: constant-speed advection, no cross-fade / reset / pulse. Plates = elongated static Worley cells stretched along the flow; 8-colour ramp; 4x4 Bayer dither; pixel-snapped.
// _MainTex = pm_bg (640x360, Point). _DataTex = pm_lavadata (Point, sRGB OFF, no compression, no mips): R = strip mask, G = signed distance, BA = unit flow dir.
// _Clock (s) is fed by HellordPactMenu.cs. Output is converted back to the active colour space.
Shader "Hidden/HellordLava1332"
{
    Properties
    {
        _MainTex ("Background (640x360, Point)", 2D) = "white" {}
        _DataTex ("Lava data", 2D) = "black" {}
        _ScreenRes ("Pixel grid", Vector) = (640, 360, 0, 0)
        _Clock ("Clock (s)", Float) = 0
        _Speed ("Flow speed (px/s)", Float) = 5
        _LenAlong ("Plate length (px)", Float) = 46
        _LenAcross ("Plate width (px)", Float) = 15
        _Intensity ("Heat intensity", Range(0.6, 1.2)) = 1
        _Dither ("Dither strength", Range(0, 1)) = 0.2
        _EmberOn ("Embers", Range(0, 1)) = 1
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always Blend Off
        Pass
        {
            Name "Lava1332"
            CGPROGRAM
            
            #pragma vertex vert_img
            #pragma fragment frag
            #pragma target 3.5
            #include "UnityCG.cginc"

            sampler2D _MainTex; sampler2D _DataTex;
            float4 _ScreenRes; float _Clock, _Speed, _LenAlong, _LenAcross, _Intensity, _Dither, _EmberOn;

            static const float3 RAMP[8] = {
                float3(31,24,41)/255.0, float3(62,39,49)/255.0, float3(110,28,42)/255.0, float3(190,38,51)/255.0,
                float3(228,59,68)/255.0, float3(247,118,34)/255.0, float3(254,174,52)/255.0, float3(254,231,97)/255.0 };
            static const float BAYER4[16] = {0,8,2,10, 12,4,14,6, 3,11,1,9, 15,7,13,5};

            uint pcg(uint v) { uint s = v * 747796405u + 2891336453u; uint w = ((s >> ((s >> 28u) + 4u)) ^ s) * 277803737u; return (w >> 22u) ^ w; }
            uint h3(float ix, float iy, uint seed) { return pcg(uint(int(ix)) + pcg(uint(int(iy)) + pcg(seed))); }
            float r01(uint h) { return float(h & 0xFFFFFFu) / 16777216.0; }
            float q5(float t) { return t*t*t*(t*(t*6.0-15.0)+10.0); }
            float vnoise(float2 p, uint seed)
            {
                float2 i = floor(p), f = p - i;
                float a = r01(h3(i.x, i.y, seed)), b = r01(h3(i.x+1, i.y, seed));
                float c = r01(h3(i.x, i.y+1, seed)), d = r01(h3(i.x+1, i.y+1, seed));
                float ux = q5(f.x), uy = q5(f.y);
                float ab = a + (b-a)*ux, cd = c + (d-c)*ux;
                return ab + (cd-ab)*uy;
            }
            float fbm3(float2 p, uint seed)
            {
                float s = 0, a = 0.5, n = 0;
                for (int o = 0; o < 3; o++) { s += a * vnoise(p, seed + uint(o)*7u); n += a; p = p*2.03 + float2(11.7, 3.1); a *= 0.5; }
                return s / n;
            }
            float sm(float a, float b, float x) { float t = saturate((x-a)/(b-a)); return t*t*(3.0-2.0*t); }

            // static feature points: plates only translate with the flow
            void worley(float2 p, out float f1, out float f2, out float id1)
            {
                float2 ip = floor(p), fp = p - ip;
                f1 = 9; f2 = 9; id1 = 0;
                for (int j = -1; j <= 1; j++) for (int i = -1; i <= 1; i++)
                {
                    uint hh = h3(ip.x + i, ip.y + j, 101u);
                    float rx = r01(hh), ry = r01(pcg(hh+1u)), rs = r01(pcg(hh+3u));
                    float d = length(float2(i + 0.15 + 0.7*rx - fp.x, j + 0.15 + 0.7*ry - fp.y));
                    if (d < f1) { f2 = f1; f1 = d; id1 = rs; } else { f2 = min(f2, d); }
                }
            }

            float4 LoadD(float2 pTop)   // PNG row 0 is the top, texture v=1 is the top
            {
                float2 ip = float2(clamp(floor(pTop.x), 0, _ScreenRes.x-1), clamp(floor(pTop.y), 0, _ScreenRes.y-1));
                return tex2Dlod(_DataTex, float4((ip.x + 0.5)/_ScreenRes.x, 1.0 - (ip.y + 0.5)/_ScreenRes.y, 0, 0));
            }

            float heat(float2 P, float t, float2 dir, float sd)
            {
                float2 perp = float2(-dir.y, dir.x);
                float pd = dot(P, dir), ac = dot(P, perp);
                float al = pd - _Speed*t;                       // monotonic downstream scroll
                float aw = pd - 0.55*_Speed*t;                  // warp field drifts at a different speed
                float qa = vnoise(float2(aw/32.0, ac/14.0), 11u) - 0.5;
                float qc = vnoise(float2(aw/32.0 + 9.2, ac/14.0 + 3.1), 13u) - 0.5;
                float a2 = al + qa*26.0, c2 = ac + qc*13.0;
                float f1, f2, rid;
                worley(float2(a2/_LenAlong, c2/_LenAcross), f1, f2, rid);
                float edge = (f2 - f1) * sqrt(_LenAlong*_LenAcross);
                float cov = 0.26 + 0.42*(1.0 - sm(0, 6, sd)) + (vnoise(float2(a2/96.0, c2/32.0), 5u) - 0.5)*0.7;
                float tau = saturate(cov - rid*0.8);
                float gap = (0.9 + (1.0 - sm(0.0, 0.5, tau))*6.0) * (0.8 + 0.4*vnoise(float2(a2/70.0, c2/24.0), 3u));
                float mott = sm(0.3, 0.75, fbm3(float2(a2/90.0, c2/8.0), 21u));
                float Hm = 0.50 + 0.30*mott + 0.08*sm(0, 10, sd);
                float Hgap = Hm * (0.82 + 0.18*(1.0 - edge/gap));
                if (gap < 2.4) Hgap = max(Hgap, 0.88 + 0.12*(1.0 - edge/gap));
                float Hp = 0.07 + 0.11*rid + 0.05*vnoise(float2(a2/7.0, c2/7.0), 31u);
                float rim = 1.0 - sm(0, 4.0, edge - gap);
                Hp += 0.27*rim*rim;
                float H = (edge < gap) ? Hgap : Hp;
                if (sd < 1.6) H = min(H, 0.13);
                if (sd < 4.0) H *= 0.7 + 0.3*sm(1.6, 4.0, sd);
                return saturate(H * _Intensity);
            }

            float3 emberPass(float3 col, float2 P, float t)
            {
                const float CE = 26.0, LIFE = 3.2;
                for (int j = 0; j < 3; j++)
                {
                    float cx = floor(P.x/CE), cy = floor(P.y/CE) + j;
                    uint hh = h3(cx, cy, 900u);
                    float r0 = r01(hh), r1 = r01(pcg(hh+1u)), r2 = r01(pcg(hh+2u));
                    float age = frac(t/LIFE + r2);
                    float2 s = float2((cx + r0)*CE, (cy + r1)*CE);
                    bool ok = LoadD(s).r > 0.78 && r1 < 0.55;
                    float ex = floor(s.x + sin(age*9.0 + r0*6.28)*3.0), ey = floor(s.y - age*34.0);
                    if (ok && ex == floor(P.x) && ey == floor(P.y) && age < 0.9)
                        col = age < 0.45 ? RAMP[6] : (age < 0.75 ? RAMP[5] : RAMP[4]);
                }
                return col;
            }

            fixed4 frag(v2f_img i) : SV_Target
            {
                float2 res = _ScreenRes.xy;
                float2 pi_ = floor(float2(i.uv.x, 1.0 - i.uv.y) * res);
                float2 P = pi_ + 0.5;                                      // pixel-snapped
                float t = _Clock;
                float4 D = LoadD(P);
                bool strip = D.r > 0.78;
                float sd = (D.g*255.0 - 128.0)/127.0*12.0;
                float2 dir = float2(D.b*255.0 - 128.0, D.a*255.0 - 128.0)/127.0;
                dir = dir / max(length(dir), 1e-4);
                float bay = BAYER4[(int(pi_.y)&3)*4 + (int(pi_.x)&3)]/16.0 - 0.5;

                float3 col = tex2Dlod(_MainTex, float4((pi_.x+0.5)/res.x, 1.0 - (pi_.y+0.5)/res.y, 0, 0)).rgb;
                #ifndef UNITY_COLORSPACE_GAMMA
                col = LinearToGammaSpace(col);                              // palette maths in sRGB
                #endif

                // steady banded glow on the rock beside the strip
                float dist = -sd;
                float spill = pow(saturate(1.0 - dist/10.0), 1.6) * 0.8;
                if (!strip && dist < 10.0 && spill > (bay + 0.5)*0.9)
                    col = lerp(col, float3(150,44,40)/255.0, 0.35);

                if (strip)
                {
                    float H = heat(P, t, dir, sd);
                    int idx = int(clamp(floor(H*7.0 + 0.5 + bay*_Dither), 0, 7));
                    col = RAMP[idx];
                }
                if (_EmberOn > 0.5) col = emberPass(col, P, t);
                #ifndef UNITY_COLORSPACE_GAMMA
                col = GammaToLinearSpace(col);
                #endif
                return fixed4(col, 1);
            }
            ENDCG
        }
    }
}
