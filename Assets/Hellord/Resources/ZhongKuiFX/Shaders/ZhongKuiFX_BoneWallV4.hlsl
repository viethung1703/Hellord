// Bone Wall v4c (Zhong Kui kit v4 movement skill M3; v4a / v4b archived in archive/): "a 3 u bone barrier across your path at the dash start for 2.5 s; blocks
// chasers and enemy bullets; enemies break it in 20 hits" (t1709 report M3, code HellordZhongKui1713.NkDashAfter1713).
// Bone and earth only: no ghosts, no spirit light, no flat rings / discs / washes. Same portable HLSL subset as Core (compiled as
// GLSL by the MOCKUP renderer).
// The wall blocks from the instant it exists, so it is solid within 3 frames:
//  f0      the centre pillars punch out of a split in the ground at 55 % height, dirt clods kick out both sides, capped base flash
//  f1      they overshoot (112 %), the next pillars punch out; dust at the bases
//  f2..f3  the ends arrive; every pillar settles at 100 %. The trench and its side cracks have run the full length
//  hold    static pixel art (no idle wobble). _Damage (1 - hp / 20) adds a crack, a snapped tip or a fallen skull per few hits
//  break   _Break 0.05..1 over 0.3 s (5 frames): cracks everywhere (glint) -> pillars split in 3 chunks -> chunks drop and chips
//          fly -> a low rubble heap with dust -> the rubble crumbles away by noise (_Fade)
//  _Mode 1 WALL   (ysort; the quad the current hook draws): pillars + dirt berms + tight contact shadow + kick + crumble.
//  _Mode 0 GROUND (ground layer, proposed extra quad): the trench, side cracks, the pillars' cast shadows, fallen tips, rubble bed.
// Pillar i (0.._Count-1) stands at s_i along perp(dash dir), v4c: 13 pillars, heights 27..48 px (saw-toothed; tallest in the middle across-screen, at the ends up/down). Light from the upper left.

float zbw_t(float t) { return floor(t * _StepFps + 0.0001); }                     // stepped frame index (15 fps)
// facet -> pale bone (v4b map): 0 ink 181425, 1 notch 733e39, 2 shadow c28569, 3 mid e8b796, 4 lit ead4aa, 5 ridge ffffff (hot)
float4 zbw_bone(float f)
{
    float i = f < 0.5 ? 0.0 : (f < 1.5 ? 2.0 : (f < 2.5 ? 3.0 : (f < 3.5 ? 4.0 : (f < 4.5 ? 5.0 : 6.0))));
    if (i > 5.5 && _Hot < 0.75) i = 5.0;
    return float4(zramp(_RampTex, 0.0, i), 1.0);
}
float4 zbw_earth(float i) { return float4(zramp(_RampTex, 3.0, i), 1.0); }      // 0 181425 1 262b44 2 3e2731 3 733e39 4 b86f50
float4 zbw_dustc(float i) { return float4(zramp(_RampTex, 1.0, i), 1.0); }      // 0 181425 1 3e2731 2 733e39 3 b86f50 4 c28569 5 e8b796
float2 zbw_dir() { float a = _Angle * 0.01745329; return float2(cos(a), sin(a)); }
float zbw_r(float i, float k) { return zhash(float2(i * 7.31 + k * 3.17 + 1.3, _Seed * 97.0 + k * 13.7)); }
float zbw_k() { return min(_Count, 13.0); }
// pillar i: along-wall offset s (px)
// v4b: hv = how much the wall runs up/down the screen (1 for an east/west dash). A vertical wall is seen across its thickness,
// so it gets two staggered ranks, wider pillars, irregular depth spacing and tall end posts, so it has mass instead of a sliver.
float zbw_hv() { float2 d = zbw_dir(); return abs(d.x); }
float zbw_s(float i) { float K = zbw_k(); return ((i + 0.5) / K - 0.5) * _Length + (zbw_r(i, 0.0) - 0.5) * (1.6 + 1.6 * zbw_hv()); }
float2 zbw_base(float i)
{
    float2 d = zbw_dir(); float2 n = float2(-d.y, d.x);
    float s = zbw_s(i); float a = (zmod(i, 2.0) < 0.5 ? -1.0 : 1.0) * (1.3 + zbw_r(i, 1.0) * 1.5) * (1.0 + 2.1 * zbw_hv());   // zig-zag ranks across the thickness (v4c: +-4..9 px on a vertical wall)
    return float2(n.x * s + d.x * a, n.y * s + d.y * a);
}
float zbw_e(float i) { return 1.0 - zsat(abs(zbw_s(i)) / (_Length * 0.5)); }      // 1 centre .. 0 ends
float zbw_type(float i) { return zmod(i * 2.0 + floor(zbw_r(i, 3.0) * 1.99) + floor(_Seed * 7.0), 3.0); }   // 0 tusk 1 skull 2 ribs
float zbw_H(float i)
{
    // v4c (HM: the wall must tower over the imps, 36-48 px): a dense jagged row. Even pillars are the main spikes (34-48 px),
    // odd pillars are the secondary spikes between them (27-41 px), so the top line is saw-toothed.
    float t = zbw_type(i); float e = zbw_e(i);
    float mid = 38.0 + 8.0 * sqrt(e);                        // across-screen wall: tallest in the middle
    float ends = 39.0 + 7.0 * zsat((1.0 - e) * 1.6 - 0.3);   // vertical wall: the end posts tower highest
    float hv = zbw_hv();
    float jag = zmod(i, 2.0) < 0.5 ? 0.0 : -7.0;
    return floor(mid * (1.0 - hv) + ends * hv + jag + (zbw_r(i, 2.0) - 0.5) * 5.0 + (t < 0.5 ? 2.0 : 0.0));
}
float zbw_side(float i) { return zbw_r(i, 4.0) < 0.5 ? -1.0 : 1.0; }
float zbw_f0(float i) { float K = zbw_k(); return floor(abs(i - (K - 1.0) * 0.5) * 0.34 + 0.25); }   // rise frame: centre first
float zbw_rise(float i, float F)
{
    float k = F - zbw_f0(i);
    return k < -0.5 ? 0.0 : (k < 0.5 ? 0.55 : (k < 1.5 ? 1.12 : 1.0));
}
// break frame 0..4 (-1 standing). _Break 0.05..1 maps the code's 0.3 s
float zbw_bf()
{
    float r = -1.0;
    if (_Break > 0.001) r = min(4.0, floor(zsat((_Break - 0.05) / 0.95) * 4.5));
    return r;
}
float zbw_tb() { return zsat((_Break - 0.05) / 0.95) * 0.3; }

