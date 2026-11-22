// Particle look for Unity ParticleSystem (see spec.md for the custom vertex streams). d = pixel offset from the particle centre
// (snapped), size = quad size px, age 0..1, rnd 0..1, vel px/s. _Kind: 0 coal with flame tail, 1 spark, 2 smoke puff,
// 3 rock chunk, 4 gold spark, 5 dark fleck, 6 plate shard (shell).
float4 ZFX_Particle(float2 d, float size, float age, float rnd, float2 vel, float2 w)
{
    float t = floor(_FxTime * _StepFps) / _StepFps;
    float4 col = float4(0.0, 0.0, 0.0, 0.0);
    float vl = length(vel);
    float2 vd = vl > 1.0 ? vel / vl : float2(0.0, 1.0);
    float heat = (1.0 - age * age) * (0.75 + 0.25 * rnd);
    if (_Kind < 0.5)
    {
        float hr = _HeadPx * (1.0 - 0.4 * age);
        float tl = min(_TailPx * vl / max(_TailRef, 1.0), _TailPx);
        float along = dot(-d, vd), across = abs(dot(d, float2(-vd.y, vd.x)));
        if (length(d) < hr) col = zfire(_RampTex, 0.55 + heat * 0.5, w, 0.0);
        else if (along > 0.0 && along < tl && across < hr * (1.0 - along / tl) + 0.35)
        {
            float4 c = zfire(_RampTex, heat * (1.0 - along / tl) * 0.9 + 0.1, w, 0.2);
            col = (c.a > 0.5) ? c : float4(zramp(_RampTex, 1.0, 0.0), 1.0);
        }
    }
    else if (_Kind < 1.5 || (_Kind > 3.5 && _Kind < 4.5))
    {
        float r = max(_HeadPx * (1.0 - age), 0.5);
        if (abs(d.x) + abs(d.y) < r + 0.25)
            col = (_Kind > 3.5) ? zrow(_RampTex, 2.0, 8.0, 0.5 + heat * 0.5, w) : zfire(_RampTex, 0.35 + heat * 0.7, w, 0.0);
    }
    else if (_Kind < 2.5)
    {
        float r = _HeadPx * (0.6 + 0.8 * age);
        float n = zN(_NoiseTex, (d + float2(rnd * 50.0, 0.0)) / 8.0 + float2(0.0, -t * 0.3)).r;
        float dens = (1.0 - length(d) / r) * 1.8 * (n + 0.3) * (1.0 - age * age);
        if (dens > 0.45) col = float4(zramp(_RampTex, 1.0, (dens > 0.9 && age < 0.5) ? 2.0 : ((n > 0.55) ? 1.0 : 0.0)), 1.0);
    }
    else if (_Kind < 3.5 || _Kind > 5.5)
    {
        float r = _HeadPx * (0.8 + 0.4 * rnd);
        float2 dq = zrot(d, rnd * 6.0 + age * 8.0 * (rnd - 0.5));
        if (abs(dq.x) * (1.0 + rnd) + abs(dq.y) < r + 0.3)
        {
            col = float4(zramp(_RampTex, 3.0, (dq.y > 0.0) ? 2.0 : 0.0), 1.0);
            if (length(dq) < 0.8 && heat > 0.35) col = zfire(_RampTex, heat, w, 0.0);
            if (_Kind > 5.5 && abs(dq.x) < 0.6) col = zfire(_RampTex, 0.4 + heat * 0.4, w, 0.0);
        }
    }
    else
    {
        if (abs(d.x) + abs(d.y) < _HeadPx) col = float4(zramp(_RampTex, 1.0, 0.0), 1.0);
    }
    return col;
}
