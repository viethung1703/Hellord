// Gonggong combat FX (t1682, water / ice kit) shared shader maths. Same pipeline as Leizu v2.1 (copied from LeizuFX_Core under new names, no
// collisions): adapted from the approved Zhurong fx_v2 core (t1622). MOCKUP-verified against Unity built-in renders, see spec.md.
// Written in a portable HLSL subset: the SAME file is compiled by Unity (HLSL) and by the preview renderer (GLSL via a #define prelude),
// so the MOCKUP shows exactly this maths. Rules for editors: float literals with a decimal point, no scalar->vector broadcasts,
// no fmod/mul/matrices/arrays, scalar ternaries only.
// Space: p = art pixel centre relative to the effect pivot (16 PPU, y up), already snapped to the world pixel grid.
#ifndef GONGGONG_FX_CORE
#define GONGGONG_FX_CORE

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
// noise texture: R fbm, G fbm2, B voronoi cell id, A voronoi edge (0 = on a crack). Gonggong t1682: zN(p / s) tiles every 4 s px
// (x0.25), so 's' reads as the feature size in px (voronoi cells ~0.4 s, fbm base ~2 s). Every Gonggong noise lookup goes through zN.
float4 zN(sampler2D t, float2 uv) { return ZTEX(t, uv * 0.25); }
// ramp rows (GGF_Ramp16x8): 0 fire, 1 smoke, 2 gold, 3 rock, 4 curse, 5 ash, 6 bloom
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
bool zclump(sampler2D n, float dens, float2 p, float seed) { return dens > zN(n, p / 9.0 + float2(seed, seed * 0.7)).g * 0.8 + 0.1; }
// rising flow noise: domain-warped fbm scrolling up (pixel space in, 0..1 out)
float zflow(sampler2D n, float2 p, float scale, float t, float speed, float seed)
{
    float2 uv = p / scale;
    float w = zN(n, uv * 0.5 + float2(seed, t * 0.11)).g - 0.5;
    return zN(n, uv + float2(w * 0.35 + seed * 0.37, -t * speed / scale + w * 0.2)).r;
}
float zsegd(float2 q, float2 a, float2 b) { float2 ab = b - a; float h = zsat(dot(q - a, ab) / max(1e-4, dot(ab, ab))); return length(q - a - ab * h); }
float zeaseOut(float x) { x = zsat(x); return 1.0 - (1.0 - x) * (1.0 - x); }
float zeaseIn(float x) { x = zsat(x); return x * x; }
// glow output helper: banded additive glow from a smooth field (Leizu v2 bands, water / ice colours).
// outer = blue (violet when tint 1) x 0.30, middle = cyan x 0.42, inner = pale c0cbdc x 0.62, all x gain.
float4 zglowT(sampler2D r, float field, float2 p, float gain, float tint)
{
    float b = zband(zsat(field), 4.0, p, 0.45);
    float3 c = zramp(r, 0.0, 2.0); float k = b > 0.5 ? 0.30 : 0.0;
    if (b < 1.5 && tint > 0.5) c = zramp(r, 5.0, 2.0);
    if (b > 1.5) { c = zramp(r, 0.0, 3.0); k = 0.42; }
    if (b > 2.5) { c = zramp(r, 0.0, 4.0); k = 0.62; }
    return float4(c * k * gain, 1.0);
}
float4 zglow(sampler2D r, float field, float2 p, float gain) { return zglowT(r, field, p, gain, 0.0); }

