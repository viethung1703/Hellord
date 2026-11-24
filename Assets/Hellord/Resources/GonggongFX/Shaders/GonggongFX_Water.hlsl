// Gonggong Water (t1682): moving water read from top-down at game zoom. Real material, not flat washes: churning blue / cyan bands
// from domain-warped flow noise, caustic light lines (two drifting voronoi edge layers), broken foam lips with an ink rim on the
// leading side, a wet dark sheen left behind that breaks up into puddles, and a few spray droplets (<= 12). 15 fps stepped.
// _Part 0 = body (ground tier, under sprites: enemies stand IN it), _Part 1 = crest / lip / spray (over sprites).
// Colours (row 1 water): 0 ink 181425, 1 teal 193c3e, 2 deep 124e89, 3 blue 0099db, 4 cyan 2ce8f5, 5 foam c0cbdc, 6 white.

// water body colour from a flow value + caustics. returns palette index (1..5)
float gw_bodyidx(float2 p, float t, float depth01)
{
    float f = zflow(_NoiseTex, p, 14.0, t, 6.0, _Seed);
    float c = gg_caustic(_NoiseTex, p, t * 8.0, 15.0);
    float v = f * 0.65 + (1.0 - depth01) * 0.35;
    float idx = 2.0 + zband(v, 3.0, p, 0.6);                    // deep / blue / cyan
    if (c > 0.55) idx = max(idx, 4.0);                           // caustic line: cyan
    if (c > 0.82) idx = 5.0;                                      // focused caustic: foam pale
    return idx;
}
// a deterministic set of spray droplets thrown from a front. rel = p - front centre (screen px, y up); dd = travel dir, dn = along the
// front. Droplets start spread along the front, fly forward along dd and arc up-screen. Returns colour index or -1. n droplets, life a 0..1.
float gw_spray(float2 rel, float2 dd, float2 dn, float a, float n, float spread, float seed)
{
    float res = -1.0;
    for (int i = 0; i < 12; i++)
    {
        float fi = float(i);
        if (fi >= n) break;
        float h = zhash(float2(fi * 3.17 + seed * 11.0, 4.1));
        float h2 = zhash(float2(fi * 1.91, seed * 7.0 + 2.3));
        float tt = a * (0.7 + 0.5 * h2);
        float2 b = dn * ((h - 0.5) * spread) + dd * ((4.0 + 14.0 * h2) * tt) + float2(0.0, 20.0 * tt * (0.8 + h) - 30.0 * tt * tt);
        float r = tt < 0.75 ? 1.5 : 1.0;
        float dr = gg_drop(rel - floor(b + float2(0.5, 0.5)), r);
        if (tt < 1.0 && dr > -0.5) res = dr < 0.5 ? 0.0 : (dr > 1.5 ? 6.0 : 4.0);
    }
    return res;
}

