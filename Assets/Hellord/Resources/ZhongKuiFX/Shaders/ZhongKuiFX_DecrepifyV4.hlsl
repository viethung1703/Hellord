// Decrepify v4c (Zhong Kui kit v4 RMB special 1): "curse an area: demons are slowed and take more damage" (code
// HellordZhongKui1713.NkSpecial1713 sp == 1: round 2.5 u = 40 px area at the cursor, 6 s, slow 0.3, +25 % damage taken).
// v4c (HM review of v4b at game zoom: "a few faint green scratches and scattered brown clods, not a cursed zone"; VH: "looks
// powerful, not slop"): VALUE CONTRAST first.
//  - The ground ROTS: dark withered blotches (181425 / 3e2731, noise-shaped, with floor showing through and violet mould flecks)
//    cover about half of the zone, densest along the fissures, and stop at the true radius in a ragged edge of rot TENDRILS that
//    reach 2-6 px out. That textured darkening is what makes the 40 px silhouette read; it is never a uniform fill, disc or ring.
//  - THICK GLOWING FISSURES: _Cracks (4) meandering fissures, up to 6 px wide: ink walls, a 3e8948 seep body and a 63c74d hot
//    core line (_Hot), violet-withered lit lips; capped green/violet glow hugs them (glow twin, 1-3 px, never a wash); seep pools
//    at their off-centre starts; a pulse runs along them every _PulseEvery s. No spokes, no cell-crack web, no pale clods.
//  - Withering motes (2x2 green -> violet -> gone) rise from the fissures all the time.
//  - The cursed demons get their own tell: ZhongKuiFX_DecrepifyMarkV4 (ZKFX_Decrepify_Mark), see spec.md.
// Bone and earth plus the curse seep: no ghosts, no spirit light. Portable HLSL subset (also compiled as GLSL).
//  ignite f0 violet burst at the cast point; f1-f3 the fissures grow out of their pools and the rot spreads behind the front;
//         f4-f6 the rim tendrils sprout; f1-f9 violet streaks fall into the area and splat green.
//  hold   seep pockets, pulses, motes. fade (_Fade, code: last 0.4 s): noise dissolve from the rim inward.
// Light from the upper left. Ground layer (code layer 0, under the sprites).

float zdc_tq() { return floor(_FxTime * _StepFps + 0.0001) / _StepFps; }
// earth row 3: 0 181425 ink, 1 262b44, 2 3e2731, 3 733e39, 4 b86f50, 5 c28569, 6 e8b796
float4 zdc_earth(float i) { return float4(zramp(_RampTex, 3.0, i), 1.0); }
// curse row 4: 0 181425, 1 262b44, 2 3e2731, 3 68386c, 4 b55088
float4 zdc_curse(float i) { return float4(zramp(_RampTex, 4.0, i), 1.0); }
// seep row 5: 0 181425, 1 193c3e, 2 265c42, 3 3e8948, 4 63c74d
float4 zdc_seep(float i) { return float4(zramp(_RampTex, 5.0, i), 1.0); }
// dust row 1: 0 181425 1 3e2731 2 733e39 3 b86f50 4 c28569 5 e8b796
float4 zdc_dust(float i) { return float4(zramp(_RampTex, 1.0, i), 1.0); }
float zdc_r(float i, float k) { return zhash(float2(i * 7.31 + k * 3.17 + 1.3, _Seed * 97.0 + k * 13.7)); }
// the rim radius at angle a (irregular; mean = R)
float zdc_rim(float a, float R)
{
    float n = zN(_NoiseTex, float2(a * 0.9 + _Seed * 5.0, _Seed * 3.1)).r;
    return R + (n - 0.45) * 5.0;
}
// fissure i: start (a seep pool near the edge, at uneven angles) and start heading: ACROSS the area along an off-centre chord,
// alternating sides, so the fissures cross each other instead of converging on one point
float zdc_sa(float i) { return _Seed * ZTAU + i * ZTAU / max(1.0, _Cracks) + (zdc_r(i, 21.0) - 0.5) * 0.9; }
float2 zdc_start(float i, float R)
{
    float a = zdc_sa(i);
    float rr = R * (0.5 + 0.3 * zdc_r(i, 22.0));
    return floor(float2(cos(a), sin(a)) * rr) + float2(0.5, 0.5);
}
float zdc_head0(float i)
{
    float sg = zmod(i, 2.0) < 0.5 ? 1.0 : -1.0;
    return zdc_sa(i) + 3.14159265 + sg * (0.3 + 0.4 * zdc_r(i, 24.0));
}
// irregular seep pool: radius pr modulated by noise around it
float zdc_pool(float2 q, float pr, float i)
{
    float a = atan2(q.y, q.x);
    float m = zN(_NoiseTex, float2(a * 0.6 + i * 1.7, i * 0.9 + _Seed * 4.0)).r;
    return length(float2(q.x, q.y * 1.25)) - pr * (0.7 + 0.6 * m);
}

