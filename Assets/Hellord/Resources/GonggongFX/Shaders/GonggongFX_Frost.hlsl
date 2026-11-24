// Gonggong Frost (t1682): frost and ice ON the ground (ground tier, under sprites, except mode 3 which is a screen overlay).
// Areas are shown with material, never a flat disc / ring: dendritic frost creeping along voronoi crack veins, frosted plates,
// ice crystal teeth at the leading edge, polished ice sheets with streak reflections and cracks, rime fringes. 15 fps stepped.
// Colours: row 3 frost (0 262b44, 1 3a4466, 2 5a6988, 3 8b9bb4, 4 c0cbdc, 5 white), row 0 ice (2 blue, 3 cyan, 4 pale, 5 white).

// tiny ice tooth at a frost edge: 1x3 px spike pointing up-screen from q (returns facet or -1)
float gf_tooth(float2 q, float h)
{
    float res = -1.0;
    float2 a = floor(q + float2(0.5, 0.5));
    if (a.y >= 0.0 && a.y < h && abs(a.x) <= (a.y < h - 1.0 ? 1.0 : 0.0)) res = (a.x < 0.0) ? 4.0 : (a.x > 0.0 ? 2.0 : 3.0);
    if (res < -0.5 && a.y >= -1.0 && a.y <= h && abs(a.x) <= 2.0 && abs(a.x) + max(a.y - h + 2.0, 0.0) <= 2.0) res = 0.0;
    return res;
}
// frost material colour index (row 3 / row 0 mix) from the coverage class and a glint test. Returns float2(row, idx), row < 0 = none.
float2 gf_frostcol(float cov, float2 p, float t, float hot, float style)
{
    float2 o = float2(-1.0, 0.0);
    if (cov > 1.5) o = float2(0.0, gg_cap(zN(_NoiseTex, p / 5.0 + float2(t * 0.05, 0.3)).r > 0.62 ? 5.0 : 4.0, 5.0, hot));   // vein: pale / white
    else if (cov > 0.5)
    {
        float n = zN(_NoiseTex, p / 4.0 + float2(0.7, _Seed)).g;
        float cb = zN(_NoiseTex, p / 21.0 + float2(_Seed * 3.7, _Seed * 1.9)).b;          // the plate's cell (same lookup as gg_frostcover)
        float sh = cb + (n - 0.5) * 0.25;
        o = float2(3.0, sh > 0.6 ? 3.0 : (sh > 0.3 ? 2.0 : 1.0));                          // frosted plates, one shade per cell
        if (style > 0.5) o = float2(sh > 0.78 ? 5.0 : 3.0, sh > 0.78 ? 2.0 : (sh > 0.4 ? 2.0 : 1.0));   // curse: slate plates, a few violet
        if (n < 0.27) o = float2(-1.0, 0.0);                                                 // organic holes: the floor shows through the rime
        float gl = zhash(floor(p / 2.0) + float2(floor(t * 5.0), _Seed * 31.0));
        if (gl > 0.985) o = float2(0.0, gg_cap(5.0, 5.0, hot));                               // twinkle glints
    }
    return o;
}
float4 gf_rc(float2 rc) { return rc.x < -0.5 ? gg_none() : float4(zramp(_RampTex, rc.x, rc.y), 1.0); }

