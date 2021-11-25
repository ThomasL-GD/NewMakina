using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimerOnTrigger : MonoBehaviour {

    [SerializeField] private TextMeshProUGUI m_text = null;
    private static bool m_isRunning = false;
    private float m_time;
    
    
    void Update()
    {
        if (m_isRunning) {
            m_time += Time.deltaTime;
            m_text.text = $"{m_time}";
        }
    }

    private void OnTriggerEnter(Collider p_other) {
        if (m_isRunning) {
            m_isRunning = false;
        }
        else if (!m_isRunning) {
            m_isRunning = true;
            m_time = 0f;
        }
    }
}
