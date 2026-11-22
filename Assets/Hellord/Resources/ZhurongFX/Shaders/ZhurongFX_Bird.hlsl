// Vermilion Bird (Zhuque): procedural fire bird in its own flight frame (+x = forward, mirrored for leftward flight so it
// never flies upside down). Ink-rimmed body / head / beak / crest, two beating swept wings with notched feather tips, three
// streaming tail ribbons, and a backward flame wake from every part. Pivot = body centre.
float zb_solid(float2 f, float ph, float t)
{
    float v = 0.0;
    float2 eb = float2(f.x / 6.5, f.y / 2.7);
    v = max(v, 1.0 - dot(eb, eb));                                            // body
    v = max(v, 1.0 - length(f - float2(6.6, 0.8)) / 2.5);                     // head
    if (f.x > 8.0 && f.x < 11.5) v = max(v, (1.0 - abs(f.y - 0.6) / max((11.5 - f.x) * 0.42, 0.01)) * 0.6);   // beak
    float s = cos(ph), sw = sin(ph);
    float span = 15.0 * (0.5 + 0.5 * s) + 3.0;
    float yy = abs(f.y);
    if (yy < span)
    {
        float yn = yy / span;
        float xle = 2.8 + 2.6 * sw * yn - 4.5 * yn * yn;
        float chord = 7.5 * pow(max(1.0 - yn, 0.0), 0.55) + 1.5 + 1.3 * sin(yn * 15.0 + 1.0) * yn;
        float dl = xle - f.x, dt = f.x - (xle - chord);
        if (dl > 0.0 && dt > 0.0) v = max(v, min(min(dl, dt) / 2.2, 1.0) * (1.0 - yn * 0.35));
    }
    float dx = -5.0 - f.x;                                                    // tail ribbons
    if (dx > 0.0 && dx < 19.0)
    {
        float L = 19.0;
        for (int i = -1; i < 2; i++)
        {
            float fi = float(i);
            float yc = fi * 0.28 * dx + sin(t * 7.0 + dx * 0.33 + fi * 1.7) * 1.4 * dx / L;
            float hw = 1.9 * pow(max(1.0 - dx / L, 0.0), 0.6) + 0.5 - abs(fi) * 0.3;
            v = max(v, (1.0 - abs(f.y - yc) / hw) * (1.0 - dx / L) * 0.85);
        }
    }
    return v;
}
float4 ZFX_Frag(float2 p, float2 w)
{
    float t = floor(_FxTime * _StepFps) / _StepFps;
    float2 d = normalize(float2(_Dir.x, _Dir.y) + float2(1e-4, 0.0));
    float2 f = zrot(p, -atan2(d.y, d.x)) / max(_Scale * (0.4 + 0.6 * zeaseOut(_Ignite)), 0.05);
    if (d.x < 0.0) f.y = -f.y;
    float ph = t * _FlapHz * ZTAU;
    float v = zb_solid(f, ph, t);
    float4 col = float4(0.0, 0.0, 0.0, 0.0);
    float dn = zN(_NoiseTex, p / 7.0 + float2(_Seed, t * 0.4)).r;
    float alive = 1.0 - smoothstep(dn - 0.2, dn, _Fade * 1.2);
    // flame wake streaming back
    float wake = 0.0;
    for (int k = 1; k < 9; k++)
    {
        float fk = float(k);
        float vk = zb_solid(f + float2(fk * 1.6, 0.0), ph - fk * 0.05, t - fk * 0.02);
        wake = max(wake, vk * (1.0 - fk / 9.5));
    }
    float wf = zflow(_NoiseTex, float2(f.x * 0.8 + t * 30.0, f.y * 1.4), 5.0, t, 0.0, _Seed);
    float wh = wake * (0.55 + 0.9 * wf) - 0.15;
    float4 wc = zfire(_RampTex, wh * alive, w, 0.2);
    if (wc.a > 0.5) col = wc;
    else if (wh > 0.12 && wh < 0.2 && alive > 0.5) col = float4(zramp(_RampTex, 0.0, 0.0), 1.0);
    if (v > 0.0 && alive > 0.5)
    {
        float core = zsat(1.0 - length(float2(f.x / 6.0, f.y / 2.0)));
        float heat = 0.3 + v * 0.38 + core * 0.45 + (zN(_NoiseTex, f / 6.0 + float2(t * 0.7, 0.0)).r - 0.5) * 0.25 + (1.0 - _Ignite) * 0.6;
        col = (v < 0.16) ? float4(zramp(_RampTex, 0.0, 0.0), 1.0) : zfire(_RampTex, heat, w, 0.0);
        if (length(f - float2(7.3, 1.3)) < 0.75) col = float4(zramp(_RampTex, 0.0, 0.0), 1.0);   // eye
    }
    float glowF = (zsat(v * 1.5) * 0.7 + zsat(wake) * 0.4) * alive;
    if (_Layer > 0.5) return zglow(_RampTex, glowF, w, _GlowGain);
    return col;
}
