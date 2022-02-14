#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
public class SpawnOnSceneCam : MonoBehaviour
{
    [SerializeField] private bool m_spawnOnSceneCameraPosition = false;

    // Start is called before the first frame update
    void Start()
    {
        SceneView.duringSceneGui += GetSceneViewPosition;
    }

    public void TeleportToSceneViewCam()
    {
        SceneView.duringSceneGui += GetSceneViewPosition;
    }
    
    private void GetSceneViewPosition(SceneView window)
    {
        Undo.RecordObject(transform, "Teleport Player to Scene View");
        SceneView.duringSceneGui -= GetSceneViewPosition;
        transform.position = window.camera.transform.position;
    }
}
#endif