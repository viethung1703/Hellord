// Gonggong Frost (t1682, v2 2026-11-25): frost and ice ON the ground (ground tier, under sprites; v2: mode 3 is a ground layer now, no screen overlay).
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
    {   // 0 ZONE (Hypothermia, v2): the area CRYSTALLISES into solid ice. Faceted ice crystals sprout from the rim inward over the delay
        // (_Ignite 0..1; taller toward the centre, leaning out), while a sheet of faceted ice plates (one shade per facet, bright facet
        // edges, holes) closes in behind them. _Ignite 1 = the curse lands: crystal tips flash (capped), then it thaws / sinks with _Fade.
        float2 g = float2(p.x, p.y / _Squash);
        float rr = length(g) / max(_Radius, 1.0);
        float ig = zsat(_Ignite); float fd = zsat(_Fade);
        float snap = zsat((ig - 0.94) / 0.06) * (1.0 - zsat(fd * 3.0));
        float nz = zN(_NoiseTex, g / 14.0 + float2(_Seed, 0.0)).r;
        float edge = 1.0 + (nz - 0.5) * 0.16;
        if (rr < edge)
        {   // ice sheet plates
            float4 cn = zN(_NoiseTex, p / 13.0 + float2(_Seed * 3.7, 1.9));
            float grow = ig * 1.35 - (edge - rr) * 0.75 - cn.b * 0.3;
            if (grow > 0.0 && zclump(_NoiseTex, 0.62 - fd * 0.9 - zsat((rr - 0.6) / 0.4) * 0.35, p, _Seed))
            {
                float sh = cn.b + (zN(_NoiseTex, p / 4.0 + float2(0.7, _Seed)).g - 0.5) * 0.2;
                rc = float2(0.0, sh > 0.72 ? 3.0 : (sh > 0.36 ? 2.0 : 1.0));
                if (_Style > 0.5 && sh < 0.12) rc = float2(5.0, 2.0);                                  // curse: a few violet facets
                if (cn.a < 0.07 + 0.04 * snap) rc = float2(0.0, gg_cap(snap > 0.5 ? 5.0 : 4.0, 5.0, hot));    // bright facet edges
                if (grow < 0.12 && cn.a > 0.07) rc = float2(0.0, 1.0);                                  // the freezing front: deep ice
                gl = cn.a < 0.07 ? 0.25 + 0.4 * snap : 0.0;
            }
        }
        // crystals (12 px cells, ~50 % used): big faceted prisms sprout rim -> centre, nearest base drawn in front
        float2 cg = floor(p / 12.0);
        float best = 999.0;
        for (int j = 0; j < 12; j++)
        {
            float2 cc = cg + float2(zmod(float(j), 3.0) - 1.0, floor(float(j) / 3.0) - 2.0);
            float h1 = zhash(cc + float2(_Seed * 13.0, 5.3)), h2 = zhash(cc * 1.3 + float2(2.1, _Seed * 9.0));
            float2 b = floor(cc * 12.0 + float2(1.0 + 10.0 * h1, 1.0 + 10.0 * h2));
            float rb = length(float2(b.x, b.y / _Squash)) / max(_Radius, 1.0);
            if (rb < 0.92 && zhash(cc * 2.7 + float2(_Seed, 0.4)) > 0.45)
            {
                float tb = 0.04 + 0.5 * (1.0 - rb) + h2 * 0.18;
                float gr = zeaseOut(zsat((ig - tb) / 0.28));
                float hh = (6.0 + 10.0 * h2) * (0.55 + 0.45 * (1.0 - rb)) * gr * (1.0 - fd);
                if (rb > 0.75) hh = min(hh, 8.0);
                if (hh >= 2.0)
                {
                    float f = gg_crystal(p - b, hh, 2.0 + h1 * 1.6, (b.x / max(_Radius, 1.0)) * 5.0 + (h2 - 0.5) * 2.0);
                    if (f > -0.5 && b.y < best)
                    {
                        best = b.y; rc = float2(0.0, gg_facetidx(f, hot));
                        if (snap > 0.5 && f > 2.5) rc = float2(0.0, gg_cap(5.0, 5.0, hot));               // the lock flash on the tips
                        gl = f > 2.5 ? 0.35 + 0.45 * snap : 0.1;
                    }
                }
            }
        }
        if (fd > 0.0 && rc.x > -0.5 && !zdith(1.0 - fd * 1.1, p + float2(0.0, t * 7.0))) rc = float2(-1.0, 0.0);
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
    {   // 3 WINTER GROUND (Eternal Winter, v2; was a screen overlay in v1 - dropped): frost and snow take the FLOOR around the hero out to
        // _Radius (ground tier). _Ignite 0..1 = the frost front creeps out (crystal teeth on it); behind it: snow drifts (white tops, slate
        // shadow underside), frosted plates with holes, glossy ice puddles, twinkles. Thins toward the edge, thaws with _Fade.
        float2 g = float2(p.x, p.y / _Squash);
        float rr = length(g) / max(_Radius, 1.0);
        float ig = zsat(_Ignite) * _Intensity; float fd = zsat(_Fade);
        float nz = zN(_NoiseTex, g / 30.0 + float2(_Seed, 0.4)).r;
        float nz2 = zN(_NoiseTex, g / 9.0 + float2(0.5, _Seed)).g;
        float front = ig * (1.1 + (nz - 0.5) * 0.45 + (nz2 - 0.5) * 0.15);
        if (rr < front)
        {
            float edgeK = zsat((rr - 0.55) / 0.5);                                     // thins out toward the edge
            float behind = (front - rr) * max(_Radius, 1.0);                           // px behind the creeping front
            // macro patchiness (tiles every ~300 px, two layers) so the floor still shows between frost fields
            float Mn = zN(_NoiseTex, float2(p.x + p.y * 0.4, p.y - p.x * 0.3) / 75.0 + float2(_Seed, 0.2)).r * 0.6 + zN(_NoiseTex, p / 37.0 + float2(0.6, _Seed * 2.0)).g * 0.4;
            float Mk = Mn - edgeK * 0.22 + (1.0 - rr) * 0.12;
            // snow fields: clean soft shapes (pale body, white lit top edge, slate shadow under the lower edge, rare sparkles);
            // a frost fringe of bright crack-veins rims each field; a few glossy ice puddles. Floor shows between the fields.
            float fe = zN(_NoiseTex, p / 9.0 + float2(_Seed * 2.0, 0.0)).r;
            float F0 = Mk + (fe - 0.5) * 0.12;
            float Fu = zN(_NoiseTex, float2((p.x + p.y * 0.4) + 0.4, (p.y + 1.0) - p.x * 0.3) / 75.0 + float2(_Seed, 0.2)).r * 0.6 + zN(_NoiseTex, (p + float2(0.0, 1.0)) / 37.0 + float2(0.6, _Seed * 2.0)).g * 0.4 - edgeK * 0.22 + (1.0 - rr) * 0.12 + (zN(_NoiseTex, (p + float2(0.0, 1.0)) / 9.0 + float2(_Seed * 2.0, 0.0)).r - 0.5) * 0.12;
            float Fd = zN(_NoiseTex, float2((p.x + p.y * 0.4) - 0.8, (p.y - 2.0) - p.x * 0.3) / 75.0 + float2(_Seed, 0.2)).r * 0.6 + zN(_NoiseTex, (p - float2(0.0, 2.0)) / 37.0 + float2(0.6, _Seed * 2.0)).g * 0.4 - edgeK * 0.22 + (1.0 - rr) * 0.12 + (zN(_NoiseTex, (p - float2(0.0, 2.0)) / 9.0 + float2(_Seed * 2.0, 0.0)).r - 0.5) * 0.12;
            float thS = 0.6;
            float4 cn = zN(_NoiseTex, p / 19.0 + float2(_Seed * 3.1, 0.9));
            if (F0 > thS)
            {
                rc = float2(3.0, 4.0);
                if (Fu <= thS) rc = float2(3.0, gg_cap(5.0, 5.0, hot));                                // lit upper edge
                else if (Fd <= thS) rc = float2(3.0, 2.0);                                             // shadowed lower edge
                else if (zN(_NoiseTex, p / 3.0 + float2(0.2, _Seed)).g > 0.72) rc = float2(3.0, 3.0);  // soft texture
                if (cn.b > 0.93 && F0 > thS + 0.04) rc = float2(0.0, (abs(frac((p.x + p.y) / 9.0) - 0.5) < 0.08) ? 4.0 : 3.0);   // glossy ice
                if (zhash(floor(p) + float2(floor(t * 4.0), _Seed * 17.0)) > 0.988) rc = float2(0.0, gg_cap(5.0, 5.0, hot));
            }
            else if (F0 > thS - 0.1)
            {
                float4 vn = zN(_NoiseTex, p / 15.0 + float2(_Seed * 50.0, 0.3));
                if (vn.a < 0.07 + (F0 - thS + 0.1) * 0.5) rc = float2(3.0, vn.r > 0.55 ? 5.0 : 4.0);      // frost fringe veins
                else if (F0 > thS - 0.03 && zN(_NoiseTex, p / 2.0).g > 0.6) rc = float2(3.0, 4.0);      // rime specks at the edge
            }
            if (rc.x > -0.5 && !zdith(1.2 - edgeK * 0.7 - fd * 1.2, p)) rc = float2(-1.0, 0.0);
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
    {   // 5 FROST SURGE (Eternal Winter tick, v2; v1 read as a ring): a broken wave of small ice crystals SPROUTS outward from the hero to
        // _Radius over _Progress, each crystal pops up, glints, and sinks again; rime dots flash at their feet. No drawn ring.
        float pr = zsat(_Progress);
        float front = zeaseOut(pr) * 1.05;
        float2 cg = floor(p / 10.0);
        float best = 999.0;
        for (int j = 0; j < 9; j++)
        {
            float2 cc = cg + float2(zmod(float(j), 3.0) - 1.0, floor(float(j) / 3.0) - 2.0);
            float h1 = zhash(cc + float2(_Seed * 11.0, 1.3)), h2 = zhash(cc * 1.9 + float2(4.1, _Seed * 3.0));
            float2 b = floor(cc * 10.0 + float2(1.0 + 8.0 * h1, 1.0 + 8.0 * h2));
            float rb = length(float2(b.x, b.y / _Squash)) / max(_Radius, 1.0);
            float la = (front - rb - (h2 - 0.5) * 0.3) / 0.3;                                  // local age: 0 = the front reaches it
            if (rb < 1.0 && h1 > 0.58 && la > 0.0 && la < 1.0)
            {
                float hh = (3.0 + 5.0 * h2) * (1.0 - rb * 0.4) * (la < 0.3 ? zeaseOut(la / 0.3) : 1.0 - zsat((la - 0.45) / 0.55));
                float2 q = p - b;
                if (hh >= 2.0)
                {
                    float f = gg_crystal(q, hh, 1.0 + h1, (b.x / max(_Radius, 1.0)) * 3.0);
                    if (f > -0.5 && b.y < best) { best = b.y; rc = float2(0.0, gg_facetidx(f, hot)); gl = f > 2.5 ? 0.4 * (1.0 - la) : 0.0; }
                }
                float rq = length(float2(q.x, q.y * 2.0));
                if (rc.x < -0.5 && rq < 2.0 + 4.0 * la && rq > 1.0 && la < 0.8 && zN(_NoiseTex, p / 2.0 + float2(_Seed, h1)).g > 0.52) rc = float2(3.0, la < 0.4 ? 4.0 : 3.0);
            }
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
