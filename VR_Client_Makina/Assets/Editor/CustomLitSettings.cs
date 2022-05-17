using System;
using static UnityEditor.EditorGUILayout;
using static UnityEngine.GUILayout;
using UnityEditor;
using UnityEngine;

public class CustomLitSettings : EditorWindow
{
    private static CustomLitSettingsSO m_customLitSettings;
    private static readonly int m_tilingShaderID = Shader.PropertyToID("_Tiling");
    private static readonly int m_lightIntensityMultiplierShaderID = Shader.PropertyToID("_Light_Intensity_multiplier");
    private static readonly int m_globalLightingShaderID = Shader.PropertyToID("_Global_Lighting");
    private static readonly int m_hashTextureID = Shader.PropertyToID("_Hash");
    private static readonly int m_occlusionShaderID = Shader.PropertyToID("_Occlusion");
    private static readonly int m_triplanarBlendSharpnessID = Shader.PropertyToID("_TriplanarBlendSharpness");
    private static readonly int m_smoothstepRID = Shader.PropertyToID("_Smoothstep_R");
    private static readonly int m_smoothstepGID = Shader.PropertyToID("_Smoothstep_G");
    private static readonly int m_smoothstepBID = Shader.PropertyToID("_Smoothstep_B");
    private static readonly int m_beaconDefaultColor = Shader.PropertyToID("_BeaconDefaultColor");
    private static readonly int m_beaconDetectionColor = Shader.PropertyToID("_BeaconDetectionColor");
    private readonly int m_beaconBitMaskShaderID = Shader.PropertyToID("_BeaconBitMask");

    [MenuItem("Tools/Custom Lit Settings")]
    static void Init()
    {
        // Instantiating or fetching the PrefabPicasso window 
        CustomLitSettings window = (CustomLitSettings)GetWindow(typeof(CustomLitSettings));
        
        // Giving the window the "Prefab Picasso" name
        window.titleContent = new GUIContent("Custom Lit Settings");
        
        // Display the window that has been created
        window.Show();
        
        // Bring the window to the front
        window.Focus();
        
        // Updating the GUI of the window
        window.Repaint();
    }

    private void OnGUI()
    {
        bool hasChanged = false;
        if (m_customLitSettings == null)
        {
            m_customLitSettings = Resources.Load("Custom Lit Settings/customLitSettings", typeof(CustomLitSettingsSO)) as CustomLitSettingsSO;
            if (m_customLitSettings == null)
            {
                AssetDatabase.CreateAsset(CreateInstance<CustomLitSettingsSO>(), "Assets/Resources/Custom Lit Settings/customLitSettings.asset");
                m_customLitSettings = Resources.Load("Custom Lit Settings/customLitSettings", typeof(CustomLitSettingsSO)) as CustomLitSettingsSO;
            }
            else return;
        }
        
        if(Event.current.type == EventType.MouseDown) Debug.Log(PrintCustomSettings(m_customLitSettings));
        
        // 360
        if (CheckSoWithShaderGlobalValues(m_customLitSettings)) Refresh();

        float newFloat = FloatField("Shadow Tiling", Mathf.Max(m_customLitSettings.shadowTiling,0f));
        UpdateNewValue(newFloat, m_tilingShaderID,ref m_customLitSettings.shadowTiling,ref hasChanged);

        newFloat = FloatField("Light Intensity Multiplier", Mathf.Max(m_customLitSettings.lightIntensityMultiplier,0f));
        UpdateNewValue(newFloat, m_lightIntensityMultiplierShaderID,ref m_customLitSettings.lightIntensityMultiplier,ref hasChanged);
        
        
        newFloat = Slider("Ambient Occlusion", m_customLitSettings.defaultOcclusionValue,0f,1f);
        UpdateNewValue(newFloat, m_occlusionShaderID,ref m_customLitSettings.defaultOcclusionValue,ref hasChanged);

        newFloat = Slider("Minimum Illumination Value", m_customLitSettings.globalIllumination, 0f, 1f);
        UpdateNewValue(newFloat,m_globalLightingShaderID,ref m_customLitSettings.globalIllumination,ref hasChanged);

        newFloat = FloatField("TriplanarBlendSharpness", Mathf.Max(m_customLitSettings.triplanarBlendSharpness, 0f));
        UpdateNewValue(newFloat,m_triplanarBlendSharpnessID,ref m_customLitSettings.triplanarBlendSharpness,ref hasChanged);
        
        Vector2 newVector2 = Vector2Field("Smoothstep R",m_customLitSettings.smoothStepR);
        UpdateNewValue(newVector2,m_smoothstepRID,ref m_customLitSettings.smoothStepR,ref hasChanged);

        newVector2 = Vector2Field("Smoothstep G",m_customLitSettings.smoothStepG);
        UpdateNewValue(newVector2,m_smoothstepGID,ref m_customLitSettings.smoothStepG,ref hasChanged);

        newVector2 = Vector2Field("Smoothstep B",m_customLitSettings.smoothStepB);
        UpdateNewValue(newVector2,m_smoothstepBID,ref m_customLitSettings.smoothStepB,ref hasChanged);
        
        Texture2D newTexture2D = ObjectField("Shadow Hash", m_customLitSettings.shadowHashTexture,typeof(Texture2D), false) as Texture2D;
        UpdateNewValue(newTexture2D, m_hashTextureID,ref m_customLitSettings.shadowHashTexture, ref hasChanged);

        Color newColor = ColorField("Beacon Color", m_customLitSettings.beaconColor);
        UpdateNewValue(newColor, m_beaconDefaultColor, ref m_customLitSettings.beaconColor, ref hasChanged);

        Color newColorDetection = ColorField("Beacon Color Detected", m_customLitSettings.beaconDetectionColor);
        UpdateNewValue(newColorDetection, m_beaconDetectionColor, ref m_customLitSettings.beaconDetectionColor, ref hasChanged);
        
        
        Undo.ClearSnapshotTarget();
        
        if (hasChanged)
        {
            EditorUtility.SetDirty(m_customLitSettings);
            AssetDatabase.SaveAssets();
        }

        if (Button("ResetBeacons"))
        {
            Shader.SetGlobalInt(m_beaconBitMaskShaderID,0);
        }
    }


