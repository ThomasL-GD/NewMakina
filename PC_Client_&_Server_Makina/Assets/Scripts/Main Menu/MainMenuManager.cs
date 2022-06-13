using System.Collections;
using System.Collections.Generic;
using Synchronizers;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject m_buttons;
    [SerializeField] private GameObject m_waiting;

    // Update is called once per frame
    void Update()
    {
        if (!SynchronizeInitialData.vrConnected) return;
        m_buttons.SetActive(true);
        m_waiting.SetActive(false);
        
        Destroy(this);
    }
}
