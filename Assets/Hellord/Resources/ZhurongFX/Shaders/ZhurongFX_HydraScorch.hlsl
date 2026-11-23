// Hydra scorch v2 (t1646): what the Hydra leaves when it is gone. A burnt ground mark that starts hot (bridging the
// Hydra2 magma pool), a few 2x2 embers dying out one by one, one or two thin smoke wisps, then cooling to dark and
// dissolving out by the end of life with no pop. Lessons from Kindling v2: dark blotchy scorch reads on both floors;
// few, clustered 2x2 embers instead of glitter.
// Drop-in for the old Ground-shader ZFX_Hydra_Scorch: the game drives _FxTime, _Seed, _Ignite 1 and
// _Cool = sat((t - 0.3) / 1.4) (drive 4). The shader also derives the same curve from _FxTime (_CoolStart/_CoolDur),
// cool = max(both), so it works with or without _Cool. Seed is mixed with the pivot so hydras on different spots differ
// even if code passes a constant _Seed.
// Pivot = hydra ground point (same as the Hydra2 pivot). Footprint = the Hydra2 pool (t1647); _Radius/_Squash are unused (kept for the API).
float4 ZFX_Frag(float2 p, float2 w)
{
    float t = floor(_FxTime * _StepFps) / _StepFps;
    float stp = floor(_FxTime * _StepFps);
    float cool = max(_Cool, zsat((t - _CoolStart) / max(_CoolDur, 0.01)));
    float sd = frac(_Seed + zhash(floor(_Origin.xy * _PPU) * 0.0137 + float2(3.1, 7.7)));
    float2 so = float2(sd * 41.0, sd * 67.0);
    float R = _Radius;
    float4 col = float4(0.0, 0.0, 0.0, 0.0);
    float glowF = 0.0;
    // ---- the Hydra2 pool, cooling in place (t1647) ----
    // Same footprint maths as zh_base in ZhurongFX_Hydra2.hlsl: ellipse semi-axes 13 x 4.6 px centred (0, +1.5) on the
    // shared pivot, edge noise n1 (p / 26 + (_Seed, 0.4)) * 0.35; lava d < 0.58, crust rim 0.58..1 (ink lip > 0.93),
    // char clumps + ember cracks 0.85..1.75. At age 0 it matches Hydra2 at _Fade = 1 (pass the hydra's seed as _Seed for
    // an exact edge match). Then it cools to char and breaks up from the OUTSIDE IN (outer clumps, rim, then the centre).
    float2 e = float2(p.x / 13.0, (p.y - 1.5) / 4.6);
    float n1 = zN(_NoiseTex, p / 26.0 + float2(_Seed, 0.4)).r;
    float d = length(e) + (n1 - 0.5) * 0.35;
    float crack = zN(_NoiseTex, p / 22.0 + float2(_Seed * 0.7, 0.1)).a;
    float n3 = zN(_NoiseTex, (p + so) / 10.0).g;
    float diss = zeaseIn(zsat((cool - 0.45) / 0.55));               // break-up front moves inward
    float edgeK = 1.8 * (1.0 - diss) + (n3 - 0.5) * 0.35 * diss;    // pixels with d beyond this are gone
    float dk = zsat(cool * 1.25);                                   // darkening: rock / lava -> char
    bool hole = n3 > 1.0 - diss * 0.55;                              // interior breaks up too, not a shrinking oval
    if (d < edgeK && d < 1.75 && abs(p.x) < 21.5 && !hole)
    {
        if (d > 0.85 && d >= 1.0)
        {
            float far = (d - 0.85) / 0.9;
            if (crack < 0.08 && far < 0.95)
            {
                float hc = (0.75 - far * 0.45) * 0.2 * (1.0 - dk);
                col = (hc > 0.15) ? zfire(_RampTex, hc, w, 0.15) : float4(zramp(_RampTex, 0.0, 1.0), 1.0);
                if (col.a < 0.5) col = float4(zramp(_RampTex, 0.0, 1.0), 1.0);
            }
            else if (zclump(_NoiseTex, (1.0 - far) * 0.85, p, _Seed)) col = float4(zramp(_RampTex, 0.0, (n1 > 0.55) ? 1.0 : 0.0), 1.0);
        }
        else if (d >= 0.58)
        {
            float lit = zsat(0.5 + (p.y - 2.0) * 0.12 - p.x * 0.02 + (n1 - 0.5) * 0.6) * (1.0 - dk * 0.85);
            col = float4(zramp(_RampTex, 3.0, zband(lit, 5.0, w, 0.35)), 1.0);
            float mc = zN(_NoiseTex, p / 12.0 + float2(0.3, _Seed)).a;
            float hm = (0.62 + 0.2 * (0.78 - d)) * 0.3 * (1.0 - dk);
            if (mc < 0.1) { col = (hm > 0.2) ? zfire(_RampTex, hm, w, 0.2) : float4(zramp(_RampTex, 0.0, 1.0), 1.0); if (col.a < 0.5) col = float4(zramp(_RampTex, 0.0, 1.0), 1.0); glowF = max(glowF, hm * 0.6); }
            if (d > 0.93) col = float4(zramp(_RampTex, 0.0, 0.0), 1.0);
        }
        else
        {
            float lf = zflow(_NoiseTex, float2(p.x * 0.8, p.y * 1.6), 8.0, t, 2.0, _Seed + 0.5);
            float crust = zN(_NoiseTex, p / 14.0 + float2(t * 0.03, _Seed)).g;
            float heat = (0.62 + 0.3 * (0.58 - d) / 0.58 + (lf - 0.5) * 0.5) * 0.25 * (1.0 - dk);
            col = (heat > 0.12) ? zfire(_RampTex, heat, w, 0.0) : float4(zramp(_RampTex, 0.0, (n3 > 0.5) ? 1.0 : 0.0), 1.0);
            if (crust > 0.62) col = float4(zramp(_RampTex, 3.0, (crust > 0.75 && dk < 0.6) ? 2.0 : 1.0), 1.0);
            if (n3 < 0.2 && dk > 0.5) col = float4(zramp(_RampTex, 5.0, 1.0), 1.0);   // ash fleck as it goes cold
            glowF = max(glowF, heat * 0.9);
        }
    }
    // ---- embers: _Embers (4..8) 2x2, inside the mark, each dies at its own time ----
    float nE = floor(_Embers + 0.5);
    for (int j = 0; j < 8; j++)
    {
        float fj = float(j);
        if (fj >= nE) continue;
        float e1 = zhash(float2(fj * 3.1 + 2.0, sd * 53.0));
        float e2 = zhash(float2(fj * 7.7 + 9.0, sd * 23.0));
        float e3 = zhash(float2(fj * 1.9 + 4.0, sd * 71.0));
        float a = e1 * ZTAU;
        float2 ep = floor(float2(cos(a) * 13.0, sin(a) * 4.6) * (0.15 + 0.65 * e2) + float2(0.0, 1.5)) - 0.5;
        float2 q = p - ep;
        float dieAt = 0.3 + 0.55 * e3;                              // cool value at which this ember is out
        float life = 1.0 - zsat(cool / dieAt);
        if (life > 0.0 && q.x >= -1.0 && q.x < 1.0 && q.y >= -1.0 && q.y < 1.0)
        {
            float fk = zhash(float2(floor(stp * 0.5) + fj * 13.0, sd * 5.0));
            float heat = life * (0.75 + 0.25 * fk);
            float idx = (heat > 0.7) ? 5.0 : ((heat > 0.5) ? 4.0 : ((heat > 0.28) ? 3.0 : 2.0));
            col = float4(zramp(_RampTex, 0.0, idx), 1.0);
            glowF = max(glowF, heat * 0.6);
        }
    }
    // ---- thin smoke wisps: _Wisps (1..2), 1-2 px wide, sinuous, broken, rising from the mark ----
    float nW = floor(_Wisps + 0.5);
    for (int k = 0; k < 2; k++)
    {
        float fk2 = float(k);
        if (fk2 >= nW) continue;
        float h1 = zhash(float2(fk2 * 4.3 + 6.0, sd * 13.0));
        float2 base = floor(float2((h1 - 0.5) * 12.0 + (fk2 - 0.5) * 8.0, 1.5 + 3.0 * zhash(float2(fk2, sd * 3.0)))) + 0.5;
        float wl = zsat((t - 0.1 - 0.25 * fk2) / 0.3) * (1.0 - zsat((cool - 0.45) / 0.45));   // in, then thins out
        float Hm = min((12.0 + 5.0 * h1) * (0.5 + 0.5 * wl), 14.0 - base.y);   // stays inside the quad top (y +15)
        float y = p.y - base.y;
        if (wl > 0.0 && y >= 0.0 && y < Hm)
        {
            float yn = y / Hm;
            float sx = sin(y * 0.5 - t * 3.2 + h1 * 6.0) * (0.3 + 1.1 * yn) + yn * 1.5 * (h1 - 0.5);
            float hw = 0.7 + 0.55 * (1.0 - yn);
            float gap = zN(_NoiseTex, float2(base.x * 0.05 + fk2 * 0.3, (y - t * 9.0) / 24.0) + so * 0.01).r;
            float dens = wl * (1.2 - yn) - (gap - 0.5) * 0.5;
            bool on = (yn < 0.55) ? (dens > 0.3) : zdith(dens + 0.1, w);
            if (abs(p.x - base.x - sx) < hw && on)
            {
                col = float4(zramp(_RampTex, 1.0, (yn < 0.5) ? 1.0 : 2.0), 1.0);   // 262b44 low -> 3e2731 up: dark smoke reads on both floors
            }
        }
    }
    float blot = zN(_NoiseTex, (p + so * 1.7) / 40.0).g;
    glowF *= (blot > 0.4) ? 1.0 : 0.65;
    if (_Layer > 0.5) return zglow(_RampTex, glowF, w, _GlowGain);
    return col;
}
