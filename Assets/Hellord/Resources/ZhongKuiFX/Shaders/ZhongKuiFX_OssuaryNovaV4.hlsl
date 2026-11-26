// Ossuary Nova v4 (Zhong Kui kit v4 ultimate, attack): "a ring of bone spears bursts out of you to 8 u". Bone and earth only:
// no ghosts, no spectral trails, no flat rings / discs / washes. Same portable HLSL subset as Core (compiled as GLSL by the MOCKUP).
// The code hits every enemy inside the radius ON THE PRESS (HellordZhongKui1713.NkUltimate1713), so the look lands in 3 frames:
//  f0      a crown of bone spikes punches out of the ground at the hero's feet (capped flash on the glow twin), the spears leave
//  f1..f2  ~20 bone spears fly out at chest height with short cool air streaks, their shadows racing on the ground
//  f3      each spear dives into the ground just inside the hit radius: dirt kick, crater, and a cluster of 3 bone spikes
//          erupts there, so the hit radius reads as a broken ring of spike clusters (PoE 2 Bone Cage), never a drawn circle
//  f4..f9  hold; the crown at the feet shatters into a few chips and sinks (f5..f8)
//  0.6 s.. the ring spikes crack: tips snap off (every other cluster throws one chip), the rest sinks into the ground with a
//          dust puff; craters crumble away by noise. Gone by ~1.12 s (code life 1.15 s).
//  _Mode 0 GROUND  (one quad at the hero, ground layer): cracked plate at the feet, spear shadows, landing craters + cracks.
//  _Mode 1 SPEAR   (one quad per spear, ysort at its landing point): the flying spear + streak, dive, spike cluster, kick, chips.
//  _Mode 2 / 3 BURST (ysort, behind / in front of the hero): the crown of spikes at the feet; the flash rides on mode 3's glow.
// Spear i: angle = TAU (i + seed + jitter) / N, lands at R - 3 .. R - 12 px. Every quad of one cast shares _Seed / _Count / _Radius.

