using System;
using System.Collections;
using CustomMessages;
using Mirror;
using Player_Scripts.Reloading;
using Synchronizers;
using UnityEngine;

public class CreateLeure : AbstractMechanic
{
    [SerializeField] private KeyCode m_leureKey = KeyCode.E;
    [SerializeField] private GameObject m_leurePrefab;
    [SerializeField] private float m_leureGravity;
    [SerializeField] private float m_leureForwardOffset = .5f;
    [SerializeField] private float m_leureLifeTime = 5f;
    [SerializeField] private float m_speed = 10f;
    
    [SerializeField] private ReloadingAbstract[] m_coolDownScripts;
    
    [SerializeField] private bool m_isTutorial = false;
    
    private bool m_canSpawnLeure = true;

    public static Action a_onLeureSpawn;

    private GameObject m_leure;
    private Coroutine m_killLeureCoroutine;

    private void Awake() {
        foreach (var cd in m_coolDownScripts) {
            cd.OnReloading += ResetCooldown;
        }
        ClientManager.OnReceiveDestroyLeure += DestroyLeure;

        ClientManager.OnReceiveReadyToGoIntoTheBowl += ResetStuff;
    }
    
    void ResetStuff(ReadyToGoIntoTheBowl p_mess)
    {
        DestroyLeure();
        ResetCooldown();
    }

    // Update is called once per frame
    void Update()
    {
        if(!SynchronizeInitialData.vrConnected && !m_isTutorial) return;
        if (m_canSpawnLeure && Input.GetKeyDown(m_leureKey))
        {
            m_canSpawnLeure = false;
            m_leure = Instantiate(m_leurePrefab, transform.position + (transform.forward * m_leureForwardOffset),transform.rotation);
            m_leure.GetComponent<LeurreMovement>().SetSpeedAndGravity(m_speed,m_leureGravity);
            m_killLeureCoroutine = StartCoroutine(KillLeure());
            a_onLeureSpawn?.Invoke();
        }
    }
    
    IEnumerator KillLeure()
    {
        NetworkClient.Send(new SpawnLeure());
        NetworkClient.Send(new PcInvisibility{isInvisible = true});
        
        UIManager.Instance.SendLeure();
        
        yield return new WaitForSeconds(m_leureLifeTime);

        DestroyLeure();
    }

    private void DestroyLeure(DestroyLeure p_message)
    {
        DestroyLeure(false);
    }
    
    private void DestroyLeure(bool p_mustSendNetworkMessage = true)
    {
        if(m_killLeureCoroutine != null)StopCoroutine(m_killLeureCoroutine);
        
        UIManager.Instance.StartLeureCooldown();
        
        Destroy(m_leure);
        Debug.Log("Decoy death");
        if(p_mustSendNetworkMessage)NetworkClient.Send(new DestroyLeure());
        NetworkClient.Send(new PcInvisibility{isInvisible = false});

        foreach (var cd in m_coolDownScripts)cd.StartReloading();
    }
    
    void ResetCooldown() {
        if(!m_canSpawnLeure)SoundManager.Instance.ReloadSound();
        
        m_canSpawnLeure = true;
        UIManager.Instance.ResetLeureCooldown();
    }
}