float4 ZFX_Frag(float2 p, float2 w)
{
    float t = gg_t();
    float4 c = gg_none(); float gl = 0.0;
    float hot = zsat(_Hot);
    float md = _Mode;
    float ang = _Angle * 0.0174533;
    float2 d = float2(cos(ang), sin(ang)); float2 nr = float2(-d.y, d.x);
    if (md < 0.5)
    {   // 0 TIDAL SURGE: a 2.4 u wave front rolls _Length px along d over _Progress 0..0.8, then crashes (0.8..1)
        float pr = zsat(_Progress);
        float sf = _Length * zeaseOut(zsat(pr / 0.8)) * 0.92 + 6.0;   // front distance from the pivot
        float crash = zsat((pr - 0.8) / 0.2);
        float u = dot(p, d), v = dot(p, nr);
        float vw = v / max(_Width, 1.0);
        float fo = 6.0 * (1.0 - vw * vw) + sin(v * 0.35 + t * 6.0) * 1.0;    // front bulges forward in the middle + ripples
        float b = sf + fo - u;                                      // >0 behind the front
        float nz = zN(_NoiseTex, p / 9.0 + float2(_Seed, t * 0.3)).r;
        float hw = _Width * (1.0 + crash * 0.35) + (nz - 0.5) * 4.0;
        float side = 1.0 - zsat((abs(v) - hw + 3.0) / 3.0);         // 1 inside, falls off over the last 3 px of the width
        float lipW = 3.0 + 1.5 * (1.0 - crash);                     // foam lip depth (>= 3 px)
        float bodyD = 9.0 + 6.0 * zsat(pr / 0.4);                   // water body depth behind the lip
        float wake = 26.0 * (1.0 - crash * 0.6);
        if (_Part < 0.5)
        {
            if (abs(v) <= hw + 1.0 && b > -1.0 && b < lipW + bodyD + wake && u > -6.0)
            {
                float db = (b - lipW) / bodyD;                       // 0..1 across the body
                float idx = -1.0;
                if (b < 0.0) idx = 0.0;                             // ink line ahead of the lip (ground contact shadow)
                else if (b < lipW) idx = gg_cap(5.0 + (zbayer(p) > 0.1 ? 1.0 : 0.0), 6.0, hot);   // foam lip, white dithered
                else if (db < 1.0)
                {
                    idx = gw_bodyidx(p - d * sf * 0.6, t, db);
                    if (gg_foam(_NoiseTex, p, b - lipW, 5.0, t) > 0.7) idx = 5.0;                 // trailing foam streaks
                    if (abs(v) > hw - 1.5) idx = 0.0;                                            // ink side edge
                    else if (abs(v) > hw - 3.0) idx = 5.0;                                       // side foam
                }
                else
                {   // wet sheen behind: dark teal + blue puddles, breaking up with distance and over the crash
                    float k = (b - lipW - bodyD) / wake;
                    float dens = (1.0 - k) * (1.0 - crash * 0.7) - _Fade;
                    if (zclump(_NoiseTex, dens, p, _Seed)) idx = zN(_NoiseTex, p / 6.0 + float2(_Seed * 2.0, 0.0)).g > 0.62 ? 2.0 : 1.0;
                    if (idx > 0.5 && gg_caustic(_NoiseTex, p, t * 6.0, 11.0) > 0.78) idx = 3.0;
                }
                if (crash > 0.0 && idx > -0.5 && idx != 0.0 && !zdith(1.0 - crash * 0.85 - _Fade, p + float2(t * 15.0, 0.0))) idx = -1.0;
                if (side <= 0.0 && idx != 0.0) idx = -1.0;
                if (idx > -0.5) c = gg_wat(_RampTex, idx);
                gl = (b > -1.0 && b < lipW + 4.0 && abs(v) < hw) ? 0.35 * (1.0 - crash) : 0.0;
            }
        }
        else
        {   // crest: rolling foam lip rising 4-6 px above the front + spray thrown forward
            float lift = 5.0 * (1.0 - crash) + 1.0;
            float2 q = p - float2(0.0, 0.0);
            float idx = -1.0;
            for (int k = 0; k < 7; k++)
            {   // the lip, drawn as stacked slices (z up), bottom slice = front line
                float z = float(k);
                if (z > lift) break;
                float2 pz = p - float2(0.0, z);
                float uz = dot(pz, d), vz = dot(pz, nr);
                float vwz = vz / max(_Width, 1.0);
                float e = uz - sf - 6.0 * (1.0 - vwz * vwz) - sin(vz * 0.35 + t * 6.0) * 1.0;   // >0 ahead of the (bulged) front
                float curl = z / max(lift, 1.0);
                float hwz = _Width * (1.0 + crash * 0.35) - 1.0 - curl * 2.0 + (zN(_NoiseTex, pz / 8.0 + float2(t * 0.4, _Seed)).r - 0.5) * 5.0;
                if (abs(vz) <= hwz && e > -2.5 + curl * 1.5 && e < 1.0 + curl * 1.5)
                {
                    float nzz = zN(_NoiseTex, pz / 5.0 + float2(t * 0.7, z * 0.13)).g;
                    idx = (z > lift - 1.5) ? gg_cap(6.0, 6.0, hot) : (nzz > 0.55 ? 5.0 : 4.0);
                    if (e > 0.0 + curl * 1.5) idx = 0.0;            // ink on the leading underside
                }
            }
            // spray: 10 droplets thrown forward in the crash, 4 during the roll
            float2 fp = d * (sf + 8.0);
            float2 rel = p - fp; float2 loc = float2(dot(rel, nr), dot(rel, d));
            float sa = crash > 0.0 ? crash * 1.1 : zmod(t * 2.5, 1.0);
            float sp = gw_spray(rel, d, nr, sa, crash > 0.0 ? 10.0 : 4.0, _Width * 1.6, _Seed + (crash > 0.0 ? 0.5 : floor(t * 2.5) * 0.17));
            if (idx < -0.5 && sp > -0.5) idx = sp > 5.5 ? gg_cap(6.0, 6.0, hot) : sp;
            if (crash > 0.0 && idx > -0.5 && sp < -0.5 && !zdith(1.0 - crash * 1.2, p)) idx = -1.0;
            if (idx > -0.5) c = gg_wat(_RampTex, idx);
            gl = (idx > 3.5) ? 0.55 : 0.0;
        }
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
    {   // 3 GREAT FLOOD: a screen-wide wave sweeping along d (default NW -> SE). Pivot = view centre.
        float pr = zsat(_Progress);
        float sf = -_Length + 2.0 * _Length * pr;                      // front distance along d
        float u = dot(p, d), v = dot(p, nr);
        float b = sf - u;                                             // >0 behind the front
        float body = 70.0;                                            // full water body depth behind the lip
        float tail = 150.0;
        float wob = (zN(_NoiseTex, float2(v / 40.0, _Seed + t * 0.05)).r - 0.5) * 16.0 + sin(v * 0.07 + t * 3.0) * 3.0;
        b += wob;
        float idx = -1.0;
        if (_Part < 0.5)
        {
            if (b > -1.0 && b < 5.0 + body + tail)
            {
                if (b < 0.0) idx = 0.0;
                else if (b < 5.0) idx = gg_cap(5.0 + (zbayer(p) > 0.0 ? 1.0 : 0.0), 6.0, hot);
                else if (b < 5.0 + body)
                {
                    float db = (b - 5.0) / body;
                    idx = gw_bodyidx(p - d * t * 40.0, t, db);
                    if (gg_foam(_NoiseTex, p * 0.8, zmod(b, 23.0), 6.0, t) > 0.75) idx = 5.0;    // foam streaks parallel to the front
                    if (db > 0.6 && !zdith((1.0 - db) * 2.5 + 0.1, p)) idx = 1.0;               // thinning to a sheet (teal shows the floor through dither)
                }
                else
                {
                    float k = (b - 5.0 - body) / tail;
                    if (zclump(_NoiseTex, (1.0 - k) * 0.9 - _Fade, p, _Seed)) idx = zN(_NoiseTex, p / 7.0).g > 0.6 ? 2.0 : 1.0;
                    if (idx > 0.5 && gg_caustic(_NoiseTex, p, t * 6.0, 13.0) > 0.8) idx = 3.0;
                }
            }
            if (idx > -0.5) c = gg_wat(_RampTex, idx);
            gl = (b > -1.0 && b < 12.0) ? 0.35 : 0.0;
        }
        else
        {   // crest: foam lip 6 px tall rolling over + spray clusters along the front (12 droplets)
            float lift = 6.0;
            for (int k = 0; k < 7; k++)
            {
                float z = float(k);
                float2 pz = p - float2(0.0, z);
                float bz = sf - dot(pz, d) + (zN(_NoiseTex, float2(dot(pz, nr) / 40.0, _Seed + t * 0.05)).r - 0.5) * 16.0 + sin(dot(pz, nr) * 0.07 + t * 3.0) * 3.0;
                float curl = z / lift;
                if (bz > -1.0 - curl * 2.0 && bz < 3.0 - curl * 1.0)
                {
                    float nzz = zN(_NoiseTex, pz / 6.0 + float2(t * 0.8, z * 0.1)).g;
                    idx = z > lift - 1.5 ? gg_cap(6.0, 6.0, hot) : (nzz > 0.5 ? 5.0 : 4.0);
                    if (bz < -curl * 2.0) idx = 0.0;
                }
            }
            float cl = floor((v + 400.0) / 110.0);                          // spray cluster cell along the front
            float2 fp = d * (sf + 3.0) + nr * ((cl + 0.5) * 110.0 - 400.0);
            float2 rel = p - fp;
            float sp = gw_spray(rel, d, nr, zmod(t * 1.7 + zhash1(cl) , 1.0), 2.0, 30.0, _Seed + cl * 0.31 + floor(t * 1.7 + zhash1(cl)) * 0.13);
            if (idx < -0.5 && sp > -0.5) idx = sp > 5.5 ? gg_cap(6.0, 6.0, hot) : sp;
            if (idx > -0.5) c = gg_wat(_RampTex, idx);
            gl = idx > 3.5 ? 0.5 : 0.0;
        }
    }
    else if (md < 4.5)
    {   // 4 RIDE THE FLOOD: water mound around the hero's feet, V wake behind (against _Vel), bow wave ahead. Over part = bow lip + spray.
        float pr = zsat(_Progress);
        float k = zsat(pr / 0.12) * (1.0 - zsat((pr - 0.85) / 0.15));
        float2 vd = _Vel.xy / max(length(_Vel.xy), 1.0);
        float2 vn = float2(-vd.y, vd.x);
        float u = dot(p, vd), v = dot(p, vn);
        float2 g = float2(u, v / 0.6);
        float rr = length(float2(u / 18.0, v / 12.0));
        float idx = -1.0;
        if (_Part < 0.5)
        {
            if (rr < 1.0 * k + 0.05)
            {   // mound
                idx = max(2.0, gw_bodyidx(p + vd * t * 50.0, t, rr) - (rr < 0.6 ? 1.0 : 0.0));
                if (rr > 0.8 * k) idx = (zN(_NoiseTex, p / 4.0 + float2(t, 0.0)).g > 0.4) ? 5.0 : 0.0;
            }
            float back = -u;
            if (back > 4.0 && back < 56.0 * k)
            {   // V wake: two broken foam lines at +-28 deg + churned water between, fading with distance
                float spread = back * 0.5 + 4.0;
                float fl = abs(abs(v) - spread);
                float age = back / 44.0;
                float nz = zN(_NoiseTex, p / 5.0 + float2(t * 0.6, _Seed)).r;
                if (fl < 1.6 && nz > 0.25 + age * 0.5) idx = age < 0.4 ? gg_cap(6.0, 6.0, hot) : 5.0;
                else if (abs(v) < spread - 1.0 && zclump(_NoiseTex, (1.0 - age) * 0.95, p + vd * t * 30.0, _Seed)) idx = (idx < -0.5) ? (gg_caustic(_NoiseTex, p, t * 8.0, 9.0) > 0.6 ? 4.0 : 3.0) : idx;
            }
            if (idx > -0.5) c = gg_wat(_RampTex, idx);
        }
        else
        {   // bow lip in front of the feet (covers them: he rides IN the water) + 4 droplets
            float fu = u - 9.0 * k;
            if (abs(v) < 10.0 * k && fu > -3.0 && fu < 1.0 && p.y < 6.0)
            {
                float nz = zN(_NoiseTex, p / 4.0 + float2(t * 0.9, 0.0)).g;
                idx = fu > 0.0 ? 0.0 : (nz > 0.45 ? gg_cap(6.0, 6.0, hot) : 5.0);
            }
            // lip under the hero's feet (front-facing arc of the mound, drawn over his feet)
            float rr2 = length(float2(p.x / 12.0, (p.y + 1.0) / 4.0));
            if (idx < -0.5 && rr2 > 0.75 && rr2 < 1.0 && p.y < -0.5 && k > 0.3) idx = (zN(_NoiseTex, p / 3.0 + float2(t * 1.3, 0.0)).g > 0.5) ? 5.0 : 4.0;
            float2 rel = p - vd * (10.0 * k);
            float sp = gw_spray(rel, vd, vn, zmod(t * 3.0, 1.0), 4.0, 14.0, _Seed + floor(t * 3.0) * 0.21);
            if (idx < -0.5 && sp > -0.5 && k > 0.3) idx = sp > 5.5 ? gg_cap(6.0, 6.0, hot) : sp;
            if (idx > -0.5) c = gg_wat(_RampTex, idx);
            gl = idx > 4.5 ? 0.4 : 0.0;
        }
    }
    else
    {   // 5 TORRENT TRAIL (Ram of Mount Buzhou charge): churned water + ice slush along pivot -> _TargetPx behind the hero (at _Progress)
        float2 T = _TargetPx.xy; float L = max(length(T), 1.0); float2 td = T / L; float2 tn = float2(-td.y, td.x);
        float u = dot(p, td), v = dot(p, tn);
        float head = L * zsat(_Progress);
        float age = (head - u) / 60.0 + zsat(_Fade);                    // 0 at the hero .. 1 after 60 px
        float nz = zN(_NoiseTex, p / 8.0 + float2(_Seed, 0.0)).r;
        float hw = _Width * (1.0 - 0.4 * zsat(age)) + (nz - 0.5) * 3.0;
        float idx = -1.0;
        if (u > -4.0 && u < head && abs(v) < hw + 1.0 && age < 1.2)
        {
            if (abs(v) > hw - 0.5) idx = 0.0;
            else
            {
                idx = gw_bodyidx(p - td * t * 30.0, t, abs(v) / hw);
                float f = gg_foam(_NoiseTex, p, abs(v) - hw + 4.0, 3.0, t);
                if (f > 0.6) idx = 5.0;
                float4 cr = zN(_NoiseTex, p / 6.0 + float2(_Seed * 4.0, 1.0));
                if (cr.a < 0.1 && cr.b > 0.5) idx = 4.0;                    // ice slush plates
            }
            if (!zdith(1.1 - age, p)) idx = (idx == 0.0 || age > 1.0) ? -1.0 : 1.0;
        }
        if (idx > -0.5) c = float4(zramp(_RampTex, 1.0, idx), 1.0);
        gl = (idx > 3.5 && age < 0.3) ? 0.4 : 0.0;
    }
    if (_Layer > 0.5)
    {
        float g2 = min(gl, _GlowCap);
        return (g2 > 0.02 && c.a > 0.5) ? zglow(_RampTex, g2, p, _GlowGain) : gg_none();
    }
    return c;
}
