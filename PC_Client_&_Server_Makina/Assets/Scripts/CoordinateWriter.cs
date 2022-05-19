using Synchronizers;
using TMPro;
using UnityEngine;

public class CoordinateWriter : MonoBehaviour {

    private string m_textBefore;
    private TextMeshProUGUI m_text;
    
    private Color m_colorDefault = Color.white;
    [SerializeField] private Color m_colorHighlight = Color.magenta;
    [SerializeField] private KeyCode m_keyToHighlight = KeyCode.H;
    [SerializeField] [Range(0, 15)] private byte m_floatLevel = 1;

    // Start is called before the first frame update
    void Start() {
        m_text = GetComponent<TextMeshProUGUI>();
        m_textBefore = m_text.text;
        m_colorDefault = m_text.color;
    }

    // Update is called once per frame
    void Update() {
        m_text.color = Input.GetKey(m_keyToHighlight) ? m_colorHighlight : m_colorDefault;

        var position = SynchronizePlayerPosition.Instance.m_player.position;
        Vector3 simplifiedPosition = new Vector3(Mathf.Round(position.x),Mathf.Round(position.y),Mathf.Round(position.z));
        m_text.text = m_textBefore + simplifiedPosition;
    }
}
