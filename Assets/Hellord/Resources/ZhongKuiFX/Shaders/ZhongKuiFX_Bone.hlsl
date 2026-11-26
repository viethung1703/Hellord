// Zhong Kui Bone (t1709): solid ivory bone, faceted, ink outlines, 3/4 view. Light from the upper left.
// 0 ossuary nova, 1 armour shell, 2 bone wall, 3 colossus merge, 4 colossus body, 5 colossus split.
// Debris <= 15 chips. White is _Hot-capped.

// standing bone giant, q relative to feet, h height px. facet or -1. crack 0..1 eats plates.
float zk_colossus(float2 q, float h, float sway, float crack, sampler2D n)
{
    float res = -1.0;
    if (h < 16.0) return res;
    float2 p = q - float2(sway * zsat(q.y / h), 0.0);
    // legs: thick bone columns
    for (int li = 0; li < 2; li++)
    {
        float side = li == 0 ? -1.0 : 1.0;
        float2 lp = p - float2(side * h * 0.11, 0.0);
        float lh = h * 0.40;
        float hw = 4.6 * (1.0 - 0.20 * zsat(lp.y / lh));
        if (lp.y >= -1.0 && lp.y <= lh && abs(lp.x) <= hw + 1.0)
        {
            if (abs(lp.x) > hw || lp.y < 0.0) res = 0.0;
            else res = lp.x * side < 0.0 ? 4.0 : 1.0;
            if (abs(lp.x) < 1.1 && lp.y > 4.0) res = 4.0;
        }
    }
    // pelvis block
    float2 pq = p - float2(0.0, h * 0.38);
    if (abs(pq.x) <= h * 0.18 + 1.0 && abs(pq.y) <= 5.0)
    {
        if (abs(pq.x) > h * 0.18 || abs(pq.y) > 4.0) res = 0.0;
        else res = pq.y > 1.0 ? 4.0 : (pq.x < 0.0 ? 3.0 : 2.0);
    }
    // ribcage: filled chest with close rib pairs + sternum (mass, not a ladder)
    float2 rc = p - float2(0.0, h * 0.58);
    float crx = 8.2; float cry = h * 0.15;
    float cer = (rc.x * rc.x) / (crx * crx) + (rc.y * rc.y) / (cry * cry);
    if (cer <= 1.12)
    {
        if (cer > 0.92) res = 0.0;
        else
        {
            res = rc.x < -1.5 ? 4.0 : (rc.x < 2.0 ? 3.0 : 1.0);
            // rib grooves
            float groove = abs(zmod(rc.y + cry + 1.0, 4.0) - 2.0);
            if (groove < 0.55 && abs(rc.x) > 1.6) res = 0.0;
        }
    }
    // sternum / spine
    if (abs(rc.x) <= 1.6 && abs(rc.y) <= cry) res = (rc.x < 0.0) ? 4.0 : 3.0;
    // skull (brow, sockets, jaw - not a smile)
    float2 sk = p - float2(0.0, h * 0.86);
    float sd = length(float2(sk.x * 0.95, sk.y * 0.85));
    if (sd <= 7.2)
    {
        if (sd > 6.1) res = 0.0;
        else res = (sk.x < -0.6 && sk.y > 0.2) ? 4.0 : 3.0;
        // brow ridge
        if (sk.y > 2.0 && sk.y < 3.6 && abs(sk.x) < 5.0) res = 0.0;
        // sockets (>= 3 px)
        if (sk.y > -0.6 && sk.y < 1.6 && abs(abs(sk.x) - 2.3) < 1.3) res = 0.0;
        // jaw line (not a rounded smile)
        if (sk.y < -3.0 && sk.y > -4.6 && abs(sk.x) < 3.2) res = (abs(sk.y + 3.6) < 0.5) ? 0.0 : 2.0;
    }
    // shoulder caps
    for (int si = 0; si < 2; si++)
    {
        float side = si == 0 ? -1.0 : 1.0;
        float2 sp = p - float2(side * h * 0.16, h * 0.70);
        if (length(sp) <= 3.4) res = (length(sp) > 2.5) ? 0.0 : (sp.x * side < 0.0 ? 4.0 : 2.0);
    }
    // arms hanging from shoulders (thicker)
    for (int ai = 0; ai < 2; ai++)
    {
        float side = ai == 0 ? -1.0 : 1.0;
        float2 ap = p - float2(side * h * 0.20, h * 0.68);
        float2 q2 = float2(ap.x * side, ap.y);
        if (q2.x >= -2.0 && q2.x <= 4.2 && q2.y <= 2.5 && q2.y >= -h * 0.30)
        {
            float hw = 2.8 + 0.4 * zsat((-q2.y) / (h * 0.30));
            if (abs(q2.x - 1.2) <= hw + 1.0)
            {
                if (abs(q2.x - 1.2) > hw) res = 0.0;
                else res = (q2.x < 1.2) ? 4.0 : 2.0;
            }
        }
    }
    if (res > 0.5 && crack > 0.02)
    {
        float e = zN(n, q / 13.0 + float2(0.2, 0.7)).a;
        if (e < 0.06 + crack * 0.18) res = (e < 0.03 + crack * 0.08) ? -1.0 : 0.0;
    }
    return res;
}

