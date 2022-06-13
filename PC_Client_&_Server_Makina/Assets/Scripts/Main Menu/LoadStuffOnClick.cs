using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LoadStuffOnClick : MonoBehaviour
{
    [SerializeField] private GameObject[] m_toEnable;
    [SerializeField] private GameObject[] m_toDisable;
    [SerializeField] private GameObject m_camera;
    [SerializeField] private float m_fadeTime = 2f;
    [SerializeField] private RawImage m_blackScreen;

    [SerializeField] private AnimationCurve m_curveIn;
    [SerializeField] private AnimationCurve m_curveOut;

    [SerializeField] private bool m_practice;

    private bool clicked = false;    
    
    // Update is called once per frame
    void OnMouseDown()
    {
        if (clicked) return;
        StartCoroutine(FadeToBlack());
    }

    IEnumerator FadeToBlack()
    {
        clicked = true;
        
        ServerManager.singleton.m_practice = m_practice;
        
        Color color = m_blackScreen.color;
        float timer = 0f;
        do
        {
            timer += Time.deltaTime / m_fadeTime;
            timer = Mathf.Min(1f, timer);

            color.a = m_curveIn.Evaluate(timer);
            
            m_blackScreen.color = color;
            yield return null;
        } while (color.a != 1f) ;
        
        foreach (GameObject go in m_toEnable) go.SetActive(true);
        
        m_camera.SetActive(false);
        
        do
        {
            timer -= Time.deltaTime / m_fadeTime;
            timer = Mathf.Max(0f, timer);

            color.a = m_curveOut.Evaluate(timer);
            
            m_blackScreen.color = color;
            yield return null;
        } while (color.a != 0f);
        
        foreach (GameObject go in m_toDisable) go.SetActive(false);
        
        clicked = false;
    }
}
