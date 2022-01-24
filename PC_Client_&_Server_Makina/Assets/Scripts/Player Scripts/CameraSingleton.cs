using UnityEngine;

public class CameraSingleton : MonoBehaviour
{
    [SerializeField] private Camera m_camera;
    public static Camera camera;

    private void Awake()
    {
        camera = m_camera;
    }
}
