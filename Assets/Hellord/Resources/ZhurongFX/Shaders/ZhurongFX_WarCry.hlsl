// War Cry (t1625, replaces Molten Shell as a skill; Molten Shell files kept).
// _Mode 0 ROAR WAVE (pivot = hero feet, Y-sorted under actors): embers are sucked in during the inhale, then an outward
//        fiery pressure front made only of material: angularly broken scorch-licked flame tongues, radial ember dashes,
//        kicked dust clumps behind the front and a fading char trail. No drawn ring, no fill.
// _Mode 1 EMPOWERED (hero frame aura, behind the hero): tongues off the silhouette + a flickering warm edge whose height
//        steps with _Charges 0..5, five charge motes over the head (lit = charges left), _Pulse = consume / inhale flash.
// _Mode 2 MARKED (enemy, pivot = feet): a few ember motes flicker and rise on the body for the 4 s kill window.
float zw_a(float2 sp)
{
    if (sp.x < 0.0 || sp.y < 0.0 || sp.x >= _SpritePx.x || sp.y >= _SpritePx.y) return 0.0;
    float2 uv = _SpriteRect.xy + (floor(sp) + float2(0.5, 0.5)) / _SpritePx.xy * _SpriteRect.zw;
    return (ZTEX(_MainTex, uv).a > 0.5) ? 1.0 : 0.0;
}
float zw_ring(float2 g, float rf, float bw, float q, out float brk, out float dr)
{
    float r = length(float2(g.x, g.y / _Squash));
    float a = atan2(g.y / _Squash, g.x);
    brk = zN(_NoiseTex, float2(cos(a), sin(a)) * 0.9 + float2(_Seed, _Seed * 0.3)).r
        + (zN(_NoiseTex, float2(a * 1.3, r * 0.05) + float2(0.5, _Seed)).g - 0.5) * 0.5;
    dr = rf - r;
    return (dr > -1.0 && dr < bw) ? (1.0 - zsat(dr / bw)) : 0.0;
}
float4 ZFX_Frag(float2 p, float2 w)
{
    float t = floor(_FxTime * _StepFps) / _StepFps;
    float4 col = float4(0.0, 0.0, 0.0, 0.0);
    float glowF = 0.0;
    if (_Mode < 0.5)
    {
        float pr = zsat(_Progress);
        float wf = _Windup;
        float2 g = float2(p.x, p.y / _Squash);
        float r = length(g);
        float a = atan2(g.y, g.x);
        if (pr < wf)
        {
            // inhale: ember specks converge on the hero, heat gathers under the feet
            float u = zeaseIn(pr / wf);
            float cell = floor((a / ZTAU + 0.5) * 14.0);
            float h = zhash1(cell + _Seed * 17.0);
            float rr = (_Radius * 0.55) * (1.0 - u) * (0.6 + 0.5 * h) + 3.0;
            float ac = (cell + 0.5) / 14.0 * ZTAU - ZPI + (h - 0.5) * 0.3;
            float2 sp0 = float2(cos(ac), sin(ac) * _Squash) * rr + float2(0.0, 2.0 + 6.0 * h);
            if (h > 0.25 && length(p - sp0) < 1.0) col = zfire(_RampTex, 0.65 + 0.3 * u, w, 0.0);
            float2 tl = sp0 + float2(cos(ac), sin(ac) * _Squash) * 2.0;   // short streak behind each speck
            if (h > 0.25 && zsegd(p, sp0, tl) < 0.6 && col.a < 0.5) col = zfire(_RampTex, 0.45, w, 0.0);
            float pool = (1.0 - r / (6.0 + 4.0 * u)) + (zN(_NoiseTex, p / 6.0 + float2(t, _Seed)).r - 0.5) * 0.6;
            if (pool > 0.3 && col.a < 0.5) col = float4(zramp(_RampTex, 0.0, 1.0), 1.0);
            glowF = zsat(pool) * u * 0.7;
        }
        else
        {
            float q = (pr - wf) / (1.0 - wf);
            float rf = _Radius * zeaseOut(q * 1.15) + 2.0;
            float bw = 4.0 + 6.0 * q;
            float keep = 0.3 + 0.3 * q;
            float fl = zflow(_NoiseTex, p + float2(_Seed * 40.0, 0.0), 5.0, t, 20.0, _Seed);
            float brk = 0.0; float dr = 0.0;
            // dust kicked behind the front (low, clumpy, cools from warm to grey)
            float dz = 0.0;
            {
                zw_ring(p, rf, bw, q, brk, dr);
                float dd = (dr - bw * 0.3) / 12.0;
                if (dd > 0.0 && dd < 1.0 && brk > keep + 0.08)
                {
                    float dens = (1.0 - dd) * (1.0 - q * 0.8) * 1.0 * zsat((brk - keep) * 4.0);
                    if (zclump(_NoiseTex, dens, p + float2(q * 9.0, 0.0), _Seed)) { col = float4(zramp(_RampTex, 1.0, (dd < 0.4) ? 3.0 : 2.0), 1.0); dz = 1.0; }
                }
                // char trail inside (fades with q)
                if (dr > bw && dr < bw + 18.0 && brk > keep + 0.05 && dz < 0.5)
                {
                    float cd = (1.0 - (dr - bw) / 18.0) * (1.0 - q) * 0.9;
                    if (zclump(_NoiseTex, cd, p * 1.3, _Seed + 0.4)) col = float4(zramp(_RampTex, 0.0, (cd > 0.5) ? 1.0 : 0.0), 1.0);
                }
            }
            // scorch-licked tongues rising from the front (tap down to find the ground point)
            float best = 0.0;
            for (int k = 0; k < 11; k++)
            {
                float fk = float(k);
                float wob = sin(t * 11.0 + p.x * 0.5 + fk * 0.6) * 0.5 * fk / 7.0;
                float2 gp = p - float2(wob, fk);
                float bk = 0.0; float dk = 0.0;
                float on = zw_ring(gp, rf, bw, q, bk, dk);
                if (on <= 0.0 || bk < keep) continue;
                // discrete tongues: one per angular cell, some cells empty
                float ga = atan2(gp.y / _Squash, gp.x);
                float gr = length(float2(gp.x, gp.y / _Squash));
                float cc = floor((ga / ZTAU + 0.5) * 34.0);
                float hc = zhash1(cc * 2.3 + _Seed * 11.0);
                if (hc < 0.3) continue;
                float lat = abs(frac((ga / ZTAU + 0.5) * 34.0) - 0.5) * (ZTAU / 34.0) * gr;
                float hk = (3.0 + 9.0 * zsat((bk - keep) / 0.4)) * (0.6 + 0.5 * hc) * (1.0 - q * 0.55) * (0.55 + 0.45 * on);
                if (fk > hk) continue;
                float hwk = 1.7 * pow(max(1.0 - fk / hk, 0.0), 0.7) + 0.4;
                if (lat > hwk) continue;
                best = max(best, on * (1.0 - fk / max(hk, 0.5)) * (1.0 - lat / (hwk + 0.5)) * (0.8 + 0.5 * (1.0 - q)));
            }
            if (best > 0.0)
            {
                float heat = min(best * 1.15 + (fl - 0.5) * 0.6, 0.84);
                float4 fc = zfire(_RampTex, heat, w, 0.22);
                if (fc.a > 0.5) col = fc;
                else if (heat > 0.1) col = float4(zramp(_RampTex, 0.0, 0.0), 1.0);   // dark lick lip (red floors)
                glowF = max(glowF, zsat(heat) * 0.9);
            }
            // radial ember dashes sweeping just ahead of / on the front
            float cell = floor((a / ZTAU + 0.5) * 40.0);
            float h = zhash1(cell * 1.7 + _Seed * 29.0);
            float ac = (cell + 0.5) / 40.0 * ZTAU - ZPI;
            float2 dir = float2(cos(ac), sin(ac));
            float re = rf + (h - 0.3) * 8.0;
            float2 e0 = float2(dir.x, dir.y * _Squash) * re;
            float2 e1 = float2(dir.x, dir.y * _Squash) * (re - 2.5 - 2.0 * h);
            if (h > 0.55 && q < 0.85 && zsegd(p, e0, e1) < 0.6)
            {
                col = zfire(_RampTex, 0.6 + 0.35 * h * (1.0 - q), w, 0.0);
                glowF = max(glowF, 0.6);
            }
        }
    }
    else if (_Mode < 1.5)
    {
        // ---- empowered hero aura
        float2 sp = p + _SpritePx.zw;
        float hh = _SpritePx.y - _SpritePx.w;
        float inside = zw_a(sp);
        float ch = clamp(_Charges, 0.0, 5.0) / 5.0;
        float pw = _Form * (0.25 + 0.75 * ch) + _Pulse * 0.6;
        float fl = zflow(_NoiseTex, p + float2(_Seed * 20.0, 0.0), 4.0, t, 16.0, _Seed);
        if (inside < 0.5 && pw > 0.02)
        {
            float heat = 0.0;
            for (int k = 1; k < 9; k++)
            {
                float fk = float(k);
                if (zw_a(sp - float2(0.0, fk)) > 0.5)
                {
                    float Hk = (1.5 + 6.0 * pw) * (0.5 + 0.8 * zN(_NoiseTex, float2(p.x / 4.0 + _Seed, t * 0.6)).g);
                    heat = max(heat, (1.0 - fk / Hk) * (0.55 + 0.7 * fl));
                    break;
                }
            }
            float side = max(max(zw_a(sp + float2(1.0, 0.0)), zw_a(sp - float2(1.0, 0.0))), zw_a(sp - float2(0.0, 1.0)));
            float side2 = max(zw_a(sp + float2(2.0, 0.0)), zw_a(sp - float2(2.0, 0.0)));
            float edge = (side > 0.5) ? 0.75 : ((side2 > 0.5) ? 0.45 : 0.0);
            edge *= pw * (0.6 + 0.6 * fl);
            float hv = max(heat * pw * 1.2, edge);
            float4 fc = zfire(_RampTex, hv + _Pulse * 0.3, w, 0.3);
            if (fc.a > 0.5) col = fc;
            glowF = zsat(hv) * 0.8;
        }
        // five charge motes over the head: lit = charges left, spent = dark ember
        float my = _HeadY + floor(sin(t * 3.0) + 0.5);
        for (int j = 0; j < 5; j++)
        {
            float fj = float(j);
            float2 mp = float2((fj - 2.0) * 4.0, my - abs(fj - 2.0) * 1.0);
            float2 d = p - mp;
            bool lit = fj < _Charges - 0.5;
            if (_Form < 0.05) continue;
            if (lit)
            {
                float fh = 2.5 + 1.5 * zN(_NoiseTex, float2(t * 1.3 + fj * 0.37, _Seed)).g + _Pulse;
                float v = (d.y > -1.0 && d.y < fh) ? (1.0 - abs(d.x) / (1.3 * (1.0 - (d.y + 1.0) / (fh + 1.5)) + 0.2)) : -1.0;
                if (v > 0.0) { col = zfire(_RampTex, 0.42 + v * 0.3 + (1.0 - (d.y + 1.0) / (fh + 1.0)) * 0.18 + _Pulse * 0.2, w, 0.0); glowF = max(glowF, 0.8); }
                if (v > -0.4 && v <= 0.0 && d.y < fh - 0.5 && col.a < 0.5) col = float4(zramp(_RampTex, 0.0, 0.0), 1.0);
            }
            else if (abs(d.x) < 1.0 && abs(d.y) < 1.0) col = float4(zramp(_RampTex, 0.0, 1.0), 1.0);
        }
    }
    else
    {
        // ---- marked enemy: ember motes flicker and rise on the body
        for (int j = 0; j < 4; j++)
        {
            float fj = float(j);
            float per = 0.55 + 0.2 * zhash1(fj + _Seed * 7.0);
            float ph = t / per + zhash1(fj * 3.3 + _Seed);
            float cyc = floor(ph);
            float u = frac(ph);
            float hx = zhash1(cyc * 1.3 + fj * 5.1 + _Seed * 3.0);
            float hy = zhash1(cyc * 2.7 + fj * 1.9 + _Seed * 5.0);
            float2 mp = float2((hx - 0.5) * 12.0, 5.0 + hy * 16.0 + u * 5.0);
            float on = (u < 0.7) ? 1.0 : 0.0;
            float2 d = p - mp;
            if (on > 0.5 && abs(d.x) < 0.75 && d.y > -0.75 && d.y < 1.25 + (1.0 - u))
            {
                col = zfire(_RampTex, (0.85 - u * 0.4) * _Intensity + 0.15, w, 0.0);
                glowF = max(glowF, 0.5 * _Intensity);
            }
            if (on > 0.5 && u > 0.35 && abs(d.x) < 0.75 && d.y > 2.0 && d.y < 3.0 && col.a < 0.5) col = float4(zramp(_RampTex, 1.0, 1.0), 1.0);
        }
    }
    if (_Layer > 0.5) return zglow(_RampTex, glowF, w, _GlowGain);
    return col;
}
