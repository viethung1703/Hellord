// Leizu ground family (t1666). Real ground material, never a flat ring / disc / wash:
//  _Mode 0 zone   : shocked ground / curse zone. Lichtenberg scorch branches burnt into the floor with current pulses running
//                   along them, patchy soot, short crawling arcs re-struck every few steps, a few charged dust motes lifting.
//  _Mode 1 wave   : shockwave front of kicked charged dust + broken arc filaments riding the front + radial scorched streaks.
//  _Mode 2 pulse  : static-field pulse: a fast front of short outward spikes (fur of sparks), interior dust lift.
//                   _Style 2 = mirror dazzle: the front is light glints on the floor instead of sparks.
// Ground plane: g = (p.x, p.y / _Squash). Sort under sprites (boss-style warnings and zones render below bodies).
// v2 (t1666 upgrade): zone veins 2-3 px and always lit (deep blue / violet base, current pulses to cyan / white, random crackle
// flicker per vein segment), veins glow, crawling arcs 2 px white core + cyan (6 slots), charged sparks pop as 5 px plus-sparks,
// the patchy pool light is cut (no disc). Wave / pulse fronts 2 px core + 2 px fringe; the impact Lichtenberg burn is 2-3 px,
// flashes white -> cyan and leaves blue veins in the scorch; a brief broken ground light marks the impact.
float3 lz_arccol(float style, float k)       // k 0 = outer, 1 = core
{
    if (style > 0.5 && style < 1.5) return k > 0.5 ? zramp(_RampTex, 2.0, 4.0) : zramp(_RampTex, 2.0, 3.0);
    if (style > 1.5) return k > 0.5 ? zramp(_RampTex, 5.0, 4.0) : zramp(_RampTex, 5.0, 3.0);
    return k > 0.5 ? zramp(_RampTex, 0.0, 4.0) : zramp(_RampTex, 0.0, 3.0);
}
// distance to Lichtenberg branch set (g space); pulse = 0..1 current brightness at the nearest branch point
float lz_lichten(float2 g, float R, float nb, float seed, float grow, out float pulse, out float uu)
{
    float best = 100000.0; pulse = 0.0; uu = 1.0;
    float t = floor(_FxTime * _StepFps) / _StepFps;
    for (int i = 0; i < 9; i++)
    {
        float fi = float(i);
        if (fi >= nb) break;
        float h = zhash(float2(fi * 3.7 + 1.0, seed * 53.0));
        float a = (fi + 0.2 + 0.6 * h) / nb * ZTAU;
        float len = R * (0.55 + 0.4 * frac(h * 7.1)) * grow;
        if (len < 2.0) continue;
        float2 dir = float2(cos(a), sin(a));
        float2 S = dir * 2.0; float2 E = dir * len;
        float u;
        float n = max(2.0, floor(len / 6.0));
        float d = lz_jag(g, S, E, n, 3.0, seed + fi * 0.37, u);
        // one fork per branch
        float kb = 1.0 + floor(frac(h * 13.3) * max(n - 1.0, 1.0));
        float2 F0 = lz_knot(S, E, kb, n, 3.0, seed + fi * 0.37);
        float side = frac(h * 5.9) > 0.5 ? 1.0 : -1.0;
        float2 F1 = F0 + zrot(dir, side * 0.7) * len * 0.4;
        float fu;
        float df = lz_jag(g, F0, F1, 2.0, 2.0, seed + fi * 0.71, fu) + 0.35;
        float uf = (kb / n) + fu * 0.4;
        if (df < d) { d = df; u = uf; }
        if (d < best)
        {
            best = d; uu = u;
            // current pulses travel outwards along the branch (period ~0.8 s, staggered per branch)
            float ph = frac(t * 1.25 + h * 3.0);
            float pos = ph * 1.4;
            pulse = zsat(1.0 - abs(u - pos) * 2.6);
        }
    }
    return best;
}
// broken jagged filament riding a front of radius fr (polar jag, windowed so it never closes into a ring). Returns distance px.
float lz_front(float rr, float ang, float fr, float st, float seed, float K, float amp, float cover)
{
    float sa = (ang / ZTAU + 0.5) * K;
    float i0 = floor(sa); float fa = sa - i0;
    float j0 = (zhash(float2(zmod(i0, K), st + seed * 13.0)) - 0.5) * amp;
    float j1 = (zhash(float2(zmod(i0 + 1.0, K), st + seed * 13.0)) - 0.5) * amp;
    float jr = fr + lerp(j0, j1, fa);
    float win = zhash(float2(zmod(floor(sa / 2.0), K), floor(st * 0.5) + seed * 7.0));
    if (win >= cover) return 100000.0;
    float slope = abs(j1 - j0) * K / ZTAU / max(rr, 1.0);
    return abs(rr - jr) / sqrt(1.0 + slope * slope);
}
float4 ZFX_Frag(float2 p, float2 w)
{
    float2 g = float2(p.x, p.y / _Squash);
    float rr = length(g); float R = max(_Radius, 1.0); float rn = rr / R;
    float t = floor(_FxTime * _StepFps) / _StepFps;
    float st = floor(_FxTime * _StepFps);
    float ang = atan2(g.y, g.x);
    float nz = zN(_NoiseTex, p / 44.0 + float2(_Seed * 7.0, _Seed * 3.0)).r;
    float nz2 = zN(_NoiseTex, p / 23.0 + float2(_Seed * 5.0 + 0.3, 0.7)).g;
    bool glow = _Layer > 0.5;
    float gl = 0.0;
    float4 c = float4(0.0, 0.0, 0.0, 0.0);
    if (_Mode < 0.5)
    {
        // ---------- zone
        float edge = 1.0 - rn + (nz - 0.5) * 0.4;
        if (edge <= 0.0) return c;
        float fadeKeep = (nz2 * 0.7 + nz * 0.3) - _Fade * 1.05;     // dissolve by noise, no pop
        if (fadeKeep < 0.0) return c;
        float grow = zeaseOut(_Ignite);
        float pulse; float uu;
        float nbr = _Branches > 0.5 ? _Branches : clamp(floor(4.0 + R / 9.0), 4.0, 9.0);   // 0 = auto from the radius
        float d = lz_lichten(g, R, nbr, _Seed, grow, pulse, uu);
        float rs = clamp(R / 32.0, 0.6, 1.0);                      // small zones (R 16) get thinner veins so they stay veins, not a pad
        float lw = (1.0 + 0.8 * (1.0 - uu)) * rs;                   // v2: 2 px at the tips, 3-4 px near the centre (R >= 32)
        bool armed = _Ignite >= 0.999;
        float live = armed ? (1.0 - zsat(_Fade * 1.6)) : (0.35 + 0.3 * _Ignite);   // arming telegraph brightens toward the arm
        // scorch line
        bool vio = _Style > 0.5 && _Style < 1.5;
        float crk = zhash(float2(floor(uu * 9.0) + floor(ang * 1.5) * 13.0, floor(st / 2.0) + _Seed * 23.0));
        float flick = (crk < 0.14 * live) ? 0.75 : 0.0;             // crackle: random vein segments flare for 2 steps
        float k = max(pulse * live, flick);
        if (d <= lw && rn <= grow * 1.1 + 0.05)
        {
            if (k > 0.85) c = float4(lz_arccol(_Style, 1.0), 1.0);
            else if (k > 0.55) c = float4(lz_arccol(_Style, 0.0), 1.0);
            else if (k > 0.25 || armed) c = float4(vio ? zramp(_RampTex, 2.0, 3.0) : zramp(_RampTex, 0.0, k > 0.25 ? 2.0 : 1.0), 1.0);
            else c = float4(vio ? zramp(_RampTex, 2.0, 2.0) : zramp(_RampTex, 0.0, 1.0), 1.0);
            gl = max(gl, max(0.35 + 0.65 * k, (vio && !armed) ? 0.4 * _Ignite : 0.0) * (armed ? live : 0.6));
        }
        else if (d <= lw + 1.0 && rn <= grow * 1.1 + 0.05 && k > 0.55)
        {
            c = float4(vio ? zramp(_RampTex, 2.0, 3.0) : zramp(_RampTex, 0.0, 2.0), 1.0);
            gl = max(gl, k * 0.6);
        }
        if (rn <= grow * 1.1 + 0.05) gl = max(gl, pow(zsat(1.0 - d / (6.0 * rs)), 1.5) * (0.3 + 0.6 * k) * (armed ? live : 0.5));
        // burnt halo hugging the branches (dithered, 2-3 px), plus a faint curse stain for style 1. No free-floating blobs.
        if (c.a < 0.5 && rn <= grow * 1.1 + 0.05)
        {
            float soot = zN(_NoiseTex, p / 14.0 + float2(_Seed * 11.0, 0.0)).r;
            float halo = (lw + 2.0 + soot * 1.5) - d;
            if (halo > 0.0 && zbayer(w) < (0.35 + 0.4 * zsat(halo / 2.0)) * (1.0 - _Fade))
                c = float4(_Style > 0.5 && _Style < 1.5 ? zramp(_RampTex, 2.0, 1.0) : zramp(_RampTex, 3.0, 0.0), 1.0);
        }
        // crawling arcs: 4 slots, each re-struck at its own period between two points inside the zone
        float arcs = floor(6.0 * _ArcRate * live + 0.5);
        for (int j = 0; j < 6; j++)
        {
            float fj = float(j);
            if (fj >= arcs) break;
            float per = 6.0 + fj * 1.5;                                  // steps per cycle
            float cyc = floor((st + fj * 3.0) / per);
            float ph = (st + fj * 3.0) - cyc * per;
            if (ph >= 3.0) continue;                                     // lives 3 steps of the cycle
            float h1 = zhash(float2(cyc * 1.3 + fj, _Seed * 17.0)); float h2 = zhash(float2(cyc * 2.1 + fj, _Seed * 29.0 + 1.0));
            float2 a0 = float2(cos(h1 * ZTAU), sin(h1 * ZTAU)) * R * (0.2 + 0.6 * h2);
            float2 a1 = a0 + zrot(float2(1.0, 0.0), h2 * ZTAU) * (10.0 + 10.0 * h1);
            if (length(a1) > R * 0.95) a1 = a1 * (R * 0.95 / length(a1));
            float u;
            float da = lz_jag(g, a0, a1, 3.0, 2.5, frac(_Seed + cyc * 0.17 + ph * 0.31), u);
            float hot = ph < 1.0 ? 1.0 : 0.0;
            if (da <= 1.0) { c = float4(lz_arccol(_Style, hot), 1.0); gl = max(gl, 1.0); }
            else if (da <= 2.0) { c = float4(hot > 0.5 ? lz_arccol(_Style, 0.0) : (vio ? zramp(_RampTex, 2.0, 3.0) : zramp(_RampTex, 0.0, 2.0)), 1.0); gl = max(gl, 0.7); }
            else gl = max(gl, pow(zsat(1.0 - da / 8.0), 1.5) * 0.7);
        }
        // charged dust: sparse 2x2 motes lifting off the floor and dropping back
        float cell = 9.0;
        float2 ci = floor(p / cell);
        float hm = zhash(ci + float2(_Seed * 31.0, 4.0));
        if (hm < 0.22 * _Dust * live && c.a < 0.5)
        {
            float cyc = 1.6 + hm * 2.0;
            float ph = frac(t / cyc + hm * 5.0);
            float lift = floor(sin(ph * ZPI) * 4.0);
            float2 mp = ci * cell + float2(2.0 + floor(frac(hm * 7.0) * 5.0), 2.0 + lift);
            float2 q = p - mp;
            bool pop = ph > 0.35 && ph < 0.55;                           // charged spark pops: 5 px plus, white heart
            bool mote = q.x >= 0.0 && q.x < 2.0 && q.y >= 0.0 && q.y < 2.0;
            float2 qc = q - float2(0.5, 0.5);
            bool plus = (abs(qc.y) < 0.6 && abs(qc.x) <= 2.6) || (abs(qc.x) < 0.6 && abs(qc.y) <= 2.6);
            if ((pop ? plus : mote) && length(float2(mp.x, mp.y / _Squash)) < R * 0.85)
            {
                c = float4(pop ? (length(qc) < 1.0 ? lz_arccol(_Style, 1.0) : lz_arccol(_Style, 0.0)) : zramp(_RampTex, 1.0, 4.0), 1.0);
                gl = max(gl, pop ? 0.8 : 0.3);
            }
        }
        if (glow)
        {
            float pool = zsat(1.0 - rn) * 0.12 * (0.6 + 0.4 * nz) * live;   // v2: cut to a trace (the veins carry the light now)
            return zglow(_RampTex, max(gl * live, pool) * _Light, w, _GlowGain);
        }
        return c;
    }
    if (_Mode < 1.5)
    {
        // ---------- shockwave front
        float pr = zsat(_Progress);
        float fr = R * zeaseOut(pow(pr, 0.8));
        float bw = 3.0 + 4.0 * (1.0 - pr);
        float dr = rr - fr;
        float decay = 1.0 - zsat((pr - 0.55) / 0.45);
        // scorched radial streaks left behind the front
        float bins = 22.0;
        float bi = floor((ang / ZTAU + 0.5) * bins);
        float hb = zhash(float2(bi, _Seed * 41.0));
        float ac = (bi + 0.5) / bins * ZTAU - ZPI + (hb - 0.5) * 0.12;
        float perp = abs(rr * sin(ang - ac));
        float r0 = R * (0.25 + 0.2 * hb); float r1 = min(fr - 2.0, R * (0.55 + 0.35 * frac(hb * 7.0)));
        if (hb < 0.38 && perp < 0.7 + 0.8 * (1.0 - rr / R) && rr > r0 && rr < r1 && nz2 > _Fade * 1.1 - 0.1)
            c = float4(zramp(_RampTex, 3.0, rr < r0 + 4.0 ? 0.0 : 1.0), 1.0);
        // kicked charged dust: separate puffs riding the front (lit top, dark underside), never a continuous band
        for (int k = -1; k < 2; k++)
        {
            float PB = 15.0;
            float pb = floor((ang / ZTAU + 0.5) * PB) + float(k);
            float hp = zhash(float2(zmod(pb, PB), _Seed * 61.0 + 3.0));
            if (hp > 0.68) continue;
            float pa = (pb + 0.5 + (hp - 0.34) * 0.8) / PB * ZTAU - ZPI;
            float pr0 = fr - 1.0 - 3.0 * frac(hp * 7.7) + 2.0 * pr;
            float2 pc = float2(cos(pa), sin(pa)) * pr0;
            float2 sp = float2(pc.x, pc.y * _Squash);                    // puff centre in screen px
            float sz = (2.0 + 2.0 * frac(hp * 3.3)) * (0.7 + 0.4 * zsat(pr * 3.0)) * (1.0 - 0.5 * zsat((pr - 0.6) / 0.4));
            float2 q = p - sp - float2(0.0, floor(pr * 4.0 * frac(hp * 5.1)));   // puffs lift a little
            float pn = zN(_NoiseTex, q / 9.0 + float2(hp * 13.0, t * 0.3)).r;
            float pd = length(q * float2(0.8, 1.15)) / max(sz, 0.5) + (pn - 0.5) * 0.7;
            if (pd < 1.0 && decay > 0.05 && c.a < 0.5)
            {
                float top = q.y / max(sz, 0.5);
                float idx = top > 0.35 ? 4.0 : (top > -0.3 ? 3.0 : 2.0);
                if (pd > 1.0 - 0.6 * (1.0 - decay)) idx = 2.0;
                if (pd > 0.75 && zbayer(w) > decay + 0.2) continue;
                c = float4(zramp(_RampTex, 1.0, idx), 1.0);
                gl = max(gl, 0.25 * decay);
            }
        }
        // broken arc filaments riding the front (polar jag, windowed so it never closes into a ring)
        float dd = lz_front(rr, ang, fr + 1.0, st, _Seed, 26.0, 5.0, 0.3 * decay + 0.05);
        if (pr < 0.85)
        {
            float hot = pr < 0.35 ? 1.0 : 0.0;
            if (dd <= 1.0) { c = float4(lz_arccol(_Style, hot), 1.0); gl = max(gl, decay); }
            else if (dd <= 2.0 && pr < 0.65) { c = float4(lz_arccol(_Style, 0.0), 1.0); gl = max(gl, 0.7 * decay); }
            else gl = max(gl, pow(zsat(1.0 - dd / 8.0), 1.5) * 0.65 * decay);
        }
        // impact centre: a short-lived Lichtenberg burn that flashes then stays as dark scorch
        if (rr < R * 0.45 && c.a < 0.5)
        {
            float pu; float u2;
            float dc = lz_lichten(g, R * 0.4, 5.0, _Seed + 0.5, zsat(pr / 0.15), pu, u2);
            if (dc <= 1.0 + 0.8 * (1.0 - u2))
            {
                if (pr < 0.15) { c = float4(lz_arccol(_Style, 1.0), 1.0); gl = max(gl, 1.0); }
                else if (pr < 0.35) { c = float4(lz_arccol(_Style, 0.0), 1.0); gl = max(gl, 0.7); }
                else if (nz2 > _Fade * 1.1 - 0.1) { c = float4(dc <= 0.6 && decay > 0.3 ? zramp(_RampTex, 0.0, 1.0) : zramp(_RampTex, 3.0, 1.0), 1.0); gl = max(gl, 0.3 * decay); }
            }
            else if (pr < 0.3) gl = max(gl, pow(zsat(1.0 - dc / 8.0), 1.5) * (1.0 - pr / 0.3) * 0.8);
        }
        if (pr < 0.3) gl = max(gl, lz_groundlight(_NoiseTex, p, R * 0.45, _Seed) * (1.0 - pr / 0.3) * 0.85);
        if (glow) return zglow(_RampTex, gl * _Light, w, _GlowGain);
        return c;
    }
    // ---------- pulse
    float pr2 = zsat(_Progress);
    float fr2 = R * zeaseOut(pr2);
    float decay2 = 1.0 - zsat((pr2 - 0.5) / 0.5);
    if (_Style > 1.5)
    {   // dazzle: no arcs. Light glints catch the floor as the flash front passes (small 4-point sparkles, 5 x 3 px)
        float GB = 26.0;
        for (int k = -1; k < 2; k++)
        {
            float gb = floor((ang / ZTAU + 0.5) * GB) + float(k);
            float hg = zhash(float2(zmod(gb, GB), _Seed * 29.0 + 5.0));
            if (hg > 0.7) continue;
            float ga = (zmod(gb, GB) + 0.5 + (hg - 0.27)) / GB * ZTAU - ZPI;
            float gr = fr2 - 2.0 - 10.0 * frac(hg * 7.0);
            float2 gp = floor(float2(cos(ga) * gr, sin(ga) * gr * _Squash) + float2(0.5, 0.5));
            float2 q = p - gp;
            float life = zsat((fr2 - gr) / 6.0) * decay2;
            float arm = life > 0.6 ? 3.0 : (life > 0.3 ? 2.0 : 1.0);
            bool on = (abs(q.y) < 0.5 && abs(q.x) <= arm) || (abs(q.x) < 0.5 && abs(q.y) <= arm - 1.0);
            if (on && life > 0.05) { c = float4(zramp(_RampTex, 5.0, length(q) < 0.5 ? 4.0 : 3.0), 1.0); gl = max(gl, 0.6 * life); }
        }
        if (glow) return zglow(_RampTex, gl * _Light, w, _GlowGain);
        return c;
    }
    // crackling front: two staggered jagged filaments (the field edge) + a few long jagged spikes thrown outward
    float dp = min(lz_front(rr, ang, fr2, st, _Seed, 34.0, 4.0, 0.62 * decay2 + 0.1),
                   lz_front(rr, ang, fr2 - 4.0, st + 7.0, _Seed + 0.4, 22.0, 5.0, 0.35 * decay2));
    if (pr2 < 0.92)
    {
        float hot2 = pr2 < 0.45 ? 1.0 : 0.0;
        if (_Style > 1.5) hot2 = 1.0;
        if (dp <= 1.0) { c = float4(lz_arccol(_Style, hot2), 1.0); gl = max(gl, decay2); }
        else if (dp <= 2.0 && pr2 < 0.55) { c = float4(lz_arccol(_Style, 0.0), 1.0); gl = max(gl, 0.7 * decay2); }
        else gl = max(gl, pow(zsat(1.0 - dp / 7.0), 1.5) * 0.6 * decay2);
    }
    float bins2 = 14.0;
    float bi2 = floor((ang / ZTAU + 0.5) * bins2);
    float hb2 = zhash(float2(bi2, _Seed * 23.0 + floor(st / 2.0)));
    if (hb2 < 0.45 && pr2 < 0.6 && c.a < 0.5)
    {
        float ac2 = (bi2 + 0.5 + (hb2 - 0.22)) / bins2 * ZTAU - ZPI;
        float2 dir2 = float2(cos(ac2), sin(ac2));
        float u3;
        float ds = lz_jag(g, dir2 * (fr2 - 2.0), dir2 * (fr2 + 6.0 + 8.0 * hb2), 3.0, 2.0, hb2 + st * 0.1, u3);
        if (ds <= 0.9) { c = float4(lz_arccol(_Style, u3 < 0.45 ? 1.0 : 0.0), 1.0); gl = max(gl, 0.85); }
        else gl = max(gl, pow(zsat(1.0 - ds / 6.0), 1.5) * 0.5);
    }
    // interior: charged dust lifted by the pulse (sparse 2x2) + a few scorch flecks under it
    float cell2 = 8.0; float2 ci2 = floor(p / cell2);
    float hm2 = zhash(ci2 + float2(_Seed * 19.0, 2.0));
    if (c.a < 0.5 && hm2 < 0.18 * _Dust && rr < fr2 - 2.0)
    {
        float lift2 = floor(zsat((fr2 - rr) / 10.0) * 3.0 * decay2);
        float2 mp2 = ci2 * cell2 + float2(2.0 + floor(frac(hm2 * 7.0) * 4.0), 2.0 + lift2);
        float2 q2 = p - mp2;
        if (q2.x >= 0.0 && q2.x < 2.0 && q2.y >= 0.0 && q2.y < 2.0 && decay2 > 0.05)
            c = float4(_Style > 1.5 ? zramp(_RampTex, 5.0, 3.0) : zramp(_RampTex, 1.0, 4.0), 1.0);
    }
    if (glow) return zglow(_RampTex, gl * _Light, w, _GlowGain);
    return c;
}
