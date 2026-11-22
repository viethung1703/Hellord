// Heat-haze maths (portable). The mask is analytic (never drawn). Offset is in whole art pixels so the warp stays pixel-snapped.
// _Shape 0 ellipse (squashed), 1 column (rises _Height px above pivot), 2 ring band.
float ZFX_HazeMask(float2 p)
{
    float m = 0.0;
    if (_Shape < 0.5) { float2 g = float2(p.x, p.y / _Squash); m = 1.0 - length(g) / _Radius; }
    else if (_Shape < 1.5) { float yn = p.y / _Height; m = (1.0 - abs(p.x) / (_Radius * (1.0 - 0.5 * zsat(yn)))) * step(-2.0, p.y) * (1.0 - zsat(yn)); }
    else { float2 g = float2(p.x, p.y / _Squash); m = 1.0 - abs(length(g) - _Radius) / max(_Height, 1.0); }
    return zsat(m * 1.5) * _Fade;
}
float2 ZFX_HazeOffset(float2 p)
{
    float t = floor(_FxTime * _StepFps) / _StepFps;
    float m = ZFX_HazeMask(p);
    float2 n = float2(zN(_NoiseTex, p / 16.0 + float2(_Seed, -t * 0.45)).r, zN(_NoiseTex, p / 16.0 + float2(0.5 + _Seed, -t * 0.45)).g) - float2(0.5, 0.5);
    return floor(n * 2.0 * _Strength * m + float2(0.5, 0.5));
}
