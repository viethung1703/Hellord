// Gonggong Ice (t1682): solid ice objects drawn as faceted 2.5D crystals (lit / mid / shadow facets + white ridge + 1 px ink),
// y-sorted with the sprites (code: order = the sprite order of the pivot y). Spikes, crystal walls, pillars (stone + ice / Ao),
// falling sky shards and shard projectiles. Debris <= 12 ice chips. 15 fps stepped. Colours: row 0 ice, row 4 stone.

// one crystal of a set, painter-sorted by the caller: returns facet (-1 none). b = base, h height, wd half width, ln lean
float gi_cr(float2 p, float2 b, float h, float wd, float ln) { return gg_crystal(p - b, h, wd, ln); }
// ice chips thrown from 'o' with life a (0..1), n chips, speed sp px/s. returns facet or -1
float gi_chips(float2 p, float2 o, float a, float n, float sp, float seed)
{
    float res = -1.0;
    for (int i = 0; i < 12; i++)
    {
        float fi = float(i);
        if (fi >= n) break;
        float an = zhash1(fi * 2.71 + seed * 17.0) * 6.2831853;
        float v = sp * (0.55 + 0.6 * zhash1(fi * 4.3 + seed));
        float tt = a * 0.6;
        float2 cp = floor(o + gg_ballistic(an, v, 160.0, tt, 0.55) + float2(0.5, 0.5));
        if (cp.y < o.y - 6.0) continue;                          // landed (below its launch ground)
        float f = gg_chip(p - cp, a < 0.6 ? 1.4 : 1.0);
        if (f > -0.5 && a < 1.0) res = f;
    }
    return res;
}
float4 ZFX_Frag(float2 p, float2 w)
{
    float t = gg_t();
    float hot = zsat(_Hot);
    float md = _Mode;
    float face = -1.0;           // facet of the front-most hit
    float row = 0.0;              // 0 ice, 4 stone
    float gl = 0.0;
    float pr = zsat(_Progress);
    float brk = zsat(_Break);
    float ang = _Angle * 0.0174533;
    float2 d = float2(cos(ang), sin(ang)); float2 nr = float2(-d.y, d.x);
    if (md < 0.5)
    {   // 0 SPIKE CLUSTER: _Count spikes erupt (0..0.12), hold, crumble + sink from 0.75. _Size = cluster radius px.
        float n = clamp(_Count, 1.0, 7.0);
        float bestY = 9999.0;
        float sink = zsat((pr - 0.75) / 0.25);
        float gd = 99.0;
        for (int i = 0; i < 7; i++)
        {
            float fi = float(i);
            if (fi >= n) break;
            float an = fi / n * 6.2831853 + _Seed * 6.0;
            float rad = fi < 0.5 ? 0.0 : _Size * (0.45 + 0.3 * zhash1(fi + _Seed * 3.0));
            float2 b = floor(float2(cos(an) * rad, sin(an) * rad * 0.5) + float2(0.5, 0.5));
            float rise = zeaseOut(zsat((pr - fi * 0.012) / 0.1));
            float hmax = (fi < 0.5 ? 1.7 : 0.9 + 0.5 * zhash1(fi * 7.0 + _Seed)) * _Size;
            float h = floor(hmax * rise * (1.0 - sink * 0.85));
            float wd = max(1.5, (fi < 0.5 ? 0.42 : 0.3) * _Size);
            float ln = (b.x) * 0.35 + (zhash1(fi * 3.0) - 0.5) * 3.0;
            float f = gi_cr(p, b, h, wd, ln);
            if (f > -0.5 && b.y < bestY) { bestY = b.y; face = f; }
            gd = min(gd, length(p - b - float2(ln * 0.5, h * 0.45)) - wd);
        }
        if (face > -0.5 && sink > 0.0 && !zdith(1.0 - sink * 0.9, p)) face = -1.0;
        if (sink > 0.0) { float ch = gi_chips(p, float2(0.0, 2.0), sink, 9.0, 2.4 * _Size, _Seed); if (ch > -0.5 && face < -0.5) face = ch; }
        gl = zsat(1.0 - gd / 7.0) * (0.35 + 0.65 * zsat(1.0 - pr / 0.25)) * (1.0 - sink);
    }
    else if (md < 1.5)
    {   // 1 CRYSTAL WALL: _Count big crystals (+ the same number of smaller fillers) on a line ACROSS the aim, bulging _Bend px toward it.
        float n = clamp(_Count, 2.0, 12.0);
        float bestY = 9999.0;
        float gd = 99.0;
        float sink = brk;
        for (int i = 0; i < 24; i++)
        {
            float fi = float(i);
            float big = zmod(fi, 2.0) < 0.5 ? 1.0 : 0.0;
            float j = floor(fi / 2.0) + (big > 0.5 ? 0.0 : 0.5);
            if (j > n - 1.0 + 0.01) break;
            float s = (j / (n - 1.0) - 0.5);                          // -0.5 .. 0.5 along the wall
            float hv = zhash1(fi * 5.7 + _Seed * 2.0);
            float alt = zmod(j, 2.0) < 0.5 ? 1.0 : -1.0;
            // big crystals stand on the wall line; fillers sit 2-3 px in front of it (toward the aim) as low base crystals
            float2 b = nr * (s * _Length) + d * (_Bend * (1.0 - 4.0 * s * s) + (big > 0.5 ? 0.0 : 2.5)) + float2(0.0, (zhash1(fi * 1.3 + _Seed) - 0.5) * 2.0);
            b = floor(b + float2(0.5, 0.5));
            float stg = abs(s) * 0.12;                                 // rises from the centre outward
            float rise = zeaseOut(zsat((pr - stg) / 0.08));
            float h = floor(_Size * (big > 0.5 ? (0.75 + 0.75 * hv) : (0.3 + 0.25 * hv)) * rise * (1.0 - sink * 0.9));
            float wd = big > 0.5 ? 3.5 + hv : 2.0;
            float ln = alt * (1.5 + 2.5 * hv) + s * 6.0;               // alternating lean: every tip reads on its own
            float f = gi_cr(p, b, h, wd, ln);
            if (f > -0.5 && b.y < bestY)
            {
                bestY = b.y; face = f;
                // damage cracks: ink zig-zags appear on the crystals as _Damage grows
                float cr = zN(_NoiseTex, (p - b) / 6.0 + float2(fi * 0.37, 0.0)).a;
                if (_Damage > 0.05 && cr < 0.05 + 0.12 * _Damage && f > 0.5) face = 0.0;
            }
            gd = min(gd, abs(p.x - b.x - ln * 0.5) + max(0.0, abs(p.y - b.y - h * 0.5) - h * 0.5) - wd);
        }
        if (face > -0.5 && sink > 0.0 && !zdith(1.0 - sink * 1.1, p)) face = -1.0;
        if (sink > 0.0)
        {
            for (int k = 0; k < 3; k++)
            {
                float2 o = floor(nr * ((float(k) - 1.0) * _Length * 0.35) + float2(0.5, 4.0));
                float ch = gi_chips(p, o, sink, 4.0, 30.0, _Seed + float(k) * 0.3);
                if (ch > -0.5 && face < -0.5) face = ch;
            }
        }
        gl = zsat(1.0 - gd / 6.0) * (0.4 + 0.4 * zsat(1.0 - pr / 0.1)) * (1.0 - sink);
    }
    else if (md < 2.5)
    {   // 2 PILLAR. _Progress: rise (0..0.1) + stand. _Break: topple along the aim (0..0.3) + crash rubble (0.3..1).
        //   _Style 2 Buzhou stone + ice crust (Toppled Pillar, falls). _Style 1 Ao ice pillar with shell bands (Pillars of Ao, stands).
        float H = _Height; float hw = floor(_Size * 0.5);
        float rise = zeaseOut(zsat(pr / 0.1));
        float fall = zsat(brk / 0.3);
        float rub = zsat((brk - 0.3) / 0.7);
        float wob = (_Style > 1.5 && pr > 0.7 && fall <= 0.0) ? sin(_FxTime * 40.0) * (pr - 0.7) * 3.0 : 0.0;
        float th = zeaseIn(fall) * 1.5708;
        float2 gd2 = float2(d.x, d.y * 0.62);                          // the fall direction on the ground (perspective squash)
        float2 top = float2(wob, 0.0) + float2(0.0, H * rise) * cos(th) + gd2 * (H * sin(th));
        float2 ax = top; float L = max(length(ax), 1.0); float2 an = ax / L; float2 aperp = float2(-an.y, an.x);
        float u = dot(p, an), v = dot(p, aperp);
        float stoneRow = _Style > 1.5 ? 4.0 : 0.0;
        if (rub <= 0.0)
        {
            // profile: plinth (u < 4) and capital (last 5 px) 2 px wider than the shaft; Ao pillars end in a crystal point
            bool ao = _Style > 0.5 && _Style < 1.5;
            float capL = ao ? 0.0 : 5.0;
            float tipL = ao ? hw * 1.6 : 0.0;
            float Ls = L - tipL;
            float hww = hw * (1.0 - 0.1 * zsat(u / L));
            if (!ao && (u < 4.0 || u > Ls - capL)) hww = hw + 2.0;
            if (ao && u > Ls) hww = hw * (1.0 - (u - Ls) / max(tipL, 1.0)) + 0.4;
            if (u >= -1.0 && u <= L && abs(v) <= hww + 1.0)
            {
                float vv = v / max(hww, 1.0);
                float f = vv < -0.4 ? 3.0 : (vv < 0.3 ? 2.0 : 1.0);
                if (abs(vv + 0.55) < 0.14) f = 4.0;
                if (abs(v) > hww - 0.5 || u < -0.5 || u > L - 0.5) f = 0.0;
                if (!ao && f > 0.5 && (abs(u - 4.0) < 0.5 || abs(u - (Ls - capL)) < 0.5)) f = 0.0;   // plinth / capital ink lines
                face = f;
                if (_Style > 1.5 && f > 0.5)
                {   // Buzhou stone: masonry blocks (joints every 10 px, staggered vertical joints), ice crust patches + rime on the capital
                    float4 n4 = zN(_NoiseTex, float2(u, v) / 12.0 + float2(_Seed, 0.0));
                    row = 4.0;
                    float blk = floor((u - 4.0) / 10.0);
                    if (u > 4.0 && u < Ls - capL && zmod(u - 4.0, 10.0) < 1.0) face = 1.0;
                    if (u > 4.0 && u < Ls - capL && abs(v - (zmod(blk, 2.0) < 0.5 ? -1.0 : 2.0)) < 0.5 && f > 1.5) face = 1.0;
                    if (n4.r > 0.6 || u > Ls - capL) { row = 0.0; if (f > 2.5 && n4.g > 0.6) face = 4.0; }   // ice crust (row 0)
                }
                if (ao && f > 0.5)
                {   // Ao: hex scale plates every 6 px (offset per column), a cyan core line on the lit side, the tip flashes on _Pulse
                    float bnd = zmod(floor(u + (zmod(floor((v + 8.0) / 3.0), 2.0)) * 3.0), 6.0);
                    if (bnd < 1.0 && u < Ls) face = 1.0;
                    if (abs(vv + 0.15) < 0.12 && u > 3.0 && u < Ls) face = 3.0;
                    if (_Pulse > 0.3 && u > Ls - 2.0) face = 4.0;
                }
                if (_Damage > 0.05 && f > 0.5 && zN(_NoiseTex, float2(u, v) / 6.0).a < 0.05 + 0.1 * _Damage) face = 0.0;
                gl = ao ? 0.3 + 0.5 * _Pulse * zsat((u - Ls * 0.5) / (Ls * 0.5)) : (fall > 0.0 ? 0.3 : 0.0);
            }
            // icicles under the capital (stone pillar): 3 x 1 px wide, 3-4 px long
            if (!ao && face < -0.5 && fall <= 0.0)
            {
                float uu2 = Ls - capL - u;
                for (int k = 0; k < 3; k++)
                {
                    float vx = (float(k) - 1.0) * (hw + 1.0) * 0.8 + (zmod(float(k), 2.0) - 0.5);
                    float ln2 = 3.0 + zmod(float(k), 2.0);
                    if (abs(v - vx) < 0.6 + 0.4 * (1.0 - uu2 / ln2) && uu2 > 0.0 && uu2 < ln2 && abs(v) > hw - 0.5) { face = 3.0; row = 0.0; }
                }
            }
            // rising: ground cracks + 6 chips burst at the base while it rises
            if (pr < 0.12 && face < -0.5) { float ch = gi_chips(p, float2(0.0, 0.0), pr / 0.12, 6.0, 55.0, _Seed); if (ch > -0.5) face = ch; }
        }
        else
        {   // crash: the pillar lies along the fall line, broken into 5 chunks that separate and sink; 10 chips thrown; dust of frost
            float2 g = gd2 * H;
            float Lg = max(length(g), 1.0); float2 gn = g / Lg; float2 gp = float2(-gn.y, gn.x);
            float uu = dot(p, gn), vv = dot(p, gp);
            // 5 masonry chunks along the fall line, each a 3/4-view block (lit top face over a front face), irregular sizes,
            // drifting apart and sinking with rub
            for (int k = 0; k < 5; k++)
            {
                float fk = float(k);
                float hk = zhash1(fk * 3.3 + _Seed * 5.0);
                float cu0 = (fk + 0.5) * (Lg / 5.0) + (hk - 0.5) * 3.0 + (fk - 2.0) * rub * 2.0;
                float clen = Lg / 5.0 * (0.6 + 0.3 * hk) * (1.0 - rub * 0.3);
                float cw = hw * (0.75 + 0.4 * zhash1(fk * 1.7 + _Seed)) * (1.0 - rub * 0.5);
                float voff = (zhash1(fk * 7.1) - 0.5) * 5.0;
                float2 cc = gn * cu0 + gp * voff;
                float2 q = p - floor(cc + float2(0.5, 0.5));
                float top = 3.0 * (1.0 - rub * 0.6);
                float qu = dot(q, gn);
                float qy = q.y;
                if (abs(qu) < clen * 0.5 && qy > -cw * 0.5 && qy < cw * 0.5 + top)
                {
                    float fc = qy > cw * 0.5 ? 3.0 : (qu < 0.0 ? 2.0 : 1.0);
                    if (abs(qu) > clen * 0.5 - 1.0 || qy < -cw * 0.5 + 1.0 || qy > cw * 0.5 + top - 1.0) fc = 0.0;
                    if (abs(qy - cw * 0.5) < 0.5) fc = 0.0;
                    face = fc;
                    row = (_Style > 1.5 && hk < 0.65) ? 4.0 : 0.0;
                    if (!zdith(1.0 - rub * 1.05, p)) face = -1.0;
                }
            }
            float ch = gi_chips(p, g * 0.5, rub * 1.4, 10.0, 75.0, _Seed + 0.4);
            if (ch > -0.5 && face < -0.5) face = ch;
            float flash = zsat(1.0 - rub / 0.15);
            gl = (abs(vv) < 6.0 && uu > 0.0 && uu < Lg) ? 0.6 * flash : 0.0;
        }
    }
    else if (md < 3.5)
    {   // 3 FALLING SKY SHARD: drops from _Height px over 0..0.5 (accelerating), shatters at 0.5, a short stub stays and sinks
        float fallT = zsat(pr / 0.5);
        float y0 = _Height * (1.0 - fallT * fallT);
        float hit = zsat((pr - 0.5) / 0.5);
        if (fallT < 1.0)
        {
            float2 b = float2(floor(-y0 * 0.08 + 0.5), floor(y0 + 0.5));
            float f = gi_cr(p, b, _Size, 2.5, 1.5);
            if (f > -0.5) face = f;
            // streak above the falling shard (2 px wide, cyan -> blue), stepped
            if (face < -0.5 && abs(p.x - b.x - 1.0) < 1.0 && p.y > b.y + _Size && p.y < b.y + _Size + 10.0 * fallT) { face = 2.0; }
            // ground shadow grows as it falls (3 px dark ellipse dither) -> telegraph
            float sh = length(float2(p.x / 4.0, p.y / 1.6));
            if (face < -0.5 && sh < 1.0 && zdith(fallT * 1.3, p)) { face = 0.0; }
            gl = (face > 1.5) ? 0.5 : 0.0;
        }
        else
        {
            float stub = floor(_Size * 0.55 * (1.0 - zsat((hit - 0.5) / 0.5)));
            float f = gi_cr(p, float2(0.0, -1.0), stub, 2.5, 0.0);
            if (f > -0.5) face = f;
            float ch = gi_chips(p, float2(0.0, 2.0), hit * 1.3, 9.0, 40.0, _Seed);
            if (ch > -0.5 && face < -0.5) face = ch;
            // impact star: capped, 0.1 s
            float sa = zsat(1.0 - hit / 0.2);
            float2 aq = abs(p - float2(0.0, 2.0));
            if (face < -0.5 && sa > 0.0 && ((aq.y < 1.0 && aq.x < 7.0 * sa) || (aq.x < 1.0 && aq.y < 5.0 * sa))) face = (aq.x + aq.y < 2.5) ? 4.0 : 3.0;
            gl = zsat(1.0 - length(p - float2(0.0, 3.0)) / 14.0) * sa * 1.2;
        }
    }
    else
    {   // 4 SHARD / ICICLE PROJECTILE along _Dir: length _Size px, 3 px wide crystal (white core), frost glints behind
        float2 dd = _Dir.xy / max(length(_Dir.xy), 0.001); float2 dp = float2(-dd.y, dd.x);
        float u = dot(p, dd), v = dot(p, dp);
        float L = _Size;
        float tip = zsat((u - L * 0.25) / (L * 0.25));
        float hwv = 1.6 * (1.0 - tip) + 0.5;
        if (u > -L * 0.5 && u < L * 0.5 && abs(v) < hwv + 1.0)
        {
            face = abs(v) > hwv ? 0.0 : (v > 0.4 ? 4.0 : (v > -0.6 ? 3.0 : 2.0));
            if (abs(v) <= 0.7 && u > -L * 0.25 && u < L * 0.3) face = 4.0;
        }
        if (face < -0.5 && u < -L * 0.5 && u > -L * 0.5 - 7.0 && abs(v) < 1.0)
        {
            float k = floor(-u - L * 0.5);
            if (zhash(float2(k, floor(_FxTime * 15.0) + _Seed * 9.0)) > 0.55) face = 2.0;
        }
        gl = zsat(1.0 - (abs(v) + max(0.0, abs(u) - L * 0.3)) / 5.0);
    }
    float4 c = gg_none();
    if (face > -0.5)
    {
        float idx = gg_facetidx(face, hot);
        if (row > 3.5) idx = face < 0.5 ? 0.0 : (face < 1.5 ? 2.0 : (face < 2.5 ? 4.0 : (face < 3.5 ? 5.0 : 6.0)));   // stone ramp
        c = float4(zramp(_RampTex, row, idx), 1.0);
    }
    if (_Layer > 0.5)
    {
        float g2 = min(gl, _GlowCap);
        return g2 > 0.02 ? zglow(_RampTex, g2, p, _GlowGain) : gg_none();
    }
    return c;
}