float zon_p15(float x) { float y = zsat(x); return y * sqrt(y); }        // x^1.5 without pow (no negative-base warning)
float zon_t(float t) { return floor(t * _StepFps + 0.0001) / _StepFps; }
float4 zon_bone(float f) { return float4(zramp(_RampTex, 0.0, zk_bidx(f, _Hot)), 1.0); }
// v4b: standing spikes use a paler map so they read against the dark floor: ink 181425, shadow c28569, mid e8b796, lit ead4aa,
// ridge ffffff (ead4aa when _Hot is low)
float4 zon_spikec(float f)
{
    float i = f < 0.5 ? 0.0 : (f < 1.5 ? 3.0 : (f < 2.5 ? 4.0 : (f < 3.5 ? 5.0 : 6.0)));
    if (i > 5.5 && _Hot < 0.75) i = 5.0;
    return float4(zramp(_RampTex, 0.0, i), 1.0);
}
float4 zon_earth(float i) { return float4(zramp(_RampTex, 3.0, i), 1.0); }
float4 zon_streak(float i) { return float4(zramp(_RampTex, 4.0, i), 1.0); }
// bone piece from the atlas (chips). q = pixel centre - piece centre. -> level 0 / .25 ink / .5 / .75 / 1
float zon_piece(float2 q, float kind, float rot)
{
    float res = 0.0;
    if (abs(q.x) < 8.0 && abs(q.y) < 8.0)
    {
        float c = floor(q.x + 8.0), r = floor(8.0 - q.y);
        float2 uv = float2((rot * 16.0 + c + 0.5) / 128.0, 1.0 - (kind * 16.0 + r + 0.5) / 144.0);
        res = ZTEX(_BoneTex, uv).r;
    }
    return res;
}
float zon_lv2f(float lv) { return lv < 0.375 ? 0.0 : (lv < 0.625 ? 1.0 : (lv < 0.875 ? 2.0 : 3.0)); }
// pixel-art dirt ball lit from the upper left; age shrinks it and bites holes (noise dissolve)
float4 zon_ball(float2 p, float2 q, float r, float age, float seed)
{
    float4 c = zk_none();
    float rr = r * (1.0 - 0.45 * age);
    float d = length(q);
    float bite = zN(_NoiseTex, p / 3.0 + float2(seed * 5.3, seed * 2.1)).r;
    if (rr >= 1.2 && d < rr && bite > age * 1.1 - 0.15)
    {
        float2 n = q / max(rr, 1.0);
        float l = dot(n, float2(-0.6, 0.8)) + zbayer(p) * 0.3;
        float i = l > 0.3 ? 4.0 : (l > -0.35 ? 3.0 : 2.0);
        if (d > rr - 1.0 && l < -0.2) i = 1.0;
        if (age > 0.55 && i > 2.5) i = i - 1.0;
        c = float4(zramp(_RampTex, 1.0, i), 1.0);
    }
    return c;
}
// jagged crack: d = perpendicular px, s = px along. 0 none, 1 ink, 2 lit lip
float zon_crackpx(float d, float s, float seed)
{
    float j = (zN(_NoiseTex, float2(s / 9.0, seed)).r - 0.5) * 3.0 + (zN(_NoiseTex, float2(s / 3.0, seed + 0.5)).g - 0.5) * 1.0;
    float e = d - j;
    float res = 0.0;
    if (abs(e) < 0.5) res = 1.0;
    else if (e > 0.0 && e < 1.5) res = 2.0;
    return res;
}
// bone SPIKE standing out of the ground, seen from the 3/4 view. q = p - base (y up), h height px, w half width at the base,
// lean = tip x offset px. -> facet -1 none, 0 ink, 1 shadow, 2 mid, 3 lit, 4 ridge highlight. Ridged: a dark notch every 4 px
// on the shadow side, a knuckle bulge at 30 %.
float zon_spike(float2 q, float h, float w, float lean, float seed)
{
    float res = -1.0;
    if (h >= 2.0 && q.y >= -1.0 && q.y <= h + 0.5)
    {
        float v = zsat(q.y / h);
        float cx = lean * v * v;
        float hw = w * (1.0 - v) + 0.3;
        if (v > 0.22 && v < 0.38) hw = hw + 0.6;                      // knuckle
        float x = q.x - cx;
        float rim = x > 0.0 ? 2.0 : 1.0;                               // v4b: 2 px ink on the shadow side so it pops off the floor
        if (abs(x) <= hw + rim)
        {
            if (abs(x) > hw || q.y > h - 0.5) res = 0.0;
            else
            {
                float ridge = -hw * 0.3;
                res = x < ridge ? 3.0 : 1.0;
                if (x >= ridge && x < ridge + max(1.0, hw * 0.5)) res = 2.0;
                if (abs(x - ridge) < 0.6 && v > 0.12 && v < 0.85) res = 4.0;
                float nt = zmod(q.y + seed * 4.0, 4.0);
                if (res < 1.5 && nt < 1.0 && v > 0.1 && v < 0.8) res = 0.0;       // ridge notches on the shadow side
            }
        }
    }
    return res;
}
// flying bone SPEAR in its own frame: a = px from the tip back toward the tail (0..L), b = px across (+ = left of the flight).
// litSide = +1 when the +b side faces the light. -> facet like zon_spike.
float zon_spear(float a, float b, float L, float litSide, float seed)
{
    float res = -1.0;
    if (a >= -0.5 && a <= L)
    {
        float hw = 1.25;
        if (a < 0.3 * L) hw = 0.2 + 1.05 * a / (0.3 * L);
        if (a > 0.62 * L && a < 0.8 * L) hw = 1.9;                     // knuckle
        float jag = zhash(float2(floor(b + 3.0), seed * 31.0)) * 2.0;
        float ab = abs(b);
        if (a <= L - jag && ab <= hw + 1.0)
        {
            if (ab > hw || a < 0.6) res = 0.0;
            else
            {
                float lit = b * litSide > 0.0 ? 1.0 : 0.0;
                res = lit > 0.5 ? 3.0 : 2.0;                                       // ivory: lit e8b796, shadow c28569
                if (lit > 0.5 && ab >= 0.5 && a > 0.15 * L && a < 0.75 * L) res = 4.0;
                float nt = zmod(a - 0.3 * L, 3.5);
                if (lit < 0.5 && a > 0.3 * L && nt < 1.0) res = 1.0;               // vertebra grooves (733e39)
            }
        }
    }
    return res;
}
// ---- spear i parameters (shared by every mode) ----
float zon_h(float i, float k) { return zhash1(i * 7.31 + _Seed * 13.1 + k * 3.7); }
float2 zon_dir(float i)
{
    float a = ZTAU * (i + _Seed + (zon_h(i, 0.0) - 0.5) * 0.36) / max(_Count, 1.0);
    return float2(cos(a), sin(a));
}
float zon_land(float i) { return _Radius - 3.0 - 9.0 * zon_h(i, 1.0); }          // lands just inside the hit radius
float zon_t0(float i) { return zon_h(i, 2.0) < 0.35 ? 1.0 / 15.0 : 0.0; }        // a third leave one frame late
ZCONST float ZON_TF = 0.2;                                                        // flight: 3 frames to the radius
float zon_tb(float i) { return 0.6 + 0.2 * zon_h(i, 3.0); }                        // ring spikes start to break

