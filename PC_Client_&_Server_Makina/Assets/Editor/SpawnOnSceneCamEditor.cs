using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SpawnOnSceneCam))]
public class SpawnOnSceneCamEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        if(GUILayout.Button("Teleport to scene view!"))
        {
            SpawnOnSceneCam script = target as SpawnOnSceneCam;
            script.TeleportToSceneViewCam();
        }
    }
}