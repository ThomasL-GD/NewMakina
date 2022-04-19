using Network;
using Synchronizers;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class BeaconBehavior : GrabbablePhysickedObject {

    [HideInInspector] public int m_index;
    [HideInInspector] public float m_serverID;
    [HideInInspector] public SynchronizeBeacons m_synchronizer;
    //
    // [SerializeField] [Tooltip("The prefab of the beacon once it is deployed.\nMust contain the InflateToSize script")] private GameObject m_prefabDeployed = null;
    
    private ObjectLoading m_beaconLoading = null;

    private bool m_isDeployed = false;

    private delegate void DestroyBeaconDelegator(BeaconBehavior p_beaconBehavior);
    private static DestroyBeaconDelegator OnDestroyBeacon;
    
    protected override void Start() {
        base.Start();
        //m_isPuttableOnlyOnce = true;

        if (gameObject.TryGetComponent(out ObjectLoading script)) {
            m_beaconLoading = script;
        }
        else {
            Debug.LogWarning("No ObjectLoading script attached on this object but don't worry i gotchu fam ( ͡° ͜ʖ ͡°)", this);
            m_beaconLoading = gameObject.AddComponent<ObjectLoading>();
        }

        m_beaconLoading.m_synchronizer = m_synchronizer;
        m_synchronizer.LoadBeaconRandomly(m_beaconLoading);

        BeaconBehavior.OnDestroyBeacon += ActualiseIndex;
    }
    
    public override void BeGrabbed(Transform p_parent, Vector3 p_offsetPositionInHand) {

        if (!m_hasBeenCaughtInLifetime) {
            m_beaconLoading.Unloading();
        }
        
        base.BeGrabbed(p_parent, p_offsetPositionInHand);
    }
    
    // private void OnDrawGizmos() {
    //     Gizmos.DrawWireSphere(transform.position, m_radius); // feedback magic ♪♪ ヽ(ˇ∀ˇ )ゞ
    // }
    
    protected override void OnFirstTimeTouchingGround(Collision p_other) {

        if (m_isDeployed) return;
        base.OnFirstTimeTouchingGround(p_other);
        
        m_isGrabbable = false;
        m_rb.isKinematic = true;
        //Destroy(m_rb);
        m_isDeployed = true;
        transform.rotation = Quaternion.Euler(Vector3.zero);

        m_synchronizer.SendBeaconActivation(m_index, m_serverID);
        
        
    }

    protected override void BeingDestroyed() {
        base.BeingDestroyed();
        
        OnDestroyBeacon -= ActualiseIndex;
        OnDestroyBeacon?.Invoke(this); //Saying everyone we get destroyed
    }

    /// <summary> </summary>
    /// <param name="p_grabbableObject"></param>
    private void ActualiseIndex(BeaconBehavior p_grabbableObject) {
        if (m_index > p_grabbableObject.m_index) m_index--;
    }
}
