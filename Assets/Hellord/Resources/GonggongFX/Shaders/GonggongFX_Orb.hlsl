// Gonggong Orb (t1682): Eye of the Black Waters (a floating black-water eye with a spiral of water arms and orbiting shards) and the
// Drowning Mark relay link (a sinuous water ribbon). Over sprites. Colours: row 2 deep, row 1 water, row 0 ice, row 5 drown.
float4 ZFX_Frag(float2 p, float2 w)
{
    float t = gg_t();
    float hot = zsat(_Hot);
    float pr = zsat(_Progress);
    float row = -1.0, idx = 0.0; float gl = 0.0;
    if (_Mode < 0.5)
    {   // 0 EYE ORB. Pivot = the orb's ground point; the orb floats 12 px above it with a shadow below.
        float form = zeaseOut(zsat(pr / 0.08));
        float endk = zsat((pr - 0.92) / 0.08);
        float S = _Size * form * (1.0 - 0.5 * endk);
        float2 oc = float2(0.0, 12.0 + sin(_FxTime * 6.0) * 1.0);
        oc = floor(oc + float2(0.5, 0.5));
        float2 q = p - oc;
        float r = length(q);
        float spin = _FxTime * 5.0;
        // shadow on the ground
        float sh = length(float2(p.x / (S * 0.9 + 1.0), p.y / (S * 0.35 + 1.0)));
        if (sh < 1.0 && zdith(0.75 - endk, p)) { row = 2.0; idx = 0.0; }
        // spiral water arms (3) around the orb, out to 2.2 S, turning: the visual of the spiral shard spray
        float a = atan2(q.y, q.x * 0.8);
        float rr = r / max(S, 1.0);
        if (rr > 1.0 && rr < 2.3)
        {
            float s = a * 3.0 / 6.2831853 - log(rr) * 1.4 - spin * 0.5;
            float arm = abs(frac(s) - 0.5) * 2.0;
            float wdt = 0.24 * (2.3 - rr) / 1.3;
            if (arm < wdt) { row = 1.0; idx = rr < 1.5 ? 4.0 : 3.0; if (arm < wdt * 0.35 && rr < 1.6) idx = gg_cap(6.0, 6.0, hot); }
            else if (arm < wdt + 0.07) { row = 1.0; idx = 0.0; }
            if (endk > 0.0 && !zdith(1.0 - endk * 1.2, p)) row = -1.0;
            gl = arm < wdt ? 0.5 * (2.3 - rr) : 0.0;
        }
        // orb body
        if (r <= S + 1.0)
        {
            if (r > S) { row = 2.0; idx = 0.0; }
            else
            {
                float2 sw = zrot(q, spin * 0.6);
                float f = zN(_NoiseTex, sw / 8.0 + float2(_Seed, 0.0)).r;
                row = 2.0; idx = 1.0 + zband(f * 0.7 + (1.0 - r / S) * 0.1, 4.0, p, 0.6);   // 262b44 .. 124e89 churning
                float rim = dot(q / max(r, 0.01), float2(-0.6, 0.8));
                if (r > S - 1.6 && rim > 0.2) { row = 1.0; idx = 4.0; }                   // lit rim (cyan)
                // the eye: cyan iris ring + black slit pupil that blinks every 1.3 s
                float blink = zmod(_FxTime, 1.3) < 0.1 ? 0.2 : 1.0;
                float2 e = float2(q.x, q.y / blink);
                float er = length(float2(e.x * 1.0, e.y * 0.8));
                if (er < S * 0.55 && er > S * 0.55 - 1.6) { row = 1.0; idx = 4.0; }
                if (abs(e.x) < 1.0 && abs(e.y) < S * 0.45) { row = 1.0; idx = 0.0; }
                if (q.x > -S * 0.6 && q.x < -S * 0.15 && q.y > S * 0.35 && q.y < S * 0.7) { row = 0.0; idx = gg_cap(5.0, 5.0, hot); }   // 3x2 specular
                if (endk > 0.0 && !zdith(1.0 - endk, p)) row = -1.0;
            }
            gl = max(gl, 0.4 + 0.4 * _Pulse);
        }
        // 4 shards orbiting on the spiral emitters (each a 3x5 crystal glint), brighter on _Pulse
        for (int i = 0; i < 4; i++)
        {
            float fi = float(i);
            float aa = spin * 1.6 + fi * 1.5708;
            float rad = S * (1.5 + 0.25 * sin(_FxTime * 3.0 + fi));
            float2 cp = floor(oc + float2(cos(aa) * rad, sin(aa) * rad * 0.8) + float2(0.5, 0.5));
            float f = gg_chip(p - cp, 1.6);
            if (f > -0.5 && endk < 0.5) { row = 0.0; idx = gg_facetidx(f, hot * (0.5 + _Pulse)); }
        }
        if (_Layer > 0.5) gl = max(gl, zsat(1.0 - (r - S) / (S * 1.2)) * 0.5);
    }
    else
    {   // 1 RELAY LINK (Drowning Mark): a 3 px water ribbon from the pivot (marked enemy chest) to _TargetPx (chilled enemy chest)
        float2 T = _TargetPx.xy; float L = max(length(T), 1.0); float2 td = T / L; float2 tn = float2(-td.y, td.x);
        float u = dot(p, td), v = dot(p, tn);
        float head = L * zeaseOut(zsat(pr / 0.3));
        float tail = L * zeaseIn(zsat((pr - 0.6) / 0.4));
        float uc = zsat(u / L);
        float wv = sin(uc * 9.0 - _FxTime * 14.0 + _Seed * 6.0) * 3.0 * sin(uc * 3.1416);
        float dv = abs(v - wv);
        if (u > tail && u < head)
        {
            if (dv < 1.0) { row = _Tint > 0.5 ? 5.0 : 1.0; idx = (zmod(floor(u / 3.0 - _FxTime * 15.0), 4.0) < 1.0) ? gg_cap(6.0, 6.0, hot) : (_Tint > 0.5 ? 5.0 : 4.0); }
            else if (dv < 2.0) { row = _Tint > 0.5 ? 5.0 : 1.0; idx = 3.0; }
            else if (dv < 3.0) { row = 1.0; idx = 0.0; }
            gl = dv < 3.0 ? 0.55 : 0.0;
        }
        // head droplet burst (3 drops) when it reaches the target
        float2 hq = p - td * head;
        if (row < -0.5 && pr > 0.25 && pr < 0.55 && length(hq) < 3.0) { row = 1.0; idx = length(hq) < 1.5 ? 5.0 : 3.0; }
        if (_Layer > 0.5) gl = (u > tail && u < head) ? zsat(1.0 - dv / 6.0) * 0.6 : 0.0;
    }
    if (_Layer > 0.5)
    {
        float g2 = min(gl, _GlowCap);
        return g2 > 0.02 ? zglowT(_RampTex, g2, p, _GlowGain, _Tint) : gg_none();
    }
    return row < -0.5 ? gg_none() : float4(zramp(_RampTex, row, idx), 1.0);
}
