// Heat-haze maths (portable). The mask is analytic (never drawn). Offset is in whole art pixels so the warp stays pixel-snapped.
// _Shape 0 ellipse (squashed), 1 column (rises _Height px above pivot), 2 ring band.
// v2.1 (built-in fix): v1 offsets were noise * 2 * _Strength * mask rounded to whole px = 0 on most pixels (the fbm stays near 0.5),
//  so the haze was invisible in game. Now: a radial lens push across the ring band (outer half pushes out, inner half pulls in) +
//  stronger noise jitter, and charged-air SHIMMER bands (dithered bright / dark 2 px refraction bands, broken by noise) that read even
//  when the floor under the ring is flat. _HazeMode 1 = no-grab fallback: the shimmer bands alone as opaque palette pixels (cutout).
float ZFX_HazeMask(float2 p)
{
    float m = 0.0;
    if (_Shape < 0.5) { float2 g = float2(p.x, p.y / _Squash); m = 1.0 - length(g) / _Radius; }
    else if (_Shape < 1.5) { float yn = p.y / _Height; m = (1.0 - abs(p.x) / (_Radius * (1.0 - 0.5 * zsat(yn)))) * step(-2.0, p.y) * (1.0 - zsat(yn)); }
    else { float2 g = float2(p.x, p.y / _Squash); m = 1.0 - abs(length(g) - _Radius) / max(_Height, 1.0); }
    return zsat(m * 1.5) * _Fade;
}
// signed position across the band: -1 inner edge .. +1 outer edge (ring); 0..1 for the other shapes
float ZFX_HazeBand(float2 p)
{
    float2 g = float2(p.x, p.y / _Squash);
    if (_Shape > 1.5) return clamp((length(g) - _Radius) / max(_Height, 1.0), -1.0, 1.0);
    return zsat(length(g) / max(_Radius, 1.0));
}
float2 ZFX_HazeOffset(float2 p)
{
    float t = floor(_FxTime * _StepFps) / _StepFps;
    float m = ZFX_HazeMask(p);
    float2 n = float2(zN(_NoiseTex, p / 12.0 + float2(_Seed, -t * 0.45)).r, zN(_NoiseTex, p / 12.0 + float2(0.5 + _Seed, -t * 0.45)).g) - float2(0.5, 0.5);
    float2 o = clamp(n * 4.0, -1.0, 1.0) * 0.6 * _Strength * m;                       // jitter
    if (_Shape > 1.5)
    {
        float2 d = p / max(length(p), 1.0);
        o += d * sin(ZFX_HazeBand(p) * 3.14159) * _Strength * m;                        // lens push across the band
    }
    return floor(o + float2(0.5, 0.5));
}
// shimmer band at p: +1 bright, -1 dark, 0 none (dithered by the mask and _Shimmer)
float ZFX_HazeShimmer(float2 p)
{
    float m = ZFX_HazeMask(p);
    float st = floor(_FxTime * _StepFps);
    float2 g = float2(p.x, p.y / _Squash);
    float r = length(g);
    float k = floor(r / 2.0 + zmod(st, 2.0) * 0.5);
    float nb = zN(_NoiseTex, p / 10.0 + float2(_Seed * 3.0, st * 0.37)).r;
    if (nb < 0.42) return 0.0;                                                          // broken, not a clean ring
    if (zbayer(p) > m * _Shimmer) return 0.0;
    return zmod(k, 2.0) < 1.0 ? 1.0 : -1.0;
}
float3 ZFX_HazeShade(float2 p, float3 col)
{
    float s = ZFX_HazeShimmer(p);
    if (s > 0.5) return lerp(col, zramp(_RampTex, 1.0, 5.0), 0.38);                     // toward c0cbdc
    if (s < -0.5) return col * 0.62;
    return col;
}
// _HazeMode 1 (no grab): opaque shimmer pixels only. a = 0 -> clip.
float4 ZFX_HazeNoGrab(float2 p)
{
    float s = ZFX_HazeShimmer(p);
    if (s > 0.5) return float4(zramp(_RampTex, 1.0, 5.0), 1.0);                        // c0cbdc bright refraction pixel
    if (s < -0.5) return float4(zramp(_RampTex, 1.0, 1.0), 1.0);                       // 262b44 dark refraction pixel
    return float4(0.0, 0.0, 0.0, 0.0);
}
