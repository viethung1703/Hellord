// Pixel FX Pack v1.2 - additive sprite shader for glows, slash arcs, sparks, afterimages and fake lights.
// Blend One One: colours add up, so overlapping glows brighten like light. Works without 2D lights; with URP HDR + Bloom it blooms.
// UNTESTED IN EDITOR - see Documentation/README.md "Known limits".
Shader "PixelFX/Additive"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Intensity ("Intensity", Range(0,8)) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
        Cull Off Lighting Off ZWrite Off
        Blend One One
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata { float4 vertex : POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };
            sampler2D _MainTex; float4 _Color; float _Intensity;
            v2f vert (appdata v) { v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.uv = v.uv; o.color = v.color * _Color; return o; }
            float4 frag (v2f i) : SV_Target
            {
                float4 c = tex2D(_MainTex, i.uv) * i.color;
                return float4(c.rgb * c.a * _Intensity, 0);
            }
            ENDHLSL
        }
    }
    Fallback "Sprites/Default"
}
