// t950 Hellord PixelBossWarn: boss warning grain ("warning C"), 1:1 port of opt.warn 'C' (Ox v1), gor.warnI (ice v2),
// b1.warnN / gor.warnI2 / qil.warnF / b4.wH (v3 depth, 4 ramps) and b4.wW (wind). One quad per tell (HellordTells46 TI46 bbox).
// NOT COMPILED, NOT TESTED IN UNITY. Opaque palette + clip, Blend Off (same conventions as PixelAoE / PixelFireWall).
// Integer image grid like np.mgrid (no +0.5). No ring, no outline, no star.
Shader "Hellord/PixelBossWarn"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite (unused, quad only)", 2D) = "white" {}
        _NoiseTex ("fx928_noise128 (Bilinear, Repeat)", 2D) = "gray" {}
        _RampTex ("fx950_boss_ramps 8x20 (Point, Clamp)", 2D) = "white" {}
        _RampRows ("Ramp rows", Float) = 20
        [Enum(C1,0,IceV2,1,DepthV3,2,Wind,3)] _Mode ("Mode", Float) = 2
        _Row ("Ramp row (C1 0, IceV2 1, Ox 2, Ice 3, Fire 4, Horse 5, Wind 6)", Float) = 2
        _RampN ("Ramp length (4 or 6)", Float) = 6
        [Enum(Ellipse,0,Lane,1,Wedge,2,Quake,3,Band,4)] _Shape ("Shape", Float) = 0
        _A ("Centre / lane start / apex, world u", Vector) = (0,0,0,0)
        _B ("Lane end, world u", Vector) = (0,0,0,0)
        _R ("Radius / reach / ring radius px", Float) = 12.8
        _Sq ("Ellipse / band y squash", Float) = 1
        _HW ("Lane / band half width px", Float) = 28
        _Deg ("Wedge aim / quake + arc gap centre, world deg", Float) = 0
        _Half ("Wedge half / quake gap half / arc gap half, deg", Float) = 30
        _Ym ("Wedge ground y mul", Float) = 1
        _Gap ("Band: 1 = has gap", Float) = 0
        _P ("Tell progress 0..1 (code)", Float) = 0
        _Seed ("Seed", Float) = 0
        _Depth ("Interior depth px (v3 / wind)", Float) = 10
        _WindAng ("Wind: 1 = linear flow along _WindDeg, 0 = spiral about _Ctr", Float) = 1
        _WindDeg ("Wind flow world deg", Float) = 0
        _Ctr ("Wind spiral centre, world u", Vector) = (0,0,0,0)
        _Fps ("Frame rate of f", Float) = 30
        _FrameOfs ("Frame offset", Float) = 0
        _PPU ("PPU", Float) = 16
        _Dim ("t975u dim: 1 = darker, sparser, no white/yellow-hot top (code: bossWarnDim985)", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="TransparentCutout" "IgnoreProjector"="True" "PreviewType"="Plane" }
        Cull Off ZWrite Off ZTest Always Blend Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            #include "HellordBossFX.cginc"
            float4 _A, _B, _Ctr;
            float _Mode, _Row, _RampN, _Shape, _R, _Sq, _HW, _Deg, _Half, _Ym, _Gap, _P, _Seed, _Depth, _WindAng, _WindDeg, _Dim;
            struct appdata { float4 vertex : POSITION; };
            struct v2f { float4 pos : SV_POSITION; float2 wpx : TEXCOORD0; };
            v2f vert (appdata v) { v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.wpx = mul(unity_ObjectToWorld, v.vertex).xy * _PPU; return o; }
            float Sd(float2 P)
            {
                float2 c = ImgPt(_A.xy), d = P - c; float deg = -_Deg;
                if (_Shape < 0.5) return _R - length(float2(d.x, d.y / _Sq));                                     // sd_circ / sd_ell
                if (_Shape < 1.5)                                                                                  // sd_lane
                {
                    float2 ab = ImgPt(_B.xy) - c; float L = length(ab); float2 u = ab / L;
                    float s = dot(d, u), n = -d.x * u.y + d.y * u.x;
                    return min(min(_HW - abs(n), s), L - s);
                }
                if (_Shape < 2.5) { d.y /= _Ym; float q = length(d); return min(_R - q, radians(_Half - AngDiff(degrees(atan2(d.y, d.x)), deg)) * q); }   // sd_wedge
                if (_Shape < 3.5) { float q = length(d); return min(_R - q, radians(AngDiff(degrees(atan2(d.y, d.x)), deg) - _Half) * q); }              // opt.sd_quake
                float q = length(float2(d.x, d.y / _Sq)), s = _HW - abs(q - _R);                                   // stomp band / lasso band / sd_arc
                if (_Gap > 0.5) { float qa = length(d); s = min(s, radians(AngDiff(degrees(atan2(d.y, d.x)), deg) - _Half) * qa); }
                return s;
            }
            half4 frag (v2f i) : SV_Target
            {
                float2 P = ImgPx(i.wpx);
                float sd = Sd(P); if (sd <= 0) discard;                   // inside = sd > 0
                float f = Frame(), p = _P, b = Bay(P);
                float n = N1(P.x / 4.0 + _Seed, P.y / 4.0);
                int idx = -1; int N = (int)_RampN;
                if (_Mode < 1.5)                                          // 0 = opt.warn 'C', 1 = gor.warnI
                {
                    float w = exp(-max(sd, 0) / (1.5 + 3 * p));
                    float v = (n - 0.5) * 0.5 + w * (0.4 + 0.6 * p) + 0.05;
                    if (_Mode > 0.5) v += max(N1(P.x / 2.5 + _Seed * 1.7 + f * 0.05, P.y / 2.5) - 0.45, 0) * (0.5 + 0.7 * p);
                    if (v > b + 0.55) idx = Quant(saturate((v - 0.2) * (1.5 + 10 * p) / 4), N);
                    if (p > 0.8 && sd < 1.2 && fmodp(P.x + P.y + f, 3) == 0 && (_Mode < 0.5 || n > 0.55)) idx = 4;   // white edge sparks (row slot 4 = white)
                }
                else if (_Mode < 2.5)                                     // 2 = warning C v3 (warnN / warnI2 / warnF / wH)
                {
                    float n2 = N1(P.x / 2.5 + _Seed * 1.7 + f * 0.05, P.y / 2.5), u = saturate(sd / _Depth);
                    float w = exp(-max(sd, 0) / (1.5 + 3 * p)) * (n > 0.5 ? 1 : 0);
                    float v = (n - 0.5) * 0.5 + 0.1 + w * 0.35 + max(n2 - 0.4, 0) * (0.6 + 0.8 * p) + u * (0.15 + 0.35 * p);
                    if (v > b + 0.55 + _Dim * (sd > 1.5 ? 0.22 : 0)) idx = Quant(saturate((u * 0.55 + n2 * 0.45) * (0.35 + 0.75 * p) + 0.1 * w), N);
                    // t975u: interior stays in the two darkest slots, edge tops out at slot N-4 (Ox e43b44 / ice 0099db), N-3 only on sparse noise; never N-2/N-1 (yellow / white)
                    if (_Dim > 0.5 && idx >= 0) idx = sd > 1.5 ? min(max(idx - 2, 0), N - 5) : min(idx, n > 0.72 ? N - 3 : N - 4);
                }
                else                                                      // 3 = b4.wW wind grain
                {
                    float w = (N1(P.x / 9 + f * 0.05 + _Seed, P.y / 9 - f * 0.04) - 0.5) * 1.6, ph;
                    if (_WindAng > 0.5) { float a = radians(-_WindDeg), ca = cos(a), sa = sin(a); ph = ((P.x * ca + P.y * sa) / 22 - f * 0.07) + (-P.x * sa + P.y * ca) / 9 * 0.25 + w * 0.35; }
                    else { float2 d = P - ImgPt(_Ctr.xy); ph = atan2(d.y, d.x) * 3 / 6.283 + length(d) / 14 - f * 0.09 + w * 0.35; }
                    float band = 1 - abs(frac(ph) - 0.5) * 2, brk = N1(P.x / 7 + _Seed * 1.3 - f * 0.1, P.y / 7);
                    float u = saturate(sd / _Depth); bool edge = N1(P.x / 3 + _Seed, P.y / 3) > 0.45 || sd > 2;
                    float v = band * band * band * (0.35 + brk) * (0.45 + 0.55 * u) * (0.55 + 0.6 * p) + b * 0.12;
                    float th = 0.28 + 0.06 * _Dim;
                    if (edge && v > th) idx = Quant(saturate((v - th) * 1.5 * (0.6 + 0.5 * p)), N);
                    if (_Dim > 0.5 && idx >= 0) idx = min(idx, sd > 2 ? N - 3 : N - 2);   // t975u: wind never hits white (row 6 slot 3)
                }
                clip(idx);
                return Ramp(_Row, idx);
            }
            ENDCG
        }
    }
}
