using UnityEngine;

public class InflateToSize : MonoBehaviour {
    
    [HideInInspector] public float m_inflationTime = .5f;
    private float m_inflationSpeed = 1f;
    [HideInInspector] public float m_targetScale = 1f;
    private bool m_isInflating = false;
    private float m_initialScale = 0f;

    void Update() {
        if (!m_isInflating) return;
        //Debug.Log($"Scale : {transform.localScale}", this);
        
        float inflation = m_inflationSpeed * Time.deltaTime;
        transform.localScale += Vector3.one * inflation;
        if (transform.localScale.x >= m_targetScale) { //When we reached desired range
            
            transform.localScale = Vector3.one * m_targetScale;
            m_isInflating = false;
            //this.enabled = false;
        }
    }

    /// <summary>
    /// Call this to make this object start inflating.
    /// Keep in mind this script will kill itself once its work is done.
    /// </summary>
    public void StartInflating() {
        transform.localScale = Vector3.one * m_initialScale;
        
        m_inflationSpeed =  m_targetScale/m_inflationTime;

        m_isInflating = true;
    }
}