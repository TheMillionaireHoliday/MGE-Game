#ifndef HJD_TOON_COMMON_INCLUDED
#define HJD_TOON_COMMON_INCLUDED

// Pipeline adapters are supplied by the small Built-in and URP shader wrappers.
// Everything below this boundary is render-pipeline independent.

float3 _LightDirection;
float _EnableLighting;
float _AmbientStrength;
float _LightWrap;
float _EnableOutline;
float _OutlineWidth;
float _OutlineDepthOffset;
half4 _OutlineColor;

float HaniLengthSquared(float3 value)
{
    return dot(value, value);
}

float3 HaniSafeNormalize(float3 value, float3 fallback)
{
    float lengthSquared = HaniLengthSquared(value);
    return lengthSquared > 1e-5 ? value * rsqrt(lengthSquared) : fallback;
}

#if defined(HJD_TOON_OUTLINE_PASS)
struct appdata
{
    float4 vertex : POSITION;
    float3 normal : NORMAL;
};

struct v2f
{
    float4 pos : SV_POSITION;
};

v2f vertOutline(appdata v)
{
    v2f o;
    float3 normal = HaniSafeNormalize(v.normal, float3(0.0, 1.0, 0.0));
    o.pos = HJDTransformObjectToHClip(v.vertex.xyz + normal * _OutlineWidth);
    o.pos.z += _OutlineDepthOffset * o.pos.w;
    return o;
}

half4 fragOutline(v2f i) : SV_Target
{
    clip(_EnableOutline - 0.5);
    return _OutlineColor;
}
#else
float4 _MainTex_ST;
float4 _MaskTex_ST;
half4 _BaseColor;
half4 _Color1;
half4 _Color2;
half4 _RimColor;
float _EnableBaseShading;
float _MappingSource;
float _ColorSource;
float _Smoothness;
float _ShadowMultiplier;
float _BlendSharpness;
float _GradientOffset;
float _GradientScale;
float _CelThreshold;
float _CelFeather;
float _CelStepCount;
float _CelStepSoftness;
float _EnableRim;
float _RimPower;
float _RimIntensity;

float3 HaniGetWorldNormal(float3 objectNormal)
{
    return HaniSafeNormalize(HJDTransformObjectToWorldNormal(objectNormal), float3(0.0, 1.0, 0.0));
}

float3 HaniGetMainLightDirection()
{
    float3 overrideDirection = HaniSafeNormalize(-_LightDirection.xyz, float3(0.0, 1.0, 0.0));
    float overrideEnabled = step(1e-5, HaniLengthSquared(_LightDirection.xyz));
    float3 pipelineDirection = HaniSafeNormalize(HJDGetMainLightDirection(), float3(0.0, 1.0, 0.0));
    return lerp(pipelineDirection, overrideDirection, overrideEnabled);
}

float HaniWrappedNdotL(float3 worldNormal, float3 lightDirection)
{
    float wrapped = (dot(worldNormal, lightDirection) + _LightWrap) / max(1.0 + _LightWrap, 1e-5);
    return saturate(wrapped);
}

float HaniToonLightFactor(float3 worldNormal, float smoothness, float multiplier)
{
    float ndotl = HaniWrappedNdotL(worldNormal, HaniGetMainLightDirection());
    ndotl = pow(ndotl, max(smoothness, 1e-4));
    return saturate(ndotl * multiplier);
}

float3 HaniApplyAmbient(float3 color, float3 worldNormal)
{
    // Ambient light is irradiance, not an emissive color.  Adding it directly
    // fills in channels which the toon ramp intentionally left dark and makes
    // saturated ramp colors turn pastel (especially with URP's SampleSH).
    // Modulate the authored color instead so ambient light can brighten the
    // surface without destroying the palette's hue.
    float3 ambient = max(HJDSampleAmbient(worldNormal), 0.0);
    return saturate(color * (1.0 + ambient * _AmbientStrength));
}

struct appdata
{
    float4 vertex : POSITION;
    float3 normal : NORMAL;
    float2 uv : TEXCOORD0;
    half4 color : COLOR;
};

struct v2f
{
    float4 pos : SV_POSITION;
    float3 worldNormal : TEXCOORD0;
    float3 worldPos : TEXCOORD1;
    float3 objectNormal : TEXCOORD2;
    float3 objectPos : TEXCOORD3;
    float2 baseUv : TEXCOORD4;
    float2 maskUv : TEXCOORD5;
    half4 vertexColor : TEXCOORD6;
};