float4 ZFX_Frag(float2 p, float2 w)
{
    float t = gg_t();
    float hot = zsat(_Hot);
    float4 c = gg_none(); float gl = 0.0;
    float md = _Mode;
    float2 rc = float2(-1.0, 0.0);
    if (md < 0.5)
    {   // 0 ZONE (Hypothermia): delay = frost creeps INWARD from the rim with ice teeth on its front (telegraph under sprites);
        // _Ignite 1 = curse lands: frost fills + glints; then it lingers and thaws with _Fade.
        float2 g = float2(p.x, p.y / _Squash);
        float rr = length(g) / max(_Radius, 1.0);
        float ig = zsat(_Ignite);
        float snap = zsat((ig - 0.97) / 0.03);
        float nz = zN(_NoiseTex, g / 14.0 + float2(_Seed, 0.0)).r;
        float edge = 1.0 + (nz - 0.5) * 0.18;
        if (rr < edge)
        {
            float seedv = (edge - rr) / edge;                                 // 0 at the rim -> creeps inward
            float cov = gg_frostcover(_NoiseTex, p, seedv, ig * 0.9 + snap * 0.4, _Seed);
            if (_Fade > 0.0 && !zdith(1.0 - _Fade * 1.1, p + float2(0.0, t * 7.0))) cov = 0.0;
            rc = gf_frostcol(cov, p, t, hot, _Style);
            if (rr > edge - 0.07 && cov < 0.5 && zN(_NoiseTex, g / 4.0).g > 0.35 - ig * 0.2) rc = float2(3.0, 1.0);   // rime fringe at the rim
            gl = cov > 1.5 ? 0.45 * (0.5 + snap) : 0.0;
        }
        // ice teeth riding the inward creep front (12 around)
        if (snap < 0.5 && _Fade < 0.3)
        {
            float a = atan2(g.y, g.x); float k = floor((a + 3.1416) / 6.2832 * 12.0);
            float ak = (k + 0.5) / 12.0 * 6.2832 - 3.1416 + (zhash1(k + _Seed * 7.0) - 0.5) * 0.3;
            float fr = edge * (1.0 - 0.75 * ig) - 0.04;
            float2 tp = floor(float2(cos(ak), sin(ak) * _Squash) * fr * _Radius + float2(0.5, 0.5));
            float f = gf_tooth(p - tp, 3.0 + floor(zhash1(k * 3.1) * 2.0));
            if (f > -0.5 && ig > 0.05) rc = float2(0.0, gg_facetidx(f, hot));
            if (f > 0.5) gl = 0.5;
        }
    }
    else if (md < 1.5)
    {   // 1 FROZEN STRIP (Frozen River): polished ice sheet along pivot -> _TargetPx, half width _Width; freeze-in runs along the path
        float2 T = _TargetPx.xy; float L = max(length(T), 1.0); float2 td = T / L; float2 tn = float2(-td.y, td.x);
        float u = dot(p, td), v = dot(p, tn);
        float nz = zN(_NoiseTex, p / 10.0 + float2(_Seed, 0.0)).r;
        float hw = _Width + (nz - 0.5) * 5.0;
        float front = (L + 12.0) * zsat(_Ignite);
        float uu = u + _Width * 0.6;
        float rr = length(float2(max(-u, max(u - L, 0.0)), v)) ;                 // rounded ends
        if (rr < hw && uu < front)
        {
            float edgeK = hw - rr;
            float idxRow = 0.0, idx = 4.0;
            // ice: pale base, blue depth patches, diagonal white streak reflections (stepped 45 deg bands), cracks from voronoi edges
            float4 n4 = zN(_NoiseTex, p / 17.0 + float2(_Seed * 2.0, 0.4));
            idx = n4.r > 0.58 ? 3.0 : 4.0;
            if (n4.g < 0.3) idx = 2.0;
            float st = zmod(floor((p.x + p.y) / 3.0) + floor(t * 2.0), 11.0);
            if (st < 1.0 && n4.r > 0.4) idx = gg_cap(5.0, 5.0, hot);              // streak glint
            if (n4.a < 0.06) { idxRow = 0.0; idx = 1.0; }                         // crack line (deep blue)
            if (edgeK < 2.2) { idxRow = 3.0; idx = (zN(_NoiseTex, p / 3.0).g > 0.5) ? 4.0 : 3.0; }   // rime fringe
            if (edgeK < 0.8) { idxRow = 3.0; idx = 0.0; }
            if (front - uu < 3.0) { idxRow = 0.0; idx = gg_cap(5.0, 5.0, hot); }  // freezing edge
            if (_Fade > 0.0)
            {   // thaw: ice breaks into plates, then dark wet patches, then gone
                float th = _Fade * 1.3 - n4.b * 0.4;
                if (th > 0.25) { idxRow = 1.0; idx = (n4.a < 0.2) ? 2.0 : 1.0; }
                if (!zdith(1.0 - _Fade * 1.2, p)) idx = -1.0;
            }
            if (idx > -0.5) rc = float2(idxRow, idx);
            gl = (idx > 4.5 || (front - uu < 3.0)) ? 0.5 : (idx > 3.5 ? 0.18 : 0.0);
        }
    }
    else if (md < 2.5)
    {   // 2 FISSURE (Buzhou Rift): a jagged ice crack opens from the pivot to _TargetPx as _Progress reveals it; glowing cyan core,
        // white-hot head, frost spreading 3-6 px either side, shards standing at the lips. _Fade closes it.
        float2 T = _TargetPx.xy; float L = max(length(T), 1.0); float2 td = T / L; float2 tn = float2(-td.y, td.x);
        float u = dot(p, td), v = dot(p, tn);
        float head = L * zsat(_Progress);
        float k = u / 9.0; float k0 = floor(k);
        float o0 = (zhash(float2(k0, _Seed * 13.0)) - 0.5) * 7.0, o1 = (zhash(float2(k0 + 1.0, _Seed * 13.0)) - 0.5) * 7.0;
        float cv = lerp(o0, o1, frac(k)) * zsat(u / 8.0) * zsat((L - u) / 8.0);
        float dv = abs(v - cv);
        float open = zsat(_Progress * 3.0) * (1.0 - zsat(_Fade));
        if (u > -3.0 && u < head + 2.0)
        {
            float age = (head - u) / 30.0;
            float wcore = 1.2 * open * (1.0 - 0.3 * zsat(age));
            float idxRow = -1.0, idx = 0.0;
            if (dv <= wcore + 0.5) { idxRow = 0.0; idx = age < 0.15 ? gg_cap(5.0, 5.0, hot) : 3.0; }
            else if (dv <= wcore + 1.5) { idxRow = 0.0; idx = 0.0; }
            else
            {
                float cov = gg_frostcover(_NoiseTex, p, dv / (10.0 * open + 0.01), 0.9 * open, _Seed);
                float2 fc = gf_frostcol(cov, p, t, hot, 0.0);
                idxRow = fc.x; idx = fc.y;
            }
            if (u > head - 1.0 && dv < 3.0) { idxRow = 0.0; idx = gg_cap(5.0, 5.0, hot); }   // head
            if (idxRow > -0.5) rc = float2(idxRow, idx);
            gl = dv < wcore + 2.0 ? (age < 0.3 ? 0.8 : 0.45) * open : 0.0;
        }
    }
    else if (md < 3.5)
    {   // 3 SCREEN FROST (Eternal Winter): frost creeps in from the view edges (pivot = view centre, _Radius = half width 320),
        // + a light snowfall of <= 14 flakes on screen. Over layer.
        float hx = _Radius, hy = _Radius * 0.5625;
        float ex = 1.0 - abs(p.x) / hx, ey = 1.0 - abs(p.y) / hy;
        float e = min(ex * 1.78, ey);                                            // 0 at the screen edge
        float ig = zsat(_Ignite) * (1.0 - zsat(_Fade)) * _Intensity;
        float seedv = e / 0.16;
        float cov = gg_frostcover(_NoiseTex, p, seedv, ig, _Seed);
        if (cov > 0.5 && e < 0.2) rc = gf_frostcol(cov, p, t, hot, 0.0);
        if (rc.x > -0.5 && cov < 1.5 && !zdith(1.2 - e * 4.0, p)) rc = float2(-1.0, 0.0);     // plates thin out inward
        // snowflakes: 14 slow flakes falling diagonally, 3 px plus shape with a white centre
        for (int i = 0; i < 14; i++)
        {
            float fi = float(i);
            float sp = 9.0 + zhash1(fi * 1.7) * 7.0;
            float2 fpos = float2(zhash1(fi * 3.3 + _Seed) * 2.0 - 1.0, 1.0 - frac(zhash1(fi * 5.9) + _FxTime * sp / (2.0 * hy) )* 2.0);
            float2 fp = floor(float2(fpos.x * hx + sin(_FxTime * 1.3 + fi) * 6.0 + fpos.y * 20.0, fpos.y * hy) + float2(0.5, 0.5));
            float2 q = p - fp;
            if (ig > 0.2 && (abs(q.x) + abs(q.y) <= 1.0)) rc = float2(3.0, (q.x == 0.0 && q.y == 0.0) ? gg_cap(5.0, 5.0, hot) : 4.0);
        }
        gl = 0.0;
    }
    else if (md < 4.5)
    {   // 4 LINE FROST (telegraph / wall base): frost along pivot -> _TargetPx (curved by _Bend), half width _Width, builds with _Ignite.
        float2 T = _TargetPx.xy; float L = max(length(T), 1.0); float2 td = T / L; float2 tn = float2(-td.y, td.x);
        float u = dot(p, td), v = dot(p, tn);
        float uc = zsat(u / L);
        float bend = _Bend * 4.0 * uc * (1.0 - uc);
        float dv = abs(v - bend);
        float ig = zsat(_Ignite);
        float rr = length(float2(max(-u, max(u - L, 0.0)), dv));
        float hw = _Width * (0.4 + 0.6 * ig);
        if (rr < hw + 3.0)
        {
            float seedv = rr / (hw + 3.0);
            float cov = gg_frostcover(_NoiseTex, p, seedv, ig * (1.0 - zsat(_Fade)), _Seed);
            rc = gf_frostcol(cov, p, t, hot, _Style);
            // crawling ice teeth along the line every 8 px, rising with _Ignite (the warning reads as a line of growing crystals)
            float kk = floor(u / 8.0); float uk = (kk + 0.5) * 8.0;
            if (uk > 0.0 && uk < L)
            {
                float vk = _Bend * 4.0 * (uk / L) * (1.0 - uk / L) + (zhash1(kk + _Seed * 5.0) - 0.5) * hw;
                float2 tp = floor(td * uk + tn * vk + float2(0.5, 0.5));
                float f = gf_tooth(p - tp, 1.0 + floor(3.0 * ig * (1.0 - zsat(_Fade))));
                if (f > -0.5 && ig > 0.15) rc = float2(0.0, gg_facetidx(f, hot));
            }
            gl = (cov > 1.5) ? 0.35 * ig : 0.0;
        }
    }
    else if (md < 5.5)
    {   // 5 RIME PULSE (Eternal Winter, once per second): a frost front races out from the pivot to _Radius over _Progress, leaving
        // frost behind that fades; the front itself is irregular crystal teeth + vein flashes, not a drawn ring.
        float2 g = float2(p.x, p.y / _Squash);
        float rr = length(g);
        float pr = zsat(_Progress);
        float fr = _Radius * zeaseOut(pr);
        float nz = zN(_NoiseTex, g / 18.0 + float2(_Seed, 0.0)).r;
        float fre = fr * (1.0 + (nz - 0.5) * 0.25);
        if (rr < fre)
        {
            float behind = (fre - rr) / 26.0;
            float cov = gg_frostcover(_NoiseTex, p, behind * 0.5, 1.0 - behind * 0.45 - pr * 0.5, _Seed + floor(pr * 3.0) * 0.1);
            if (behind < 0.12 && zN(_NoiseTex, g / 3.0 + float2(pr, 0.0)).g > 0.45) cov = 2.0;      // the front: bright broken veins
            if (!zdith(1.0 - behind * 0.8 - pr * 0.4, p)) cov = 0.0;
            rc = gf_frostcol(cov, p, t, hot, 0.0);
            gl = (behind < 0.15 && cov > 1.5) ? 0.4 * (1.0 - pr) : 0.0;
        }
    }
    else if (md < 6.5)
    {   // 6 SQUARE FIELD (Pillars of Ao): an ice ridge binds the 4 pillars (corners at +-_Radius); light frost inside.
        float hx = _Radius, hy = _Radius;
        float2 a = abs(p);
        float ig = zsat(_Ignite) * (1.0 - zsat(_Fade));
        float dEdge = min(abs(a.x - hx) + max(a.y - hy, 0.0), abs(a.y - hy) + max(a.x - hx, 0.0));
        float nz = zN(_NoiseTex, p / 6.0 + float2(_Seed, t * 0.1)).r;
        float ridge = 1.6 + nz * 1.4;
        if (dEdge < ridge * ig + 0.01)
        {
            float f = (p.x + p.y < 0.0) ? 2.0 : 3.0;
            float idx = dEdge < 0.8 ? gg_cap(5.0, 5.0, hot) : (dEdge < ridge - 0.8 ? 4.0 : 0.0);
            if (zmod(floor((a.x + a.y) / 4.0) + floor(t * 4.0), 7.0) < 1.0 && idx > 3.5) idx = 3.0;   // energy crawling along the ridge
            rc = float2(0.0, idx);
            gl = idx > 3.5 ? 0.45 : 0.0;
        }
        else if (a.x < hx && a.y < hy)
        {
            float cov = gg_frostcover(_NoiseTex, p, 0.6 + 0.4 * (1.0 - max(a.x / hx, a.y / hy)), ig * 0.75, _Seed);
            if (cov > 0.5) rc = gf_frostcol(cov, p, t, hot, 0.0);
        }
    }
    else
    {   // 7 ICE DECAL (shatter / shard landing): a frozen splat with crystal spikes rim, fading with _Fade
        float2 g = float2(p.x, p.y / _Squash);
        float rr = length(g) / max(_Radius, 1.0);
        float a = atan2(g.y, g.x);
        float spikes = 0.75 + 0.3 * abs(sin(a * 3.5 + _Seed * 20.0)) + (zN(_NoiseTex, g / 6.0 + float2(_Seed, 0.0)).r - 0.5) * 0.3;
        if (rr < spikes)
        {
            float cov = gg_frostcover(_NoiseTex, p, rr * 0.7, 1.0 - zsat(_Fade) * 1.2, _Seed);
            rc = gf_frostcol(max(cov, rr < 0.35 ? 1.0 : 0.0), p, t, hot, 0.0);
            if (rr < 0.3 && zN(_NoiseTex, p / 3.0).g > 0.5) rc = float2(0.0, 3.0);
            if (rr > spikes - 0.1 && rc.x > -0.5) rc = float2(0.0, 0.0);
            if (!zdith(1.0 - zsat(_Fade) * 1.1, p)) rc = float2(-1.0, 0.0);
        }
    }
    c = gf_rc(rc);
    if (_Layer > 0.5)
    {
        float g2 = min(gl, _GlowCap);
        return (g2 > 0.02 && c.a > 0.5) ? zglowT(_RampTex, g2, p, _GlowGain, _Style) : gg_none();
    }
    return c;
}
