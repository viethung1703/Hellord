// Bone Colossus v4 (Zhong Kui kit v4): bone / earth only, no spectral figures. The body is the game's own sprite sheet
// (NkDrawColossus1736); these modes are the FX around it. Same portable HLSL subset as Core (compiled as GLSL by the MOCKUP).
//  _Mode 0 MERGE  (ysort, 0.8 s, _Progress): each minion's bones tear loose where it stood and arc into the giant's body.
//  _Mode 1 SPLIT  (ysort, 0.8 s, _Progress): the giant collapses; its bones blow outward, bounce once and crumble to dust.
//  _Mode 2 STOMP ground (under sprites, _FxTime): radial cracked earth + crater plates, out to _Radius.
//  _Mode 3 STOMP front (over, _FxTime): bone spurs erupt in a wave out to the hit radius (front half), dust kicks, chips.
//  _Mode 4 STOMP back (ground tier, _FxTime): the back half of the spurs (behind the giant), so they never draw over its body.
// Bone pieces / spurs are code-drawn masks (ZKF_ColBones, Point) that the shader moves, spins in 45 deg steps and dissolves.

float zcb_T()
{
    float t = (_Mode < 1.5) ? _Progress * 0.8 : (_FxTime - _Delay);
    return floor(t * _StepFps) / _StepFps;
}
// bone piece sample. q = pixel centre - piece centre (piece centre on whole px). kind 0..7, rot 0..7. -> level 0 / .25 ink / .5 / .75 / 1
float zcb_piece(float2 q, float kind, float rot)
{
    float res = 0.0;
    if (abs(q.x) < 8.0 && abs(q.y) < 8.0)
    {
        float c = floor(q.x + 8.0), r = floor(8.0 - q.y);
        float2 uv = float2((rot * 16.0 + c + 0.5) / 128.0, 1.0 - (kind * 16.0 + r + 0.5) / 160.0);
        res = ZTEX(_BoneTex, uv).r;
    }
    return res;
}
// bone spur sample. q relative to the spur base (ground contact), rise 0..1 pushes it up out of the ground. flip mirrors.
float zcb_spike(float2 q, float kind, float rise, float flip)
{
    float res = 0.0;
    float qx = flip > 0.5 ? -q.x : q.x;
    float qy = q.y - (1.0 - rise) * 22.0;                  // still under the ground below y 0
    if (q.y >= -1.0 && abs(qx) < 8.0 && qy > -2.0 && qy < 30.0)
    {
        float c = floor(qx + 8.0), r = floor(30.0 - qy);
        float2 uv = float2((kind * 16.0 + c + 0.5) / 128.0, 1.0 - (128.0 + r + 0.5) / 160.0);
        res = ZTEX(_BoneTex, uv).r;
    }
    return res;
}
float4 zcb_bone(float lv, float dim)
{
    float i = lv < 0.375 ? 0.0 : (lv < 0.625 ? 3.0 : (lv < 0.875 ? 4.0 : 5.0));
    if (dim > 0.5 && i > 0.5) i = i - 1.0;
    if (i > 4.5 && _Hot < 0.5) i = 4.0;
    return float4(zramp(_RampTex, 0.0, i), 1.0);
}
// rolling dust billow: density 0..1 (q relative to the billow centre). age 0..1 thins it from the inside out (noise dissolve).
float zcb_billow(float2 q, float rx, float ry, float age, float seed, float t)
{
    float2 e2 = float2(q.x / max(rx, 0.5), q.y / max(ry, 0.5));
    float e = length(e2);
    float n = zN(_NoiseTex, q / 5.0 + float2(seed * 7.1 + t * 0.6, seed * 3.3 - t * 0.9)).r;
    float n2 = zN(_NoiseTex, q / 2.6 + float2(seed * 1.7, seed * 5.9 + t * 1.3)).g;
    return zsat((1.0 - e) * 1.7 + (n - 0.5) * 1.3 + (n2 - 0.5) * 0.5 - age * 1.25);
}
// dust colour. dens = billow density here, densL = density one px toward the light (upper left): lit where the lump faces it.
float4 zcb_dustL(float dens, float densL, float lit, float2 p)
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
float4 zcb_dust(float dens, float lit, float2 p) { return zcb_dustL(dens, dens - 0.05, lit, p); }
// pixel-art dust ball (Blasphemous / Dead Cells style): a round puff lit from the upper left, no ink rim; age shrinks it and
// bites holes in it (noise dissolve). q relative to the ball centre. Returns a palette colour or none.
float4 zcb_ball(float2 p, float2 q, float r, float age, float seed)
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
float4 zcb_puff(float2 p, float2 q, float rx, float ry, float age, float seed, float t, float lit)
{
    float4 c = zk_none();
    float h = zhash1(seed * 13.7 + 1.3);
    float2 o1 = float2(-rx * 0.55, ry * (0.25 + 0.3 * h)); float2 o2 = float2(rx * 0.5, ry * 0.1);
    float4 b1 = zcb_ball(p, q - o1, min(rx, ry) * 0.7, age, seed + 0.1);
    float4 b2 = zcb_ball(p, q - o2, min(rx, ry) * 0.75, age, seed + 0.2);
    float4 b0 = zcb_ball(p, q - float2(0.0, -ry * 0.1), min(rx, ry) * 1.0, age, seed);
    if (b1.a > 0.5) c = b1;
    if (b2.a > 0.5) c = b2;
    if (b0.a > 0.5) c = b0;
    return c;
}
float2 zcb_src(float i)
{
    float2 s = _SrcA.xy;
    if (i > 0.5) s = _SrcA.zw;
    if (i > 1.5) s = _SrcB.xy;
    if (i > 2.5) s = _SrcB.zw;
    if (i > 3.5) s = _SrcC.xy;
    if (i > 4.5) s = _SrcC.zw;
    if (i > 5.5) s = _SrcD.xy;
    if (i > 6.5) s = _SrcD.zw;
    if (_SrcN < 0.5)
    {
        // fallback (hook does not pass the stash): 5 minions on a loose ring around the giant
        float a = (i / 5.0 + _Seed * 0.37) * ZTAU + (zhash1(i * 3.1 + _Seed) - 0.5) * 0.7;
        float rr = 26.0 + 20.0 * zhash1(i * 5.7 + _Seed * 2.0);
        s = float2(cos(a) * rr, sin(a) * rr * 0.6);
    }
    return float2(clamp(s.x, -62.0, 62.0), clamp(s.y, -12.0, 56.0));
}
// low dirt lumps thrown out from a point (no closed ring): n lumps, mostly sideways. q relative to the source on the ground.
float4 zcb_lumps(float2 p, float2 q, float u, float reach, float seed, float t)
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
        float4 dc = zcb_puff(p, q - c, (3.5 + u * 3.0) * sz, (2.6 + u * 2.4) * sz, u * 1.15 + (zhash1(fk * 7.1 + seed) - 0.5) * 0.2, seed + fk * 0.13, t, 0.35 + (q.y - c.y) / 8.0);
        if (dc.a > 0.5) col = dc;
    }
    return col;
}
// MERGE pieces: 5 per minion, laid out on its skeleton (skull, ribs, pelvis, femur, arm). It falls apart where it stands,
// then each piece is torn loose and yanked into the giant. xy = pos, z = 1 in hold / flight, w = age since landing (< 0 before)
float2 zcb_anat(float j)
{
    float2 o = float2(0.0, 27.0);                 // skull
    if (j > 0.5) o = float2(0.0, 19.0);           // ribs
    if (j > 1.5) o = float2(1.0, 13.0);           // pelvis
    if (j > 2.5) o = float2(-2.0, 7.0);           // femur
    if (j > 3.5) o = float2(5.0, 18.0);           // arm
    return o;
}
float zcb_mkind(float j, float i)
{
    float k = 0.0;
    if (j > 0.5) k = 2.0;
    if (j > 1.5) k = 5.0;
    if (j > 2.5) k = 1.0;
    if (j > 3.5) k = 6.0;
    return k;
}
float4 zcb_mpath(float i, float j, float T)
{
    float h1 = zhash1(i * 13.1 + j * 3.7 + _Seed * 11.0), h2 = zhash1(i * 7.3 + j * 9.1 + _Seed * 5.0);
    float order = j < 0.5 ? 0.10 : (j < 1.5 ? 0.05 : (j < 2.5 ? 0.03 : (j < 3.5 ? 0.0 : 0.02)));   // legs go first, the skull last
    float t0 = 0.07 + order + h1 * 0.04 + i * 0.018;
    float D = 0.26 + h2 * 0.10;
    float2 s = zcb_src(i) + zcb_anat(j);
    float2 e = float2((h2 - 0.5) * 24.0, 14.0 + (j < 0.5 ? 40.0 + h1 * 12.0 : h1 * 44.0));
    float2 c = (s + e) * 0.5 + float2(0.0, 16.0 + h1 * 14.0);
    float2 pos = s;
    float vis = 0.0;
    if (T >= 0.0 && T < t0) { vis = 1.0; float sl = zsat(T / t0); pos = s + float2((h1 - 0.5) * 3.0 * sl, -(j < 3.5 ? 4.0 : 2.0) * sl * sl); }   // slumps as it falls apart
    else if (T >= t0 && T < t0 + D)
    {
        float u = pow(zsat((T - t0) / D), 1.5);
        s = s + float2(0.0, -2.0);
        pos = lerp(lerp(s, c, u), lerp(c, e, u), u);
        vis = 1.0;
    }
    else if (T >= t0 + D) pos = e;
    return float4(floor(pos + float2(0.5, 0.5)), vis, (T - t0 - D));
}
float zcb_rot(float kind, float i, float T, float spin)
{
    float r0 = floor(zhash1(i * 2.9 + kind) * 8.0);
    float r = zmod(r0 + floor(T * 15.0 * spin), 8.0);
    if (kind < 0.5 || kind > 6.5) { float w = zmod(floor(T * 15.0 * 0.5) + r0, 4.0); r = w < 0.5 ? 0.0 : (w < 1.5 ? 1.0 : (w < 2.5 ? 0.0 : 7.0)); }   // skulls only wobble
    return r;
}

