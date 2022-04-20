#ifndef CUSTOM_LIGHTING_INCLUDED
#define CUSTOM_LIGHTING_INCLUDED

// This is a neat trick to work around a bug in the shader graph when
// enabling shadow keywords. Created by @cyanilux
// https://github.com/Cyanilux/URP_ShaderGraphCustomLighting
#ifndef SHADERGRAPH_PREVIEW
    #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
    #if (SHADERPASS != SHADERPASS_FORWARD)
        #undef REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR
    #endif
#endif

// Une struct qui contient le lighting data du vertex actuel
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
    
    // Baked lighting
    float3 bakedGI;
    float4 shadowMask;
    float fogFactor;
};

// Une struct qui contient les data's qui vont être renvoyés au shader
struct Outputs
{
    float3 color;
    float lightLevel;
};

// Translate a [0, 1] smoothness value to an exponent 
float GetSmoothnessPower(float rawSmoothness) {
    return exp2(10 * rawSmoothness + 1);
}

// Sorting through the shader graph preview to avoid errors
#ifndef SHADERGRAPH_PREVIEW

// Handling the light
Outputs CustomLightHandling(CustomLightingData d, Light light)
{
    // setting the level of light (radiance)
    float3 radiance = light.color * (light.distanceAttenuation * light.shadowAttenuation);

    // Getting the luminance level of light
    float luminance =  0.3  * light.color.r + 0.59  * light.color.g + 0.11  * light.color.b;
    float lightLvl = light.distanceAttenuation * light.shadowAttenuation * luminance;
    
    
    // Getting the diffuse lighting
    float diffuse = saturate(dot(d.normalWS,light.direction));

    // Getting the specular lighting
    float specularDot = saturate(dot(d.normalWS, normalize(light.direction + d.viewDirectionWS)));
    float specular = pow(specularDot, GetSmoothnessPower(d.smoothness)) * diffuse;

    // Setting the color to it's new value
    float3 color = d.albedo * radiance * (diffuse + specular);

    //returning the outputs
    Outputs outputs;
    outputs.color = color;
    outputs.lightLevel = lightLvl;
    
    return outputs;
}
#endif


Outputs CustomGlobalIllumination(CustomLightingData d) {
    Outputs outputs;
    float3 indirectDiffuse = d.albedo * d.bakedGI * d.ambientOcclusion;

    float3 reflectVector = reflect(-d.viewDirectionWS, d.normalWS);
    outputs.color = indirectDiffuse;
    outputs.lightLevel =  0.3  * outputs.color.r + 0.59  * outputs.color.g + 0.11  * outputs.color.b;
    
    return outputs;
}

// The function to calculate the custom lighting
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
    // Get the main light
    Light mainLight = GetMainLight(d.shadowCoord, d.positionWS,  d.shadowMask);

    // setting initial values
    outputs.color = d.albedo * d.ambientOcclusion;
    outputs.lightLevel = 0;
    
    // Shading the main light
    // Outputs results;
    
    Outputs results;
    results = CustomGlobalIllumination(d);
    Outputs results2 = CustomLightHandling(d,mainLight);

    outputs.color += results.color + results2.color; 
    outputs.lightLevel += results.lightLevel + results2.lightLevel; 
    
    #ifdef _ADDITIONAL_LIGHTS
    // Running the lighting for every light interacting wih the object
    uint numAdditionalLights = GetAdditionalLightsCount();
    for (uint i = 0; i < numAdditionalLights; i++) {
        Light light = GetAdditionalLight(i, d.positionWS, 1);
        
        Outputs results = CustomLightHandling(d,light);
        outputs.color += results.color; 
        outputs.lightLevel += results.lightLevel; 
    }
    #endif
    // mixing the color with the fog
    outputs.color = MixFog(outputs.color, d.fogFactor);
    return outputs;
#endif
}

// the function called by the custom node
void CalculateCustomLighting_float(float3 Position, float3 Normal,float3 ViewDirection,
    float3 Albedo, float Smoothness, float AmbientOcclusion,float2 LightmapUV,
    out float3 Color, out float LightLevel) {

    CustomLightingData d;
    d.positionWS = Position;
    d.normalWS = Normal;
    d.viewDirectionWS = ViewDirection;
    d.albedo = Albedo;
    d.smoothness = Smoothness;
    d.ambientOcclusion = AmbientOcclusion;

    #ifdef SHADERGRAPH_PREVIEW
    // In preview, there's no shadows
    d.shadowCoord = 0;

    d.fogFactor = 0;
    #else
    // Calculate the main light shadow coords depending on if cascades are enabled
    float4 positionCS = TransformWorldToHClip(Position);
    #if SHADOWS_SCREEN
        d.shadowCoord = ComputeScreenPos(positionCS);
    #else
        d.shadowCoord = TransformWorldToShadowCoord(Position);
    #endif

    // The lightmap UV is usually in TEXCOORD1
    // If lightmaps are disabled, OUTPUT_LIGHTMAP_UV does nothing
    float2 lightmapUV;
    OUTPUT_LIGHTMAP_UV(LightmapUV, unity_LightmapST, lightmapUV);
    // Samples spherical harmonics, which encode light probe data
    float3 vertexSH;
    OUTPUT_SH(Normal, vertexSH);
    // This function calculates the final baked lighting from light maps or probes
    d.bakedGI = SAMPLE_GI(lightmapUV, vertexSH, Normal);
    // This function calculates the shadow mask if baked shadows are enabled
    d.shadowMask = SAMPLE_SHADOWMASK(lightmapUV);
    
    d.fogFactor = ComputeFogFactor(positionCS.z);
    #endif
    Outputs outputs = CalculateCustomLighting(d);
    Color = outputs.color;
    LightLevel = outputs.lightLevel;
}

#endif