// one bone PILLAR seen in 3/4. q = px from its base (y up), H height, w half width at the base, lean = tip x offset,
// ty 0 tusk / 1 skull-capped spine / 2 spine with two hooked ribs; c1 c2 cracks; sn snapped top; sk skull still on.
// -> facet -1 none, 0 ink, 1 notch, 2 shadow, 3 mid, 4 lit, 5 ridge
float zbw_pillar(float2 q, float H, float w, float lean, float ty, float c1, float c2, float sn, float sk, float seed)
{
    float res = -1.0;
    float Hc = (ty > 0.5 && ty < 1.5) ? H - 5.0 : H;
    float v = zsat(q.y / max(Hc, 1.0));
    float cx = lean * v * v;
    float x = q.x - cx;
    // ribs (behind the column): two quarter arcs hooking up and outward
    if (ty > 1.5)
    {
        for (int k = 0; k < 2; k++)
        {
            float fk = float(k);
            if (sn < 0.5 || fk < 0.5)
            {
                float y0 = floor(Hc * (0.52 + 0.24 * fk));
                float sd = (fk < 0.5 ? 1.0 : -1.0) * (seed < 0.5 ? 1.0 : -1.0);
                float R = (5.0 - 1.0 * fk) * clamp(Hc / 26.0, 1.0, 1.5);
                float vv = zsat(y0 / max(Hc, 1.0));
                float2 c = float2(lean * vv * vv + sd * (R + 1.0), y0);
                float2 rl = q - c;
                float dd = length(rl);
                if (rl.y >= -0.5 && rl.x * sd <= 0.4)
                {
                    float th = 0.95 - 0.5 * zsat(rl.y / R);
                    float e = abs(dd - R);
                    if (e <= th) res = (rl.y > R * 0.45 || dd < R) ? 4.0 : 3.0;
                    else if (e <= th + 1.0 && rl.y <= R + 0.5) res = 0.0;
                }
            }
        }
    }
    // v4c: barbs. Two short hooked spurs per pillar (alternating sides, pointing up and out) make the row read jagged
    for (int k2 = 0; k2 < 2; k2++)
    {
        float fk2 = float(k2);
        float yb0 = floor(Hc * (0.34 + 0.27 * fk2 + 0.08 * seed));
        if ((sn < 0.5 || fk2 < 0.5) && Hc > 14.0)
        {
            float sd2 = (fk2 < 0.5 ? 1.0 : -1.0) * (seed < 0.5 ? 1.0 : -1.0);
            float vb = zsat(yb0 / max(Hc, 1.0));
            float wb = ty < 0.5 ? w * (1.0 - vb) : w * (1.0 - 0.62 * vb);
            float2 a0 = float2(lean * vb * vb + sd2 * (wb - 0.5), yb0);
            float lb = 4.0 + 2.0 * zhash(float2(seed * 13.0, fk2 + 1.0));
            float2 a1 = a0 + float2(sd2 * lb * 0.8, lb * 0.75);
            float db = zsegd(q, a0, a1);
            float tt = zsat(dot(q - a0, a1 - a0) / dot(a1 - a0, a1 - a0));
            float thb = 1.1 - 0.6 * tt;
            if (db <= thb) res = q.y > a0.y + (a1.y - a0.y) * tt ? 4.0 : 3.0;   // upper face lit
            else if (db <= thb + 1.0 && res < -0.5) res = 0.0;
        }
    }
    float topx = Hc;
    if (sn > 0.5) topx = floor(Hc * 0.68) + floor((zhash(float2(floor(x + 9.0), seed * 11.0 + 2.0)) - 0.5) * 2.6);
    float taper = ty < 0.5 ? 1.0 : (ty > 1.5 ? 0.72 : 0.5);   // v4c: spines taper to spikes
    float hw = ty < 0.5 ? w * (1.0 - v) * (1.0 - v * 0.25) + 0.15 : w * (1.0 - taper * v) + 0.35;
    float ph = zmod(q.y + floor(seed * 5.0), 5.0);
    if (ty > 0.5 && ph < 1.0 && v > 0.1 && v < 0.85) hw = hw + 0.6;                // vertebra disc (spines only; tusks are smooth)
    float rimR = 2.0;
    if (q.y >= -1.0 && q.y <= topx + 1.0 && x >= -hw - 1.0 && x <= hw + rimR)
    {
        if (x < -hw || x > hw || q.y > topx) res = 0.0;
        else
        {
            float rl2 = -hw * 0.35;
            res = x < rl2 ? 4.0 : (x < rl2 + max(1.0, hw * 0.6) ? 3.0 : 2.0);
            if (abs(x - (rl2 - 0.7)) < 0.5 && v > 0.12 && v < 0.85 && ty > 0.5) res = 5.0;    // ridge glint
            if (ty < 0.5 && abs(x - rl2 * 0.5) < 0.5 && v > 0.2 && v < 0.75) res = 5.0;
            if (ty > 0.5 && ph >= 1.0 && ph < 2.0 && x > rl2 && v > 0.1 && v < 0.85) res = 1.0;   // seam notch, shadow side
            if (ty < 0.5 && x > rl2 + 1.0 && zmod(q.y + floor(seed * 3.0), 7.0) < 1.0 && v > 0.15 && v < 0.7) res = 1.0;   // tusk growth line
            if (sn > 0.5 && q.y > topx - 1.0) res = 2.0;                                         // broken face
            if (c1 > 0.5 && v > 0.22 && v < 0.88)
            {
                float jx = (zN(_NoiseTex, float2(seed * 3.1 + 0.2, q.y / 6.0)).r - 0.5) * 4.0;
                if (abs(x - jx) < 0.55) res = 0.3;      // crack ink (0.3 = ink, flagged for the break glint)
            }
            if (c2 > 0.5)
            {
                float yc = floor(Hc * 0.52) + floor((zN(_NoiseTex, float2(x / 5.0, seed * 2.3 + 0.6)).r - 0.5) * 3.0);
                if (abs(q.y - yc) < 0.55) res = 0.3;
                else if (abs(q.y - yc + 1.0) < 0.55 && res > 2.5) res = 2.0;
            }
        }
    }
    // skull cap
    if (ty > 0.5 && ty < 1.5 && sk > 0.5 && sn < 0.5)
    {
        float2 r = q - float2(lean, Hc + 2.4);
        float ex = r.x / 3.4, ey = r.y / 3.0;
        float ex1 = r.x / (r.x > 0.0 ? 5.4 : 4.4), ey1 = r.y / 4.0;
        bool jaw = abs(r.x) < 2.3 && r.y >= -4.2 && r.y < -1.5;
        bool jaw1 = abs(r.x) < (r.x > 0.0 ? 4.3 : 3.3) && r.y >= -5.2 && r.y < -1.0;
        if (ex * ex + ey * ey <= 1.0 || jaw)
        {
            res = (r.x < -0.6 && r.y > -0.8) ? 4.0 : ((r.x > 1.3 || r.y < -1.6) ? 2.0 : 3.0);
            if (abs(r.x + 1.6) < 0.6 && abs(r.y - 1.6) < 0.6) res = 5.0;
            if ((abs(r.x + 1.4) < 0.9 || abs(r.x - 1.4) < 0.9) && r.y > -1.1 && r.y < 0.6) res = 0.0;   // sockets
            if (abs(r.x) < 0.5 && r.y > -2.2 && r.y < -1.2) res = 1.0;                                  // nose
            if (jaw && abs(r.y + 3.0) < 0.5 && zmod(floor(r.x + 8.0), 2.0) < 1.0) res = 1.0;            // teeth line
        }
        else if (ex1 * ex1 + ey1 * ey1 <= 1.0 || jaw1) res = (res > 0.5 && r.y < -3.0) ? res : 0.0;
    }
    return res;
}
// dirt BERM heaped at a pillar base. -> -1 none, earth index 0..4 (0 ink). front = 1 when the pixel is in the front half
float zbw_berm(float2 q, float2 p, float w, float big, float seed)
{
    float res = -1.0;
    float rx = w + 2.6 + big, ry = 2.2 + 0.5 * big;
    float2 r = q - float2(0.4, 0.3);
    float bump = (zN(_NoiseTex, p / 4.0 + float2(seed * 3.7, seed * 1.9)).r - 0.5) * 0.6;
    float e = (r.x / rx) * (r.x / rx) + (r.y / ry) * (r.y / ry) + bump;
    float rx1 = rx + 1.0, ry1 = ry + 1.0;
    float e1 = (r.x / rx1) * (r.x / rx1) + (r.y / ry1) * (r.y / ry1) + bump;
    if (e <= 1.0)
    {
        res = r.y > ry * 0.35 ? 4.0 : (r.y > -ry * 0.45 ? 3.0 : 2.0);
        if (r.x > rx * 0.45 && res > 3.5) res = 3.0;
        if (zN(_NoiseTex, p / 2.5 + float2(seed, 0.3)).g > 0.66 && res > 2.5) res = res - 1.0;   // clods
    }
    else if (e1 <= 1.0 && r.y < ry * 0.6) res = 0.0;
    return res;
}
// pixel-art dirt ball lit from the upper left (as Ossuary Nova); age shrinks it and bites holes
float4 zbw_ball(float2 p, float2 q, float r, float age, float seed)
{
    float4 c = zk_none();
    float rr = r * (1.0 - 0.4 * age);
    float d = length(q);
    float bite = zN(_NoiseTex, p / 3.0 + float2(seed * 5.3, seed * 2.1)).r;
    if (rr >= 1.2 && d < rr && bite > age * 1.1 - 0.15)
    {
        float2 nn = q / max(rr, 1.0);
        float l = dot(nn, float2(-0.6, 0.8)) + zbayer(p) * 0.3;
        float i = l > 0.3 ? 4.0 : (l > -0.35 ? 3.0 : 2.0);
        if (d > rr - 1.0 && l < -0.2) i = 0.0;
        c = float4(zramp(_RampTex, 1.0, i), 1.0);
    }
    return c;
}
// bone piece from the atlas. q = pixel centre - piece centre. -> level 0 / .25 ink / .5 / .75 / 1
float zbw_piece(float2 q, float kind, float rot)
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
float zbw_lv2f(float lv) { return lv < 0.375 ? 0.0 : (lv < 0.625 ? 2.0 : (lv < 0.875 ? 3.0 : 4.0)); }
// dust puff at a base: clumpy, ground-hugging, never a flat ellipse. -> -1 none or dust index
float zbw_dust(float2 q, float2 p, float rad, float dens, float seed)
{
    float res = -1.0;
    float2 r = q - float2(0.0, 1.0);
    float e = (r.x / rad) * (r.x / rad) + (r.y / (rad * 0.55)) * (r.y / (rad * 0.55));
    if (e < 1.0 && rad > 1.0)
    {
        float nz = zN(_NoiseTex, p / 3.5 + float2(seed * 4.1, seed * 2.7)).r;
        if (nz * (1.15 - e * 0.6) > 1.0 - dens)
        {
            float l = r.y - r.x * 0.5;
            res = l > rad * 0.15 ? 5.0 : (l > -rad * 0.2 ? 4.0 : 3.0);
        }
    }
    return res;
}

