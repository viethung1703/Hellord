// Gonggong Breath (t1682): Breath of the Deep, a held 40 deg frost cone (4 u). Mode 0 = the torrent (over sprites): streaming frost
// streaks flowing outward in polar space (flow noise), a dense white-cyan core near the mouth (>= 3 px), ice crystals forming at
// the cone edge, sparse snow flecks; NOT a flat triangle (coverage breaks up with distance). Mode 1 = frost growing on the ground
// under the cone with hold time (ground tier). Colours: row 3 frost, row 0 ice.
float4 ZFX_Frag(float2 p, float2 w)
{
    float t = gg_t();
    float hot = zsat(_Hot);
    float ang = _Angle * 0.0174533;
    float2 d = float2(cos(ang), sin(ang)); float2 nr = float2(-d.y, d.x);
    float u = dot(p, d), v = dot(p, nr);
    float R = _Radius * (0.35 + 0.65 * zsat(_Intensity));
    float hlf = _Arc * 0.5 * 0.0174533;
    float r = length(p);
    float a = atan2(v, max(u, 0.001));
    float an = abs(a) / hlf;                                    // 0 centre .. 1 cone edge
    float rn = r / max(R, 1.0);
    float row = -1.0, idx = 0.0, gl = 0.0;
    if (_Mode < 0.5)
    {
        if (u > 0.0 && an < 1.15 && rn < 1.05)
        {
            // polar flow: streaks elongated along the radius, moving outward fast; jittered per 15 fps step
            float s1 = zN(_NoiseTex, float2(r * 0.12 - _FxTime * 4.0, a * 9.0 + _Seed * 7.0)).r;
            float s2 = zN(_NoiseTex, float2(r * 0.2 - _FxTime * 6.0, a * 15.0 + 3.1)).g;
            float body = (1.0 - an * an) * (0.95 - 0.3 * rn) * zsat(_Intensity);
            float dens = body + (s1 - 0.5) * 0.9;
            if (dens > 0.22 && rn < 1.0 - (s2 - 0.5) * 0.2)
            {
                row = 3.0; idx = dens > 0.8 ? 4.0 : (dens > 0.5 ? 3.0 : 2.0);
                if (s2 > 0.62 && dens > 0.35) { row = 0.0; idx = 3.0; }            // cyan streaks
                if (rn < 0.2 && an < 0.5) { row = 0.0; idx = gg_cap(5.0, 5.0, hot); }   // dense white core at the mouth (>= 3 px)
                else if (rn < 0.32 && an < 0.65) { row = 0.0; idx = 4.0; }
                if (dens < 0.36 && !zdith(0.75, p + float2(t * 15.0, 0.0))) row = -1.0;   // dithered fringe only at the thin edge
            }
            gl = (rn < 0.4 && an < 0.7) ? 0.7 * (1.0 - rn) * zsat(_Intensity) : (dens > 0.6 ? 0.25 : 0.0);
            // ice crystals forming along the cone edges (5 per side, 2x4 px, appear with hold time)
            for (int i = 0; i < 10; i++)
            {
                float fi = float(i);
                float side = fi < 4.5 ? 1.0 : -1.0;
                float k = zmod(fi, 5.0);
                float rk = R * (0.35 + 0.13 * k);
                float ak = side * hlf * (0.92 + 0.1 * zhash1(fi + _Seed));
                float2 cp = floor(float2(cos(ak) * rk, sin(ak) * rk) + float2(0.5, 0.5));
                float f = gg_chip(p - floor(cp.x * d + cp.y * nr + float2(0.5, 0.5)), 1.3);
                if (f > -0.5 && _Hold > 0.15 + k * 0.25) { row = 0.0; idx = gg_facetidx(f, hot); }
            }
        }
        // snow flecks thrown past the cone end: 6 single + plus-shaped flecks
        for (int j = 0; j < 6; j++)
        {
            float fj = float(j);
            float ph = frac(zhash1(fj * 1.9 + _Seed) + _FxTime * 1.8);
            float aj = (zhash1(fj * 3.7) - 0.5) * 2.0 * hlf;
            float rj = R * (0.6 + 0.55 * ph);
            float2 fp = floor(d * (cos(aj) * rj) + nr * (sin(aj) * rj) + float2(0.0, -ph * 4.0) + float2(0.5, 0.5));
            float2 q = p - fp;
            if (abs(q.x) + abs(q.y) <= 1.0 && _Intensity > 0.3) { row = 3.0; idx = (q.x == 0.0 && q.y == 0.0) ? 5.0 : 4.0; }
        }
    }
    else
    {   // 1 GROUND FROST under the cone: grows from the mouth outward with _Hold, thaws with 1 - _Intensity
        float2 pg = float2(p.x, p.y);
        float vg = dot(pg, nr), ug = dot(pg, d);
        float ag = abs(atan2(vg, max(ug, 0.001))) / hlf;
        float rg = length(pg) / max(_Radius, 1.0);
        if (ug > 0.0 && ag < 1.2 && rg < 1.1)
        {
            float grow = zsat(_Hold / 1.5) * zsat(_Intensity * 1.5);
            float cov = gg_frostcover(_NoiseTex, p, rg * 0.8 + ag * 0.25, grow, _Seed);
            if (cov > 1.5) { row = 0.0; idx = gg_cap(zN(_NoiseTex, p / 5.0).r > 0.6 ? 5.0 : 4.0, 5.0, hot); }
            else if (cov > 0.5)
            {
                float fn = zN(_NoiseTex, p / 4.0 + float2(0.7, _Seed)).g;
                if (fn > 0.27) { row = 3.0; idx = fn > 0.62 ? 4.0 : 3.0; }
            }
            gl = cov > 1.5 ? 0.3 : 0.0;
        }
    }
    if (_Layer > 0.5)
    {
        float g2 = min(gl, _GlowCap);
        return (g2 > 0.02) ? zglow(_RampTex, g2, p, _GlowGain) : gg_none();
    }
    return row < -0.5 ? gg_none() : float4(zramp(_RampTex, row, idx), 1.0);
}
