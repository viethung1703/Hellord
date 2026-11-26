// Zhong Kui Ghost / Soul (t1709): ectoplasm, pale-green soul fire, dark rims, wispy edges. Not a flat wash.
// 0 army wave, 1 banquet retinue, 2 ghost step, 3 soul bolt, 4 soul burst, 5 plague mark, 6 plague spread, 7 emerging ghost.
// White is _Hot-capped. Features stay >= 3 px.

float zk_soulidx(float kind, float hot)
{
    // kind: 0 ink, 1 deep, 2 mid, 3 bright, 4 pale, 5 white
    float i = kind < 0.5 ? 0.0 : (kind < 1.5 ? 1.0 : (kind < 2.5 ? 3.0 : (kind < 3.5 ? 4.0 : (kind < 4.5 ? 6.0 : 7.0))));
    return zk_cap(i, 7.0, hot);
}

float4 ZFX_Frag(float2 p, float2 w)
{
    float t = zk_t();
    float hot = zsat(_Hot);
    float pr = zsat(_Progress);
    float fade = zsat(_Fade);
    float form = zsat(_Form);
    float inten = zsat(_Intensity);
    float row = -1.0, idx = 0.0, gl = 0.0;
    if (_Mode < 0.5)
    {   // 0 ARMY OF THE DEAD: a pack of ghost soldiers charges along aim, 2.5 u wide, not a wash.
        float ang = _Angle * ZPI / 180.0;
        float2 d = float2(cos(ang), sin(ang));
        float2 nr = float2(-d.y, d.x);
        float front = _Length * zeaseOut(pr);
        float n = clamp(_Count, 5.0, 10.0);
        float live = inten * (1.0 - fade);
        for (int i = 0; i < 10; i++)
        {
            float fi = float(i);
            if (fi < n)
            {
                float lane = ((zhash1(fi + _Seed) - 0.5) * 2.0) * _Width;
                float along0 = zhash1(fi + 2.3) * 22.0;
                float spacing = 18.0;
                float along = front - along0 - fi * spacing * 0.55;
                // loop a second rank behind so the pack feels continuous over 6 s
                if (along < front - 86.0) along += 86.0;
                if (along > 8.0 && along < front + 2.0 && along < _Length + 6.0)
                {
                    float bob = sin(t * 9.0 + fi * 1.7) * 1.0;
                    float2 gp = d * along + nr * lane + float2(0.0, 2.0 + bob);
                    float gh = _Size * (0.85 + 0.25 * zhash1(fi + 4.0)) * live;
                    float f = zk_ghost(p - floor(gp + float2(0.5, 0.5)), gh);
                    if (f > -0.5)
                    {
                        row = 1.0;
                        idx = f < 0.5 ? 0.0 : (f < 1.5 ? zk_soulidx(2.0, hot) : zk_soulidx(4.0, hot));
                        gl = max(gl, 0.32 * live);
                    }
                    // short ectoplasm streak behind each soldier (3+ px, broken)
                    float2 q = p - floor(gp + float2(0.5, 0.5));
                    float back = dot(q, -d);
                    float lat = abs(dot(q, nr));
                    if (row < -0.5 && back > 1.0 && back < 8.0 && lat < 2.2 && zdith(0.7 * live * (1.0 - back / 8.0), p + float2(t, fi)))
                    { row = 1.0; idx = 3.0; }
                }
            }
        }
    }
    else if (_Mode < 1.5)
    {   // 1 BANQUET: five diminutive ghost attendants orbit the hero. One carries a lantern.
        float n = clamp(_Count, 3.0, 6.0);
        float2 mid = float2(_Body.z, _Body.y * 0.35);
        float live = inten * form * (1.0 - fade);
        for (int i = 0; i < 6; i++)
        {
            float fi = float(i);
            if (fi < n)
            {
                float an = t * (0.7 + 0.15 * fi) + fi / n * ZTAU + _Seed * 2.0;
                float rad = _Radius * (0.7 + 0.3 * zhash1(fi));
                float2 gp = mid + float2(cos(an) * rad, sin(an) * rad * 0.45 + 2.0 * sin(t * 2.0 + fi));
                float gh = 14.0 * live;
                float f = zk_ghost(p - floor(gp + float2(0.5, 0.5)), gh);
                if (f > -0.5 && live > 0.05)
                {
                    row = 1.0;
                    idx = f < 0.5 ? 0.0 : (f < 1.5 ? zk_soulidx(2.0, hot) : zk_soulidx(4.0, hot));
                    gl = max(gl, 0.28 * live);
                }
                if (fi < 0.5)
                {   // lantern on the first attendant: 3x4 gold lamp
                    float2 lp = floor(gp + float2(4.0, gh * 0.72) + float2(0.5, 0.5));
                    float2 q = p - lp;
                    if (abs(q.x) <= 1.5 && q.y >= 0.0 && q.y <= 4.0)
                    {
                        row = 5.0;
                        idx = (q.y > 2.0) ? 5.0 : 3.0;
                        if (abs(q.x) > 1.0 || q.y < 0.5) idx = 1.0;
                        gl = max(gl, 0.45 * live);
                    }
                }
            }
        }
    }
    else if (_Mode < 2.5)
    {   // 2 GHOST STEP: ectoplasm afterimages along the dash, trail dissolves. Not a teleport smear wash.
        float2 dest = _TargetPx.xy;
        if (length(dest) < 1.0) dest = float2(cos(_Angle * ZPI / 180.0), sin(_Angle * ZPI / 180.0)) * _Length;
        float2 dn = dest;
        float L = max(8.0, length(dn));
        float2 u = dn / L;
        float along = L * zeaseOut(pr);
        float live = inten * (1.0 - fade) * (1.0 - zeaseIn(zsat((pr - 0.75) / 0.25)));
        for (int i = 0; i < 3; i++)
        {
            float fi = float(i);
            float k = fi / 3.0;
            float posA = along - k * 22.0;
            if (posA > 4.0 && posA < L + 4.0 && live > 0.04)
            {
                float2 gp = u * posA;
                float faded = live * (1.0 - k * 0.55);
                float f = zk_ghost(p - floor(gp + float2(0.5, 0.5)), _Body.y * 0.9 * faded);
                if (f > -0.5)
                {
                    if (k < 0.2 || zdithS(0.85 * faded, p + float2(fi, t), 0.5))
                    {
                        row = 1.0;
                        idx = f < 0.5 ? 0.0 : (f < 1.5 ? zk_soulidx(2.0, hot * faded) : zk_soulidx(4.0, hot * faded));
                        gl = max(gl, 0.30 * faded);
                    }
                }
            }
        }
        // sparse trail dust only, never a ribbon
        float uu = dot(p, u);
        float vv = abs(dot(p, float2(-u.y, u.x)));
        if (row < -0.5 && uu > 0.0 && uu < along && vv < 3.0 && live > 0.04)
        {
            float nz = zN(_NoiseTex, p / 7.0 + float2(_Seed, -t * 0.4)).g;
            if (zclump(_NoiseTex, (1.0 - vv / 3.0) * 0.35 * live * nz, p, _Seed))
            { row = 1.0; idx = 2.0; }
        }
    }
    else if (_Mode < 3.5)
    {   // 3 SOUL BOLT: dark-teal body, pale-green rim, capped white core, short wisp tail.
        float2 d = _Dir.xy;
        if (length(d) < 0.01) d = float2(1.0, 0.0);
        d = d / max(0.001, length(d));
        float2 q = p;
        float uu = dot(q, d), vv = dot(q, float2(-d.y, d.x));
        float S = _Size;
        float live = inten * (1.0 - fade);
        // elongated orb
        float rx = S * 1.6, ry = S * 0.85;
        float er = (uu * uu) / (rx * rx) + (vv * vv) / (ry * ry);
        if (er <= 1.15 && live > 0.02)
        {
            row = 1.0;
            idx = zk_soulidx(2.0, hot);
            if (er > 0.85) idx = 0.0;
            else if (er < 0.22 * (0.4 + 0.6 * hot)) idx = zk_soulidx(5.0, hot);
            else if (er < 0.50) idx = zk_soulidx(4.0, hot);
            else if (uu < 0.0) idx = zk_soulidx(3.0, hot);
            gl = max(gl, (1.0 - er) * 0.55 * live);
        }
        // tail wisps
        if (row < -0.5 && uu < -S * 0.8 && uu > -S * 5.5 && abs(vv) < 3.5 && live > 0.02)
        {
            float nz = zN(_NoiseTex, p / 6.0 + float2(_Seed, -t * 0.8)).r;
            if (nz > 0.42 + abs(vv) * 0.12 && zdith((1.0 + uu / (S * 5.5)) * live, p))
            { row = 1.0; idx = zk_soulidx(2.0, hot); gl = max(gl, 0.25 * live); }
        }
    }
    else if (_Mode < 4.5)
    {   // 4 SOUL BURST: 1.6 u jagged ectoplasm burst, 8 shards, few puffs. Not a disc.
        float2 g = float2(p.x, p.y / max(_Squash, 0.25));
        float rr = length(g) / max(_Radius, 1.0);
        float live = (1.0 - fade) * (1.0 - zeaseIn(pr));
        float up = zeaseOut(zsat(pr / 0.3));
        // jagged flare (noise-broken, not a ring)
        float nz = zN(_NoiseTex, g / 9.0 + float2(_Seed, t * 0.2)).r;
        float edge = 0.45 + 0.55 * up + (nz - 0.5) * 0.45;
        if (rr < edge && rr > edge - 0.28 && live > 0.05 && nz > 0.32)
        { row = 1.0; idx = zk_soulidx(rr < edge - 0.12 ? 3.0 : 2.0, hot * live); gl = max(gl, 0.4 * live); }
        // core flash, capped
        if (rr < 0.16 * (1.0 - pr) * live)
        { row = 1.0; idx = zk_soulidx(3.0, 0.35 * hot); gl = max(gl, 0.40 * live); }
        for (int i = 0; i < 8; i++)
        {
            float fi = float(i);
            float an = fi / 8.0 * ZTAU + _Seed * 2.0;
            float2 dn = float2(cos(an), sin(an) * _Squash);
            float2 cpos = floor(dn * (_Radius * 0.55 * up) + float2(0.5, 0.5));
            float2 q = p - cpos;
            float uu = dot(q, dn / max(0.001, length(dn))), vv = length(q - (dn / max(0.001, length(dn))) * uu);
            if (abs(uu) < 4.0 && vv < 1.6 && live > 0.05 && pr < 0.85)
            { row = 1.0; idx = zk_soulidx(uu > 1.0 ? 4.0 : 2.0, hot); }
        }
        // 4 small puffs, no plumes
        for (int k = 0; k < 4; k++)
        {
            float fk = float(k);
            float an = fk / 4.0 * ZTAU + 0.4;
            float2 pp = float2(cos(an), sin(an) * 0.55) * (_Radius * 0.4 + pr * 8.0);
            if (length(p - pp) < 2.4 * (1.0 - pr) && pr > 0.15 && zdith(0.7, p + fk))
            { row = 1.0; idx = 3.0; }
        }
    }
    else if (_Mode < 5.5)
    {   // 5 SOUL PLAGUE MARK: sickly veins crawling over the body, occasional drip. Not a tint wash.
        float2 b = _Body.xy;
        float2 q = p - float2(0.0, b.y * 0.5);
        float live = inten * form * (1.0 - fade);
        float bx = b.x * 0.55 + 3.0, by = b.y * 0.55 + 2.0;
        float er = abs(q.x) / bx + abs(q.y) / by;
        if (er < 1.2 && live > 0.04)
        {
            float2 uv = q / 7.0 + float2(_Seed, t * 0.07);
            float e = zN(_NoiseTex, uv).a;
            float fl = zflow(_NoiseTex, q, 11.0, t, 6.0, _Seed);
            if (e < 0.14 + 0.08 * fl && er < 1.05)
            { row = 2.0; idx = (e < 0.06) ? 4.0 : 3.0; gl = max(gl, 0.36 * live); }
            else if (er < 0.95 && fl > 0.48 && zdith(0.55 * live, p))
            { row = 1.0; idx = 3.0; gl = max(gl, 0.22 * live); }
            // sickly shoulder patches
            if (er > 0.35 && er < 0.9 && zclump(_NoiseTex, 0.45 * live * (1.0 - er), q, _Seed))
            { row = 2.0; idx = 2.0; }
        }
        // drip under the feet
        float drip = zmod(t * 0.8 + _Seed, 1.0);
        float2 dp = float2((zhash1(_Seed) - 0.5) * 6.0, -drip * 10.0);
        if (length(p - dp) < 1.4 && drip < 0.7 && live > 0.2)
        { row = 1.0; idx = 4.0; }
        if (zsat(_Pulse) > 0.2 && er < 1.0)
        { gl = max(gl, 0.45 * _Pulse * live); }
    }
    else if (_Mode < 6.5)
    {   // 6 PLAGUE SPREAD: 5 leaping wisps / tendrils to 2.5 u. Not a disc.
        float n = clamp(_Count, 3.0, 6.0);
        float live = (1.0 - fade) * (1.0 - zeaseIn(zsat((pr - 0.7) / 0.3)));
        for (int i = 0; i < 6; i++)
        {
            float fi = float(i);
            if (fi < n)
            {
                float an = fi / n * ZTAU + (zhash1(fi + _Seed) - 0.5) * 0.5;
                float reach = _Radius * zeaseOut(zsat(pr / 0.55));
                float2 dn = float2(cos(an), sin(an) * _Squash);
                // tendril along the leap
                float2 a = float2(0.0, 4.0);
                float2 b = dn * reach * (0.75 + 0.45 * zhash1(fi + _Seed)) + float2(0.0, 14.0 * sin(pr * 2.4 + fi * 1.7));
                float d = zsegd(p, a, b);
                float along = zsat(dot(p - a, b - a) / max(1.0, dot(b - a, b - a)));
                if (d < 1.3 + 1.2 * sin(along * 11.0 + fi) && live > 0.05 && zN(_NoiseTex, p / 6.0 + float2(fi, t * 0.1)).g > 0.22)
                {
                    row = 1.0;
                    idx = zk_soulidx(along > 0.8 ? 4.0 : 2.0, hot);
                    gl = max(gl, 0.36 * live);
                }
                // wisp head
                float f = zk_ghost(p - floor(b + float2(0.5, 0.5)), 8.0 * live);
                if (f > -0.5 && pr > 0.2) { row = 1.0; idx = f < 0.5 ? 0.0 : zk_soulidx(3.0, hot); gl = max(gl, 0.4 * live); }
            }
        }
        // small pop at the dying body, white capped hard
        if (length(p) < 4.0 * (1.0 - pr) && pr < 0.28 && zdith(0.7, p))
        { row = 1.0; idx = zk_soulidx(3.0, 0.25); gl = max(gl, 0.32 * (1.0 - pr)); }
    }
    else
    {   // 7 EMERGING GHOST: rises from the pivot (gate mouth), then walks along _Dir.
        float2 d = _Dir.xy;
        if (length(d) < 0.01) d = float2(cos(_Angle * ZPI / 180.0), sin(_Angle * ZPI / 180.0));
        d = d / max(0.001, length(d));
        float rise = zeaseOut(zsat(pr / 0.28));
        float walk = max(0.0, pr - 0.28) * 36.0;
        float2 gp = d * walk + float2(0.0, 2.0);
        float gh = _Size * rise * form * (1.0 - fade) * (1.0 - zeaseIn(zsat((pr - 0.8) / 0.2)));
        float f = zk_ghost(p - floor(gp + float2(0.5, 0.5)), gh);
        if (f > -0.5 && gh > 3.0)
        {
            row = 1.0;
            idx = f < 0.5 ? 0.0 : (f < 1.5 ? zk_soulidx(2.0, hot) : zk_soulidx(4.0, hot));
            gl = max(gl, 0.3 * rise);
        }
        // birth puff in the mouth, 3 small motes
        if (pr < 0.35)
        {
            float nz = zN(_NoiseTex, p / 6.0 + float2(_Seed, t)).g;
            if (length(p) < 5.0 * (1.0 - pr) && nz > 0.4) { row = 1.0; idx = 3.0; }
        }
    }
    if (row < -0.5) return zk_none();
    if (_Layer > 0.5)
    {
        gl = min(gl, _GlowCap);
        if (gl < 0.02) return zk_none();
        return zk_glow(_RampTex, gl, p + w * 0.01, _GlowGain, 1.0);
    }
    return float4(zramp(_RampTex, row, idx), 1.0);
}
