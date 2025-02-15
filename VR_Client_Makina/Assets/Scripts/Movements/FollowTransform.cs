using System;
using UnityEngine;

public class FollowTransform : MonoBehaviour {

    [SerializeField, HideInInspector] private Transform m_transformToFollow = null;
    
    [SerializeField, HideInInspector] private bool m_positions = false;
    [SerializeField, HideInInspector] private bool m_rotations = false;
    [SerializeField, HideInInspector] private bool m_scale = false;
    
    private Vector3 positionDifference = Vector3.zero;
    private Quaternion rotationDifference = Quaternion.Euler(Vector3.zero);
    private Vector3 scaleDifference = Vector3.zero;
    
    //Positions
    [SerializeField, HideInInspector] private bool m_pX = false;
    [SerializeField, HideInInspector] private bool m_pY = false;
    [SerializeField, HideInInspector] private bool m_pZ = false;
    
    //Rotations
    [SerializeField, HideInInspector] private bool m_rX = false;
    [SerializeField, HideInInspector] private bool m_rY = false;
    [SerializeField, HideInInspector] private bool m_rZ = false;
    
    //scale
    [SerializeField, HideInInspector] private bool m_sX = false;
    [SerializeField, HideInInspector] private bool m_sY = false;
    [SerializeField, HideInInspector] private bool m_sZ = false;

    private void OnEnable() {
        Transform transform1 = transform;
        positionDifference = transform1.position - m_transformToFollow.position;
        rotationDifference = transform1.rotation * Quaternion.Inverse(m_transformToFollow.rotation);
        scaleDifference = transform1.localScale - m_transformToFollow.localScale;
    }


    private void Update() {

        if(m_positions) {

            if (m_pX && m_pY && m_pZ) {
                transform.position = m_transformToFollow.position;
            }
            else {
                Vector3 newPosition = transform.position;
                if (m_pX) newPosition.x = m_transformToFollow.position.x;
                if (m_pY) newPosition.y = m_transformToFollow.position.y;
                if (m_pZ) newPosition.z = m_transformToFollow.position.z;
                transform.position = newPosition + positionDifference;
            }
        }

        if(m_rotations) {

            if (m_rX && m_rY && m_rZ) {
                transform.rotation = m_transformToFollow.rotation;
            }
            else {
                Vector3 newEulerRotation = transform.rotation.eulerAngles;
                if (m_rX) newEulerRotation.x = m_transformToFollow.rotation.eulerAngles.x;
                if (m_rY) newEulerRotation.y = m_transformToFollow.rotation.eulerAngles.y;
                if (m_rZ) newEulerRotation.z = m_transformToFollow.rotation.eulerAngles.z;
                transform.rotation = Quaternion.Euler(newEulerRotation);
            }
        }

        if(m_scale) {

            if (m_sX && m_sY && m_sZ) {
                transform.localScale = m_transformToFollow.localScale;
            }
            else {
                Vector3 newLocalScale = transform.localScale;
                if (m_sX) newLocalScale.x = m_transformToFollow.localScale.x;
                if (m_sY) newLocalScale.y = m_transformToFollow.localScale.y;
                if (m_sZ) newLocalScale.z = m_transformToFollow.localScale.z;
                transform.localScale = newLocalScale + scaleDifference;
            }
        }
    }
}
