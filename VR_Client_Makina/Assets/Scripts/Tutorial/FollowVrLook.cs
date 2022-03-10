using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowVrLook : MonoBehaviour {

    [SerializeField] private Transform m_thingToFollow;
    //[SerializeField] private bool m_stayOnScreenAtAllCosts = false;

    // Update is called once per frame
    void Update() {
        transform.LookAt(m_thingToFollow);
    }
}