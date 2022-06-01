using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeToBlack : MonoBehaviour
{
    [SerializeField] private RawImage m_blackSceen;

    [SerializeField] private AnimationCurve m_fadeCurve;
    
    public delegate void FadeToBalckDelegator(Transform p_player, Vector3 p_position);
    public static FadeToBalckDelegator FadeToBlackNow;
    
    // Start is called before the first frame update
    void Start()
    {
        Color color = m_blackSceen.color;
        color.a = 0f;
        m_blackSceen.color = color;
    }

    private void Awake() => FadeToBlackNow += FadeAndTp;
    void FadeAndTp(Transform p_player, Vector3 p_position) => StartCoroutine(FadeInAndOut(p_player, p_position));
    
    IEnumerator FadeInAndOut(Transform p_player, Vector3 p_position)
    {
        Color color = m_blackSceen.color;
        float timer = 0f;
        while (m_blackSceen.color.a != 1)
        {
            
            color.a = Mathf.Min(m_fadeCurve.Evaluate(timer += Time.deltaTime), 1f);
            m_blackSceen.color = color;
            yield return null;
        }

        p_player.position = p_position;
        
        timer = 1f;
        while (m_blackSceen.color.a != 0)
        {
            color.a = Mathf.Max(m_fadeCurve.Evaluate(timer -= Time.deltaTime), 0f);
            m_blackSceen.color = color;
            
            yield return null;
        }
    }
}
