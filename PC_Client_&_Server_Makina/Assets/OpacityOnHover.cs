using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OpacityOnHover : MonoBehaviour
{
    [SerializeField] private TextMeshPro m_text;
    private void OnMouseEnter()
    {
        Color color = m_text.color;
        color.a = 1f;
        m_text.color = color;
    }
    private void OnMouseExit()
    {
        Color color = m_text.color;
        color.a = 0f;
        m_text.color = color;
    }
}
