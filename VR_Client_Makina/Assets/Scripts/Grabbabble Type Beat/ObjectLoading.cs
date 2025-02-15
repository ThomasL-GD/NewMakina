using Grabbabble_Type_Beat;
using Synchronizers;
using UnityEngine;

[RequireComponent(typeof(GrabbableObject))]
public class ObjectLoading : MonoBehaviour {

    [HideInInspector] public SynchronizeLoadedObjectsAbstract m_synchronizer;
    
    /*[SerializeField] [Tooltip("For Debug Only.")]/**/ protected int m_indexInArm = 0;
    
    /// <summary>
    /// You MUST call this function to initialize the loading in arm.
    /// It is used to set its index and subscribe to some internal delegates.
    /// It also sets their position.
    /// </summary>
    public virtual void Initialization(int p_index) {
        m_indexInArm = p_index;
        SetPosition();
    }

    /// <summary> Call this to unload a beacon from the arm </summary>
    public virtual void Unloading() {
        m_synchronizer.RestoreAvailability(m_indexInArm);
        Destroy(this);
    }

    /// <summary> Set the position and parent of this object according to its current index in the arm </summary>
    protected void SetPosition() {
        
        if(m_indexInArm > m_synchronizer.m_maxSlotsLoading) Debug.LogWarning($"wtf plzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz don't tell me this error occured   index : {m_indexInArm}    >      maxSlotxs : {m_synchronizer.m_maxSlotsLoading}", this);
        
        Transform reference = m_synchronizer.m_loadingPositions[m_indexInArm];
        transform.position = reference.position;
        
        if(m_synchronizer.m_stickToSpawnPosition)transform.SetParent(reference);
    }
}