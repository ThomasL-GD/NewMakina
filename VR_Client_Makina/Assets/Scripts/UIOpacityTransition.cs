using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIOpacityTransition : MonoBehaviour
{
    [SerializeField] private RawImage m_rawImage;
    [SerializeField] private TextMeshProUGUI m_text;
    [SerializeField] private float m_transitionTime;

    private bool m_transitioning;
    // Update is called once per frame
    void Update()
    {
        if(!m_transitioning) return;
        Color color = m_rawImage.color;
        float aplha = color.a;
        aplha -= (1 / m_transitionTime) * Time.deltaTime;
        if (aplha < 0)
        {
            aplha = 0;
            m_transitioning = false;
        }

        color.a = aplha;
        m_rawImage.color = color;

        color = m_text.color;
        color.a = aplha;

        m_text.color = color;
    }

    public IEnumerator WaitFortransition(float p_waitTime)
    {
        Color color = m_rawImage.color;
        color.a = 1;
        m_rawImage.color = color;

        color = m_text.color;
        color.a = 1;
        m_text.color = color;
        
        
        yield return new WaitForSeconds(p_waitTime);
        m_transitioning = true;
    }
}
