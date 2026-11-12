// t950 Hellord PixelBossSackTint: proj2.sackv slate tint of the thrown sack sprite. l = mean(sRGB); rgb = SLATE6[clip(int(l*2.6),0,5)]
// (fx950_boss_ramps row 14), alpha cut at .5 like c.sprite. Swap onto the sack SpriteRenderer while it flies. NOT COMPILED, NOT TESTED IN UNITY.
Shader "Hellord/PixelBossSackTint"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _RampTex ("fx950_boss_ramps 8x20 (Point, Clamp)", 2D) = "white" {}
        _RampRows ("Ramp rows", Float) = 20
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="TransparentCutout" "IgnoreProjector"="True" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
        Cull Off ZWrite Off Blend Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            sampler2D _MainTex, _RampTex; float _RampRows;
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };
            v2f vert (appdata v) { v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.uv = v.uv; return o; }
            half4 frag (v2f i) : SV_Target
            {
                half4 c = tex2D(_MainTex, i.uv); clip(c.a - 0.5);
                half3 s = c.rgb;
                #ifndef UNITY_COLORSPACE_GAMMA
                s = LinearToGammaSpace(s);   // the mock-up averages sRGB values
                #endif
                int k = clamp((int)floor((s.r + s.g + s.b) / 3.0 * 2.6), 0, 5);
                return half4(tex2Dlod(_RampTex, float4((k + 0.5) / 8.0, 1.0 - 14.5 / _RampRows, 0, 0)).rgb, 1);
            }
            ENDCG
        }
    }
}
