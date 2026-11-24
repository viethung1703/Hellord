// Gonggong Water (t1682 v2): moving water read from top-down at game zoom. v2 (after the in-game clips, 2026-11-25): water is drawn as a
// RAISED MASS in the game's 3/4 view (crest columns with a lit top, a streaked front face and a dark foot), an opaque streaked body with
// ink-rimmed edges and a broken foam tail, so it reads on the blue Frozen Hell floor and on the light Mortar Hell sand alike.
// Never a flat wash / flat triangle / slab: every area is streak flow + caustics + foam lace; edges are noise-broken, never straight.
// _Part 0 = everything (ground tier, under sprites: enemies stand IN it), _Part 1 = crest top / lip / spray only (over sprites).
// Colours (row 1 water): 0 ink 181425, 1 teal 193c3e, 2 deep 124e89, 3 blue 0099db, 4 cyan 2ce8f5, 5 foam c0cbdc, 6 white.

// water body colour from a flow value + caustics. returns palette index (1..5)  [v1, used by whirlpool / puddle]
float gw_bodyidx(float2 p, float t, float depth01)
{
    float f = zflow(_NoiseTex, p, 14.0, t, 6.0, _Seed);
    float c = gg_caustic(_NoiseTex, p, t * 8.0, 15.0);
    float v = f * 0.65 + (1.0 - depth01) * 0.35;
    float idx = 2.0 + zband(v, 3.0, p, 0.6);
    if (c > 0.55) idx = max(idx, 4.0);
    if (c > 0.82) idx = 5.0;
    return idx;
}
// spray droplets thrown from a front (rel = p - front centre, dd = travel dir, dn = along the front). Returns colour index or -1.
float gw_spray(float2 rel, float2 dd, float2 dn, float a, float n, float spread, float seed)
{
    float res = -1.0;
    for (int i = 0; i < 12; i++)
    {
        float fi = float(i);
        if (fi >= n) break;
        float h = zhash(float2(fi * 3.17 + seed * 11.0, 4.1));
        float h2 = zhash(float2(fi * 1.91, seed * 7.0 + 2.3));
        float tt = zsat(a * (0.7 + 0.5 * h2));
        float2 b = dn * ((h - 0.5) * spread) + dd * ((4.0 + 14.0 * h2) * tt) + float2(0.0, 20.0 * tt * (0.8 + h) - 30.0 * tt * tt);
        float r = tt < 0.75 ? 1.5 : 1.0;
        float dr = gg_drop(rel - floor(b + float2(0.5, 0.5)), r);
        if (tt < 1.0 && dr > -0.5) res = dr < 0.5 ? 0.0 : (dr > 1.5 ? 6.0 : 4.0);
    }
    return res;
}
// streaked water body (ground level). q = (along, across) px in the flow frame; flow = scroll px along. depthK 0 edge .. 1 deep.
float gw_streak(float2 p, float along, float across, float flow, float t)
{
    float s1 = zN(_NoiseTex, float2((along - flow) / 22.0, across / 5.0) + float2(_Seed * 3.1, 0.0)).r;      // long streaks along the flow
    float s2 = zN(_NoiseTex, float2((along - flow * 1.3) / 9.0, across / 3.0) + float2(0.4, _Seed)).g;       // shorter ripples
    float c = gg_caustic(_NoiseTex, p, t * 8.0, 13.0);
    float v = s1 * 0.7 + s2 * 0.3;
    float idx = v < 0.36 ? 1.0 : (v < 0.55 ? 2.0 : (v < 0.7 ? 3.0 : 4.0));                                  // teal trough / deep / blue / cyan
    if (c > 0.6 && idx > 1.5) idx = max(idx, 4.0);                                                             // caustic net: cyan
    if (c > 0.86) idx = 5.0;                                                                                   // focused: foam pale
    return idx;
}
// crest height profile across its depth: s = ground distance relative to the front foot (0) ... -cw at its back.
float gw_crestH(float s, float cw, float H)
{
    float x = (s + cw) / max(cw, 1.0);
    float h = -1.0;
    if (x >= 0.0 && x <= 1.0) h = x < 0.68 ? H * sin(x / 0.68 * 1.5708) : H * sqrt(max(cos((x - 0.68) / 0.32 * 1.5708), 0.0));
    return h;
}
// 3/4-view raised wave renderer (columns). For screen pixel p, march heights z from the top down: the first column hit is the nearest
// (ground y = p.y - z). Front distance sf (+ bulge fo(v) + wobble), lateral half width hw (hw > 9000 = endless), crest depth cw, height H,
// body depth BD behind the crest, tail TL. Returns float4(idx, glow, kind, k): kind 0 none, 1 body, 2 tail, 3 crest face, 4 crest top.
float4 gw_wave(float2 p, float2 d, float2 nr, float sf, float hw, float cw, float H, float BD, float TL, float bulge, float wob, float t, float part)
{
    float4 o = float4(-1.0, 0.0, 0.0, 0.0);
    float s0 = dot(p, d) - sf; float v0 = dot(p, nr);
    // crest columns
    if (H >= 1.5 && s0 > -cw - H - 4.0 && s0 < H + 6.0)
    {
        for (int i = 0; i < 24; i++)
        {
            float z = float(23 - i);
            if (z > H + 1.0) continue;
            float2 g = p - float2(0.0, z);
            float v = dot(g, nr);
            float vw = hw > 9000.0 ? 0.0 : v / max(hw, 1.0);
            float fo = bulge * (1.0 - vw * vw) + wob * (zN(_NoiseTex, float2(v / 40.0, t * 0.05 + _Seed)).r - 0.5) + sin(v * 0.21 + t * 5.0) * 1.0;
            float s = dot(g, d) - sf - fo;
            float side = hw > 9000.0 ? 1.0 : zsat((hw - abs(v)) / 7.0);
            float Hv = H * (0.8 + 0.4 * zN(_NoiseTex, float2(v / 26.0, 0.3 + _Seed)).g) * sqrt(side);
            float h = gw_crestH(s, cw, Hv);
            if (side > 0.0 && h >= z && o.z < 0.5)
            {
                float x = (s + cw) / max(cw, 1.0);
                float top = h - z;
                float fn = zN(_NoiseTex, float2(v / 6.0 + t * 0.4, s / 4.0 - t * 0.9)).r;
                if (top < 1.6)
                {   // crest top: foam lip on the front half, cyan / blue back slope
                    o.x = x > 0.5 ? (fn > 0.42 ? 6.0 : 5.0) : (fn > 0.58 ? 4.0 : 3.0);
                    if (x > 0.5 && fn < 0.25) o.x = 4.0;
                    o.y = x > 0.5 ? 0.55 : 0.2; o.z = 4.0;
                }
                else
                {   // front face: vertical streaks pouring down, darker toward the foot, foam spilling at the foot
                    float fs = zN(_NoiseTex, float2(v / 3.0 + _Seed, (z + t * 26.0) / 9.0)).g;
                    o.x = fs > 0.66 ? 4.0 : (fs > 0.42 ? 3.0 : 2.0);
                    if (z < 2.5) o.x = fn > 0.5 ? 5.0 : 2.0;
                    if (top > h * 0.75 && z > 1.5 && fs < 0.3) o.x = 1.0;
                    o.y = 0.15; o.z = 3.0;
                }
                o.w = x;
            }
        }
    }
    if (o.z > 0.5 || part > 0.5) return o;
    // ground body + tail
    float vw0 = hw > 9000.0 ? 0.0 : v0 / max(hw, 1.0);
    float fo0 = bulge * (1.0 - vw0 * vw0) + wob * (zN(_NoiseTex, float2(v0 / 40.0, t * 0.05 + _Seed)).r - 0.5) + sin(v0 * 0.21 + t * 5.0) * 1.0;
    float s = s0 - fo0;
    float en = zN(_NoiseTex, float2(s / 9.0, v0 / 7.0) + float2(_Seed, 1.7)).r;
    float en2 = zN(_NoiseTex, float2(s / 23.0, v0 / 19.0) + float2(0.3, _Seed * 1.3)).g;
    float tap = hw > 9000.0 ? 1.0 : 0.5 + 0.5 * zsat(1.0 + (s + cw * 0.5) / max(BD + cw + TL * 0.5, 1.0));     // narrows toward the back
    float hwe = hw * tap + (en - 0.5) * 5.0 + (en2 - 0.5) * 9.0;
    if (hw < 9000.0 && abs(v0) > hwe + 1.0) return o;
    if (s <= 0.5 && s > -cw - BD)
    {
        o.x = gw_streak(p, s + sf, v0, t * 40.0, t); o.y = o.x > 3.5 ? 0.25 : 0.0; o.z = 1.0;
        if (s > -cw - 6.0 && gg_foam(_NoiseTex, p, -s - cw + 2.0, 6.0, t) > 0.55) o.x = 5.0;           // foam boiling at the crest's back
        if (s > -1.0) o.x = 0.0;                                                                         // ink foot under the front
        if (hw < 9000.0)
        {
            float ed = hwe - abs(v0);
            if (ed < 1.0) o.x = 0.0;                                                                     // ink rim (reads on light sand)
            else if (ed < 4.0) o.x = en > 0.45 ? 5.0 : 4.0;                                              // foam frill inside the rim
        }
    }
    else if (s <= -cw - BD && s > -cw - BD - TL)
    {
        float k = (-cw - BD - s) / max(TL, 1.0);                                                         // 0 body end .. 1 tail end
        float cl = zN(_NoiseTex, p / 8.0 + float2(_Seed * 2.0, 0.5)).g;
        float lane = abs(frac(v0 / 6.0 + zN(_NoiseTex, float2(s / 30.0, v0 / 40.0)).r * 1.5) - 0.5);
        if (k < 1.0 - en * 0.35)
        {
            if (cl > 0.25 + k * 0.6) { o.x = gw_streak(p, s + sf, v0, t * 40.0, t); o.z = 2.0; }      // the body breaks into wet clumps
            else if (lane < 0.06 * (1.0 - k) && cl > 0.2 && en2 > 0.4) { o.x = 5.0; o.z = 2.0; }          // broken foam lanes trailing behind
            else if (cl > 0.12 + k * 0.5 && zdith(1.0 - k, p)) { o.x = 1.0; o.z = 2.0; }               // dark wet sheen, dithered out
        }
    }
    return o;
}