// ---------------- Gonggong water / ice helpers (t1682) ----------------
// palette shortcuts (rows of GGF_Ramp16x8)
float4 gg_ice(sampler2D r, float idx) { return float4(zramp(r, 0.0, idx), 1.0); }     // 0 ink 1 deep 2 blue 3 cyan 4 pale 5 white
float4 gg_wat(sampler2D r, float idx) { return float4(zramp(r, 1.0, idx), 1.0); }     // 0 ink 1 teal 2 deep 3 blue 4 cyan 5 foam 6 white
float4 gg_frost(sampler2D r, float idx) { return float4(zramp(r, 3.0, idx), 1.0); }   // 0 262b44 1 3a4466 2 5a6988 3 8b9bb4 4 pale 5 white
float4 gg_none() { return float4(0.0, 0.0, 0.0, 0.0); }
// white amount cap (Leizu v2.1 lesson): _Hot style. A palette index of 'white' (top) is demoted to pale / cyan when hot is low.
float gg_cap(float idx, float top, float hot) { return (idx >= top - 0.5 && hot < 0.5) ? top - 1.0 : idx; }
// step seed: re-roll a shape every 'every' animation steps
float gg_stepseed(float seed, float every)
{
    float st = floor(_FxTime * _StepFps / max(every, 1.0));
    return frac(seed + st * 0.1373);
}
float gg_t() { return floor(_FxTime * _StepFps) / _StepFps; }   // stepped time (15 fps)
// faceted ice crystal (2.5D prism seen from the game's 3/4 view): q = p - base (feet of the crystal), h = height px, w = half width px,
// lean = top x offset px. Returns facet: -1 outside, 0 ink outline, 1 shadow facet, 2 mid facet, 3 lit facet, 4 white edge highlight.
float gg_crystal(float2 q, float h, float w, float lean)
{
    float res = -1.0;
    if (h >= 1.0 && w >= 0.5)
    {
        float v = q.y / h;                                     // 0 base .. 1 tip
        float cx = lean * zsat(v);
        float tipK = zsat((v - 0.62) / 0.38);                  // last 38 %: taper to a point
        float hw = w * (1.0 - tipK) + 0.35 * (1.0 - tipK);
        float x = q.x - cx;
        float baseCut = -1.0 - 0.35 * w;                       // slightly rounded base sunk in the ground
        if (q.y >= baseCut && v <= 1.0 && abs(x) <= hw + 1.0)
        {
            if (abs(x) > hw - 0.0 || v > 0.98 || q.y < baseCut + 1.0) res = 0.0;
            else
            {
                float ridge = cx * 0.0 - w * 0.25;                 // facet ridge left of centre (light from the upper left)
                res = x < ridge ? 3.0 : 1.0;
                if (x >= ridge && x < ridge + max(1.0, w * 0.45)) res = 2.0;
                if (abs(x - ridge) < 0.75 && v > 0.15) res = 4.0;   // ridge highlight
                if (x < ridge && v > 0.55 && x < ridge - w * 0.45) res = 2.0;
            }
        }
    }
    return res;
}
// facet -> ice colour index (row 0). hot 0..1 caps white.
float gg_facetidx(float f, float hot)
{
    float i = f < 0.5 ? 0.0 : (f < 1.5 ? 2.0 : (f < 2.5 ? 3.0 : (f < 3.5 ? 4.0 : 5.0)));
    return (i > 4.5 && hot < 0.5) ? 4.0 : i;
}
// small ice chip (debris): 3x2..3x3 px diamond with a 1 px white glint. q = p - chip centre. s 1..2. Returns facet like gg_crystal.
float gg_chip(float2 q, float s)
{
    float2 a = abs(floor(q + float2(0.5, 0.5)));
    float d = a.x / (s + 0.6) + a.y / (s * 0.75 + 0.5);
    float res = -1.0;
    if (d <= 1.0) res = (q.x < 0.0 && q.y > -0.5) ? 4.0 : (q.y > -0.5 ? 3.0 : 2.0);
    else if (d <= 1.55) res = 0.0;
    return res;
}
// round droplet (spray): r px. Returns 0 ink rim, 1 body, 2 highlight, -1 outside.
float gg_drop(float2 q, float r)
{
    float d = length(q);
    float res = -1.0;
    if (d <= r) res = (q.x < -r * 0.2 && q.y > r * 0.2) ? 2.0 : 1.0;
    else if (d <= r + 1.0) res = 0.0;
    return res;
}
// ballistic debris position: launched from the origin along angle a with speed v (px/s), gravity g px/s^2 (2.5D: y up), time t.
float2 gg_ballistic(float a, float v, float g, float t, float squash)
{
    return float2(cos(a) * v * t, sin(a) * v * t * squash + v * 0.55 * t - 0.5 * g * t * t);
}
// caustic field: bright where two drifting voronoi edge layers both approach 0 (light focused by the water surface). 0..1
float gg_caustic(sampler2D n, float2 p, float t, float scale)
{
    float e1 = zN(n, p / scale + float2(t * 0.031, t * 0.017)).a;
    float e2 = zN(n, p / (scale * 0.77) + float2(-t * 0.023, t * 0.029) + float2(0.37, 0.61)).a;
    return zsat(1.0 - (e1 + e2) * 1.6);
}
// foam field along a front: noise-broken, 0..1. d = signed distance behind the front (px, >0 behind), w = foam depth px.
float gg_foam(sampler2D n, float2 p, float d, float w, float t)
{
    float nz = zN(n, p / 7.0 + float2(t * 0.21, -t * 0.13)).r;
    float cell = zN(n, p / 13.0 + float2(-t * 0.05, t * 0.09)).a;
    float f = 1.0 - abs(d - w * 0.35) / max(w, 1.0);
    return zsat(f * 1.4 + (nz - 0.5) * 0.9 - (cell < 0.12 ? 0.0 : 0.15));
}
// creeping frost coverage (dendritic, built from the voronoi crack edges): grows as 'grow' goes 0..1 from where 'seedv' is low.
// seedv: 0..1 (0 = frosts first, e.g. normalised distance from the frost source). Returns 0 none, 1 frost, 2 frost vein (bright).
float gg_frostcover(sampler2D n, float2 p, float seedv, float grow, float seed)
{
    float4 nz = zN(n, p / 21.0 + float2(seed * 3.7, seed * 1.9));
    float edge = nz.a; float cellv = nz.b;
    float g = grow * 1.25 - seedv - (nz.r - 0.5) * 0.35;
    float res = 0.0;
    if (g > 0.0 && edge < min(0.05 + g * 0.5, 0.15)) res = 2.0;     // veins first (along cell edges), stay thin
    else if (g > 0.18 + cellv * 0.25 && cellv < 0.86) res = 1.0;      // then the cells fill (frosted plates); ~1 in 7 cells stays bare
    return res;
}
#endif
