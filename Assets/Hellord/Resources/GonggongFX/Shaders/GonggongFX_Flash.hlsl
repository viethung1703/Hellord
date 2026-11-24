// Gonggong Flash (t1682): short impacts over sprites. Leizu v2.1 lessons built in: _Hot caps the white area / duration, _GlowCap caps
// the glow, and nothing pre-flashes: frame 0 IS the impact (hooks can spawn these at the hit moment). Debris <= 12 bits.
// 0 ice hit (chill), 1 water splash crown, 2 water spout (whirlpool collapse), 3 shatter burst, 4 rime proc, 5 sky crack.
// Colours: row 0 ice, row 1 water, row 5 drown (tint 1), row 3 frost.

// crystal shard lying along direction dn (for bursts): q relative to the shard centre, len, half width. facet or -1
float gfl_shard(float2 q, float2 dn, float len, float hw)
{
    float2 dp = float2(-dn.y, dn.x);
    float u = dot(q, dn), v = dot(q, dp);
    float tip = zsat((abs(u) - len * 0.2) / (len * 0.3));
    float hv = hw * (1.0 - tip) + 0.4;
    float res = -1.0;
    if (abs(u) < len * 0.5 + 0.5 && abs(v) < hv + 1.0) res = abs(v) > hv ? 0.0 : (v > hv * 0.25 ? 4.0 : (v > -hv * 0.3 ? 3.0 : 2.0));
    return res;
}
float4 ZFX_Frag(float2 p, float2 w)
{
    float t = gg_t();
    float hot = zsat(_Hot);
    float pr = zsat(_Progress);
    float S = _Size;
    float row = -1.0, idx = 0.0, gl = 0.0;
    float wrow = _Tint > 0.5 ? 5.0 : 1.0;      // water row
    if (_Mode < 0.5)
    {   // 0 ICE HIT: frame A (first 30 %) frost star + 4 shards jutting out; frame B shards fly, frost puff thins. 0.25 s.
        float A = zsat(1.0 - pr / 0.3);
        float2 q = p;
        float sx = abs(q.x), sy = abs(q.y);
        float arm = S * (0.6 + 0.4 * A);
        // frost star (plus + diagonal), white heart capped
        if (pr < 0.55)
        {
            if ((sy < 1.0 && sx < arm) || (sx < 1.0 && sy < arm * 0.75)) { row = 0.0; idx = (sx + sy < 1.6 + 1.4 * hot * A) ? gg_cap(5.0, 5.0, hot * A + 0.2) : 3.0; }
            if (abs(sx - sy) < 0.8 && sx + sy < arm * 0.9 && sx + sy > 1.0) { row = 0.0; idx = 4.0; }
        }
        // 4 shards on the diagonals, flying out
        for (int i = 0; i < 4; i++)
        {
            float fi = float(i);
            float an = fi * 1.5708 + 0.785 + (zhash1(fi + _Seed * 5.0) - 0.5) * 0.5;
            float2 dn = float2(cos(an), sin(an));
            float dist = S * (0.5 + 1.1 * zeaseOut(pr));
            float2 cpos = floor(dn * dist + float2(0.0, -pr * pr * 6.0) + float2(0.5, 0.5));
            float f = gfl_shard(p - cpos, dn, 4.0, 1.2);
            if (f > -0.5 && pr < 0.85) { row = 0.0; idx = gg_facetidx(f, hot * A); }
        }
        // frost puff (dither), thinning
        float rr = length(p) / (S * (0.8 + pr));
        if (row < -0.5 && rr < 1.0 && zdith((1.0 - rr) * (1.0 - pr) * 1.4, p + float2(t * 15.0, 0.0))) { row = 3.0; idx = rr < 0.5 ? 4.0 : 3.0; }
        gl = max(zsat(1.0 - length(p) / (S * 0.9)) * (0.3 + 0.7 * A), (row == 0.0 && idx > 3.5) ? 0.5 : 0.0);
    }
    else if (_Mode < 1.5)
    {   // 1 SPLASH CROWN: 8 rising crown fingers on an ellipse, central splash column, broken foam ring, falling droplets (<= 12)
        float up = zeaseOut(zsat(pr / 0.35));
        float dn2 = zsat((pr - 0.35) / 0.65);
        float hgt = S * 0.6 * up * (1.0 - dn2 * dn2);
        float2 g = float2(p.x, p.y / 0.5);
        float rr = length(g) / S;
        // foam ring on the ground: broken by noise (not a flat ring)
        float nz = zN(_NoiseTex, g / 5.0 + float2(_Seed, t * 0.3)).g;
        if (abs(rr - (0.7 + 0.5 * pr)) < 0.12 && nz > 0.35 + pr * 0.4) { row = wrow; idx = nz > 0.7 ? 5.0 : 4.0; }
        // crown fingers
        for (int i = 0; i < 8; i++)
        {
            float fi = float(i);
            float an = fi / 8.0 * 6.2831853 + _Seed * 3.0;
            float2 bp = floor(float2(cos(an) * S * (0.7 + 0.3 * pr), sin(an) * S * 0.38 * (0.7 + 0.3 * pr)) + float2(0.5, 0.5));
            float hh = hgt * (0.7 + 0.5 * zhash1(fi + _Seed));
            float2 q = p - bp;
            float fw = 1.6 * (1.0 - q.y / max(hh + 1.0, 1.0)) + 0.4;          // tapered finger: 3 px at the base, 1 px at the tip
            if (abs(q.x) <= fw && q.y >= 0.0 && q.y <= hh) { row = wrow; idx = (q.y > hh - 2.0) ? gg_cap(6.0, 6.0, hot) : (q.x < 0.0 ? 5.0 : 3.0); }
            // finger tip droplet thrown outward on the way down
            float2 dp = bp + float2(cos(an) * dn2 * 8.0, hh + 2.0 - dn2 * dn2 * 18.0);
            float dd = gg_drop(p - floor(dp + float2(0.5, 0.5)), 1.2);
            if (dn2 > 0.05 && dd > -0.5 && fi < 6.0 && dp.y > -6.0) { row = wrow; idx = dd < 0.5 ? 0.0 : (dd > 1.5 ? 6.0 : 4.0); }
        }
        // central column
        float ch = S * 1.1 * up * (1.0 - dn2);
        if (abs(p.x) <= 1.5 && p.y >= 0.0 && p.y <= ch) { row = wrow; idx = p.y > ch - 2.5 ? gg_cap(6.0, 6.0, hot) : (p.x < 0.0 ? 5.0 : 4.0); }
        else if (abs(p.x) <= 2.5 && p.y >= 0.0 && p.y <= ch - 1.0) { row = wrow; idx = 0.0; }
        gl = (row > -0.5 && idx > 3.5) ? 0.5 * (1.0 - pr) : 0.0;
    }
    else if (_Mode < 2.5)
    {   // 2 SPOUT: a water column bursts up to _Height (0..0.35), its top freezes into ice crystals (white glints), then falls (0.5..1)
        float up = zeaseOut(zsat(pr / 0.35));
        float dn2 = zsat((pr - 0.5) / 0.5);
        float H = _Height * up * (1.0 - dn2 * 0.8);
        float hw = S * 0.32 * (1.0 + 0.4 * dn2);
        float wav = sin(p.y * 0.5 - _FxTime * 30.0) * 1.0;
        float x = p.x - wav;
        if (p.y >= -2.0 && p.y <= H && abs(x) <= hw + 1.0)
        {
            row = wrow;
            idx = abs(x) > hw ? 0.0 : (x < -hw * 0.3 ? 5.0 : (x < hw * 0.3 ? 4.0 : 3.0));
            float f = zN(_NoiseTex, float2(x, p.y - _FxTime * 60.0) / 5.0).r;
            if (f > 0.68 && idx > 2.5) idx = gg_cap(6.0, 6.0, hot);
            if (dn2 > 0.0 && !zdith(1.0 - dn2 * 1.1, p)) row = -1.0;
        }
        // frozen crown: 5 crystals at the top
        for (int i = 0; i < 5; i++)
        {
            float fi = float(i);
            float an = 1.5708 + (fi - 2.0) * 0.55;
            float2 cpos = floor(float2(0.0, H) + float2(cos(an), sin(an)) * (3.0 + 3.0 * up) + float2(0.0, -dn2 * dn2 * H * 0.9) + float2(0.5, 0.5));
            float f = gfl_shard(p - cpos, float2(cos(an), sin(an)), 4.0, 1.2);
            if (f > -0.5 && pr > 0.25) { row = 0.0; idx = gg_facetidx(f, hot); }
        }
        // ground foam at the base (broken), 4 droplets
        float2 g = float2(p.x / (S * 0.9), p.y / (S * 0.3));
        float nz = zN(_NoiseTex, p / 4.0 + float2(t * 0.6, _Seed)).g;
        if (row < -0.5 && length(g) < 1.0 && length(g) > 0.55 && nz > 0.4) { row = wrow; idx = nz > 0.7 ? 5.0 : 3.0; }
        gl = (p.y > H - 8.0 && p.y < H + 6.0 && abs(p.x) < 8.0) ? 0.6 * up * (1.0 - dn2) : 0.0;
    }
    else if (_Mode < 3.5)
    {   // 3 SHATTER BURST (radius _Size): 6 big shards + 6 chips fly ballistic, frost dust ring of puffs, a 2-frame capped white heart
        float A = zsat(1.0 - pr / 0.25);
        if (A > 0.0)
        {
            float2 a2 = abs(p - float2(0.0, 8.0));
            float hr = (3.0 + S * 0.15) * zsat(A * 1.6);
            if (a2.x + a2.y < hr) { row = 0.0; idx = gg_cap(5.0, 5.0, hot); }
            else if (a2.x + a2.y < hr + 2.0) { row = 0.0; idx = 3.0; }
        }
        // frost dust: puffs expanding to S, dithered thin (no ring outline)
        float2 g = float2(p.x, (p.y - 4.0) / 0.6);
        float rr = length(g) / (S * zeaseOut(pr * 1.4));
        float nz = zN(_NoiseTex, g / 6.0 + float2(_Seed, pr)).r;
        if (row < -0.5 && rr < 1.0 && rr > 0.35 && zdith((nz - 0.25) * (1.0 - pr) * 2.2, p)) { row = 3.0; idx = nz > 0.6 ? 4.0 : 3.0; }
        // shards
        for (int i = 0; i < 12; i++)
        {
            float fi = float(i);
            float big = fi < 6.0 ? 1.0 : 0.0;
            float an = zhash1(fi * 1.37 + _Seed * 9.0) * 6.2831853;
            float sp = S * (big > 0.5 ? 2.2 : 3.0) * (0.6 + 0.5 * zhash1(fi * 3.1));
            float tt = 0.55 * zeaseOut(pr) * (0.5 + 0.5 * pr);
            float2 o = float2(0.0, 8.0) + gg_ballistic(an, sp, 150.0, tt, 0.6);
            if (o.y < -3.0) continue;
            float2 dn = normalize(float2(cos(an), sin(an) * 0.6 + 0.4 - tt));
            float f = big > 0.5 ? gfl_shard(p - floor(o + float2(0.5, 0.5)), dn, 7.0, 2.0) : gg_chip(p - floor(o + float2(0.5, 0.5)), 1.3);
            if (f > -0.5 && pr < 0.95) { row = 0.0; idx = gg_facetidx(f, hot * (0.3 + A)); }
        }
        gl = max(zsat(1.0 - length(p - float2(0.0, 8.0)) / (S * 0.4)) * A, (row == 0.0 && idx > 2.5) ? 0.45 * (1.0 - pr) : 0.0);
    }
    else if (_Mode < 4.5)
    {   // 4 RIME PROC: 3 ice spikes jut from the hero toward the attacker (_Dir) and hold briefly, then crumble
        float2 dn = _Dir.xy / max(length(_Dir.xy), 0.001); float2 dp = float2(-dn.y, dn.x);
        float g = zeaseOut(zsat(pr / 0.15)); float cr = zsat((pr - 0.6) / 0.4);
        for (int i = 0; i < 3; i++)
        {
            float fi = float(i);
            float2 sd = normalize(dn + dp * (fi - 1.0) * 0.75);
            float len = (S + 4.0 * (1.0 - abs(fi - 1.0))) * g;
            float2 cpos = floor(float2(0.0, 10.0) + sd * (5.0 + len * 0.5) + float2(0.5, 0.5));
            float f = gfl_shard(p - cpos, sd, len, 1.8);
            if (f > -0.5 && zdith(1.0 - cr * 1.1, p)) { row = 0.0; idx = gg_facetidx(f, hot); }
        }
        float2 bp = float2(0.0, 10.0) + dn * (S + 7.0);
        float rr = length(p - bp) / (5.0 + 4.0 * pr);
        if (row < -0.5 && rr < 1.0 && zdith((1.0 - rr) * (1.0 - pr) * 1.5, p)) { row = 3.0; idx = 4.0; }
        gl = zsat(1.0 - length(p - float2(0.0, 10.0) - dn * S * 0.6) / (S * 1.2)) * (1.0 - cr) * 0.7;
    }
    else
    {   // 5 SKY CRACK (Ram of Mount Buzhou): heaven fractures _Height px above the pivot. A lens-shaped RIFT (not a bolt): a jagged
        // polygonal gap, widest (<= 7 px) in the middle, showing the dark deep-water sky behind it, pale ice rims, short 1 px side
        // cracks and loose sky plates; it opens over 0..0.2, holds, then frost seals it (0.7..1).
        float2 q = p - float2(0.0, _Height);
        float open = zeaseOut(zsat(pr / 0.2)) * (1.0 - zsat((pr - 0.7) / 0.3));
        float W = S * 2.2;
        float xn = q.x / W;
        // jagged centre line: straight segments every 8 px with hashed kinks
        float k0 = floor(q.x / 8.0);
        float y0 = (zhash(float2(k0, _Seed * 7.0)) - 0.5) * 5.0, y1 = (zhash(float2(k0 + 1.0, _Seed * 7.0)) - 0.5) * 5.0;
        float cy = lerp(y0, y1, frac(q.x / 8.0)) * zsat(1.0 - abs(xn));
        float hwv = open * 5.0 * zsat(1.0 - xn * xn) + (abs(xn) < 1.0 ? 0.6 : -1.0);
        float dv = abs(q.y - cy);
        if (abs(xn) < 1.0 && dv < hwv + 2.0)
        {
            if (dv < hwv - 1.0) { row = 2.0; idx = zN(_NoiseTex, q / 5.0 + float2(_Seed, 0.0)).r > 0.6 ? 3.0 : 1.0;   // the deep sky behind
                                  if (zhash(floor(q) + float2(_Seed * 13.0, 0.0)) > 0.97) { row = 0.0; idx = 4.0; } }      // a star of ice light
            else if (dv < hwv) { row = 0.0; idx = abs(xn) < 0.35 ? gg_cap(5.0, 5.0, hot) : 4.0; }                     // pale / white rim
            else if (dv < hwv + 1.0) { row = 0.0; idx = 3.0; }
            else { row = 0.0; idx = 0.0; }
        }
        // short 1 px side cracks (5), each a straight 5-9 px line leaving the rift at a slant
        for (int i = 0; i < 5; i++)
        {
            float fi = float(i);
            float sx = (fi - 2.0) / 2.5 * W * 0.8 + (zhash1(fi + _Seed) - 0.5) * 6.0;
            float sd = zmod(fi, 2.0) < 0.5 ? 1.0 : -1.0;
            float2 a0 = float2(sx, sd * (open * 3.0 * zsat(1.0 - (sx / W) * (sx / W)) + 1.0));
            float2 a1 = a0 + float2((zhash1(fi * 3.0) - 0.5) * 8.0, sd * (4.0 + 5.0 * zhash1(fi * 5.0))) * open;
            if (zsegd(q, a0, a1) < 0.6 && open > 0.2 && row < -0.5) { row = 0.0; idx = 4.0; }
        }
        // loose sky plates (4) drifting down from the rim, 3x3 px facets
        for (int j = 0; j < 4; j++)
        {
            float fj = float(j);
            float ph = zsat((pr - 0.15 - fj * 0.08) / 0.5);
            float2 pp = floor(float2((fj - 1.5) * W * 0.4, -3.0 - ph * ph * 30.0) + float2(0.5, 0.5));
            float f = gg_chip(q - pp, 1.4);
            if (f > -0.5 && ph > 0.0 && ph < 1.0) { row = 0.0; idx = gg_facetidx(f, hot); }
        }
        if (pr > 0.7 && row > -0.5 && zdith((pr - 0.7) / 0.3 * 1.2, p)) { row = 3.0; idx = 4.0; }   // frost seals it
        gl = (abs(xn) < 1.0) ? zsat(1.0 - (dv - hwv) / 7.0) * open * zsat(1.0 - abs(xn)) : 0.0;
    }
    if (_Layer > 0.5)
    {
        float g2 = min(gl, _GlowCap);
        return g2 > 0.02 ? zglowT(_RampTex, g2, p, _GlowGain, _Tint) : gg_none();
    }
    return row < -0.5 ? gg_none() : float4(zramp(_RampTex, row, idx), 1.0);
}
