using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EnableAbilityOnTrigger : MonoBehaviour
{
    [SerializeField,Tooltip("the mechanic to enable on trigger")] private AbstractMechanic m_mechanic;
    [SerializeField,Tooltip("the ui element to enable on trigger")] private GameObject m_uiElement;
    [SerializeField,Tooltip("player's layer mask")] private LayerMask m_playerLayerMask = 1<<12;
    
    private void OnTriggerEnter(Collider other)
    {
        if ((m_playerLayerMask.value & (1 << other.gameObject.layer)) == 0) return;
        m_mechanic.enabled =true;
        m_uiElement.SetActive(true);
        Destroy(this);
    }
}
