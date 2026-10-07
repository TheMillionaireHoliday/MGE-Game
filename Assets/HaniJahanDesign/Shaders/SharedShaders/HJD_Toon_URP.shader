Shader "Hani Jahan Design/Toon/URP"
{
    Properties
    {
        [Header(Surface)]
        [MainTexture] _MainTex ("Base Map", 2D) = "white" {}
        [MainColor] _BaseColor ("Tint", Color) = (1, 1, 1, 1)

        [Header(Toon Shading)]
        [Toggle] _EnableBaseShading ("Enabled", Float) = 1
        _MappingSource ("Shading Source", Float) = 0
        [Enum(Color Ramp, 0, Smooth Two Color, 1, Hard Two Color, 2, Color Steps, 3)] _ColorSource ("Color Mode", Float) = 0
        _ColorRamp ("Color Ramp", 2D) = "white" {}
        _Color1 ("Shadow Color", Color) = (1, 0.5, 0, 1)
        _Color2 ("Lit Color", Color) = (0.8, 0, 0.5, 1)
        _CelThreshold ("Shadow Size", Range(0, 1)) = 0.5
        _CelFeather ("Shadow Softness", Range(0, 0.25)) = 0.02
        _CelStepCount ("Shade Steps", Range(2, 8)) = 3
        _CelStepSoftness ("Step Softness", Range(0, 1)) = 0
        _GradientScale ("Height Scale", Float) = 1
        _MaskTex ("Mapping Mask", 2D) = "white" {}
        _GradientOffset ("Shading Offset", Range(-1, 1)) = 0
        _BlendSharpness ("Shading Contrast", Range(0.1, 10)) = 1

        [Header(Scene Lighting)]
        [Toggle] _EnableLighting ("Enabled", Float) = 1
        _AmbientStrength ("Ambient Strength", Range(0, 1)) = 0.1
        _LightWrap ("Wrap", Range(0, 1)) = 0
        _Smoothness ("Shadow Falloff", Range(0.1, 10)) = 1
        _ShadowMultiplier ("Light Strength", Range(0, 5)) = 1
        _LightDirection ("Direction Override", Vector) = (0, 0, 0, 0)

        [Header(Rim Light)]
        [Toggle] _EnableRim ("Enabled", Float) = 0
        _RimColor ("Rim Color", Color) = (1, 1, 1, 1)
        _RimPower ("Rim Power", Range(1, 8)) = 4
        _RimIntensity ("Rim Intensity", Range(0, 3)) = 1

        [Header(Outline)]
        [Toggle] _EnableOutline ("Enabled", Float) = 0
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
        _OutlineWidth ("Outline Width", Float) = 0.03
        _OutlineDepthOffset ("Outline Depth Offset", Range(-0.01, 0.01)) = 0

        // Unity's TextCore material reset command is available for every material and
        // writes these values without first checking whether the shader is a text shader.
        // Keep them hidden and unused so invoking that editor command stays warning-free.
        [HideInInspector] _TextureWidth ("Texture Width", Float) = 512
        [HideInInspector] _TextureHeight ("Texture Height", Float) = 512
        [HideInInspector] _WeightNormal ("Weight Normal", Float) = 0
        [HideInInspector] _WeightBold ("Weight Bold", Float) = 0.5
    }

    SubShader
    {
        PackageRequirements
        {
            "com.unity.render-pipelines.universal"
        }

        Tags { "Queue" = "Geometry" "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 200

        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode" = "UniversalForward" }
            Cull Back

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_MaskTex);
            SAMPLER(sampler_MaskTex);
            TEXTURE2D(_ColorRamp);
            SAMPLER(sampler_ColorRamp);

            float4 HJDTransformObjectToHClip(float3 positionOS)
            {
                return TransformObjectToHClip(positionOS);
            }

            float3 HJDTransformObjectToWorld(float3 positionOS)
            {
                return TransformObjectToWorld(positionOS);
            }

            float3 HJDTransformObjectToWorldNormal(float3 normalOS)
            {
                return TransformObjectToWorldNormal(normalOS);
            }

            float3 HJDGetMainLightDirection()
            {
                return GetMainLight().direction;
            }

            float3 HJDGetCameraPositionWS()
            {
                return GetCameraPositionWS();
            }

            float3 HJDSampleAmbient(float3 normalWS)
            {
                return SampleSH(normalWS);
            }

            half4 HJDSampleMainTexture(float2 uv)
            {
                return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
            }

            half4 HJDSampleMaskTexture(float2 uv)
            {
                return SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, uv);
            }

            half4 HJDSampleColorRamp(float2 uv)
            {
                return SAMPLE_TEXTURE2D(_ColorRamp, sampler_ColorRamp, uv);
            }

            #include "HJD_Toon_Common.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            Cull Back
            ZWrite On
            ColorMask 0

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct DepthOnlyAttributes
            {
                float4 positionOS : POSITION;
            };

            struct DepthOnlyVaryings
            {
                float4 positionCS : SV_POSITION;
            };

            DepthOnlyVaryings DepthOnlyVertex(DepthOnlyAttributes input)
            {
                DepthOnlyVaryings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }

            half4 DepthOnlyFragment(DepthOnlyVaryings input) : SV_Target
            {
                return 0;
            }

            // If alpha clipping or vertex displacement is added to the surface,
            // reproduce it here so the depth silhouette stays accurate.
            ENDHLSL
        }

        Pass
        {
            Name "OUTLINE"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            Cull Front
            ZWrite On
            ColorMask RGB

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex vertOutline
            #pragma fragment fragOutline

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #define HJD_TOON_OUTLINE_PASS

            float4 HJDTransformObjectToHClip(float3 positionOS)
            {
                return TransformObjectToHClip(positionOS);
            }

            #include "HJD_Toon_Common.hlsl"
            ENDHLSL
        }
    }

    CustomEditor "HaniJahanDesign.Shaders.HJDToonShaderGUI"
}
