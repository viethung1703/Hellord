// Fire burst: compressed white core (hit-hold) -> lobed billow of tongues that rises, hollows and erodes from the
// centre -> rolling dark smoke rim -> gone. Irregular lobes from angular noise (never a disc, never spikes).
// _Style 0 pop, 1 big ground impact (low wide shock + billow), 2 gold cleanse (white-gold pulse + dark flecks leaving),
// 3 snap (embers implode for the first 35 %, then a pop). Pivot = ground / burst point.
float4 ZFX_Frag(float2 p, float2 w)
{
    float t = floor(_FxTime * _StepFps) / _StepFps;
    float pr = zsat(_Progress);
    float R = _Radius;
    float4 col = float4(0.0, 0.0, 0.0, 0.0);
    float glowF = 0.0;
    float sq = _Squash;
    // snap: implosion phase
    if (_Style > 2.5)
    {
        float ip = zsat(pr / 0.35);
        if (pr < 0.35)
        {
            float2 e0 = float2(p.x, p.y / 0.7);
            float rr0 = R * 1.25 * (1.0 - zeaseIn(ip));
            float a0 = atan2(e0.y, e0.x) + ip * 2.2;
            float k = floor((a0 / ZTAU + 0.5) * 16.0);
            float on = zhash1(k + _Seed * 31.0);
            float fk = frac((a0 / ZTAU + 0.5) * 16.0);
            float dr = abs(length(e0) - rr0 - on * 4.0);
            if (on > 0.4 && fk < 0.3 + 0.15 * on && dr < 1.0) col = zfire(_RampTex, 0.55 + 0.45 * ip + on * 0.2, w, 0.0);
            float core0 = 1.0 - length(e0) / (2.0 + 3.0 * ip);
            if (core0 > 0.0) col = zfire(_RampTex, 0.6 + core0, w, 0.0);
            glowF = zsat(core0 + 0.3) * ip * 0.8;
            if (_Layer > 0.5) return zglow(_RampTex, glowF, w, _GlowGain);
            return col;
        }
        pr = (pr - 0.35) / 0.65;
        R = R * 0.8;
    }
    float big = (_Style > 0.5 && _Style < 1.5) ? 1.0 : 0.0;
    float g = zeaseOut(pr / 0.32);
    float life = 1.0 - pr;
    float2 q = p - float2(0.0, _Rise * pow(max(pr, 0.0), 0.75));
    float2 e = float2(q.x, q.y / sq);
    float a = atan2(e.y, e.x);
    float lob = zN(_NoiseTex, float2(cos(a), sin(a)) * 0.55 + float2(_Seed, pr * 0.12)).r;
    float warp = zN(_NoiseTex, e / 15.0 + float2(_Seed * 3.1, -t * 0.3)).g - 0.5;
    float Rt = R * (0.22 + 0.78 * g);
    float d = (length(e) + warp * R * 0.32) / (Rt * (0.72 + 0.55 * lob));
    float det = zflow(_NoiseTex, q + float2(_Seed * 40.0, 0.0), 6.0, t, 24.0, _Seed);
    float heat = (1.0 - d) * 1.25 * pow(max(life, 0.0), 0.55) + (det - 0.5) * 0.7 - pr * pr * 0.8 * (0.5 + det);
    heat -= smoothstep(0.45, 1.0, pr) * zsat(1.0 - d * 1.3) * 0.9;           // centre burns out first
    // big impact: an extra low, wide ground-hugging shock of flame
    if (big > 0.5)
    {
        float2 e2 = float2(p.x, p.y / 0.32);
        float d2 = (length(e2) + warp * R * 0.4) / (R * (0.3 + 1.05 * zeaseOut(pr / 0.4)) * (0.8 + 0.4 * lob));
        float shock = (1.0 - abs(d2 - 0.85) / 0.3) * (1.0 - zsat(pr / 0.6)) * 1.3 + (det - 0.5) * 0.5;
        heat = max(heat, shock * step(p.y, R * 0.3));
    }
    heat = min(heat, 0.78);
    float coreR = R * (0.18 + pr * 1.2);
    float core = (pr < 0.14) ? (1.0 - length(float2(p.x, p.y / 0.85)) / coreR) * _Hot : 0.0;
    heat = max(heat, core * 1.4);
    glowF = zsat((1.0 - d) * 1.2) * life * 1.1 + zsat(core);
    // smoke: rolling rim that lifts and thins
    float2 qs = p - float2(0.0, _Rise * pr + R * 0.55 * pr);
    float2 es = float2(qs.x, qs.y / (sq * 0.9));
    float ds = (length(es) + warp * R * 0.45) / (R * (0.45 + 0.75 * g) * (0.8 + 0.45 * lob));
    float sdet = zflow(_NoiseTex, qs + float2(17.0, _Seed * 9.0), 8.0, t, 10.0, _Seed + 0.5);
    float sm = _SmokeAmt * smoothstep(0.08, 0.35, pr) * (1.0 - smoothstep(0.7, 1.0, pr)) * zsat(1.25 - ds) * (sdet * 2.2 - 0.75 + 0.35 * zsat(qs.y / R));
    if (_Style > 1.5 && _Style < 2.5)
    {
        // cleanse: dark flecks (ailments) thrown outward, no smoke
        float rad = length(float2(p.x, p.y / 0.8));
        float k2 = floor((a / ZTAU + 0.5) * 22.0);
        float fr = R * (0.3 + 1.3 * zeaseOut(pr)) + zhash1(k2 * 3.0) * 6.0;
        bool fleck = zhash1(k2 + 5.0 + _Seed * 7.0) > 0.45 && frac((a / ZTAU + 0.5) * 22.0) < 0.3 && abs(rad - fr) < 1.0 && pr > 0.1 && pr < 0.85;
        if (fleck) col = float4(zramp(_RampTex, 1.0, 1.0), 1.0);
        float4 gc = (heat > 0.18) ? zrow(_RampTex, 2.0, 8.0, (heat - 0.18) / 0.82, w) : float4(0.0, 0.0, 0.0, 0.0);
        if (gc.a > 0.5) col = gc;
    }
    else
    {
        if (zclump(_NoiseTex, sm, qs, _Seed + 0.2)) col = zrow(_RampTex, 1.0, 3.0, sm * 0.5 + 0.2, w);
        float4 fc = zfire(_RampTex, heat, w, 0.16);
        if (fc.a > 0.5) col = fc;
        else if (heat > 0.08 && zdithS(heat * 5.0 - 0.1, w, 0.6)) col = float4(zramp(_RampTex, 1.0, 0.0), 1.0);   // ink lip between fire and smoke
    }
    if (_Layer > 0.5) return zglow(_RampTex, glowF, w, _GlowGain);
    return col;
}
