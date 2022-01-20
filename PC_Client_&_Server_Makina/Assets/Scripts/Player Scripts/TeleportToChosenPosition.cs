using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportToChosenPosition : MonoBehaviour
{
    [SerializeField] private KeyCode m_placeOrTeleportKey;
    [SerializeField] private GameObject m_teleportLocationPrefab;

    private GameObject m_teleportLocation;
    private bool m_placed;
    
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(m_placeOrTeleportKey))
        {
            if (m_placed)
            {
                transform.position = m_teleportLocation.transform.position;
                Destroy(m_teleportLocation);
                m_placed = false;
                return;
            }

            m_teleportLocation = Instantiate(m_teleportLocationPrefab, transform.position,transform.rotation);
            m_placed = true;
        }
    }
}
