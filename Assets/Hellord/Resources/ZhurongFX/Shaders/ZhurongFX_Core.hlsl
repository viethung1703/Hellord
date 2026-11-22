// Zhurong fx_v2 (t1622) shared shader maths. MOCKUP-verified, see spec.md.
// Written in a portable HLSL subset: the SAME file is compiled by Unity (HLSL) and by the preview renderer (GLSL via a #define prelude),
// so the MOCKUP shows exactly this maths. Rules for editors: float literals with a decimal point, no scalar->vector broadcasts,
// no fmod/mul/matrices/arrays, scalar ternaries only.
// Space: p = art pixel centre relative to the effect pivot (16 PPU, y up), already snapped to the world pixel grid.
#ifndef ZHURONG_FX_CORE
#define ZHURONG_FX_CORE

#ifndef ZFX_GLSL
  #define ZCONST static const
  #define ZTEX(t, uv) tex2Dlod(t, float4((uv), 0.0, 0.0))
#endif

ZCONST float ZPI = 3.14159265;
ZCONST float ZTAU = 6.2831853;

float zmod(float x, float y) { return x - y * floor(x / y); }
float zsat(float x) { return clamp(x, 0.0, 1.0); }
float2 zrot(float2 v, float a) { float c = cos(a), s = sin(a); return float2(c * v.x - s * v.y, s * v.x + c * v.y); }
// stable float hash (no sin, identical on GPU / preview)
float zhash(float2 p)
{
    float3 q = frac(float3(p.x, p.y, p.x) * 0.1031);
    q += dot(q, float3(q.y, q.z, q.x) + 33.33);
    return frac((q.x + q.y) * q.z);
}
float zhash1(float x) { return zhash(float2(x, x * 1.618 + 7.0)); }
// 4x4 ordered dither in [-0.5, 0.5)
float zbayer(float2 p)
{
    float x = zmod(floor(p.x), 4.0), y = zmod(floor(p.y), 4.0);
    float a = zmod(x, 2.0), b = zmod(y, 2.0), c = floor(x * 0.5), d = floor(y * 0.5);
    float s2 = (b < 0.5) ? ((a < 0.5) ? 0.0 : 2.0) : ((a < 0.5) ? 3.0 : 1.0);
    float s1 = (d < 0.5) ? ((c < 0.5) ? 0.0 : 2.0) : ((c < 0.5) ? 3.0 : 1.0);
    return (s2 * 4.0 + s1) / 16.0 - 0.46875;
}
// noise texture: R fbm, G fbm2, B voronoi cell id, A voronoi edge (0 = on a crack). Tiles every 1.0.
float4 zN(sampler2D t, float2 uv) { return ZTEX(t, uv); }
// ramp rows (ZFX_Ramp16x8): 0 fire, 1 smoke, 2 gold, 3 rock, 4 curse, 5 ash, 6 bloom
float3 zramp(sampler2D r, float row, float idx) { return ZTEX(r, float2((clamp(idx, 0.0, 15.0) + 0.5) / 16.0, 1.0 - (row + 0.5) / 8.0)).rgb; }
// banded index with ordered dither: v in [0,1] -> 0..n-1
float zband(float v, float n, float2 p, float ditherAmt) { return clamp(floor(v * n + zbayer(p) * ditherAmt + 0.5), 0.0, n - 1.0); }
// fire ramp: heat 0..1 -> index 1..7 (0 ink is reserved for rims). alpha 0 below 'cut'.
float4 zfire(sampler2D r, float heat, float2 p, float cut)
{
    if (heat < cut) return float4(0.0, 0.0, 0.0, 0.0);
    float i = 1.0 + zband(pow(zsat((heat - cut) / (1.0 - cut)), 1.35), 7.0, p, 0.5);
    return float4(zramp(r, 0.0, i), 1.0);
}
float4 zrow(sampler2D r, float row, float n, float v, float2 p)
{
    return float4(zramp(r, row, zband(zsat(v), n, p, 0.5)), 1.0);
}
// dithered coverage test: true if density beats the bayer threshold
bool zdith(float dens, float2 p) { return dens > zbayer(p) * 0.45 + 0.5; }
bool zdithS(float dens, float2 p, float amt) { return dens > zbayer(p) * amt + 0.5; }
// clumpy coverage (organic blotches, no checkerboard): density vs high-frequency noise
bool zclump(sampler2D n, float dens, float2 p, float seed) { return dens > ZTEX(n, p / 9.0 + float2(seed, seed * 0.7)).g * 0.8 + 0.1; }
// rising flow noise: domain-warped fbm scrolling up (pixel space in, 0..1 out)
float zflow(sampler2D n, float2 p, float scale, float t, float speed, float seed)
{
    float2 uv = p / scale;
    float w = ZTEX(n, uv * 0.5 + float2(seed, t * 0.11)).g - 0.5;
    return ZTEX(n, uv + float2(w * 0.35 + seed * 0.37, -t * speed / scale + w * 0.2)).r;
}
float zsegd(float2 q, float2 a, float2 b) { float2 ab = b - a; float h = zsat(dot(q - a, ab) / max(1e-4, dot(ab, ab))); return length(q - a - ab * h); }
float zeaseOut(float x) { x = zsat(x); return 1.0 - (1.0 - x) * (1.0 - x); }
float zeaseIn(float x) { x = zsat(x); return x * x; }
// glow output helper: banded additive glow from a smooth field
float4 zglow(sampler2D r, float field, float2 p, float gain)
{
    float b = zband(zsat(field), 4.0, p, 0.45) / 3.0;
    if (b <= 0.0) return float4(0.0, 0.0, 0.0, 0.0);
    float3 c = zramp(r, 6.0, 1.0 + b * 5.0);
    return float4(c * b * gain * 0.45, 1.0);
}
#endif
