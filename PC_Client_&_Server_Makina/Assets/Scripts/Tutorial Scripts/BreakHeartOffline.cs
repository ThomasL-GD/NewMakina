using System;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BreakHeartOffline : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_heartRadiusFeedback;
    [SerializeField, Tooltip("the amount of time it will take to break a heart"), Range(0, 10)]
    private int m_breakDuration = 3;

    [SerializeField,Tooltip("player's layer mask")] private LayerMask m_playerLayerMask = 1<<12;
    [SerializeField,TextArea] private String m_congratulatoryText = "Congratulations! You've completed the tutorial!";

    private float m_timer;
    private bool m_breaking;

    void Update()
    {
        if (!m_breaking) return;

        m_timer += Time.deltaTime;
        
        if(m_timer >= m_breakDuration)
        {
            ResetTimer();
            Destroy(gameObject);
            return;
        }
            
        string timer = m_timer.ToString();
        
        string text = "x,y";
        text = text.Replace('x',timer[0]);
        
        char car = timer.Length > 3 ? timer[2] : '0';
        text = text.Replace('y',car);
        
        m_heartRadiusFeedback.text = $"Breaking Heart : {text}/{m_breakDuration}";
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if ((m_playerLayerMask.value & (1 << other.gameObject.layer)) == 0) return;
        m_breaking = true;
        m_heartRadiusFeedback.enabled = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if ((m_playerLayerMask.value & (1 << other.gameObject.layer)) == 0) return;
        ResetTimer();
        m_heartRadiusFeedback.enabled = false; 
    }

    private void ResetTimer()
    {
        m_timer = 0;
        m_breaking = false;
        m_heartRadiusFeedback.text = m_congratulatoryText;
    }
}
