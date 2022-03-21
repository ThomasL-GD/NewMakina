
using static UnityEditor.EditorGUILayout;
using UnityEditor;
using UnityEngine;
using ObjectField = UnityEditor.UIElements.ObjectField;

public class CustomLitSettings : EditorWindow
{
    private static readonly int m_tilingShaderValue = Shader.PropertyToID("_Tiling");
    private static readonly int m_lightIntensityMultiplierShaderValue = Shader.PropertyToID("_Light_Intensity_multiplier");
    private static readonly int m_globalLightingShaderValue = Shader.PropertyToID("_Global_Lighting");
    private static readonly int m_hashTextureShaderValue = Shader.PropertyToID("_Hash");
    private static readonly int m_occlusionShadervalue = Shader.PropertyToID("_Occlusion");

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
        float shadowTiling = FloatField("Shadow Tiling", EditorPrefs.GetFloat("Shadow Tiling Editor Pref"));
        Shader.SetGlobalFloat(m_tilingShaderValue,shadowTiling);
        EditorPrefs.SetFloat("Shadow Tiling Editor Pref", shadowTiling);

        float lightIntensityMultiplier = FloatField("Light Intensity Multiplier", EditorPrefs.GetFloat("Light Intensity Multiplier"));
        Shader.SetGlobalFloat(m_lightIntensityMultiplierShaderValue,lightIntensityMultiplier);
        EditorPrefs.SetFloat("Light Intensity Multiplier", lightIntensityMultiplier);

        float defaultOcclusionValue = Slider("Ambient Occlusion", EditorPrefs.GetFloat("Ambient Occlusion"),0f,1f);
        Shader.SetGlobalFloat(m_occlusionShadervalue,defaultOcclusionValue);
        EditorPrefs.SetFloat("Ambient Occlusion", defaultOcclusionValue);
        
        float globalIllumination = Slider("Global Illumination", EditorPrefs.GetFloat("Global Illumination"), 0f, 1f);
        Shader.SetGlobalFloat(m_globalLightingShaderValue,globalIllumination);
        EditorPrefs.SetFloat("Global Illumination", globalIllumination);

        if (Shader.GetGlobalTexture(m_hashTextureShaderValue) == null) 
            Shader.SetGlobalTexture(m_hashTextureShaderValue,Resources.Load("Hash V3", typeof(Texture2D)) as Texture2D);
        
        Shader.SetGlobalTexture(m_hashTextureShaderValue, (Texture2D)ObjectField("Shadow Hash", Shader.GetGlobalTexture(m_hashTextureShaderValue),typeof(Texture2D)));
    }
}
