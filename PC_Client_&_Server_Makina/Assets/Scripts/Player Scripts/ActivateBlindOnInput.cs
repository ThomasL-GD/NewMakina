using System.Collections;
using System.Collections.Generic;
using CustomMessages;
using Mirror;
using Unity.Mathematics;
using UnityEngine;

public class ActivateBlindOnInput : MonoBehaviour
{
    [SerializeField] private KeyCode m_key;
    [SerializeField, Min(0f)] private float m_blindingTime = 3f;

    private bool m_alreadyActive;
    
    // Update is called once per frame
    void Update()
    {
        if(!m_alreadyActive && Input.GetKeyDown(m_key))
        {
            NetworkClient.Send(new ActivateBlind());
            StartCoroutine(BlindTimer());
        }
    }

    IEnumerator BlindTimer()
    {
        m_alreadyActive = true;
        yield return new WaitForSeconds(m_blindingTime);
        NetworkClient.Send(new DeActivateBlind());
        m_alreadyActive = false;
    }
}
