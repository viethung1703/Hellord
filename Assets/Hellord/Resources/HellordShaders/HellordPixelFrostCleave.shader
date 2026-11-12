// Hellord PixelFrostCleave: 1:1 port of the VH-LOCKED sample_v7 Frost Cleave (t901u),
// /workspace/fx_audit/sample_v7/sample_v7_mockup.py v7 smear() (line 927) + frost_cleave() wind-up blade (line 940).
// _Mode 0 = wind-up blade (ink 7 | blue 5 | cyan 3 | cyan/white 1 px stacked lines), _Mode 1 = tapered smear crescent.
// Inverse-mapped splat: each fragment searches the +-4 angle samples / +-4 radial steps around it and keeps the LAST
// mock-up write that rounds onto it (same draw order as the Python put()), so the result is pixel-identical to the mock.
// Opaque Endesga palette + clip(), no blending, no noise, no dither (the locked v7 look has none). Pipeline-agnostic CGPROGRAM.
// NOT COMPILED, NOT TESTED IN UNITY.
// Pixel space: world px at 16 PPU, snapped floor()+0.5, y flipped to the mock's y-down image space (angles are image-space radians).
Shader "Hellord/PixelFrostCleave"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite (unused, quad only)", 2D) = "white" {}
        _RampTex ("Ramp 6x1 (Point, Clamp): ink,navy,blue,cyan,white,pale", 2D) = "white" {}
        _RampCount ("Ramp colours", Float) = 6
        _GapTex ("Gaps 1024x2 (Point, Clamp, R): row0 seed3 .35, row1 seed4 .6", 2D) = "black" {}
        _GapRow ("Gap row (-1 = solid)", Float) = -1
        _Mode ("0 blade, 1 smear", Float) = 1
        _Origin ("Swing origin world xy (u) = hero pos + (0, 14 px)", Vector) = (0,0,0,0)
        _ROut ("Smear outer radius px", Float) = 64
        _ATail ("Smear tail angle (image rad)", Float) = -2.3
        _ALead ("Smear lead angle (image rad)", Float) = 1.75
        _Thick ("Smear max thickness px", Float) = 26
        _Sq ("Ellipse squash", Float) = 0.72
        _P0 ("Blade start image px (xy)", Vector) = (0,0,0,0)
        _P1 ("Blade tip image px (xy)", Vector) = (0,0,0,0)
        _TipWhite ("Blade 1 px core white (last wind-up frame)", Float) = 0
        _PPU ("PPU", Float) = 16
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
            sampler2D _RampTex, _GapTex;
            float4 _Origin, _P0, _P1;
            float _RampCount, _GapRow, _Mode, _ROut, _ATail, _ALead, _Thick, _Sq, _TipWhite, _PPU;
            struct appdata { float4 vertex : POSITION; };
            struct v2f { float4 pos : SV_POSITION; float2 wpx : TEXCOORD0; };
            v2f vert (appdata v) { v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.wpx = mul(unity_ObjectToWorld, v.vertex).xy * _PPU; return o; }
            half4 Ramp(int i) { return tex2Dlod(_RampTex, float4((i + 0.5) / _RampCount, 0.5, 0, 0)); }
            bool Gap(float k) { return _GapRow >= 0 && tex2Dlod(_GapTex, float4((k + 0.5) / 1024.0, 1.0 - (_GapRow + 0.5) / 2.0, 0, 0)).r > 0.5; } // PNG row 0 = top

            int Smear(float2 P)                                   // smear(): -1 = empty, else ramp index
            {
                float2 o = float2(_Origin.x, -_Origin.y) * _PPU;
                float span = _ALead - _ATail; float n = floor(abs(span) * _ROut * 1.6) + 1;
                float dx = P.x - o.x, dy = (P.y - o.y) / _Sq, q = sqrt(dx * dx + dy * dy), a = atan2(dy, dx);
                const float TAU = 6.2831853;
                float rel = a - _ATail;
                rel = span > 0 ? rel - TAU * floor(rel / TAU) : -((-rel) - TAU * floor((-rel) / TAU));
                float k0 = round(rel / span * n), j0 = round((_ROut - q) / 0.5);
                int col = -1;
                [loop] for (int dk = -4; dk <= 4; dk++)
                {
                    float k = k0 + dk; if (k < 0 || k > n || Gap(k)) continue;
                    float u = k / n, ak = _ATail + span * u;
                    float th = max(1.5, _Thick * pow(u, 1.3)); th *= 1 - max(0, (u - 0.93) / 0.07) * 0.6;
                    float jn = ceil((th + 2) / 0.5); float ca = cos(ak), sa = sin(ak);
                    [loop] for (int dj = -4; dj <= 4; dj++)
                    {
                        float j = j0 + dj; if (j < 0 || j >= jn) continue;
                        float dr = j * 0.5, r = _ROut - dr;
                        if (round(o.x + ca * r) != P.x || round(o.y + sa * r * _Sq) != P.y) continue;
                        float qq = dr / (th + 1);
                        col = (dr < 0.8 || dr > th + 0.5) ? 0 : qq < 0.3 ? 4 : qq < 0.55 ? 3 : qq < 0.8 ? 2 : 1;   // ink|white|cyan|blue|navy
                    }
                }
                return col;
            }
            int Blade(float2 P)                                   // 4 x c.line(): ink w7, blue w5, cyan w3, cyan/white w1
            {
                float2 d = _P1.xy - _P0.xy; float n = floor(max(abs(d.x), abs(d.y))) + 1; int col = -1;
                [loop] for (int i = 0; i <= 96; i++)
                {
                    if (i > n) break;
                    float2 p = _P0.xy + d * (i / n); if (round(p.y) != P.y) continue;
                    float e = P.x - round(p.x);
                    int c = (e == 0) ? (_TipWhite > 0.5 ? 4 : 3) : (e >= -1 && e <= 1) ? 3 : (e >= -2 && e <= 2) ? 2 : (e >= -3 && e <= 3) ? 0 : -1;
                    col = max(col, c);                           // draw order ink<blue<cyan<core == ramp index order 0<2<3<(3|4)
                }
                return col;
            }
            half4 frag (v2f i) : SV_Target
            {
                float2 w = floor(i.wpx);
                float2 P = float2(w.x, -w.y - 1);                // integer image px (y down), same as the boss wire ImgPx
                int c = _Mode < 0.5 ? Blade(P) : Smear(P);
                clip(c + 0.5);
                return Ramp(c);
            }
            ENDCG
        }
    }
}
