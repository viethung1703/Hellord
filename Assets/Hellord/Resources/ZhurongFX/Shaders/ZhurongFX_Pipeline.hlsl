// URP / built-in switch for the Zhurong shaders (keeps the effect maths pipeline-agnostic).
#ifndef ZHURONG_FX_PIPELINE
#define ZHURONG_FX_PIPELINE
#if defined(ZFX_URP)
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #define ZFX_OBJ2CLIP(v) TransformObjectToHClip(v)
    #define ZFX_OBJ2WORLD(v) TransformObjectToWorld(v)
#else
    #include "UnityCG.cginc"
    #define ZFX_OBJ2CLIP(v) UnityObjectToClipPos(float4(v, 1.0))
    #define ZFX_OBJ2WORLD(v) mul(unity_ObjectToWorld, float4(v, 1.0)).xyz
#endif
#endif
