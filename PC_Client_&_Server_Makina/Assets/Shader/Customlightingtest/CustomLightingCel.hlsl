#ifndef CUSTOM_LIGHTING_CEL_INCLUDED
#define CUSTOM_LIGHTING_CEL_INCLUDED

// This is a neat trick to work around a bug in the shader graph when
// enabling shadow keywords. Created by @cyanilux
// https://github.com/Cyanilux/URP_ShaderGraphCustomLighting
#ifndef SHADERGRAPH_PREVIEW
    #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
    #if (SHADERPASS != SHADERPASS_FORWARD)
        #undef REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR
    #endif
#endif

struct CustomLightingData {

    // position and orientation
    float3 positionWS;
    float3 normalWS;
    float3 viewDirectionWS;
    float4 shadowCoord;
    
    // Surface attributes
    float3 albedo;
    float smoothness;
    float ambientOcclusion;
    
    float fogFactor;
};

struct Outputs
{
    float3 color;
    float lightLevel;
};

// Translate a [0, 1] smoothness value to an exponent 
float GetSmoothnessPower(float rawSmoothness) {
    return exp2(10 * rawSmoothness + 1);
}

#ifndef SHADERGRAPH_PREVIEW
float3 CustomGlobalIllumination(CustomLightingData d) {
    float3 indirectDiffuse = d.albedo * d.ambientOcclusion;

    return indirectDiffuse;
}

Outputs CustomLightHandling(CustomLightingData d, Light light)
{
    float3 radiance = light.color * (light.distanceAttenuation * light.shadowAttenuation);
    
    float lightLvl = light.distanceAttenuation * light.shadowAttenuation * max(max(light.color.r,light.color.g),light.color.b);
    
    float diffuse = saturate(dot(d.normalWS,light.direction));
    float specularDot = saturate(dot(d.normalWS, normalize(light.direction + d.viewDirectionWS)));
    float specular = pow(specularDot, GetSmoothnessPower(d.smoothness)) * diffuse;
    
    float3 color = d.albedo * radiance * (diffuse + specular);

    lightLvl = lightLvl;
    
    Outputs outputs;
    outputs.color = color;
    outputs.lightLevel = lightLvl;
    
    return outputs;
}
#endif

Outputs CalculateCustomLighting(CustomLightingData d) {
    Outputs outputs;
#ifdef SHADERGRAPH_PREVIEW

    // In preview, estimate diffuse + specular
    float3 lightDir = float3(0.5, 0.5, 0);
    float intensity = saturate(dot(d.normalWS, lightDir)) + pow(saturate(dot(d.normalWS, normalize(d.viewDirectionWS + lightDir))), GetSmoothnessPower(d.smoothness));
    
    outputs.color = d.albedo * intensity;
    outputs.lightLevel = 1;
    return outputs;
#else
    // Get the main light. Located in URP/ShaderLibrary/Lighting.hlsl
    Light mainLight = GetMainLight(d.shadowCoord, d.positionWS, 1);

    outputs.color = CustomGlobalIllumination(d);
    outputs.lightLevel = 0;
    // Shading the main light
    Outputs results = CustomLightHandling(d,mainLight);
    outputs.color += results.color; 
    outputs.lightLevel += results.lightLevel; 
    
    #ifdef _ADDITIONAL_LIGHTS
    // Shade additional cone and point lights. Functions in URP/ShaderLibrary/Lighting.hlsl
    uint numAdditionalLights = GetAdditionalLightsCount();
    for (uint i = 0; i < numAdditionalLights; i++) {
        Light light = GetAdditionalLight(i, d.positionWS, 1);
        
        Outputs results = CustomLightHandling(d,light);
        outputs.color += results.color; 
        outputs.lightLevel += results.lightLevel; 
    }
    #endif
    outputs.color = MixFog(outputs.color, d.fogFactor);
    return outputs;
#endif
}

void CalculateCustomLighting_float(float3 Position, float3 Normal,float3 ViewDirection,
    float3 Albedo, float Smoothness, float AmbientOcclusion,
    out float3 Color, out float LightLevel) {

    CustomLightingData d;
    d.positionWS = Position;
    d.normalWS = Normal;
    d.viewDirectionWS = ViewDirection;
    d.albedo = Albedo;
    d.smoothness = Smoothness;
    d.ambientOcclusion = AmbientOcclusion;

    #ifdef SHADERGRAPH_PREVIEW
    // In preview, there's no shadows or bakedGI
    d.shadowCoord = 0;

    d.fogFactor = 0;
    #else
    // Calculate the main light shadow coord
    // There are two types depending on if cascades are enabled
    float4 positionCS = TransformWorldToHClip(Position);
    #if SHADOWS_SCREEN
        d.shadowCoord = ComputeScreenPos(positionCS);
    #else
        d.shadowCoord = TransformWorldToShadowCoord(Position);
    #endif

    d.fogFactor = ComputeFogFactor(positionCS.z);
    #endif
    
    Outputs outputs = CalculateCustomLighting(d);
    Color = outputs.color;
    LightLevel = outputs.lightLevel;
}

#endif