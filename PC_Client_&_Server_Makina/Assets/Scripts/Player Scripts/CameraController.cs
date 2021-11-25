using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField][Range(1f,25f)][Tooltip("the sensitivity of the camera (mesured by dpi multipliers)")] private float m_cameraSensitivity;
    
    
    [SerializeField]private Camera m_playerCamera;
    private Vector2 m_cameraRotation;

    private bool m_frozen = false; 
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        m_cameraRotation.x = transform.rotation.eulerAngles.x;
        m_cameraRotation.y = m_playerCamera.transform.rotation.eulerAngles.x;
    }

    // Update is called once per frame
    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.P))
        {
            m_frozen = !m_frozen;
            Cursor.lockState = m_frozen? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = m_frozen;
        }
        
        // m_frozen = Input.GetKeyDown(KeyCode.P) ? !m_frozen : m_frozen;
        
        if(!m_frozen){
            float mouseX = Input.GetAxisRaw("Mouse X");
            float mouseY = Input.GetAxisRaw("Mouse Y");

            m_cameraRotation += new Vector2(mouseX, -mouseY) * m_cameraSensitivity;

            m_cameraRotation.y = Mathf.Clamp(m_cameraRotation.y, -85f, 85f);

            m_playerCamera.transform.localRotation = Quaternion.Euler(m_cameraRotation.y, 0, 0);
            
            transform.rotation = Quaternion.Euler(0, m_cameraRotation.x, 0);
        }
    }
}