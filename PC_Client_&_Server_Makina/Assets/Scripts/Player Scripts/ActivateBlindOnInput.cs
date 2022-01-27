using System;
using System.Collections;
using System.Collections.Generic;
using CustomMessages;
using Mirror;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class ActivateBlindOnInput : MonoBehaviour
{
    [SerializeField] private KeyCode m_key;
    [SerializeField] private float m_forwardOffset = 1.2f;
    [SerializeField] private TextMeshProUGUI m_uiElement;
    [SerializeField] private TextMeshProUGUI m_vrBlindUiElement;

    private bool m_alreadyActive;

    private void Awake()
    {
        ClientManager.OnReceiveDeActivateBlind += SetBoolToFalse;
        m_uiElement.text = "Press A to flash";
        m_vrBlindUiElement.enabled = false;
    }

    private void SetBoolToFalse(DeActivateBlind p_message)
    {
        m_alreadyActive = false;
        m_uiElement.text = "Press A to flash";
        m_vrBlindUiElement.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(!m_alreadyActive && Input.GetKeyDown(m_key))
        {
            Vector3 flairStartPosition = transform.position + transform.forward * m_forwardOffset;
    
            NetworkClient.Send(new ActivateFlair() {startPosition = flairStartPosition});
            m_alreadyActive = true;
            m_uiElement.text = " ";

            m_vrBlindUiElement.enabled = true;
        }
    }
}
