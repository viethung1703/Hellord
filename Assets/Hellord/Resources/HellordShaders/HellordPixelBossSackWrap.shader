// t950 Hellord PixelBossSackWrap: Horse-Face rice sack v2 wind wrap (proj2.sackv). Two quads on the sack transform (~100 x 80 px):
// _Front 0 = behind the sack (3 shear streaks + back half of the 3 wrap strokes), _Front 1 = in front (front half).
// The sack sprite itself gets the slate tint from PixelSpriteFX mode 3 (SPRITEFX_MODE3.md). Draw order: back quad, sack, front quad.
// NOT COMPILED, NOT TESTED IN UNITY. Opaque palette + clip, Blend Off. Row 13 = SLATE 0..3, CUT 4..6 (INK = SLATE[0]).
Shader "Hellord/PixelBossSackWrap"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite (unused, quad only)", 2D) = "white" {}
        _RampTex ("fx950_boss_ramps 8x20 (Point, Clamp)", 2D) = "white" {}
        _RampRows ("Ramp rows", Float) = 20
        _Front ("0 back pass (streaks + back strokes), 1 front pass", Float) = 0
        _Ctr ("Sack position (sprite pivot), world u", Vector) = (0,0,0,0)
        _Vel ("Sack velocity, world (streak dir)", Vector) = (1,0,0,0)
        _Fps ("Frame rate of f", Float) = 30
        _FrameOfs ("Frame offset", Float) = 0
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
            #include "HellordBossFX.cginc"
            float4 _Ctr, _Vel; float _Front;
            struct appdata { float4 vertex : POSITION; };
            struct v2f { float4 pos : SV_POSITION; float2 wpx : TEXCOORD0; };
            v2f vert (appdata v) { v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.wpx = mul(unity_ObjectToWorld, v.vertex).xy * _PPU; return o; }
            half4 frag (v2f i) : SV_Target
            {
                float2 P = ImgPx(i.wpx), X = ImgPt(_Ctr.xy); float f = Frame(); int idx = -1;
                bool front = _Front > 0.5;
                if (!front)   // shear streaks (drawn first)
                {
                    float2 u = ImgDir(_Vel.xy); u /= max(length(u), 1e-6);
                    [loop] for (int k = 0; k < 3; k++)
                    {
                        float off = (k - 1) * 7.0; int L = 16 - 4 * abs(k - 1);
                        [loop] for (int j = 0; j < 16; j++)
                        {
                            if (j >= L || fmodp(j + f, 6) == 5) continue;
                            float2 p = float2(X.x - u.x * (20 + j) - u.y * off, X.y - u.y * (20 + j) + u.x * off);
                            if (all(round(p) == P)) idx = j < 5 ? 5 : j < 11 ? 4 : 3;
                        }
                    }
                }
                [loop] for (int k = 0; k < 3; k++)   // wrap strokes: 64 points per arm, ink at o = -3, 2, 3, colour at o = -2..1
                {
                    float a0 = -f * 0.45 + k * 2.094;
                    [loop] for (int j = 0; j < 64; j++)
                    {
                        float t = j / 63.0, a = a0 + t * 1.8, rr = 25 + t * 9;
                        if ((sin(a) > 0) != front) continue;
                        float px = X.x + cos(a) * rr, py = X.y + sin(a) * rr * 0.72;
                        if (round(px) != P.x) continue;
                        int pc = t > 0.7 ? 6 : t > 0.3 ? 5 : 4;
                        if (round(py - 3) == P.y || round(py + 2) == P.y || round(py + 3) == P.y) idx = 0;
                        if (round(py - 2) == P.y) idx = 4;
                        if (round(py - 1) == P.y || round(py) == P.y || round(py + 1) == P.y) idx = pc;
                    }
                }
                clip(idx);
                return Ramp(13, idx);
            }
            ENDCG
        }
    }
}
