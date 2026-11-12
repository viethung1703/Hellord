// t928u Hellord PixelAoE: port of /workspace/fx_audit/shader_sample/shader_sample.py pixel_aoe() (curse veins, grave liquid v3)
// and fireball/fireball_shader.py burst().
// t930u: + shader_batch3 (batch3.py): merged grave pools (_Pools/_PoolCount), 3 Burning City hybrid, 5 Ghost Mushroom pool, 6 Death Bell, 7 Ghost Banner wisp. Pipeline-agnostic like HellordLight/Atmos (plain UnityCG, no LightMode tag).
// t1001u: Fireball mode-2 warm Endesga ramp (brown/red/orange/yellow/white) + heat bias / early flash / thinner soot lip — kills purple dither disc.
// Opaque output + clip(); no blending. Pixel coords are WORLD px (16 PPU), y flipped to the prototype's y-down so "far lip" = up on screen.
Shader "Hellord/PixelAoE"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite (unused, quad only)", 2D) = "white" {}
        _NoiseTex ("fbm R 128 (Repeat, Bilinear)", 2D) = "gray" {}
        _CellTex ("Voronoi R=F2-F1 G=id 128 (Repeat, Bilinear)", 2D) = "white" {}
        _RampTex ("Ramp Nx1 (Point, Clamp)", 2D) = "white" {}
        _RampCount ("Ramp colours", Float) = 6
        _Mode ("0 curse, 1 grave, 2 fireball, 3 burning city, 5 mushroom pool, 6 death bell, 7 ghost wisp, 8 fire wall", Float) = 0
        _PoolCount ("Merged pools (0 = single centre)", Float) = 0
        _Side ("City/bell: 0 back(ground) 1 front", Float) = 0
        _BandW ("City band half width px / ghost width px", Float) = 3.36
        _FlameH ("City band flame height px / ghost wave px", Float) = 6
        _Frame ("Bell frames since kill (30 fps)", Float) = 0
        _Fb ("Bell frames since flare (-1 none)", Float) = -1
        _TelF ("Bell telegraph frames", Float) = 18
        _Dir ("Ghost dir world xy", Vector) = (1,0,0,0)
        _WFreq ("Ghost wave freq", Float) = 0.3
        _Gain ("Ghost dissolve gain", Float) = 0.8
        _Center ("Centre world xy", Vector) = (0,0,0,0)
        _RadiusPx ("Radius px", Float) = 28.8
        _Squash ("Ground squash", Float) = 0.75
        _Grow ("Dissolve threshold", Float) = 1
        _Life ("Burst life 0..1", Float) = 0
        _Pulse ("Tick pulse", Float) = 0
        _CellPx ("Px per texel", Float) = 1.2
        _Flow ("Flow xy", Vector) = (0.02,0.012,0,0)
        _Scroll ("Scroll xy", Vector) = (0.02,0.01,0,0)
        _Warp ("Warp", Float) = 0.35
        _EdgeNoise ("Edge noise", Float) = 0.35
        _EdgeW ("Edge band (grave)", Float) = 0.16
        _VeinW ("Vein width", Float) = 0.11
        _NGain ("Grave depth noise gain", Float) = 2.2
        _Seed ("Seed", Float) = 0
        _TimeStep ("Stepped fps", Float) = 12
        _T0 ("Time offset", Float) = 0
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
            sampler2D _NoiseTex, _CellTex, _RampTex;
            float _PoolCount, _Side, _BandW, _FlameH, _Frame, _Fb, _TelF, _WFreq, _Gain; float4 _Dir; float4 _Pools[16];
            float _RampCount, _Mode, _RadiusPx, _Squash, _Grow, _Life, _Pulse, _CellPx, _Warp, _EdgeNoise, _EdgeW, _VeinW, _NGain, _Seed, _TimeStep, _T0, _PPU;
            float4 _Center, _Flow, _Scroll;
            struct appdata { float4 vertex : POSITION; };
            struct v2f { float4 pos : SV_POSITION; float2 wpx : TEXCOORD0; };
            v2f vert (appdata v) { v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.wpx = mul(unity_ObjectToWorld, v.vertex).xy * _PPU; return o; }
            float N1(float2 uv) { return tex2Dlod(_NoiseTex, float4(uv, 0, 0)).r; }
            float Bay(float2 p)
            {
                int x = (int)fmod(fmod(p.x, 4) + 4, 4), y = (int)fmod(fmod(p.y, 4) + 4, 4);
                float4 r0 = float4(0,8,2,10), r1 = float4(12,4,14,6), r2 = float4(3,11,1,9), r3 = float4(15,7,13,5);
                float4 r = y == 0 ? r0 : y == 1 ? r1 : y == 2 ? r2 : r3;
                return (x == 0 ? r.x : x == 1 ? r.y : x == 2 ? r.z : r.w) / 16.0;
            }
            float Hs(float a, float b, float s) { return frac(sin(a * 12.9898 + b * 78.233 + s) * 43758.5453); }
            float Cell(float2 uv) { return tex2Dlod(_CellTex, float4(uv, 0, 0)).r; }
            // batch3 liquid_field(): q = merged field (min d_i/R_i), qu = field 3 px above (far lip test), world-space uv
            float Liquid(float2 p, float q, float qu, float t, float d, bool mush)
            {
                float2 uv = p / (128.0 * _CellPx);
                float wu = N1(uv * 0.5 + float2(t * _Flow.x + 0.3, t * _Flow.y)) - 0.5;
                float wv = N1(uv * 0.5 + float2(-t * _Flow.y + 0.61, t * _Flow.x + 0.17)) - 0.5;
                float amp = _Warp * (1 + 0.5 * sin(t * 0.7)); float2 uw = uv + float2(wu, wv) * amp;
                float rim = q + (N1(uv * 2 + float2(0.45, 0.2)) - 0.5) * _EdgeNoise;
                if (rim >= 1) return -1;
                float n = N1(uw + t * _Scroll.xy);
                float depth = q + (n - 0.5) * _NGain * 0.25; float de = depth + d * 0.12;
                float idx = de < 0.22 ? 0 : (de < 0.62 ? 1 : 2);
                if (abs(n - 0.5) < 0.035 && depth > 0.3) idx = min(idx + 1, 3);
                bool band = rim > 1 - _EdgeW, far = qu > q + 0.02;
                if (band && far) idx = 4;
                if (band && far && rim > 1 - _EdgeW * 0.45) idx = 5;
                if (!mush)
                {   // bubbles on a world grid
                    float gx = floor(p.x / 7), gy = floor(p.y / 5), h = Hs(gx, gy, 0), ph = frac(t * 1.5 + h * 7);
                    float bd = max(abs(p.x - (gx + 0.3 + h * 0.4) * 7), abs(p.y - (gy + 0.5) * 5)); bool live = h > 0.55 && q < 0.8;
                    if (live && ph < 0.25 && bd < 1) idx = 2;
                    if (live && ph >= 0.25 && ph < 0.33 && abs(bd - 1.5) < 0.01) idx = 6;
                    if (live && ph >= 0.25 && ph < 0.33 && bd < 0.5) idx = 7;
                }
                else
                {   // mycelium threads (fine Voronoi edges) + spore specks drifting up 9 px/s
                    float e = Cell(uv * 2.2 + float2(0.31, 0.07));
                    if (e < 0.032 && q < 0.9) idx = Hs(floor(p.x / 3), floor(p.y / 3), 0) > 0.5 ? 6 : 5;
                    float gx = floor(p.x / 5), gy = floor((p.y + t * 9) / 5), h = Hs(gx, gy, 3.1);
                    float sx = (gx + 0.5 + 0.3 * sin(t * 2 + h * 6)) * 5, sy = (gy + 0.5) * 5 - t * 9;
                    if (h > 0.82 && abs(p.x - sx) < 0.6 && abs(p.y - sy) < 0.6) idx = 7;
                }
                return idx;
            }
            fixed4 frag (v2f i) : SV_Target
            {
                float2 w = floor(i.wpx) + 0.5;                     // 16 PPU snap
                float2 p = float2(w.x, -w.y);                      // prototype space (y down)
                float2 c = float2(_Center.x, -_Center.y) * _PPU;
                float t = floor((_Time.y + _T0) * _TimeStep) / _TimeStep;
                float d = Bay(p) - 0.5;
                float idx = -1;
                if ((_Mode > 0.5 && _Mode < 1.5 && _PoolCount > 0.5) || (_Mode > 4.5 && _Mode < 5.5))
                {   // MERGED pools (grave) / mushroom pool: one silhouette, no seams
                    float q = 1e3, qu = 1e3; int cnt = (int)max(1, _PoolCount);
                    for (int k = 0; k < 16; k++)
                    {
                        if (k >= cnt) break;
                        float2 pc = float2(_Pools[k].x, -_Pools[k].y) * _PPU; float R = max(_Pools[k].z, 0.5);
                        if (_PoolCount < 0.5) { pc = c; R = max(_RadiusPx * _Grow, 0.5); }
                        float2 dk = p - pc;
                        q = min(q, length(float2(dk.x, dk.y / _Squash)) / R);
                        qu = min(qu, length(float2(dk.x, (dk.y - 3) / _Squash)) / R);
                    }
                    idx = Liquid(p, q, qu, t, d, _Mode > 4.5);
                    if (idx < 0) discard;
                }
                else if (_Mode > 2.5 && _Mode < 3.5)
                {   // BURNING CITY hybrid: low shader ember band (H = _FlameH) + opt A hand tongues (20, depth-split by _Side)
                    bool back = _Side < 0.5; float R = _RadiusPx, sq = _Squash, Hh = max(1, _FlameH);
                    float heat = 0, side = 0;
                    for (int k = 0; k < 12; k++)
                    {
                        if (k >= (int)Hh) break;
                        float dx = p.x - c.x, dy = (p.y + k - c.y) / sq; float r = sqrt(dx * dx + dy * dy);
                        float h = saturate(1 - abs(r - R) / max(_BandW, 0.1)) * pow(1 - k / Hh, 1.2);
                        if (h > heat) { heat = h; side = dy; }
                    }
                    float2 u = p / 128.0;
                    float n = N1(float2(u.x * 3 + (N1(float2(u.x, u.y + t * 0.3)) - 0.5) * 0.4, u.y * 2.2 + t * 0.9));
                    float tongue = smoothstep(0.35, 0.7, N1(float2(u.x * 6 + 0.2, u.y * 1.5 + t * 1.6)));
                    float val = heat * (0.45 + 0.9 * (n - 0.3)) * (0.25 + 1.1 * tongue) + d * 0.12;
                    if (val > 0.16 && ((side < 0) == back)) { idx = clamp(floor((val - 0.16) / 0.17) + 1, 1, 5); if (val < 0.2) idx = 0; }
                    float f = t * 30, tl = -1;
                    for (int j = 0; j < 20; j++)
                    {
                        float a = j * 6.2832 / 20; if ((sin(a) < 0) != back) continue;
                        float bx = c.x + cos(a) * R, by = c.y + sin(a) * R * sq;
                        float hj = 9 + 5 * fmod(j * 7, 3) + 3 * sin(f * 0.5 + j);
                        float sdx = p.x - bx, sdy = (p.y - by - 1) / 0.4; if (sdx * sdx + sdy * sdy < 9) tl = max(tl, 0);
                        for (int L = 0; L < 5; L++)
                        {
                            float ww = 3.2 - L * 0.64, hh = hj - L * hj / 6; if (ww < 0.5 || hh < 1) break;
                            float yy = floor(by - L * 0.3 - p.y + 0.5); if (yy < 0 || yy > floor(hh)) continue;
                            float uu = yy / max(1, hh); float hwid = ww * (1 - pow(uu, 1.5)) * (1 + 0.2 * sin(yy * 0.9 + f * 1.9 + j));
                            float sway = sin(uu * 3 + f * 0.8 + j) * 1.5 * uu;
                            if (abs(p.x - (bx + sway)) <= floor(hwid) + (L == 0 ? 1 : 0) + 0.5) tl = max(tl, L);
                        }
                    }
                    if (tl >= 0) idx = tl;
                    if (idx < 0) discard;
                }
                else if (_Mode > 5.5 && _Mode < 6.5)
                {   // DEATH BELL: pale/bone toll rings rolling inward over the telegraph, true-radius ticks, then white gong shock
                    float dx = p.x - c.x, dy = (p.y - c.y) / _Squash; float q = sqrt(dx * dx + dy * dy) / _RadiusPx;
                    float2 u = p / 128.0; float k = floor(_Frame), fb = floor(_Fb), tel = max(1, _TelF);
                    if (_Side < 0.5 && k >= 0 && k < tel + 4)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            float rr = 1 - fmod(k + j * 6, tel) / tel, w = 0.05 + 0.03 * (1 - rr);
                            if (abs(q + (N1(float2(u.x * 3, u.y * 3 + t * 0.2)) - 0.5) * 0.06 - rr) < w && d > -0.5 + rr * 0.6 && q < 1) idx = rr > 0.5 ? 4 : 3;
                        }
                        float ang = fmod(degrees(atan2(p.y - c.y, p.x - c.x)) + 360, 30);
                        if (abs(q - 1) < 0.03 && ang < 6) idx = 2;
                    }
                    if (_Side > 0.5 && fb >= 0 && fb < 7)
                    {
                        float n = N1(float2(u.x * 2.5 + 0.2, u.y * 2.5 - t * 0.4)); float front = min(1, (fb + 1) / 3);
                        if (abs(q - front) < 0.07 + 0.02 * fb && n > fb / 8) idx = fb < 2 ? 5 : (fb < 4 ? 4 : 2);
                        if (q < front * 0.5 && n > 0.45 + fb * 0.08) idx = fb < 3 ? 5 : 3;
                    }
                    if (idx < 0) discard;
                }
                else if (_Mode > 7.5)
                {   // t948u FIRE WALL: Burning City ember band along a segment (centre c, dir _Dir, half-length _RadiusPx, half-thick _BandW)
                    // + hand tongues every ~7 px along it (height x _Gain x _Grow). Footprint = the gameplay segment exactly (no fill, no outline).
                    float2 dir = normalize(float2(_Dir.x, -_Dir.y) + 1e-5); float2 nr = float2(-dir.y, dir.x);
                    float L = max(_RadiusPx, 1), Hh = max(1, _FlameH), heat = 0, g = saturate(_Grow);
                    for (int k = 0; k < 12; k++)
                    {
                        if (k >= (int)Hh) break;
                        float2 r = float2(p.x - c.x, p.y + k - c.y); float al = dot(r, dir), la = dot(r, nr);
                        float ex = max(abs(al) - L, 0); float dd = sqrt(ex * ex + la * la);
                        float h = saturate(1 - dd / max(_BandW, 0.5)) * pow(1 - k / Hh, 1.2);
                        heat = max(heat, h);
                    }
                    heat *= g;
                    float2 u = p / 128.0;
                    float n = N1(float2(u.x * 3 + (N1(float2(u.x, u.y + t * 0.3)) - 0.5) * 0.4, u.y * 2.2 + t * 0.9));
                    float tongue = smoothstep(0.35, 0.7, N1(float2(u.x * 6 + 0.2, u.y * 1.5 + t * 1.6)));
                    float val = heat * (0.45 + 0.9 * (n - 0.3)) * (0.25 + 1.1 * tongue) + d * 0.12;
                    if (val > 0.16) { idx = clamp(floor((val - 0.16) / 0.17) + 1, 1, 5); if (val < 0.2) idx = 0; }
                    float f = t * 30, tl = -1; int nt = (int)clamp(floor(2 * L / 7), 2, 40);
                    for (int j = 0; j < 40; j++)
                    {
                        if (j >= nt) break;
                        float s01 = (j + 0.5) / nt; float2 b = c + dir * (s01 * 2 - 1) * L; float bx = b.x, by = b.y;
                        float hj = (9 + 5 * fmod(j * 7, 3) + 3 * sin(f * 0.5 + j)) * _Gain * g;
                        for (int L2 = 0; L2 < 5; L2++)
                        {
                            float ww = 3.2 - L2 * 0.64, hh = hj - L2 * hj / 6; if (ww < 0.5 || hh < 1) break;
                            float yy = floor(by - L2 * 0.3 - p.y + 0.5); if (yy < 0 || yy > floor(hh)) continue;
                            float uu = yy / max(1, hh); float hwid = ww * (1 - pow(uu, 1.5)) * (1 + 0.2 * sin(yy * 0.9 + f * 1.9 + j));
                            float sway = sin(uu * 3 + f * 0.8 + j) * 1.5 * uu;
                            if (abs(p.x - (bx + sway)) <= floor(hwid) + (L2 == 0 ? 1 : 0) + 0.5) tl = max(tl, L2);
                        }
                    }
                    if (tl >= 0) idx = tl;
                    if (idx < 0) discard;
                }
                else if (_Mode > 6.5 && _Mode < 7.5)
                {   // GHOST BANNER wisp: swimming bone-white ribbon tail + wobbly head + ink eyes
                    float2 dir = normalize(float2(_Dir.x, -_Dir.y) + 1e-5); float2 nr = float2(-dir.y, dir.x);
                    float2 r = p - c; float L = max(_RadiusPx, 1); float al = -dot(r, dir), la = dot(r, nr), s01 = al / L;
                    la -= _FlameH * sin(al * _WFreq - t * 14) * saturate(s01 * 1.5);
                    float nz = N1(float2(al / 40.0 - t * 0.8 + _Seed, la / 20.0 + 0.3));
                    float wd = _BandW * pow(max(1 - s01, 0), 0.8) + 0.3;
                    if (al > 0 && al < L && abs(la) < wd && nz + d * 0.15 > 0.25 + s01 * 0.7 * _Gain)
                        idx = floor(clamp((1 - s01) * 1.2 - abs(la) / (wd + 0.01) * 0.4 + (nz - 0.5) * 0.5, 0, 0.999) * 6);
                    float hd = length(float2(r.x, r.y * 1.15)); float hn = N1(float2(p.x / 30.0 + t * 0.5, p.y / 30.0));
                    if (abs(r.x) < 4.5 && abs(r.y) < 4.5 && hd + (hn - 0.5) * 1.5 < 3.4) idx = hd > 2.4 ? 4 : 5;
                    float2 fw = c + dir * 0.5;
                    for (int j = -1; j <= 1; j += 2) { float2 ey = fw + nr * j * 1.2 - float2(0, 0.5); if (all(floor(p) == floor(ey))) idx = 6; }
                    if (idx < 0) discard;
                }
                else if (_Mode > 1.5 && _Mode < 2.5)
                {   // FIREBALL burst (t1001u/t1002u): opaque warm fire ONLY — discard cool/dark samples so we never paint a filled soot/purple disc.
                    // Built-in + URP safe (no GrabPass). t988u centre-relative UV kept.
                    float2 dd = p - c; float q = length(dd) / _RadiusPx;
                    float2 uv = dd / 192.0;
                    float n = N1(uv + (N1(uv * 0.5 + float2(0, -t * 0.2)) - 0.5) * 1.2 + float2(0, t * 0.35));
                    n = saturate((n - 0.5) * 3 + 0.5);
                    float front = min(1, pow(max(_Life * 14.0 / 5.0, 1e-4), 0.6));
                    // dissolve sooner + require stronger noise → jagged fire cloud, not a solid bubble
                    if (!(q < front && q < 1 && n > _Life * 1.35 - 0.02)) discard;
                    float heat = (1 - q / max(front, 1e-3)) * (1 - _Life) * 1.8 + (n - 0.5) * 0.85 - _Life * 0.15 + 0.45;
                    float rc = max(2.0, _RampCount);
                    idx = floor(clamp(heat + d * 0.55 / 7.0, 0, 0.999) * rc);
                    if (idx > rc - 1) idx = rc - 1;
                    // early contact = white/yellow flash (hand-drawn rollout A)
                    if (_Life < 0.2 && q < 0.4) idx = (int)(rc - 1);
                    else if (_Life < 0.32 && q < 0.65) idx = max(idx, (int)(rc - 3));
                    // never paint dark soot/brown fill — holes instead of a translucent disc wash
                    if (idx < 3) discard; // ramp 0..2 unused for body; keep dred+ only (warm)
                    // late outer embers: sparse dred/red chips, not a soot ring
                    if (_Life > 0.55 && q > front - 0.1 && n > 0.62) idx = 3; // dorg ember flecks
                }
                else
                {
                    float dx = p.x - c.x, dy = (p.y - c.y) / _Squash; float q = sqrt(dx * dx + dy * dy) / _RadiusPx;
                    float2 uv = (p - c) / (128.0 * _CellPx);
                    float wu = N1(uv * 0.5 + float2(t * _Flow.x + 0.3, t * _Flow.y)) - 0.5;
                    float wv = N1(uv * 0.5 + float2(-t * _Flow.y + 0.61, t * _Flow.x + 0.17)) - 0.5;
                    float rim = q + (N1(uv * 2 + float2(0.45, 0.2)) - 0.5) * _EdgeNoise;
                    if (rim >= _Grow) discard;
                    if (_Mode < 0.5)
                    {   // CURSE: warped Voronoi crack veins, broken dark ground, no ring
                        float2 uw = uv + float2(wu, wv) * _Warp;
                        float e = tex2Dlod(_CellTex, float4(uw + float2(0.13, 0.29), 0, 0)).r;
                        float fade = 1 - smoothstep(0.55, 1.0, rim / _Grow);
                        float vw = _VeinW * (0.35 + 0.65 * fade);
                        bool crack = e < vw, core = e < vw * 0.45;
                        idx = core ? 3 : 2;
                        bool glow = (_Pulse * (1 - q) + 0.15 * (N1(uw * 3 + float2(0, -t * 0.05)) - 0.5) * 2) > 0.25;
                        if (core && glow) idx = 4;
                        if (core && _Pulse * (1 - q) > 0.5) idx = 5;
                        float g = N1(uv * 3 + float2(0.7, 0.1));
                        bool ground = (g + d * 0.08 < 0.42 - 0.25 * (1 - fade)) && !crack;
                        idx = crack ? idx : (ground ? (g < 0.3 ? 0 : 1) : -1);
                    }
                    else
                    {   // GRAVE v3: depth bands, flow lines, far-lip meniscus, popping bubbles
                        float amp = _Warp * (1 + 0.5 * sin(t * 0.7 + _Seed));
                        float2 uw = uv + float2(wu, wv) * amp;
                        float n = N1(uw + t * _Scroll.xy);
                        float depth = q + (n - 0.5) * _NGain * 0.25;
                        float de = depth + d * 0.12; idx = de < 0.22 ? 0 : (de < 0.62 ? 1 : 2);
                        if (abs(n - 0.5) < 0.035 && depth > 0.3) idx = min(idx + 1, 3);
                        bool band = rim > _Grow - _EdgeW, far = dy < -_RadiusPx * 0.25;
                        if (band && far) idx = 4;
                        if (band && far && rim > _Grow - _EdgeW * 0.45 && dy < -_RadiusPx * 0.55) idx = 5;
                        float gx = floor((p.x - c.x) / 7), gy = floor((p.y - c.y) / 5);
                        float h = frac(sin(gx * 12.9898 + gy * 78.233 + _Seed) * 43758.5453);
                        float ph = frac(t * 1.5 + h * 7);
                        float bx = c.x + (gx + 0.3 + h * 0.4) * 7, by = c.y + (gy + 0.5) * 5;
                        float bd = max(abs(p.x - bx), abs(p.y - by));
                        bool live = h > 0.55 && q < 0.8;
                        if (live && ph < 0.25 && bd < 1) idx = 2;
                        if (live && ph >= 0.25 && ph < 0.33 && abs(bd - 1.5) < 0.01) idx = 6;
                        if (live && ph >= 0.25 && ph < 0.33 && bd < 0.5) idx = 7;
                    }
                }
                clip(idx);
                return tex2Dlod(_RampTex, float4((idx + 0.5) / _RampCount, 0.5, 0, 0));
            }
            ENDCG
        }
    }
}
