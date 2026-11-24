// Leizu Storm Stride trail (t1666). Pivot = dash start (feet), _TargetPx = dash end (feet) in px. A dash segment leaves:
// 3 jagged afterimage streaks at body height that retract toward the end point, a white head at the end, and dark
// footfall scuffs on the floor (scorch row) that fade last. _Progress 0..1 over the trail life.
// v2 (t1666 upgrade): streaks 2 px (were 1 px), glow x1.2.
float4 ZFX_Frag(float2 p, float2 w)
{
    float2 B = _TargetPx.xy; float L = max(length(B), 1.0); float2 d = B / L; float2 n = float2(-d.y, d.x);
    float pr = zsat(_Progress);
    float st = floor(_FxTime * _StepFps);
    float s = dot(p, d) / L;                         // 0 start .. 1 end
    float gl = 0.0;
    float4 c = float4(0.0, 0.0, 0.0, 0.0);
    float tail = zeaseIn(zsat(pr / 0.75));          // tail retracts toward the end
    // footfall scuffs (on the floor), every ~9 px along the path, fade by dissolve
    float fs = floor(dot(p, d) / 9.0);
    float2 fp = d * (fs * 9.0 + 4.0) + n * ((zmod(fs, 2.0) < 0.5 ? 1.0 : -1.0) * 2.0);
    float2 fq = p - fp; fq = float2(dot(fq, d), dot(fq, n));
    float scu = zN(_NoiseTex, p / 12.0 + float2(_Seed * 3.0, 0.0)).r;
    if (abs(fq.x) <= 2.0 && abs(fq.y) <= 0.8 && fs * 9.0 + 4.0 < L && fs >= 0.0 && scu > pr * 1.1 - 0.1)
        c = float4(zramp(_RampTex, 3.0, pr < 0.3 ? 2.0 : 1.0), 1.0);
    // afterimage streaks at body height
    for (int i = 0; i < 3; i++)
    {
        float fi = float(i);
        float off = _Lift + (fi - 1.0) * _Width * 0.6;
        float2 A0 = d * L * max(tail, 0.0) + float2(0.0, off); float2 A1 = B + float2(0.0, off) - d * 3.0;
        if (dot(A1 - A0, d) < 2.0) continue;
        float u;
        float dd = lz_jag(p, A0, A1, max(2.0, floor(length(A1 - A0) / 8.0)), 1.6, lz_stepseed(_Seed + fi * 0.3, 2.0), u);
        float hot = (u > 0.8 && pr < 0.5) ? 1.0 : (pr < 0.4 ? 0.5 : 0.0);
        float keep = zhash(float2(floor(u * L / 5.0), fi + _Seed * 9.0));
        if (keep < (0.35 + 0.5 * pr) * (1.0 - u)) continue;     // broken toward the old end
        if (dd <= 0.9 && !(fi > 1.5 && hot < 0.4)) { c = float4(zramp(_RampTex, 0.0, 2.0 + hot * 2.0), 1.0); gl = max(gl, 0.6 * (0.4 + hot)); }
        else gl = max(gl, zsat(1.0 - dd / 4.0) * 0.25 * (1.0 - pr));
    }
    if (_Layer > 0.5) return zglow(_RampTex, gl, w, _GlowGain);
    return c;
}
