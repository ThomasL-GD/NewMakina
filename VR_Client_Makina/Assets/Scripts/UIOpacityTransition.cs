using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIOpacityTransition : MonoBehaviour
{
    [SerializeField] private RawImage m_rawImage;
    [SerializeField] private float m_transitionTime;

    private bool m_transitioning;
    // Update is called once per frame
    void Update()
    {
        if(!m_transitioning) return;
        Color color = m_rawImage.color;
        color.a -= (1 / m_transitionTime) * Time.deltaTime;
        if (color.a < 0)
        {
            color.a = 0;
            m_transitioning = false;
        }
        m_rawImage.color = color;
    }

    public IEnumerator WaitFortransition(float p_waitTime)
    {
        Color color = m_rawImage.color;
        color.a = 1;
        m_rawImage.color = color;
        
        yield return new WaitForSeconds(p_waitTime);
        m_transitioning = true;
    }
}
