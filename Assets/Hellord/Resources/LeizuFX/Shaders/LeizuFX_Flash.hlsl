// Leizu flash family (t1666): short, hard, few-frame lightning flashes. All shapes are strokes / spikes (no discs).
//  _Mode 0 impact star   1 teleport out (body wrapped in strands -> white column -> retracts upward)
//  _Mode 2 teleport in   (bolt drops into the spot -> strands burst off the body -> ground spikes)
//  _Mode 3 mirror dazzle (big 4-ray light star + glints; pale white / yellow, no blue arcs: it is light, not lightning)
//  _Mode 4 bite snap     (two zig-zag jaws close along _Dir, spark spray on the snap)
//  _Mode 5 hand spark    (looping crackle ball: Live Wire hand, strike anticipation; _Tint 2 = Infusion pickup, pale yellow core)
//  _Mode 6 warp target   (looping glyph over a cull-ready elite: a small down-pointing bolt, bobbing, ink outline)
float4 lz_fcol(float k, float tint)    // 2 = white, 1 = cyan, 0 = blue
{
    if (tint > 0.5 && k < 0.5) return float4(zramp(_RampTex, 2.0, 3.0), 1.0);
    return float4(zramp(_RampTex, 0.0, 2.0 + k), 1.0);
}
float4 ZFX_Frag(float2 p, float2 w)
{
    float pr = zsat(_Progress);
    float st = floor(_FxTime * _StepFps);
    float gl = 0.0;
    float4 c = float4(0.0, 0.0, 0.0, 0.0);
    float S = _Size;
    if (_Mode < 0.5)
    {   // ---- impact star: frame A white star, frame B cyan smaller, then 4-6 spark dashes fly out
        float sz = pr < 0.25 ? S : (pr < 0.5 ? S * 0.6 : 0.0);
        if (sz > 0.0)
        {
            float f = lz_star(p, sz, pr < 0.25 ? 1.0 : 0.5);
            if (f > 0.0) c = lz_fcol(pr < 0.25 ? (f > 0.45 ? 2.0 : 1.0) : (f > 0.5 ? 1.0 : 0.0), _Tint);
            gl = max(gl, zsat(1.0 - length(p * float2(1.0, 1.4)) / (sz * 1.3)) * (pr < 0.25 ? 1.0 : 0.6));
        }
        if (pr >= 0.25 && pr < 0.95)
        {
            for (int i = 0; i < 6; i++)
            {
                float fi = float(i);
                float h = zhash(float2(fi * 2.3, _Seed * 41.0));
                float a = (fi + h * 0.6) / 6.0 * ZTAU;
                float2 dir = float2(cos(a), sin(a) * 0.8 + 0.25);
                float tr = (pr - 0.25) / 0.7;
                float2 hd = dir * (S * 0.5 + S * 1.3 * zeaseOut(tr)) - float2(0.0, 6.0 * tr * tr);
                float d = zsegd(p, hd, hd - dir * 2.5);
                if (d <= 0.6 && h > 0.25) { c = lz_fcol(tr < 0.4 ? 1.0 : 0.0, _Tint); gl = max(gl, 0.4 * (1.0 - tr)); }
            }
        }
    }
    else if (_Mode < 2.5)
    {   // ---- teleport out (1) / in (2). Body column at the pivot (feet), height _Height
        float H = _Height; float bw = 6.0;
        float q = _Mode < 1.5 ? pr : 1.0 - pr;               // in = out reversed in shape, with its own drop-in
        bool inMode = _Mode > 1.5;
        float seed = lz_stepseed(_Seed, 1.0);
        // strands wrapping the body (phase q < 0.45): 3 jagged vertical strands that pull in to the centre line
        if ((!inMode && q < 0.5) || (inMode && pr >= 0.42))
        {
            float pull = inMode ? 1.0 - zsat((pr - 0.42) / 0.5) : zsat(q / 0.45);
            for (int i = 0; i < 3; i++)
            {
                float fi = float(i);
                float x = (fi - 1.0) * bw * (1.0 - pull) + (zhash(float2(fi, seed * 50.0)) - 0.5) * 2.0;
                float u;
                float d = lz_jag(p, float2(x * 0.6, 1.0), float2(x, H * (0.7 + 0.3 * zhash(float2(fi + 4.0, seed * 30.0)))), 5.0, 3.0 * (1.0 - pull) + 1.0, seed + fi * 0.3, u);
                if (d <= 0.6) { c = lz_fcol(pull > 0.6 ? 2.0 : 1.0, _Tint); gl = max(gl, 0.7); }
                else if (d <= 1.6 && pull > 0.4) { c = lz_fcol(0.0, _Tint); gl = max(gl, 0.4); }
            }
        }
        // the white column (q 0.45..0.75), 3 px, retracting upward (out) / dropping in from the sky (in)
        float colLo = 0.0; float colHi = H;
        bool col = false;
        if (!inMode && q >= 0.45) { col = true; colLo = H * zeaseIn((q - 0.45) / 0.5); colHi = H + 6.0 * zsat((q - 0.6) / 0.3); }
        if (inMode && pr < 0.25) { col = true; colLo = H * (1.0 - zeaseIn(pr / 0.15)) - 2.0; colHi = H + 10.0; }
        if (inMode && pr >= 0.25 && pr < 0.45) { col = true; colLo = 0.0; colHi = H * (1.0 - (pr - 0.25) / 0.2); }
        if (col && p.y >= colLo && p.y <= colHi)
        {
            float jx = floor((zhash(float2(floor(p.y / 4.0), seed * 20.0)) - 0.5) * 2.0 + 0.5);
            float ax = abs(p.x - jx);
            float thin = (!inMode && q > 0.8) || (inMode && pr > 0.35) ? 0.0 : 1.0;
            if (ax <= thin + 0.5) c = lz_fcol(2.0, _Tint);
            else if (ax <= thin + 1.5) c = lz_fcol(1.0, _Tint);
            gl = max(gl, zsat(1.0 - ax / 7.0));
        }
        // ground spikes: radial jagged spikes at the feet when the column touches the ground
        float gs = !inMode ? zsat(1.0 - abs(q - 0.5) / 0.15) : zsat(1.0 - abs(pr - 0.3) / 0.15);
        if (gs > 0.0)
        {
            float2 g = float2(p.x, p.y / 0.55);
            float ang = atan2(g.y, g.x); float rr = length(g);
            float bi = floor((ang / ZTAU + 0.5) * 9.0);
            float h = zhash(float2(bi, _Seed * 17.0 + floor(st / 2.0)));
            float ac = (bi + 0.5) / 9.0 * ZTAU - ZPI;
            float perp = abs(rr * sin(ang - ac)) - (zhash(float2(bi, floor(rr / 3.0))) - 0.5) * 1.6;
            float ln = (5.0 + 7.0 * h) * gs;
            if (h < 0.75 && abs(perp) < 0.7 && rr > 2.0 && rr < ln && cos(ang - ac) > 0.0) { c = lz_fcol(rr < ln * 0.5 ? 2.0 : 1.0, _Tint); gl = max(gl, 0.6); }
            gl = max(gl, zsat(1.0 - rr / 12.0) * gs * 0.8);
        }
        // a lingering scorch mark under the feet (teleport out leaves it, teleport in burns it): Lichtenberg X
        float sm = !inMode ? zsat((q - 0.45) * 4.0) : zsat((pr - 0.3) * 4.0);
        float2 g2 = float2(p.x, p.y / 0.55);
        if (sm > 0.0 && c.a < 0.5 && pr < 0.97)
        {
            float a2 = abs(abs(g2.x) - abs(g2.y)) ; float r2 = length(g2);
            if (a2 < 1.0 && r2 < 6.0 * sm && r2 > 1.0) c = float4(zramp(_RampTex, 3.0, 1.0), 1.0);
        }
    }
    else if (_Mode < 3.5)
    {   // ---- mirror dazzle (chest pivot). Rays: horizontal 2.2 S, vertical 1.2 S, diagonals 0.6 S. Light, not lightning.
        float s2 = pr < 0.15 ? zeaseOut(pr / 0.15) : 1.0 - zeaseIn((pr - 0.15) / 0.85);
        float ax = abs(p.x); float ay = abs(p.y);
        float th = 1.0 + 1.5 * s2;
        float rh = (ay <= th * (1.0 - ax / (S * 2.2 * s2 + 0.01))) ? 1.0 - ax / (S * 2.2 * s2 + 0.01) : 0.0;
        float rv = (ax <= th * (1.0 - ay / (S * 1.2 * s2 + 0.01))) ? 1.0 - ay / (S * 1.2 * s2 + 0.01) : 0.0;
        float rd = (abs(ax - ay) <= th * 0.7) ? 1.0 - (ax + ay) / (S * 1.0 * s2 + 0.01) : 0.0;
        float f = max(max(rh, rv), rd * 0.9);
        if (f > 0.0)
        {
            float idx = f > 0.7 ? 4.0 : (f > 0.4 ? 3.0 : 2.0);
            if (f > 0.88 && pr < 0.3) idx = 5.0;                 // pale yellow heart on the first frames
            c = float4(zramp(_RampTex, 5.0, idx), 1.0);
        }
        gl = max(gl, f * s2);
        gl = max(gl, zsat(1.0 - length(p * float2(0.7, 1.0)) / (S * 0.6)) * s2 * 0.8);
        // glints thrown outward: 8 short 3 px dashes
        if (pr > 0.08 && pr < 0.8)
        {
            for (int i = 0; i < 8; i++)
            {
                float fi = float(i); float h = zhash(float2(fi * 1.7, _Seed * 13.0));
                float a = (fi + 0.5 * h) / 8.0 * ZTAU;
                float2 dir = float2(cos(a), sin(a) * 0.7);
                float2 hd = dir * (S * 0.8 + S * 1.6 * zeaseOut((pr - 0.08) / 0.72));
                float d = zsegd(p, hd, hd - dir * 2.0);
                if (d <= 0.6) { c = float4(zramp(_RampTex, 5.0, pr < 0.45 ? 4.0 : 3.0), 1.0); gl = max(gl, 0.4); }
            }
        }
    }
    else if (_Mode < 4.5)
    {   // ---- bite: two zig-zag jaws close along _Dir (local x = dir), snap at pr 0.35, sparks after
        float2 dir = normalize(_Dir.xy + float2(0.0001, 0.0));
        float2 l = float2(dot(p, dir), dot(p, float2(-dir.y, dir.x)));
        float close = zeaseIn(zsat(pr / 0.35));
        float gap = 5.0 * (1.0 - close);
        float span = S;
        if (pr < 0.55 && l.x > -span * 0.5 && l.x < span * 0.6)
        {
            float tooth = abs(zmod(l.x + span, 4.0) - 2.0);      // zig-zag 4 px period
            float curve = (l.x / span) * (l.x / span) * 4.0;
            float yu = gap + curve - tooth * 0.8; float yd = -gap - curve + tooth * 0.8;
            float du = abs(l.y - yu); float dd = abs(l.y - yd);
            float d = min(du, dd);
            if (d <= 0.7) { c = lz_fcol(pr > 0.3 ? 2.0 : 1.0, _Tint); gl = max(gl, 0.8); }
            else if (d <= 1.7 && pr > 0.25) { c = lz_fcol(0.0, _Tint); gl = max(gl, 0.4); }
        }
        if (pr >= 0.35 && pr < 0.6)
        {
            float f = lz_star(l - float2(span * 0.1, 0.0), 5.0, 0.7);
            if (f > 0.0) c = lz_fcol(f > 0.5 ? 2.0 : 1.0, _Tint);
            gl = max(gl, zsat(1.0 - length(l) / 9.0));
        }
        if (pr >= 0.4)
        {
            for (int i = 0; i < 5; i++)
            {
                float fi = float(i); float h = zhash(float2(fi * 3.1, _Seed * 7.0));
                float a = (h - 0.5) * 2.4;
                float2 sd = zrot(dir, a);
                float tr = (pr - 0.4) / 0.6;
                float2 hd = sd * (3.0 + 12.0 * zeaseOut(tr)) - float2(0.0, 5.0 * tr * tr);
                float d = zsegd(p, hd, hd - sd * 2.5);
                if (d <= 0.6) { c = lz_fcol(tr < 0.5 ? 1.0 : 0.0, _Tint); gl = max(gl, 0.4 * (1.0 - tr)); }
            }
        }
    }
    else if (_Mode > 5.5)
    {   // ---- warp target glyph: 5 x 9 zig-zag bolt pointing down + 1 px ink outline, flashes white every 6 steps
        float2 q = p - float2(0.0, floor(sin(_FxTime * 6.0) * 1.0 + 0.5));
        float zz = (q.y > 1.0) ? (q.x - (q.y - 1.0) * 0.5 + 1.0) : (q.x + (q.y + 3.0) * 0.5 - 1.5);
        bool inB = abs(zz) <= 1.0 && q.y >= -4.5 && q.y <= 4.5;
        bool edge = !inB && abs(zz) <= 2.0 && q.y >= -5.5 && q.y <= 5.5;
        bool flash = zmod(st, 6.0) < 1.0;
        if (inB) c = flash ? float4(zramp(_RampTex, 0.0, 4.0), 1.0) : float4(zramp(_RampTex, 5.0, 5.0), 1.0);
        else if (edge) c = float4(zramp(_RampTex, 7.0, 0.0), 1.0);
        gl = max(gl, zsat(1.0 - length(q * float2(1.0, 0.8)) / 5.0) * (flash ? 1.0 : 0.6));
    }
    else
    {   // ---- hand spark loop: crackle ball (3 jagged arcs re-struck every step) + 2 px core
        float seed = lz_stepseed(_Seed, 1.0);
        float rr = length(p);
        if (rr <= 1.2) c = (_Tint > 1.5) ? float4(zramp(_RampTex, 5.0, rr < 0.6 ? 4.0 : 5.0), 1.0) : lz_fcol(2.0, _Tint);
        if (_Tint > 1.5 && rr > 1.2 && rr <= 2.2) c = float4(zramp(_RampTex, 5.0, 5.0), 1.0);
        for (int i = 0; i < 3; i++)
        {
            float fi = float(i); float h = zhash(float2(fi * 4.1, seed * 37.0));
            float a = h * ZTAU;
            float u;
            float d = lz_jag(p, float2(0.0, 0.0), float2(cos(a), sin(a)) * (S * (0.6 + 0.4 * frac(h * 5.0))), 3.0, 1.6, seed + fi * 0.2, u);
            if (d <= 0.55 && c.a < 0.5) { c = lz_fcol(u < 0.45 ? 2.0 : 1.0, _Tint > 1.5 ? 0.0 : _Tint); }
        }
        gl = max(gl, zsat(1.0 - rr / (S * 1.0)) * 0.8);
    }
    if (_Layer > 0.5) return zglow(_RampTex, gl, w, _GlowGain);
    return c;
}
