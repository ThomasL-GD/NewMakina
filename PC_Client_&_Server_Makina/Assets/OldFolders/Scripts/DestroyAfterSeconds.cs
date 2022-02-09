using System.Collections;
using UnityEngine;

public class DestroyAfterSeconds : MonoBehaviour {
    [SerializeField] float m_timeBeforeDestroy = 3f;
    
    // Start is called before the first frame update
    void Start() => StartCoroutine(DelayedDestroy());

    IEnumerator DelayedDestroy()
    {
        yield return new WaitForSeconds(m_timeBeforeDestroy);
        Destroy(gameObject);
    }
}