float4 zbw_wall(float2 p, float2 w, float F, float glow)
{
    float4 c = zk_none();
    float2 pw = p + _WallOff.xy;
    float2 d = zbw_dir();
    float K = zbw_k();
    float bf = zbw_bf(); float tb = zbw_tb();
    float bestD = 99999.0; float4 best = zk_none(); float bestKey = -1.0;
    float4 over = zk_none();
    float4 dust = zk_none();
    float ao = 0.0;
    float gfield = 0.0;
    float dmg = _Damage;
    for (int ii = 0; ii < 13; ii++)
    {
        float fi = float(ii);
        if (fi < K - 0.5 && fi >= _Part.x - 0.01 && fi <= _Part.y + 0.01)
        {
            float2 b = zbw_base(fi);
            float2 q = pw - b;
            float sc = zbw_rise(fi, F);
            float kf = F - zbw_f0(fi);
            float H0 = zbw_H(fi);
            float ty = zbw_type(fi);
            float wd = ((ty < 0.5 ? 3.0 : 2.4) + zbw_r(fi, 5.0) * 0.5) * (1.0 + 0.4 * zbw_hv());
            float sd = zbw_side(fi);
            float sdr = zbw_r(fi, 6.0);
            float e = zbw_e(fi);
            float alt = zmod(fi, 2.0) < 0.5 ? -1.0 : 1.0;
            float lean = alt * (1.0 + 1.8 * zbw_r(fi, 7.0)) * (1.0 + 1.3 * zbw_hv()) + sd * 1.6 * (1.0 - e);   // v4b: the ranks give a vertical wall its width; lean only a little (v4a leaned +1.8 and fanned out)
            if (ty < 0.5) lean = lean + alt * 1.2;
            float c1 = dmg > 0.04 + 0.62 * zbw_r(fi, 8.0) ? 1.0 : 0.0;
            float c2 = dmg > 0.30 + 0.60 * zbw_r(fi, 9.0) ? 1.0 : 0.0;
            float sn = dmg > 0.35 + 0.55 * zbw_r(fi, 10.0) ? 1.0 : 0.0;
            float sk = dmg > 0.50 + 0.45 * zbw_r(fi, 11.0) ? 0.0 : 1.0;
            if (dmg > 0.86) lean = lean + sd * 1.0;
            if (bf > -0.5) { c1 = 1.0; c2 = 1.0; }      // breaking: cracks everywhere (snaps / skulls follow _Damage, 1 with the current hook)
            if (abs(q.x) < 18.0 && q.y > -8.0 && q.y < 62.0)
            {
                float H = floor(H0 * sc);
                float fc = -1.0;
                float bermBig = kf < 0.5 ? 1.2 : 0.0;
                float showBerm = 1.0;
                if (sc > 0.01 && bf < 0.5)
                {
                    fc = zbw_pillar(q, H, wd, lean * sc, ty, c1, c2, sn, sk, sdr);
                    if (glow > 0.5 && bf > -0.5 && fc > 0.2 && fc < 0.5 && q.y > 2.0) gfield = max(gfield, 0.85);   // crack glint
                }
                else if (sc > 0.01 && bf < 2.5)
                {
                    // split in 3 chunks that slide off and drop; the bottom chunk sinks
                    float k1 = bf < 1.5 ? 1.0 : 2.6;
                    float yb = floor(H * 0.38), ym = floor(H * 0.70);
                    float2 o0 = float2(0.0, -2.0 * (k1 - 1.0));
                    float2 o1 = float2(sd * 1.0 * k1, -1.5 * k1);
                    float2 o2 = float2(sd * 2.0 * k1, -3.5 * k1 * k1 * 0.6);
                    float f0 = zbw_pillar(q - o0, H, wd, lean, ty, 1.0, 1.0, sn, sk, sdr);
                    if (f0 > -0.5 && (q - o0).y < yb && q.y >= -0.5) fc = f0;
                    float f1 = zbw_pillar(q - o1, H, wd, lean, ty, 1.0, 1.0, sn, sk, sdr);
                    if (f1 > -0.5 && (q - o1).y >= yb && (q - o1).y < ym && q.y >= 0.0) fc = ((q - o1).y < yb + 1.0) ? 0.0 : f1;
                    float f2 = zbw_pillar(q - o2, H, wd, lean, ty, 1.0, 1.0, sn, sk, sdr);
                    if (f2 > -0.5 && (q - o2).y >= ym && q.y >= 0.0) fc = ((q - o2).y < ym + 1.0) ? 0.0 : f2;
                    bermBig = 0.6;
                }
                else if (sc > 0.01)
                {
                    bermBig = 1.6 - (bf - 3.0) * 0.8;
                }
                // berm + pillar compose (berm front half covers the column foot)
                float bm = showBerm > 0.5 && sc > 0.01 ? zbw_berm(q, pw, wd, bermBig, sdr) : -1.0;
                float4 col = zk_none(); float key = -1.0;
                if (fc > -0.5) { col = zbw_bone(fc); key = fc; }
                if (bm > -0.5 && (fc < -0.5 || q.y < 1.0)) { col = zbw_earth(bm); key = 10.0 + bm; }
                // rubble heap after the fall
                if (bf > 2.5)
                {
                    float diss = zsat((_Fade - 0.55) / 0.45);
                    {
                        float fm = 0.0;
                        float kind = (ty > 0.5 && ty < 1.5 && sk > 0.5) ? 0.0 : (ty < 0.5 ? 6.0 : 3.0);
                        float2 rc = float2(sd * 1.5, 3.0 - (bf - 3.0) * 1.5);
                        float lv = zbw_piece(q - rc, kind, zmod(fi + fm * 3.0 + floor(_Seed * 8.0), 8.0));
                        if (lv > 0.1 && q.y >= 0.0 && zN(_NoiseTex, pw / 3.0 + float2(fi * 0.37, fm)).r > diss * 1.1 - 0.05) { col = zbw_bone(zbw_lv2f(lv)); key = 20.0; }
                    }
                }
                if (col.a > 0.5 && b.y < bestD) { bestD = b.y; best = col; bestKey = key; }
                // tight contact shadow under the berm (ground contact even without the ground quad)
                if (sc > 0.01 && bf < 3.5)
                {
                    float2 ra = q - float2(1.6, -0.9);
                    float rxa = wd + 4.2, rya = 3.3;
                    if ((ra.x / rxa) * (ra.x / rxa) + (ra.y / rya) * (ra.y / rya) <= 1.0) ao = 1.0;
                }
                // dust at the base: rise k 0..2, break 2..4
                float dk = -1.0;
                if (kf > -0.5 && kf < 2.5) dk = kf;
                if (bf > 1.5) dk = bf - 2.0 + 0.5;
                if (dk > -0.5)
                {
                    float dr = zbw_dust(q - float2(sd * 1.0, 0.0), pw, 4.0 + dk * 2.4, 0.55 - dk * 0.2, sdr + fi);
                    if (dr > -0.5 && dust.a < 0.5) dust = zbw_dustc(dr);
                }
                // base flash on the glow twin (f0, capped)
                if (glow > 0.5 && kf > -0.5 && kf < 1.5)
                {
                    float2 rg = q - float2(0.0, 3.0);
                    float eg = (rg.x / 7.0) * (rg.x / 7.0) + (rg.y / 6.0) * (rg.y / 6.0);
                    if (eg < 1.0) gfield = max(gfield, (1.0 - eg) * (kf < 0.5 ? 1.0 : 0.45));
                }
            }
            // dirt clods thrown both ways across the wall on the rise (about 12 per wall)
            for (int j = 0; j < 2; j++)
            {
                float fj = float(j);
                float hr = zbw_r(fi, 12.0 + fj);
                if (hr < 0.68)
                {
                    float t = (F - zbw_f0(fi)) / 15.0 + 0.033;
                    if (t > 0.03 && t < 0.42)
                    {
                        float sj = (fj < 0.5 ? 1.0 : -1.0) * (zbw_r(fi, 14.0) < 0.5 ? 1.0 : -1.0);
                        float vx = 28.0 + 26.0 * hr, vy = 70.0 + 40.0 * zbw_r(fi, 15.0 + fj);
                        float along = (zbw_r(fi, 17.0 + fj) - 0.5) * 30.0;
                        float2 n2 = float2(-d.y, d.x);
                        float2 g = float2(d.x * sj * vx * t + n2.x * along * t, d.y * sj * vx * t + n2.y * along * t);
                        float hgt = 3.0 + vy * t - 260.0 * t * t;
                        if (hgt > -1.0)
                        {
                            float2 cp = b + g + float2(0.0, hgt);
                            float4 bc = zbw_ball(pw, pw - floor(cp) - float2(0.5, 0.5), 1.8 + hr * 0.9, zsat(t / 0.42), hr);
                            if (bc.a > 0.5) over = bc;
                        }
                    }
                }
            }
            // break chips: one bone chip per pillar flicks off (frames 1..3)
            if (bf > 0.5 && bf < 3.5 && sc > 0.01 && zmod(fi, 2.0) < 0.5)
            {
                float t = tb - 0.06;
                float2 cp = b + float2(sd * (18.0 + 14.0 * sdr) * t + sd * 2.0, floor(zbw_H(fi) * 0.75) + 50.0 * t - 380.0 * t * t);
                if (cp.y > b.y - 1.0)
                {
                    float lv = zbw_piece(pw - floor(cp) - float2(0.5, 0.5), zbw_r(fi, 19.0) < 0.5 ? 4.0 : 6.0, zmod(floor(tb * 15.0) + fi, 8.0));
                    if (lv > 0.1) over = zbw_bone(zbw_lv2f(lv));
                }
            }
        }
    }
    if (glow > 0.5)
    {
        c = zk_none();
        float gf = min(gfield, _GlowCap);
        if (gf > 0.04) c = zk_glow(_RampTex, gf, w, _GlowGain, 2.0);
    }
    else
    {
        if (ao > 0.5 && zdithS(0.9, w, 0.4)) c = zbw_earth(0.0);
        if (dust.a > 0.5) c = dust;
        if (best.a > 0.5) c = best;
        if (dust.a > 0.5 && bestKey > 9.5 && bestKey < 19.5) c = dust;                // dust rolls over the berms
        if (over.a > 0.5) c = over;
        if (_Fade > 0.001 && c.a > 0.5 && bestKey < 19.5 && zN(_NoiseTex, w / 4.0 + float2(_Seed * 3.0, 0.7)).g < _Fade * 1.2 - 0.25) c = zk_none();
    }
    return c;
}