float4 zcb_merge(float2 p, float T, float glow)
{
    float4 col = zk_none(); float g = 0.0;
    float n = _SrcN < 0.5 ? 5.0 : clamp(_SrcN, 1.0, 8.0);
    // 1) where each minion stood: dirt thrown out low as it collapses (0 .. 0.45 s)
    for (int ii = 0; ii < 8; ii++)
    {
        float fi = float(ii);
        if (fi < n && T < 0.5)
        {
            float2 s = zcb_src(fi);
            float4 dc = zcb_lumps(p, p - s, zsat((T + 0.02) / 0.48), 10.0, _Seed + fi * 0.37, T);
            if (dc.a > 0.5) col = dc;
        }
    }
    // 2) the giant's feet: one heavy settle of dirt rolling out (the code shakes the screen at 0 s)
    float4 fc = zcb_lumps(p, p, zsat((T + 0.04) / 0.62), 26.0, _Seed + 0.5, T);
    if (fc.a > 0.5) col = fc;
    // 2b) the dust column: balls burst up the giant's body in 2 frames (hides the sprite pop-in, gives the bones a pale
    //     ground to read against), then shrink and break up as the bones lock on
    for (int kc = 6; kc >= 0; kc--)
    {
        float fk = float(kc);
        float hk = zhash1(fk * 4.7 + _Seed * 3.0);
        float tb = fk * 0.022;
        float u = zsat((T - tb + 0.067) / 0.12);
        float2 c0 = float2((hk - 0.5) * 18.0 + (zmod(fk, 2.0) - 0.5) * 10.0, 6.0 + fk * 9.0 * zeaseOut(u));
        float agec = zsat((T - 0.1 - fk * 0.03) / 0.5);
        float4 bc = zcb_ball(p, p - c0 - float2(0.0, agec * 6.0), (11.0 - fk * 0.6) * (0.5 + 0.5 * u), agec, _Seed + fk * 0.31);
        if (bc.a > 0.5 && T >= tb - 0.067) col = bc;
    }
    // 3) bones: hold (falls apart) -> flight with a 2-frame bone-dust smear -> a dust pop + glint where it locks on
    for (int i2 = 0; i2 < 8; i2++)
    {
        float fi = float(i2);
        if (fi < n)
        {
            for (int j2 = 0; j2 < 5; j2++)
            {
                float fj = float(j2);
                float kind = zcb_mkind(fj, fi);
                float4 pa = zcb_mpath(fi, fj, T);
                float rot = (pa.w < -0.27) ? (kind < 0.5 ? 0.0 : (kind > 2.5 && kind < 4.5 ? 2.0 : 0.0)) : zcb_rot(kind, fi * 5.0 + fj, T, 1.0);
                if (kind > 0.5 && kind < 1.5 && pa.w < -0.27) rot = 2.0;              // femur upright while it still stands
                float4 pb = zcb_mpath(fi, fj, T - 1.0 / 15.0);
                if (pb.z > 0.5 && pa.z > 0.5 && pa.w > -0.24)
                {
                    float lvb = zcb_piece(p - pb.xy, kind, rot);
                    if (lvb > 0.6) col = float4(zramp(_RampTex, 1.0, 2.0), 1.0);      // 1-frame smear (solid, no checker)
                }
                if (pa.z > 0.5)
                {
                    float lv = zcb_piece(p - pa.xy, kind, rot);
                    if (lv > 0.1) col = zcb_bone(lv, 0.0);
                }
                if (pa.w >= 0.0 && pa.w < 0.134)
                {
                    float2 q = p - pa.xy;
                    float sp = zhash(floor(q) + float2(fi * 7.0, fj * 3.0));
                    float rr = length(q);
                    if (rr > 1.5 + pa.w * 20.0 && rr < 3.0 + pa.w * 26.0 && sp > 0.72) col = float4(zramp(_RampTex, 0.0, 4.0), 1.0);   // bone grit pops off
                    if (pa.w < 0.067) g = max(g, zsat(1.0 - rr / 3.0) * 0.9);     // capped attach glint
                }
            }
        }
    }
    float4 res = col;
    if (glow > 0.5) res = (g > 0.05) ? zk_glow(_RampTex, min(g, _GlowCap), p, _GlowGain, 0.0) : zk_none();
    return res;
}