    /// <param name="p_newValue"> the new value gotten from the serialization </param>
    /// <param name="p_shaderPropertyId"> the shader property ID of the shader to modify </param>
    /// <param name="p_soValue"> reference to the so value </param>
    /// <param name="p_hasChanged"> reference to the has changed bool </param>
    void UpdateNewValue(float p_newValue, int p_shaderPropertyId,ref float p_soValue,ref bool p_hasChanged)
    {
        if (p_newValue != p_soValue)
        {
            p_soValue = p_newValue;
            Shader.SetGlobalFloat(p_shaderPropertyId, p_newValue);
            p_hasChanged = true;
        }
    }
    
    /// <param name="p_newValue"> the new value gotten from the serialization </param>
    /// <param name="p_shaderPropertyId"> the shader property ID of the shader to modify </param>
    /// <param name="p_soValue"> reference to the so value </param>
    /// <param name="p_hasChanged"> reference to the has changed bool </param>
    void UpdateNewValue(Vector2 p_newValue, int p_shaderPropertyId,ref Vector2 p_soValue,ref bool p_hasChanged)
    {
        if (p_newValue != p_soValue)
        {
            p_soValue = p_newValue;
            Shader.SetGlobalVector(p_shaderPropertyId, Vector2To4(p_newValue));
            p_hasChanged = true;
        }
    }
    
    /// <param name="p_newValue"> the new value gotten from the serialization </param>
    /// <param name="p_shaderPropertyId"> the shader property ID of the shader to modify </param>
    /// <param name="p_soValue"> reference to the so value </param>
    /// <param name="p_hasChanged"> reference to the has changed bool </param>
    void UpdateNewValue(Texture2D p_newValue, int p_shaderPropertyId,ref Texture2D p_soValue,ref bool p_hasChanged)
    {
        if (p_newValue != p_soValue)
        {
            p_soValue = p_newValue;
            Shader.SetGlobalTexture(p_shaderPropertyId, p_newValue);
            p_hasChanged = true;
        }
    }
    
    /// <param name="p_newValue"> the new value gotten from the serialization </param>
    /// <param name="p_shaderPropertyId"> the shader property ID of the shader to modify </param>
    /// <param name="p_soValue"> reference to the so value </param>
    /// <param name="p_hasChanged"> reference to the has changed bool </param>
    void UpdateNewValue(Color p_newValue, int p_shaderPropertyId,ref Color p_soValue,ref bool p_hasChanged)
    {
        if (p_newValue != p_soValue)
        {
            p_soValue = p_newValue;
            Shader.SetGlobalColor(p_shaderPropertyId, p_newValue);
            p_hasChanged = true;
        }
    }

