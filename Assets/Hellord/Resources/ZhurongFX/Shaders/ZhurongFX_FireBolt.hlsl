// Hydra fire bolt: small, fast, any angle. White-yellow core, teardrop flame envelope streaming back with flow noise,
// ink lip on the tail edge (reads on red floors), a few trailing embers and dark smoke specks. Pivot = bolt core.
// Code drives _Dir (unit) and _TravelPx (px flown so far, so the trail never pokes behind the mouth).
float4 ZFX_Frag(float2 p, float2 w)
{
    float t = floor(_FxTime * _StepFps) / _StepFps;
    float2 d = normalize(float2(_Dir.x, _Dir.y) + float2(1e-4, 0.0));
    float2 f = zrot(p, -atan2(d.y, d.x));
    float4 col = float4(0.0, 0.0, 0.0, 0.0);
    float glowF = 0.0;
    float L = max(2.0, min(_TrailPx, _TravelPx));
    float sz = _Size * (0.7 + 0.3 * zsat(_TravelPx / 12.0));
    float nf = zN(_NoiseTex, float2(f.x * 0.18 - t * 3.1, f.y * 0.3) + float2(_Seed, _Seed * 0.6)).r;
    float flick = zN(_NoiseTex, float2(t * 1.7, _Seed)).g;
    // flame envelope
    if (f.x > -L - 1.0 && f.x < 3.0 * sz)
    {
        float xn = zsat(-f.x / L);
        float hw = (2.4 * pow(max(1.0 - xn, 0.0), 0.8) + 0.4) * sz * (0.85 + 0.3 * flick);
        float front = (f.x > 0.0) ? zsat(1.0 - f.x / (3.0 * sz)) : 1.0;
        float wav = sin(f.x * 0.9 + t * 25.0) * 0.6 * xn;
        float v = (1.0 - abs(f.y - wav) / max(hw, 0.01)) * front;
        float heat = v * (1.15 - 0.7 * xn) + (nf - 0.5) * 0.6 * (0.4 + xn);
        float4 fc = zfire(_RampTex, heat, w, 0.24);
        if (fc.a > 0.5) col = fc;
        else if (heat > 0.13 && xn > 0.25) col = float4(zramp(_RampTex, 0.0, 0.0), 1.0);
        glowF = max(glowF, zsat(v) * (1.0 - xn * 0.6));
    }
    // hot core
    float core = 1.0 - length(float2((f.x - 0.3) / (2.2 * sz), f.y / (1.5 * sz)));
    if (core > 0.0) { col = zfire(_RampTex, 0.82 + core * 0.6, w, 0.0); glowF = 1.0; }
    // trailing embers (1 art px, hashed cells behind the tail) and smoke specks
    float cx = floor(-f.x / 2.5);
    if (cx > L / 2.5 - 2.0 && cx < L / 2.5 + 3.0 && L > 6.0)
    {
        float hcell = zhash1(cx + floor(_TravelPx / 2.5) * 0.0 + _Seed * 13.0 + floor(t * 15.0) * 0.37);
        float2 ep = float2(-(cx + 0.5) * 2.5, (hcell - 0.5) * 4.0);
        if (hcell > 0.45 && length(f - ep) < 0.75) col = zfire(_RampTex, 0.55 + hcell * 0.4, w, 0.0);
        float ns = zN(_NoiseTex, f / 3.0 + float2(t * 0.4, _Seed)).g;
        if (col.a < 0.5 && -f.x > L + 0.5 && -f.x < L + 4.0 && abs(f.y) < 1.6 && ns > 0.58) col = float4(zramp(_RampTex, 1.0, 1.0), 1.0);
    }
    if (_Layer > 0.5) return zglow(_RampTex, glowF, w, _GlowGain);
    return col;
}
