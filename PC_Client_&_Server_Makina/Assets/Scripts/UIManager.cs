using System;
using System.Collections;
using CustomMessages;
using Mirror;
using Player_Scripts.Reloading;
using Synchronizers;
using TMPro;
using UnityEngine;

public class UIManager : Synchronizer<UIManager> {
    
    [SerializeField] private UIElementWithReload m_leureElement;
    [SerializeField] private UIElementWithReload m_tpElement;

    [SerializeField] private HealthContainer m_vrHealth;
    [SerializeField] private HealthContainer m_pcHealth;
    [SerializeField] private Transform m_healthParent;
    
    [Serializable]
    private struct UIElementWithReload {
        public Animator animator;
        public TextMeshProUGUI timer;
        public ReloadingAbstract[] coolDownScripts;
        [HideInInspector] public int timerStartValue;
    }

    [Serializable]
    private struct HealthContainer
    {
        public RectTransform parent;
        public Vector2 parentMargins;
        public Vector2 childMargins;
        public Vector2 offset;
        public GameObject healthElementPrefab;
        [Range(0f, 10f)] public float timeBeforeDisappearing;
        [HideInInspector] public RectTransform[] healthElements;
    }

    // Start is called before the first frame update
    void Awake()
    {
        // LEURE
        foreach (ReloadingAbstract cd in m_leureElement.coolDownScripts)
            if (cd is TimedReload reload) {
                m_leureElement.timerStartValue = Mathf.CeilToInt(reload.m_cooldownTime);
                m_leureElement.animator.SetFloat(reloadTimePropertyId, 1 / reload.m_cooldownTime);
            }
        
        // TELEPORT
        foreach (var cd in m_tpElement.coolDownScripts)
            if (cd is TimedReload reload) {
                m_tpElement.timerStartValue = Mathf.CeilToInt(reload.m_cooldownTime);
                m_tpElement.animator.SetFloat(reloadTimePropertyId, 1 / reload.m_cooldownTime);
            }

        ClientManager.OnReceiveInitialData += ReceiveInitialData;
        
        ClientManager.OnReceiveLaser += SynchroniseHealthPC;
        ClientManager.OnReceiveHeartBreak += SynchroniseHealthVR;
    }
    
    #region Abilities

    // LEURE ___________________________________________________________________________________________________________

    private static readonly int reloadTimePropertyId = Animator.StringToHash("Reload Time");
    private static readonly int startLeureCooldownPropertyId = Animator.StringToHash("UseOfClone");
    private static readonly int resetPropertyId = Animator.StringToHash("Reset");
    
    private Coroutine m_leureTimerCo;   
    
    public void StartLeureCooldown() {
        m_leureElement.animator.SetTrigger(startLeureCooldownPropertyId);
        if(m_leureTimerCo!=null) StopCoroutine(m_leureTimerCo);
        m_leureTimerCo = StartCoroutine(LeureCooldownCounter());
    }
    private IEnumerator LeureCooldownCounter()
    {
        int counter = 0;
        int startValue = m_leureElement.timerStartValue;
        m_leureElement.timer.text = startValue.ToString();
        while (counter != startValue)
        {
            yield return new WaitForSeconds(1f);
            ++counter;
            m_leureElement.timer.text = (startValue - counter).ToString();
        }
    }
    
    public void ResetLeureCooldown()
    {
        m_leureElement.animator.SetTrigger(resetPropertyId);
        if(m_leureTimerCo != null)StopCoroutine(m_leureTimerCo);
    }
    
    // TELEPORT ________________________________________________________________________________________________________
    
    private static readonly int placedTeleporterPropertyId = Animator.StringToHash("Placed Teleporter");
    private static readonly int teleportedPropertyId = Animator.StringToHash("Teleported");
    
    private Coroutine m_tpTimerCo;   
    
    public void PlacedTeleporter() {
        m_tpElement.animator.ResetTrigger(teleportedPropertyId);
        m_tpElement.animator.SetTrigger(placedTeleporterPropertyId);
    }
    
    public void TeleportRollback() {
        m_tpElement.animator.ResetTrigger(placedTeleporterPropertyId);
        m_tpElement.animator.SetTrigger(teleportedPropertyId);
        
        //if(m_tpTimerCo!=null) StopCoroutine(m_leureTimerCo);
        m_tpTimerCo = StartCoroutine(TpCooldownCounter());
    }

    private IEnumerator TpCooldownCounter()
    {
        int counter = 0;
        int startValue = m_tpElement.timerStartValue;
        m_tpElement.timer.text = startValue.ToString();
        while (counter != startValue)
        {
            yield return new WaitForSeconds(1f);
            ++counter;
            m_tpElement.timer.text = (startValue - counter).ToString();
        }
    }
    
