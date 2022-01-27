using Player_Scripts.Reloading;
using TMPro;
using UnityEngine;

public class TeleportToChosenPosition : MonoBehaviour
{
    [SerializeField] private KeyCode m_placeOrTeleportKey;
    [SerializeField] private GameObject m_teleportLocationPrefab;
    [SerializeField] private TextMeshProUGUI m_uiElement;

    private GameObject m_teleportLocation;
    private bool m_placed;
    private bool m_canUse = true;

    [SerializeField] private ReloadingAbstract m_coolDownScript;
    
    private void Awake() => m_coolDownScript.OnReloading += Reset;
    
    private void Reset()
    {
        Destroy(m_teleportLocation);
        m_placed = false;
        m_uiElement.text = "Press R to drop a teleport point";
    }

    // Update is called once per frame
    void Update()
    {
        if (m_canUse && Input.GetKeyDown(m_placeOrTeleportKey))
        {
            if (m_placed)
            {
                transform.position = m_teleportLocation.transform.position;
                m_canUse = false;
                m_coolDownScript.StartReloading();
                return;
            }

            m_teleportLocation = Instantiate(m_teleportLocationPrefab, transform.position,transform.rotation);
            m_uiElement.text = "Press R to teleport to placed point";
            m_placed = true;
        }
    }
}
