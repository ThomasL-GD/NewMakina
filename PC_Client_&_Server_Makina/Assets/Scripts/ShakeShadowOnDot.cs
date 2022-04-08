using UnityEngine;

public class ShakeShadowOnDot : MonoBehaviour
{
    [SerializeField] private Transform m_playerTr;
    [SerializeField, Range(0f,1f)] private float m_minDotProduct = .75f;
    //[SerializeField] private float m_shakeSpeed = 1000f;
    
    [SerializeField] [Tooltip("The sound played when the VR head look at the PC guy")] private AudioSource m_panikSound;

    private static readonly int RotationSpeed = Shader.PropertyToID("_RotationSpeed");

    // Start is called before the first frame update
    void Start() => Shader.SetGlobalFloat(RotationSpeed, 0f);

    // Update is called once per frame
    void Update()
    {
        if(Vector3.Dot(transform.forward, (m_playerTr.position - transform.position).normalized) > m_minDotProduct) {
            //Shader.SetGlobalFloat(RotationSpeed, m_shakeSpeed);
            
            if(!m_panikSound.isPlaying) m_panikSound.Play();
        }else {
            //Shader.SetGlobalFloat(RotationSpeed, 0f);
            
            if(m_panikSound.isPlaying) m_panikSound.Stop();
        }
    }
}
