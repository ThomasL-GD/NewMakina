using Synchronizers;
using UnityEngine;

public class FollowVrLookWithDelay : MonoBehaviour {

    public float m_distanceFromEyes = 50f;
    public float m_smoothTime = 1f;
    private Vector3 m_currentVelocity;


    // Update is called once per frame
    void Update() {
        transform.rotation = Quaternion.LookRotation(SynchronizeSendVrRig.Instance.m_head.position - transform.position);
        transform.position = Vector3.SmoothDamp(transform.position, SynchronizeSendVrRig.Instance.m_head.position + SynchronizeSendVrRig.Instance.m_head.forward * m_distanceFromEyes, ref m_currentVelocity, m_smoothTime);
    }
}
