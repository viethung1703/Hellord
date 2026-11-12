// v0.7 Ox Pit light overlays (handoff blend rules).
// _Mode 0 = additive: rgb += tex.rgb * tex.a * strength          (Blend One One)
// _Mode 1 = multiply: rgb *= 1 - tex.a * strength * 0.55          (Blend Zero OneMinusSrcAlpha)
Shader "Hellord/LightOverlay"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
        _Strength ("Strength", Float) = 1
        _Mode ("Mode (0 add, 1 mul)", Float) = 0
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst", Float) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
        Cull Off ZWrite Off ZTest Always
        Blend [_SrcBlend] [_DstBlend]
        Pass
        {
            // URP (Universal + 2D renderer) draws untagged/SRPDefaultUnlit passes; built-in ignores this tag
            // v0.0.22: no LightMode tag. URP draws untagged passes as SRPDefaultUnlit anyway; the tag made the box (Built-in + URP package) build strip the pass.
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #include "UnityCG.cginc"
            sampler2D _MainTex; float _Strength; float _Mode;
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; float4 color : COLOR; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; float4 color : COLOR; };
            v2f vert (appdata v) { v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.uv = v.uv; o.color = v.color; return o; }
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 t = tex2D(_MainTex, i.uv);
                float a = t.a * _Strength * i.color.a;
                if (_Mode < 0.5) return fixed4(t.rgb * a, 1);
                return fixed4(0, 0, 0, saturate(a * 0.55));
            }
            ENDCG
        }
    }
}
