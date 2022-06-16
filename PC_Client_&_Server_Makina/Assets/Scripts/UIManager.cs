using System;
using System.Collections;
using CustomMessages;
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

    private int m_minimumLobyIndex = 0;
    
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
        public Vector2 healthAdditive;
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
    private static readonly int startLeureReloadPropertyId = Animator.StringToHash("StartReload");
    private static readonly int resetPropertyId = Animator.StringToHash("Reset");
    
    private Coroutine m_leureTimerCo;   
    public void SendLeure() {
        m_leureElement.animator.SetTrigger(startLeureCooldownPropertyId);
    }
    
    public void StartLeureCooldown() {
        if(m_leureTimerCo!=null) StopCoroutine(m_leureTimerCo);
        m_leureTimerCo = StartCoroutine(LeureCooldownCounter());
    }
    private IEnumerator LeureCooldownCounter()
    {
        m_leureElement.animator.SetTrigger(startLeureReloadPropertyId);
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

#if UNITY_EDITOR
    [ContextMenu("test")]
    void test()
    {
        ReceiveInitialData(new InitialData() { healthVrPlayer = 5, healthPcPlayer = 5 });
    }
    
#endif
    
    #region Health
    private void ReceiveInitialData(InitialData p_message)
    {
        m_minimumLobyIndex = p_message.firstLobbyHeartIndex;
        
        foreach (RectTransform t in m_vrHealth.healthElements) Destroy(t);
        foreach (RectTransform t in m_pcHealth.healthElements) Destroy(t);

        m_vrHealthIncrementor = 1;
        m_pcHealthIncrementor = 1;
        
        int healthAmount = p_message.healthVrPlayer;
        
        m_vrHealth.healthElements = new RectTransform[healthAmount];
        
        // VR HEALTH
        
        float divider = (m_vrHealth.parent.anchorMax.x - m_vrHealth.parent.anchorMin.x - m_vrHealth.parentMargins.x * 2f) / healthAmount;
        
        for (int i = 0; i < m_vrHealth.healthElements.Length; i++) {

            RectTransform healthElement = Instantiate(m_vrHealth.healthElementPrefab, m_healthParent).GetComponent<RectTransform>();

            Vector2 parentMin = m_vrHealth.parent.anchorMin;
            Vector2 parentMax = m_vrHealth.parent.anchorMax;

            float minX = m_vrHealth.parentMargins.x + parentMin.x + i * divider + m_vrHealth.offset.x - m_vrHealth.healthAdditive.x;
            float minY = parentMin.y + m_vrHealth.offset.y - m_vrHealth.healthAdditive.y;
            healthElement.anchorMin = new Vector2(minX, minY);

            float maxX = minX + divider + 2f * m_vrHealth.healthAdditive.x;
            float maxY = parentMax.y + m_vrHealth.healthAdditive.y + m_vrHealth.offset.y;
            healthElement.anchorMax = new Vector2(maxX, maxY);
            
            m_vrHealth.healthElements[i] = healthElement;
        }
        
        // PC Health
        
        healthAmount = p_message.healthPcPlayer;
        
        divider = (m_pcHealth.parent.anchorMax.x - m_pcHealth.parent.anchorMin.x - m_pcHealth.parentMargins.x * 2f) / healthAmount;
        m_pcHealth.healthElements = new RectTransform[healthAmount];
        
        for (int i = 0; i < m_pcHealth.healthElements.Length; i++) {

            RectTransform healthElement = Instantiate(m_pcHealth.healthElementPrefab, m_healthParent).GetComponent<RectTransform>();

            Vector2 parentMin = m_pcHealth.parent.anchorMin;
            Vector2 parentMax = m_pcHealth.parent.anchorMax;

            float minX = m_pcHealth.parentMargins.x + parentMin.x + i * divider + m_pcHealth.offset.x - m_pcHealth.healthAdditive.x;
            float minY = parentMin.y + m_pcHealth.offset.y - m_pcHealth.healthAdditive.y;
            healthElement.anchorMin = new Vector2(minX, minY);

            float maxX = minX + divider + 2f * m_pcHealth.healthAdditive.x;
            float maxY = parentMax.y + m_pcHealth.healthAdditive.y + m_pcHealth.offset.y;
            healthElement.anchorMax = new Vector2(maxX, maxY);
            
            m_pcHealth.healthElements[i] = healthElement;
        }
    }

    private static readonly int m_shatterAnimatorTrigger = Animator.StringToHash("Shatter");
    private int m_vrHealthIncrementor = 1;
    private void SynchroniseHealthVR(HeartBreak p_heartbreak)
    {
        if(p_heartbreak.index >= m_minimumLobyIndex) return;
        
        int index = m_vrHealth.healthElements.Length - m_vrHealthIncrementor++;
        m_vrHealth.healthElements[index].GetComponent<Animator>().SetTrigger(m_shatterAnimatorTrigger);
        StartCoroutine(DepopAfterDelay(m_vrHealth.healthElements[index].transform, m_vrHealth.timeBeforeDisappearing));
    }
    
    private int m_pcHealthIncrementor = 1;
    private static readonly int m_heartAnimatorTrigger = Animator.StringToHash("Consume Heart");

    private void SynchroniseHealthPC(Laser p_laser)
    {
        if (m_pcHealth.healthElements[0] == null) return;
        if (!m_pcHealth.healthElements[0].gameObject.activeSelf) return;
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
