// Decrepify v4c MARK (new, proposed hook): the tell on each CURSED demon (code: e.nkDecr1713 > runTime). One small quad per
// cursed demon, pivot = the demon's feet (follows it), drawn as TWO quads with the same MPB: _Part 1 (ZKFX_Decrepify_MarkFeet,
// ground layer, over the curse area, under the sprites) and _Part 2 (ZKFX_Decrepify_Mark, over-sprite layer, like an overhead
// icon, so a demon in front never hides it). _Part 0 draws both (one quad, ysort). v4c (HM: the v4b 9x9 marks were
// "near-invisible specks"):
//  - a 15 x 12 px hovering curse SIGIL with 2 px strokes and a 1 px ink outline: a violet eye diamond (lit upper half) with an
//    ink-filled socket and a green pupil, cupped by withered horns that droop at the tips, and a crown spike (D2R-style curse
//    icon). It bobs 1 px, pops in from 6 px higher over 3 frames, and a green DRIP falls from it onto the head every 1.1 s.
//  - a WITHERING AURA at the feet: 5 writhing rot TENDRILS (2 px violet core, ink outline, green tips) creeping out 6-10 px over
//    the ground to the sides and front (never behind the sprite), and 3 withering motes (2x2 green -> violet -> gone) rising
//    beside the body. Separate tendrils and motes: no ring, no disc, no wash over the sprite.
//  - The sprite tint is CODE (the Frozen tint path, spec.md), not this shader.
//  _FxTime = seconds since this demon was cursed; _Fade = InverseLerp(expiry - 0.4, expiry, now); _Seed per demon (desync);
//  _Head = px from the feet to the top of the head (imp 36). Portable HLSL subset (also compiled as GLSL).