// SPLIT piece: launched from inside the body volume. xy pos, z visible, w = rest age (s since landing, <0 in flight)
float4 zcb_spath(float i, float T)
{
    float h1 = zhash1(i * 11.3 + _Seed * 7.0), h2 = zhash1(i * 5.9 + _Seed * 3.0), h3 = zhash1(i * 17.1 + _Seed);
    float2 s0 = float2((h1 - 0.5) * 26.0, 8.0 + h2 * 56.0);
    if (i < 0.5) s0 = float2(0.0, 64.0);                           // the big skull drops from head height
    float side = h1 < 0.5 ? -1.0 : 1.0;
    float vx = side * (26.0 + h3 * 46.0) + (h1 - 0.5) * 20.0;
    float vy = 18.0 + h2 * 40.0;
    float gy = (h3 - 0.5) * 18.0 - 2.0;                            // landing depth on the ground plane (screen y)
    if (i < 0.5) { vx = 14.0; vy = 16.0; gy = -6.0; }
    float G = 300.0;
    // time to land: s0.y + vy t - G t^2 / 2 = gy
    float A = 0.5 * G, Bq = -vy, C = gy - s0.y;
    float tl = (-Bq + sqrt(max(Bq * Bq - 4.0 * A * C, 0.0))) / (2.0 * A);
    float t = T;
    float2 pos = float2(0.0, 0.0); float rest = -1.0;
    if (t < tl) pos = float2(s0.x + vx * t, s0.y + vy * t - 0.5 * G * t * t);
    else
    {
        float tb = t - tl; float bv = 34.0 + h2 * 20.0; float tbb = 2.0 * bv / G;    // one bounce
        float xl = s0.x + vx * tl;
        if (tb < tbb) pos = float2(xl + vx * 0.35 * tb, gy + bv * tb - 0.5 * G * tb * tb);
        else { pos = float2(xl + vx * 0.35 * tbb + vx * 0.08 * min(tb - tbb, 0.15), gy); rest = tb - tbb; }
    }
    return float4(floor(pos + float2(0.5, 0.5)), 1.0, rest);
}
float4 zcb_split(float2 p, float T, float glow)
{
    float4 col = zk_none(); float g = 0.0;
    // 1) the body's dust: a ball stack fills the giant's volume at 0 s and slumps to the ground
    for (int kc = 6; kc >= 0; kc--)
    {
        float fk = float(kc);
        float hk = zhash1(fk * 2.9 + _Seed * 5.0);
        float sl = zeaseIn(zsat((T - fk * 0.01) / 0.35));
        float2 c0 = float2((hk - 0.5) * 20.0 + (zmod(fk, 2.0) - 0.5) * 12.0 * (1.0 + sl * 1.5), (6.0 + fk * 9.0) * (1.0 - sl * 0.85));
        float agec = zsat((T - 0.08 - fk * 0.02) / 0.6);
        float4 bc = zcb_ball(p, p - c0, 11.0 - fk * 0.5, agec, _Seed + fk * 0.27);
        if (bc.a > 0.5) col = bc;
    }
    float4 fc = zcb_lumps(p, p, zsat((T + 0.03) / 0.75), 34.0, _Seed + 0.3, T);
    if (fc.a > 0.5) col = fc;
    // 2) 16 bones (piece 0 = the big skull) fly, bounce once, lie, then crumble (noise dissolve) from 0.5 s
    for (int ii = 19; ii >= 0; ii--)
    {
        float fi = float(ii);
        float4 sp = zcb_spath(fi, T);
        float kind = fi < 0.5 ? 7.0 : zcb_mkind(1.0 + zmod(fi, 2.0), fi);
        if (fi > 0.5 && fi < 3.5) kind = 0.0;                    // three minion skulls
        float rot = (sp.w < 0.0) ? zcb_rot(kind, fi, T, 1.0) : zcb_rot(kind, fi, T - sp.w, 1.0);
        if (fi < 0.5 && sp.w >= 0.0) rot = zmod(floor((T - sp.w) * 15.0) + floor(min(sp.w, 0.2) * 15.0), 8.0) < 0.5 ? 0.0 : 1.0;   // the skull rolls one step and stops
        float lv = zcb_piece(p - sp.xy, kind, rot);
        if (lv > 0.1)
        {
            float crumb = zsat((T - 0.5 - zhash1(fi) * 0.1) / 0.28);
            float nz = zN(_NoiseTex, p / 2.2 + float2(fi * 0.31, 0.0)).r;
            if (nz > crumb) col = zcb_bone(lv, crumb > 0.3 ? 1.0 : 0.0);
            else if (nz > crumb - 0.25) col = float4(zramp(_RampTex, 1.0, 3.0), 1.0);   // bone dust edge of the dissolve
        }
    }
    // 3) capped burst light in the chest for 2 frames
    if (T < 0.13) g = zsat(1.0 - length((p - float2(0.0, 34.0)) / float2(10.0, 14.0))) * (T < 0.06 ? 1.0 : 0.5);
    float4 res = col;
    if (glow > 0.5) res = (g > 0.05) ? zk_glow(_RampTex, min(g, _GlowCap), p, _GlowGain, 0.0) : zk_none();
    return res;
}

