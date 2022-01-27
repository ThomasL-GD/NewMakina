using CustomMessages;
using Mirror;
using Player_Scripts.Reloading;
using TMPro;
using UnityEngine;

public class BlindOnInput : MonoBehaviour
{
    [SerializeField] private KeyCode m_key;
    [SerializeField] private float m_forwardOffset = 1.2f;
    [SerializeField] private TextMeshProUGUI m_uiElement;
    [SerializeField] private TextMeshProUGUI m_vrBlindUiElement;

    [SerializeField] private ReloadingAbstract m_coolDownScript;
    
    private bool m_canFlash = true;

    private void Awake()
    {
        m_coolDownScript.OnReloading += ResetCooldown;
        ClientManager.OnReceiveDeActivateBlind += SayVRIsBlind;
        m_uiElement.text = "Press A to flash";
        m_vrBlindUiElement.enabled = false;
    }
    
    private void ResetCooldown()
    {
        m_canFlash = true;
        m_uiElement.text = "Press A to flash";
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
            m_uiElement.text = " ";

            m_vrBlindUiElement.enabled = true;
        }
    }
}
