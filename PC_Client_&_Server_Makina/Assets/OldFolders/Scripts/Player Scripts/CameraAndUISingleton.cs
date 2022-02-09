using TMPro;
using UnityEngine;

public class CameraAndUISingleton : MonoBehaviour
{
    [SerializeField] private Camera m_camera;
    [SerializeField] private GameObject m_elevatorButtonFeedback;
    public static Camera camera;
    public static GameObject elevatorButtonFeedback;

    private void Awake()
    {
        camera = m_camera;
        elevatorButtonFeedback = m_elevatorButtonFeedback;
    }
}
