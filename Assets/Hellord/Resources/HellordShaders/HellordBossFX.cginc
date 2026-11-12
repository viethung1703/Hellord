// t950 Hellord boss FX shared helpers (include from PixelBossWarn / PixelBossOrb / PixelBossGround / PixelBossSackWrap).
// NOT COMPILED, NOT TESTED IN UNITY. 1:1 with the numpy mock-ups (S.tex2D, B2.bayer, quant, Python round = round-half-even).
// Pixel space: world px = world u * 16. Integer image px (y down, = np.mgrid) P = (floor(wx), -floor(wy)-1); pixel centre = P + 0.5 (= B2.grid).
// World inputs (_A, _B, _Ctr in u; _Deg in world degrees, CCW) are converted here: img = (x, -y) * PPU, degImg = -deg.
#ifndef HELLORD_BOSS_FX
#define HELLORD_BOSS_FX
sampler2D _NoiseTex, _RampTex;
float _PPU, _Fps, _FrameOfs, _RampRows;
// S.tex2D(u, v) = NOISE[row = v*128-.5 from the TOP]. Unity v runs from the bottom, so v -> 1 - v is the exact equivalent
// (bilinear weights mirror too). Assumes fx928_noise128.png row 0 = NOISE[0]; drop the 1 - if the live PNG was saved bottom-up.
float N1(float u, float v) { return tex2Dlod(_NoiseTex, float4(u, 1.0 - v, 0, 0)).r; }  // Bilinear + Repeat, no mips
float fmodp(float a, float m) { return a - m * floor(a / m); }                            // Python % (always >= 0)
float Bay(float2 p)                                                                       // B2.bayer: BAYER[y%4, x%4] - 0.5
{
    int x = (int)fmodp(floor(p.x), 4), y = (int)fmodp(floor(p.y), 4);
    float4 r = y == 0 ? float4(0,8,2,10) : y == 1 ? float4(12,4,14,6) : y == 2 ? float4(3,11,1,9) : float4(15,7,13,5);
    return (x == 0 ? r.x : x == 1 ? r.y : x == 2 ? r.z : r.w) / 16.0 - 0.5;
}
// fx950_boss_ramps.png is 8 x _RampRows (20). PNG row 0 is the TOP row, Unity v = 0 is the bottom, hence 1 - ...
half4 Ramp(float row, float i) { return tex2Dlod(_RampTex, float4((i + 0.5) / 8.0, 1.0 - (row + 0.5) / _RampRows, 0, 0)); }
int Quant(float v, int n) { return clamp((int)floor(v * n), 0, n - 1); }                  // quant(): ramp[clip(int(v*len))]
float2 ImgPx(float2 wpx) { return float2(floor(wpx.x), -floor(wpx.y) - 1); }
float2 ImgPt(float2 worldU) { return float2(worldU.x, -worldU.y) * _PPU; }
float2 ImgDir(float2 worldV) { return float2(worldV.x, -worldV.y); }
float Frame() { return floor(_Time.y * _Fps) + _FrameOfs; }                              // mock-up f (30 fps, unstepped)
float AngDiff(float a, float b) { return abs(fmodp(a - b + 180.0, 360.0) - 180.0); }      // |(a - b + 180) % 360 - 180|
// B2.grid(cx, cy, rx, ry) footprint (the mock-up only writes inside it)
bool InGrid(float2 P, float2 c, float rx, float ry)
{ return P.y >= floor(c.y - ry) - 2 && P.y < floor(c.y + ry) + 3 && P.x >= floor(c.x - rx) - 2 && P.x < floor(c.x + rx) + 3; }
// b1._glow (replaces AO.star, VH: no 4-point stars): soft dithered round glow, own 4x4 matrix ((x&3)*4+(y&3))*7%16
int Glow(float2 P, float2 c, float r) // c = int centre; 0 none, 1 arm, 2 core
{
    r = max(1, floor(r * 0.6)); float q = length(P - c) / (r + 0.5);
    float b = fmodp((fmodp(P.x, 4) * 4 + fmodp(P.y, 4)) * 7, 16) / 16.0;
    return (q < 0.55 && 0.55 - q > b * 0.35) ? 2 : (q < 1 && 1 - q > b * 0.8) ? 1 : 0;
}
#endif
