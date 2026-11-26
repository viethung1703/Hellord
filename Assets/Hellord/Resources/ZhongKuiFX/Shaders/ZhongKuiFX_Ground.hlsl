// Zhong Kui Ground (t1709): cursed earth, clay slam, hell-gate threshold. Under sprites. No flat rings, discs or washes.
// 0 decrepify (2.5 u, 6 s), 1 clay golem slam, 2 hell-gate threshold.

float4 ZFX_Frag(float2 p, float2 w)
{
    float t = zk_t();
    float hot = zsat(_Hot);
    float pr = zsat(_Progress);
    float fade = zsat(_Fade);
    float ign = zsat(_Ignite);
    float inten = zsat(_Intensity);
    float row = -1.0, idx = 0.0, gl = 0.0;
    float2 g = float2(p.x, p.y / max(_Squash, 0.2));
    float rr = length(g) / max(_Radius, 1.0);
    if (_Mode < 0.5)
    {   // 0 DECREPIFY: blotchy cracked earth, bruise-violet and sickly-green veins. Instant, 6 s, fades at the end.
        float live = ign * inten * (1.0 - fade);
        if (rr < 1.12 && live > 0.02)
        {
            float4 nz = zN(_NoiseTex, p / 13.0 + float2(_Seed * 3.1, 0.4));
            float fl = zflow(_NoiseTex, p, 16.0, t, 3.5, _Seed);
            float cov = (1.0 - rr) * 1.2 + (nz.r - 0.5) * 0.55 - (1.0 - live) * 0.4;
            // broken rim, not a hard ellipse
            if (cov > 0.10 && nz.b < 0.92)
            {
                if (nz.a < 0.09 + 0.05 * fl)
                {   // veins in the cracks
                    row = 2.0;
                    idx = (nz.a < 0.04) ? ((fl > 0.55) ? 4.0 : 3.0) : 2.0;
                    gl = max(gl, 0.28 * live);
                }
                else if (zclump(_NoiseTex, cov * live, p, _Seed + 0.2))
                {   // lifted earth plates
                    row = 3.0;
                    idx = (nz.g > 0.6 && p.x < 0.0) ? 3.0 : 2.0;
                    if (nz.a < 0.16) { row = 2.0; idx = 1.0; }
                }
            }
        }
    }
    else if (_Mode < 1.5)
    {   // 1 CLAY SLAM: lifted terracotta plates, radial cracks, 12 chunks, 5 puffs. No shock ring.
        float up = zeaseOut(zsat(pr / 0.22));
        float live = (1.0 - fade) * (1.0 - zeaseIn(zsat((pr - 0.45) / 0.55)));
        if (rr < 1.15 * up && live > 0.03)
        {
            float4 nz = zN(_NoiseTex, p / 10.0 + float2(_Seed, 0.2));
            float ang = atan2(g.y, g.x);
            float wedge = abs(frac(ang / ZTAU * 7.0 + _Seed) - 0.5);
            // cracks along irregular radials
            if (wedge < 0.06 + (1.0 - rr) * 0.03 && nz.a < 0.2)
            { row = 3.0; idx = 0.0; }
            else if (zclump(_NoiseTex, (1.0 - rr) * 1.1 * live, p, _Seed))
            {
                row = 3.0;
                idx = (nz.r > 0.55) ? 4.0 : 2.0;
                if (p.y > 0.0 && nz.g > 0.65) idx = 5.0;   // lit lifted face
            }
        }
        // 12 dirt / clay chunks
        for (int k = 0; k < 12; k++)
        {
            float fk = float(k);
            float an = fk / 12.0 * ZTAU + _Angle * ZPI / 180.0 * 0.15 + _Seed;
            float2 pos = gg_ballistic(an, 30.0 + 16.0 * zhash1(fk), 100.0, pr * 0.65, _Squash);
            float ch = gg_chip(p - floor(pos + float2(0.5, 0.5)), 1.1 + 0.5 * zhash1(fk + 2.0));
            if (ch > -0.5 && pr < 0.9 && pos.y > -8.0) { row = 3.0; idx = (ch > 3.0) ? 5.0 : 3.0; }
        }
        // 5 small dust puffs, no plumes
        for (int j = 0; j < 5; j++)
        {
            float fj = float(j);
            float an = fj / 5.0 * ZTAU + 0.3;
            float2 pp = float2(cos(an), sin(an) * _Squash) * (_Radius * 0.35 + pr * 10.0) + float2(0.0, 3.0 + pr * 4.0);
            float d = length(p - pp);
            if (d < 2.6 * (1.0 - pr * 0.6) && pr > 0.05 && zdith(0.65 * live, p + fj))
            { row = 4.0; idx = 3.0; }
        }
        gl = max(gl, zsat(1.0 - rr) * 0.25 * live * (1.0 - pr));
    }
    else
    {   // 2 HELL GATE THRESHOLD: cracked earth under the arch, soul fire in the deep cracks.
        float live = inten * (1.0 - fade);
        if (rr < 1.15 && live > 0.02)
        {
            float4 nz = zN(_NoiseTex, p / 11.0 + float2(_Seed, 0.1));
            float fl = zflow(_NoiseTex, p, 12.0, t, 5.0, _Seed);
            float cov = (1.0 - rr) * 1.15 + (nz.r - 0.5) * 0.4;
            if (cov > 0.08)
            {
                if (nz.a < 0.10)
                {
                    row = 1.0;
                    idx = (fl > 0.55) ? zk_cap(5.0, 7.0, hot) : 3.0;
                    gl = max(gl, 0.34 * live);
                }
                else if (zclump(_NoiseTex, cov * live, p, _Seed))
                { row = 3.0; idx = (nz.g > 0.6) ? 2.0 : 1.0; }
            }
        }
    }
    if (row < -0.5) return zk_none();
    if (_Layer > 0.5)
    {
        gl = min(gl, _GlowCap);
        if (gl < 0.02) return zk_none();
        float style = _Mode < 0.5 ? 2.0 : (_Mode < 1.5 ? 0.0 : 1.0);
        return zk_glow(_RampTex, gl, p + w * 0.01, _GlowGain, style);
    }
    return float4(zramp(_RampTex, row, idx), 1.0);
}
