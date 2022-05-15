using System.Collections;
using UnityEngine;

public class DisableAfterTime : MonoBehaviour
{
    [SerializeField] private float m_waitTime = 5f;
    // Start is called before the first frame update
    private void OnEnable()=>StartCoroutine(WaitThenDisable());
    

    IEnumerator WaitThenDisable()
    {
        yield return new WaitForSeconds(m_waitTime);
        gameObject.SetActive(false);
    }
}