// ---------------- 0 GROUND ----------------
float4 zon_ground(float2 p, float T, float glow)
{
    float4 col = zk_none(); float g = 0.0;
    // cracked plate at the feet (hit on the press), crumbles away from 0.55 s
    float dd = length(float2(p.x, p.y * 1.7));
    float rr = 6.0 + 12.0 * zeaseOut(zsat((T + 0.067) / 0.13));
    float nz = zN(_NoiseTex, p / 3.0 + float2(_Seed * 4.0, 0.0)).r;
    if (dd < rr && nz > zsat((T - 0.55) / 0.45))
    {
        float4 cz = zN(_NoiseTex, p / 5.0 + float2(_Seed, _Seed * 2.0));
        if (cz.a < 0.12) col = zon_earth(0.0);
        else if (cz.a < 0.2 && p.y > -1.0) col = zon_earth(4.0);
        else if (dd > rr - 1.5) col = zon_earth(2.0);
        if (T < 0.13 && cz.a < 0.12) g = 0.6;
    }
    // the three spears nearest this pixel's bearing
    float N = max(_Count, 1.0);
    float ia = atan2(p.y, p.x) / ZTAU * N - _Seed;
    float i0 = floor(ia + 0.5);
    for (int k = -1; k <= 1; k++)
    {
        float i = zmod(i0 + float(k), N);
        float2 dir = zon_dir(i); float2 nrm = float2(-dir.y, dir.x);
        float R = zon_land(i), t0 = zon_t0(i);
        float L = 18.0 + 6.0 * zon_h(i, 8.0);
        float u = (T - t0) / ZON_TF;
        float s = dot(p, dir), d = dot(p, nrm);
        // v4b: no flight shadow (it read as dashes at game zoom)
        // landing crater + two short cracks running on outward
        float tl = t0 + ZON_TF;
        float alive = T >= tl - 0.001 ? 1.0 : 0.0;
        float gone = zsat((T - 0.8 - 0.1 * zon_h(i, 4.0)) / 0.18);
        float2 q = p - dir * R;
        float qs = dot(q, dir), qd = dot(q, nrm);
        if (alive > 0.5 && zN(_NoiseTex, p / 2.5 + float2(i, 0.0)).g > gone)
        {
            float e = length(float2(q.x / 9.0, q.y / 3.6));
            float es = length(float2((q.x - 7.0) / 7.0, (q.y + 1.5) / 2.4));       // cast shadow of the spikes, light from the upper left
            if (e < 0.7 || es < 0.8) col = zon_earth(0.0);
            else if ((e < 1.0 || es < 1.0) && zbayer(p) > -0.25) col = zon_earth(0.0);
            else if (e < 1.18 && q.y > 0.5) col = zon_earth(4.0);
            for (int c = 0; c < 2; c++)
            {
                float sg2 = c == 0 ? -1.0 : 1.0;
                float2 cd = zrot(dir, sg2 * (0.32 + 0.3 * zon_h(i, 5.0 + float(c))));
                float2 cn = float2(-cd.y, cd.x);
                float cs = dot(q, cd), cl = 6.0 + 5.0 * zon_h(i, 7.0 + float(c));
                if (cs > 3.0 && cs < 3.0 + cl)
                {
                    float cr = zon_crackpx(dot(q, cn), cs, i * 0.37 + float(c) * 0.5);
                    if (cr > 0.5 && cr < 1.5) col = zon_earth(0.0);
                    if (cr > 1.5 && zbayer(p) < 0.2) col = zon_earth(4.0);
                }
            }
        }
        if (alive > 0.5 && T < tl + 0.07 && length(float2(q.x / 6.0, q.y / 3.0)) < 1.0) g = max(g, 0.7);
    }
    if (_Fade > 0.001 && col.a > 0.5 && zN(_NoiseTex, p / 2.0).r < _Fade * 1.2) col = zk_none();
    float4 res = col;
    if (glow > 0.5) res = (g > 0.05 && col.a > 0.5) ? zk_glow(_RampTex, min(g, _GlowCap), p, _GlowGain, 2.0) : zk_none();
    return res;
}

