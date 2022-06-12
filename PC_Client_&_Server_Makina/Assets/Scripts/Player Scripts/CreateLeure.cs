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
    [SerializeField] private float m_speed = 10f;
    
    [SerializeField] private ReloadingAbstract[] m_coolDownScripts;
    
    [SerializeField] private bool m_isTutorial = false;
    
    [SerializeField] [Tooltip("The sound played when the invisibility starts")] private AudioSource m_invisibilityBeginSound;
    [SerializeField] [Tooltip("The sound played when the invisibility is active")] private AudioSource m_invisibilityLastsSound;
    [SerializeField] [Tooltip("The sound played when the invisibility ends")] private AudioSource m_invisibilityEndSound;
    
    private bool m_canSpawnLeure = true;

    private GameObject m_leure;
    private Coroutine m_killLeureCoroutine;
    private Coroutine m_soundCoroutine;

    private void Awake()
    {
        foreach (var cd in m_coolDownScripts)cd.OnReloading += ResetCooldown;
        

        ClientManager.OnReceiveDestroyLeure += DestroyLeure;
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
            
            m_soundCoroutine = StartCoroutine(PlaySoundThatLastsWhenBeginIsOver());
        }
    }
    
    IEnumerator KillLeure()
    {
        NetworkClient.Send(new SpawnLeure());
        NetworkClient.Send(new PcInvisibility{isInvisible = true});
        
        UIManager.Instance.SendLeure();
        
        yield return new WaitForSeconds(m_leureLifeTime);
        UIManager.Instance.StartLeureCooldown();

        Debug.Log("Decoy natural death");
        DestroyLeure();
    }

    private void DestroyLeure(DestroyLeure p_message)
    {
        Debug.Log("Decoy network death");
        DestroyLeure(false);
    }
    
    private void DestroyLeure(bool p_mustSendNetworkMessage = true)
    {
        StopCoroutine(m_killLeureCoroutine);
        StopCoroutine(m_soundCoroutine); // We stop the sound in case the decoy was destroyed before the first sound is done playing
        
        m_invisibilityLastsSound.Stop();
        m_invisibilityEndSound.Play();
        
        Destroy(m_leure);
        Debug.Log("Decoy death");
        if(p_mustSendNetworkMessage)NetworkClient.Send(new DestroyLeure());
        NetworkClient.Send(new PcInvisibility{isInvisible = false});

        foreach (var cd in m_coolDownScripts)cd.StartReloading();
    }
    
    void ResetCooldown()
    {
        m_canSpawnLeure = true;
        UIManager.Instance.ResetLeureCooldown();
    }

    /// <summary>Will play the m_invisibilityLastsSound once m_invisibilityBeginSound is done playing (plays both in the function)</summary>
    /// <remarks>I know it is anti-modular but there's no similar uses of this type of things in the script</remarks>
    IEnumerator PlaySoundThatLastsWhenBeginIsOver() {
        m_invisibilityBeginSound.Play();

        yield return new WaitForSeconds(m_invisibilityBeginSound.clip.length);

        m_invisibilityLastsSound.loop = true;
        m_invisibilityLastsSound.Play();
    }
}
