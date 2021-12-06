using Network;
using Synchronizers;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class BeaconBehavior : GrabbablePhysickedObject {

    [HideInInspector] public int m_index;
    [HideInInspector] public SynchronizeBeacons m_synchronizer;
    
    private BeaconLoading m_beaconLoading = null;

    private bool m_isDeployed = false;
    
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

        BeaconBehavior.OnDestroyBeacon += ActualiseIndex;
    }

    public delegate void DestroyBeaconDelegator(BeaconBehavior p_beaconBehavior);
    private static DestroyBeaconDelegator OnDestroyBeacon;

    /// <summary>
    /// We override ActualiseParent to let the SynchronizeBeacons know when beacons are grabbed or not
    /// </summary>
    public override void ActualiseParent() {
        base.ActualiseParent();

        if(!MyNetworkManager.singleton.m_canSend) return;
        
        if (m_isCaught) {
            m_beaconLoading.Unloading();
        }
    }
    
    // private void OnDrawGizmos() {
    //     Gizmos.DrawWireSphere(transform.position, m_radius); // feedback magic ♪♪ ヽ(ˇ∀ˇ )ゞ
    // }

    //TODO comment ! (╬ ಠ益ಠ)
    protected override void OnFirstTimeTouchingGround() {

        if (m_isDeployed) return;
        base.OnFirstTimeTouchingGround();
        
        m_isGrabbable = false;
        m_rb.isKinematic = true;
        //Destroy(m_rb);
        m_isDeployed = true;
        transform.rotation = Quaternion.Euler(Vector3.zero);

        //For each children of this object, make them inflate and add the script in the case they don't have it already
        foreach (Transform child in transform) {
            if (!child.gameObject.TryGetComponent(out InflateToSize script)){
                script = child.gameObject.AddComponent<InflateToSize>();
            }
            
            script.StartInflating();
        }

        m_synchronizer.SendBeaconActivation(m_index);
    }

    protected override void BeingDestroyed() {
        base.BeingDestroyed();
        
        OnDestroyBeacon?.Invoke(this); //Saying everyone we get destroyed
    }

    /// <summary> </summary>
    /// <param name="p_grabbableObject"></param>
    private void ActualiseIndex(BeaconBehavior p_grabbableObject) {
        if (m_index > p_grabbableObject.m_index) m_index--;
    }
}