// ---------------- 1 SPEAR (one quad per spear) ----------------
float4 zon_spearfx(float2 p, float2 w, float T, float glow)
{
    float4 col = zk_none(); float g = 0.0;
    float i = _SpearIdx;
    float2 ph = p - _HeroOff.xy;                                      // relative to the hero feet
    float2 dir = zon_dir(i); float2 nrm = float2(-dir.y, dir.x);
    float R = zon_land(i), t0 = zon_t0(i), tl = t0 + ZON_TF;
    float L = 18.0 + 6.0 * zon_h(i, 8.0);
    float litSide = dot(nrm, float2(-0.6, 0.8)) > 0.0 ? 1.0 : -1.0;
    float2 I = dir * R;                                            // landing point (hero space)
    float2 q = ph - I;
    // --- flight: f(t0)..f(t0+2) ---
    float u = (T - t0) / ZON_TF;
    if (u >= 0.0 && u < 0.999)
    {
        float sg = 16.0 + (R - 16.0) * (1.0 - zon_p15(1.0 - u));
        float sp = 16.0 + (R - 16.0) * (1.0 - zon_p15(1.0 - max(u - 1.0 / 3.0, 0.0)));   // where it was one frame ago
        if (u < 0.01) sp = 2.0;
        float Z = 11.0 - 5.0 * u * u;                              // chest height, dipping toward the end
        float2 tip = dir * (sg + L * 0.5) + float2(0.0, Z);
        float2 v = ph - tip;
        float a = -dot(v, dir), b = dot(v, nrm);
        // air streak (cool grey, dithered out toward the tail): from the tail back to where the tail was a frame ago
        float sl = max(sg - sp, 4.0) * 0.7;
        float ka = (a - L) / max(sl, 1.0);
        if (ka >= 0.0 && ka < 1.0)
        {
            float hw = 1.6 * (1.0 - ka) + 0.2;
            if (abs(b) < hw && zbayer(p) + 0.5 > ka * 0.9)
            {
                float si = ka < 0.25 ? 4.0 : (ka < 0.6 ? 3.0 : 2.0);
                if (abs(b) > hw - 0.8 && si > 2.5) si = si - 1.0;
                col = zon_streak(si);
            }
        }
        float f = zon_spear(a, b, L, litSide, i * 0.13 + _Seed);
        if (f > -0.5) { col = zon_bone(f); g = f > 2.5 ? 0.55 : 0.3; }
    }
    // --- dive (impact frame): the tail half sticks out of the ground, tilted back toward the hero ---
    float k = floor((T - tl) * 15.0 + 0.5);
    if (T >= tl - 0.001 && k < 0.5)
    {
        float2 ax = normalize(float2(-dir.x * 0.6, -dir.y * 0.6 + 0.8));    // tail direction on screen
        float2 an = float2(-ax.y, ax.x);
        float a = dot(q, ax) + L * 0.45, b = dot(q, an);
        float f = zon_spear(a, b, L, dot(an, float2(-0.6, 0.8)) > 0.0 ? 1.0 : -1.0, i * 0.13 + _Seed);
        if (f > -0.5 && q.y > -1.5) { col = zon_bone(f); g = 0.4; }
    }
    // --- the spike cluster (3 spikes) that erupts where it dives in ---
    float4 sc = zk_none();
    if (T >= tl - 0.001)
    {
        float hk = k < 0.5 ? 0.45 : (k < 1.5 ? 1.12 : 1.0);
        float tb = zon_tb(i);
        float br = max(T - tb, 0.0);
        float sink = br > 0.0 ? 4.0 + br * 50.0 : 0.0;
        // dirt kick: 3 balls thrown outward and up (behind the spikes)
        for (int m = 0; m < 2; m++)
        {
            float fm = float(m);
            float tt = T - tl;
            float side = fm < 0.5 ? -1.0 : 1.0;
            float2 kd = normalize(dir + nrm * side * 0.9);
            float vv = 40.0 + 30.0 * zon_h(i, 10.0 + fm);
            float2 c = I + kd * vv * tt + float2(0.0, 2.0 + 55.0 * tt - 0.5 * 300.0 * tt * tt);
            float age = zsat(tt / 0.3);
            if (tt < 0.3)
            {
                float4 bb = zon_ball(p, ph - c, 1.8 + 0.8 * zon_h(i, 13.0 + fm), zsat(age + 0.3), i + fm * 0.3);
                if (bb.a > 0.5) sc = bb;
            }
        }
        for (int j = 0; j < 3; j++)
        {
            float fj = float(j);
            float off = j == 0 ? 0.0 : (j == 1 ? -6.5 - zon_h(i, 20.0) : 6.0 + zon_h(i, 21.0));
            float hh = (j == 0 ? 20.0 + 6.0 * zon_h(i, 22.0) : 11.0 + 4.0 * zon_h(i, 23.0 + fj)) * hk;
            float ww = j == 0 ? 3.2 : 2.3;
            float lean = dir.x * (j == 0 ? 3.0 : 2.0) + (j == 0 ? 0.0 : (j == 1 ? -1.5 : 1.5));
            float2 base = I + nrm * off + float2(0.0, j == 0 ? 0.0 : -0.5 * dir.y);
            float2 qs = ph - floor(base + float2(0.5, 0.5)) - float2(0.0, 0.5);
            // break: the top third of the main spike snaps off (thrown as a chip on every other cluster), then all sink
            float hv = hh;
            if (br > 0.0 && j == 0) hv = hh * 0.62;
            float2 qq = qs + float2(0.0, sink);
            float f = zon_spike(qq, hv, ww, lean, i + fj);
            float bite = zN(_NoiseTex, p / 2.0 + float2(i * 0.7, fj)).g;
            if (f > -0.5 && qs.y >= -0.5 && bite > zsat(br * 2.2) - 0.25)
            {
                sc = zon_spikec(f);
                if (k < 0.5 && f > 2.5) g = max(g, 0.6);
            }
            // dirt collar at the base while it stands
            if (qs.y >= -1.0 && qs.y < 1.0 && abs(qs.x) < ww + 1.5 && sink < hh && zbayer(p) > -0.1) sc = float4(zramp(_RampTex, 1.0, qs.x < 0.0 ? 3.0 : 2.0), 1.0);
        }
        // snapped tip chip (every other cluster): tumbles outward and falls
        if (br > 0.0 && zmod(i, 2.0) < 0.5 && br < 0.36)
        {
            float2 c = I + dir * (10.0 * br + 2.0) + float2(dir.x * 6.0 * br, 13.0 + 18.0 * br - 0.5 * 260.0 * br * br);
            float lv = zon_piece(ph - floor(c + float2(0.5, 0.5)), 4.0, zmod(floor(br * 15.0) + i, 8.0));
            if (lv > 0.1 && c.y > -2.0) sc = zon_bone(zon_lv2f(lv));
        }
        // dust puff at the base as it sinks
        if (br > 0.0 && br < 0.4)
        {
            float4 bb = zon_ball(p, q - float2(-2.0 * dir.x + 1.0, 2.0 + br * 8.0), 2.8, zsat(br / 0.4), i + 0.7);
            if (bb.a > 0.5) sc = bb;
        }
    }
    if (sc.a > 0.5) col = sc;
    if (_Fade > 0.001 && col.a > 0.5 && zN(_NoiseTex, p / 2.0).r < _Fade * 1.2) col = zk_none();
    float4 res = col;
    if (glow > 0.5) res = (g > 0.05 && col.a > 0.5) ? zk_glow(_RampTex, min(g, _GlowCap), p, _GlowGain, 2.0) : zk_none();
    return res;
}

