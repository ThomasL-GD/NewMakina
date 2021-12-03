using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class InflateToSize : MonoBehaviour
{
    [SerializeField][Tooltip("the inflation time in seconds")] private float m_inflationTime = .5f;

    private float m_inflationSpeed;
    private float m_targetScale;
    
    void Awake()
    {
        m_targetScale = transform.localScale.x;

        transform.localScale = Vector3.zero;
        m_inflationSpeed =  m_targetScale/m_inflationTime;
        Debug.Log(Time.time);
    }

    void Update()
    {
        float inflation = m_inflationSpeed * Time.deltaTime;
        transform.localScale += new Vector3(1f, 1f, 1f) * inflation;
        if (transform.localScale.x >= m_targetScale)
        {
            transform.localScale = new Vector3(1f, 1f, 1f) * m_targetScale;
            Debug.Log(Time.time);
            Destroy(this);
        }
    }
}
