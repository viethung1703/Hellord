// Gonggong Aura (t1682): body overlays drawn over the actor sprite. Pivot = feet; _Body = (w, h) of the body box above the feet.
// v2 (2026-11-25): 2 = ice ENCASEMENT, new 4 = Eternal Winter frost on demons.
// 0 Rime Coat (hero: crust of ice plates on the body outline + one standing crystal per freeze charge), 1 Drowning Mark (water sigil
// over the head, drips, dark violet ripple at the feet), 2 Hypothermia cursed (frost creeping up from the feet, breath puffs,
// sinking flecks), 3 Ram head (ice ram bow with two horn curls ahead of the charging hero, water streaming back).
float4 ZFX_Frag(float2 p, float2 w)
{
    float t = gg_t();
    float hot = zsat(_Hot);
    float I = zsat(_Intensity) * zeaseOut(zsat(_Form));
    float bw = _Body.x, bh = _Body.y;
    float2 c = float2(0.0, bh * 0.5);
    float row = -1.0, idx = 0.0, gl = 0.0;
    if (_Mode < 0.5)
    {   // 0 RIME COAT
        float2 q = (p - c) / float2(bw * 0.5 + 3.0, bh * 0.5 + 3.0);
        float rr = length(q);
        float a = atan2(q.y, q.x);
        float4 n4 = zN(_NoiseTex, float2(a * 6.0, rr * 4.0) / 3.0 + float2(_Seed, 0.0));
        float thick = (0.13 + 0.07 * n4.r) * I;
        float band = abs(rr - 0.92);
        // plates: broken into cells; the top half is sparser so the face reads
        bool keep = n4.b > (q.y > 0.3 ? 0.55 : 0.3);
        if (band < thick && keep)
        {
            row = 0.0;
            idx = band > thick - 0.06 ? 0.0 : (n4.a < 0.12 ? gg_cap(5.0, 5.0, hot) : (q.x < 0.0 ? 4.0 : (n4.g > 0.5 ? 3.0 : 2.0)));
        }
        // standing charge crystals at the hips (one per remaining freeze)
        for (int i = 0; i < 2; i++)
        {
            float fi = float(i);
            if (fi < _Charges - 0.5)
            {
                float sx = fi < 0.5 ? -1.0 : 1.0;
                float2 base = floor(float2(sx * (bw * 0.5 + 3.0), bh * 0.15) + float2(0.5, 0.5));
                float f = gg_crystal(p - base, 9.0 * I + _Pulse * 3.0, 2.0, sx * 1.5);
                if (f > -0.5) { row = 0.0; idx = gg_facetidx(f, hot); }
            }
        }
        // frost motes rising (5)
        for (int j = 0; j < 5; j++)
        {
            float fj = float(j);
            float ph = frac(zhash1(fj + _Seed * 3.0) + _FxTime * 0.6);
            float2 mp = floor(float2((zhash1(fj * 2.3) - 0.5) * (bw + 6.0), bh * (0.1 + ph)) + float2(0.5, 0.5));
            float2 m = abs(p - mp);
            if (m.x + m.y <= 1.0 && I > 0.3 && ph < 0.85) { row = 3.0; idx = (m.x + m.y < 0.5) ? 5.0 : 4.0; }
        }
        gl = (band < thick + 0.15 && keep) ? 0.45 * I : 0.0;
    }
    else if (_Mode < 1.5)
    {   // 1 DROWNING MARK
        float2 sc = floor(float2(0.0, bh + 9.0 + sin(_FxTime * 4.0) * 1.0) + float2(0.5, 0.5));
        float2 q = (p - sc) / 1.35;
        // teardrop sigil (7 x 10): round bottom r 3.5, pointed top
        float yk = zsat((q.y + 0.5) / 6.0);
        float hw = q.y < 0.0 ? sqrt(max(0.0, 12.25 - q.y * q.y)) : 3.5 * (1.0 - yk);
        if (q.y > -4.5 && q.y < 6.5 && abs(q.x) <= hw + 1.0)
        {
            row = 5.0;
            idx = abs(q.x) > hw - 0.3 ? 0.0 : (q.x < -0.8 && q.y > -1.5 ? 6.0 : 4.0);
            // swirling inner eye: a 2 px ring rotating, the pupil dark
            float2 e = zrot(q + float2(0.0, 1.0), _FxTime * 6.0);
            float er = length(e);
            if (er < 1.3) { row = 5.0; idx = 1.0; }
            else if (er < 2.4 && e.x > 0.0) { row = 5.0; idx = gg_cap(6.0, 6.0, hot); }
        }
        // drips falling along the body sides (3)
        for (int i = 0; i < 3; i++)
        {
            float fi = float(i);
            float ph = frac(zhash1(fi * 1.7 + _Seed) + _FxTime * 1.1);
            float2 dp = floor(float2((fi - 1.0) * bw * 0.45, bh * (0.85 - ph)) + float2(0.5, 0.5));
            float2 dq = p - dp;
            if (abs(dq.x) < 0.5 && dq.y > -1.0 && dq.y < 2.0) { row = 5.0; idx = dq.y < 0.5 ? 4.0 : 3.0; }
        }
        // dark violet ripples at the feet: 2 broken ellipses expanding (dithered, not flat)
        float2 g = float2(p.x / (bw * 0.9 + 2.0), p.y / (bw * 0.35 + 1.0));
        float gr = length(g);
        float ph2 = frac(_FxTime * 0.8);
        float nz = zN(_NoiseTex, p / 4.0 + float2(_Seed, t * 0.2)).g;
        for (int k = 0; k < 2; k++)
        {
            float rk = frac(ph2 + float(k) * 0.5) * 1.1 + _Pulse * 0.3;
            if (abs(gr - rk) < 0.09 && nz > 0.3 + rk * 0.35 && row < -0.5) { row = 5.0; idx = rk < 0.6 ? 3.0 : 2.0; }
        }
        if (row < -0.5 && gr < 0.45 && zdith(0.5, p)) { row = 5.0; idx = 1.0; }
        if (row > -0.5 && !zdith(I * 1.1, p)) row = -1.0;
        gl = zsat(1.0 - length(q) / 9.0) * (0.6 + 0.4 * _Pulse) * I;
    }
    else if (_Mode < 2.5)
    {   // 2 HYPOTHERMIA CURSED (v2): the demon is ENCASED as solid ice crystallises over it. _Form 0..1: crystals sprout at the feet, then
        // a faceted translucent ice shell climbs the body to ~80 % of its height (the sprite shows through a 1/3 dither; bright facet
        // edges, cyan rim, a glint sweeping across), with 6 big faceted crystals standing around it (the back one peeks over the shell).
        float F = zsat(_Form); float In = zsat(_Intensity);
        float hs = bh * (0.12 + 0.68 * zeaseOut(zsat((F - 0.25) / 0.75))) * (0.4 + 0.6 * In);
        float sw0 = bw * 0.5 + 3.5;
        // ground rime at the feet
        float2 gq = float2(p.x / (sw0 + 3.0), p.y / 4.0);
        float gr = length(gq);
        if (gr < 1.0 && p.y < 2.0 && F > 0.05)
        {
            float cov = gg_frostcover(_NoiseTex, p + float2(_Seed * 9.0, 0.0), gr * 0.7, zsat(F * 1.5) * In, _Seed);
            if (cov > 1.5) { row = 0.0; idx = gg_cap(4.0, 5.0, hot); }
            else if (cov > 0.5 && zN(_NoiseTex, p / 3.0 + float2(_Seed, 0.2)).g > 0.3) { row = 3.0; idx = 3.0; }
        }
        // the shell
        float yb = p.y;
        if (yb >= -1.0 && hs > 3.0)
        {
            float band = floor((yb + _Seed * 7.0) / 4.0);
            float yn = zsat(yb / hs);
            float sw = sw0 * (1.0 - 0.28 * yn * yn) + (zhash1(band + _Seed * 3.0) - 0.5) * 3.0;
            float col = floor((p.x + 40.0) / 3.0);
            float topE = hs - 1.0 + (zhash1(col * 1.7 + _Seed * 5.0) - 0.3) * 4.0 - abs(p.x) / sw0 * 3.0;
            if (abs(p.x) < sw && yb < topE)
            {
                float4 cn = zN(_NoiseTex, (p + float2(_Seed * 31.0, 0.0)) / 9.0);
                float sh = cn.b;
                bool rim = abs(p.x) > sw - 1.0 || yb > topE - 1.0;
                if (rim) { row = 0.0; idx = yb > topE - 1.0 ? 4.0 : 3.0; }
                else if (cn.a < 0.075) { row = 0.0; idx = gg_cap(sh > 0.5 ? 5.0 : 4.0, 5.0, hot); }        // facet edges
                else if (zbayer(p + float2(sh * 4.0, 0.0)) > 0.14) { row = 0.0; idx = sh > 0.66 ? 4.0 : (sh > 0.33 ? 3.0 : 2.0); }   // translucent facets
                float sweep = zmod(p.x + yb * 0.6 - _FxTime * 24.0 + _Seed * 50.0, 40.0);
                if (sweep < 2.0 && !rim) { row = 0.0; idx = gg_cap(5.0, 5.0, hot); }                       // glint sweeping across
                gl = rim || cn.a < 0.075 ? 0.3 * In : 0.0;
            }
        }
        // 6 big crystals around the body (nearest base in front)
        float bestY = 999.0;
        for (int i = 0; i < 6; i++)
        {
            float fi = float(i);
            float side = zmod(fi, 2.0) < 0.5 ? -1.0 : 1.0;
            float bx = i < 2 ? side * (sw0 + 0.5) : (i < 4 ? side * bw * 0.22 : (i < 5 ? 2.0 : side * (sw0 + 5.0)));
            float by = i < 2 ? -1.0 : (i < 4 ? -3.0 : (i < 5 ? 2.0 : -2.0));
            float hk = i < 2 ? 0.62 : (i < 4 ? 0.34 : (i < 5 ? 0.92 : 0.3));
            float wk = i < 2 ? 2.5 : (i < 4 ? 2.0 : (i < 5 ? 3.0 : 1.5));
            float lk = i < 2 ? side * 3.0 : (i < 4 ? side * 1.5 : (i < 5 ? -1.0 : side * 2.5));
            float2 cb = floor(float2(bx, by) + float2(0.5, 0.5));
            float gk = zeaseOut(zsat(F * 1.8 - fi * 0.13)) * (0.5 + 0.5 * In);
            float hgt = bh * hk * (0.85 + 0.3 * zhash1(fi + _Seed * 3.0)) * gk;
            if (hgt >= 2.0)
            {
                float f = gg_crystal(p - cb, hgt, wk, lk);
                if (f > -0.5 && cb.y < bestY) { bestY = cb.y; row = 0.0; idx = gg_facetidx(f, hot); gl = f > 2.5 ? 0.35 * In : gl; }
            }
        }
        // cold breath puffs at the head every 1.2 s
        float ph = frac(_FxTime / 1.2);
        float2 bp = float2(_Body.z + 3.0 + ph * 6.0, bh * 0.9 + ph * 3.0);
        float br = length(p - bp) / (2.0 + ph * 3.0);
        if (br < 1.0 && zdith((1.0 - br) * (1.0 - ph) * 1.6, p) && row < -0.5 && F > 0.5) { row = 3.0; idx = 4.0; }
        if (row > -0.5 && In < 0.999 && !zdith(In * 1.15, p)) row = -1.0;
    }
    else if (_Mode < 3.5)
    {   // 3 RAM HEAD: ahead of the hero along _Dir (screen space), centred at chest height
        float2 dn = _Dir.xy / max(length(_Dir.xy), 0.001); float2 dp = float2(-dn.y, dn.x);
        float2 hc = float2(0.0, bh * 0.45) + dn * (bw * 0.5 + 4.0);
        float2 q = p - hc;
        float u = dot(q, dn), v = dot(q, dp);
        // ram brow: a 5 px thick ice chevron pointing forward (lit top half, ink rim) + two curled horns sweeping back from its ends
        float wv = u + abs(v) * 0.75 - 7.0;
        if (abs(v) < 11.0 * I && wv < 0.0 && wv > -5.0)
        {
            float f = v > 1.0 ? 3.0 : (v > -2.0 ? 2.0 : 1.0);
            if (wv > -1.0 || wv < -4.0 || abs(v) > 11.0 * I - 1.0) f = 0.0;
            if (abs(v) < 1.5 && wv > -3.0 && f > 0.5) f = 4.0;
            row = 0.0; idx = gg_facetidx(f, hot);
        }
        for (int i = 0; i < 2; i++)
        {
            float sv = i == 0 ? 1.0 : -1.0;
            float2 cc = float2(-4.0, sv * 9.0 * I);
            float2 hq = float2(u, v) - cc;
            float hr = length(hq);
            float ha = atan2(hq.y * sv, -hq.x);                // 0 pointing back
            float sp = 2.0 + zsat((ha + 3.1416) / 6.2832) * 4.5;
            if (abs(hr - sp) < 1.2 && I > 0.4 && hr < 7.0) { row = 0.0; idx = hr - sp > 0.4 ? 0.0 : (hq.y * sv > 0.0 ? 4.0 : 2.0); }
        }
        // water streaming back from the bow (6 streaks)
        for (int j = 0; j < 6; j++)
        {
            float fj = float(j);
            float vj = (fj - 2.5) * 4.0;
            float ph = frac(zhash1(fj + _Seed) + _FxTime * 4.0);
            float us = 2.0 - 4.0 - ph * 20.0;
            float len = 6.0 * (1.0 - ph) + 2.0;
            if (abs(v - vj - sin(u * 0.4) * 0.5) < 0.8 && u < us + len && u > us && I > 0.2) { row = 1.0; idx = ph < 0.4 ? 5.0 : 3.0; }
        }
        gl = zsat(1.0 - length(q - dn * 3.0) / 16.0) * I * 0.7;
    }
    else
    {   // 4 WINTER FROST (Eternal Winter, v2, new): frost settles ON each demon in reach: a rime patch and 3 small crystals at the feet,
        // a jagged snow cap on the head (white top, slate underside), 3 twinkling frost glints on the body, 3 snowflakes drifting down.
        float F = zsat(_Form); float In = zsat(_Intensity) * zeaseOut(F);
        float2 gq = float2(p.x / (bw * 0.6 + 3.0), p.y / 3.5);
        float gr = length(gq);
        if (gr < 1.0 && p.y < 2.0)
        {
            float cov = gg_frostcover(_NoiseTex, p + float2(_Seed * 9.0, 0.0), gr * 0.6, In, _Seed);
            if (cov > 1.5) { row = 0.0; idx = 4.0; }
            else if (cov > 0.5 && zN(_NoiseTex, p / 3.0 + float2(_Seed, 0.2)).g > 0.35) { row = 3.0; idx = 3.0; }
        }
        float bestY = 999.0;
        for (int i = 0; i < 3; i++)
        {
            float fi = float(i);
            float2 cb = floor(float2((fi - 1.0) * bw * 0.45 + (zhash1(fi + _Seed) - 0.5) * 3.0, fi == 1.0 ? -2.0 : 0.0) + float2(0.5, 0.5));
            float hgt = (3.0 + 3.0 * zhash1(fi * 2.3 + _Seed)) * In;
            float f = gg_crystal(p - cb, hgt, 1.2, (fi - 1.0) * 2.0);
            if (hgt >= 2.0 && f > -0.5 && cb.y < bestY) { bestY = cb.y; row = 0.0; idx = gg_facetidx(f, hot); }
        }
        // snow cap
        float capW = bw * 0.38 * (0.6 + 0.4 * In);
        float cx = p.x - _Body.z * 0.5;
        float cyT = bh + 1.0 + (zhash1(floor(cx / 2.0) + _Seed * 9.0) - 0.5) * 2.0;
        float cyB = bh - 2.0 - (1.0 - abs(cx) / max(capW, 1.0)) * 1.5;
        if (abs(cx) < capW && p.y <= cyT * In + bh * (1.0 - In) && p.y >= cyB && In > 0.3)
        {
            row = 3.0; idx = p.y > cyT - 1.0 ? gg_cap(5.0, 5.0, hot) : (p.y < cyB + 1.0 ? 2.0 : 4.0);
        }
        for (int j = 0; j < 3; j++)
        {
            float fj = float(j);
            float ph = frac(zhash1(fj * 4.7 + _Seed) + _FxTime * 0.8);
            float2 gp = floor(float2((zhash1(fj * 1.3 + _Seed) - 0.5) * bw, bh * (0.3 + 0.5 * zhash1(fj * 2.1))) + float2(0.5, 0.5));
            float2 q = abs(p - gp);
            if (ph < 0.25 && In > 0.5 && q.x + q.y <= 1.0 + (ph < 0.12 ? 1.0 : 0.0) && min(q.x, q.y) < 0.5) { row = 0.0; idx = q.x + q.y < 0.5 ? gg_cap(5.0, 5.0, hot) : 4.0; gl = 0.4; }
            float sp = frac(zhash1(fj * 3.1 + _Seed * 2.0) + _FxTime * 0.45);
            float2 fp = floor(float2((zhash1(fj * 7.7) - 0.5) * (bw + 10.0) + sin(_FxTime * 2.0 + fj) * 2.0, bh * 1.12 * (1.0 - sp)) + float2(0.5, 0.5));
            float2 m = abs(p - fp);
            if (m.x + m.y <= 1.0 && row < -0.5 && In > 0.3) { row = 3.0; idx = m.x + m.y < 0.5 ? 5.0 : 4.0; }
        }
    }
    if (_Layer > 0.5)
    {
        float g2 = min(gl, _GlowCap);
        return g2 > 0.02 ? zglowT(_RampTex, g2, p, _GlowGain, _Mode > 0.5 && _Mode < 1.5 ? 1.0 : 0.0) : gg_none();
    }
    return row < -0.5 ? gg_none() : float4(zramp(_RampTex, row, idx), 1.0);
}
