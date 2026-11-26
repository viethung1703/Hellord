// Grave Legion v4 (Zhong Kui kit v4, NEW ultimate): a mass skeleton rise out of the grave earth. Bone, earth and grave dirt only:
// no ghosts, no spectral figures. Same portable HLSL subset as Core (compiled as GLSL by the MOCKUP).
//  _Mode 0 ARMED    (ground, at the hero's feet, loops while the player sits at the minion cap): hairline grave cracks open and
//                   close, a bony fingertip pokes out of one, a pinch of dirt; faint red in the cracks (glow twin). Subtle.
//  _Mode 1 CAST     (ground, at the hero, 0.6 s): the earth under the feet breaks into plates and throws low dirt.
//  _Mode 2 RISE GROUND (ground, one per grave): a crack races from the hero to the grave, the grave tears open (pit + broken
//                   rim + radial cracks, red hell-light in the pit on the glow twin), then fills into a dirt mound and fades.
//  _Mode 3 RISE     (ysort, one per grave): dirt bursts up, two skeletal hands claw out of the pit, then the minion's REAL
//                   sprite rises: warrior = its death_south sheet played backwards (bone pile -> standing) while pushing up
//                   out of the ground; archer = its idle frame emerging from the pit, shedding dirt. Ends with a hand-off
//                   to the game's minion sprite.
//  _Mode 4 CRUMBLE  (ysort, end beat, PROVISIONAL): an over-cap skeleton collapses (death sheet forward, or a top-down
//                   dissolve for the archer), its pile breaks into bone bits and dust and sinks into the ground.
// _SkelTex = the minion's real sprite sheet (the hook passes it); _SkelInfo = (frames, frame w, frame h, pivot x),
// _SkelInfo2 = (pivot y from the top, sheet kind 0 death / 1 idle, flip, 0).

