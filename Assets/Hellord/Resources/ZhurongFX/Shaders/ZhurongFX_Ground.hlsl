// Ground fire material (areas, rings, paths). Layers bottom -> top: cracked scorch / oil / ash -> ember-lit cracks (bright at the edge,
// quiet dark body) -> coals -> thin dark smoke -> creeping flame tongues standing on the footprint (vertical columns, 3/4 view).
// Ring shapes are broken material bands (angular gaps); never an outline or a fill.
float zg_sd(float2 p, float t)
{
    float2 gp = float2(p.x, p.y / _Squash);
    float sd = 0.0;
    float amp = 0.0;
    float2 nuv = gp / 30.0 + float2(_Seed * 5.0, 0.0);
    if (_Shape < 0.5) { sd = length(gp) - _Radius; amp = _Radius * 0.32; }
    else if (_Shape < 1.5)
    {
        float rm = (_Radius + _Inner) * 0.5, hw = (_Radius - _Inner) * 0.5;
        float2 rg = zrot(gp, -t * _Swirl);
        nuv = rg / 30.0 + float2(_Seed * 5.0, 0.0);
        sd = abs(length(gp) - rm) - hw;
        amp = hw * 0.55;
        float ang = atan2(rg.y, rg.x);
        float gap = zN(_NoiseTex, float2(cos(ang), sin(ang)) * 0.33 + float2(_Seed * 2.0, 0.71)).g;
        sd += smoothstep(0.56, 0.7, gap) * hw * 2.4;                       // broken band
    }
    else
    {
        float2 dg = normalize(float2(_Dir.x, _Dir.y / _Squash));
        sd = zsegd(gp, float2(0.0, 0.0), dg * _Length) - _Inner;
        amp = _Inner * 0.9;
    }
    return sd + (zN(_NoiseTex, nuv).r - 0.5) * 2.0 * amp;
}
float4 ZFX_Frag(float2 p, float2 w)
{
    float t = floor(_FxTime * _StepFps) / _StepFps;
    float2 gp = float2(p.x, p.y / _Squash);
    float sd = zg_sd(p, t);
    float ign = _Ignite, cool = _Cool;
    float R = _Radius;
    if (_Shape > 1.5)
    {
        float2 dg = normalize(float2(_Dir.x, _Dir.y / _Squash));
        float along = dot(gp, dg);
        float ageS = (_TrailHead - along) / max(_TrailSpeed, 1.0);
        ign = (along > _TrailHead + 2.0 || along < -_Inner * 2.0) ? 0.0 : zsat(ageS / 0.16 + 0.15);
        cool = zsat((ageS - _TrailHold) / max(_TrailFade, 0.01));
        R = _Inner * 2.0;
    }
    else if (_Shape < 0.5 && _Style < 1.5)
    {
        ign = zsat((_Ignite * 1.35 * R - length(gp)) / (R * 0.35) + 0.25);    // spreads from the centre
    }
    float live = ign * (1.0 - cool);
    float4 col = float4(0.0, 0.0, 0.0, 0.0);
    float glowF = 0.0;
    bool tele = (_Style > 1.5 && _Style < 2.5);
    bool curse = (_Style > 0.5 && _Style < 1.5);
    // ---------- scorch / oil / umbra
    float scR = tele ? R * zsat(_Progress / 0.85) : R;
    float sdS = tele ? (length(gp) - scR + (zN(_NoiseTex, gp / 16.0 + float2(_Seed * 5.0, 0.0)).r - 0.5) * scR * 0.5) : sd;
    float dis = zN(_NoiseTex, gp / 11.0 + float2(0.3, _Seed)).g;
    float scorchAmt = zsat(ign * 4.0 - 1.2) * (1.0 - smoothstep(0.55, 1.0, cool + (dis - 0.5) * 0.5));
    float sdens = zsat(-sdS / 4.0 + 0.45) * scorchAmt;
    float edgeB = zsat(1.0 + sdS / (R * 0.45));                              // 1 at the edge, 0 deep inside
    if (zclump(_NoiseTex, sdens, gp, _Seed))
    {
        float sn = zN(_NoiseTex, gp / 13.0 + float2(_Seed, 0.5)).r;
        if (curse) col = float4(zramp(_RampTex, 4.0, (sn > 0.64) ? 3.0 : ((sn > 0.42) ? 2.0 : 1.0)), 1.0);
        else if (cool > 0.3 && sn > 0.6) col = float4(zramp(_RampTex, 5.0, 2.0 + step(0.72, sn)), 1.0);
        else col = float4(zramp(_RampTex, 5.0, (sn > 0.5) ? 1.0 : 0.0), 1.0);
        if (tele && length(gp) < scR * 0.98 && sn > 0.42 + 0.45 * edgeB) col = float4(0.0, 0.0, 0.0, 0.0);   // umbra: open inside
    }
    // ---------- cracks
    float e = zN(_NoiseTex, gp / _CrackScale + float2(_Seed * 0.7, 0.2)).a;
    bool crack = e < 0.085 && sdS < -0.5;
    if (curse)
    {
        float ang = atan2(gp.y, gp.x);
        float spoke = abs(sin(ang * 3.5 + (zN(_NoiseTex, gp / 34.0).r - 0.5) * 2.4 + _Seed * 6.0));
        crack = (crack || (spoke < 0.17 && length(gp) > 5.0)) && length(gp) < R * 1.15 * zsat(_Ignite * 1.2) && sd < -1.0;
    }
    if (crack && scorchAmt > 0.05)
    {
        float pulse = zN(_NoiseTex, gp / 40.0 + float2(t * 0.09, -t * 0.05)).r;
        float ch = (0.3 + 0.55 * edgeB + 0.35 * (pulse - 0.4) + (curse ? 0.15 : 0.0)) * zsat(ign * 2.0) * (1.0 - cool);
        if (tele) ch = (0.3 + 0.6 * edgeB) * zsat(_Progress * 2.0) + _Blink;
        float4 cc = zfire(_RampTex, ch, w, 0.18);
        col = (cc.a > 0.5) ? cc : float4(zramp(_RampTex, (cool > 0.4) ? 5.0 : 0.0, (cool > 0.4) ? 2.0 : 0.0), 1.0);
        glowF = max(glowF, ch * 0.45);
    }
    // ---------- telegraph: tightening sun shadow
    if (tele)
    {
        float shR = R * (0.12 + 0.5 * (1.0 - _Shadow));
        float shd = (length(gp) + (zN(_NoiseTex, gp / 12.0).r - 0.5) * 6.0) / shR;
        if (_Shadow > 0.01 && zclump(_NoiseTex, (1.0 - shd) * 1.5 * (0.4 + 0.6 * _Shadow), gp, _Seed + 0.6)) col = float4(zramp(_RampTex, 1.0, 0.0), 1.0);
    }
    // ---------- coals
    if (sd < 0.0 && _Coal > 0.0)
    {
        float cn = zN(_NoiseTex, gp / 16.0 + float2(_Seed * 3.0, 0.9)).g;
        float th = 1.0 - _Coal * 0.32;
        if (cn > th)
        {
            float flick = 0.7 + 0.3 * sin(t * 3.0 + cn * 40.0);
            float ch = ((cn - th) / (1.0 - th) * 0.75 + 0.35) * flick * zsat(ign * 1.6) * (1.0 - cool * 0.85);
            float4 cc = zfire(_RampTex, ch, w, 0.12);
            col = (cc.a > 0.5) ? cc : float4(zramp(_RampTex, 5.0, 1.0), 1.0);
            glowF = max(glowF, ch * 0.5);
        }
    }
    // ---------- creeping tongues (columns rising from the footprint)
    float heat = 0.0;
    float Hm = _FlameH * live;
    if (Hm > 0.5)
    {
        // discrete tapered tongues: sources on a 7x4 px grid, each a hash-gated tongue standing on the footprint
        float cw = 6.0, ch = 4.0;
        float cx0 = floor(p.x / cw), cy0 = floor(p.y / ch);
        for (int j = 0; j < 10; j++)
        {
            float cy = cy0 - float(j);
            if (float(j) * ch > Hm + ch) break;
            for (int i = -1; i < 2; i++)
            {
                float cx = cx0 + float(i);
                float2 cid = float2(cx, cy) + float2(_Seed * 97.0, 0.0);
                float hs = zhash(cid);
                float2 src = float2((cx + 0.2 + 0.6 * zhash(cid + 3.1)) * cw, (cy + zhash(cid + 7.7)) * ch);
                float sds = zg_sd(src, t);
                if (sds > -1.0) continue;
                float2 gs = float2(src.x, src.y / _Squash);
                float2 rs = zrot(gs, -t * _Swirl);
                float gn = zN(_NoiseTex, rs / 110.0 + float2(_Seed, t * _Creep * 0.05)).g;
                float clus = zsat((gn - (0.78 - _Flame * 0.45)) * 5.0);
                if (clus * (0.6 + 0.6 * hs) < 0.35) continue;
                if (curse && sds < -7.0) continue;                           // rim tongues only
                float h = Hm * (0.3 + 0.7 * hs) * (0.55 + 0.45 * clus) * (0.7 + 0.3 * sin(t * (5.0 + 4.0 * hs) + hs * 40.0)) * zsat(-sds / 4.0 + 0.3);
                float dy = p.y - src.y;
                if (dy < -1.0 || dy > h) continue;
                float yn = zsat(dy / max(h, 1.0));
                float sway = sin(t * 6.0 + hs * 30.0 + dy * 0.35) * yn * yn * 3.0;
                float prof = (yn < 0.25) ? sqrt(max(0.0, 1.0 - (0.25 - yn) * (0.25 - yn) * 10.0)) : pow(1.0 - (yn - 0.25) / 0.75, 0.75);
                float hw = (1.6 + 0.14 * h + 1.0 * zhash(cid + 1.3)) * prof + 0.4;
                float dx = abs(p.x - src.x - sway);
                if (dx > hw) continue;
                float fe = zflow(_NoiseTex, p + float2(hs * 40.0, 0.0), 4.0, t, 22.0, _Seed + hs);
                float f = (1.0 - yn) * (0.45 + 0.35 * hs) + (1.0 - dx / hw) * 0.35 - (1.0 - fe) * yn * 0.8 + 0.12;
                heat = max(heat, f);
            }
        }
        float fl = zflow(_NoiseTex, p + float2(_Seed * 30.0, 0.0), 5.0, t, 20.0, _Seed);
        heat *= 0.8 + 0.4 * fl;
    }
    // ---------- smoke above the burning footprint
    float sAbove = zsat(-zg_sd(p - float2(0.0, 9.0 + _FlameH * 0.5), t) / 7.0) * live;
    float sf = zflow(_NoiseTex, p + float2(0.0, _Seed * 21.0), 9.0, t, 8.0, _Seed + 0.25);
    float smk = _SmokeAmt * sAbove * (sf * 1.8 - 0.55);
    if (heat < 0.2 && zclump(_NoiseTex, smk, p + float2(0.0, -t * 9.0), _Seed + 0.3)) col = zrow(_RampTex, 1.0, 3.0, smk * 0.6, w);
    float4 fc = zfire(_RampTex, heat, w, 0.2);
    if (fc.a > 0.5) col = fc;
    else if (heat > 0.12) col = float4(zramp(_RampTex, 0.0, 1.0), 1.0);     // dark lip around tongues (reads on red floors)
    glowF = max(glowF, heat * 0.5) * 0.8;
    if (_Layer > 0.5) return zglow(_RampTex, glowF, w, _GlowGain);
    return col;
}