    public static void Refresh()
    {
        if (m_customLitSettings == null)
        {
            m_customLitSettings = Resources.Load("Custom Lit Settings/customLitSettings", typeof(CustomLitSettingsSO)) as CustomLitSettingsSO;
            if (m_customLitSettings == null)
            {
                AssetDatabase.CreateAsset(CreateInstance<CustomLitSettingsSO>(), "Assets/Resources/Custom Lit Settings/customLitSettings.asset");
                m_customLitSettings = Resources.Load("Custom Lit Settings/customLitSettings", typeof(CustomLitSettingsSO)) as CustomLitSettingsSO;
            }
            else return;
        }
        
        Shader.SetGlobalFloat(m_tilingShaderID,m_customLitSettings.shadowTiling);
        Shader.SetGlobalFloat(m_lightIntensityMultiplierShaderID,m_customLitSettings.lightIntensityMultiplier);
        Shader.SetGlobalFloat(m_globalLightingShaderID,m_customLitSettings.globalIllumination);
        Shader.SetGlobalFloat(m_occlusionShaderID,m_customLitSettings.defaultOcclusionValue);
        Shader.SetGlobalFloat(m_triplanarBlendSharpnessID,m_customLitSettings.triplanarBlendSharpness);
        Shader.SetGlobalVector(m_smoothstepRID,Vector2To4(m_customLitSettings.smoothStepR));
        Shader.SetGlobalVector(m_smoothstepGID,Vector2To4(m_customLitSettings.smoothStepG));
        Shader.SetGlobalVector(m_smoothstepBID,Vector2To4(m_customLitSettings.smoothStepB));
        Shader.SetGlobalTexture(m_hashTextureID,m_customLitSettings.shadowHashTexture);
    }

    /// <summary/> Converts a vector 2 to a vector4 with z and w = 0
    private static Vector4 Vector2To4(Vector2 p_vector2)
    {
        return new Vector4(p_vector2.x, p_vector2.y, 0f, 0f);
    }

    /// <returns>returns true if SO not compatible</returns>
    bool CheckSoWithShaderGlobalValues(CustomLitSettingsSO p_so)
    {
        if(p_so.shadowTiling != Shader.GetGlobalFloat(m_tilingShaderID)) return true;
        if(p_so.lightIntensityMultiplier != Shader.GetGlobalFloat(m_lightIntensityMultiplierShaderID)) return true;
        if(p_so.globalIllumination != Shader.GetGlobalFloat(m_globalLightingShaderID)) return true;
        if(p_so.defaultOcclusionValue != Shader.GetGlobalFloat(m_occlusionShaderID)) return true;
        if(p_so.triplanarBlendSharpness != Shader.GetGlobalFloat(m_triplanarBlendSharpnessID)) return true;
        
        if(Vector2To4(m_customLitSettings.smoothStepR) != Shader.GetGlobalVector(m_smoothstepRID)) return true;
        if(Vector2To4(m_customLitSettings.smoothStepG) != Shader.GetGlobalVector(m_smoothstepGID)) return true;
        if(Vector2To4(m_customLitSettings.smoothStepB) != Shader.GetGlobalVector(m_smoothstepBID)) return true;

        if(m_customLitSettings.shadowHashTexture != Shader.GetGlobalTexture(m_hashTextureID)) return true;
        
        if(m_customLitSettings.beaconColor != Shader.GetGlobalColor(m_beaconDefaultColor))return true;
        if(m_customLitSettings.beaconDetectionColor != Shader.GetGlobalColor(m_beaconDetectionColor))return true;
        
        return false;
    }
    
    private string PrintCustomSettings(CustomLitSettingsSO p_customLitSettings)
    {
        String message = "";
        message += $" Shadow Tiling :{p_customLitSettings.shadowTiling}\n";
        message += $" Light Intensity Multiplier :{p_customLitSettings.lightIntensityMultiplier}\n";
        message += $" Default Occlusion Value :{p_customLitSettings.defaultOcclusionValue}\n";
        message += $" Global Illumination :{p_customLitSettings.globalIllumination}\n";
        message += $" Triplanar Blend Sharpness :{p_customLitSettings.triplanarBlendSharpness}\n";
        message += $" Smooth Step R :{p_customLitSettings.smoothStepR}\n";
        message += $" Smooth Step G :{p_customLitSettings.smoothStepG}\n";
        message += $" Smooth Step B :{p_customLitSettings.smoothStepB}\n";
        message += $" Shadow Hash Texture :{p_customLitSettings.shadowHashTexture}\n";

        return message;
    }
}