// bone piece sample. q = pixel centre - piece centre (piece centre on whole px). kind 0..7, rot 0..7. -> level 0 / .25 ink / .5 / .75 / 1
float zgl_piece(float2 q, float kind, float rot)
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
float4 zgl_bone(float lv, float dim)
{
    float i = lv < 0.375 ? 0.0 : (lv < 0.625 ? 3.0 : (lv < 0.875 ? 4.0 : 5.0));
    if (dim > 0.5 && i > 0.5) i = i - 1.0;
    if (i > 4.5 && _Hot < 0.5) i = 4.0;
    return float4(zramp(_RampTex, 0.0, i), 1.0);
}
// rolling dust billow: density 0..1 (q relative to the billow centre). age 0..1 thins it from the inside out (noise dissolve).
float zgl_billow(float2 q, float rx, float ry, float age, float seed, float t)
{
    float2 e2 = float2(q.x / max(rx, 0.5), q.y / max(ry, 0.5));
    float e = length(e2);
    float n = zN(_NoiseTex, q / 5.0 + float2(seed * 7.1 + t * 0.6, seed * 3.3 - t * 0.9)).r;
    float n2 = zN(_NoiseTex, q / 2.6 + float2(seed * 1.7, seed * 5.9 + t * 1.3)).g;
    return zsat((1.0 - e) * 1.7 + (n - 0.5) * 1.3 + (n2 - 0.5) * 0.5 - age * 1.25);
}
// dust colour. dens = billow density here, densL = density one px toward the light (upper left): lit where the lump faces it.
float4 zgl_dustL(float dens, float densL, float lit, float2 p)
{
    float4 c = zk_none();
    if (zdithS(dens, p, 0.6))
    {
        float face = zsat((dens - densL) * 5.0 + 0.5);            // > 0.5: this px faces the light
        float v = zsat(lit * 0.45 + face * 0.55);
        float i = 2.0 + zband(v, 3.0, p, 0.7);                  // 2..4: 733e39 b86f50 c28569 (reads paler than the floor)
        if (dens < 0.25 && i > 2.5) i = i - 1.0;                 // thin edges sink back toward the floor
        if (face > 0.85 && dens > 0.6 && lit > 0.55) i = 5.0;     // e8b796 on lit lump tops
        c = float4(zramp(_RampTex, 1.0, i), 1.0);
    }
    return c;
}
float4 zgl_dust(float dens, float lit, float2 p) { return zgl_dustL(dens, dens - 0.05, lit, p); }
// pixel-art dust ball (Blasphemous / Dead Cells style): a round puff lit from the upper left, no ink rim; age shrinks it and
// bites holes in it (noise dissolve). q relative to the ball centre. Returns a palette colour or none.
float4 zgl_ball(float2 p, float2 q, float r, float age, float seed)
{
    float4 c = zk_none();
    float rr = r * (1.0 - 0.45 * age);
    float d = length(q);
    float bite = zN(_NoiseTex, p / 3.0 + float2(seed * 5.3, seed * 2.1)).r;
    if (rr >= 1.5 && d < rr && bite > age * 1.1 - 0.15)
    {
        float2 n = q / max(rr, 1.0);
        float l = dot(n, float2(-0.6, 0.8)) + zbayer(p) * 0.3;
        float i = l > 0.3 ? 4.0 : (l > -0.35 ? 3.0 : 2.0);
        if (l > 0.62 && rr > 3.0 && age < 0.6) i = 5.0;
        if (d > rr - 1.0 && l < -0.2) i = 1.0;                       // bottom-right edge darkens into the floor
        if (age > 0.55 && i > 2.5) i = i - 1.0;                       // settling dust dims
        c = float4(zramp(_RampTex, 1.0, i), 1.0);
    }
    return c;
}
// a puff = 3 balls (one big, two satellites), lower balls in front
float4 zgl_puff(float2 p, float2 q, float rx, float ry, float age, float seed, float t, float lit)
{
    float4 c = zk_none();
    float h = zhash1(seed * 13.7 + 1.3);
    float2 o1 = float2(-rx * 0.55, ry * (0.25 + 0.3 * h)); float2 o2 = float2(rx * 0.5, ry * 0.1);
    float4 b1 = zgl_ball(p, q - o1, min(rx, ry) * 0.7, age, seed + 0.1);
    float4 b2 = zgl_ball(p, q - o2, min(rx, ry) * 0.75, age, seed + 0.2);
    float4 b0 = zgl_ball(p, q - float2(0.0, -ry * 0.1), min(rx, ry) * 1.0, age, seed);
    if (b1.a > 0.5) c = b1;
    if (b2.a > 0.5) c = b2;
    if (b0.a > 0.5) c = b0;
    return c;
}
// low dirt lumps thrown out from a point (no closed ring): n lumps, mostly sideways. q relative to the source on the ground.
float4 zgl_lumps(float2 p, float2 q, float u, float reach, float seed, float t)
{
    float4 col = zk_none();
    for (int k = 0; k < 6; k++)
    {
        float fk = float(k);
        float side = zmod(fk, 2.0) < 0.5 ? -1.0 : 1.0;
        float a = (side < 0.0 ? ZPI : 0.0) + side * (0.25 + 0.9 * zhash1(fk * 3.7 + seed * 9.0)) * ((fk > 3.5) ? 1.6 : 0.6);
        float dist = reach * (0.45 + 0.55 * zhash1(fk * 5.3 + seed)) * zeaseOut(u);
        float2 c = float2(cos(a) * dist, sin(a) * dist * 0.45 + 2.0 + u * 5.0);
        float sz = 1.0 + 0.6 * zhash1(fk + seed * 2.0);
        float4 dc = zgl_puff(p, q - c, (3.5 + u * 3.0) * sz, (2.6 + u * 2.4) * sz, u * 1.15 + (zhash1(fk * 7.1 + seed) - 0.5) * 0.2, seed + fk * 0.13, t, 0.35 + (q.y - c.y) / 8.0);
        if (dc.a > 0.5) col = dc;
    }
    return col;
}
float zgl_t(float t) { return floor(t * _StepFps) / _StepFps; }
// real sprite sample. q = pixel centre relative to the feet pivot (y up). returns rgba (a = 1 opaque, 0 empty)
float4 zgl_sprite(float2 q, float frame)
{
    float4 res = float4(0.0, 0.0, 0.0, 0.0);
    float W = _SkelInfo.y, H = _SkelInfo.z, px = _SkelInfo.w, py = _SkelInfo2.x;
    float c = floor(q.x) + px;
    if (_SkelInfo2.z > 0.5) c = px - 1.0 - floor(q.x) + (W - 2.0 * px);      // mirror (flipX)
    float r = py - 1.0 - floor(q.y);
    if (c >= 0.0 && c < W && r >= 0.0 && r < H)
    {
        float2 uv = float2((frame * W + c + 0.5) / (_SkelInfo.x * W), 1.0 - (r + 0.5) / H);
        float4 s = ZTEX(_SkelTex, uv);
        if (s.a > 0.5) res = float4(s.rgb, 1.0);
    }
    return res;
}
// skeletal hand from the atlas row 8. q relative to the wrist; pose 0 claw, 1 reach, 2 grip, 3 fist; mirror adds 4
float zgl_hand(float2 q, float pose)
{
    float res = 0.0;
    if (abs(q.x) < 8.0 && q.y > -2.0 && q.y < 14.0)
    {
        float c = floor(q.x + 8.0), r = floor(15.0 - q.y);
        float2 uv = float2((pose * 16.0 + c + 0.5) / 128.0, 1.0 - (128.0 + r + 0.5) / 144.0);
        res = ZTEX(_BoneTex, uv).r;
    }
    return res;
}
float4 zgl_earth(float i) { return float4(zramp(_RampTex, 3.0, i), 1.0); }
// jagged crack test along a polyline-ish path: d = signed perpendicular px, s = px along. returns 0 none, 1 ink, 2 lip
float zgl_crackpx(float d, float s, float seed, float w)
{
    float j = (zN(_NoiseTex, float2(s / 9.0, seed)).r - 0.5) * 4.0 + (zN(_NoiseTex, float2(s / 3.0, seed + 0.5)).g - 0.5) * 1.2;
    float e = d - j;
    float res = 0.0;
    if (abs(e) < 0.5 + w) res = 1.0;
    else if (e > 0.0 && e < 1.5 + w) res = 2.0;
    return res;
}

