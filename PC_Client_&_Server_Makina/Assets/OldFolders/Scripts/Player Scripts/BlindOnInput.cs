using System;
using CustomMessages;
using Mirror;
using Player_Scripts.Reloading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BlindOnInput : MonoBehaviour
{
    [SerializeField] private KeyCode m_key;
    [SerializeField] private float m_forwardOffset = 1.2f;
    [SerializeField] private RawImage m_uiElement;
    [SerializeField] private TextMeshProUGUI m_vrBlindUiElement;

    [SerializeField] private ReloadingAbstract m_coolDownScript;
    
    private bool m_canFlash = true;

    private void Awake()
    {
        m_coolDownScript.OnReloading += ResetCooldown;
        ClientManager.OnReceiveDeActivateBlind += SayVRIsBlind;
    }

    private void Start()
    {
        m_uiElement.enabled = true;
        m_vrBlindUiElement.enabled = false;
    }

    private void ResetCooldown()
    {
        m_canFlash = true;
        m_uiElement.enabled = true;
    }

    private void SayVRIsBlind(DeActivateBlind p_deactivateblind)
    {
        m_vrBlindUiElement.enabled = false;
    }
    
    // Update is called once per frame
    void Update()
    {
        if(m_canFlash && Input.GetKeyDown(m_key))
        {
            Vector3 flairStartPosition = transform.position + transform.forward * m_forwardOffset;
    
            NetworkClient.Send(new ActivateFlair() {startPosition = flairStartPosition});
            m_canFlash = false;
            m_coolDownScript.StartReloading();
            m_uiElement.enabled = false;

            m_vrBlindUiElement.enabled = true;
        }
    }
}
