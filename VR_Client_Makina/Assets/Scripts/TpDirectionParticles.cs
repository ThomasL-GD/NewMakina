using UnityEngine;

public class TpDirectionParticles : MonoBehaviour {
    
    private float m_speed;

    private void Start() {
        m_speed = SynchronizeTpRollback.Instance.m_particlesSpeed;
    }

    private void Update() {
        transform.Translate(Vector3.forward * Time.deltaTime * m_speed);
    }
}