// ---------------- 0 ARMED ----------------
float4 zgl_armed(float2 p, float T, float glow)
{
    float4 col = zk_none(); float g = 0.0;
    float P = 1.6;
    float cyc = floor(T / P); float ph = (T - cyc * P) / P;
    float fade = 1.0 - _Fade;
    for (int k = 0; k < 3; k++)
    {
        float fk = float(k);
        float h = zhash1(fk * 7.7 + cyc * 3.1 + _Seed * 9.0);
        float a = ZPI + (0.12 + 0.76 * (fk + h * 0.6) / 3.0) * ZPI;          // the front arc (below / beside the feet), never under the body
        float r0 = 11.0 + 4.0 * h;
        float st = fk * 0.08;
        float open = zsat((ph - st) / 0.18) * (1.0 - zsat((ph - 0.62 - st) / 0.2));
        float len = 9.0 * open;
        float2 dir = float2(cos(a), sin(a) * 0.55);
        float2 nrm = float2(-dir.y, dir.x);
        float2 q = p - dir * r0;
        float s = dot(q, dir), d = dot(q, nrm);
        if (open > 0.02 && s > 0.0 && s < len)
        {
            float cr = zgl_crackpx(d, s + fk * 17.0, _Seed + fk * 0.3, 0.0);
            if (cr > 1.5 && zbayer(p) < 0.2) col = zgl_earth(3.0);
            if (cr > 0.5 && cr < 1.5) { col = zgl_earth(0.0); g = max(g, open * 0.45); }
        }
        // a fingertip pokes out of the first crack, mid cycle
        if (k == 0)
        {
            float poke = zsat((ph - 0.34) / 0.1) * (1.0 - zsat((ph - 0.52) / 0.1));
            float2 tip = dir * (r0 + len * 0.8);
            float2 qh = p - floor(tip + float2(0.5, 0.5)) - float2(0.0, -9.0 + 5.0 * poke);
            float lv = zgl_hand(qh, h < 0.5 ? 1.0 : 5.0);
            if (poke > 0.05 && lv > 0.1 && (p.y - tip.y) > -0.5) col = zgl_bone(lv, 1.0);
        }
        // a pinch of grave dirt at the mouth while it opens
        if (open > 0.3 && open < 0.95)
        {
            float4 b = zgl_ball(p, p - dir * (r0 + 1.0) - float2(0.0, 1.0 + ph * 2.0), 1.8, zsat(ph * 1.5), _Seed + fk);
            if (b.a > 0.5) col = b;
        }
    }
    if (fade < 0.999 && col.a > 0.5 && zbayer(p) > fade - 0.5) col = zk_none();
    float4 res = col;
    if (glow > 0.5) res = (g > 0.05 && col.a > 0.5) ? zk_glow(_RampTex, min(g, _GlowCap) * fade, p, _GlowGain, 2.0) : zk_none();
    return res;
}

