#ifndef PIXELFX_COMMON_INCLUDED
#define PIXELFX_COMMON_INCLUDED
// Shared helpers for Pixel FX Pack shaders. Pipeline-agnostic (no URP/Built-in includes).

float2 PFX_SnapToTexel(float2 uv, float2 cell)
{
    return (floor(uv / cell) + 0.5) * cell;
}

float PFX_Hash21(float2 p)
{
    p = frac(p * float2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return frac(p.x * p.y);
}

// LUT layout: width = colorsPerRow texels, height = rows. Row 0 (top) = source colors, row N = replacement set N.
float3 PFX_PaletteSwap(float3 col, sampler2D lut, float row, float rows, float colors, float tol)
{
    float best = 1e5; float bestI = -1;
    [loop] for (int i = 0; i < 16; i++)
    {
        if (i >= (int)colors) break;
        float3 src = tex2Dlod(lut, float4((i + 0.5) / colors, 1.0 - 0.5 / rows, 0, 0)).rgb;
        float d = distance(src, col);
        if (d < best) { best = d; bestI = i; }
    }
    if (bestI < 0 || best > tol) return col;
    return tex2Dlod(lut, float4((bestI + 0.5) / colors, 1.0 - (row + 0.5) / rows, 0, 0)).rgb;
}
// v1.3
float3 PFX_HueShift(float3 c, float shift)
{
    // YIQ rotation
    float3x3 toYIQ = float3x3(0.299, 0.587, 0.114, 0.596, -0.274, -0.322, 0.211, -0.523, 0.312);
    float3x3 toRGB = float3x3(1, 0.956, 0.621, 1, -0.272, -0.647, 1, -1.106, 1.703);
    float3 yiq = mul(toYIQ, c);
    float a = shift * 6.2831853, ca = cos(a), sa = sin(a);
    yiq.yz = float2(yiq.y * ca - yiq.z * sa, yiq.y * sa + yiq.z * ca);
    return saturate(mul(toRGB, yiq));
}
float PFX_Bayer4(float2 p)
{
    int i = (int)p.x + (int)p.y * 4;
    const float b[16] = { 0, 8, 2, 10, 12, 4, 14, 6, 3, 11, 1, 9, 15, 7, 13, 5 };
    return (b[i] + 0.5) / 16.0;
}
#endif
