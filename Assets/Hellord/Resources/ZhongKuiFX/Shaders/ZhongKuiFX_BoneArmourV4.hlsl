// Bone Armour v4b (Zhong Kui kit v4 movement skill M2): "for 3 s after the dash a revolving bone shell absorbs a pool of damage
// equal to 30 % of max HP, then crumbles" (t1709 report M2; code HellordZhongKui1713.NkDashAfter1713 m == 2, NkArmourAbsorb1713).
// Look from D2R Bone Armor (refs/): 2 big segmented bone CRESCENTS (chunky tapered vertebra chains, about 1/3 of their orbit) revolve
// on wide tilted orbits around the hero; a skull rides the head of the upper one. v4b (HM review of v4a "a thin white ring smeared
// over the hero"): the crescents are about 8 px thick with a full 1 px ink outline and a pale-bone value (no white halo, no glints
// while holding); the orbits are wide (rx 22-23 px, the hero is 23 px wide) and low in front, so the front pass crosses the legs and
// the arcs mostly sit OUTSIDE the hero silhouette; the default is the Back/Front split (two y-sorted quads), so the back halves go
// properly behind the hero. Bone and earth only: no ghosts, no spirit light, no flat rings / discs / washes. Portable HLSL subset.
//  form   _Form 0..1 (code: t / 0.25) in 4 steps: the crescents spiral up from the feet out of a bone-dust kick
//  hold   shapes are static, only the orbits move (15 fps steps). Front halves: bone e8b796 / ead4aa top, c28569 underside,
//         733e39 shadow lip, ink 181425 outline + vertebra notches; back halves one step darker
//  break  _Break ~0.2..1 (~0.28 s): each crescent snaps at its joints into 4 pieces that fly out, drop to feet level and crumble
//         (_Fade). Nothing is drawn at _Break >= 0.97
//  optional visuals (defaults OFF = as coded): _Pool (the crescents shorten and crack as the pool drains), _HitT (a 2-frame jolt
//         inward + ridge glint)
//  _Mode 0: everything in ONE quad over the hero (the hook as coded, layer 2): back halves are masked by the _Body ellipse.
//  _Mode 1 / 2: back / front halves only (the v4b default split: back quad y-sorted just behind the hero, front quad just in front).
// Light from the upper left.

