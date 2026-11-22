// Fire Hydra v2 (t1630): same pool / timing / API as ZhurongFX_Hydra (v1 kept), but the heads are a PixelLab-sourced
// dragon head sprite (_HeadTex, cleaned to Endesga 32) placed on each neck tip: pixel-snapped, flipped/rotated by aim,
// lit by fire (molten throat + scales flicker, under-rim heat, crest licks, eye glow), UV-hinged jaw that opens HOT.
// Necks: thicker, textured with the PixelLab neck strip (dark scaled back, molten belly), tapering into the throat.
// Fire Hydra (t1625, replaces Volcanic Orb). D2/D2R Hydra read: three serpent heads on S-curved necks rising from a
// molten pool, swaying and breathing, each spitting fire bolts on its own clock. Everything here is procedural (no sprite):
// crusted rock rim + lava pool + base tongues, ink-rimmed scaled necks with a hot belly and a flame mane, dragon heads with
// a dark crown, glowing eye and a hinged jaw, per-head windup (pull back, heat gulp travels up the neck, ember in the jaw),
// lunge + muzzle flash along the true aim angle, recoil. Summon: cracks spread, pool erupts, heads rise white-hot.
// Fade: heads sink and burn away top-down with a bright erosion lip, pool cools to crust.
// Pivot = ground point at the centre of the base. Code drives _Rise, _Fade, _SpitTime.xyz, _AimAngle.xyz (see spec.md).
float zh_pick(float4 v, float i) { return (i < 0.5) ? v.x : ((i < 1.5) ? v.y : v.z); }
float2 zh_basePt(float i) { return (i < 0.5) ? float2(-5.0, 3.0) : ((i < 1.5) ? float2(0.0, 4.0) : float2(5.0, 3.0)); }
float2 zh_restPt(float i) { return (i < 0.5) ? float2(-15.0, 17.0) : ((i < 1.5) ? float2(1.0, 25.0) : float2(15.0, 19.0)); }
// pose of head i at stepped time t. H = head centre, hd = facing axis (local +x), yax = local up, r = rise 0..1
void zh_pose(float i, float t, out float2 H, out float2 hd, out float2 yax, out float jaw, out float charge, out float flash, out float2 ad, out float r, out float wu)
{
    float W = _Windup;
    float s = t - zh_pick(_SpitTime, i);
    float aim = zh_pick(_AimAngle, i);
    ad = float2(cos(aim), sin(aim));
    float face = (ad.x >= 0.0) ? 1.0 : -1.0;
    float tilt = floor(clamp(atan2(ad.y, abs(ad.x)), -0.6, 0.4) / 0.2 + 0.5) * 0.2;   // quantized: stable sprite rotation
    hd = float2(face * cos(tilt), sin(tilt));
    yax = float2(-hd.y, hd.x) * face;
    float st = (i < 0.5) ? 0.08 : ((i < 1.5) ? 0.0 : 0.15);
    float rr = zeaseOut(zsat((_Rise - 0.3 - st) / 0.5));
    r = rr * (1.0 - 0.55 * zeaseIn(zsat(_Fade / 0.8)));
    float2 B = zh_basePt(i);
    float2 sway = float2(sin(t * 1.6 + i * 2.1) * 1.5, sin(t * 2.2 + i * 1.3));
    H = B + (zh_restPt(i) - B) * r + sway * r;
    wu = (s >= 0.0 && s < W) ? s / W : 0.0;
    float lg = 0.0;
    if (s >= W && s < W + 0.1) lg = 1.0;
    else if (s >= W + 0.1 && s < W + 0.45) lg = 1.0 - zeaseOut((s - W - 0.1) / 0.35);
    H = H - hd * (3.0 * zeaseOut(wu)) + float2(0.0, 1.0) * wu + hd * (4.0 * lg);
    H = floor(H + float2(0.5, 0.5));   // whole art px: the head sprite never shimmers
    float jo = (s >= W && s < W + 0.3) ? 1.0 - zeaseIn((s - W) / 0.3) : 0.0;
    jaw = max(wu * 0.35, jo);
    charge = max(wu, (s >= W && s < W + 0.2) ? 1.0 - (s - W) / 0.2 : 0.0);
    flash = (s >= W && s < W + 0.15) ? 1.0 - (s - W) / 0.15 : 0.0;
}
float2 zh_bez(float2 a, float2 b, float2 c, float2 d, float u)
{
    float v = 1.0 - u;
    return a * (v * v * v) + b * (3.0 * v * v * u) + c * (3.0 * v * u * u) + d * (u * u * u);
}
// head sprite cell: 18x16 at atlas (0,16)-(18,32) in uv space (y up), facing +x. Local origin = sprite px (10, 7).
float4 zh_spr(float2 f)
{
    float2 sp = floor(f + float2(10.0, 7.0));
    if (sp.x < 0.0 || sp.y < 0.0 || sp.x > 17.0 || sp.y > 15.0) return float4(0.0, 0.0, 0.0, 0.0);
    return ZTEX(_HeadTex, float2((sp.x + 0.5) / 32.0, (16.0 + sp.y + 0.5) / 32.0));
}
// sprite lookup with the UV-hinged lower jaw (hinge local (-1,-2.5), mouth line y = -2.5); returns a = 1 if covered,
// sets mouthGap = 1 inside the opened mouth wedge
float4 zh_jawSpr(float2 f, float ja, out float mouthGap)
{
    mouthGap = 0.0;
    float2 hinge = float2(-1.0, -2.5);
    float4 up = (f.y >= -2.5) ? zh_spr(f) : float4(0.0, 0.0, 0.0, 0.0);
    float2 fl = zrot(f - hinge, ja) + hinge;
    float4 lo = (fl.y < -2.5 && f.x > hinge.x - 0.5) ? zh_spr(fl) : float4(0.0, 0.0, 0.0, 0.0);
    if (f.y < -2.5 && f.x <= hinge.x - 0.5) lo = zh_spr(f);   // back of the jaw / throat stays put
    float2 fm = f - hinge;
    float am = atan2(fm.y, max(fm.x, 1e-3));
    if (ja > 0.04 && fm.x > 0.0 && fm.x < 8.5 && am < 0.0 && am > -ja) mouthGap = 1.0;
    if (up.a > 0.5) return up;
    return lo;
}
float4 zh_head(float2 p, float2 w, float i, float t, out float mane, out float glow, out float body)
{
    float2 H; float2 hd; float2 yax; float jaw; float charge; float flash; float2 ad; float r; float wu;
    zh_pose(i, t, H, hd, yax, jaw, charge, flash, ad, r, wu);
    float4 col = float4(0.0, 0.0, 0.0, 0.0);
    mane = 0.0; glow = 0.0; body = 0.0;
    if (r < 0.02) return col;
    float2 A = H - hd * 4.0 - yax * 6.0;                                   // throat attach point
    float2 P0 = zh_basePt(i);
    float2 P1 = P0 + float2(0.0, 8.0 * r);
    float2 P2 = A - (hd * 2.0 + yax * 6.0) * min(1.0, r * 1.5);
    float2 P3 = A;
    float dmin = 99.0; float umin = 0.0; float2 cmin = P0; float2 seg = float2(0.0, 1.0);
    float2 prev = P0;
    for (int k = 1; k < 9; k++)
    {
        float u1 = float(k) / 8.0;
        float2 nx = zh_bez(P0, P1, P2, P3, u1);
        float2 ab = nx - prev;
        float h = zsat(dot(p - prev, ab) / max(1e-4, dot(ab, ab)));
        float2 c = prev + ab * h;
        float dd = length(p - c);
        if (dd < dmin) { dmin = dd; umin = (float(k) - 1.0 + h) / 8.0; cmin = c; seg = ab; }
        prev = nx;
    }
    float hw = lerp(3.7, 2.6, umin);
    float breath = 0.5 + 0.5 * sin(t * 2.6 + i * 1.9);
    float gulp = (wu > 0.0) ? exp(-(umin - wu * 1.1) * (umin - wu * 1.1) / 0.02) : 0.0;
    float emerge = (1.0 - r) * 0.6;
    float fl = zflow(_NoiseTex, p + float2(i * 23.0 + _Seed * 50.0, 0.0), 7.0, t, 10.0, _Seed);
    float dn = zN(_NoiseTex, p / 5.0 + float2(_Seed + i * 0.3, 0.2)).r;
    float th = _Fade * 1.7 - (1.0 - zsat(p.y / 32.0)) * 0.6;
    bool gone = dn < th - 0.1;
    bool lip = dn < th;
    // ---- neck: PixelLab strip mapped along the curve (row 7 = back), ink rim, molten belly flicker
    if (dmin < hw && !gone && p.y > 4.0)
    {
        float2 perp = normalize(float2(-seg.y, seg.x) + float2(1e-4, 0.0));
        float2 bref = float2(0.0, 0.7) - hd * 0.7;
        if (dot(perp, bref) < 0.0) perp = -perp;
        float v = zsat(0.5 + 0.5 * dot(p - cmin, perp) / hw);
        float along = umin * 34.0;
        float4 nc = ZTEX(_HeadTex, float2((floor(zmod(along, 24.0)) + 0.5) / 32.0, (floor(v * 7.99) + 0.5) / 32.0));
        float lum = nc.r + nc.g + nc.b;
        col = float4(nc.rgb, 1.0);
        bool molten = nc.r > 0.55 && nc.b < 0.35;
        float chev = frac((along + abs(dot(p - cmin, perp)) * 1.2) / 3.0 + i * 0.37);
        if (v < 0.42)
        {
            // hot belly: smooth band, brightest at the centre line of the belly, banded scales
            float heat = 0.5 + (0.42 - v) * 0.6 + (fl - 0.5) * 0.12 + 0.06 * breath + gulp * 0.5 + emerge - ((chev < 0.25) ? 0.12 : 0.0);
            col = zfire(_RampTex, min(heat, 0.78 + gulp * 0.08), w, 0.0);
        }
        else
        {
            // dark scaled back from the PixelLab strip (its molten specks become sparse ember scales)
            col = molten ? float4(zramp(_RampTex, 0.0, (fl > 0.55) ? 3.0 : 2.0), 1.0) : float4(nc.rgb, 1.0);
            if (gulp > 0.3 || emerge > 0.2) col = zfire(_RampTex, min(0.3 + gulp * 0.4 + emerge, 0.74), w, 0.0);
        }
        if (dmin > hw - 0.9) col = float4(zramp(_RampTex, 0.0, 0.0), 1.0);
        if (lip) col = zfire(_RampTex, 0.78, w, 0.0);
        body = 1.0;
        glow = max(glow, (0.2 + 0.2 * breath + gulp * 0.6 + emerge) * (1.0 - v) * (1.0 - dmin / hw));
    }
    // ---- head sprite (mirrored by yax, rotated by the quantized tilt, jaw UV-hinged)
    float2 q = p - H;
    float2 f = float2(dot(q, hd), dot(q, yax));
    float ja = jaw * 0.5;
    float gap = 0.0;
    float4 hs = zh_jawSpr(f, ja, gap);
    if (hs.a > 0.5 && !gone)
    {
        float lum = hs.r + hs.g + hs.b;
        bool molten = hs.r > 0.55 && hs.b < 0.4;
        bool eye = molten && hs.g > 0.55 && f.y > -1.0 && f.y < 2.0 && f.x > -1.0 && f.x < 3.0;
        col = float4(hs.rgb, 1.0);
        if (eye)
        {
            col = float4(zramp(_RampTex, 0.0, (breath + charge > 0.9) ? 7.0 : 6.0), 1.0);
            glow = max(glow, 0.9 + charge);
        }
        else if (molten)
        {
            float heat = 0.5 + 0.2 * hs.g + (fl - 0.5) * 0.3 + 0.1 * breath + charge * 0.3 + emerge;
            col = zfire(_RampTex, heat, w, 0.0);
            glow = max(glow, 0.45 + charge * 0.5);
        }
        else
        {
            // fire under-light: dark scales with open space below glow hot (hot belly/jaw rim)
            float4 below = zh_spr(float2(dot(q - float2(0.0, 1.0), hd), dot(q - float2(0.0, 1.0), yax)));
            if (below.a < 0.5 && f.y < -1.5 && f.x > -1.0) col = float4(zramp(_RampTex, 0.0, (charge > 0.3) ? 4.0 : 2.0), 1.0);   // under-jaw fire rim
            if (emerge > 0.15) col = zfire(_RampTex, 0.4 + emerge, w, 0.0);
            glow = max(glow, 0.15 + emerge);
        }
        if (lip) col = zfire(_RampTex, 0.78, w, 0.0);
        body = 1.0;
    }
    else if (gap > 0.5 && !gone)
    {
        // HOT open mouth: throat glow, brightest deep in the jaw
        float2 fm = f - float2(-1.0, -2.5);
        col = zfire(_RampTex, 0.82 + 0.25 * charge - fm.x * 0.035 + (fl - 0.5) * 0.2, w, 0.0);
        if (fm.x > 6.5) col = float4(zramp(_RampTex, 0.0, 7.0), 1.0);   // teeth glint at the tip
        body = 1.0;
        glow = max(glow, 0.9);
    }
    // ember charging in the jaw during the windup
    float2 mo = H + hd * 6.0 - yax * 2.5;
    float ce = charge * (1.4 + 1.0 * charge) - length(p - mo);
    if (ce > 0.0 && !gone && body < 0.5) { col = zfire(_RampTex, 0.8 + ce * 0.3, w, 0.0); body = 1.0; glow = max(glow, charge); }
    if (ce > 0.0 && !gone && hs.a > 0.5 && f.y < -1.0) { col = zfire(_RampTex, 0.75 + ce * 0.3, w, 0.0); glow = max(glow, charge); }
    // crest licks: short tongues rising off the top of the head sprite (world up), dark lip
    if (body < 0.5 && !gone)
    {
        for (int k = 1; k < 6; k++)
        {
            float fk = float(k);
            float2 qk = q - float2(sin(t * 8.0 + fk + i * 2.0) * 0.4 * fk / 5.0, fk);
            float4 sk = zh_spr(float2(dot(qk, hd), dot(qk, yax)));
            if (sk.a > 0.5)
            {
                float Hk = (1.5 + 2.5 * zN(_NoiseTex, float2(p.x / 3.0 + i, t * 0.7)).g) * (0.8 + 0.4 * breath + emerge);
                mane = max(mane, (1.0 - fk / Hk) * (0.7 + 0.6 * fl) - 0.15);
                break;
            }
        }
        // neck mane: low licks along the back
        float2 perp2 = normalize(float2(-seg.y, seg.x) + float2(1e-4, 0.0));
        float2 bref2 = float2(0.0, 0.7) - hd * 0.7;
        if (dot(perp2, bref2) < 0.0) perp2 = -perp2;
        float md = (dmin - hw) / 2.5;
        if (dmin >= hw && md < 1.0 && dot(p - cmin, perp2) > 0.0 && umin > 0.3 && umin < 0.85)
            mane = max(mane, (1.0 - md) * (0.45 + 0.3 * breath) + (fl - 0.5) * 0.9 - 0.5);
    }
    mane *= r;
    return col;
}
// molten base: scorch clumps, crusted rock rim with molten cracks, lava pool, crack spread on summon, cooling on fade
float4 zh_base(float2 p, float2 w, float t, out float glow)
{
    glow = 0.0;
    float4 col = float4(0.0, 0.0, 0.0, 0.0);
    float2 e = float2(p.x / 13.0, (p.y - 1.5) / 4.6);
    float n1 = zN(_NoiseTex, p / 26.0 + float2(_Seed, 0.4)).r;
    float d = length(e) + (n1 - 0.5) * 0.35;
    float spread = zeaseOut(_Rise / 0.35);
    float cool = zsat(_Fade);
    float crack = zN(_NoiseTex, p / 22.0 + float2(_Seed * 0.7, 0.1)).a;
    // spreading ember cracks + char clumps on the floor around the base
    if (d > 0.85 && d < 1.0 + 0.75 * spread)
    {
        float far = (d - 0.85) / (0.15 + 0.75 * spread);
        if (crack < 0.08 && far < 0.95)
        {
            col = zfire(_RampTex, (0.75 - far * 0.45) * (1.0 - cool * 0.8) + (1.0 - spread) * 0.3, w, 0.15);
            if (col.a < 0.5) col = float4(zramp(_RampTex, 0.0, 1.0), 1.0);
            glow = max(glow, (0.4 - far * 0.3) * (1.0 - cool));
        }
        else if (zclump(_NoiseTex, (1.0 - far) * 0.85 * spread, p, _Seed)) col = float4(zramp(_RampTex, 0.0, (n1 > 0.55) ? 1.0 : 0.0), 1.0);
    }
    if (spread < 0.05) return col;
    float rimIn = 0.58;
    if (d < 1.0 && d >= rimIn)
    {
        float lit = zsat(0.5 + (p.y - 2.0) * 0.12 - p.x * 0.02 + (n1 - 0.5) * 0.6);
        col = float4(zramp(_RampTex, 3.0, zband(lit, 5.0, w, 0.35)), 1.0);
        float mc = zN(_NoiseTex, p / 12.0 + float2(0.3, _Seed)).a;
        if (mc < 0.1) col = zfire(_RampTex, (0.62 + 0.2 * (rimIn + 0.2 - d)) * (1.0 - cool * 0.7), w, 0.2);
        if (d > 0.93) col = float4(zramp(_RampTex, 0.0, 0.0), 1.0);
        glow = max(glow, 0.2 * (1.0 - cool));
    }
    else if (d < rimIn)
    {
        float lf = zflow(_NoiseTex, float2(p.x * 0.8, p.y * 1.6), 8.0, t, 2.0, _Seed + 0.5);
        float crust = zN(_NoiseTex, p / 14.0 + float2(t * 0.03, _Seed)).g;
        float heat = (0.62 + 0.3 * (rimIn - d) / rimIn + (lf - 0.5) * 0.5) * (1.0 - cool * 0.75);
        col = zfire(_RampTex, heat, w, 0.0);
        if (crust > 0.62 + 0.25 * (1.0 - cool)) col = float4(zramp(_RampTex, 3.0, (crust > 0.75) ? 2.0 : 1.0), 1.0);
        glow = max(glow, (0.55 + 0.3 * lf) * (1.0 - cool * 0.8));
    }
    return col;
}
// base tongues (drawn over the neck roots) + the summon eruption gout
float zh_tongues(float2 p, float t)
{
    if (p.y < 1.0 || p.y > 24.0 || abs(p.x) > 13.0) return 0.0;
    float erupt = exp(-((_Rise - 0.38) * (_Rise - 0.38)) / 0.012) * step(0.05, _Rise);
    float alive = zsat(_Rise / 0.3) * (1.0 - zeaseOut(_Fade / 0.7));
    float best = 0.0;
    float cw = 3.2;
    float ci = floor((p.x + 13.0) / cw);
    for (int k = -1; k < 2; k++)
    {
        float c = ci + float(k);
        float cx = -13.0 + (c + 0.5) * cw + (zhash1(c + 11.0 + _Seed * 7.0) - 0.5) * 1.6;
        if (abs(cx) > 11.5) continue;
        float y0 = 2.0 + 2.6 * zsat(1.0 - abs(cx) / 12.0);
        float hk = (3.0 + 5.0 * zhash1(c * 3.1 + _Seed) + 2.0 * sin(t * 6.0 + c * 1.7)) * (1.0 - abs(cx) / 16.0) * alive + erupt * (14.0 - abs(cx) * 0.8);
        if (hk < 1.0) continue;
        float yn = (p.y - y0) / hk;
        if (yn < 0.0 || yn > 1.0) continue;
        float sway = sin(t * 5.0 + c * 2.3 + p.y * 0.4) * 0.9 * yn;
        float hw = 1.7 * pow(max(1.0 - yn, 0.0), 0.7) + 0.35 + erupt * 1.2;
        float v = (1.0 - abs(p.x - cx - sway) / hw) * (1.05 - yn * 0.55);
        best = max(best, v);
    }
    return best;
}
float4 ZFX_Frag(float2 p, float2 w)
{
    float t = floor(_FxTime * _StepFps) / _StepFps;
    float glowF = 0.0;
    float gb = 0.0;
    float4 col = zh_base(p, w, t, gb);
    glowF = gb;
    float fl = zflow(_NoiseTex, p + float2(_Seed * 31.0, 0.0), 6.0, t, 14.0, _Seed + 0.2);
    // heads back to front: centre (tallest) behind, then left, then right
    for (int k = 0; k < 3; k++)
    {
        float i = (k == 0) ? 1.0 : ((k == 1) ? 0.0 : 2.0);
        float mh = 0.0; float g = 0.0; float b = 0.0;
        float4 hc = zh_head(p, w, i, t, mh, g, b);
        if (b > 0.5) col = hc;
        else if (mh > 0.0)
        {
            float4 mc = zfire(_RampTex, mh * 0.85, w, 0.22);
            if (mc.a > 0.5) col = mc;
            else if (mh > 0.12) col = float4(zramp(_RampTex, 0.0, 0.0), 1.0);   // dark lick lip (reads on red floors)
        }
        glowF = max(glowF, max(g * 0.6, mh * 0.5));
    }
    // base tongues over the neck roots
    float tv = zh_tongues(p, t);
    if (tv > 0.0)
    {
        float th = tv * 1.15 + (fl - 0.5) * 0.8;
        float4 tc = zfire(_RampTex, th, w, 0.25);
        if (tc.a > 0.5) col = tc;
        else if (th > 0.12 && p.y > 4.0) col = float4(zramp(_RampTex, 0.0, 0.0), 1.0);
        glowF = max(glowF, zsat(th) * 0.6);
    }
    // muzzle flash + spit smoke, along the true aim angle
    for (int j = 0; j < 3; j++)
    {
        float i = float(j);
        float2 H; float2 hd; float2 yax; float jaw; float charge; float flash; float2 ad; float r; float wu;
        zh_pose(i, t, H, hd, yax, jaw, charge, flash, ad, r, wu);
        float s = t - zh_pick(_SpitTime, i) - _Windup;
        float2 M = H + hd * 7.5 - yax * 2.5;
        float2 a = float2(dot(p - M, ad), dot(p - M, float2(-ad.y, ad.x)));
        if (flash > 0.0 && r > 0.5)
        {
            float L = 4.0 + 6.0 * flash;
            float nf = zN(_NoiseTex, a / 4.0 + float2(t * 2.3, i * 0.31 + _Seed)).r;
            bool inF = a.x > -1.0 && a.x < L;
            float wid = (1.0 + 2.6 * sin(zsat((a.x + 1.0) / (L + 1.0)) * 3.0)) * (0.7 + 0.6 * nf);
            float side = (a.x > -1.0 && a.x < 4.0) ? (2.5 - abs(abs(a.y) - (1.0 + a.x * 0.9))) * 0.5 : 0.0;   // two side spurts
            float v = max(inF ? (1.0 - abs(a.y) / wid) * (1.2 - a.x / (L * 1.4)) : 0.0, side * flash * 0.8);
            float hh = v * (0.6 + 0.6 * flash) + (nf - 0.5) * 0.5;
            float4 fc = zfire(_RampTex, hh, w, 0.2);
            if (fc.a > 0.5) col = fc;
            float core = 1.0 - length(float2(a.x - 1.0, a.y)) / (1.5 + 1.5 * flash);
            if (core > 0.0) col = zfire(_RampTex, 1.0, w, 0.0);
            glowF = max(glowF, zsat(v) * flash * 1.1);
        }
        if (s > 0.08 && s < 0.6 && r > 0.5)   // a small clumpy dark puff left at the mouth, drifting up
        {
            float sp = (s - 0.08) / 0.52;
            float2 c = M + ad * (2.0 + 3.0 * sp) + float2(0.0, 5.0 * sp);
            float rr = 1.5 + 2.2 * sp;
            float ns = zN(_NoiseTex, (p - c) / 3.5 + float2(i * 0.5, t * 0.2)).g;
            float dens = (1.0 - length(p - c) / rr) * 1.6 * (1.0 - sp) + (ns - 0.5) * 0.9;
            if (dens > 0.25 && col.a < 0.5) col = float4(zramp(_RampTex, 1.0, (dens > 0.6) ? 2.0 : 1.0), 1.0);
        }
    }
    if (_Layer > 0.5) return zglow(_RampTex, glowF, w, _GlowGain);
    return col;
}
