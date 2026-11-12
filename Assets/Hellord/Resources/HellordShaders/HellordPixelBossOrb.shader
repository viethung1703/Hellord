// t950 Hellord PixelBossOrb: boss projectiles. Mode 0 = cursed orb "option B" (opt.proj_B / gor.orb / qil.orb, element rows),
// mode 1 = Horse-Face wind vortex v2 (proj2.vorb, incl. its 1-3 shear streaks). One quad per bullet, replaces the PLACEHOLDER46 sprite.
// NOT COMPILED, NOT TESTED IN UNITY. Opaque palette + clip, Blend Off. Pixel-centre grid (B2.grid, +0.5).
Shader "Hellord/PixelBossOrb"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite (unused, quad only)", 2D) = "white" {}
        _NoiseTex ("fx928_noise128 (Bilinear, Repeat)", 2D) = "gray" {}
        _RampTex ("fx950_boss_ramps 8x20 (Point, Clamp)", 2D) = "white" {}
        _RampRows ("Ramp rows", Float) = 20
        [Enum(Orb,0,Vortex,1)] _Mode ("Mode", Float) = 0
        _Row ("Orb body row: cursed 7, ice 9, fire 11 (rim/glow row = _Row + 1)", Float) = 7
        _Pos ("Bullet centre, world u (b.p; NOT pixel snapped)", Vector) = (0,0,0,0)
        _Vel ("Bullet velocity, world (vortex streak dir)", Vector) = (1,0,0,0)
        _Rpx ("Orb radius px (b.r * 16)", Float) = 7.2
        _RX ("Vortex rx px", Float) = 5
        _RY ("Vortex ry px", Float) = 4.5
        _Ang ("Vortex axis, world deg", Float) = 0
        _Ntr ("Vortex streaks 0..3", Float) = 1
        _I ("Per-bullet index i (noise phase)", Float) = 0
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
            float4 _Pos, _Vel;
            float _Mode, _Row, _Rpx, _RX, _RY, _Ang, _Ntr, _I;
            struct appdata { float4 vertex : POSITION; };
            struct v2f { float4 pos : SV_POSITION; float2 wpx : TEXCOORD0; };
            v2f vert (appdata v) { v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.wpx = mul(unity_ObjectToWorld, v.vertex).xy * _PPU; return o; }
            half4 frag (v2f i) : SV_Target
            {
                float2 P = ImgPx(i.wpx), C = P + 0.5, X = ImgPt(_Pos.xy);  // C = xx, yy (pixel centres); X = x, y
                float f = Frame(), ii = _I, b = Bay(C);
                half4 col = 0; bool hit = false;
                if (_Mode < 0.5)
                {
                    float r = _Rpx, q = length(C - X + 0.5), a = atan2(C.y - X.y, C.x - X.x);
                    if (q >= r && q < r + 4)                                                         // banded dithered glow
                    {
                        float band = floor((1 - (q - r) / 4) * 3) / 3;
                        if (band + 0.15 * sin(f * 0.6 + ii) > b + 0.55) { col = Ramp(_Row + 1, band > 0.6 ? 4 : 5); hit = true; }
                    }
                    if (q < r - 1)                                                                   // churning body
                    {
                        float n = N1((C.x - X.x) / 3 + cos(f * 0.3) * 2, (C.y - X.y) / 3 + sin(f * 0.3) * 2 + ii), sw = N1(a * 1.3 + f * 0.4, q / 2);
                        col = Ramp(_Row, Quant(saturate(0.25 + (n - 0.5) * 0.9 + (sw - 0.5) * 0.6 - q / r * 0.1), 5)); hit = true;
                    }
                    if (q >= r - 1.5 && q < r) { col = Ramp(_Row + 1, Quant(saturate(N1(a * 2 + f * 0.5, ii)), 4)); hit = true; }   // ember rim
                }
                else
                {   // row 13 = SLATE 0..3 (181425 262b44 3a4466 5a6988), CUT 4..6 (8b9bb4 c0cbdc ffffff)
                    float rx = _RX, ry = _RY, R = max(rx, ry) + 2, r = min(rx, ry), an = radians(-_Ang), ca = cos(an), sa = sin(an);
                    float2 dv = ImgDir(_Vel.xy); float sp = length(dv); if (sp == 0) sp = 1; float ux = dv.x / sp, uy = dv.y / sp;
                    // shear streaks (c.put = round half even, same as HLSL round); the last put wins -> keep the last hit
                    [loop] for (int k = 0; k < 3; k++)
                    {
                        if (k >= (int)_Ntr) break;
                        float kc = k - (_Ntr - 1) / 2, off = kc * max(2, r * 0.9), L = (3 + r * 1.4) * (1 - 0.25 * abs(kc)), ph = fmodp(f * 2 + k * 3, 4);
                        [loop] for (int j = 0; j < 16; j++)
                        {
                            if (j >= (int)L) break;
                            if (fmodp(j + f, 5) == 4) continue;
                            float s = R - 1 + j + ph * 0.5, t = j / L;
                            float2 pp = float2(X.x - ux * s - uy * off, X.y - uy * s + ux * off);
                            if (all(round(pp) == P)) { col = Ramp(13, t < 0.3 ? 5 : t < 0.7 ? 4 : 3); hit = true; }
                        }
                    }
                    float dx = C.x - X.x + 0.5, dy = C.y - X.y + 0.5, u = (dx * ca + dy * sa) / rx, w = (-dx * sa + dy * ca) / ry;
                    float q = length(float2(u, w)), a = atan2(w, u);
                    if (q < 1)
                    {
                        float n = N1(u * rx / 2.5 + cos(f * 0.5 + ii) * 2, w * ry / 2.5 + sin(f * 0.5) * 2 + ii), sw = N1(a * 1.5 - f * 0.7, q * 3);
                        col = Ramp(13, 1 + Quant(saturate(0.35 + (n - 0.5) * 0.8 + (sw - 0.5) * 0.7 - q * 0.2), 3)); hit = true;
                        float arc = fmodp(a / 6.283 * 3 - f * 0.22 + ii * 0.3, 1);
                        if (q > 1 - 2.2 / r && arc < 0.42) col = Ramp(13, arc < 0.16 ? 6 : 5);            // cutting arcs
                        if (q < 0.7 && q > 0.25 && fmodp(a / 6.283 * 2 + q * 0.9 - f * 0.18, 1) < 0.12) col = Ramp(13, 4);   // inner churn
                    }
                    if (q >= 1 && q < 1 + 1.2 / r) { col = Ramp(13, 0); hit = true; }                    // ink outline
                }
                if (!hit) discard;
                return col;
            }
            ENDCG
        }
    }
}
