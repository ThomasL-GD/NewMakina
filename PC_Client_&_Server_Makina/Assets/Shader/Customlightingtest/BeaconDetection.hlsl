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
float3 p_beaconPos8,float3 p_beaconPos9, float beaconTimer0, float beaconTimer1, float beaconTimer2, float beaconTimer3,
float beaconTimer4, float beaconTimer5, float beaconTimer6, float beaconTimer7, float beaconTimer8, float beaconTimer9, float p_time, float p_Range,
out float distanceFromBeacon, out float distanceFromDetectedBeacon) {

    float3 beaconPositions[10] = {p_beaconPos0, p_beaconPos1, p_beaconPos2, p_beaconPos3, p_beaconPos4,
        p_beaconPos5, p_beaconPos6, p_beaconPos7, p_beaconPos8, p_beaconPos9};

    float beaconTimers[10] = {beaconTimer0, beaconTimer1, beaconTimer2, beaconTimer3,
        beaconTimer4, beaconTimer5, beaconTimer6, beaconTimer7, beaconTimer8, beaconTimer9};
    
    float beaconDistance = 999999;
    
    float3 position2D  = Position;
    position2D.y = 0;
    
    for (uint i = 0; i < 10; i++)
    {
        if((ActiveBitMask & 1<<i) != 1<<i) continue;
        // TODO : make 2D
        float3 beaconPosition2D = beaconPositions[i];
        beaconPosition2D.y = 0;
        beaconDistance = min(distance(beaconPosition2D,position2D) / min((p_time - beaconTimers[i]) * 10 ,1),beaconDistance);
    }
    
    beaconDistance /= p_Range;
    beaconDistance = 1- beaconDistance;

    float beaconDistanceDetected = 999999;

    for (uint j = 0; j < 10; j++)
    {
        if((ActiveBitMask & 1<<j) == 0 || (p_detectedBeaconBitMask & 1<<j)== 0) continue;
        // TODO : make 2D
        
        float3 beaconPosition2D = beaconPositions[j];
        beaconPosition2D.y = 0;
        
        beaconDistanceDetected = min(distance(beaconPosition2D,position2D),beaconDistanceDetected);
    }
    
    beaconDistanceDetected /= p_Range;
    beaconDistanceDetected = 1- beaconDistanceDetected;
    
    distanceFromDetectedBeacon = beaconDistanceDetected;
    distanceFromBeacon = beaconDistance;
}
#endif