    public void ResetTeleportRollbackCooldown()
    {
        m_tpElement.animator.SetTrigger(resetPropertyId);
        if(m_tpTimerCo != null)StopCoroutine(m_tpTimerCo);
    }
    #endregion

    #region Health
    private void ReceiveInitialData(InitialData p_message)
    {
        foreach (RectTransform t in m_vrHealth.healthElements) Destroy(t);
        foreach (RectTransform t in m_pcHealth.healthElements) Destroy(t);

        m_vrHealthIncrementor = 1;
        m_pcHealthIncrementor = 1;
        
        int healthAmount = p_message.healthVrPlayer;
        
        m_vrHealth.healthElements = new RectTransform[healthAmount];
        
        float ao = (Screen.width * 9) / (Screen.height * 16);
        
        // VR HEALTH
        
        float divider = (m_vrHealth.parent.anchorMax.x - m_vrHealth.parent.anchorMin.x - m_vrHealth.parentMargins.x * 2f) / (healthAmount * ao);
        
        for (int i = 0; i < m_vrHealth.healthElements.Length; i++) {
            RectTransform healthElement = Instantiate(m_vrHealth.healthElementPrefab, m_vrHealth.parent).GetComponent<RectTransform>();
            float left = m_vrHealth.parentMargins.x + (.5f - m_vrHealth.parentMargins.x) * (ao - 1)/2;
            Vector2 anchormin = new Vector2(left + i * divider - m_vrHealth.childMargins.x / ao + m_vrHealth.offset.x / ao, -m_vrHealth.childMargins.y + m_vrHealth.offset.y);
            Vector2 anchormax = new Vector2(anchormin.x + divider + 2*m_vrHealth.childMargins.x/ao + m_vrHealth.offset.x/ao, m_vrHealth.childMargins.y+ m_vrHealth.offset.y);

            healthElement.anchorMin = anchormin;
            healthElement.anchorMax = anchormax;

            healthElement.parent = m_healthParent;
            
            m_vrHealth.healthElements[i] = healthElement;
        }
        
        // PC Health
        healthAmount = p_message.healthPcPlayer;
        
        divider = (m_pcHealth.parent.anchorMax.x - m_pcHealth.parent.anchorMin.x - m_pcHealth.parentMargins.x * 2f) / (healthAmount * ao);
        m_pcHealth.healthElements = new RectTransform[healthAmount];
        
        for (int i = 0; i < m_pcHealth.healthElements.Length; i++) {
            RectTransform healthElement = Instantiate(m_pcHealth.healthElementPrefab, m_pcHealth.parent).GetComponent<RectTransform>();
            float left = m_pcHealth.parentMargins.x + (.5f - m_pcHealth.parentMargins.x) * (ao - 1)/2;
            Vector2 anchormin = new Vector2(left + i * divider - m_pcHealth.childMargins.x / ao + m_pcHealth.offset.x / ao, -m_pcHealth.childMargins.y + m_pcHealth.offset.y);
            Vector2 anchormax = new Vector2(anchormin.x + divider + 2*m_pcHealth.childMargins.x/ao + m_pcHealth.offset.x/ao, m_pcHealth.childMargins.y+ m_pcHealth.offset.y);

            healthElement.anchorMin = anchormin;
            healthElement.anchorMax = anchormax;

            healthElement.parent = m_healthParent;
            
            m_pcHealth.healthElements[i] = healthElement;
        }
    }

    private static readonly int m_shatterAnimatorTrigger = Animator.StringToHash("Shatter");
    private int m_vrHealthIncrementor = 1;
    private void SynchroniseHealthVR(HeartBreak p_heartbreak)
    {
        int index = m_vrHealth.healthElements.Length - m_vrHealthIncrementor++;
        m_vrHealth.healthElements[index].GetComponent<Animator>().SetTrigger(m_shatterAnimatorTrigger);
        StartCoroutine(DepopAfterDelay(m_vrHealth.healthElements[index].transform, m_vrHealth.timeBeforeDisappearing));
    }
    
    private int m_pcHealthIncrementor = 1;
    private static readonly int m_heartAnimatorTrigger = Animator.StringToHash("Consume Heart");

    private void SynchroniseHealthPC(Laser p_laser)
    {
        if(!p_laser.hit) return;

        int index = m_pcHealth.healthElements.Length - m_pcHealthIncrementor++;
        m_pcHealth.healthElements[index].GetComponent<Animator>().SetTrigger(m_heartAnimatorTrigger);
    }

    /// <summary>Will set unactive an object after a defined time </summary>
    /// <param name="p_object">The object you want to set unactive</param>
    /// <param name="p_time">The time after which you want it set unactive</param>
    IEnumerator DepopAfterDelay(Transform p_object, float p_time) {
        yield return new WaitForSeconds(p_time);
        p_object.gameObject.SetActive(false);
    }

    #endregion
}
