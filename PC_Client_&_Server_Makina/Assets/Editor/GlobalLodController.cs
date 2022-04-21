using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static UnityEditor.EditorGUILayout;
using static UnityEngine.GUILayout;

public class GlobalLodController : EditorWindow
{
    [MenuItem("Tools/Global LOD Controller")]
    static void Init()
    {
        // Instantiating or fetching the PrefabPicasso window 
        GlobalLodController window = GetWindow(typeof(GlobalLodController)) as GlobalLodController;
        
        // Giving the window the "Prefab Picasso" name
        window.titleContent = new GUIContent("Global LOD Controller");
        
        // Display the window that has been created
        window.Show();
        
        // Bring the window to the front
        window.Focus();
        
        // Updating the GUI of the window
        window.Repaint();
    }

    private static int m_lodValueToModify = 0;
    private static float m_valueToSetItToo = 12f;
    
    private void OnGUI()
    {
        m_lodValueToModify = IntField("LOD level to modify",m_lodValueToModify);
        m_valueToSetItToo = Slider("Value to set",m_valueToSetItToo,0f,100f);

        
        if (Button("Set Values !")) SetValue();
    }

    private void SetValue()
    {
        List<string> prefabs = AssetDatabase.FindAssets ("t:Prefab", null).ToList();
        Debug.Log(prefabs.Count);
        
        foreach (string adress in prefabs)
        {
            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(adress));
            LODGroup[] lodgs = go.GetComponentsInChildren<LODGroup>(true);

            foreach (LODGroup lodg in lodgs)
            {
                LOD[] lods = lodg.GetLODs();
                lods[m_lodValueToModify].screenRelativeTransitionHeight = m_valueToSetItToo / 100f;
                lodg.SetLODs(lods);
                Debug.Log(go.name, go);
            }
        }
    }
}

// for (int i = 0; i < prefabs.Count; i++)
// {
//     GameObject go = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(prefabs[i]), typeof(GameObject)) as GameObject;
//     
//     if(go.CompareTag("Do not modify LOD with tool"))
//     {
//         prefabs.RemoveAt(i);
//         i = 0;
//         continue;
//     }
//     
//     if (go.TryGetComponent(out LODGroup lodg)) continue;
//
//     LODGroup[] lodgs = go.GetComponentsInChildren<LODGroup>();
//
//     if (lodgs.Length != 0) continue;
//     
//     prefabs.RemoveAt(i);
//     i = 0;
// }

// for (int i = 0; i < prefabs.Count-1; i++)
// {
//     for (int j = i+1; j < prefabs.Count; j++)
//     {
//         if(prefabs[i] == prefabs[j])
//         {
//             i = 0;
//             prefabs.RemoveAt(j);
//             break;
//         }
//     }
// }