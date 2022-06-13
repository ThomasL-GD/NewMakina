using UnityEngine;

public class RecenterVrRig : MonoBehaviour {

    [SerializeField] private Transform m_anchor = null;
    [SerializeField] private bool m_recenterOnStart = true;
    [SerializeField] private bool m_recenterOnTransition = true;
    [HideInInspector] public Vector3 m_startPos = Vector3.zero;

    private void OnEnable() {
        m_startPos = transform.position;
        SubscribeToRecenter();
    }

    private void Start() {
        if (!m_recenterOnStart) return;
            Recenter();
    }

    // Update is called once per frame
    void Update() {
        if (OVRInput.GetDown(OVRInput.Button.Two) && m_anchor != null) {
            Recenter();
        }
    }

    private void Recenter() {
        Transform transform1 = transform;
        Vector3 position = transform1.position;
        position -= m_anchor.position;
        position += m_startPos;
        transform1.position = position;
    }

    private void SubscribeToRecenter() {
        if(m_recenterOnTransition) {
            Transition.a_transitionDone += Recenter;
            Transition.a_transitionDone += SubscribeToRecenter;
            Transition.a_openToSubscriptions += SubscribeToRecenter;
        }
    }
}
