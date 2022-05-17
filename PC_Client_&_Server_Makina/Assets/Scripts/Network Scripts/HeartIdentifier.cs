using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class HeartIdentifier : MonoBehaviour
{
    [HideInInspector]public int heartIndex = -1;
    [SerializeField] public VisualEffect m_anticipation;
    [SerializeField] public VisualEffect m_detonation;
    
    private Coroutine m_anticipationCo;
    
    // Start is called before the first frame update
    public void StartAnticipation(float p_timer = 2f)
    {
        if(!gameObject.activeSelf) return;
        m_anticipationCo ??= StartCoroutine(Anticipation(p_timer));
    }

    public void StopAnticipation()
    {
        if (m_anticipationCo != null) StopCoroutine(m_anticipationCo);
        m_anticipationCo = null;
        m_anticipation.SetFloat("OutwardsForce",0f);
        m_anticipation.SetFloat("InwardsForce",0f);
    }
    
    IEnumerator Anticipation(float p_timer = 2f)
    {
        float timer = 0f;
        bool running = true;
        while(running)
        {
            m_anticipation.SetFloat("OutwardsForce",Mathf.Min(1f, (timer / p_timer) * 2f));
            timer += Time.deltaTime;
            
            if(timer -p_timer > -.2f) m_anticipation.SetFloat("InwardsForce",1f);
            if(timer/p_timer > 1f)
            {
                m_detonation.SendEvent("Explode");
                running = false;
                SynchroniseReady.Instance.StartReady();
                gameObject.SetActive(false);
                StopAnticipation();
            }
            yield return null;
        }
    }
}