// ---------------- 1 CAST ----------------
float4 zgl_cast(float2 p, float T, float glow)
{
    float4 col = zk_none(); float g = 0.0;
    float d = length(float2(p.x, p.y * 1.6));
    float rr = 4.0 + 12.0 * zeaseOut(zsat(T / 0.12));
    float fade = zsat((T - 0.35) / 0.3);
    float nz = zN(_NoiseTex, p / 3.0 + float2(_Seed * 4.0, 0.0)).r;
    if (T >= 0.0 && d < rr && nz > fade)
    {
        float4 cz = zN(_NoiseTex, p / 5.0 + float2(_Seed, _Seed * 2.0));
        if (cz.a < 0.12) col = zgl_earth(0.0);
        else if (cz.a < 0.22 && p.y > -1.0) col = zgl_earth(4.0);
        else if (d > rr - 2.0) col = zgl_earth(2.0);
    }
    float4 lc = zgl_lumps(p, p, zsat((T + 0.03) / 0.5), 16.0, _Seed + 0.7, T);
    if (lc.a > 0.5 && T < 0.6) col = lc;
    if (T < 0.13) g = zsat(1.0 - d / 10.0) * (T < 0.067 ? 1.0 : 0.5);
    float4 res = col;
    if (glow > 0.5) res = (g > 0.05) ? zk_glow(_RampTex, min(g, _GlowCap), p, _GlowGain, 2.0) : zk_none();
    return res;
}

