using UnityEngine;
using UnityEngine.VFX;

public class HeartIdentifier : MonoBehaviour
{
    [HideInInspector] public int heartIndex = -1;
    [SerializeField] private VisualEffect m_explosion;
    [SerializeField] GameObject m_crystal;

    public void Detonate()
    {
        m_explosion.SendEvent("Explode");
        m_crystal.SetActive(false);
    }
}
