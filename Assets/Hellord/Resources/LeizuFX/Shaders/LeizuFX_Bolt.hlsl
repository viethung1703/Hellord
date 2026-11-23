// Leizu bolt family (t1666): one jagged lightning stroke from the pivot (source) to _TargetPx (destination, art px from the pivot).
// Used for sky strikes (Drum Strikes, Judgement, Decree, Tribunal), chain links (Heaven-Splitting Arc, Judgement arcs, Ward zap)
// and the held Live Wire beam. Endpoint API: code sets _Origin = source (units) and _TargetPx = (dest - source) * 16 every frame.
// Strike / link life (_Progress 0..1) follows the approved Storm Ring v4 frames: fresh white bolt with forks -> cyan/blue thinner
// -> 1 px blue residual that breaks up and is gone. Beam (_Mode 1) re-jitters every _Rejit steps and never goes residual.
float lz_boltfield(float2 p, float2 A, float2 B, float seed, float n, float amp, float forks, float hotForks, out float u, out float dfork)
{
    float d = lz_jag(p, A, B, n, amp, seed, u);
    dfork = 100000.0;
    float L = length(B - A); float2 dir = (B - A) / max(L, 1.0);
    for (int i = 0; i < 3; i++)
    {
        float fi = float(i);
        if (fi >= forks) break;
        float h1 = zhash(float2(seed * 37.0 + fi * 5.3, 1.7)); float h2 = zhash(float2(seed * 17.0 + fi * 2.9, 8.1));
        float kb = 1.0 + floor(h1 * max(n * 0.55, 1.0));
        float2 S = lz_knot(A, B, kb, n, amp, seed);
        float side = (h2 > 0.5) ? 1.0 : -1.0;
        float2 fd = zrot(dir, side * (0.35 + 0.4 * frac(h2 * 7.3)));   // 20..43 deg off the main line
        float fl = L * _ForkLen * (0.6 + 0.4 * frac(h1 * 3.7));
        float2 E = S + fd * fl;
        float fu;
        float df = lz_jag(p, S, E, max(2.0, floor(fl / _SegLen + 0.5)), amp * 0.6, seed + fi * 0.31 + 0.17, fu);
        df += fu * 1.2 * (1.0 - hotForks);      // forks thin towards the tip
        dfork = min(dfork, df);
    }
    return d;
}
float4 ZFX_Frag(float2 p, float2 w)
{
    float2 A = float2(0.0, 0.0); float2 B = _TargetPx.xy;
    float L = max(length(B - A), 1.0);
    float n = max(2.0, floor(L / max(_SegLen, 2.0) + 0.5));
    float pr = zsat(_Progress);
    float hot; float width; float forks; float seed; float reveal = 1.0; float breakup = 0.0;
    if (_Mode > 0.5 && _Mode < 1.5)
    {   // held beam: crackles in place, white flicker on alternate re-jitters
        seed = lz_stepseed(_Seed, _Rejit);
        float st = floor(_FxTime * _StepFps / max(_Rejit, 1.0));
        hot = zmod(st, 2.0) < 0.5 ? 1.0 : 0.5;
        width = _Width + floor(_Overload * 1.99);
        forks = _Forks + floor(_Overload * 2.0);
    }
    else
    {
        seed = _Seed;
        hot = pr < 0.2 ? 1.0 : (pr < 0.42 ? 0.5 : 0.0);
        width = pr < 0.2 ? _Width : (pr < 0.42 ? max(_Width - 1.0, 0.0) : 0.0);
        forks = pr < 0.42 ? _Forks : min(_Forks, 1.0);
        if (_Mode > 1.5) reveal = zsat(pr / 0.12);                     // chain link shoots out from the source
        breakup = zsat((pr - 0.45) / 0.55);                            // residual breaks up and is gone by 1
        if (pr >= 0.42) seed = frac(seed + floor(_FxTime * _StepFps / max(_Rejit, 1.0)) * 0.013); // residual crawls a little
    }
    float amp = _Amp * (1.0 + 0.35 * _Overload);
    float u; float dfork;
    float d = lz_boltfield(p, A, B, seed, n, amp, forks, hot, u, dfork);
    // twin strand while overloaded
    float d2 = 100000.0;
    if (_Mode > 0.5 && _Mode < 1.5 && _Overload > 0.01)
    {
        float u2; d2 = lz_jag(p, A, B, n + 1.0, amp * 1.4, seed + 0.53, u2);
    }
    // breakup: residual dissolves in 3 px chunks along the stroke (never single-pixel glitter)
    float chunk = zhash(float2(floor(u * L / 4.0), floor(_Seed * 97.0)));
    bool alive = (u <= reveal + 0.001) && (chunk >= breakup);
    bool falive = (dfork < 100.0) && (chunk * 0.8 >= breakup * 1.2 - 0.05) && (reveal > 0.99);
    // stars at the ends
    float2 qe = p - B; float2 qs = p - A;
    float sh = hot > 0.75 ? 1.0 : (hot > 0.35 ? 0.6 : 0.0);
    float se = _EndStar * sh * (reveal > 0.99 ? 1.0 : 0.0);
    float slen = 3.0 + 4.0 * se * (_Width + 1.0);
    float fe = se > 0.0 ? lz_star(qe, slen, 0.5) : 0.0;
    float fs = (_StartStar * sh > 0.0) ? lz_star(qs, 2.0 + 3.0 * _StartStar * sh, 0.5) : 0.0;
    if (_Layer > 0.5)
    {
        float g = 0.0;
        float gr = 4.0 + 2.0 * width + 1.5 * _Overload;
        if (alive) g = max(g, zsat(1.0 - d / gr) * (0.35 + 0.65 * hot) * (1.0 - breakup));
        if (falive) g = max(g, zsat(1.0 - dfork / (gr * 0.7)) * 0.6 * hot);
        if (d2 < 100.0) g = max(g, zsat(1.0 - d2 / gr) * 0.45);
        g = max(g, zsat(1.0 - length(qe * float2(1.0, 1.6)) / (8.0 + 6.0 * _Width)) * se);
        g = max(g, zsat(1.0 - length(qs) / 6.0) * _StartStar * sh);
        return zglow(_RampTex, g, w, _GlowGain);
    }
    float4 c = float4(0.0, 0.0, 0.0, 0.0);
    if (alive) c = lz_boltcol(_RampTex, d, width, hot, true);
    if (c.a < 0.5 && falive) c = lz_boltcol(_RampTex, dfork, max(width - 1.0, 0.0), hot * 0.6, hot > 0.35);
    if (c.a < 0.5 && d2 < 100.0)
    {
        c = lz_boltcol(_RampTex, d2, 0.0, 0.5, false);
        if (c.a < 0.5 && d2 <= 1.5) c = float4(zramp(_RampTex, 2.0, 3.0), 1.0);
    }
    // violet fringe (judgement / overload): replaces the ink outline ring
    if ((_Tint > 0.5 || _Overload > 0.5) && c.a > 0.5 && dot(c.rgb, float3(1.0, 1.0, 1.0)) < 0.35) c = float4(zramp(_RampTex, 2.0, 2.0), 1.0);
    // end / start stars on top
    if (fe > 0.0) { float3 sc = fe > 0.55 ? zramp(_RampTex, 0.0, 4.0) : zramp(_RampTex, 0.0, 3.0); c = float4(sc, 1.0); }
    if (fs > 0.0) { float3 sc = fs > 0.5 ? zramp(_RampTex, 0.0, 4.0) : zramp(_RampTex, 0.0, 3.0); c = float4(sc, 1.0); }
    return c;
}