float zba_tq() { return floor(_FxTime * _StepFps + 0.0001) / _StepFps; }
// bone ramp row 0: 0 181425 ink, 1 3e2731, 2 733e39, 3 c28569, 4 e8b796, 5 ead4aa, 6 ffffff
float4 zba_bone(float i) { return float4(zramp(_RampTex, 0.0, i), 1.0); }
float4 zba_dustc(float i) { return float4(zramp(_RampTex, 1.0, i), 1.0); }      // 0 181425 1 3e2731 2 733e39 3 b86f50 4 c28569 5 e8b796
float zba_r(float i, float k) { return zhash(float2(i * 7.31 + k * 3.17 + 1.3, _Seed * 97.0 + k * 13.7)); }
float zba_form() { return min(1.0, floor(_Form * 4.0 + 0.001) * 0.25); }       // 0, .25, .5, .75, 1
float zba_tb() { return _Break < 0.01 ? 0.0 : floor(zsat((_Break - 0.18) / 0.8) * 0.3 * _StepFps + 0.0001) / _StepFps; }   // s since the break
// crescent k (0 low, 1 high): orbit centre y, radii, screen tilt, head angle
float zba_cy(float k) { return _Orbit.y + k * 7.0; }
float zba_rx(float k) { return _Orbit.z + k * 1.0; }
float zba_ry(float k) { return _Orbit.w - k * 1.0; }
float zba_tilt(float k) { return k < 0.5 ? 0.14 : -0.12; }
// same speed, half a turn apart (+ a little jitter), so one crescent is in front while the other is behind
float zba_head(float k, float t) { return ZTAU * (_Speed * t + k * 0.5 + (zba_r(k, 6.0) - 0.5) * 0.05 + _Seed); }
// one crescent sample. q = px from the orbit centre, rx ry radii, tl tilt, hd head angle, L arc length (rad), [ua, ub] the part of
// the arc to draw (0 head .. 1 tail), crack. Returns float4(facet, depth sin(theta), u, 1 if hit) ; facet -1 none
float4 zba_arc(float2 q, float rx, float ry, float tl, float hd, float L, float ua, float ub, float crack)
{
    float4 res = float4(-1.0, 0.0, 0.0, 0.0);
    float2 qr = zrot(q, -tl);
    float2 n = float2(qr.x / rx, qr.y / ry);
    float d = length(n);
    float th = atan2(n.y, n.x);
    float cs = cos(th), sn = sin(th);
    float reff = rx * ry / max(0.001, sqrt((ry * cs) * (ry * cs) + (rx * sn) * (rx * sn)));
    float e = (d - 1.0) * reff;
    float u = zmod(hd - th, ZTAU) / L;
    if (u >= ua && u <= ub && u <= 1.0)
    {
        float spx = u * L * rx;                                               // px along the arc (stable segment spacing)
        float sp = zmod(spx, 7.0);
        float bulge = 0.82 + 0.18 * (1.0 - abs(sp / 3.5 - 1.0));               // vertebra: each 7 px segment bulges in its middle
        float hwid = (1.0 + 2.6 * sin(3.14159 * zsat(u * 1.1 - 0.03))) * bulge;   // pointed ends, about 7 px thick mid-arc
        float up = e * (sn > 0.0 ? 1.0 : -1.0);                               // > 0 on the upper (screen) edge
        if (abs(e) <= hwid)
        {
            float f = up > hwid * 0.35 ? 5.0 : (up > -hwid * 0.35 ? 4.0 : 3.0);
            if (up < -hwid + 1.0 && hwid > 2.0) f = 2.0;                      // shadow lip on the underside
            if (sp < 1.0 && u > 0.05 && u < 0.95 && abs(e) > hwid - 1.6) f = 0.0;   // vertebra notch (ink V at both edges)
            if (sp < 1.0 && u > 0.05 && u < 0.95 && abs(e) <= hwid - 1.6) f = f > 3.5 ? 3.0 : f;   // joint line, one step darker
            if (abs(up - hwid * 0.55) < 0.5 && hwid > 2.2 && u > 0.1 && u < 0.85 && (_Hot > 0.75 || _HitT < 0.14) && zmod(floor(spx / 7.0), 2.0) < 0.5) f = 6.0;   // ridge glint (hit only)
            if (crack > 0.5 && abs(zmod(spx + 3.5, 14.0) - 7.0) < 0.6 && abs(e) < hwid) f = 0.3;
            if (sn > 0.0) f = f > 4.5 ? 4.0 : (f > 3.5 ? 3.0 : (f > 2.5 ? 2.0 : f));   // back half: one step darker
            res = float4(f, sn, u, 1.0);
        }
        else if (abs(e) <= hwid + 1.0) res = float4(0.0, sn, u, 1.0);       // ink outline
    }
    else if (u > ub && u < ub + 0.12 && ub > 0.99 && abs(e) < 0.8) res = float4(-2.0, sn, u, 1.0);   // tail dust, flagged -2
    return res;
}
// the skull: cranium + jaw, facing out. back = the back of the skull (no face)
float zba_skull(float2 q, float s, float back)
{
    float res = -1.0;
    float2 r = q / max(s, 0.3);
    float ex = r.x / 3.6, ey = (r.y - 0.6) / 3.2;
    bool jaw = abs(r.x) < 2.4 && r.y >= -3.8 && r.y < -1.2;
    float ex1 = r.x / 4.6, ey1 = (r.y - 0.6) / 4.2;
    bool jaw1 = abs(r.x) < 3.4 && r.y >= -4.8 && r.y < -0.6;
    if (ex * ex + ey * ey <= 1.0 || jaw)
    {
        res = (r.x < -0.6 && r.y > -0.6) ? 5.0 : ((r.x > 1.4 || r.y < -1.8) ? 3.0 : 4.0);
        if (back < 0.5)
        {
            if (abs(r.x + 1.7) < 0.6 && abs(r.y - 2.2) < 0.6 && _Hot > 0.75) res = 6.0;
            if ((abs(r.x + 1.4) < 0.95 || abs(r.x - 1.4) < 0.95) && r.y > -0.9 && r.y < 0.9) res = 0.0;   // sockets
            if (abs(r.x) < 0.5 && r.y > -2.0 && r.y < -1.0) res = 1.0;                                    // nose
            if (jaw && abs(r.y + 2.8) < 0.5 && zmod(floor(r.x + 8.0), 2.0) < 1.0) res = 1.0;              // teeth
        }
    }
    else if (ex1 * ex1 + ey1 * ey1 <= 1.0 || jaw1) res = 0.0;
    return res;
}
// clumpy ground-hugging dust (never a flat ellipse). -> -1 none or dust index
float zba_dust(float2 q, float2 p, float rad, float dens, float seed)
{
    float res = -1.0;
    float e = (q.x / rad) * (q.x / rad) + (q.y / (rad * 0.55)) * (q.y / (rad * 0.55));
    if (e < 1.0 && rad > 1.0)
    {
        float nz = zN(_NoiseTex, p / 3.5 + float2(seed * 4.1, seed * 2.7)).r;
        if (nz * (1.15 - e * 0.6) > 1.0 - dens) { float l = q.y - q.x * 0.5; res = l > rad * 0.15 ? 5.0 : (l > -rad * 0.2 ? 4.0 : 3.0); }
    }
    return res;
}
float zba_inbody(float2 p) { float2 r = (p - _Body.xy) / max(_Body.zw, float2(1.0, 1.0)); return dot(r, r) <= 1.0 ? 1.0 : 0.0; }

