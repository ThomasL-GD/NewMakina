using System.Collections;
using CustomMessages;
using Mirror;
using Player_Scripts.Reloading;
using Synchronizers;
using UnityEngine;
using UnityEngine.UI;

public class CreateLeure : AbstractMechanic
{
    [SerializeField] private KeyCode m_leureKey = KeyCode.E;
    [SerializeField] private GameObject m_leurePrefab;
    [SerializeField] private float m_leureGravity;
    [SerializeField] private float m_leureForwardOffset = .5f;
    [SerializeField] private float m_leureLifeTime = 5f;
    [SerializeField] private RawImage m_uiElement;
    [SerializeField] private float m_speed = 10f;
    
    [SerializeField] private ReloadingAbstract m_coolDownScript;
    
    private bool m_canSpawnLeure = true;

    private GameObject m_leure;
    private Coroutine m_killLeureCoroutine;

    private void Awake()
    {
        m_coolDownScript.OnReloading += ResetCooldown;
        ClientManager.OnReceiveDestroyLeure += DestroyLeure;
    }

    // Update is called once per frame
    void Update()
    {
        if(!SynchronizeInitialData.vrConnected) return;
        if (m_canSpawnLeure && Input.GetKeyDown(m_leureKey))
        {
            m_canSpawnLeure = false;
            m_leure = Instantiate(m_leurePrefab, transform.position + (transform.forward * m_leureForwardOffset),transform.rotation);
            
            m_leure.GetComponent<LeurreMovement>().SetSpeedAndGravity(m_speed,m_leureGravity);
            m_killLeureCoroutine = StartCoroutine(KillLeure());
        }
    }
    
    IEnumerator KillLeure()
    {
        NetworkClient.Send(new SpawnLeure());
        NetworkClient.Send(new PcInvisibility{isInvisible = true});
        m_uiElement.enabled = false;
        yield return new WaitForSeconds(m_leureLifeTime);

        DestroyLeure();
    }

    private void DestroyLeure(DestroyLeure p_message)
    {
        DestroyLeure();
    }
    private void DestroyLeure()
    {
        StopCoroutine(m_killLeureCoroutine);
        
        Destroy(m_leure);
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