float4 ZFX_Frag(float2 p, float2 w)
{
    float t = zk_t();
    float hot = zsat(_Hot);
    float pr = zsat(_Progress);
    float fade = zsat(_Fade);
    float form = zsat(_Form);
    float brk = zsat(_Break);
    float row = -1.0, idx = 0.0, gl = 0.0;
    if (_Mode < 0.5)
    {   // 0 OSSUARY NOVA: 14 ivory spears burst out to _Radius, plus <= 12 chips from the feet.
        float n = clamp(_Count, 8.0, 16.0);
        float fly = _Radius * zeaseOut(zsat(pr / 0.55));
        float live = zsat(1.0 - fade) * zsat((0.95 - pr) / 0.2 + 0.2);
        for (int i = 0; i < 16; i++)
        {
            float fi = float(i);
            if (fi < n) {
            float an = fi / n * ZTAU + (zhash1(fi + _Seed * 7.0) - 0.5) * 0.22;
            float2 dn = float2(cos(an), sin(an) * _Squash);
            float len = _Size * (0.75 + 0.4 * zhash1(fi + 3.1));
            float2 cpos = floor(dn * max(0.0, fly - len * 0.35) + float2(0.0, 3.0 + 2.0 * zhash1(fi)) + float2(0.5, 0.5));
            float ndl = max(0.001, length(dn));
            float2 u = dn / ndl;
            float f = -1.0;
            float2 q = p - cpos;
            float uu = dot(q, u), vv = dot(q, float2(-u.y, u.x));
            float tip = zsat((uu - len * 0.15) / (len * 0.45));
            float hw = 2.4 * (1.0 - tip) + 0.5;
            if (live > 0.01 && uu > -len * 0.55 && uu < len * 0.55 + 0.5 && abs(vv) < hw + 1.0)
                f = abs(vv) > hw ? 0.0 : (vv > 0.5 ? 4.0 : (vv > -0.6 ? 3.0 : 1.0));
            if (f > -0.5) { row = 0.0; idx = zk_bidx(f, hot * live); gl = max(gl, (idx > 3.5 ? 0.55 : 0.28) * live); }
            }
        }
        // foot burst chips
        for (int k = 0; k < 12; k++)
        {
            float fk = float(k);
            float an = fk / 12.0 * ZTAU + _Seed;
            float2 pos = gg_ballistic(an, 38.0 + 18.0 * zhash1(fk + _Seed), 90.0, pr * 0.7, _Squash);
            float f = gg_chip(p - floor(pos + float2(0.5, 0.5)), 1.0 + zhash1(fk) * 0.6);
            if (f > -0.5 && pr < 0.85 && pos.y > -8.0) { row = 0.0; idx = zk_bidx(f, hot * (1.0 - pr)); }
        }
        // cracked plates at the feet (not a ring)
        float2 g = float2(p.x, p.y / max(_Squash, 0.2));
        float rr = length(g) / 14.0;
        float e = zN(_NoiseTex, p / 9.0 + float2(_Seed, 0.2)).a;
        if (rr < 1.0 && e < 0.10 && pr < 0.45) { row = 0.0; idx = 1.0; }
    }
    else if (_Mode < 1.5)
    {   // 1 BONE ARMOUR: 7 revolving plates around the body, crumble on _Break.
        float2 mid = float2(_Body.z, _Body.y * 0.48);
        float n = clamp(_Count, 5.0, 8.0);
        float live = zsat(_Intensity) * form * (1.0 - fade);
        for (int i = 0; i < 8; i++)
        {
            float fi = float(i);
            if (fi < n) {
            float an = t * 2.1 + fi / n * ZTAU + _Seed * 3.0;
            float2 c0 = mid + float2(cos(an) * (_Body.x * 0.62 + 5.0), sin(an) * (_Body.y * 0.32 + 2.0));
            float2 pos = c0;
            if (brk > 0.01)
            {
                float2 fall = gg_ballistic(an, 22.0, 110.0, brk * 0.55, 1.0);
                pos = c0 + fall;
            }
            float f = gg_crystal(p - floor(pos + float2(0.5, -_Size * 0.3)), _Size * live * (1.0 - brk * 0.4), 2.4, (zhash1(fi) - 0.5) * 2.0);
            if (f > -0.5 && live > 0.05 && pos.y > -10.0) { row = 0.0; idx = zk_bidx(f, hot * live); gl = max(gl, 0.35 * live); }
            }
        }
    }
    else if (_Mode < 2.5)
    {   // 2 BONE WALL: irregular pillars across the aim, cracks with _Damage, collapse with _Break.
        float n = clamp(_Count, 4.0, 8.0);
        float ang = _Angle * ZPI / 180.0;
        float2 d = float2(cos(ang), sin(ang));
        float2 nr = float2(-d.y, d.x);
        float live = (1.0 - fade);
        for (int i = 0; i < 8; i++)
        {
            float fi = float(i);
            if (fi < n) {
            float u = (fi - (n - 1.0) * 0.5) * (_Length / max(n, 1.0));
            float jit = (zhash1(fi + _Seed * 5.0) - 0.5) * 3.0;
            float2 base = floor(nr * u + d * jit + float2(0.5, 0.5));
            float hh = _Size * (0.75 + 0.5 * zhash1(fi + 1.7)) * live;
            if (brk > 0.01) hh *= zsat(1.0 - brk * 1.4);
            float lean = (zhash1(fi + 4.2) - 0.5) * 3.0;
            float f = gg_crystal(p - base, hh, 3.0 + zhash1(fi) * 1.2, lean);
            if (f > -0.5)
            {
                row = 0.0; idx = zk_bidx(f, hot);
                if (_Damage > 0.05)
                {
                    float e = zN(_NoiseTex, (p - base) / 8.0 + float2(fi, _Seed)).a;
                    if (e < 0.05 + _Damage * 0.16) { idx = 0.0; if (e < _Damage * 0.07) row = -1.0; }
                }
                gl = max(gl, (idx > 3.5 ? 0.45 : 0.22) * live);
            }
            // break chips
            if (brk > 0.05)
            {
                float2 pos = base + gg_ballistic(fi * 0.9 + _Seed, 28.0, 100.0, brk * 0.6, 0.8);
                float ch = gg_chip(p - floor(pos + float2(0.5, 0.5)), 1.2);
                if (ch > -0.5 && pos.y > -8.0) { row = 0.0; idx = zk_bidx(ch, hot); }
            }
            }
        }
    }
    else if (_Mode < 3.5)
    {   // 3 COLOSSUS MERGE: 14 chunks spiral in and fuse into the giant (form = pr).
        float n = clamp(_Count, 10.0, 15.0);
        float bodyF = zeaseOut(zsat((pr - 0.35) / 0.65));
        if (bodyF > 0.05)
        {
            float f = zk_colossus(p, _Height * (0.45 + 0.55 * bodyF), 0.0, 0.0, _NoiseTex);
            if (f > -0.5) { row = 0.0; idx = zk_bidx(f, hot * bodyF); gl = max(gl, 0.35 * bodyF); }
        }
        for (int i = 0; i < 15; i++)
        {
            float fi = float(i);
            if (fi < n) {
            float arrive = 0.20 + 0.55 * zhash1(fi + _Seed);
            float k = zeaseOut(zsat(pr / arrive));
            float an = zhash1(fi * 1.7 + _Seed) * ZTAU + t * 1.8 * (1.0 - k);
            float rad = lerp(_Radius, 3.0, k);
            float2 pos = float2(cos(an), sin(an) * 0.62) * rad + float2(0.0, 8.0 * k);
            float f = gg_crystal(p - floor(pos + float2(0.5, -4.0)), _Size * (0.5 + 0.5 * (1.0 - k)), 2.2, (zhash1(fi) - 0.5) * 3.0);
            if (f > -0.5 && k < 0.97) { row = 0.0; idx = zk_bidx(f, hot); gl = max(gl, 0.3); }
            }
        }
    }
    else if (_Mode < 4.5)
    {   // 4 COLOSSUS BODY: idle sway, intensity fades it.
        float live = zsat(_Intensity) * form * (1.0 - fade);
        float sway = sin(t * 1.3 + _Seed) * 1.2 * live;
        float f = zk_colossus(p, _Height * live, sway, 0.0, _NoiseTex);
        if (f > -0.5 && live > 0.05) { row = 0.0; idx = zk_bidx(f, hot * live); gl = max(gl, 0.32 * live); }
    }
    else
    {   // 5 COLOSSUS SPLIT: cracks, then 14 plates fly out.
        float crack = zsat(pr * 1.4);
        float live = zsat(1.0 - zeaseIn(zsat((pr - 0.35) / 0.65))) * (1.0 - fade);
        if (live > 0.05)
        {
            float f = zk_colossus(p, _Height, 0.0, crack, _NoiseTex);
            if (f > -0.5) { row = 0.0; idx = zk_bidx(f, hot * live); gl = max(gl, 0.4 * live); }
        }
        float n = clamp(_Count, 10.0, 15.0);
        for (int i = 0; i < 15; i++)
        {
            float fi = float(i);
            if (fi < n) {
            float an = fi / n * ZTAU + _Seed;
            float2 pos = gg_ballistic(an, 36.0 + 20.0 * zhash1(fi), 95.0, max(0.0, pr - 0.25) * 0.85, 0.7);
            pos.y += _Height * 0.45;
            float f = gg_crystal(p - floor(pos + float2(0.5, -5.0)), _Size * 0.7, 2.2, (zhash1(fi) - 0.5) * 4.0);
            if (f > -0.5 && pr > 0.25 && pos.y > -10.0) { row = 0.0; idx = zk_bidx(f, hot); }
            }
        }
    }
    if (row < -0.5) return zk_none();
    if (_Layer > 0.5)
    {
        gl = min(gl, _GlowCap);
        if (gl < 0.02) return zk_none();
        return zk_glow(_RampTex, gl, p + w * 0.01, _GlowGain, 0.0);
    }
    return float4(zramp(_RampTex, row, idx), 1.0);
}
