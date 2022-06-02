       using System;
using System.Collections;
using CustomMessages;
using UnityEngine;
using UnityEngine.VFX;

public class HeartIdentifier : MonoBehaviour
{
    [HideInInspector]public int heartIndex = -1;
    [SerializeField] public VisualEffect m_anticipation;
    [SerializeField] public VisualEffect m_detonation;
    [SerializeField] public GameObject[] m_anticipationElements;
    
    private Coroutine m_anticipationCo;

    public void Awake()
    {
        ClientManager.OnReceiveInitialData += DestroyThis;
    }

    private void DestroyThis(InitialData p_id)
    {
        StartCoroutine(DestroyMe());
    }

    IEnumerator DestroyMe()
    {
        yield return null;
        yield return null;
        Destroy(gameObject);
        Debug.Log("heyyy");
    }
    
    public void StartAnticipation(float p_dt,float p_timer = 2f)
    {
        if(!gameObject.activeSelf) return;
        m_anticipationCo ??= StartCoroutine(Anticipation(p_timer));
        m_timer = p_dt;
    }

    public void StopAnticipation()
    {
        if (m_anticipationCo != null) StopCoroutine(m_anticipationCo);
        m_anticipationCo = null;
        m_anticipation.SetFloat("OutwardsForce",0f);
        m_anticipation.SetFloat("InwardsForce",0f);
        m_timer = 0f;
    }

    public void Break() {
        foreach (GameObject elem in m_anticipationElements) elem.SetActive(false); } 

    float m_timer = 0f;
    IEnumerator Anticipation(float p_timer = 2f)
    {
        bool running = true;
        while(running)
        {
            m_anticipation.SetFloat("OutwardsForce",Mathf.Min(1f, (m_timer / p_timer) * 2f));
            
            if(m_timer -p_timer > -.2f) m_anticipation.SetFloat("InwardsForce",1f);
            if(m_timer/p_timer > 1f)
            {
                m_detonation.SendEvent("Explode");
                running = false;
                SynchroniseReady.Instance.StartReady();
                StopAnticipation();
            }
            yield return null;
        }
    }
}