// ---------------- 2 RISE GROUND ----------------
float zgl_open(float u) { return zeaseOut(zsat(u / 0.13)); }
float4 zgl_riseground(float2 p, float T, float glow)
{
    float4 col = zk_none(); float g = 0.0;
    float u = T - _Delay;
    // a) the crack from the hero's feet to this grave (it arrives as the grave opens: speed = distance / _Delay)
    float2 H = _HeroOff.xy; float L = length(H);
    if (L > 12.0 && T >= 0.0)
    {
        float2 dir = -H / L; float2 nrm = float2(-dir.y, dir.x);
        float2 q = p - H;
        float s = dot(q, dir), dd = dot(q, nrm);
        float front = T * L / max(_Delay, 0.05);
        float age = T - s / (L / max(_Delay, 0.05));
        float vis = (s > 9.0 && s < min(front, L - 6.0)) ? 1.0 : 0.0;
        float dis = zsat((age - 0.35) / 0.4);
        if (vis > 0.5 && zN(_NoiseTex, p / 2.5).r > dis)
        {
            float cr = zgl_crackpx(dd, s, _Seed * 3.0 + 0.1, 0.0);
            if (cr > 0.5 && cr < 1.5) col = zgl_earth(0.0);
            else if (cr > 1.5 && zbayer(p) < 0.25) col = zgl_earth(3.0);
        }
    }
    // b) the grave tears open
    if (u >= 0.0)
    {
        float op = zgl_open(u);
        float close = zsat((u - 0.62) / 0.3);
        float gone = zsat((u - 0.95) / 0.45);
        float a = atan2(p.y, p.x);
        float jag = (zN(_NoiseTex, float2(a * 1.3 + _Seed * 5.0, _Seed)).r - 0.5) * 0.35;
        float e = length(float2(p.x / (10.0 * op + 0.01), p.y / (4.6 * op + 0.01))) + jag;
        float nz = zN(_NoiseTex, p / 2.4 + float2(_Seed * 2.0, 0.0)).r;
        if (op > 0.05 && nz > gone)
        {
            // radial cracks out of the rim
            for (int k = 0; k < 6; k++)
            {
                float fk = float(k);
                float ak = (fk / 6.0 + zhash1(fk + _Seed * 7.0) * 0.12) * ZTAU;
                float2 dir = float2(cos(ak), sin(ak) * 0.5);
                float2 nrm = float2(-dir.y, dir.x);
                float2 q = p - dir * 9.0 * op;
                float s = dot(q, dir), dd = dot(q, nrm);
                float len = (5.0 + 6.0 * zhash1(fk * 3.1 + _Seed)) * op;
                if (s > -1.0 && s < len)
                {
                    float cr = zgl_crackpx(dd * 1.0, s + fk * 11.0, _Seed + fk * 0.21, 0.0);
                    if (cr > 0.5 && cr < 1.5) col = zgl_earth(0.0);
                }
            }
            if (e < 1.0)
            {
                if (close < 0.02 || e > close)
                {
                    // the pit: ink, a cool deep band on the far wall, red light low in it (glow twin)
                    col = zgl_earth(0.0);
                    if (p.y > 0.5 && e > 0.55) col = zgl_earth(1.0);
                    g = max(g, zsat(1.0 - e * 1.3) * zsat(u / 0.07) * (1.0 - zsat((u - 0.3) / 0.25)));
                }
                else
                {
                    // filled back in: a dirt mound with clods
                    float4 cz = zN(_NoiseTex, p / 3.0 + float2(_Seed * 3.0, 1.0));
                    float li = (p.y > 0.0 ? 1.0 : 0.0) + (cz.r > 0.55 ? 1.0 : 0.0);
                    col = zgl_earth(2.0 + li);
                    if (cz.a < 0.1) col = zgl_earth(0.0);
                }
            }
            else if (e < 1.35)
            {
                // broken rim: lifted slabs (lit near lip, dark gaps)
                float4 cz = zN(_NoiseTex, p / 4.0 + float2(_Seed, 2.0));
                col = (cz.a < 0.14) ? zgl_earth(0.0) : (p.y < 0.0 ? zgl_earth(4.0) : zgl_earth(3.0));
                if (e > 1.25 && zbayer(p) > 0.1) col = zgl_earth(2.0);
            }
        }
    }
    float4 res = col;
    if (glow > 0.5) res = (g > 0.05) ? zk_glow(_RampTex, min(g, _GlowCap), p, _GlowGain, 2.0) : zk_none();
    return res;
}

