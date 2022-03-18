
using static UnityEditor.EditorGUILayout;
using UnityEditor;
using UnityEngine;
using ObjectField = UnityEditor.UIElements.ObjectField;

public class CustomLitSettings : EditorWindow
{
    private static readonly int Tiling = Shader.PropertyToID("_Tiling");
    private static readonly int LightIntensityMultiplier = Shader.PropertyToID("_Light_Intensity_multiplier");
    private static readonly int GlobalLighting = Shader.PropertyToID("_Global_Lighting");
    private static readonly int Hash = Shader.PropertyToID("_Hash");

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
        Shader.SetGlobalFloat(Tiling,FloatField("Shadow Tiling",Shader.GetGlobalFloat(Tiling)));
        Shader.SetGlobalFloat(LightIntensityMultiplier,FloatField("Light Intensity Multiplier",Shader.GetGlobalFloat(LightIntensityMultiplier)));
        Shader.SetGlobalFloat(GlobalLighting,Slider("Global Illumination",Shader.GetGlobalFloat(GlobalLighting),0f,1f));
        Shader.SetGlobalTexture(Hash, (Texture2D)ObjectField("Shadow Hash", Shader.GetGlobalTexture(Hash),typeof(Texture2D)));
    }
}
