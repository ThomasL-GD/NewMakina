using System;
using System.Collections;
using System.Collections.Generic;
using Synchronizers;
using UnityEngine;

public class SetReadyOnIput : MonoBehaviour
{
    [SerializeField] private GameObject m_uiElement;
    [SerializeField] private KeyCode m_key = KeyCode.F;
    [SerializeField] private float m_range = 4f;

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(transform.position, SynchronizePlayerPosition.Instance.m_player.position) <= m_range)
        {
            m_uiElement.SetActive(true);
            if (Input.GetKeyDown(m_key)) SynchroniseReady.Instance.StartReady();
            return;
        }
        m_uiElement.SetActive(false);
    }
    
    private void OnDisable() => m_uiElement.SetActive(false);
}
