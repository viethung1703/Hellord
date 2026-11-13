// Hellord PixelIce (v3 DRAFT, RESTRAINED) -- 1:1 HLSL port plan of /workspace/fx_audit/ice_skills_v3/ice_skills_v3.py (numpy mock-up). Function names match the python ones.
// v3 vs v2: no white, ~1/3 density; COLUMNS = a few small spikes (gap .91, hmax <= 8); PLATES darker (cyan veins); ORB 3 streaks; SCOUR + SHELL removed; new SHARD mode (Diablo 2 Frozen Armor shards); SPRITE: no ice block (tint + rim + chips).
// t1409u: integrated (Resources/HellordShaders, loaded by HellordIceFX3.cs). NOT YET SEEN IN UNITY. Mode 5 samples the sprite by its LOCAL pixel (rect + pivot + world pos) so it works on sheet sprites and needs no padded mesh.
// Pipeline-agnostic CGPROGRAM + UnityCG (same as HellordPixelFrostCleave / AoE): no GrabPass, no LightMode, opaque Endesga colours + clip(), no blending.
// One quad per effect, size set by the C# hook. P = world pixel (16 PPU), snapped to the pixel centre, y flipped to the mock-up's image space (y down).
// Textures (import: sRGB OFF, Point NOT used -> Bilinear, Repeat, no mipmaps, no compression):
//   _NoiseTex  = code/textures/ice_noise128.png   (R = value noise, same as NOISE in the .py)
//   _CellTex   = code/textures/ice_cell10_128.png (R = voronoi edge distance, G = cell id, 10x10 cells per tile, same as CELLB / CELLBID)
// _Mode: 0 PLATES (sparse frost veins: nova/bomb floor, chill patches) | 1 COLUMNS (a few small spikes: nova front, bomb pulse band + detonation, orb burst) | 2 ORB (frostbolt)
//        3 SHARD (one 4x7 ice shard of the Frozen-Armor orbit; centre = _Ctr) | 5 SPRITE (_SpTex = enemy/hero sprite texture: chill 1/2 tint+rim, freeze tint+rim+chips)
Shader "Hellord/PixelIce"
{
    Properties
    {
        [PerRendererData] _MainTex ("Unused (SpriteRenderer quad)", 2D) = "white" {}
        _SpTex ("Mode 5: the enemy / hero sprite texture (set per quad)", 2D) = "white" {}
        _NoiseTex ("Noise 128", 2D) = "gray" {}
        _CellTex ("Cell 128 (R edge, G id)", 2D) = "black" {}
        _Mode ("Mode", Float) = 0
        _Ctr ("Centre world u (xy)", Vector) = (0,0,0,0)
        _R ("Radius px", Float) = 40
        _Reach ("Frost reach px (Mode 0)", Float) = 40
        _Melt ("Melt threshold 0..1 (whole plates vanish)", Float) = 0
        _Bright ("Plate brightness lift (bomb wind-up flicker)", Float) = 0
        _BareT ("Bare-vein plate fraction", Float) = 0.26
        _Seed ("Seed", Float) = 0
        _T ("Effect time in FRAMES @30fps (the shader steps it to 12 fps)", Float) = 0
        _Dis ("Dissolve 0..1 (whole cells vanish)", Float) = 0
        _Amp ("Column amplitude", Float) = 1
        _HMax ("Column max height px", Float) = 16
        _TFront ("Column front duration frames (11.4 nova / 8 bomb / 4 burst)", Float) = 11.4
        _Gap ("Column gap threshold", Float) = 0.70
        _RIn ("Column inner radius fraction", Float) = 0.24
        _Band ("Pulse band: x = radius px (-1 = off), y = half width px", Vector) = (-1,7,0,0)
        _Dir ("Orb direction (image space)", Vector) = (1,0,0,0)
        _Pop ("Orb pop scale", Float) = 1
        _Foot ("Sprite feet world u (xy), Mode 5 chips", Vector) = (0,0,0,0)
        _Half ("Sprite half width px", Float) = 12
        _Level ("Sprite: 1 chill L1, 2 chill L2, 3 freeze, 4 hero rim", Float) = 0
        _SpRect ("Mode 5: sprite rect in the texture, px (x,y,w,h), y up", Vector) = (0,0,32,32)
        _SpPiv ("Mode 5: sprite pivot px", Vector) = (16,0,0,0)
        _SpPos ("Mode 5: sprite transform world pos u (xy)", Vector) = (0,0,0,0)
        _SpTexSize ("Mode 5: sprite texture size px", Vector) = (32,32,0,0)
        _SpFlip ("Mode 5: sprite flipX (0/1)", Float) = 0
        _Sq ("Ground squash", Float) = 0.75
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
            #pragma target 3.5
            #include "UnityCG.cginc"
            sampler2D _NoiseTex, _CellTex, _SpTex;
            float4 _Ctr, _Band, _Dir, _Foot, _SpRect, _SpPiv, _SpPos, _SpTexSize;
            float _Mode, _R, _Reach, _Melt, _Bright, _BareT, _Seed, _T, _Dis, _Amp, _HMax, _TFront, _Gap, _RIn, _Pop, _Half, _Level, _Sq, _PPU, _SpFlip;
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; float2 wpx : TEXCOORD0; float2 uv : TEXCOORD1; };
            v2f vert (appdata v) { v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.wpx = mul(unity_ObjectToWorld, v.vertex).xy * _PPU; o.uv = v.uv; return o; }

            // ---- Endesga 32 subset (sRGB hex; if the project is Linear colour space convert with GammaToLinearSpace) ----
            static const float3 INK = float3(24,20,37)/255, NAVY = float3(18,78,137)/255, BLUE = float3(0,153,219)/255, CYAN = float3(44,232,245)/255, PALE = float3(192,203,220)/255, WHITE = float3(1,1,1);
            static const float TAU = 6.2831853;
            float N(float2 uv) { return tex2Dlod(_NoiseTex, float4(uv, 0, 0)).r; }                         // = tex(u,v)
            float2 Cell(float2 uv) { return tex2Dlod(_CellTex, float4(uv, 0, 0)).rg; }                      // = (CELLB, CELLBID)
            float hash1(float x) { return frac(sin(x * 127.1 + 311.7) * 43758.5453); }
            static const float BAYER[16] = { 0,8,2,10, 12,4,14,6, 3,11,1,9, 15,7,13,5 };
            float bay(float2 p) { int x = (int)p.x & 3, y = (int)p.y & 3; return BAYER[y * 4 + x] / 16.0; }
            float tstep(float f) { return floor(f / 30.0 * 12.0) / 12.0; }                                  // shader time stepped to 12 fps
            float3 ramp7(float v, float2 P, float amp, float3 a, float3 b, float3 c, float3 d, float3 e, float3 f, float3 g)
            { float k = saturate(v + (bay(P) - 0.5) * amp) * 0.999 * 7.0; int i = (int)k; return i < 1 ? a : i < 2 ? b : i < 3 ? c : i < 4 ? d : i < 5 ? e : i < 6 ? f : g; }
            float3 quant5(float v, float3 a, float3 b, float3 c, float3 d, float3 e) { int i = (int)(saturate(v) * 0.999 * 5.0); return i < 1 ? a : i < 2 ? b : i < 3 ? c : i < 4 ? d : e; }

            // ----- PLATES (sh_plates / sh_plate_rgb / sh_frost_field) -----
            void Plates(float2 P, float seed, out float ed, out float pid, out float ed1)
            {
                const float PL_S = 84.0;
                float u = P.x / PL_S + seed * .137 + (N(P / 61.0) - .5) * .03, v = P.y / _Sq / PL_S + seed * .071;
                float2 a = Cell(float2(u, v)); ed = a.x; pid = a.y; ed1 = Cell(float2(u + 1.0 / PL_S, v + 1.0 / (PL_S * _Sq))).x;
            }
            bool FrostField(float2 P, float2 C, float R, float reach, float melt, float seed, float bright, float bareT, out float3 col)
            {
                col = 0; if (reach < 3) return false;
                float dx = P.x - C.x, dy = (P.y - C.y) / _Sq, r = sqrt(dx * dx + dy * dy), ang = atan2(dy, dx);
                float ed, pid, ed1; Plates(P, seed, ed, pid, ed1);
                float lump = N(float2(cos(ang) * .21 + seed * .19, sin(ang) * .21 + .3)) - .5;
                float arrive = r + (pid - .5) * reach * .42 + lump * reach * .30;           // frost CREEPS: plate appears when arrive < reach
                if (arrive >= reach) return false;
                if ((hash1(pid * 91.7 + seed) + (r / max(R, 1.0)) * .35) * .75 < melt) return false;   // melt: whole plate vanishes
                if (pid < bareT) { if (ed >= .09) return false; col = N(P / 6.0) > .5 ? CYAN : BLUE; return true; }   // bare veins: net of seams, floor shows through
                float n = N(P / 17.0 + seed), v = .05 + pid * .40 + (n - .5) * .30 + bright;                 // v3: darker plates
                col = ramp7(v, P, .16, NAVY, NAVY, BLUE, BLUE, CYAN, CYAN, CYAN);                      // v3: no PALE/WHITE
                bool seam = ed < .13, lit = ed1 > ed;
                if (seam) col = lit ? CYAN : INK;
                else if (pid > .35 && abs(N(float2(ang / TAU * 7. + seed, r / 31.)) - .5) < .022) col = (((int)P.x + (int)P.y) % 3 == 0) ? CYAN : BLUE;   // radial fracture lines (v3: no sparkle pixels)
                return true;
            }

            // ----- COLUMNS (sh_columns) : per-pixel lift loop, nearest/tallest wins. hmax <= 24 (loop is [loop] over z) -----
            bool Columns(float2 P, float2 C, float R, float kf, float seed, float dis, float amp, float hmax, float T, float gap, float rin, float2 band, out float3 col)
            {
                col = 0; if (kf < 0 || dis >= 1) return false; const float S = 80.0;
                [loop] for (int zi = (int)hmax; zi >= 0; zi--)
                {
                    float z = zi, yv = P.y + z, dx = P.x - C.x, dy = (yv - C.y) / _Sq, r = sqrt(dx * dx + dy * dy) + 1e-3, ang = atan2(dy, dx);
                    float u = P.x / S + seed * .11, v = yv / _Sq / S + seed * .07;
                    float2 a = Cell(float2(u, v)); float ed = a.x, pid = a.y, ed1 = Cell(float2(u + 1.0 / S, v + 1.0 / (S * _Sq))).x;
                    float clump = N(float2(ang / TAU * 4. + seed * .31, .4 + seed * .1)) - .5, hid = hash1(pid * 53.1 + seed);
                    bool present = (hid + clump * .55 > gap) && r < R * (.97 + .05 * (hid - .5)) && r > R * rin;
                    float te = T * (1 - sqrt(saturate(1 - r / R))) + hid * 1.6, age = kf - te, up = saturate((age + .6) / 2.4); up *= 1 + .12 * (age > 1 && age < 3.5 ? 1 : 0);
                    if (band.x >= 0) { float bw = band.y * (.55 + .6 * hid); present = (hid + clump * .55 > gap) && abs(r - band.x) < bw && r > R * rin; up = pow(saturate(1 - abs(r - band.x) / bw), .7); }
                    float hc = hmax * (.28 + .72 * pow(hash1(pid * 7.7 + seed * 3.), 1.5)) * amp * up * (.65 + .35 * pow(saturate(r / R), .6));
                    bool gone = hash1(pid * 13.3 + seed) < dis;
                    bool vis = present && !gone && hc >= max(z, 2.0) && (ed > .05 || zi == 0);
                    if (!vis) continue;
                    bool lit = ed1 > ed;
                    if (hc - z < 1.5)                                                                           // top face
                    {
                        col = quant5(.45 + pid * .5 + (N(float2(P.x / 5., yv / 5.)) - .5) * .3, CYAN, CYAN, PALE, PALE, PALE);   // v3: no WHITE
                        if (ed < .16) col = lit ? PALE : (ed < .08 ? BLUE : CYAN);
                    }
                    else                                                                                         // side face
                    {
                        float sd = saturate((hc - z) / max(hc, 1.0));
                        col = quant5(.30 + (1 - sd) * .38 + pid * .15 - (sd > .9 ? .2 : 0), NAVY, BLUE, BLUE, CYAN, CYAN);
                        if (ed < .2) col = lit ? CYAN : (ed < .06 ? INK : NAVY);
                        if (zi == 0) col = (((int)(P.x + P.y)) & 1) == 0 ? INK : NAVY;                           // contact shadow
                    }
                    return true;
                }
                return false;
            }

            // ----- ORB (sh_orb): jagged polar body + 5 shard streaks fanned behind the motion -----
            bool Orb(float2 P, float2 O, float2 d, float R0, float seed, float pop, float t, out float3 col)
            {
                col = 0; float R = R0 * pop, L = 30.0; float2 r2 = P - O - .5; float q = length(r2) + 1e-3, th = atan2(r2.y, r2.x);
                float rt = R * clamp(.82 + (N(float2(th / TAU * 3. + seed, t * 1.3)) - .5) * .9, .62, 1.18);
                float along = -dot(r2, d), perp = -r2.x * d.y + r2.y * d.x; bool shard = false; float3 sc = 0;
                [unroll] for (int j = 0; j < 3; j++)                                                              // v3: 3 streaks
                {
                    float off = (j - 1) * .34, ca = cos(off), sa = sin(off), s_ = along * ca - perp * sa, n_ = along * sa + perp * ca;
                    float Lj = 10 + 6 * frac(sin(j * 12.9 + seed * 3.3) * 4375.5), w = (3.0 - 2.7 * saturate(s_ / Lj)) * (j == 1 ? 1.0 : .7);
                    float p = saturate((s_ - R * .45) / Lj);
                    bool m = s_ > R * .45 && s_ < Lj + R * .45 && abs(n_) < w * .5 + .35 && bay(P) < 1.0 - p * .8;
                    if (m && !shard) { sc = p < .22 ? WHITE : p < .5 ? PALE : p < .78 ? CYAN : BLUE; shard = true; }
                }
                if (q < rt)
                {
                    float v = saturate(1.08 - (q / rt) * 1.05 + (N(float2(r2.x / 5. + seed, r2.y / 5. - t * 1.5)) - .5) * .5);
                    col = ramp7(v, P, .1, BLUE, BLUE, CYAN, CYAN, CYAN, PALE, WHITE);
                    if (abs(frac(th / TAU * 5 + t * 1.2) - .5) < .06 && q > rt * .25 && q < rt * .85) col = q < rt * .6 ? WHITE : CYAN;
                    return true;
                }
                if (q < rt + 1.3 && !(shard && along > R * .8)) { col = INK; return true; }
                if (shard) { col = sc; return true; }
                return false;
            }

            // ----- SHARD (Diablo 2 Frozen Armor idea): one 4x7 crystal, 1 px ink outline; C# moves the quad along the orbit (see IceFX3.Armor). Pattern = hero-local px offsets. -----
            static const int SHARDT[28] = { 0,1,0,0,  0,1,2,0,  2,1,2,3,  2,2,2,3,  3,2,3,4,  0,3,4,0,  0,4,0,0 };
            bool Shard(float2 P, float2 C, out float3 col)
            {
                col = 0; int2 q = (int2)floor(P - C) + int2(2, 3);                                          // 0..3 x 0..6 inside the crystal, quad is 8x10 px
                if (q.x < -1 || q.x > 4 || q.y < -1 || q.y > 7) return false;
                // rows y=0..6, 4 columns; 0 empty, 1 PALE, 2 CYAN, 3 BLUE, 4 NAVY (same table as the python shard_draw)
                if (q.x >= 0 && q.x < 4 && q.y >= 0 && q.y < 7)
                {
                    int t = SHARDT[q.y * 4 + q.x]; if (t != 0) { col = t == 1 ? PALE : t == 2 ? CYAN : t == 3 ? BLUE : NAVY; return true; }
                }
                col = INK; return true;                                                                       // outline ring around the crystal
            }

            // ----- SPRITE (sh_chill_sprite / sh_freeze_sprite / hero_rim): tint through the ice ramp by the sprite's OWN luminance, sparse 1 px rim, chips. Never writes over a sprite pixel. -----
            // _SpTex + _SpRect/_SpPiv/_SpPos locate the sprite; the quad is padded 8 px by the C# side (rim / chips). NO block, NO fill: demons stay readable.
            float Lum(float3 c) { return dot(c, float3(.299, .587, .114)); }
            float2 SpLocal(float2 P) { float2 l = float2(P.x, -P.y) - _SpPos.xy * _PPU + _SpPiv.xy; if (_SpFlip > .5) l.x = 2.0 * _SpPiv.x - l.x; return l; }                // image-space pixel centre -> sprite-local texel (origin bottom-left)
            float4 SpTex(float2 l) { if (l.x < 0 || l.y < 0 || l.x >= _SpRect.z || l.y >= _SpRect.w) return 0; return tex2Dlod(_SpTex, float4((_SpRect.xy + l) / _SpTexSize.xy, 0, 0)); }
            bool Chip(float2 P, float2 base, float ht, float wd, out float3 col)                         // chip(x0,y0,ht,wd) of the python: tapering needle
            {
                col = 0; float k = base.y - P.y; if (k < 0 || k >= ht) return false; float ww = max(1.0, wd - floor(k / 2.0)); if (abs(P.x - base.x + .5) > ww * .5 + .01) return false;
                col = k >= ht - 2 ? PALE : (k >= 1 ? CYAN : BLUE); return true;
            }
            bool SpriteIce(float2 P, float level, float seed, out float3 col)
            {
                col = 0; float2 l = SpLocal(P); float4 s = SpTex(l); bool a = s.a > .5;
                float n1 = max(max(SpTex(l + float2(1, 0)).a, SpTex(l - float2(1, 0)).a), max(SpTex(l + float2(0, 1)).a, SpTex(l - float2(0, 1)).a));
                float2 F = float2(_Foot.x, -_Foot.y) * _PPU;                                                // feet px, image space
                if (level > 3.5)                                                                             // HERO RIM (Frozen Armor): ~30 % of the edge pixels, re-rolled every 4 frames
                { if (!a && n1 > .5 && N(P / 1.9 + seed + floor(_T / 4.0) * .37 + 2.1) > .70) { col = (((int)P.x + (int)P.y) & 1) == 0 ? CYAN : PALE; return true; } return false; }
                if (level < 2.5)                                                                             // CHILL: dark tint (L1 ~40 % of pixels, L2 ~80 %), sparse rim, L2 icicles in the C# padding
                {
                    if (a)
                    {
                        float cover = level > 1.5 ? .82 : .42;
                        if (N(P / 7. + seed * .7 + float2(0, seed * .3)) < cover) { col = quant5(Lum(s.rgb) * 1.1, INK, NAVY, BLUE, CYAN, PALE); return true; }
                        return false;                                                                    // untouched pixels stay the sprite's own (keeps its tint / flash)
                    }
                    if (n1 > .5 && N(P / 2.3 + seed + float2(0, 3.1)) > (level > 1.5 ? .46 : .64)) { col = (((int)P.x + (int)P.y) & 1) == 0 ? CYAN : BLUE; return true; }
                    return false;
                }
                // FREEZE: whole body re-lit LIGHTER than chill (still by its own luminance), near-complete rim, 6 chips. No block.
                if (a) { col = quant5(Lum(s.rgb) * 1.2 + .12, INK, NAVY, BLUE, CYAN, PALE); return true; }
                if (n1 > .5 && N(P / 2.1 + seed) > .16) { col = ((((int)P.x + (int)P.y) % 5) == 0) ? PALE : CYAN; return true; }
                float h = _Half;
                if (Chip(P, F + float2(-9, 2), 4, 2, col) || Chip(P, F + float2(1, 2), 5, 2, col) || Chip(P, F + float2(9, 2), 4, 2, col)) return true;           // feet
                if (Chip(P, F + float2(-h - 1, -28), 4, 2, col) || Chip(P, F + float2(h + 1, -28), 3, 2, col) || Chip(P, F + float2(2, -37), 3, 1, col)) return true; // shoulders, head
                return false;
            }

            half4 frag (v2f i) : SV_Target
            {
                float2 P = floor(i.wpx) + .5; P.y = -P.y;                                                       // pixel centre, y flipped to image space
                float2 C = float2(_Ctr.x, -_Ctr.y) * _PPU; float3 col = 0; bool hit = false; float t = tstep(_T);
                if (_Mode < .5) hit = FrostField(P, C, _R, _Reach, _Melt, _Seed, _Bright, _BareT, col);
                else if (_Mode < 1.5) hit = Columns(P, C, _R, _T, _Seed, _Dis, _Amp, _HMax, _TFront, _Gap, _RIn, _Band.xy, col);
                else if (_Mode < 2.5) hit = Orb(P, C, _Dir.xy, _R, _Seed, _Pop, t, col);
                else if (_Mode < 3.5) hit = Shard(P, C, col);
                else hit = SpriteIce(P, _Level, _Seed, col);
                clip(hit ? 1 : -1); return half4(col, 1);
            }
            ENDCG
        }
    }
}
