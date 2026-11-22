// Kindling pact node v2 (t1634b): small burning ground patch left by every Fireball hit (radius ~19 px, 2.0 s).
// v1 (scattered ember confetti) is archived in _archive/kindling_v1/. v2 makes the FLAME LICKS the star:
//   - 3-5 discrete flame tongues per patch (5-9 px tall, 3-4 px wide): yellow-white core -> orange -> dark red tip,
//     dark lip, flicker + sway on 15 fps steps, a small flame bit breaking off the tip and rising
//   - a dark blotchy scorch underlay (darkest under each lick, broken fbm blotches toward the centre), lingers through cool
//   - few 2x2 embers, clustered around the lick feet (none out at the edge)
//   - glow layer: warm soft banded light pool under each lick + lick/ember glow; screen-blended glow material
// Cool: licks die first, embers sink to dark red, then the scorch dissolves. Per-instance _Seed moves every lick/blotch.
// Pivot = patch centre on the ground. _FxTime = seconds since spawn.
float4 ZFX_Frag(float2 p, float2 w)
{
    float t = floor(_FxTime * _StepFps) / _StepFps;
    float stp = floor(_FxTime * _StepFps);
    float ign = zsat(t / max(_IgniteT, 0.01));
    float cool = zsat((t - (_Life - _CoolT)) / max(_CoolT, 0.01));
    float R = _Radius;
    float2 so = float2(_Seed * 37.0, _Seed * 53.0);
    float4 col = float4(0.0, 0.0, 0.0, 0.0);
    float glowF = 0.0;
    float scorch = 0.0;                        // scorch field (char underlay)
    float lickV = -1.0;                        // best lick heat at this pixel
    float lickEdge = 0.0;
    float nLick = 3.0 + floor(_Flames * 2.0 + 0.5);
    for (int i = 0; i < 5; i++)
    {
        float fi = float(i);
        if (fi >= nLick) continue;
        float h1 = zhash(float2(fi * 7.1 + 1.0, _Seed * 97.0 + 3.0));
        float h2 = zhash(float2(fi * 3.7 + 5.0, _Seed * 61.0 + 9.0));
        float h3 = zhash(float2(fi * 5.3 + 2.0, _Seed * 43.0 + 1.0));
        float a = (fi + 0.35 * h1) * ZTAU / nLick + _Seed * ZTAU;
        float rr = R * (0.28 + 0.42 * h2);
        float2 foot = floor(float2(cos(a) * rr, sin(a) * rr * _Squash)) + 0.5;
        float2 q = p - foot;
        // life of this lick: staggered ignite, dies early in the cool
        float grow = zeaseOut((t - 0.03 - 0.16 * h3) / 0.2);
        float die = 1.0 - zsat((cool - 0.2 * h1) / 0.45);
        float life = grow * die;
        // scorch blob under the lick (stays after the lick dies)
        float sb = 1.0 - length(float2(q.x / 8.0, (q.y + 0.5) / 4.5));
        scorch = max(scorch, sb * zsat((t - 0.02 - 0.16 * h3) / 0.15));
        // warm light pool (glow layer)
        float pool = 1.0 - length(float2(q.x / 9.0, (q.y + 1.0) / 5.0));
        glowF = max(glowF, zsat(pool) * 0.75 * _Light * life);
        if (life <= 0.02) continue;
        // 15 fps flicker: height and width jitter per step
        float fk = zhash(float2(stp * 1.37 + fi * 11.0, _Seed * 17.0));
        float H = (_LickH - 4.0 + 4.0 * h1) * (0.8 + 0.3 * fk) * (0.35 + 0.65 * life);
        float W = (1.55 + 0.45 * h2) * (0.6 + 0.4 * life) * (0.9 + 0.2 * fk);
        if (q.y >= -0.5 && q.y <= H)
        {
            float yn = zsat(q.y / H);
            float sway = sin(t * 5.5 + fi * 2.3 + yn * 2.2) * 1.1 * yn;
            float prof = (yn < 0.28) ? (0.8 + 0.2 * yn / 0.28) : pow(max(1.0 - (yn - 0.28) / 0.72, 0.0), 0.85);
            float fl = zflow(_NoiseTex, p + so, 4.0, t, 12.0, _Seed + fi);
            float hw = W * prof * (0.85 + 0.3 * fl) + 0.25;
            float dx = abs(q.x - sway);
            if (dx < hw + 0.75)
            {
                float cx = zsat(1.0 - dx / hw);
                float heat = cx * 0.62 + (1.0 - yn) * 0.5 - yn * 0.15 + (fl - 0.5) * 0.25;
                if (dx >= hw) heat = -0.5;                          // dark lip just outside the tongue
                if (heat > lickV) { lickV = heat; lickEdge = (dx >= hw) ? 1.0 : 0.0; }
            }
        }
        // a small flame bit breaks off the tip and rises (cycle 0.4 s, stepped)
        float ph = zmod(t / 0.4 + h3, 1.0);
        float2 bit = float2(floor(sin(t * 5.5 + fi * 2.3 + 2.2) * 1.1 + 0.5), floor(H + 1.0 + ph * 5.0));
        float2 qb = q - bit;
        if (life > 0.5 && ph < 0.75 && qb.x >= -1.0 && qb.x < 1.0 && qb.y >= 0.0 && qb.y < 2.0 - floor(ph * 2.0))
        {
            float bh = 0.5 - ph * 0.45;
            if (bh > lickV) { lickV = bh; lickEdge = 0.0; }
        }
    }
    // --- scorch / char underlay: blotchy, broken by fbm; lingers through the cool then dissolves ---
    float nE = zN(_NoiseTex, (p + so) / 60.0).r;
    float d = length(float2(p.x, p.y / _Squash)) / R + (nE - 0.5) * 0.4;
    float cn = zN(_NoiseTex, (p + so * 0.37) / 24.0).r;
    float centre = zsat(1.0 - d * 1.2) * 0.8 * ign;
    float sc = max(scorch, centre) * _Char;
    float diss = zsat((cool - 0.55) / 0.45);
    float thr = 0.25 + cn * 0.55 + diss * 0.7;
    if (sc > thr) col = float4(zramp(_RampTex, 0.0, (sc > thr + 0.22) ? 0.0 : 1.0), 1.0);
    // --- embers: few 2x2, clustered around the lick feet ---
    float nEmb = floor(_Embers * 8.0 + 0.5);
    for (int j = 0; j < 8; j++)
    {
        float fj = float(j);
        if (fj >= nEmb) continue;
        float li = zmod(fj, nLick);
        float h1 = zhash(float2(li * 7.1 + 1.0, _Seed * 97.0 + 3.0));
        float h2 = zhash(float2(li * 3.7 + 5.0, _Seed * 61.0 + 9.0));
        float a = (li + 0.35 * h1) * ZTAU / nLick + _Seed * ZTAU;
        float rr = R * (0.28 + 0.42 * h2);
        float2 foot = floor(float2(cos(a) * rr, sin(a) * rr * _Squash)) + 0.5;
        float e1 = zhash(float2(fj * 2.9 + 4.0, _Seed * 29.0));
        float e2 = zhash(float2(fj * 6.1 + 8.0, _Seed * 71.0));
        float2 ep = foot + floor(float2((e1 - 0.5) * 11.0, (e2 - 0.5) * 6.0 - 0.5));
        float2 qe = p - ep;
        float born = zsat((t - 0.05 - 0.2 * e1) / 0.1);
        if (born > 0.5 && cool < 0.92 && qe.x >= -0.5 && qe.x < 1.5 && qe.y >= -0.5 && qe.y < 1.5)
        {
            float fk = zhash(float2(floor(stp * 0.5) + fj * 13.0, _Seed * 5.0));
            float heat = (0.5 + 0.28 * fk) * (1.0 - cool * 0.9);
            float idx = (heat > 0.62) ? 5.0 : ((heat > 0.45) ? 4.0 : ((heat > 0.25) ? 2.0 : 1.0));
            col = float4(zramp(_RampTex, 0.0, idx), 1.0);
            glowF = max(glowF, heat * 0.55);
        }
    }
    // --- licks on top ---
    if (lickV > -1.0)
    {
        if (lickEdge > 0.5) col = float4(zramp(_RampTex, 0.0, 1.0), 1.0);
        else
        {
            float idx = 2.0 + zband(zsat(lickV) * 0.9, 6.0, w, 0.5);     // 2 dark red tip .. 7 white core
            col = float4(zramp(_RampTex, 0.0, idx), 1.0);
            glowF = max(glowF, 0.35 + zsat(lickV) * 0.55);
        }
    }
    // patchy light: blotches break the pools so overlaps keep gaps
    float blot = zN(_NoiseTex, (p + so * 1.7) / 40.0).g;
    glowF *= (blot > 0.42) ? 1.0 : 0.6;
    if (_Layer > 0.5) return zglow(_RampTex, glowF, w, _GlowGain);
    return col;
}
