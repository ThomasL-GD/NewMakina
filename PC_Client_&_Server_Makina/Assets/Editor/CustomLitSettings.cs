using static UnityEditor.EditorGUILayout;
using UnityEditor;
using UnityEngine;

public class CustomLitSettings : EditorWindow
{
    private static readonly int m_tilingShaderValue = Shader.PropertyToID("_Tiling");
    private static readonly int m_lightIntensityMultiplierShaderValue = Shader.PropertyToID("_Light_Intensity_multiplier");
    private static readonly int m_globalLightingShaderValue = Shader.PropertyToID("_Global_Lighting");
    private static readonly int m_hashTextureShaderValue = Shader.PropertyToID("_Hash");
    private static readonly int m_occlusionShadervalue = Shader.PropertyToID("_Occlusion");
    private static CustomLitSettingsSO m_customLitSettings;
    
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
        
        m_customLitSettings.shadowTiling = FloatField("Shadow Tiling", Mathf.Max(m_customLitSettings.shadowTiling,0f));
        Shader.SetGlobalFloat(m_tilingShaderValue,m_customLitSettings.shadowTiling);

        m_customLitSettings.lightIntensityMultiplier = FloatField("Light Intensity Multiplier", Mathf.Max(m_customLitSettings.lightIntensityMultiplier,0f));
        Shader.SetGlobalFloat(m_lightIntensityMultiplierShaderValue,m_customLitSettings.lightIntensityMultiplier);

        m_customLitSettings.defaultOcclusionValue = Slider("Ambient Occlusion", m_customLitSettings.defaultOcclusionValue,0f,1f);
        Shader.SetGlobalFloat(m_occlusionShadervalue,m_customLitSettings.defaultOcclusionValue);

        m_customLitSettings.globalIllumination = Slider("Minimum Illumination Value", m_customLitSettings.globalIllumination, 0f, 1f);
        Shader.SetGlobalFloat(m_globalLightingShaderValue,m_customLitSettings.globalIllumination);

        m_customLitSettings.triplanarBlendSharpness = FloatField("TriplanarBlendSharpness", Mathf.Max(m_customLitSettings.triplanarBlendSharpness, 0f));
        Shader.SetGlobalFloat("_TriplanarBlendSharpness", m_customLitSettings.triplanarBlendSharpness);
        
        m_customLitSettings.smoothStepR = Vector2Field("Smoothstep R",m_customLitSettings.smoothStepR);
        Shader.SetGlobalVector("_Smoothstep_R", m_customLitSettings.smoothStepR);

        m_customLitSettings.smoothStepG = Vector2Field("Smoothstep G",m_customLitSettings.smoothStepG);
        Shader.SetGlobalVector("_Smoothstep_G", m_customLitSettings.smoothStepG);

        m_customLitSettings.smoothStepB = Vector2Field("Smoothstep B",m_customLitSettings.smoothStepB);
        Shader.SetGlobalVector("_Smoothstep_B", new Vector4(m_customLitSettings.smoothStepB.x,m_customLitSettings.smoothStepB.y,0,0));
        
        m_customLitSettings.shadowHashTexture = (Texture2D) ObjectField("Shadow Hash",
            Shader.GetGlobalTexture(m_hashTextureShaderValue), typeof(Texture2D));
        Shader.SetGlobalTexture(m_hashTextureShaderValue, m_customLitSettings.shadowHashTexture);
    }
}
