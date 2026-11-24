// Gonggong Breath (t1682 v2): Breath of the Deep, a held 40 deg cone (4 u). v2 after the in-game clips: the v1 torrent was a faint streak
// smear plus a blocky white wedge. Mode 0 = the torrent (over sprites): a ROLLING BILLOW STREAM of icy mist and water, blobs that grow
// as they travel out (advected in polar space), each lit from the upper left (white / pale tops, blue undersides) with a deep-blue rim
// so it reads on the blue Frozen Hell floor and on light sand; a cyan jet at the mouth; ice shards and snow flecks thrown out.
// Mode 1 = the ground under the cone (ground tier): faceted ice crystals sprout outward from the mouth as you hold (rime dots at
// their feet) - no flat white slab. Colours: row 3 frost (2 5a6988, 3 8b9bb4, 4 c0cbdc, 5 white), row 0 ice (1 deep, 2 blue, 3 cyan, 4 pale).
float gb_dens(float2 p, float2 d, float2 nr, float R, float hlf, float I)
{
    float u = dot(p, d), v = dot(p, nr);
    float r = length(p);
    float res = -1.0;
    if (u > 0.5)
    {
        float a = atan2(v, u);
        float rn = r / max(R, 1.0);
        float an = abs(a) / (hlf * (0.85 + 0.35 * rn));
        float sc = 3.0 + r * 0.12;                                           // billow size grows outward (3 -> 11 px)
        float2 q = float2((r - _FxTime * 72.0) / sc, a * r / sc + _Seed * 5.0);
        float n = zN(_NoiseTex, q / 0.55).r * 0.7 + zN(_NoiseTex, q / 0.28 + float2(0.3, 0.7)).g * 0.3;
        float prof = (1.0 - an * an) * (1.0 - 0.45 * rn * rn) - max(rn - 0.82, 0.0) * 4.5;
        res = prof * I + (n - 0.5) * 0.95 - (rn < 0.12 ? 0.0 : 0.0);
    }
    return res;
}
float4 ZFX_Frag(float2 p, float2 w)
{
    float t = gg_t();
    float hot = zsat(_Hot);
    float ang = _Angle * 0.0174533;
    float2 d = float2(cos(ang), sin(ang)); float2 nr = float2(-d.y, d.x);
    float I = zsat(_Intensity);
    float R = _Radius * (0.35 + 0.65 * I);
    float hlf = _Arc * 0.5 * 0.0174533;
    float u = dot(p, d), v = dot(p, nr);
    float r = length(p);
    float a = atan2(v, max(u, 0.001));
    float an = abs(a) / hlf;
    float rn = r / max(R, 1.0);
    float row = -1.0, idx = 0.0, gl = 0.0;
    if (_Mode < 0.5)
    {
        float th = 0.4;
        float de = gb_dens(p, d, nr, R, hlf, I);
        if (de > th)
        {
            float dl = gb_dens(p + float2(-1.5, 1.5), d, nr, R, hlf, I);   // toward the light (upper left)
            float e = de - dl;
            if (de < th + 0.07) { row = 0.0; idx = 1.0; }                     // deep-blue rim
            else if (e > 0.08) { row = 3.0; idx = gg_cap(5.0, 5.0, hot); }    // lit billow tops
            else if (e > 0.0) { row = 3.0; idx = 4.0; }                       // pale
            else if (e > -0.07) { row = 0.0; idx = 3.0; }                     // cyan body
            else { row = 0.0; idx = 2.0; }                                    // blue undersides
            float s2 = zN(_NoiseTex, float2(r * 0.18 - _FxTime * 7.0, a * 14.0 + 3.1)).g;
            if (s2 > 0.74 && de > th + 0.12 && e > -0.07) { row = 0.0; idx = 4.0; }     // pale sparkle streaks
            // the mist lets the demons show through deep inside the billows (1/3 stipple, never on rims / tops)
            if (de > th + 0.25 && e <= 0.0 && rn > 0.3 && zbayer(p) > 0.16) row = -1.0;
            gl = de > 0.6 && row > -0.5 ? 0.3 * I : 0.0;
        }
        // the jet at the mouth: a 3-5 px cyan / pale water stream (white core capped)
        float jw = 1.5 + rn * 5.0;
        if (u > 0.0 && rn < 0.32 && abs(v) < jw)
        {
            float js = zN(_NoiseTex, float2((u - _FxTime * 90.0) / 6.0, v / 2.0) + float2(_Seed, 0.0)).r;
            row = 0.0; idx = abs(v) > jw - 1.0 ? 1.0 : (js > 0.55 ? gg_cap(5.0, 5.0, hot * 0.6) : (js > 0.35 ? 4.0 : 3.0));
            gl = 0.6 * I;
        }
        // ice shards flung out (8 chips riding the stream) + snow flecks past the end (6)
        for (int i = 0; i < 8; i++)
        {
            float fi = float(i);
            float ph = frac(zhash1(fi * 2.3 + _Seed) + _FxTime * 1.7);
            float ai = (zhash1(fi * 5.1 + 0.7) - 0.5) * 1.6 * hlf;
            float ri = R * (0.25 + 0.8 * ph);
            float2 cp = floor(d * (cos(ai) * ri) + nr * (sin(ai) * ri) + float2(0.0, -ph * 3.0) + float2(0.5, 0.5));
            float f = gg_chip(p - cp, 1.3);
            if (f > -0.5 && I > 0.4 && ph < 0.92) { row = 0.0; idx = gg_facetidx(f, hot); }
        }
        for (int j = 0; j < 6; j++)
        {
            float fj = float(j);
            float ph = frac(zhash1(fj * 1.9 + _Seed) + _FxTime * 1.5);
            float aj = (zhash1(fj * 3.7) - 0.5) * 2.2 * hlf;
            float rj = R * (0.75 + 0.45 * ph);
            float2 fp = floor(d * (cos(aj) * rj) + nr * (sin(aj) * rj) + float2(0.0, -ph * 5.0) + float2(0.5, 0.5));
            float2 q = abs(p - fp);
            if (q.x + q.y <= 1.0 && I > 0.3) { row = 3.0; idx = (q.x + q.y < 0.5) ? gg_cap(5.0, 5.0, hot) : 4.0; }
        }
    }
    else
    {   // 1 GROUND: ice crystals sprout under the cone, nearest the mouth first, as the hold time grows (8 px cells, ~60 % used)
        float2 cg = floor(p / 8.0);
        float best = 999.0;
        float grow0 = zsat(I * 1.5);
        for (int j = 0; j < 9; j++)
        {
            float2 cc = cg + float2(zmod(float(j), 3.0) - 1.0, floor(float(j) / 3.0) - 2.0);
            float h1 = zhash(cc + float2(_Seed * 13.0, 3.3)), h2 = zhash(cc * 1.7 + float2(9.1, _Seed * 7.0));
            float2 b = floor(cc * 8.0 + float2(1.0 + 6.0 * h1, 1.0 + 6.0 * h2));
            float bu = dot(b, d), bv = dot(b, nr);
            float bn = length(b) / max(_Radius, 1.0);
            float ba = abs(atan2(bv, max(bu, 0.001))) / hlf;
            if (bu > 8.0 && ba < 1.0 && bn < 1.0 && h1 > 0.35)
            {
                float tb = 0.12 + bn * 1.0 + h2 * 0.35;                             // seconds of hold before this one sprouts
                float g = zeaseOut(zsat((_Hold - tb) / 0.3)) * grow0;
                float hh = (3.0 + 5.0 * h2 * (1.0 - bn * 0.4)) * g;
                if (hh >= 2.0)
                {
                    float f = gg_crystal(p - b, hh, 1.0 + h1, (h1 - 0.67) * 5.0);
                    if (f > -0.5 && b.y < best) { best = b.y; row = 0.0; idx = gg_facetidx(f, hot); gl = f > 3.5 ? 0.3 : 0.1; }
                    // rime dots around its foot
                    float2 q = p - b;
                    float rr = length(float2(q.x, q.y * 2.0));
                    if (row < -0.5 && rr < 2.0 + 3.0 * g && rr > 1.0 && zN(_NoiseTex, p / 2.0 + float2(_Seed, h1)).g > 0.5) { row = 3.0; idx = rr < 3.0 ? 4.0 : 3.0; }
                }
            }
        }
    }
    if (_Layer > 0.5)
    {
        float g2 = min(gl, _GlowCap);
        return (g2 > 0.02 && row > -0.5) ? zglow(_RampTex, g2, p, _GlowGain) : gg_none();
    }
    return row < -0.5 ? gg_none() : float4(zramp(_RampTex, row, idx), 1.0);
}
