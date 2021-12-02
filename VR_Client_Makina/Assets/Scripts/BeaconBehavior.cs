using Network;
using Synchronizers;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class BeaconBehavior : GrabbablePhysickedObject {

    [HideInInspector] public int m_index;
    public SynchronizeBeacons m_synchronizer;

    private BeaconLoading m_beaconLoading = null;

    protected override void Start() {
        base.Start();
        //m_isPuttableOnlyOnce = true;

        if (gameObject.TryGetComponent(out BeaconLoading script)) {
            m_beaconLoading = script;
        }
        else {
            Debug.LogWarning("No BeaconLoading script attached on this object but don't worry i gotchu fam ( ͡° ͜ʖ ͡°)", this);
            m_beaconLoading = gameObject.AddComponent<BeaconLoading>();
        }
    }

    /// <summary>
    /// We override ActualiseParent to let the SynchronizeBeacons know when beacons are grabbed or not
    /// </summary>
    public override void ActualiseParent() {
        base.ActualiseParent();

        if(!MyNetworkManager.singleton.m_canSend) return;
        
        if (m_isCaught) {
            m_synchronizer.BeaconGrabbed(m_index);
            m_beaconLoading.Unloading();
        }
        else if (!m_isCaught) m_synchronizer.BeaconLetGo(m_index);
    }
    
    // private void OnDrawGizmos() {
    //     Gizmos.DrawWireSphere(transform.position, m_radius); // feedback magic ♪♪ ヽ(ˇ∀ˇ )ゞ
    // }

    private void OnCollisionEnter(Collision p_other) {
        if (p_other.gameObject.layer == 8) {
            transform.rotation = Quaternion.Euler(0,0,0);
            //TODO : m_rb.
        }
    }
}