float4 ZFX_Frag(float2 p, float2 w)
{
    float4 c = zk_none();
    float glow = _Layer > 0.5 ? 1.0 : 0.0;
    float tq = zdc_tq();
    float R = max(8.0, _Radius);
    float r = length(p);
    float ang = atan2(p.y, p.x);
    float rimR = zdc_rim(ang, R);
    float2 L = float2(-0.7071, 0.7071);
    float grow = tq < 0.27 ? 60.0 * zeaseOut((tq + 0.067) / 0.334) : 999.0;     // px of fissure grown since the cast
    float rf = tq < 0.27 ? (R + 2.0) * zeaseOut((tq + 0.067) / 0.334) : R + 8.0; // the rot front
    float fn = zN(_NoiseTex, p / 9.0 + float2(_Seed * 3.7, 0.4)).r;
    bool faded = _Fade > 0.001 && _Fade > 1.05 - 0.55 * zsat(r / R) - 0.4 * fn;
    float ph = zmod(tq - 0.4, max(0.5, _PulseEvery));
    bool pulseOn = tq >= 0.4 && ph < 0.47;
    float sf = ph / 0.4 * 56.0;                                                  // pulse front, px along each fissure
    float gf = 0.0;
    float crack = -1.0;            // 0..6 earth; 10+ seep; 20+ curse
    float inside = r < rimR - 1.0 && !faded ? 1.0 : 0.0;
    float NF = clamp(floor(_Cracks + 0.5), 2.0, 5.0);
    // ---- fissures (random-walk polylines) ----
    float bd = 99.0; float bside = 0.0; float bhw = 0.0; float bs = 0.0; float bpool = 99.0; float bpr = 0.0; float bfi = 0.0;
    float2 bn = float2(0.0, 1.0);
    for (int ii = 0; ii < 5; ii++)
    {
        float fi = float(ii);
        if (fi < NF)
        {
            float2 P = zdc_start(fi, R);
            float hd = zdc_head0(fi);
            float pr = 3.2 + 1.6 * zdc_r(fi, 25.0);
            float dpl = zdc_pool(p - P, pr, fi);
            if (dpl < bpool) { bpool = dpl; bpr = pr; }
            float2 B = P; float bh = hd; float s0 = 0.0;
            float kb = 1.0 + floor(zdc_r(fi, 26.0) * 3.0);
            for (int kk = 0; kk < 10; kk++)
            {
                float fk = float(kk);
                // 0..6 main fissure, 7..9 its branch (from vertex kb)
                bool br = fk > 6.5;
                if (fk > 6.5 && fk < 7.5) { P = B; hd = bh; s0 = kb * 8.0; }
                float seglen = br ? 6.5 : 8.0;
                hd = hd + (zdc_r(fi * 17.0 + fk, 27.0) - 0.5) * 0.95;
                float2 A = P;
                float2 Bq = P + float2(cos(hd), sin(hd)) * seglen;
                float2 ab = Bq - A; float2 ap = p - A;
                float h = zsat(dot(ap, ab) / max(0.001, dot(ab, ab)));
                float d = length(ap - ab * h);
                float u = br ? (fk - 7.0 + h) / 3.0 : (fk + h) / 7.0;
                float hw = br ? 0.85 + 0.75 * sin(3.14159 * zsat(u)) * (1.0 - u * 0.5) : 1.15 + 1.75 * sin(3.14159 * zsat(u * 0.92 + 0.04));
                float s = s0 + (br ? (fk - 7.0) * seglen : fk * seglen) + h * seglen;
                hw = hw * (0.7 + 0.6 * zN(_NoiseTex, float2(s * 0.09 + fi * 0.71, fi * 1.3 + _Seed * 3.0)).r);   // uneven width
                d = d + (zN(_NoiseTex, p / 3.0 + float2(fi * 0.3, _Seed * 7.0)).g - 0.5) * 1.4;            // ragged, broken edges
                if (s <= grow && d - hw < bd)
                {
                    bd = d - hw; bhw = hw; bs = s; bfi = fi;
                    float cr = ab.x * ap.y - ab.y * ap.x;
                    bside = cr > 0.0 ? 1.0 : -1.0;
                    bn = float2(-ab.y, ab.x) / max(0.001, length(ab)) * bside;
                }
                if (fk > kb - 0.5 && fk < kb + 0.5) { B = Bq; bh = hd + (zdc_r(fi, 28.0) > 0.5 ? 0.85 : -0.85); }
                P = Bq;
            }
        }
    }
    // ---- the rot: dark withered blotches between the fissures (textured, never a fill), fraying out past the true radius ----
    float rotv = -1.0;
    float out0 = r - rimR;                                                       // px past the (irregular) true radius
    if (!faded && r < rf && out0 < 7.0)
    {
        float rn = zN(_NoiseTex, (p + float2(_Seed * 41.0, _Seed * 17.0 + 5.0)) / 13.0).r;
        float rn2 = zN(_NoiseTex, (p + float2(_Seed * 13.0, 3.0)) / 4.0).g;
        float near = zsat(1.0 - max(0.0, bd) / 9.0);
        float npool = zsat(1.0 - max(0.0, bpool) / 8.0);
        float sc = rn * 1.0 + rn2 * 0.5 + near * 0.25 + npool * 0.2 + 0.06;
        sc = sc - 0.15 * zsat((out0 + 4.0) / 4.0);                                 // the blotches thin out towards the edge
        if (out0 > -4.0)
        {
            // ... and the edge frays into rot TENDRILS in clumps (polar-stretched noise: thin fingers pointing outward, 0-7 px
            // past the radius, with gaps between the clumps: never a closed outline)
            float kk2 = floor(ZTAU * R / 16.0 + 0.5) * 4.0;
            float pn = zN(_NoiseTex, float2((ang + 3.14159265) / ZTAU * kk2, r / 22.0 + _Seed * 3.0)).g;
            float reach = (tq < 0.4 ? zsat((tq - 0.26) / 0.14) : 1.0) * (pn - 0.54) * 34.0 - 3.0;
            float st = -4.0 + (0.62 - pn) * 20.0;                                   // weak spots start later: no closed band
            if (out0 < reach && out0 > st) sc = max(sc, 0.8 + (pn - 0.5) * 0.8);
        }
        if (sc > 0.93) rotv = 0.0;                                               // 181425 deep rot
        else if (sc > 0.77) rotv = 2.0;                                          // 3e2731 withered
        else if (sc > 0.69 && zbayer(w) > 0.0) rotv = 2.0;                       // ragged dithered edge
        if (rotv > -0.5 && out0 < -2.0 && zhash(floor(p) + float2(_Seed * 31.0, 7.0)) > 0.95) rotv = 23.0;   // violet mould flecks
    }
    if (inside > 0.5)
    {
        float ld = dot(bn, L);
        float lit = abs(ld) < 0.03 ? (bside > 0.0 ? 1.0 : 0.0) : (ld < 0.0 ? 1.0 : 0.0);   // the wall facing away from the light catches it
        float dc = bd + bhw;                                                    // distance from the fissure centre line
        if (bd <= 0.0)
        {
            crack = 0.0;                                                       // ink walls
            if (bd < -0.75)
            {
                float pk = zN(_NoiseTex, float2(bs * 0.045 + bfi * 0.37, _Seed * 2.0 + bfi)).g;
                crack = 13.0;                                                  // 3e8948 seep body
                if (_Hot > 0.5 && ((dc < 0.8 && bhw > 1.5) || (pk > 0.6 && dc < 1.3))) crack = 14.0;   // 63c74d hot core line
                if (pulseOn && abs(bs - sf) < 3.0) crack = 14.0;
            }
        }
        else if (bd <= 1.0) crack = lit > 0.5 ? 23.0 : 0.0;                     // withered violet lip / shadow lip
        // seep glow up out of the fissure cores (glow twin): hugs them, 1-3 px
        if (bd <= 0.5 && bhw > 1.0) gf = max(gf, 0.62);
        else if (bd <= 2.5) gf = max(gf, 0.3);
        if (pulseOn && abs(bs - sf) < 5.0 && bd <= 2.0) gf = max(gf, 0.95);
        // seep pools at the fissure starts
        if (tq >= 0.067 && bpool <= 0.0)
        {
            crack = bpool < -0.9 ? 13.0 : 0.0;                                       // ink rim, 3e8948 body
            if (bpool < -0.9 && zN(_NoiseTex, w / 2.0 + float2(tq * 0.7, 0.3)).g > 0.52) crack = 14.0;   // slow-bubbling 63c74d
            if (bpool < -0.9 && bpool > -1.8) crack = 12.0;
            if (bpool < -0.9 && pulseOn && ph < 0.2) crack = 14.0;
        }
        if (tq >= 0.067 && bpool < 3.0) gf = max(gf, 0.7 * (1.0 - zsat((bpool + bpr) / (bpr + 3.0))) + 0.2);
        // f0-f2 the violet cast burst at the cast point
        if (tq < 0.2 && r < 5.0 - tq * 10.0 && crack < 9.5) crack = r < 2.5 - tq * 8.0 ? 24.0 : 23.0;
    }
    if (crack < -0.5) crack = rotv;
    // ---- cast streaks (violet, falling in) and withering motes (rising from the fissures, all the time) ----
    float ash = -1.0;
    for (int jj = 0; jj < 12; jj++)
    {
        float fj = float(jj);
        if (jj < 9)
        {
            float ar = R * 0.82 * sqrt(zdc_r(fj, 11.0));
            float aa = ZTAU * zdc_r(fj, 12.0);
            float2 pj = floor(float2(cos(aa), sin(aa)) * ar);
            float t0 = 0.067 + 0.045 * fj;
            float k = (tq - t0) / 0.2;
            if (k >= 0.0 && k < 1.0)
            {
                float2 q = p - pj - float2(0.0, floor(20.0 * (1.0 - k)));
                if (q.x > -0.6 && q.x < 1.4 && q.y >= 0.0 && q.y < 5.0) ash = q.y < 1.9 ? 24.0 : 23.0;
            }
            else if (k >= 1.0 && k < 1.67)
            {
                float2 q = p - pj;
                if (q.y > -0.6 && q.y < 1.4 && q.x > -1.6 && q.x < 1.4) ash = 14.0;
            }
        }
        // withering motes: period 1.6 s, desynced; each rises 14 px from a point on a fissure and withers green -> violet -> gone
        if (tq >= 0.33 && !faded)
        {
            float per = 1.6;
            float tt = tq + fj * 0.133 * per + zdc_r(fj, 31.0) * 0.1;
            float cyc = floor(tt / per);
            float km = (tt - cyc * per) / per;
            float fi = zmod(fj + cyc, NF);
            float2 P0 = zdc_start(fi, R) + float2(cos(zdc_head0(fi)), sin(zdc_head0(fi))) * (4.0 + 26.0 * zdc_r(fj + cyc * 3.0, 32.0));
            float2 pm = floor(P0 + float2(floor(sin(km * 9.0 + fj) * 1.2 + 0.5), 14.0 * km));
            float2 q = p - pm;
            bool big = km < 0.45;
            if (q.x > -0.6 && q.x < (big ? 1.4 : 0.6) && q.y > -0.6 && q.y < (big ? 1.4 : 0.6) && length(pm) < R - 3.0)
            {
                float m = km < 0.25 ? 14.0 : (km < 0.5 ? 13.0 : (km < 0.8 ? 24.0 : 23.0));
                if (km < 0.8 || zbayer(w) > 0.0) ash = m;
                if (km < 0.45) gf = max(gf, 0.5);
            }
        }
    }
    if (glow > 0.5)
    {
        float g = gf;
        if (tq < 0.2 && r < 10.0) g = max(g, (1.0 - r / 10.0) * (1.0 - tq / 0.2) * 1.2);   // the cast flash
        if (inside < 0.5 && !(ash > -0.5)) g = 0.0;
        g = min(g, _GlowCap);
        if (g > 0.04) c = zk_glow(_RampTex, g, w, _GlowGain, 2.0);
        c = float4(c.rgb * _Intensity * _Ignite, c.a);
    }
    else
    {
        if (crack > 19.5) c = zdc_curse(crack - 20.0);
        else if (crack > 9.5) c = zdc_seep(crack - 10.0);
        else if (crack > -0.5) c = zdc_earth(crack);
        if (ash > 19.5) c = zdc_curse(ash - 20.0);
        else if (ash > 9.5) c = zdc_seep(ash - 10.0);
        float k = _Intensity * _Ignite;
        if (k < 0.999 && c.a > 0.5 && zbayer(w) > k - 0.5) c = zk_none();
    }
    return c;
}
