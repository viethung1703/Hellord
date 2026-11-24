// Leizu body overlays (t1666). Pivot = the unit's feet. _Body.xy = body box w,h px; _Body.z = extra head offset px.
// Everything is strokes over / around the silhouette (no body-sized blobs), re-struck on the 15 fps step.
// v2 (t1666 upgrade): every arc / fur / streak stroke 2 px (was 1 px), hot shock arcs get a 1 px cyan fringe, overload strikes 3 px,
// ward nodes 5 px with a fatter absorb star and a spark where the grounding arc lands, glows wider (field^1.5).
//  0 shock status   1 grounding ward (2 orbiting charge nodes)   2 storm stride static   3 mark of judgement (sigil + charge)
//  4 thunder hound aura   5 tribunal third eye   6 dazzled (v2.1: gold / white stars with ink outline orbiting the head)   7 live wire overload   8 conductivity cursed
//  9 thunder drum beat pips (3 diamonds over the head, _Charge = beats / 3, _Pulse = beat flare)
float4 lz_acol(float k, float vi)     // k 2 white, 1 cyan, 0 blue; vi = violet variant
{
    if (vi > 0.5) return float4(zramp(_RampTex, 2.0, k > 1.5 ? 4.0 : (k > 0.5 ? 3.0 : 2.0)), 1.0);
    return float4(zramp(_RampTex, 0.0, 2.0 + k), 1.0);
}
float2 lz_perim(float a, float2 ctr, float2 rad) { return ctr + float2(cos(a), sin(a)) * rad; }
// one re-struck arc across / around the body. Returns distance, sets hot (1 on its first step)
float lz_bodyarc(float2 p, float2 ctr, float2 rad, float fi, float per, float lifeSteps, float seed, float amp, out float hot)
{
    float st = floor(_FxTime * _StepFps) + fi * 5.0;
    float cyc = floor(st / per); float ph = st - cyc * per;
    hot = ph < 1.0 ? 1.0 : 0.0;
    if (ph >= lifeSteps) return 100000.0;
    float h1 = zhash(float2(cyc + fi * 7.0, seed * 41.0)); float h2 = zhash(float2(cyc * 1.7 + fi, seed * 13.0 + 2.0));
    float a0 = h1 * ZTAU; float a1 = a0 + ZPI * (0.45 + 0.6 * h2);
    float u;
    return lz_jag(p, lz_perim(a0, ctr, rad), lz_perim(a1, ctr, rad * (0.7 + 0.3 * h2)), 4.0, amp, frac(seed + cyc * 0.13 + ph * 0.29), u);
}
float4 ZFX_Frag(float2 p, float2 w)
{
    float bw = _Body.x; float bh = _Body.y;
    float2 ctr = float2(0.0, bh * 0.5);
    float2 rad = float2(bw * 0.5 + 1.0, bh * 0.5 + 1.0);
    float st = floor(_FxTime * _StepFps);
    float I = _Intensity;
    float gl = 0.0;
    float4 c = float4(0.0, 0.0, 0.0, 0.0);
    if (I <= 0.001) return c;
    if (_Mode < 0.5 || (_Mode > 6.5 && _Mode < 8.5))
    {   // ---- shock (0), overload (7), cursed (8)
        float vi = _Mode > 7.5 ? 1.0 : 0.0;
        float n = _Mode < 0.5 ? 2.0 : (_Mode < 7.5 ? 4.0 : 1.0);
        for (int i = 0; i < 4; i++)
        {
            float fi = float(i);
            if (fi >= n * I + 0.01) break;
            float hot;
            float d = lz_bodyarc(p, ctr, rad, fi, _Mode < 7.5 ? 4.0 + fi : 7.0, _Mode > 6.5 && _Mode < 7.5 ? 3.0 : 2.0, _Seed, 2.5, hot);
            float wd = (_Mode > 6.5 && _Mode < 7.5 && hot > 0.5) ? 1.5 : 0.9;      // v2: 2 px arcs (3 px on overload strikes)
            if (d <= wd) { c = lz_acol(hot > 0.5 ? 2.0 : 1.0, vi); gl = max(gl, 0.9 * I); }
            else if (d <= wd + 1.0 && hot > 0.5) { c = lz_acol(vi > 0.5 ? 1.0 : 0.0, vi); gl = max(gl, 0.6 * I); }
            else gl = max(gl, pow(zsat(1.0 - d / 6.0), 1.5) * 0.6 * I);
        }
        // overload: the whole silhouette spikes (electric fur) on alternate steps
        if (_Mode > 6.5 && _Mode < 7.5)
        {
            float2 q = (p - ctr) / rad; float rq = length(q); float aq = atan2(q.y, q.x);
            float bi = floor((aq / ZTAU + 0.5) * 16.0);
            float hb = zhash(float2(bi, st));
            float ac = (bi + 0.5) / 16.0 * ZTAU - ZPI;
            float perp = abs(length(p - ctr) * sin(aq - ac));
            if (hb < 0.5 && perp < 0.9 && rq > 0.95 && rq < 1.0 + (0.25 + 0.3 * hb) && q.y > -0.45 && c.a < 0.5) { c = lz_acol(1.0, 0.0); gl = max(gl, 0.6); }
            gl = max(gl, zsat(1.0 - abs(rq - 1.0) * 4.0) * 0.25 * (zmod(st, 2.0) < 0.5 ? 1.0 : 0.5) * I);
        }
        // cursed: two violet motes rising off the shoulders
        if (_Mode > 7.5)
        {
            for (int k = 0; k < 2; k++)
            {
                float fk = float(k);
                float ph = frac(_FxTime * 0.7 + fk * 0.5 + _Seed);
                float2 mp = float2((fk - 0.5) * bw * 0.8, bh * 0.6 + floor(ph * 10.0));
                float2 q = p - mp;
                if (q.x >= 0.0 && q.x < 2.0 && q.y >= 0.0 && q.y < 2.0 && ph < 0.8) { c = float4(zramp(_RampTex, 2.0, 3.0), 1.0); gl = max(gl, 0.3 * I); }
            }
        }
    }
    else if (_Mode < 1.5)
    {   // ---- grounding ward: up to 2 charge nodes orbiting at chest height, arcs to the ground, absorb flare (_Pulse)
        float cy = bh * 0.55; float orx = bw * 0.5 + 5.0; float ory = 3.0;
        float charges = floor(_Charges + 0.5);
        for (int i = 0; i < 2; i++)
        {
            float fi = float(i);
            if (fi >= charges) break;
            float a = _FxTime * 3.2 + fi * ZPI + _Seed * ZTAU;
            float2 np = float2(cos(a) * orx, cy + sin(a) * ory);
            bool back = sin(a) > 0.0;
            if (back && abs(p.x) < bw * 0.5 - 1.0 && p.y > 0.0 && p.y < bh) continue;   // passes behind the body
            float2 q = p - np; float rq = length(q);
            float seed = lz_stepseed(_Seed + fi * 0.37, 1.0);
            if (rq <= 2.4) c = lz_acol(rq <= 1.3 ? 2.0 : 1.0, 0.0);
            for (int k = 0; k < 2; k++)
            {
                float fk = float(k); float h = zhash(float2(fk * 3.1 + fi, seed * 31.0));
                float aa = h * ZTAU; float u;
                float d = lz_jag(p, np, np + float2(cos(aa), sin(aa)) * (4.0 + 3.0 * _Pulse), 2.0, 1.2, seed + fk * 0.3, u);
                if (d <= 0.9 && c.a < 0.5) c = lz_acol(1.0, 0.0);
            }
            if (_Pulse > 0.3) { float f = lz_star(q, 4.0 + 6.0 * _Pulse, 1.0); if (f > 0.0) c = lz_acol(f > 0.4 ? 2.0 : 1.0, 0.0); }
            gl = max(gl, zsat(1.0 - rq / (6.0 + 4.0 * _Pulse)) * (1.0 + _Pulse));
            // grounding arc: every 8 steps, one step long, node -> ground beside the feet
            float cyc = floor((st + fi * 4.0) / 8.0); float ph = (st + fi * 4.0) - cyc * 8.0;
            if (ph < 1.0)
            {
                float2 gp = float2(np.x * 1.2 + (zhash(float2(cyc, fi)) - 0.5) * 4.0, -1.0);
                float u;
                float dg = lz_jag(p, np, gp, 4.0, 2.0, frac(cyc * 0.31 + fi), u);
                if (dg <= 0.9 && c.a < 0.5) c = lz_acol(u > 0.85 ? 2.0 : 1.0, 0.0);
                gl = max(gl, pow(zsat(1.0 - dg / 6.0), 1.5) * 0.7);
                float fg = lz_star(p - gp, 3.0, 0.5);                       // spark where it grounds
                if (fg > 0.0 && c.a < 0.5) c = lz_acol(fg > 0.4 ? 2.0 : 1.0, 0.0);
            }
        }
    }
    else if (_Mode < 2.5)
    {   // ---- storm stride static: crackle at the feet + streaks peeling off the back of the body
        float2 vd = length(_Vel.xy) > 1.0 ? normalize(_Vel.xy) : float2(1.0, 0.0);
        float hot;
        float2 fc = float2(0.0, 1.0); float2 fr = float2(bw * 0.5 + 2.0, 2.0);
        float d = lz_bodyarc(float2(p.x, p.y), fc, fr, 0.0, 3.0, 2.0, _Seed, 1.5, hot);
        if (d <= 0.9) { c = lz_acol(hot > 0.5 ? 2.0 : 1.0, 0.0); gl = max(gl, 0.7 * I); }
        for (int i = 0; i < 2; i++)
        {
            float fi = float(i);
            float y = bh * (0.35 + 0.3 * fi) + (zhash(float2(st, fi)) - 0.5) * 3.0;
            float2 a0 = float2(-vd.x * bw * 0.45, y); float2 a1 = a0 - vd * (6.0 + 6.0 * I * zhash(float2(st * 1.3, fi + 2.0)));
            float u;
            float ds = lz_jag(p, a0, a1, 3.0, 1.2, frac(_Seed + st * 0.11 + fi * 0.4), u);
            if (ds <= 0.9 && zhash(float2(st, fi + 9.0)) < 0.75 * I) { c = lz_acol(u < 0.3 ? 1.0 : 0.0, 0.0); gl = max(gl, 0.4 * I); }
        }
    }
    else if (_Mode < 3.5)
    {   // ---- mark of judgement: violet/white diamond sigil over the head, charge crackle on the body
        float hy = bh + 7.0 + _Body.z;
        float2 q = p - float2(0.0, hy + floor(sin(_FxTime * 3.0) * 1.0 + 0.5));
        float form = zeaseOut(_Form);
        float dia = abs(q.x) / 4.0 + abs(q.y) / 6.0;                 // diamond 9 x 13
        float ring = abs(dia - 1.0) * 4.0;
        bool drawn = (atan2(q.x, q.y) / ZTAU + 0.5) <= form;          // traced in from the top
        float ch = _Charge;
        if (ring <= 0.9 && drawn)
        {
            float k = ch > 0.66 ? 2.0 : (ch > 0.33 ? 1.0 : 0.0);
            c = (ch > 0.99 && zmod(st, 2.0) < 0.5) ? lz_acol(2.0, 0.0) : lz_acol(k, 1.0);
            gl = max(gl, 0.4 + 0.6 * ch);
        }
        if (dia < 0.4 && form > 0.95) { c = (ch > 0.5) ? lz_acol(2.0, 0.0) : lz_acol(1.0, 1.0); gl = max(gl, 0.5 + 0.5 * ch); }
        // tick marks (four short rays) appear as charge builds
        float f = lz_star(q, 13.0, 0.5);
        if (f > 0.0 && dia > 1.25 && f > 1.0 - ch * 0.7 && form > 0.95) { c = lz_acol(ch > 0.8 ? 2.0 : 1.0, 1.0); gl = max(gl, 0.5); }
        // charge crackle on the body (more arcs with more charge)
        float n = bh >= 1.0 ? floor(ch * 3.0 + 0.5) : 0.0;     // pivot-on-the-head use (_Body.y 0): sigil only
        for (int i = 0; i < 3; i++)
        {
            float fi = float(i);
            if (fi >= n) break;
            float hot;
            float d = lz_bodyarc(p, ctr, rad, fi, 6.0 - fi, 2.0, _Seed, 2.0, hot);
            if (d <= 0.9 && c.a < 0.5) { c = lz_acol(hot > 0.5 ? 2.0 : 1.0, 1.0); gl = max(gl, 0.6); }
        }
    }
    else if (_Mode < 4.5)
    {   // ---- thunder hound aura: electric fur along the top of the body + trailing sparks + paw crackle
        float2 vd = length(_Vel.xy) > 1.0 ? normalize(_Vel.xy) : float2(1.0, 0.0);
        float2 q = (p - ctr) / rad; float rq = length(q); float aq = atan2(q.y, q.x);
        float bi = floor((aq / ZTAU + 0.5) * 18.0);
        float hb = zhash(float2(bi, floor(st / 2.0) + _Seed * 9.0));
        float ac = (bi + 0.5) / 18.0 * ZTAU - ZPI;
        float perp = abs(length(p - ctr) * sin(aq - ac));
        if (q.y > -0.2 && hb < 0.55 && perp < 0.9 && rq > 0.9 && rq < 1.0 + 0.2 + 0.35 * hb) { c = lz_acol(hb < 0.15 ? 2.0 : 1.0, 0.0); gl = max(gl, 0.6 * I); }
        float hot;
        float d = lz_bodyarc(p, ctr, rad * 0.85, 0.0, 3.0, 2.0, _Seed, 2.0, hot);
        if (d <= 0.9 && c.a < 0.5) { c = lz_acol(hot > 0.5 ? 2.0 : 1.0, 0.0); gl = max(gl, 0.7 * I); }
        for (int i = 0; i < 3; i++)
        {
            float fi = float(i);
            float ph = frac(_FxTime * 3.0 + fi / 3.0 + _Seed);
            float2 sp = ctr - vd * (bw * 0.4 + ph * 14.0) + float2(0.0, (fi - 1.0) * 3.0 - ph * 2.0);
            float ds = zsegd(p, sp, sp - vd * 2.0);
            if (ds <= 0.9 && ph < 0.8) { c = lz_acol(ph < 0.3 ? 1.0 : 0.0, 0.0); gl = max(gl, 0.3 * I); }
        }
        gl = max(gl, zsat(1.0 - abs(rq - 1.0) * 2.5) * 0.25 * I);
    }
    else if (_Mode < 5.5)
    {   // ---- tribunal third eye over the head: vesica eye opening with _Form, cyan iris, violet lids, 4 rays
        float hy = bh + 8.0 + _Body.z;
        float2 q = p - float2(0.0, hy);
        float open = zeaseOut(_Form);
        float ew = 7.0; float eh = 3.5 * open + 0.5;
        float xn = q.x / ew;
        float lid = eh * (1.0 - xn * xn);
        float dl = abs(abs(q.y) - lid);
        if (abs(xn) <= 1.05 && dl <= 0.9) { c = lz_acol(1.0, 1.0); gl = max(gl, 0.5); }
        if (abs(q.y) < lid - 0.5 && length(q) <= 2.2 * open) { c = (_Pulse > 0.3 || zmod(st, 6.0) < 1.0) ? lz_acol(2.0, 0.0) : lz_acol(1.0, 0.0); gl = max(gl, 0.9); }
        float f = lz_star(q, 6.0 + 4.0 * _Pulse, 0.5);
        if (f > 0.0 && abs(q.y) > lid + 1.5 && open > 0.9) { c = lz_acol(_Pulse > 0.3 ? 2.0 : 1.0, 0.0); gl = max(gl, 0.4 + 0.4 * _Pulse); }
        gl = max(gl, zsat(1.0 - length(q * float2(0.6, 1.2)) / 8.0) * open * 0.6);
    }
    else if (_Mode > 8.5)
    {   // ---- drum beat pips: three 5x5 diamonds over the head; lit ones cyan, the newest flares white on _Pulse
        float hy = bh + 6.0 + _Body.z;
        float lit = floor(_Charge * 3.0 + 0.01);
        for (int i = 0; i < 3; i++)
        {
            float fi = float(i);
            float2 q = p - float2((fi - 1.0) * 6.0, hy);
            float dm = abs(q.x) + abs(q.y);
            if (dm > 2.6) continue;
            bool on = fi < lit;
            bool newest = abs(fi - (lit - 1.0)) < 0.5;
            if (on)
            {
                c = (newest && _Pulse > 0.3) || dm < 1.1 ? lz_acol(2.0, 0.0) : lz_acol(1.0, 0.0);
                gl = max(gl, 0.6 + 0.4 * (newest ? _Pulse : 0.0));
            }
            else if (dm > 1.6) c = float4(zramp(_RampTex, 0.0, 1.0), 1.0);
            else c = float4(zramp(_RampTex, 7.0, 0.0), 1.0);
        }
    }
    else
    {   // ---- dazzled (v2.1): three gold / white 4-point stars orbiting the head on a flat ellipse (rx 6, ry 2), each a 3 px white
        //      square heart + gold arms (4 px horizontal / 1 px vertical, +1 on its twinkle step) + a 1 px ink outline so it reads on any
        //      floor at game zoom. Warm glow (gold, not the blue lightning glow). Stars behind the head (upper half of the orbit) are dimmer
        //      and hidden where they cross the head. Pivot = feet; orbit centre = _Body.y + 5 (+ _Body.z). Fits the game rect (-14, h-4, 28, 16).
        float hy = bh + 5.0 + _Body.z;
        float best = 0.0;        // 3 heart white, 2 arm gold, 1 tip pale, 0.5 outline, 0 none
        bool back = false;
        for (int i = 0; i < 3; i++)
        {
            float fi = float(i);
            float a = floor(_FxTime * _StepFps) / _StepFps * 4.0 + fi * ZTAU / 3.0;
            float2 sp = float2(floor(cos(a) * 6.0 + 0.5), hy + floor(sin(a) * 2.0 + 0.5));
            float2 q = floor(p - sp);                                         // whole-pixel offsets (p sits on pixel centres)
            float ax = abs(q.x); float ay = abs(q.y);
            bool behind = sin(a) > 0.35;
            if (behind && abs(sp.x) < bw * 0.35) continue;                    // hidden behind the head
            float tw = zmod(st + fi * 2.0, 4.0) < 1.0 ? 1.0 : 0.0;           // twinkle step: arms +1
            float lh = 4.0 + tw; float lv = 1.0 + tw;
            float k = 0.0;
            if (ax <= 1.0 && ay <= 1.0) k = behind ? 2.0 : 3.0;                 // 3x3 px white heart (gold when behind)
            else if ((ay < 0.5 && ax <= lh) || (ax < 0.5 && ay <= lv)) k = (ax >= lh - 0.5 || ay >= lv - 0.5) ? 1.0 : 2.0;
            else if ((ay <= 1.5 && ax <= lh + 1.0) || (ax <= 1.5 && ay <= lv + 1.0) || (ax <= 2.0 && ay <= 2.0 && ax + ay <= 3.0)) k = 0.5;
            if (k > best) { best = k; back = behind; }
            gl = max(gl, zsat(1.0 - length(q * float2(0.8, 1.2)) / 4.0) * (behind ? 0.4 : 0.8) * (1.0 + 0.3 * tw));
        }
        if (zbayer(w) > I * 1.05) best = 0.0;                                         // _Intensity fade-out: ordered dither
        if (best > 2.5) c = float4(zramp(_RampTex, 5.0, 4.0), 1.0);                   // ffffff
        else if (best > 1.5) c = float4(zramp(_RampTex, 4.0, back ? 5.0 : 6.0), 1.0);  // feae34 gold (b86f50 behind)
        else if (best > 0.75) c = float4(zramp(_RampTex, 4.0, 7.0), 1.0);              // fee761 pale tip
        else if (best > 0.25) c = float4(zramp(_RampTex, 0.0, 0.0), 1.0);              // 181425 ink outline
        if (_Layer > 0.5)
        {   // warm additive glow (2 dithered bands: dark gold, gold), not the blue lightning glow
            float b = zband(zsat(gl * I), 3.0, w, 0.45);
            float3 gcol = b > 1.5 ? zramp(_RampTex, 4.0, 6.0) * 0.40 : (b > 0.5 ? zramp(_RampTex, 4.0, 5.0) * 0.28 : float3(0.0, 0.0, 0.0));
            c = float4(gcol * _GlowGain, 1.0);
            gl = -1.0;                                                         // handled here
        }
    }
    if (_Layer > 0.5 && gl >= 0.0) c = zglow(_RampTex, gl * I, w, _GlowGain);
    return c;
}
