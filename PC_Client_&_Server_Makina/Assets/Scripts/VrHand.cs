using UnityEngine;

public class VrHand : MonoBehaviour {

    [HideInInspector] public Animator m_animator;
    
    // Start is called before the first frame update
    void Start() {
        m_animator = GetComponent<Animator>();
    }
    
}
