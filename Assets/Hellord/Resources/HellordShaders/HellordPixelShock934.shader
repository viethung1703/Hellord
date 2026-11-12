// t934u Hellord PixelShock934 (batch6 mode 20 GROUND SHOCK). Displacement only: re-samples what is already drawn (floor + ground decals)
// outward in a travelling, pixel-snapped sine band. Adds NO colour (no drawn ring). The game runs the Built-in pipeline, so the URP
// Opaque Texture of the draft is replaced by a named GrabPass (one grab per frame, shared by all shocks; works on WebGL).
// t1002u: BROKEN under URP/SRP — GrabPass cannot run on job/SRP threads. Shock934 is gated off when currentRenderPipeline != null until a URP-safe rewrite.
// Draw it between the floor/decals and the units (sorting order fxShockOrder934, units are about -1300..0).
Shader "Hellord/PixelShock934"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite (unused)", 2D) = "white" {}
        _Center ("Centre world xy", Vector) = (0,0,0,0)
        _RadiusPx ("Radius px", Float) = 40
        _R0 ("Start radius fraction", Float) = 0
        _Squash ("Squash", Float) = 0.55
        _Age ("Age frames", Float) = 0
        _LifeF ("Life frames", Float) = 9
        _Amp ("Amp px", Float) = 3.5
        _Width ("Band px", Float) = 6
        _Inward ("1 = runs inward", Float) = 0
        _PPU ("PPU", Float) = 16
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "PreviewType"="Plane" }
        Cull Off ZWrite Off ZTest Always Blend Off
        GrabPass { "_HellordShockGrab934" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            sampler2D _HellordShockGrab934; float4 _HellordShockGrab934_TexelSize;
            float4 _Center; float _RadiusPx, _R0, _Squash, _Age, _LifeF, _Amp, _Width, _Inward, _PPU;
            struct appdata { float4 vertex : POSITION; };
            struct v2f { float4 pos : SV_POSITION; float2 wpx : TEXCOORD0; float4 gp : TEXCOORD1; };
            v2f vert (appdata v) { v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.wpx = mul(unity_ObjectToWorld, v.vertex).xy * _PPU; o.gp = ComputeGrabScreenPos(o.pos); return o; }
            fixed4 frag (v2f i) : SV_Target
            {
                float2 p = floor(i.wpx) + 0.5; float2 d = p - _Center.xy * _PPU; float2 ds = float2(d.x, d.y / max(0.05, _Squash));
                float r = length(ds) + 1e-4;
                float k = min(1.15, (_Age + 1) / (_LifeF * 0.6)); if (_Inward > 0.5) k = 1.15 - k;
                float front = _RadiusPx * (_R0 + (1 - _R0) * k); float amp = _Amp * (1 - _Age / _LifeF);
                float s = (r - front) / _Width;
                if (abs(s) >= 1 || amp <= 0) discard;
                float prof = -sin((s + 1) * 3.14159) * amp;
                float2 off = round(float2(ds.x / r * prof, ds.y / r * prof * _Squash));   // world px, pixel-snapped
                // world px -> screen px: one world px covers (_ScreenParams.y / (2*ortho*PPU)) screen px
                float spp = _ScreenParams.y / (2.0 * unity_OrthoParams.y * _PPU);
                float2 uv = i.gp.xy / i.gp.w - off * spp * _HellordShockGrab934_TexelSize.xy;
                return tex2D(_HellordShockGrab934, uv);
            }
            ENDCG
        }
    }
}