float4 ZFX_Frag(float2 p, float2 w)
{
    float4 c = zk_none();
    float glow = _Layer > 0.5 ? 1.0 : 0.0;
    float tq = zba_tq();
    float fm = zba_form();
    float tb = zba_tb();
    bool broken = _Break > 0.01;
    float hj = (_HitT < 0.14 && !broken) ? 1.0 : 0.0;
    float L = 1.85 * (0.45 + 0.55 * zsat(_Pool * 1.25)) * (0.35 + 0.65 * fm);   // arc length (rad); proposed _Pool shortens it
    float crack = _Pool < 0.6 ? 1.0 : 0.0;
    float inb = zba_inbody(p);
    float bestD = 9.0; float4 best = zk_none();
    float4 under = zk_none();
    float gf = 0.0;
    if (_Break < 0.97)
    {
        for (int kk = 0; kk < 2; kk++)
        {
            float fk = float(kk);
            float rk = 0.4 + 0.6 * fm;
            float rx = zba_rx(fk) * rk - 2.0 * hj, ry = zba_ry(fk) * rk - 0.8 * hj;   // hit jolt (optional _HitT)
            float cy = lerp(0.0, zba_cy(fk), fm);
            float tl = zba_tilt(fk) * fm;
            float hd = zba_head(fk, tq - tb) - (1.0 - fm) * 2.4;
            float2 cc = float2(_Orbit.x, cy);
            for (int jj = 0; jj < 4; jj++)
            {
                float fj = float(jj);
                float ua = broken ? fj * 0.25 : 0.0, ub = broken ? fj * 0.25 + 0.25 : 1.0;
                float2 off = float2(0.0, 0.0);
                if (broken)
                {
                    float thm = hd - (fj + 0.5) * 0.25 * L;              // the piece's centre angle at the break
                    float2 rd = normalize(float2(cos(thm), sin(thm) * 0.6 + 0.0001));
                    float v = 26.0 + 16.0 * zba_r(fk * 4.0 + fj, 3.0);
                    off = rd * v * tb + float2(0.0, 30.0 * tb - 320.0 * tb * tb);
                    float yb = cy + ry * sin(thm);
                    float yg = -2.0 + sin(thm) * 5.0;
                    if (yb + off.y < yg) off.y = yg - yb;
                    if (tb < 0.001) off = float2(0.0, 0.0);
                }
                if (broken || jj < 1)
                {
                    float2 q = p - cc - off;
                    float4 a = zba_arc(q, rx, ry, tl, hd, L, ua, ub, broken ? 1.0 : crack);
                    if (broken && tb > 0.001) a.x = a.x > -1.5 && a.x < -0.5 ? a.x : a.x;   // pieces keep their shading
                    bool back = a.y > 0.0;
                    bool show = a.w > 0.5 && (_Mode < 0.5 || (_Mode < 1.5 ? back : !back));
                    if (show && _Mode < 0.5 && back && inb > 0.5 && !broken) show = false;       // behind the hero body
                    if (show && broken && _Fade > 0.62 && zN(_NoiseTex, w / 3.0 + float2(fk * 0.37 + fj * 0.11, _Seed)).r < (_Fade - 0.62) * 2.6) show = false;
                    if (show && a.x < -1.5)
                    {
                        if (!broken && fm > 0.99 && zN(_NoiseTex, w / 2.5 + float2(tq * 3.1, fk)).g > 0.45 && under.a < 0.5) under = zba_dustc(4.0);
                    }
                    else if (show && a.y < bestD) { bestD = a.y; best = a.x < 0.5 ? zba_bone(0.0) : zba_bone(floor(a.x + 0.001)); if (glow > 0.5 && a.x > 4.5 && (hj > 0.5 || (broken && tb < 0.07))) gf = max(gf, 0.8); }
                }
            }
            // the skull rides the head of the middle crescent
            if (kk == 1)
            {
                float2 hp = float2(rx * cos(hd), ry * sin(hd));
                hp = zrot(hp, tl);
                float sback = sin(hd) > 0.0 ? 1.0 : 0.0;
                float2 so = float2(0.0, 0.0);
                if (broken && tb > 0.001)
                {
                    float2 rd = normalize(float2(cos(hd), sin(hd) * 0.6 + 0.0001));
                    so = rd * 34.0 * tb + float2(0.0, 34.0 * tb - 320.0 * tb * tb);
                    float yg = -2.0 + sin(hd) * 5.0;
                    if (cy + hp.y + so.y < yg) so.y = yg - cy - hp.y;
                }
                float2 qs = p - floor(cc + hp + so + float2(0.0, 1.0)) - float2(0.5, 0.5);
                float sf = zba_skull(qs, 0.6 + 0.35 * fm, sback);
                bool show = sf > -0.5 && sin(hd) < 0.15 && (_Mode < 0.5 || (_Mode < 1.5 ? sback > 0.5 : sback < 0.5));   // deep behind: hidden
                if (show && _Mode < 0.5 && sback > 0.5 && inb > 0.5 && !broken) show = false;
                if (show && broken && _Fade > 0.62 && zN(_NoiseTex, w / 3.0 + float2(0.9, _Seed)).r < (_Fade - 0.62) * 2.6) show = false;
                float sd = sin(hd) - 0.05;
                if (show && sd < bestD) { bestD = sd; best = sf < 0.5 ? zba_bone(0.0) : zba_bone(floor(sf + 0.001)); }
            }
            // form: a bone-dust kick at the feet under each crescent (frames 0..2)
            if (fm < 0.74 && !broken && _Mode != 1.0)
            {
                float2 g0 = float2(_Orbit.x + zba_rx(fk) * 0.55 * cos(hd - 1.0), _Orbit.w * 0.4 * sin(hd - 1.0));
                float dr = zba_dust(p - g0, w, 3.0 + 4.0 * fm, 0.6 - 0.4 * fm, zba_r(fk, 5.0));
                if (dr > -0.5 && under.a < 0.5) under = zba_dustc(dr);
                if (glow > 0.5 && fm < 0.3) { float2 qg = p - g0 - float2(0.0, 2.0); float e = dot(qg, qg) / 16.0; if (e < 1.0) gf = max(gf, (1.0 - e) * 0.9); }
            }
        }
    }
    if (glow > 0.5)
    {
        float g = min(gf, _GlowCap);
        if (g > 0.04) c = zk_glow(_RampTex, g, w, _GlowGain, 2.0);
        c = float4(c.rgb * _Intensity, c.a);
    }
    else
    {
        if (under.a > 0.5) c = under;
        if (best.a > 0.5) c = best;
        if (_Intensity < 0.999 && c.a > 0.5 && zbayer(w) > _Intensity - 0.5) c = zk_none();
    }
    return c;
}