// ---------------- 3 RISE ----------------
float4 zgl_rise(float2 p, float T, float glow)
{
    float4 col = zk_none(); float g = 0.0;
    float u = T - _Delay;
    if (u >= 0.0 && u < 1.0)
    {
        // a) dirt thrown up out of the pit (behind the body: even balls)
        for (int k = 0; k < 6; k++)
        {
            float fk = float(k);
            float h1 = zhash1(fk * 5.1 + _Seed * 3.0), h2 = zhash1(fk * 2.3 + _Seed * 7.0);
            float t = u - 0.12 - fk * 0.02;
            float2 c = float2((h1 - 0.5) * 16.0 + (h1 - 0.5) * 30.0 * t, (34.0 + 26.0 * h2) * t - 0.5 * 220.0 * t * t + 2.0);
            if (t > 0.0 && c.y > -2.0 && zmod(fk, 2.0) < 0.5)
            {
                float4 b = zgl_ball(p, p - floor(c + float2(0.5, 0.5)), 3.0 + 1.5 * h2, zsat(t / 0.55), _Seed + fk * 0.13);
                if (b.a > 0.5) col = b;
            }
        }
        // b) two skeletal hands claw up out of the pit, grip the rim, then the body comes
        for (int hk = 0; hk < 2; hk++)
        {
            float side = hk == 0 ? -1.0 : 1.0;
            float t0 = 0.06 + float(hk) * 0.04;
            float v = u - t0;
            if (v >= 0.0 && v < 0.3)
            {
                float up = zeaseOut(zsat(v / 0.12));
                float pose = v < 0.067 ? 3.0 : (v < 0.2 ? 0.0 : 2.0);
                if (side < 0.0) pose = pose + 4.0;
                float2 w = float2(side * (6.0 + (v > 0.2 ? 2.0 : 0.0)), -6.0 + 12.0 * up - (v > 0.2 ? 4.0 : 0.0));   // forearm and claw clear the rim
                float2 qh = p - w;
                float lv = zgl_hand(qh, pose);
                if (lv > 0.1 && p.y > -1.5) col = zgl_bone(lv, 0.0);
            }
        }
        // c) the body: the real minion sprite
        float rs = u - 0.2;
        if (rs >= 0.0 && rs < 0.54)
        {
            float frame = 0.0; float off = 0.0; float clipY = -3.0;
            if (_SkelInfo2.y < 0.5)
            {
                frame = clamp(_SkelInfo.x - 1.0 - floor(rs * 15.0), 0.0, _SkelInfo.x - 1.0);   // death sheet backwards: pile -> standing
                off = -10.0 * (1.0 - zeaseOut(zsat(rs / 0.2)));
                clipY = off < -0.5 ? 0.0 : -3.0;
            }
            else
            {
                off = -34.0 * (1.0 - zeaseOut(zsat(rs / 0.4)));
                clipY = off < -0.5 ? 0.0 : -3.0;
                off = floor(off);
            }
            float2 qs = p - float2(0.0, floor(off));
            float4 sc = zgl_sprite(qs, frame);
            if (sc.a > 0.5 && p.y >= clipY)
            {
                col = sc;
                // grave dirt still on it while it comes up
                float dirt = zN(_NoiseTex, qs / 2.0 + float2(_Seed * 4.0, 0.0)).r;
                if (dirt > 0.62 + rs * 0.9) col = float4(zramp(_RampTex, 1.0, dirt > 0.8 ? 2.0 : 3.0), 1.0);
            }
            // dirt falling off it
            for (int fk2 = 0; fk2 < 3; fk2++)
            {
                float ff = float(fk2);
                float t = zmod(rs * 2.4 + ff * 0.33, 1.0);
                float2 c = float2((zhash1(ff + _Seed * 3.0) - 0.5) * 12.0, (22.0 + off) * (1.0 - t * t) + 2.0);
                if (rs < 0.45 && length(p - floor(c + float2(0.5, 0.5))) < 1.0) col = float4(zramp(_RampTex, 1.0, 3.0), 1.0);
            }
        }
        // d) front dirt (odd balls) over the body, and earth clods
        for (int k2 = 0; k2 < 6; k2++)
        {
            float fk = float(k2);
            float h1 = zhash1(fk * 5.1 + _Seed * 3.0), h2 = zhash1(fk * 2.3 + _Seed * 7.0);
            float t = u - 0.04 - fk * 0.02;
            float2 c = float2((h1 - 0.5) * 16.0 + (h1 - 0.5) * 30.0 * t, (34.0 + 26.0 * h2) * t - 0.5 * 220.0 * t * t - 1.0);
            if (t > 0.0 && c.y > -4.0 && zmod(fk, 2.0) > 0.5)
            {
                float4 b = zgl_ball(p, p - floor(c + float2(0.5, 0.5)), 2.6 + 1.4 * h2, zsat(t / 0.5), _Seed + fk * 0.17);
                if (b.a > 0.5) col = b;
            }
        }
        for (int k3 = 0; k3 < 4; k3++)
        {
            float fk = float(k3);
            float a = (0.15 + 0.7 * zhash1(fk * 3.3 + _Seed)) * ZPI;
            float v = 40.0 + 30.0 * zhash1(fk * 9.1 + _Seed);
            float t = u - 0.03;
            float2 c = float2(cos(a) * v * t, sin(a) * v * t - 0.5 * 300.0 * t * t);
            if (t > 0.0 && t < 0.45 && c.y > -3.0)
            {
                float lvc = gg_chip(p - floor(c + float2(0.5, 1.5)), 1.0 + zmod(fk, 2.0));
                if (lvc > -0.5) col = zgl_earth(lvc < 0.5 ? 0.0 : (lvc < 3.5 ? 3.0 : 4.0));
            }
        }
        // a 1-frame bone glint as it stands up
        if (rs > 0.46 && rs < 0.53) g = zsat(1.0 - length(p - float2(0.0, 26.0)) / 4.0) * 0.6;
    }
    float4 res = col;
    if (glow > 0.5) res = (g > 0.05) ? zk_glow(_RampTex, min(g, _GlowCap), p, _GlowGain, 0.0) : zk_none();
    return res;
}

