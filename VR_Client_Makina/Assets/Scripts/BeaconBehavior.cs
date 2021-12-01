using System;
using System.Collections;
using System.Collections.Generic;
using CustomMessages;
using Network;
using Synchronizers;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class BeaconBehavior : GrabbableObject {

    [HideInInspector] public int m_index;
    public SynchronizeBeacons m_synchronizer;

    protected override void Start() {
        base.Start();
        //m_isPuttableOnlyOnce = true;
    }

    /// <summary>
    /// We override ActualiseParent to let the SynchronizeBeacons know when beacons are grabbed or not
    /// </summary>
    public override void ActualiseParent() {
        base.ActualiseParent();

        if(!MyNetworkManager.singleton.m_canSend) return;
        
        if (m_isCaught) m_synchronizer.BeaconGrabbed(m_index);
        else if (!m_isCaught) m_synchronizer.BeaconLetGo(m_index);
    }
    
    // private void OnDrawGizmos() {
    //     Gizmos.DrawWireSphere(transform.position, m_radius); // feedback magic ♪♪ ヽ(ˇ∀ˇ )ゞ
    // }
}
