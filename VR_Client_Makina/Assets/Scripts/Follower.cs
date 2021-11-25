using UnityEngine;

public class Follower : MonoBehaviour {

    [SerializeField] private Transform m_transformToFollow = null;
    [SerializeField] private Vector3 m_offset = Vector3.zero;
    
    // Start is called before the first frame update
    void Start()
    {
        if(m_transformToFollow == null)Debug.LogError("You suck at this, gimme the controller", this);
    }

    // Update is called once per frame
    void Update() {
        transform.position = m_transformToFollow.position + m_offset;
    }
}
