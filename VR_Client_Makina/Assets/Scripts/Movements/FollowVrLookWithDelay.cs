using System;
using Synchronizers;
using UnityEngine;

public class FollowVrLookWithDelay : MonoBehaviour {

    [Flags] private enum Axis {
        X = 0b0000_0001,
        Y = 0b0000_0010,
        Z = 0b0000_0100
    }
    
    public float m_distanceFromEyes = 50f;
    public float m_smoothTime = 1f;
    private Vector3 m_currentVelocity;
    [SerializeField] private Axis m_positionAxis = Axis.X | Axis.Y | Axis.Z;
    [SerializeField] private Axis m_rotationAxis = Axis.X | Axis.Y | Axis.Z;

    // Update is called once per frame
    void Update() {
        Quaternion rot = Quaternion.LookRotation(SynchronizeSendVrRig.Instance.m_head.position - transform.position);
        Transform transform1;
        (transform1 = transform).rotation = Quaternion.Euler(m_rotationAxis.HasFlag(Axis.X) ? rot.eulerAngles.x : transform.rotation.eulerAngles.x, m_rotationAxis.HasFlag(Axis.Y) ? rot.eulerAngles.y : transform.rotation.eulerAngles.y, m_rotationAxis.HasFlag(Axis.Z) ? rot.eulerAngles.z : transform.rotation.eulerAngles.z);
        
        Vector3 pos = Vector3.SmoothDamp(transform1.position, SynchronizeSendVrRig.Instance.m_head.position + SynchronizeSendVrRig.Instance.m_head.forward * m_distanceFromEyes, ref m_currentVelocity, m_smoothTime);
        transform.position = new Vector3(m_positionAxis.HasFlag(Axis.X) ? pos.x : transform.position.x, m_positionAxis.HasFlag(Axis.Y) ? pos.y : transform.position.y, m_positionAxis.HasFlag(Axis.Z) ? pos.z : transform.position.z);
    }
}
