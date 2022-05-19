using Synchronizers;
using TMPro;
using UnityEngine;

public class CoordinateWriter : MonoBehaviour {

    private string m_textBefore;
    private TextMeshProUGUI m_text;
    
    private Color m_colorDefault = Color.white;
    [SerializeField] private Color m_colorHighlight = Color.magenta;
    [SerializeField] private KeyCode m_keyToHighlight = KeyCode.H;

    // Start is called before the first frame update
    void Start() {
        m_text = GetComponent<TextMeshProUGUI>();
        m_textBefore = m_text.text;
        m_colorDefault = m_text.color;
    }

    // Update is called once per frame
    void Update() {
        m_text.color = Input.GetKeyDown(m_keyToHighlight) ? m_colorHighlight : m_colorDefault;

        m_text.text = m_textBefore + SynchronizePlayerPosition.Instance.m_player.position;
    }
}