float4 zbw_ground(float2 p, float2 w, float F, float glow)
{
    float4 c = zk_none();
    float2 pw = p + _WallOff.xy;
    float2 d = zbw_dir(); float2 n = float2(-d.y, d.x);
    float s = dot(pw, n), a = dot(pw, d);
    float K = zbw_k();
    float bf = zbw_bf();
    float half_ = _Length * 0.5;
    float grow = F < 0.5 ? 0.45 : (F < 1.5 ? 0.8 : 1.0);
    float diss = zsat((_Fade - 0.3) / 0.7);
    if (glow < 0.5)
    {
        // cast shadows: a slanted capsule per pillar toward the lower right (light from the upper left), dithered ink
        float sh = 0.0;
        for (int ii = 0; ii < 13; ii++)
        {
            float fi = float(ii);
            if (fi < K - 0.5)
            {
                float2 b = zbw_base(fi);
                float sc = zbw_rise(fi, F);
                if (bf > 0.5) sc = sc * (bf < 1.5 ? 0.8 : (bf < 2.5 ? 0.45 : 0.2));
                float H = zbw_H(fi) * sc;
                if (H > 1.0)
                {
                    float2 q = pw - b;
                    float hv = zbw_hv();
                    float dd = zsegd(q, float2(1.0, -1.0), float2(1.0 + H * (0.42 + 0.18 * hv), -1.0 - H * 0.22));
                    float rr = 2.6 + 1.2 * hv;
                    if (dd < rr) sh = max(sh, dd < rr - 1.0 ? 1.0 : 0.6);
                }
            }
        }
        if (sh > 0.0 && zdithS(sh * 0.85, w, 0.5)) c = zbw_earth(0.0);
        // the trench the wall rose out of: 2 px ink split with a lit far lip, running a little past both ends
        float ext = (half_ + 5.0) * grow;
        if (abs(s) <= ext)
        {
            float j = (zN(_NoiseTex, float2(s / 9.0, _Seed * 3.0 + 0.1)).r - 0.5) * 3.0 + (zN(_NoiseTex, float2(s / 3.0, _Seed * 3.0 + 0.6)).g - 0.5) * 1.2;
            float hv = zbw_hv();
            float wdt = abs(s) > half_ ? 0.6 : 1.1 + 0.6 * hv;
            float ee = a - j;
            // v4b: heaved dirt lips both sides of the split (broken by noise), so the footprint reads past the pillars
            float lip = 2.0 + 1.5 * hv;
            float ae = abs(ee);
            if (abs(s) <= half_ + 2.0 && ae >= wdt + 1.0 && ae < wdt + 1.0 + lip && zN(_NoiseTex, pw / 3.0 + float2(_Seed * 5.0, 0.4)).r > 0.38)
            {
                float lit = (ee * (d.x * -0.6 + d.y * 0.8)) > 0.0 ? 1.0 : 0.0;
                c = zbw_earth(ae > wdt + lip ? 0.0 : (lit > 0.5 ? 4.0 : 3.0));
            }
            if (abs(ee) < wdt) c = zbw_earth(0.0);
            else if (ee >= wdt && ee < wdt + 1.0) c = zbw_earth(abs(s) > half_ ? 3.0 : 4.0);
            else if (ee <= -wdt && ee > -wdt - 1.0) c = zbw_earth(2.0);
        }
        // side cracks: 6 short jagged cracks branching off the trench, growing f0..f2
        for (int k = 0; k < 6; k++)
        {
            float fk = float(k);
            float hk = zhash(float2(fk * 5.1 + 2.0, _Seed * 41.0));
            float sk = ((fk + 0.5) / 6.0 - 0.5) * _Length * 1.05 + (hk - 0.5) * 5.0;
            float side = zmod(fk, 2.0) < 0.5 ? 1.0 : -1.0;
            float ang = (hk - 0.5) * 1.0;
            float2 dir2 = float2(d.x * cos(ang) - d.y * sin(ang), d.x * sin(ang) + d.y * cos(ang)) * side;
            float len = (5.0 + 5.0 * zhash(float2(fk, _Seed * 7.0 + 1.0))) * grow;
            float2 o = n * sk + d * side * 1.5;
            float2 q = pw - o;
            float al = dot(q, dir2);
            float pr = dot(q, float2(-dir2.y, dir2.x));
            if (al > 0.0 && al < len)
            {
                float jj = (zN(_NoiseTex, float2(al / 4.0, fk * 0.31 + _Seed)).r - 0.5) * 2.4;
                if (abs(pr - jj) < 0.55) c = zbw_earth(0.0);
                else if (pr - jj >= 0.55 && pr - jj < 1.4 && al < len * 0.7) c = zbw_earth(3.0);
            }
        }
        // heave: a few lifted dirt clods along both sides (restrained), plus the snapped tips lying on the ground
        float2 cl = floor(pw / 4.0);
        float hc = zhash(cl + float2(_Seed * 19.0, 3.0));
        if (abs(a) > 2.5 && abs(a) < 8.0 && abs(s) < half_ * grow && hc > 0.82)
        {
            float2 qc = pw - (cl * 4.0 + float2(2.0, 2.0));
            float dc = length(qc);
            if (dc < 1.6) c = zbw_earth(qc.y - qc.x * 0.5 > 0.0 ? 4.0 : 3.0);
            else if (dc < 2.3 && qc.y < 0.5) c = zbw_earth(0.0);
        }
        for (int jj = 0; jj < 13; jj++)
        {
            float fi = float(jj);
            if (fi < K - 0.5 && _Damage > 0.35 + 0.55 * zbw_r(fi, 10.0))
            {
                float2 b = zbw_base(fi);
                float sd = zbw_side(fi);
                float2 cp = b + d * sd * 5.5 + n * (zbw_r(fi, 20.0) - 0.5) * 4.0;
                float lv = zbw_piece(pw - floor(cp) - float2(0.5, 0.5), 4.0, zmod(fi * 3.0, 8.0));
                if (lv > 0.1) c = zbw_bone(zbw_lv2f(lv));
            }
        }
        if (c.a > 0.5 && diss > 0.0 && zN(_NoiseTex, w / 5.0 + float2(_Seed, 0.2)).r < diss * 1.1 - 0.05) c = zk_none();
    }
    return c;
}

float4 ZFX_Frag(float2 p, float2 w)
{
    float F = zbw_t(_FxTime);
    float glow = _Layer > 0.5 ? 1.0 : 0.0;
    float4 c = zk_none();
    if (_Mode < 0.5) c = zbw_ground(p, w, F, glow);
    else c = zbw_wall(p, w, F, glow);
    if (_Intensity < 0.999 && c.a > 0.5 && glow < 0.5 && zbayer(w) > _Intensity - 0.5) c = zk_none();
    if (glow > 0.5) c = float4(c.rgb * _Intensity, c.a);
    return c;
}
