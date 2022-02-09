using System.Collections;
using UnityEngine;

public class MoveUp : MonoBehaviour
{
    private bool m_raise;
    private float m_raiseSpeed;
    private float m_raiseTime;

    void Update()
    {
        if (!m_raise) return;
        
        transform.position += Vector3.up * (m_raiseSpeed * Time.deltaTime);
    }

    public void StartRaise(Vector3 p_startPosition)
    {
        transform.position = p_startPosition;
        StartCoroutine(Raise());
    }

    IEnumerator Raise()
    {
        m_raise = true;
        yield return new WaitForSeconds(m_raiseTime);
        
        m_raise = false;
        transform.position = Vector3.down * 1000f;
    }

    public void SetRaiseSpeedAndTime(float p_raiseSpeed, float p_raiseTime)
    {
        m_raiseSpeed = p_raiseSpeed;
        m_raiseTime = p_raiseTime;
    }
}
