#ifndef BEACON_DETECTION
#define BEACON_DETECTION

// This is a neat trick to work around a bug in the shader graph when
// enabling shadow keywords. Created by @cyanilux
// https://github.com/Cyanilux/URP_ShaderGraphCustomLighting
#ifndef SHADERGRAPH_PREVIEW
    #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
    #if (SHADERPASS != SHADERPASS_FORWARD)
        #undef REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR
    #endif
#endif

void CalculateBeaconDetectionDistance_float(float3 Position,int ActiveBitMask,int p_detectedBeaconBitMask,
float3 p_beaconPos0,float3 p_beaconPos1,float3 p_beaconPos2,float3 p_beaconPos3,
float3 p_beaconPos4,float3 p_beaconPos5,float3 p_beaconPos6,float3 p_beaconPos7,
float3 p_beaconPos8,float3 p_beaconPos9,out float distanceFromBeacon, out float distanceFromDetectedBeacon) {

    float3 beaconPositions[10] = {p_beaconPos0, p_beaconPos1, p_beaconPos2, p_beaconPos3, p_beaconPos4,
        p_beaconPos5, p_beaconPos6, p_beaconPos7, p_beaconPos8, p_beaconPos9};
    
    float beaconDistance = 999999;
    
    for (uint i = 0; i < 10; i++)
    {
        if((ActiveBitMask & 1<<i) != 1<<i) continue;
        beaconDistance = min(distance(beaconPositions[i],Position), beaconDistance);
    }

    float beaconDistanceDetected = 999999;

    for (uint i = 0; i < 10; i++)
    {
        if((p_detectedBeaconBitMask & 1<<i) != 1<<i && (ActiveBitMask & 1<<i) != 1<<i) continue;
        beaconDistanceDetected = min(distance(beaconPositions[p_detectedBeaconBitMask],Position),beaconDistanceDetected);
    }
    
    distanceFromDetectedBeacon = beaconDistanceDetected;
    distanceFromBeacon = beaconDistance;
}
#endif