// Leizu combat FX (t1666) shared shader maths, adapted from the approved Zhurong fx_v2 core (t1622). MOCKUP-verified, see spec.md.
// Written in a portable HLSL subset: the SAME file is compiled by Unity (HLSL) and by the preview renderer (GLSL via a #define prelude),
// so the MOCKUP shows exactly this maths. Rules for editors: float literals with a decimal point, no scalar->vector broadcasts,
// no fmod/mul/matrices/arrays, scalar ternaries only.
// Space: p = art pixel centre relative to the effect pivot (16 PPU, y up), already snapped to the world pixel grid.
#ifndef LEIZU_FX_CORE
#define LEIZU_FX_CORE

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
// ramp rows (LZF_Ramp16x8): 0 fire, 1 smoke, 2 gold, 3 rock, 4 curse, 5 ash, 6 bloom
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

// ---------------- Leizu lightning helpers (t1666) ----------------
// ramp rows (LZF_Ramp16x8): 0 bolt (ink, deep blue, blue, cyan, white), 1 dust, 2 violet, 3 scorch, 4 drum, 5 dazzle, 6 bloom
// perpendicular knot offset of a jagged bolt: ends pinned, sqrt(sin) envelope, hash per knot + seed
float lz_off(float k, float n, float amp, float seed)
{
    if (k <= 0.0 || k >= n) return 0.0;
    float env = sqrt(sin(ZPI * k / n));
    return (zhash(float2(k * 1.37 + 3.1, seed * 91.7 + 0.3)) - 0.5) * 2.0 * amp * env;
}
// knot position k of the jagged bolt A->B (n segments)
float2 lz_knot(float2 A, float2 B, float k, float n, float amp, float seed)
{
    float2 d = B - A; float L = max(length(d), 1.0); float2 nr = float2(-d.y, d.x) / L;
    return A + d * (k / n) + nr * lz_off(k, n, amp, seed);
}
// distance from p to the jagged polyline A->B; u = 0..1 along it
float lz_jag(float2 p, float2 A, float2 B, float n, float amp, float seed, out float u)
{
    float2 d = B - A; float L = max(length(d), 1.0); float2 dir = d / L;
    float tt = dot(p - A, dir) / L;
    float k0 = floor(clamp(tt, 0.0, 0.9999) * n);
    float best = 100000.0; u = zsat(tt);
    for (int j = -1; j < 2; j++)
    {
        float k = k0 + float(j);
        if (k < 0.0 || k > n - 1.0) continue;
        float2 a = lz_knot(A, B, k, n, amp, seed);
        float2 b = lz_knot(A, B, k + 1.0, n, amp, seed);
        float dd = zsegd(p, a, b);
        if (dd < best) { best = dd; u = (k + zsat(dot(p - a, b - a) / max(dot(b - a, b - a), 0.0001))) / n; }
    }
    return best;
}
// bolt colour from distance d to the stroke centre and a half width w (px): white core, cyan, blue, ink outline.
// hot 1 = fresh frame (white core), 0.5 = second frame (cyan core), 0 = residual (blue 1 px). Alpha 0 outside.
float4 lz_boltcol(sampler2D r, float d, float w, float hot, bool ink)
{
    w = w + 0.5;
    if (hot > 0.75)
    {
        if (d <= w) return float4(zramp(r, 0.0, 4.0), 1.0);
        if (d <= w + 1.0) return float4(zramp(r, 0.0, 3.0), 1.0);
        if (ink && d <= w + 2.0) return float4(zramp(r, 0.0, 0.0), 1.0);
    }
    else if (hot > 0.35)
    {
        if (d <= w) return float4(zramp(r, 0.0, 3.0), 1.0);
        if (d <= w + 1.0) return float4(zramp(r, 0.0, 2.0), 1.0);
        if (ink && d <= w + 2.0) return float4(zramp(r, 0.0, 0.0), 1.0);
    }
    else
    {
        if (d <= w) return float4(zramp(r, 0.0, 2.0), 1.0);
        if (ink && d <= w + 1.0) return float4(zramp(r, 0.0, 1.0), 1.0);
    }
    return float4(0.0, 0.0, 0.0, 0.0);
}
// small plus-shaped star (impact / node). len = spike length px, returns field 0..1 (1 at centre)
float lz_star(float2 q, float len, float th)
{
    float ax = abs(q.x); float ay = abs(q.y);
    float s1 = (ay <= th) ? 1.0 - ax / max(len, 0.01) : 0.0;
    float s2 = (ax <= th) ? 1.0 - ay / max(len, 0.01) : 0.0;
    float dg = abs(ax - ay) <= th * 0.75 ? 1.0 - (ax + ay) / max(len * 1.1, 0.01) : 0.0;
    return zsat(max(max(s1, s2), dg * 0.8));
}
// step seed: re-jitter a shape every 'every' animation steps
float lz_stepseed(float seed, float every)
{
    float st = floor(_FxTime * _StepFps / max(every, 1.0));
    return frac(seed + st * 0.1373);
}

#endif
