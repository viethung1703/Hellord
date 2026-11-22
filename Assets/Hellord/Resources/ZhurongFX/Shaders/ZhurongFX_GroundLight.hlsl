// Blotchy, banded additive floor light (never a flat disc): noise-broken ellipse / ring, 3 bands + ordered dither, flicker.
float4 ZFX_Frag(float2 p, float2 w)
{
    float t = floor(_FxTime * _StepFps) / _StepFps;
    float2 gp = float2(p.x, p.y / _Squash);
    float r = length(gp);
    float f = (_Shape < 0.5) ? zsat(1.0 - r / _Radius) : zsat(1.0 - abs(r - (_Radius + _Inner) * 0.5) / max((_Radius - _Inner) * 0.5, 1.0));
    float n = zN(_NoiseTex, gp / 20.0 + float2(_Seed, t * 0.06)).r;
    float fl = 1.0 - _Flicker + _Flicker * zN(_NoiseTex, float2(t * 0.9, _Seed)).g * 1.3;
    float v = f * (0.35 + 0.9 * n) * _Intensity * fl;
    float b = zband(zsat(v), 4.0, w, 0.35) / 3.0;
    if (b <= 0.0) return float4(0.0, 0.0, 0.0, 0.0);
    float3 c = zramp(_RampTex, _Row, (_Row > 5.5) ? (1.0 + b * 3.0) : (3.0 + b * 3.0));
    return float4(c * b * _GlowGain * 0.3, 1.0);
}
