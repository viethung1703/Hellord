// v0.0.22 hell atmosphere grade. Pipeline-agnostic (plain UnityCG + SRPDefaultUnlit tag, same as HellordLight).
// Drawn as a camera-sized quad between the room props (-31000) and the room lights (-30000), i.e. UNDER every gameplay sprite,
// so the hero, enemies, bullets, warnings and pickups are never graded.
// _Mode 0 = 2x multiply (Blend DstColor SrcColor): dst * _Tint     _Mode 1 = additive lift (Blend One One): dst + _Tint
Shader "Hellord/Atmos"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
        _Tint ("Tint (raw, set from code)", Vector) = (1,1,1,1)
        _Mode ("Mode", Float) = 0
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src", Float) = 2
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst", Float) = 3
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "PreviewType"="Plane" }
        Cull Off ZWrite Off ZTest Always
        Blend [_SrcBlend] [_DstBlend]
        Pass
        {
            // v0.0.22: no LightMode tag. URP draws untagged passes as SRPDefaultUnlit anyway; the tag made the box (Built-in + URP package) build strip the pass.
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #include "UnityCG.cginc"
            float4 _Tint; float _Mode;
            struct appdata { float4 vertex : POSITION; };
            struct v2f { float4 pos : SV_POSITION; };
            v2f vert (appdata v) { v2f o; o.pos = UnityObjectToClipPos(v.vertex); return o; }
            fixed4 frag (v2f i) : SV_Target
            {
                if (_Mode < 0.5) return fixed4(saturate(_Tint.rgb * 0.5), 1);
                return fixed4(_Tint.rgb, 1);
            }
            ENDCG
        }
    }
}
