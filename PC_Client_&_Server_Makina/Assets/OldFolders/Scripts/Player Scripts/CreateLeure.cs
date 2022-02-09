using System;
using System.Collections;
using System.Collections.Generic;
using CustomMessages;
using Mirror;
using Player_Scripts.Reloading;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class CreateLeure : MonoBehaviour
{
    [SerializeField] private KeyCode m_leureKey = KeyCode.E;
    [SerializeField] private GameObject m_leurePrefab;
    [SerializeField] private float m_leureGravity;
    [SerializeField] private float m_leureForwardOffset = .5f;
    [SerializeField] private float m_leureLifeTime = 5f;
    [SerializeField] private RawImage m_uiElement;
    
    [SerializeField] private ReloadingAbstract m_coolDownScript;
    
    private bool m_canSpawnLeure = true;

    private void Awake()
    {
        m_coolDownScript.OnReloading += ResetCooldown;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_canSpawnLeure && Input.GetKeyDown(m_leureKey))
        {
            m_canSpawnLeure = false;
            GameObject leure = Instantiate(m_leurePrefab, transform.position + (transform.forward * m_leureForwardOffset),transform.rotation);
            
            leure.GetComponent<LeurreMovement>().SetSpeedAndGravity(10f,m_leureGravity);
            StartCoroutine(KillLeure(leure));
        }
    }
    
    IEnumerator KillLeure(GameObject p_leure)
    {
        
        NetworkClient.Send(new SpawnLeure());
        NetworkClient.Send(new PcInvisibility{isInvisible = true});
        m_uiElement.enabled = false;
        yield return new WaitForSeconds(m_leureLifeTime);
        Destroy(p_leure);
        
        NetworkClient.Send(new DestroyLeure());
        NetworkClient.Send(new PcInvisibility{isInvisible = false});
        
        m_coolDownScript.StartReloading();
    }

    void ResetCooldown()
    {
        m_canSpawnLeure = true;
        m_uiElement.enabled = true;
    }
}
