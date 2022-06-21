using System;
using CustomMessages;
using Mirror;
using Synchronizers;
using TMPro;
using UnityEngine;

public class SetReadyInArea : MonoBehaviour
{
    [SerializeField] private float m_range = 14f;
    [SerializeField] private float m_maxHeartTime = 2f;
    private float m_timer;
    
    [SerializeField] private TextMeshProUGUI m_heartRadiusFeedback;
    [SerializeField] private HeartVFXHandler m_vfxHandler;

    [SerializeField] private GameObject[] m_healthElements;

    private bool quick = false;
    
    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(transform.position, SynchronizePlayerPosition.Instance.m_player.position) <= m_range)
        {
            quick = true;
            m_timer += Time.deltaTime;

            m_heartRadiusFeedback.enabled = true;
            string timer = m_timer.ToString();
            string text = "x,y";
            text = text.Replace('x',timer[0]);
            char car = timer.Length > 3 ? timer[2] : '0';
            text = text.Replace('y', car);
            

            m_heartRadiusFeedback.text = $"Setting Ready : {text}/{m_maxHeartTime}";
            m_vfxHandler.StartAnticipation(m_maxHeartTime);
            if (m_timer > m_maxHeartTime)
            {
                NetworkClient.Send(new ReadyToGoIntoTheBowl());
                m_heartRadiusFeedback.enabled = false;
                foreach (var healthElements in m_healthElements) healthElements.SetActive(true);
                return;
            }
            return;
        }

        if (!quick) return;
        quick = false;
        m_vfxHandler.StopAnticipation();

        m_heartRadiusFeedback.enabled = false;
        m_heartRadiusFeedback.text = "";
        
        m_timer = 0f;
    }
}
