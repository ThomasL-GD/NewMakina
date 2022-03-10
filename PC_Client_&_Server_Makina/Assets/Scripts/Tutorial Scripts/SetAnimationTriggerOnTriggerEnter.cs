using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SetAnimationTriggerOnTriggerEnter : MonoBehaviour
{
    [SerializeField] private Animator m_animator;
    [SerializeField] private string m_triggerName;
    [SerializeField,Tooltip("player's layer mask")] private LayerMask m_playerLayerMask = 1<<12;

    private void OnTriggerEnter(Collider other)
    {
        if ((m_playerLayerMask.value & (1 << other.gameObject.layer)) == 0) return;
        m_animator.SetTrigger(m_triggerName);
        Destroy(this);
    }
}
