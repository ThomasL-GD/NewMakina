using static UnityEditor.EditorGUILayout;
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
        
        Texture2D newTexture2D = ObjectField("Shadow Hash", m_customLitSettings.shadowHashTexture, typeof(Texture2D)) as Texture2D;
        UpdateNewValue(newTexture2D, m_hashTextureID,ref m_customLitSettings.shadowHashTexture, ref hasChanged);

        if (hasChanged)
        {
            EditorUtility.SetDirty(m_customLitSettings);
            AssetDatabase.SaveAssets();
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
            Shader.SetGlobalVector(p_shaderPropertyId, new Vector4(p_newValue.x,p_newValue.y,0,0));
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
}
