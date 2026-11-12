// t950 Hellord PixelBossGround: small per-instance boss materials, 1:1 ports. One quad per instance, sized to the footprint.
// 0 heat trail (b1.heat_trail)      1 lava puddle (qil.puddle)      2 frost trail (gor.frost)     3 breath flame (qil.breath)
// 4 wind sweep (b4.wind, Spin v2)   5 charging core (core/icore/fcore/hcore = 2 x _glow)
// 6 hit flash (hit_flash/iflash/fflash = blob + _glow)   7 ice spike (gor.spike)   8 dither dot shadow (b1.dither_dot)
// NOT COMPILED, NOT TESTED IN UNITY. Opaque palette + clip, Blend Off.
Shader "Hellord/PixelBossGround"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite (unused, quad only)", 2D) = "white" {}
        _NoiseTex ("fx928_noise128 (Bilinear, Repeat)", 2D) = "gray" {}
        _RampTex ("fx950_boss_ramps 8x20 (Point, Clamp)", 2D) = "white" {}
        _RampRows ("Ramp rows", Float) = 20
        [Enum(HeatTrail,0,Lava,1,Frost,2,Breath,3,Wind,4,Core,5,Flash,6,Spike,7,DitherDot,8)] _Mode ("Mode", Float) = 1
        _Ctr ("Centre / origin / trail head, world u", Vector) = (0,0,0,0)
        _Vel ("Heat trail direction (world)", Vector) = (1,0,0,0)
        _R ("Radius px (lava/frost r, breath B_R, wind R, core pul, flash blob r, spike r, dot r)", Float) = 9
        _R2 ("Second radius px (core pul//2, flash glow r)", Float) = 1
        _Age ("Age (frames) / heat trail n / spike h 0..1", Float) = 0
        _Life ("Life (frames)", Float) = 40
        _Seed ("Seed / i", Float) = 0
        _Deg ("Breath current aim, world deg", Float) = 0
        _HF ("Breath flame half deg", Float) = 14
        _Dens ("Dither dot density", Float) = 0.5
        _ColA ("Colour A (trail[0] / core core / flash outer / dot)", Color) = (1,1,1,1)
        _ColB ("Colour B (trail[1] / core arm / flash inner)", Color) = (1,1,1,1)
        _ColC ("Colour C (trail[2] / core2 core / flash core)", Color) = (1,1,1,1)
        _ColD ("Colour D (core2 arm / flash glow core)", Color) = (1,1,1,1)
        _ColE ("Colour E (flash glow arm)", Color) = (1,1,1,1)
        _Ink ("Ink (E ink, blob outline)", Color) = (0.094,0.078,0.145,1)
        _Fps ("Frame rate of f", Float) = 30
        _FrameOfs ("Frame offset", Float) = 0
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
            #include "HellordBossFX.cginc"
            float4 _Ctr, _Vel; half4 _ColA, _ColB, _ColC, _ColD, _ColE, _Ink;
            float _Mode, _R, _R2, _Age, _Life, _Seed, _Deg, _HF, _Dens;
            struct appdata { float4 vertex : POSITION; };
            struct v2f { float4 pos : SV_POSITION; float2 wpx : TEXCOORD0; };
            v2f vert (appdata v) { v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.wpx = mul(unity_ObjectToWorld, v.vertex).xy * _PPU; return o; }
            bool Disc(float2 P, float2 c, float r, float sq) { return length(float2(P.x - c.x, (P.y - c.y) / sq)) <= r; }  // c.disc, int grid
            half4 frag (v2f i) : SV_Target
            {
                float2 P = ImgPx(i.wpx), C = P + 0.5, X = ImgPt(_Ctr.xy);
                float f = Frame(), r = _R; int m = (int)_Mode; half4 col = 0; bool hit = false;
                if (m == 0)        // heat trail: n points 2 px apart behind X, last put wins
                {
                    float2 u = ImgDir(_Vel.xy); u /= max(length(u), 1e-6); int n = (int)_Age;
                    [loop] for (int k = 1; k <= 8; k++)
                    {
                        if (k > n) break;
                        float2 p = X - u * k * 2; float t = 1 - k / (n + 1.0);
                        if (all(round(p) == P) && t + N1(p.x / 7 + f * 0.17, p.y / 7 + _Seed) * 0.4 - 0.2 > Bay(trunc(p)) + 0.5)
                        { int j = min(2, (int)(t * 3)); col = j == 0 ? _ColA : j == 1 ? _ColB : _ColC; hit = true; }
                    }
                }
                else if (m == 1 || m == 2)   // 1 lava (row 16, 5), 2 frost (row 17, 2)
                {
                    float sq = m == 1 ? 0.5 : 0.7, q = length(float2(C.x - X.x, (C.y - X.y) / sq)), u = _Age / _Life;
                    if (InGrid(P, X, r + 1, r * sq + 1) && q < r)
                    {
                        if (m == 1)
                        {
                            float n = N1(C.x / 3 + _Seed + _Age * 0.1, C.y / 3);
                            if (n - q / r * 0.35 > 0.3 + u * 0.55 && Bay(C) + 0.45 < 0.75) { col = Ramp(16, Quant(saturate((n - 0.3) * 1.6 * (1 - u) + 0.1), 5)); hit = true; }
                        }
                        else
                        {
                            float n = N1(C.x / 3 + _Seed, C.y / 3);
                            if (n - q / r * 0.3 > 0.35 + u * 0.6 && Bay(C) + 0.5 < 0.7) { col = Ramp(17, n > 0.6 ? 1 : 0); hit = true; }
                        }
                    }
                }
                else if (m == 3)   // breath flame (row 18, 5): wedge HF about the current aim, ground y * .35
                {
                    float2 d = float2(C.x - X.x, (C.y - X.y) / 0.35); float q = length(d), da = AngDiff(degrees(atan2(d.y, d.x)), -_Deg);
                    if (InGrid(P, X, r + 2, r + 2) && da < _HF && q < r)
                    {
                        float n = N1(C.x / 3 - f * 0.8, C.y / 2 + f * 0.2);
                        if (n + 0.3 - q / r * 0.3 > Bay(C) + 0.3) { col = Ramp(18, Quant(saturate(1 - q / r * 0.8 + (n - 0.5) * 0.6 - da / (_HF * 2.85)), 5)); hit = true; }
                    }
                }
                else if (m == 4)   // wind sweep (row 6, 4), R = SPIN_R + 6, centre = pos + 23 px
                {
                    float2 d = float2(C.x - X.x, (C.y - X.y) / 0.7); float q = length(d), ang = atan2(d.y, d.x);
                    float w = (N1(d.x / 9 + f * 0.05, d.y / 9 - f * 0.04) - 0.5) * 1.6, ph = ang * 3 / 6.283 + q / 14 - f * 0.09 + w * 0.35;
                    float band = 1 - abs(frac(ph) - 0.5) * 2, brk = N1(ang * 1.5 - f * 0.18, q / 7), env = saturate(1 - abs(q - r * 0.55) / (r * 0.5));
                    float v = band * band * band * env * (0.55 + brk) + Bay(C) * 0.12;
                    if (InGrid(P, X, r, r) && v > 0.3) { col = Ramp(6, Quant(saturate((v - 0.3) * 1.4), 4)); hit = true; }
                }
                else if (m == 5 || m == 6)  // core: _glow(pul, A, B) then _glow(pul//2, C, D).  flash: blob(r, A, B, C) then _glow(_R2, D, E)
                {
                    float2 ci = floor(X);
                    if (m == 5)
                    {
                        int g = Glow(P, ci, r); if (g > 0) { col = g == 2 ? _ColA : _ColB; hit = true; }
                        g = Glow(P, ci, _R2); if (g > 0) { col = g == 2 ? _ColC : _ColD; hit = true; }
                    }
                    else
                    {
                        if (Disc(P, X, r + 1, 0.85)) { col = _Ink; hit = true; }
                        if (Disc(P, X, r, 0.85)) { col = _ColA; hit = true; }
                        if (r >= 2 && Disc(P, X - float2(0.5, 0.6), r * 0.6, 0.85)) { col = _ColB; hit = true; }
                        if (r >= 3 && Disc(P, X - 1, r * 0.25 + 0.5, 1)) { col = _ColC; hit = true; }
                        int g = Glow(P, ci, _R2); if (g > 0) { col = g == 2 ? _ColD : _ColE; hit = true; }
                    }
                }
                else if (m == 7)   // ice spike (row 19: Ko, BD, BL, ICE2, ICE, Wh), h = _Age, base at X, last put wins
                {
                    float H = floor(r * 2.6 * _Age), W = r, rx0 = round(X.x);
                    [loop] for (int yy = -48; yy <= 0; yy++)
                    {
                        if (yy < -H || H < 1) continue;
                        if (round(X.y + yy) != P.y) continue;
                        float hw = W * (1 + yy / H * 0.95);
                        [unroll] for (int dk = -1; dk <= 1; dk++)
                        {
                            float xx = P.x - rx0 + dk;
                            if (xx < -floor(hw) - 1 || xx > floor(hw) + 1 || round(X.x + xx) != P.x) continue;
                            float dd = abs(xx) - hw;
                            if (dd > 1) continue;
                            if (dd > 0 || yy == 0) { col = Ramp(19, 0); hit = true; continue; }
                            float v = (xx < 0 ? 0.6 : 0.25) + (N1((X.x + xx) / 3 + _Seed, (X.y + yy) / 3) - 0.5) * 0.5 + (-yy / H) * 0.3;
                            col = Ramp(19, 1 + (int)clamp(v * 5, 0, 4)); hit = true;
                        }
                    }
                }
                else               // dither dot (shadow under thrown objects)
                {
                    float q = length(C - X) / max(r, 0.5);
                    if (InGrid(P, X, r + 1, r + 1) && q < 1 && (1 - q) * _Dens > Bay(C) + 0.5) { col = _ColA; hit = true; }
                }
                if (!hit) discard;
                return col;
            }
            ENDCG
        }
    }
}