// ---------------- 4 CRUMBLE (end beat, PROVISIONAL) ----------------
float4 zgl_crumble(float2 p, float T, float glow)
{
    float4 col = zk_none();
    float u = T - _Delay;
    if (u >= 0.0 && u < 1.1)
    {
        float sink = floor(4.0 * zsat((u - 0.45) / 0.5));
        float2 qs = p + float2(0.0, sink);
        float4 sc = float4(0.0, 0.0, 0.0, 0.0);
        float nz = zN(_NoiseTex, qs / 2.2 + float2(_Seed * 6.0, 0.0)).r;
        if (_SkelInfo2.y < 0.5)
        {
            float frame = clamp(floor(u * 15.0), 0.0, _SkelInfo.x - 1.0);   // death sheet forward: collapses to a pile
            sc = zgl_sprite(qs, frame);
            float dis = zsat((u - 0.5) / 0.45);
            if (sc.a > 0.5 && nz < dis) sc = (nz > dis - 0.18) ? float4(zramp(_RampTex, 1.0, 3.0), 1.0) : float4(0.0, 0.0, 0.0, 0.0);
        }
        else
        {
            // archer has no death sheet: it slumps and dissolves top-down into dust
            float slump = floor(6.0 * zsat(u / 0.5));
            sc = zgl_sprite(qs + float2(0.0, slump * zsat(qs.y / 20.0)), 0.0);
            float th = 1.0 - zsat(u / 0.75);
            float hv = zsat(qs.y / 34.0) * 0.75 + nz * 0.25;
            if (sc.a > 0.5 && hv > th) sc = (hv < th + 0.1) ? float4(zramp(_RampTex, 1.0, 3.0), 1.0) : float4(0.0, 0.0, 0.0, 0.0);
        }
        if (sc.a > 0.5 && p.y >= -3.0) col = sc;
        // bone bits tumble off and dust puffs at the pile
        for (int k = 0; k < 4; k++)
        {
            float fk = float(k);
            float t = u - 0.3 - fk * 0.05;
            float side = zmod(fk, 2.0) < 0.5 ? -1.0 : 1.0;
            float2 c = float2(side * (8.0 + 18.0 * zhash1(fk + _Seed)) * t * 2.0, 6.0 + 30.0 * t - 0.5 * 260.0 * t * t);
            float kind = k == 0 ? 6.0 : (k == 1 ? 3.0 : (k == 2 ? 4.0 : 1.0));
            if (t > 0.0 && t < 0.55)
            {
                float lv = zgl_piece(p - floor(float2(c.x, max(c.y, -2.0)) + float2(0.5, 0.5)), kind, zmod(floor(t * 15.0) + fk * 3.0, 8.0));
                float fadeb = zsat((t - 0.35) / 0.2);
                if (lv > 0.1 && zN(_NoiseTex, p / 2.0).g > fadeb) col = zgl_bone(lv, 1.0);
            }
        }
        float4 lc = zgl_lumps(p, p, zsat((u - 0.4) / 0.6), 12.0, _Seed + 0.4, T);
        if (lc.a > 0.5 && u > 0.4) col = lc;
    }
    float4 res = col;
    if (glow > 0.5) res = zk_none();
    return res;
}

float4 ZFX_Frag(float2 p, float2 w)
{
    float T = zgl_t(_FxTime);
    float glow = _Layer > 0.5 ? 1.0 : 0.0;
    float4 c = zk_none();
    if (_Mode < 0.5) c = zgl_armed(p, T, glow);
    else if (_Mode < 1.5) c = zgl_cast(p, T, glow);
    else if (_Mode < 2.5) c = zgl_riseground(p, T, glow);
    else if (_Mode < 3.5) c = zgl_rise(p, T, glow);
    else c = zgl_crumble(p, T, glow);
    if (_Intensity < 0.999 && c.a > 0.5 && glow < 0.5 && zbayer(w) > _Intensity - 0.5) c = zk_none();
    if (glow > 0.5) c = float4(c.rgb * _Intensity, c.a);
    return c;
}
