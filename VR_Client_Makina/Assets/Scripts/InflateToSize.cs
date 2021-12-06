using UnityEngine;

public class InflateToSize : MonoBehaviour
{
    [HideInInspector] public float m_inflationTime = .5f;
    private float m_inflationSpeed = 1f;
    [HideInInspector] public float m_targetScale = 1f;
    private bool m_isInflating = false;

    void Update() {
        if (!m_isInflating) return;
        
        float inflation = m_inflationSpeed * Time.deltaTime;
        transform.localScale += Vector3.one * inflation;
        if (transform.localScale.x >= m_targetScale)
        {
            transform.localScale = Vector3.one * m_targetScale;
            Destroy(this);
        }
    }

    public void StartInflating() {
        transform.localScale = Vector3.zero;
        
        m_inflationSpeed =  m_targetScale/m_inflationTime;

        m_isInflating = true;
    }
}