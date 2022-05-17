using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class DestroyAfterSeconds : MonoBehaviour {
    [SerializeField] [Range(0.05f,600f)] float m_timeBeforeDestroy = 3f;
    [SerializeField] private VisualEffect m_vfx;
    
    // Start is called before the first frame update
    void Start() => StartCoroutine(DelayedDestroy());

    IEnumerator DelayedDestroy() {
        yield return new WaitForSeconds(m_timeBeforeDestroy - 5f);
        if(m_vfx == true)m_vfx.SendEvent("Stop");
        yield return new WaitForSeconds(m_timeBeforeDestroy);
        Destroy(gameObject);
    }
}