v2f vert(appdata v)
{
    v2f o;
    o.pos = HJDTransformObjectToHClip(v.vertex.xyz);
    o.worldNormal = HaniGetWorldNormal(v.normal);
    o.worldPos = HJDTransformObjectToWorld(v.vertex.xyz);
    o.objectNormal = HaniSafeNormalize(v.normal, float3(0.0, 1.0, 0.0));
    o.objectPos = v.vertex.xyz;
    o.baseUv = v.uv * _MainTex_ST.xy + _MainTex_ST.zw;
    o.maskUv = v.uv * _MaskTex_ST.xy + _MaskTex_ST.zw;
    o.vertexColor = v.color;
    return o;
}

half4 frag(v2f i) : SV_Target
{
    float3 normal = HaniSafeNormalize(i.worldNormal, float3(0.0, 1.0, 0.0));
    float lightingEnabled = step(0.5, _EnableLighting);
    float lightFactor = lerp(1.0, HaniToonLightFactor(normal, _Smoothness, _ShadowMultiplier), lightingEnabled);
    float3 viewDirection = HaniSafeNormalize(HJDGetCameraPositionWS() - i.worldPos, float3(0.0, 0.0, 1.0));

    float mapping = lightFactor;
    mapping = _MappingSource < 1.5 ? (_MappingSource < 0.5 ? lightFactor : normal.y * 0.5 + 0.5) : mapping;
    mapping = (_MappingSource >= 1.5 && _MappingSource < 2.5) ? i.objectNormal.y * 0.5 + 0.5 : mapping;
    mapping = (_MappingSource >= 2.5 && _MappingSource < 3.5) ? i.worldPos.y * _GradientScale + 0.5 : mapping;
    mapping = (_MappingSource >= 3.5 && _MappingSource < 4.5) ? i.objectPos.y * _GradientScale + 0.5 : mapping;
    mapping = (_MappingSource >= 4.5 && _MappingSource < 5.5) ? 1.0 - saturate(dot(normal, viewDirection)) : mapping;
    mapping = (_MappingSource >= 5.5 && _MappingSource < 6.5) ? i.vertexColor.a : mapping;
    if (_MappingSource >= 6.5)
        mapping = HJDSampleMaskTexture(i.maskUv).r;
    mapping = pow(saturate(mapping + _GradientOffset), max(_BlendSharpness, 1e-4));

    half4 shadingColor;
    if (_ColorSource < 0.5)
        shadingColor = HJDSampleColorRamp(float2(mapping, 0.5));
    else if (_ColorSource < 1.5)
        shadingColor = lerp(_Color1, _Color2, mapping);
    else if (_ColorSource < 2.5)
    {
        float feather = max(_CelFeather, 1e-5);
        float cel = smoothstep(_CelThreshold - feather, _CelThreshold + feather, mapping);
        shadingColor = lerp(_Color1, _Color2, cel);
    }
    else
    {
        // Quantize the dark-to-light blend into evenly spaced painted-looking bands.
        // Softness widens only the transition between bands and leaves their count intact.
        float stepCount = clamp(round(_CelStepCount), 2.0, 8.0);
        float scaledMapping = mapping * (stepCount - 1.0);
        float lowerStep = floor(scaledMapping);
        float stepBlend = step(0.5, frac(scaledMapping));
        float blendWidth = saturate(_CelStepSoftness) * 0.5;
        stepBlend = lerp(stepBlend, smoothstep(0.5 - blendWidth, 0.5 + blendWidth, frac(scaledMapping)), step(1e-5, blendWidth));
        float steppedMapping = saturate((lowerStep + stepBlend) / (stepCount - 1.0));
        shadingColor = lerp(_Color1, _Color2, steppedMapping);
    }
    shadingColor = lerp(half4(1.0, 1.0, 1.0, 1.0), shadingColor, step(0.5, _EnableBaseShading));

    half4 color = HJDSampleMainTexture(i.baseUv) * _BaseColor * shadingColor;
    color.rgb = lerp(color.rgb, HaniApplyAmbient(color.rgb, normal), lightingEnabled);
    float rim = pow(1.0 - saturate(dot(normal, viewDirection)), max(_RimPower, 1e-4));
    color.rgb += rim * _RimIntensity * _RimColor.rgb * _RimColor.a * step(0.5, _EnableRim);
    return saturate(color);
}
#endif
#endif
