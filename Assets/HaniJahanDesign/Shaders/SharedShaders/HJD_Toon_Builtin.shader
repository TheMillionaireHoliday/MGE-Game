Shader "Hani Jahan Design/Toon/Built-in"
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
        Tags { "RenderType" = "Opaque" }
        LOD 200

        Pass
        {
            Name "OUTLINE"
            Cull Front
            ZWrite On
            ColorMask RGB
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex vertOutline
            #pragma fragment fragOutline

            #include "UnityCG.cginc"

            #define HJD_TOON_OUTLINE_PASS

            float4 HJDTransformObjectToHClip(float3 positionOS)
            {
                return UnityObjectToClipPos(float4(positionOS, 1.0));
            }

            #include "HJD_Toon_Common.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "BASE"
            Cull Back
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _MaskTex;
            sampler2D _ColorRamp;

            float4 HJDTransformObjectToHClip(float3 positionOS)
            {
                return UnityObjectToClipPos(float4(positionOS, 1.0));
            }

            float3 HJDTransformObjectToWorld(float3 positionOS)
            {
                return mul(unity_ObjectToWorld, float4(positionOS, 1.0)).xyz;
            }

            float3 HJDTransformObjectToWorldNormal(float3 normalOS)
            {
                return UnityObjectToWorldNormal(normalOS);
            }

            float3 HJDGetMainLightDirection()
            {
                return _WorldSpaceLightPos0.xyz;
            }

            float3 HJDGetCameraPositionWS()
            {
                return _WorldSpaceCameraPos;
            }

            float3 HJDSampleAmbient(float3 normalWS)
            {
                return ShadeSH9(float4(normalWS, 1.0));
            }

            half4 HJDSampleMainTexture(float2 uv)
            {
                return tex2D(_MainTex, uv);
            }

            half4 HJDSampleMaskTexture(float2 uv)
            {
                return tex2D(_MaskTex, uv);
            }

            half4 HJDSampleColorRamp(float2 uv)
            {
                return tex2D(_ColorRamp, uv);
            }

            #include "HJD_Toon_Common.hlsl"
            ENDHLSL
        }
    }

    FallBack "Diffuse"
    CustomEditor "HaniJahanDesign.Shaders.HJDToonShaderGUI"
}
