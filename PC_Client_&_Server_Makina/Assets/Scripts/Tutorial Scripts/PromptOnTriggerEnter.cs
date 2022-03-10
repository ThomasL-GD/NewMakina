using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PromptOnTriggerEnter : MonoBehaviour
{
    [TextArea,SerializeField,Tooltip("the message to show the player on prompt")] private String m_message;
    [SerializeField,Tooltip("the key the player has to press to end he prompt")] private KeyCode m_keyCode;
    [SerializeField,Tooltip("player's layer mask")] private LayerMask m_playerLayerMask = 1<<12;

    // Update is called once per frame
    private void OnTriggerEnter(Collider other)
    {
        if ((m_playerLayerMask.value & (1 << other.gameObject.layer)) == 0) return;
        
        TutorialPrompt.OnPrompt?.Invoke(m_message, m_keyCode == KeyCode.None ? null : (KeyCode?)m_keyCode);
        Destroy(this);
    }
}