// STOMP: shared spur layout. i 0..13; xy = base pos (px), z = erupt time, w = kind + 8 * flip
float4 zcb_spur(float i)
{
    float h1 = zhash1(i * 9.7 + _Seed * 13.0), h2 = zhash1(i * 4.1 + _Seed * 2.0);
    // 12 on the rim (the hit radius), in loose pairs so it never reads as an even ring of teeth
    float a = (floor(i * 0.5) / 6.0 + zmod(i, 2.0) * (0.035 + 0.02 * h1) + (h1 - 0.5) * 0.04) * ZTAU;
    float rr = _Radius * (0.82 + 0.16 * h2);
    float2 b = floor(float2(cos(a) * rr, sin(a) * rr) + float2(0.5, 0.5));
    float te = 0.02 + (rr / max(_Radius, 1.0)) * 0.10 + h1 * 0.03;
    float kind = floor(h2 * 8.0); float flip = (b.x < 0.0) ? 1.0 : 0.0;
    return float4(b, te, kind + 8.0 * flip);
}
float zcb_spurrise(float T, float te, float h)
{
    float u = T - te;
    float rise = 0.0;
    if (u >= 0.0) rise = u < 0.067 ? 0.55 : 1.0;                  // 2-frame punch up
    float sink = zsat((u - 0.30 - h * 0.08) / 0.2);
    return rise * (1.0 - sink);
}
float4 zcb_stomp_spurs(float2 p, float T, float glow, float back)
{
    float4 col = zk_none(); float g = 0.0;
    float inHalf = 0.0;
    // spurs, far (high y) first so nearer ones overlap them
    for (int ii = 0; ii < 12; ii++)
    {
        float fi = float(ii);
        float4 s = zcb_spur(fi);
        float isBack = s.y > 0.0 ? 1.0 : 0.0;
        if (abs(isBack - back) < 0.5 && T >= s.z)
        {
            float h = zhash1(fi * 3.3 + _Seed);
            float rise = zcb_spurrise(T, s.z, h);
            float kind = zmod(s.w, 8.0); float flip = s.w > 7.5 ? 1.0 : 0.0;
            float lv = zcb_spike(p - s.xy, kind, rise, flip);
            if (lv > 0.1 && rise > 0.0) col = zcb_bone(lv, (T - s.z) > 0.32 ? 1.0 : 0.0);
            // dirt kicked up at the base when it punches through
            float u = (T - s.z) / 0.3;
            if (u < 1.0)
            {
                float2 q = p - s.xy - float2(0.0, 1.0 + u * 4.0);
                float4 dc = zcb_puff(p, q, 4.0 + u * 3.0, 2.0 + u * 2.0, u * 1.1, fi * 0.21, T, q.y / 4.0 + 0.5);
                if (dc.a > 0.5 && lv < 0.1) col = dc;
            }
            if (T - s.z < 0.067) g = max(g, zsat(1.0 - length(p - s.xy - float2(0.0, 2.0)) / 3.5) * 0.7);
        }
    }
    // centre: the foot's own dirt burst + 10 earth chips (front half only)
    if (back < 0.5 && T >= 0.0)
    {
        float4 dc = zcb_lumps(p, p, zsat((T + 0.03) / 0.5), 20.0, _Seed + 0.9, T);
        if (dc.a > 0.5) col = dc;
        for (int k = 0; k < 10; k++)
        {
            float fk = float(k);
            float a = (fk / 10.0 + zhash1(fk + _Seed * 3.0) * 0.08) * ZTAU;
            float v = 40.0 + 40.0 * zhash1(fk * 7.0 + _Seed);
            float2 c = float2(cos(a) * v * T * 0.8, sin(a) * v * T * 0.35 + 70.0 * T - 0.5 * 360.0 * T * T);
            if (c.y > -8.0 && T < 0.42)
            {
                float lvc = gg_chip(p - floor(c + float2(0.5, 3.5)), 1.0 + zmod(fk, 2.0));
                if (lvc > -0.5) col = float4(zramp(_RampTex, 3.0, lvc < 0.5 ? 0.0 : (lvc < 3.5 ? 3.0 : 4.0)), 1.0);
            }
        }
        if (T < 0.13) g = max(g, zsat(1.0 - length((p - float2(0.0, 3.0)) / float2(9.0, 4.0))) * (T < 0.067 ? 1.0 : 0.45));
    }
    float4 res = col;
    if (glow > 0.5) res = (g > 0.05) ? zk_glow(_RampTex, min(g, _GlowCap), p, _GlowGain, 0.0) : zk_none();
    return res;
}
// ground: radial cracks that race out to the radius, crater plates at the foot; dithers away 0.45 .. 0.75 s
float4 zcb_stomp_ground(float2 p, float T)
{
    float4 col = zk_none();
    float d = length(p); float a = atan2(p.y, p.x);
    float front = _Radius * 1.05 * zeaseOut(zsat(T / 0.14));
    float fade = zsat((T - 0.45) / 0.3);
    float nz = zN(_NoiseTex, p / 3.0 + float2(_Seed * 5.0, 0.0)).r;
    if (T >= 0.0 && d < front && nz > fade)
    {
        // 9 cracks with a wandering angle; thin (1 px ink) + 1 px warm lip on the lit side
        float best = 99.0; float lip = 0.0;
        for (int i = 0; i < 9; i++)
        {
            float fi = float(i);
            float len = _Radius * (0.62 + 0.42 * zhash1(fi * 2.3 + _Seed));
            if (d < len && d > 3.0)
            {
                float ai = (fi / 9.0 + (zhash1(fi * 5.1 + _Seed) - 0.5) * 0.07) * ZTAU;
                float wob = (zN(_NoiseTex, float2(d / 7.0, fi * 0.37 + _Seed)).r - 0.5) * 0.55;
                float da = a - ai - wob * (d / max(_Radius, 1.0));
                da = da - ZTAU * floor(da / ZTAU + 0.5);
                float pxd = abs(da) * d;
                float w = 0.9 * (1.0 - d / len) + 0.45;
                // a branch at mid length
                float ab = ai + (zhash1(fi * 8.0) < 0.5 ? 0.35 : -0.35);
                float db = a - ab - wob * 0.6; db = db - ZTAU * floor(db / ZTAU + 0.5);
                float pxb = (d > len * 0.45 && d < len * 0.8) ? abs(db) * d + (d - len * 0.45) * 0.08 : 99.0;
                float m = min(pxd - w, pxb - 0.4);
                if (m < best) { best = m; lip = (da > 0.0) ? 1.0 : 0.0; }
            }
        }
        if (best < 0.0) col = float4(zramp(_RampTex, 3.0, 0.0), 1.0);
        else if (best < 1.0 && lip > 0.5) col = float4(zramp(_RampTex, 3.0, 4.0), 1.0);
        else if (best < 1.0) col = float4(zramp(_RampTex, 3.0, 2.0), 1.0);
        // sockets: the ground broken open where each spur punches through
        for (int k = 0; k < 12; k++)
        {
            float4 sp = zcb_spur(float(k));
            float2 qs = (p - sp.xy) / float2(4.5, 2.2);
            float ls = length(qs);
            if (T >= sp.z && T < sp.z + 0.62)
            {
                if (ls < 0.75) col = float4(zramp(_RampTex, 3.0, 0.0), 1.0);
                else if (ls < 1.15 && qs.y < 0.2) col = float4(zramp(_RampTex, 3.0, 4.0), 1.0);   // broken lip, lit side
                else if (ls < 1.15) col = float4(zramp(_RampTex, 3.0, 2.0), 1.0);
            }
        }
        // crater: broken plates (voronoi cell edges) inside 9 px
        float4 cz = zN(_NoiseTex, p / 6.0 + float2(_Seed * 2.0, _Seed));
        if (d < 9.0 + nz * 2.0)
        {
            if (cz.a < 0.10) col = float4(zramp(_RampTex, 3.0, 0.0), 1.0);
            else if (cz.a < 0.2 && p.y > 0.0) col = float4(zramp(_RampTex, 3.0, 4.0), 1.0);
            else if (cz.b < 0.35 && zbayer(p) < 0.0) col = float4(zramp(_RampTex, 3.0, 1.0), 1.0);
        }
    }
    return col;
}

float4 ZFX_Frag(float2 p, float2 w)
{
    float T = zcb_T();
    float glow = _Layer > 0.5 ? 1.0 : 0.0;
    float4 c = zk_none();
    if (_Mode < 0.5) c = zcb_merge(p, T, glow);
    else if (_Mode < 1.5) c = zcb_split(p, T, glow);
    else if (_Mode < 2.5) c = (glow > 0.5) ? zk_none() : zcb_stomp_ground(p, T);
    else if (_Mode < 3.5) c = zcb_stomp_spurs(p, T, glow, 0.0);
    else c = zcb_stomp_spurs(p, T, glow, 1.0);
    if (_Intensity < 0.999 && c.a > 0.5 && glow < 0.5 && zbayer(w) > _Intensity - 0.5) c = zk_none();
    if (glow > 0.5) c = float4(c.rgb * _Intensity, c.a);
    return c;
}
