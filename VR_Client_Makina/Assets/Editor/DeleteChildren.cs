using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeleteChildren : MonoBehaviour
{/// <summary/> Selects the parent of all the actively selected HoudinAllRight classed objects
    [MenuItem("Tools/Delete Children &d")]
    public static void ParentSelect()
    {
        List<GameObject> goToDestroy = new List<GameObject>();
        List<Scene> scenesToSetDirty = new List<Scene>();
        
        for (int i = 0; i < Selection.objects.Length; i++)
        {
            GameObject go = ((GameObject) Selection.objects[i]);

            if(go == null)
            {
                Debug.Log("nope");
                continue;
            }
            
            Transform got = go.transform;
            
            for (int j = 0; j < got.childCount; j++)
                goToDestroy.Add(got.GetChild(j).gameObject);
            Scene currentScene = go.scene;
            bool toContinue = false;
            foreach (var scene in scenesToSetDirty)
                if(scene == currentScene) {
                    toContinue = true;
                    break;
                }
            
            if(toContinue) continue;
            
            scenesToSetDirty.Add(currentScene);
        }

        for (int i = goToDestroy.Count-1; i !=-1 ; i--) DestroyImmediate(goToDestroy[i]);
        
        foreach (var scene in scenesToSetDirty) EditorSceneManager.MarkSceneDirty(scene);
        
    }
}
