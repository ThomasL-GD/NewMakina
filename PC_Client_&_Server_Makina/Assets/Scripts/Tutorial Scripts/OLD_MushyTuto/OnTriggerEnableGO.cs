using UnityEngine;

[RequireComponent(typeof(Collider))]
public class OnTriggerEnableGO : MonoBehaviour
{
    [SerializeField,Tooltip("the GameObject to enable on trigger")] private GameObject m_goToEnable;
    [SerializeField,Tooltip("player's layer mask")] private LayerMask m_playerLayerMask = 1<<12;
    // Start is called before the first frame update
    void Awake()
    {
        m_goToEnable.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((m_playerLayerMask.value & (1 << other.gameObject.layer)) == 0) return;
        m_goToEnable.SetActive(true);
    }
}