float4 ZFX_Frag(float2 p, float2 w)
{
    float t = gg_t();
    float4 c = gg_none(); float gl = 0.0;
    float hot = zsat(_Hot);
    float md = _Mode;
    float ang = _Angle * 0.0174533;
    float2 d = float2(cos(ang), sin(ang)); float2 nr = float2(-d.y, d.x);
    float wi = -1.0;
    if (md < 0.5)
    {   // 0 TIDAL SURGE (v2): a raised 2.4 u wave (crest _Height px) rolls _Length px along d over _Progress 0..0.8, then crashes (0.8..1):
        // the crest collapses forward into foam. Body behind it grows with the distance travelled; a broken wet tail marks the path.
        float pr = zsat(_Progress);
        float crash = zsat((pr - 0.8) / 0.2);
        float sf = _Length * zeaseOut(zsat(pr / 0.8)) * 0.92 + 6.0 + crash * 8.0;
        float H = _Height * (1.0 - crash) * zsat(pr / 0.06 + 0.3);
        float hw = _Width * (1.0 + crash * 0.3);
        float BD = min(10.0 + 16.0 * zsat(pr / 0.5), max(sf - 9.0, 2.0));
        float TL = max(min(sf - 9.0 - BD, 36.0), 1.0);
        float4 o = gw_wave(p, d, nr, sf, hw, 9.0, H, BD, TL, 6.0, 3.0, t, _Part);
        wi = o.x; gl = o.y;
        if (crash > 0.0 && _Part < 0.5)
        {   // the crest collapses forward into a broken foam lace (no slab): lace thins as it runs out
            float sc = dot(p, d) - sf, vc = dot(p, nr);
            float ln = zN(_NoiseTex, float2(vc / 30.0, 0.7 + _Seed)).r;
            float reach = 3.0 + 14.0 * crash * (0.6 + 0.8 * ln);
            float hwc = hw * (1.0 - 0.25 * (vc * vc) / max(hw * hw, 1.0)) + (ln - 0.5) * 6.0;
            if (sc > -12.0 && sc < reach && abs(vc) < hwc)
            {
                float fn = zN(_NoiseTex, p / 3.5 + float2(t * 0.6, _Seed)).r;
                float th = 0.32 + crash * 0.3 + zsat((sc - reach + 5.0) / 5.0) * 0.3;
                if (fn > th) { wi = fn > th + 0.18 ? 6.0 : 5.0; gl = 0.3 * (1.0 - crash); }
                else if (fn > th - 0.1) wi = 4.0;
                else if (sc > -1.0 && o.z < 0.5) wi = -1.0;
            }
        }
        if (_Part > 0.5)
        {
            float2 rel = p - d * (sf + 2.0) - float2(0.0, H * 0.8);
            float sp = gw_spray(rel, d, nr, frac(_FxTime * 2.2), 9.0, hw * 1.6, _Seed + floor(_FxTime * 2.2) * 0.37);
            if (sp > -0.5 && wi < -0.5 && pr < 0.95) { wi = sp; gl = sp > 5.5 ? 0.4 : 0.0; }
        }
        if (_Fade > 0.0 && wi > -0.5 && !zdith(1.0 - _Fade * 1.1, p + float2(t * 9.0, 0.0))) wi = -1.0;
        if (wi > -0.5) c = gg_wat(_RampTex, gg_cap(wi, 6.0, hot));
    }
    else if (md < 1.5)
    {   // 1 WHIRLPOOL: churning spiral (log-spiral foam arms rotating inward), dark eye, broken rim, ice chips sucked in.
        float pr = zsat(_Progress);
        float form = zeaseOut(zsat(pr / 0.15));
        float coll = zsat((pr - 0.82) / 0.18);
        float R = _Radius * form * (1.0 - 0.75 * coll * coll) + 0.01;
        float2 g = float2(p.x, p.y / _Squash);
        float rr = length(g) / R;
        float a = atan2(g.y, g.x);
        float spin = t * (5.0 + 6.0 * coll);
        float s = a * 3.0 / 6.2831853 + log(max(rr, 0.02)) * 1.6 + spin;     // 3 arms
        float arm = abs(frac(s) - 0.5) * 2.0;                                  // 0 on an arm
        float nz = zN(_NoiseTex, g / 10.0 + float2(_Seed, -spin * 0.05)).r;
        float rim = 1.0 + (nz - 0.5) * 0.22;
        float idx = -1.0;
        if (_Part < 0.5)
        {
            if (rr < rim)
            {
                float depth = 1.0 - rr;
                float sw = zN(_NoiseTex, float2(a * 2.0 - spin * 0.6, log(max(rr, 0.02)) * 2.5) + float2(_Seed, 0.0)).g;   // swirling flow streaks
                idx = 1.0 + zband(zsat(rr * 0.75 + (sw - 0.5) * 0.6), 3.0, p, 0.7);   // teal eye -> deep -> blue out
                if (arm < 0.16 + 0.12 * rr) idx = (rr > 0.35) ? gg_cap(5.0 + (zbayer(p) > 0.15 ? 1.0 : 0.0), 6.0, hot) : 4.0;   // foam arms
                else if (arm < 0.3 + 0.1 * rr && rr > 0.25) idx = 4.0;
                else if (arm < 0.42 && rr > 0.3 && sw > 0.55) idx = 3.0;
                if (rr < 0.2 + 0.05 * sin(spin * 2.0)) idx = rr < 0.12 ? 0.0 : 1.0;      // the eye: ink / teal
                if (rr > rim - 0.08) idx = (zN(_NoiseTex, g / 5.0 + float2(spin * 0.02, 0.0)).g > 0.45) ? 5.0 : 3.0;  // broken rim foam
                if (rr > rim - 0.02) idx = 0.0;
                if (pr < 0.15 && !zdith(form * 1.2 - rr * 0.3, p)) idx = -1.0;
                if (coll > 0.0 && !zdith(1.0 - coll * 0.6, p + float2(t * 15.0, 1.0))) idx = -1.0;
                gl = (arm < 0.2 && rr > 0.3 && rr < 0.95) ? 0.3 : 0.0;
            }
            // 8 ice chips spiralling inward (sucked enemies' visual cue), each a 3 px chip
            for (int i = 0; i < 8; i++)
            {
                float fi = float(i);
                float ph = frac(zhash1(fi * 2.3 + _Seed * 9.0) + t * 0.55);
                float rad = R * (1.05 - ph * 0.95);
                float aa = zhash1(fi * 5.1 + 1.0) * 6.2831853 + spin * 1.3 + ph * 3.0;
                float2 cp = floor(float2(cos(aa) * rad, sin(aa) * rad * _Squash) + float2(0.5, 0.5));
                float f = gg_chip(p - cp, 1.2);
                if (f > -0.5 && ph < 0.9 && coll < 0.6) { idx = f < 0.5 ? 0.0 : (f > 3.5 ? gg_cap(6.0, 6.0, hot) : 5.0); }
            }
            if (idx > -0.5) c = gg_wat(_RampTex, idx);
        }
    }
    else if (md < 2.5)
    {   // 2 PUDDLE: churning shallow water, broken outline, ripples broken by noise (no flat disc)
        float2 g = float2(p.x, p.y / _Squash);
        float nz = zN(_NoiseTex, g / 9.0 + float2(_Seed * 3.0, 0.0)).r;
        float rr = length(g) / max(_Radius, 1.0) + (nz - 0.5) * 0.35;
        float alive = 1.0 - zsat(_Fade);
        if (rr < 1.0 && zclump(_NoiseTex, (1.0 - rr) * 2.2 * alive, p, _Seed))
        {
            float idx = gw_bodyidx(p, t, rr);
            float rip = abs(frac(rr * 3.0 - t * 1.2) - 0.5);
            if (rip < 0.07 && zN(_NoiseTex, g / 6.0 + float2(t * 0.1, 0.0)).g > 0.5) idx = 5.0;
            if (rr > 0.9) idx = 1.0;
            c = gg_wat(_RampTex, idx);
        }
    }
    else if (md < 3.5)
    {   // 3 GREAT FLOOD (v2): ONE continuous raised water mass sweeping along d (game: (1,-1), NW -> SE). Pivot = cast point; the front runs
        // -_Length -> +_Length over _Progress. Endless along the front; solid streaked body 120 px deep (no holes), then a 70 px broken tail.
        float pr = zsat(_Progress);
        float sf = -_Length + 2.0 * _Length * pr;
        float4 o = gw_wave(p, d, nr, sf, 99999.0, 22.0, _Height, 120.0, 70.0, 0.0, 10.0, t, _Part);
        wi = o.x; gl = o.y;
        if (_Part > 0.5)
        {   // spray tiled along the front (one burst set per 56 px of front)
            float v = dot(p, nr); float cell = floor(v / 56.0);
            float2 rel = p - d * (sf + 4.0) - nr * (cell * 56.0 + 28.0) - float2(0.0, _Height * 0.8);
            float sp = gw_spray(rel, d, nr, frac(_FxTime * 2.0 + zhash1(cell) ), 6.0, 48.0, _Seed + cell * 0.31 + floor(_FxTime * 2.0 + zhash1(cell)) * 0.37);
            if (sp > -0.5 && wi < -0.5) { wi = sp; gl = sp > 5.5 ? 0.4 : 0.0; }
        }
        float fade = zsat((pr - 0.9) / 0.1);
        if (fade > 0.0 && wi > -0.5 && !zdith(1.0 - fade, p + float2(t * 9.0, 0.0))) wi = -1.0;
        if (wi > -0.5) c = gg_wat(_RampTex, gg_cap(wi, 6.0, hot));
    }
    else if (md < 4.5)
    {   // 4 RIDE THE FLOOD (v2): the hero surfs a raised water mound (3/4 columns, foam lip on the bow, streaked face), leaving a Kelvin
        // wake: two foam arms diverging behind with turbulent streaked water between them that breaks into clumps (no flat wedge).
        float2 vel = _Vel.xy; float spd = length(vel);
        float2 dv = spd > 1.0 ? vel / spd : float2(1.0, 0.0); float2 dn = float2(-dv.y, dv.x);
        float pr = zsat(_Progress);
        float I = zsat(pr / 0.08) * (1.0 - zsat((pr - 0.9) / 0.1));
        float L = _Width * 2.4 * I;                                           // wake length past the stern (29 px at W 12)
        float H = 10.0 * I;
        float hit = -1.0;
        for (int i = 0; i < 12; i++)
        {
            float z = float(11 - i);
            if (z > H + 0.5 || hit > -0.5) continue;
            float2 g = p - float2(0.0, z);
            float gu = dot(g, dv), gv = dot(g, dn);
            float en0 = zN(_NoiseTex, float2(gv / 9.0, gu / 9.0 + t * 0.3) + float2(_Seed, 2.0)).r;
            float gw = _Width * 1.3 * (1.0 - 0.3 * zsat(-(gu + 8.0) / 20.0));                        // narrower stern
            float e = ((gu + 8.0) / 20.0) * ((gu + 8.0) / 20.0) + (gv / gw) * (gv / gw) + (en0 - 0.5) * 0.3;
            float h = H * sqrt(max(1.0 - e, 0.0)) * (0.7 + 0.3 * zsat((gu + 26.0) / 30.0));
            if (e < 1.0 && h >= z)
            {
                float fn = zN(_NoiseTex, float2(gv / 5.0 + t * 0.6, gu / 4.0 - t * 1.4) + float2(_Seed, 0.0)).r;
                if (h - z < 1.6) hit = (gu > 4.0) ? (fn > 0.4 ? 6.0 : 5.0) : (fn > 0.62 ? 5.0 : (fn > 0.4 ? 4.0 : 3.0));   // top: foam bow, cyan back
                else
                {   // face: streaks pouring down off the bow, deep at the foot, ink outline at the foot
                    float fs = zN(_NoiseTex, float2(gv / 3.0 + _Seed, (z + t * 24.0) / 7.0)).g;
                    hit = fs > 0.66 ? 4.0 : (fs > 0.4 ? 3.0 : 2.0);
                    if (z < 1.5) hit = (gu > 4.0 && fn > 0.45) ? 5.0 : 1.0;
                    if (z < 0.5 && e > 0.82) hit = 0.0;
                }
                gl = gu > 3.0 ? 0.45 : 0.15;
            }
        }
        if (_Part > 0.5)
        {   // spray off both sides of the bow + a few droplets ahead
            float ph = frac(_FxTime * 3.0);
            float sp = gw_spray(p - dv * 10.0 - float2(0.0, 5.0), dn, dv, ph, 5.0, 10.0, _Seed + floor(_FxTime * 3.0) * 0.41);
            float sp2 = gw_spray(p - dv * 10.0 - float2(0.0, 5.0), -dn, dv, frac(ph + 0.5), 5.0, 10.0, _Seed + 0.5 + floor(_FxTime * 3.0 + 0.5) * 0.41);
            if (hit < -0.5) hit = max(sp, sp2);
            if (hit > -0.5 && I < 0.2) hit = -1.0;
            wi = hit; if (wi > 5.5) gl = 0.4;
        }
        else if (hit > -0.5) wi = hit;
        else
        {
            float u = dot(p, dv), v = dot(p, dn);
            float a = -u - 20.0;                                                     // distance behind the stern
            if (a > -10.0 && a < L)
            {
                float k = zsat(a / max(L, 1.0));
                float en = zN(_NoiseTex, float2(u / 8.0, v / 6.0) + float2(_Seed, t * 0.2)).r;
                float arm = _Width * 0.9 + max(a, 0.0) * 0.36 + (en - 0.5) * 3.0;            // ~20 deg Kelvin arms
                float da = abs(abs(v) - arm);
                if (a > 0.0 && da < 2.2 + k * 0.8 && en > 0.18 + k * 0.5) { wi = k < 0.4 ? 6.0 : 5.0; gl = 0.3 * (1.0 - k); }  // foam arms
                else if (abs(v) < arm - 1.0)
                {
                    float cl = zN(_NoiseTex, p / 6.0 + float2(t * 0.3, _Seed)).g;
                    if (cl > 0.08 + k * 0.72)
                    {
                        wi = gw_streak(p, u + t * 60.0, v, 0.0, t);                         // churned water (streaks run back along the wake)
                        if (abs(v) > arm - 3.0 || (a < 2.0 && en > 0.5)) wi = 5.0;          // foam edging / stern boil
                        if (k > 0.55 && wi > 1.5 && !zdith(1.4 - k, p)) wi = 1.0;           // dark wet sheen as it settles
                    }
                    else if (a < 0.0 && abs(v) < _Width * 0.9) wi = 2.0;
                }
            }
        }
        if (wi > -0.5) c = gg_wat(_RampTex, gg_cap(wi, 6.0, hot));
    }
    else
    {   // 5 RAM TRAIL (v2, Ram of Mount Buzhou): a surging foam bow mound at the hero (pivot + _TargetPx * _Progress), behind it a torrent
        // that rushes back, narrows and FREEZES: ice crust plates and jutting ice shards rise in the wake, then all fades (_Fade).
        float2 T = _TargetPx.xy; float L = max(length(T), 1.0); float2 td = T / L; float2 tn = float2(-td.y, td.x);
        float pr = zsat(_Progress); float fd = zsat(_Fade);
        float head = L * pr;
        float hit = -1.0; float row = 1.0;
        // bow surge mound (columns), collapses after the charge
        float H = 8.0 * (1.0 - zsat(fd * 2.5));
        for (int i = 0; i < 10; i++)
        {
            float z = float(9 - i);
            if (z > H + 0.5 || hit > -0.5) continue;
            float2 g = p - float2(0.0, z);
            float gu = dot(g, td) - head, gv = dot(g, tn);
            float wob = zN(_NoiseTex, float2(atan2(gv, gu) * 1.3, t * 0.7) + float2(_Seed, 0.0)).r;
            float e = ((gu + 2.0) / 11.0) * ((gu + 2.0) / 11.0) + (gv / (_Width + 1.0)) * (gv / (_Width + 1.0));
            float h = H * (0.75 + 0.5 * wob) * sqrt(max(1.0 - e, 0.0));
            if (e < 1.0 && h >= z)
            {
                float fn = zN(_NoiseTex, float2(gv / 4.0 + t * 0.8, gu / 4.0 - t * 1.6) + float2(_Seed, 0.3)).r;
                hit = (h - z < 1.6) ? (fn > 0.35 ? 6.0 : 4.0) : (z < 1.5 ? 2.0 : (fn > 0.55 ? 4.0 : 3.0));
                gl = 0.5;
            }
        }
        float u = dot(p, td), v = dot(p, tn);
        if (_Part > 0.5)
        {
            float ph = frac(_FxTime * 3.5);
            float sp = gw_spray(p - td * (head + 4.0) - float2(0.0, 6.0), tn, -td, ph, 6.0, 14.0, _Seed + floor(_FxTime * 3.5) * 0.29);
            float sp2 = gw_spray(p - td * (head + 4.0) - float2(0.0, 6.0), -tn, -td, frac(ph + 0.5), 6.0, 14.0, _Seed + 0.5 + floor(_FxTime * 3.5 + 0.5) * 0.29);
            if (hit < -0.5 && fd < 0.3) hit = max(sp, sp2);
        }
        else if (hit < -0.5)
        {
            float a = head - u;                                                      // px behind the hero
            float age = a / 150.0 + fd;
            float en = zN(_NoiseTex, float2(u / 7.0, 0.5) + float2(_Seed, 0.0)).r;
            float en2 = zN(_NoiseTex, float2(u / 23.0, 0.9) + float2(_Seed * 1.7, 0.0)).g;
            float hw = _Width * (1.0 - 0.4 * zsat(a / 60.0)) * (1.0 - 0.5 * fd) + (en - 0.5) * 5.0 + (en2 - 0.5) * 8.0 + sin(u * 0.33 + t * 8.0) * 1.0;
            float frz = zsat((a - 18.0) / 30.0 + fd);                                // the torrent freezes as it falls behind
            if (u > -3.0 && a > -2.0 && abs(v) < hw && age < 1.3)
            {
                float4 cn = zN(_NoiseTex, p / 7.0 + float2(_Seed * 4.0, 1.0));
                if (cn.b < frz * 1.15)
                {   // solid ice crust plates (row 0): faceted shade per cell, bright facet edges, ink outer rim
                    row = 0.0; hit = cn.a < 0.07 ? 4.0 : (cn.b > 0.62 ? 3.0 : (cn.b > 0.3 ? 2.0 : 1.0));
                    if (hw - abs(v) < 1.0) hit = 0.0;
                }
                else if (frz < 0.95)
                {
                    hit = gw_streak(p, u + t * 90.0, v, 0.0, t);                               // torrent rushing back
                    if (hw - abs(v) < 3.0) hit = en > 0.4 ? 5.0 : 4.0;                         // foam edges
                    if (hw - abs(v) < 1.0 && age < 0.5) hit = 0.0;                             // ink rim near the hero
                }
                if (hit > -0.5 && !zdith(1.0 - fd * 1.15 + zsat(a / 400.0) * 0.0, p)) hit = -1.0;
            }
        }
        // a jagged RIDGE of ice crystals freezes up along the wake (5 px steps, alternating sides, overlapping, leaning back from the
        // charge, 4..11 px tall) - the nearest base is drawn in front, so it reads as one continuous frozen tide, not a row of pegs
        if (_Part < 0.5)
        {
            float k0 = floor(u / 5.0);
            float bestY = 999.0;
            for (int j = 0; j < 9; j++)
            {
                float kk = k0 - 5.0 + float(j);
                float uk = kk * 5.0 + 2.0;
                float hk = zhash1(kk * 1.37 + _Seed * 5.0), hk2 = zhash1(kk * 2.9 + 1.0);
                float side = zmod(kk, 2.0) < 0.5 ? -1.0 : 1.0;
                float2 b = floor(td * uk + tn * (side * (1.0 + hk * _Width * 0.45)) + float2(0.5, 0.5));
                float ak = head - uk;
                float gr = zeaseOut(zsat((ak - 14.0) / 22.0)) * (1.0 - zsat(fd * 1.7 - hk * 0.5));
                float hh = (4.0 + 7.0 * hk2 * (1.0 - 0.4 * zsat(ak / 160.0))) * gr;
                if (uk > 2.0 && uk < head && hk > 0.12 && hh >= 2.0)
                {
                    float f = gg_crystal(p - b, hh, 1.6 + hk * 1.4, -td.x * (2.0 + 2.0 * hk) + side * 1.0);
                    if (f > -0.5 && b.y < bestY) { bestY = b.y; row = 0.0; hit = gg_facetidx(f, hot); gl = f > 3.5 ? 0.35 : gl; }
                }
            }
        }
        if (hit > -0.5) c = float4(zramp(_RampTex, row, row > 0.5 ? gg_cap(hit, 6.0, hot) : hit), 1.0);
        gl = hit > -0.5 ? gl : 0.0;
    }
    if (_Layer > 0.5)
    {
        float g2 = min(gl, _GlowCap);
        return (g2 > 0.02 && c.a > 0.5) ? zglow(_RampTex, g2, p, _GlowGain) : gg_none();
    }
    return c;
}
