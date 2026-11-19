// t1567 Hellord HeatWave: optional screen distortion inside a ring / blob sprite (slam impact + eruption, chop shockwave).
// The sprite's alpha is the distortion mask; vertexColor.a fades it. Offset = (noise - 0.5) * _Strength screen px, scrolled by _Speed.
// URP (Unity 6): samples _CameraOpaqueTexture (turn Opaque Texture ON in the URP asset; with the 2D renderer use the Camera Sorting Layer Texture
// and set _UseSortingLayerTex = 1). Built-in pipeline: falls back to a GrabPass. Off by default (HellordTuning.pbHeatWave1567).
Shader "Hellord/HeatWave1567"
{
    Properties
    {
        [PerRendererData] _MainTex ("Mask (alpha)", 2D) = "white" {}
        _Strength ("Strength (screen px)", Float) = 2
        _Speed ("Scroll speed", Float) = 6
        _Freq ("Noise frequency", Float) = 0.35
        _UseSortingLayerTex ("URP 2D: use _CameraSortingLayerTexture", Float) = 0
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent+10" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Cull Off ZWrite Off ZTest Always Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            sampler2D _MainTex; sampler2D _CameraOpaqueTexture; sampler2D _CameraSortingLayerTexture; float4 _CameraOpaqueTexture_TexelSize;
            float _Strength, _Speed, _Freq, _UseSortingLayerTex;
            struct appdata { float4 vertex : POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; float4 col : COLOR; float2 uv : TEXCOORD0; float4 sp : TEXCOORD1; };
            v2f vert (appdata v) { v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.col = v.color; o.uv = v.uv; o.sp = ComputeScreenPos(o.pos); return o; }
            float N(float2 p) { return frac(sin(dot(floor(p), float2(12.9898, 78.233))) * 43758.5453); }
            float4 frag (v2f i) : SV_Target
            {
                float m = tex2D(_MainTex, i.uv).a * i.col.a; if (m <= 0.01) discard;
                float2 suv = i.sp.xy / i.sp.w; float2 px = suv * _ScreenParams.xy * _Freq + _Time.y * _Speed;
                float2 off = (float2(N(px), N(px + 17.3)) - 0.5) * _Strength * m / _ScreenParams.xy;
                float4 c = _UseSortingLayerTex > 0.5 ? tex2D(_CameraSortingLayerTexture, suv + off) : tex2D(_CameraOpaqueTexture, suv + off);
                return float4(c.rgb, 1);
            }
            ENDCG
        }
    }
    SubShader
    {
        Tags { "Queue"="Transparent+10" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Cull Off ZWrite Off ZTest Always
        GrabPass { "_PbGrab1567" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            sampler2D _MainTex; sampler2D _PbGrab1567; float _Strength, _Speed, _Freq;
            struct appdata { float4 vertex : POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; float4 col : COLOR; float2 uv : TEXCOORD0; float4 gp : TEXCOORD1; };
            v2f vert (appdata v) { v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.col = v.color; o.uv = v.uv; o.gp = ComputeGrabScreenPos(o.pos); return o; }
            float N(float2 p) { return frac(sin(dot(floor(p), float2(12.9898, 78.233))) * 43758.5453); }
            fixed4 frag (v2f i) : SV_Target
            {
                float m = tex2D(_MainTex, i.uv).a * i.col.a; if (m <= 0.01) discard;
                float2 guv = i.gp.xy / i.gp.w; float2 px = guv * _ScreenParams.xy * _Freq + _Time.y * _Speed;
                float2 off = (float2(N(px), N(px + 17.3)) - 0.5) * _Strength * m / _ScreenParams.xy;
                return fixed4(tex2D(_PbGrab1567, guv + off).rgb, 1);
            }
            ENDCG
        }
    }
}
