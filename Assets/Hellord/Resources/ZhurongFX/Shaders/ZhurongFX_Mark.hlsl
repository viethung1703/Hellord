// Pyre Mark brand (style 0): a small coal seal (ink rim, crimson body, cracked hot inner glyph) with a vertical lick, bobbing;
// _Intensity (burn build-up) makes the cracks hotter and the lick taller. Pivot = point above the enemy head.
// Flammability tick (style 1, one-shot via _Progress): a 3 px lick that flares up and dies. Pivot = enemy chest.
float4 ZFX_Frag(float2 p, float2 w)
{
    float t = floor(_FxTime * _StepFps) / _StepFps;
    float4 col = float4(0.0, 0.0, 0.0, 0.0);
    float glowF = 0.0;
    if (_Style < 0.5)
    {
        float sp = zeaseOut(_Progress);
        float2 q = p - float2(0.0, 5.0 + floor(sin(t * 4.0) + 0.5));
        q = q / max(sp, 0.2);
        float dia = (abs(q.x) + abs(q.y) * 0.95) * 0.82;
        float lickH = 4.0 + 4.0 * _Intensity;
        // lick above the seal
        float fl = zflow(_NoiseTex, q + float2(_Seed * 9.0, 0.0), 3.5, t, 14.0, _Seed);
        float yl = q.y - 4.5;
        if (yl > 0.0 && yl < lickH)
        {
            float yn = yl / lickH;
            float hw = 2.2 * pow(max(1.0 - yn, 0.0), 0.7) + 0.4;
            float sx = abs(q.x - sin(t * 7.0 + yl * 0.5) * yn * 1.2);
            float lh = (1.0 - sx / hw) * 0.6 + (1.0 - yn) * 0.6 + (fl - 0.5) * 0.5;
            float4 lc = zfire(_RampTex, lh * 0.85, w, 0.3);
            if (lc.a > 0.5) col = lc;
            glowF = zsat(lh) * 0.6;
        }
        if (dia < 5.2)
        {
            float glyph = min(abs(q.x) * 1.4, abs(abs(q.x) - 1.8) + max(0.0, -q.y + 0.5));
            float hot = 0.5 + 0.5 * sin(t * (3.0 + 4.0 * _Intensity));
            if (dia > 4.2) col = float4(zramp(_RampTex, 0.0, 0.0), 1.0);
            else if (glyph < 0.7 && abs(q.y) < 3.0) col = zfire(_RampTex, 0.42 + 0.2 * hot + 0.25 * _Intensity, w, 0.0);
            else col = (zN(_NoiseTex, q / 6.0 + float2(t * 0.2, _Seed)).r > 0.5) ? float4(zramp(_RampTex, 0.0, 2.0), 1.0) : float4(zramp(_RampTex, 0.0, 1.0), 1.0);
            glowF = max(glowF, 0.25 + 0.3 * _Intensity);
        }
    }
    else
    {
        float pr = zsat(_Progress);
        float hgt = 7.0 * sin(pr * ZPI);
        float yn = p.y / max(hgt, 0.5);
        float fl = zflow(_NoiseTex, p + float2(_Seed * 9.0, 0.0), 3.0, t, 16.0, _Seed);
        if (p.y >= 0.0 && yn < 1.0)
        {
            float hw = 1.8 * pow(max(1.0 - yn, 0.0), 0.6) + 0.3;
            float lh = (1.0 - abs(p.x - sin(t * 9.0) * yn) / hw) * 0.6 + (1.0 - yn) * 0.5 + (fl - 0.5) * 0.4;
            float4 lc = zfire(_RampTex, lh, w, 0.3);
            if (lc.a > 0.5) col = lc;
            else if (lh > 0.15) col = float4(zramp(_RampTex, 0.0, 1.0), 1.0);
            glowF = zsat(lh) * 0.5;
        }
    }
    if (_Layer > 0.5) return zglow(_RampTex, glowF, w, _GlowGain);
    return col;
}
