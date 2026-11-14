// t1419 Hellord NovaHaze (Ice Nova burst): short heat-haze ripple. Displacement only: re-samples what is already drawn (GrabPass, BUILT-IN pipeline) outward in a
// pixel-snapped sine band that sits just BEHIND the wave front. Adds no colour. Cheap: one quad of about 2R x 2R px, one grab per frame, one tex tap.
// Not usable under SRP (GrabPass): the caller disables it when GraphicsSettings.currentRenderPipeline != null.
Shader "Hellord/NovaHaze1419"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite (unused)", 2D) = "white" {}
        _Center ("Centre world xy", Vector) = (0,0,0,0)
        _FrontPx ("Front radius px", Float) = 20
        _Width ("Band px (behind the front)", Float) = 14
        _Amp ("Amp px", Float) = 2
        _Squash ("Squash", Float) = 0.6
        _PPU ("PPU", Float) = 16
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "PreviewType"="Plane" }
        Cull Off ZWrite Off ZTest Always Blend Off
        GrabPass { "_HellordNovaGrab1419" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            sampler2D _HellordNovaGrab1419; float4 _HellordNovaGrab1419_TexelSize;
            float4 _Center; float _FrontPx, _Width, _Amp, _Squash, _PPU;
            struct appdata { float4 vertex : POSITION; };
            struct v2f { float4 pos : SV_POSITION; float2 wpx : TEXCOORD0; float4 gp : TEXCOORD1; };
            v2f vert (appdata v) { v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.wpx = mul(unity_ObjectToWorld, v.vertex).xy * _PPU; o.gp = ComputeGrabScreenPos(o.pos); return o; }
            fixed4 frag (v2f i) : SV_Target
            {
                float2 p = floor(i.wpx) + 0.5; float2 d = p - _Center.xy * _PPU; float2 ds = float2(d.x, d.y / max(0.05, _Squash));
                float r = length(ds) + 1e-4;
                float u = (r - (_FrontPx - _Width)) / (_Width + 12.0);   // 0..1 across a window: _Width px behind the front .. 12 px ahead of it (the strips cover most of the inner part)
                if (u <= 0.0 || u >= 1.0 || _Amp <= 0.0) discard;
                float prof = sin(u * 6.2831853 * 2.0) * _Amp * sin(u * 3.1415927);
                float2 off = round(float2(ds.x / r * prof, ds.y / r * prof * _Squash));     // world px, pixel-snapped
                float spp = _ScreenParams.y / (2.0 * unity_OrthoParams.y * _PPU);            // world px -> screen px
                float2 uv = i.gp.xy / i.gp.w - off * spp * _HellordNovaGrab1419_TexelSize.xy;
                return tex2D(_HellordNovaGrab1419, uv);
            }
            ENDCG
        }
    }
}
