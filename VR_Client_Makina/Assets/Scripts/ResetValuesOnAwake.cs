using UnityEngine;

public class ResetValuesOnAwake : MonoBehaviour
{
    [SerializeField]private CustomLitSettingsSO m_customLitSettings;
    
    private readonly int m_tilingShaderID = Shader.PropertyToID("_Tiling");
    private readonly int m_lightIntensityMultiplierShaderID = Shader.PropertyToID("_Light_Intensity_multiplier");
    private readonly int m_globalLightingShaderID = Shader.PropertyToID("_Global_Lighting");
    private readonly int m_hashTextureID = Shader.PropertyToID("_Hash");
    private readonly int m_occlusionShaderID = Shader.PropertyToID("_Occlusion");
    private readonly int m_triplanarBlendSharpnessID = Shader.PropertyToID("_TriplanarBlendSharpness");
    private readonly int m_smoothstepRID = Shader.PropertyToID("_Smoothstep_R");
    private readonly int m_smoothstepGID = Shader.PropertyToID("_Smoothstep_G");
    private readonly int m_smoothstepBID = Shader.PropertyToID("_Smoothstep_B");
    private readonly int m_beaconDefaultColorShaderID = Shader.PropertyToID("_BeaconDefaultColor");
    private readonly int m_beaconDetectionColorShaderID = Shader.PropertyToID("_BeaconDetectionColor");
    private readonly int m_beaconBitMaskShaderID = Shader.PropertyToID("_BeaconBitMask");

    void Awake()
    {
        Shader.SetGlobalFloat(m_tilingShaderID,m_customLitSettings.shadowTiling);
        Shader.SetGlobalFloat(m_lightIntensityMultiplierShaderID,m_customLitSettings.lightIntensityMultiplier);
        Shader.SetGlobalFloat(m_globalLightingShaderID,m_customLitSettings.globalIllumination);
        Shader.SetGlobalFloat(m_occlusionShaderID,m_customLitSettings.defaultOcclusionValue);
        Shader.SetGlobalFloat(m_triplanarBlendSharpnessID,m_customLitSettings.triplanarBlendSharpness);
        Shader.SetGlobalVector(m_smoothstepRID,Vector2To4(m_customLitSettings.smoothStepR));
        Shader.SetGlobalVector(m_smoothstepGID,Vector2To4(m_customLitSettings.smoothStepG));
        Shader.SetGlobalVector(m_smoothstepBID,Vector2To4(m_customLitSettings.smoothStepB));
        Shader.SetGlobalTexture(m_hashTextureID,m_customLitSettings.shadowHashTexture);
        Shader.SetGlobalColor(m_beaconDefaultColorShaderID, m_customLitSettings.beaconColor);
        Shader.SetGlobalColor(m_beaconDetectionColorShaderID, m_customLitSettings.beaconDetectionColor);
        Shader.SetGlobalInt(m_beaconBitMaskShaderID,0);
        
        Destroy(this);
    }

    /// <summary/> Converts a vector 2 to a vector4 with z and w = 0
    private Vector4 Vector2To4(Vector2 p_vector2)
    {return new Vector4(p_vector2.x, p_vector2.y, 0f, 0f);}
}