float zdm_tq() { return floor(_FxTime * _StepFps + 0.0001) / _StepFps; }
float4 zdm_curse(float i) { return float4(zramp(_RampTex, 4.0, i), 1.0); }   // 0 181425 1 262b44 2 3e2731 3 68386c 4 b55088
float4 zdm_seep(float i) { return float4(zramp(_RampTex, 5.0, i), 1.0); }    // 0 181425 1 193c3e 2 265c42 3 3e8948 4 63c74d
float zdm_r(float i, float k) { return zhash(float2(i * 7.31 + k * 3.17 + 1.3, _Seed * 97.0 + k * 13.7)); }
// the sigil at integer px q (origin = the eye centre): 0 none, 1 dark stroke, 2 lit stroke, 3 pupil hot, 4 pupil, 5 socket (ink)
float zdm_glyph(float2 q)
{
    float res = 0.0;
    float ax = abs(q.x);
    float dm = ax + abs(q.y);
    if (dm > 3.5 && dm < 5.5) res = q.y > 0.5 || (q.y > -0.5 && q.x < 0.5) ? 2.0 : 1.0;                  // eye diamond, 2 px stroke (11 px)
    if (dm < 3.5) res = 5.0;                                                                             // ink socket
    if (dm < 1.5) res = dm < 0.5 ? 3.0 : 4.0;                                                            // pupil: hot centre, green cross
    if (ax > 5.5 && ax < 7.5 && q.y > -2.5 && q.y < 3.5) res = q.y > 1.5 ? 2.0 : 1.0;                   // horns, 2 px, cupping the eye
    if (ax > 4.5 && ax < 6.5 && q.y > 3.5 && q.y < 5.5) res = 2.0;                                       // horn tops, curling in
    if (ax > 5.5 && ax < 6.5 && q.y > -4.5 && q.y < -2.5) res = 1.0;                                     // drooping, withered tips (taper)
    if (q.y > 5.5 && q.y < 7.5 && ax < 1.5 - (q.y - 5.5)) res = 2.0;                                     // crown spike
    return res;
}
float4 ZFX_Frag(float2 p, float2 w)
{
    float4 c = zk_none();
    float glow = _Layer > 0.5 ? 1.0 : 0.0;
    float tq = zdm_tq();
    float ph = zdm_r(0.0, 1.0);
    float bob = zmod(floor((tq + ph) * 2.4), 2.0) < 0.5 ? 0.0 : 1.0;
    float drop = tq < 0.2 ? floor((0.2 - tq) * 30.0) : 0.0;                      // pop-in: it drops 6 px into place over 3 frames
    float2 E = float2(0.0, floor(_Head + 8.0) + bob + drop);
    float2 q = floor(p) - E;
    float gf = 0.0;
    float col = -1.0;      // 10+ seep, 20+ curse
    // sigil + its 1 px ink outline
    bool top = _Part < 0.5 || _Part > 1.5;                                       // 0 all, 1 feet only (ground), 2 overhead only
    bool feet = _Part < 1.5;
    float gl = top ? zdm_glyph(q) : 0.0;
    if (gl > 4.5) col = 20.0;
    else if (gl > 0.5) col = gl > 3.5 ? 13.0 : (gl > 2.5 ? (zmod(floor((tq + ph) * 1.5), 3.0) < 0.5 ? 13.0 : 14.0) : (gl > 1.5 ? 24.0 : 23.0));
    else if (top)
    {
        float o = 0.0;
        for (int yy = -1; yy < 2; yy++)
        {
            for (int xx = -1; xx < 2; xx++) { if (zdm_glyph(q + float2(float(xx), float(yy))) > 0.5) o = 1.0; }
        }
        if (o > 0.5) col = 20.0;
    }
    if (gl > 0.5 && gl < 4.5) gf = max(gf, gl > 2.5 ? 0.6 : 0.3);
    if (top && tq < 0.14 && length(q) < 8.0) gf = max(gf, 0.9);                           // the pop flash
    // the drip: from under the eye down onto the head, every 1.1 s
    float dper = 1.1;
    float dt = zmod(tq + ph * dper, dper);
    if (top && dt < 0.6 && tq >= 0.2)
    {
        float dy = -6.0 - floor(dt / 0.6 * 8.0);
        float2 dq = floor(p) - E - float2(0.0, dy);
        if (dq.x > -0.5 && dq.x < 1.5 && dq.y > -0.5 && dq.y < (dt < 0.2 ? 1.5 : 0.5)) col = dt < 0.4 ? 14.0 : 13.0;
        if (dq.x > -0.5 && dq.x < 1.5 && dq.y > -0.5 && dq.y < 1.5) gf = max(gf, 0.4);
    }
    // withering aura at the feet: 5 writhing rot tendrils (sides and front only, on the ground)
    float grow = zsat(tq / 0.33);
    float2 pc = floor(p) + float2(0.5, 0.5);
    float td = 99.0; float tu = 0.0;
    for (int jj = 0; jj < 5; jj++)
    {
        float fj = float(jj);
        float a = 0.25 - (3.14159265 + 0.5) * (fj + 0.5 + (zdm_r(fj, 5.0) - 0.5) * 0.5) / 5.0;      // spread over the sides and front
        float2 P = float2(cos(a) * 5.5, sin(a) * 3.0);
        float hd = a + (zdm_r(fj, 6.0) - 0.5) * 0.5;
        float curl = (zdm_r(fj, 7.0) > 0.5 ? 1.0 : -1.0) * (0.3 + 0.25 * sin(tq * 3.1 + fj * 1.7 + ph * 6.0));   // they writhe
        float sl = (2.4 + 1.2 * zdm_r(fj, 8.0)) * grow;
        for (int kk = 0; kk < 3; kk++)
        {
            float fk = float(kk);
            float2 Q = P + float2(cos(hd), sin(hd) * 0.75) * sl;
            float2 ab = Q - P; float2 ap = pc - P;
            float h = zsat(dot(ap, ab) / max(0.001, dot(ab, ab)));
            float d = length(ap - ab * h);
            float u = (fk + h) / 3.0;
            float hw = 0.95 - 0.6 * u;
            if (d - hw < td) { td = d - hw; tu = u; }
            P = Q; hd = hd + curl;
        }
    }
    if (feet && tq >= 0.067)
    {
        if (td <= 0.0) col = tu > 0.8 ? 13.0 : (tu < 0.3 && td < -0.6 ? 24.0 : 23.0);          // violet core, green tip
        else if (td <= 1.0 && col < -0.5) col = 20.0;                                            // ink outline
        if (td <= 0.0 && tu > 0.75) gf = max(gf, 0.35);
    }
    // 3 withering motes rising beside the body
    for (int mm = 0; mm < 3; mm++)
    {
        float fj = float(mm);
        float per = 1.3;
        float tt = tq + fj * 0.43 + ph * per;
        float cyc = floor(tt / per);
        float km = (tt - cyc * per) / per;
        float sx = zmod(fj, 2.0) < 0.5 ? -1.0 : 1.0;
        float2 m0 = float2(sx * (7.0 + floor(3.0 * zdm_r(fj + cyc * 2.0, 3.0))), 2.0 + floor(14.0 * zdm_r(fj + cyc * 2.0, 4.0)));
        float2 pm = m0 + float2(sx * floor(km * 2.0), floor(km * 12.0));
        float2 mq = floor(p) - pm;
        bool big = km < 0.45;
        if (mq.x > -0.5 && mq.x < (big ? 1.5 : 0.5) && mq.y > -0.5 && mq.y < (big ? 1.5 : 0.5) && tq >= 0.2 && top)
        {
            float m = km < 0.25 ? 14.0 : (km < 0.5 ? 13.0 : (km < 0.8 ? 24.0 : 23.0));
            if (km < 0.8 || zbayer(w) > 0.0) col = m;
            if (km < 0.45) gf = max(gf, 0.45);
        }
    }
    // fade: dither out (the code's last 0.4 s)
    bool gone = _Fade > 0.001 && zbayer(w) + 0.5 < _Fade;   // zbayer is -0.47..0.47
    if (glow > 0.5)
    {
        float g = min(gf, _GlowCap);
        if (col < -0.5 && gf < 0.85) g = 0.0;                                       // glow only on drawn pixels (except the pop)
        if (col > 19.5 && col < 20.5) g = 0.0;                                      // never on the ink
        if (gone) g = 0.0;
        if (g > 0.04) c = zk_glow(_RampTex, g, w, _GlowGain, 2.0);
        c = float4(c.rgb * _Intensity, c.a);
    }
    else
    {
        if (col > 19.5) c = zdm_curse(col - 20.0);
        else if (col > 9.5) c = zdm_seep(col - 10.0);
        if (gone) c = zk_none();
        if (_Intensity < 0.999 && c.a > 0.5 && zbayer(w) > _Intensity - 0.5) c = zk_none();
    }
    return c;
}
