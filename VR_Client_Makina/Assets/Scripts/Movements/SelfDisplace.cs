using UnityEngine;

public class SelfDisplace : MonoBehaviour {

    [SerializeField] [Range(0.1f, 1000f)] private float m_maxDisplacement = 1f;
    
    void OnEnable() {
        Vector3 position = transform.position;
        position = new Vector3(position.x + Random.Range(-m_maxDisplacement, m_maxDisplacement), position.y, position.y + Random.Range(-m_maxDisplacement, m_maxDisplacement));
        transform.position = position;
    }
}