// ---------------- 2 / 3 BURST: the crown of spikes at the feet ----------------
float4 zon_burst(float2 p, float T, float glow, float front)
{
    float4 col = zk_none(); float g = 0.0;
    float k = floor(T * 15.0 + 0.5);
    for (int j = 0; j < 9; j++)
    {
        float fj = float(j);
        float a = ZTAU * (fj + 0.35 * zon_h(fj, 30.0)) / 9.0 + _Seed * 1.7;
        float sa = sin(a), ca = cos(a);
        float isFront = sa <= 0.0 ? 1.0 : 0.0;
        if (abs(isFront - front) < 0.5)
        {
            float rr = 11.0 + 4.0 * zon_h(fj, 31.0);
            float2 base = floor(float2(ca * rr, sa * rr * 0.6) + float2(0.5, 0.5));
            float hh = (isFront > 0.5 ? 9.0 + 4.0 * zon_h(fj, 32.0) : 14.0 + 5.0 * zon_h(fj, 32.0));
            float hk = k < 0.5 ? 0.7 : (k < 1.5 ? 1.12 : 1.0);
            float tb = (5.0 + zmod(fj, 2.0)) / 15.0;
            float br = max(T - tb, 0.0);
            float hv = hh * hk;
            if (br > 0.0) hv = hh * 0.55;
            float2 qs = p - base;
            float f = zon_spike(qs + float2(0.0, br * 40.0), hv, 2.3, ca * 6.0, fj + 3.0);
            float bite = zN(_NoiseTex, p / 2.0 + float2(fj, 3.0)).g;
            if (f > -0.5 && qs.y >= -0.5 && bite > zsat(br * 3.0) - 0.3) { col = zon_bone(f); if (k < 1.5 && f > 2.5) g = max(g, 0.5); }
            if (qs.y >= -1.0 && qs.y < 1.0 && abs(qs.x) < 3.0 && br < 0.25 && zbayer(p) > -0.1) col = float4(zramp(_RampTex, 1.0, qs.x < 0.0 ? 3.0 : 2.0), 1.0);
            // the snapped tip flies outward (every other spike)
            if (br > 0.0 && br < 0.33 && zmod(fj, 2.0) < 0.5)
            {
                float2 c = base + float2(ca * (40.0 * br + 3.0), hh * 0.8 + 26.0 * br - 0.5 * 280.0 * br * br + sa * 10.0 * br);
                float lv = zon_piece(p - floor(c + float2(0.5, 0.5)), j == 0 ? 6.0 : 4.0, zmod(floor(br * 15.0) * 2.0 + fj, 8.0));
                if (lv > 0.1) col = zon_bone(zon_lv2f(lv));
            }
        }
    }
    // capped flash at the feet (front quad's glow only): 2 frames
    if (front > 0.5 && glow > 0.5 && T < 0.13)
    {
        float dd = length(float2(p.x, (p.y - 6.0) * 1.3));
        float fl = zsat(1.0 - dd / 15.0) * (k < 0.5 ? 1.0 : 0.55);
        if (fl > g) g = fl;
        if (col.a < 0.5 && fl > 0.05) col = float4(1.0, 1.0, 1.0, 1.0);
    }
    if (_Fade > 0.001 && col.a > 0.5 && zN(_NoiseTex, p / 2.0).r < _Fade * 1.2) col = zk_none();
    float4 res = col;
    if (glow > 0.5) res = (g > 0.05 && col.a > 0.5) ? zk_glow(_RampTex, min(g, _GlowCap), p, _GlowGain, 2.0) : zk_none();
    return res;
}

float4 ZFX_Frag(float2 p, float2 w)
{
    float T = zon_t(_FxTime);
    float glow = _Layer > 0.5 ? 1.0 : 0.0;
    float4 c = zk_none();
    if (_Mode < 0.5) c = zon_ground(p, T, glow);
    else if (_Mode < 1.5) c = zon_spearfx(p, w, T, glow);
    else if (_Mode < 2.5) c = zon_burst(p, T, glow, 0.0);
    else c = zon_burst(p, T, glow, 1.0);
    if (_Intensity < 0.999 && c.a > 0.5 && glow < 0.5 && zbayer(w) > _Intensity - 0.5) c = zk_none();
    if (glow > 0.5) c = float4(c.rgb * _Intensity, c.a);
    return c;
}
