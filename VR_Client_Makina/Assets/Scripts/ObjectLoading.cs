using Synchronizers;
using UnityEngine;

[RequireComponent(typeof(GrabbablePhysickedObject))]
public class ObjectLoading : MonoBehaviour {

    [HideInInspector] public SynchronizeLoadedObjectsAbstract m_synchronizer;
    
    [SerializeField] [Tooltip("For Debug Only.")] private int m_indexInArm = 0;
    
    private delegate void UnloadingLoadedItemDelegator(int p_index);
    /// <summary> Is called when a beacon is unloaded and gives the index of the unloaded beacon </summary>
    private static UnloadingLoadedItemDelegator OnLoadedObjectUnloaded;

    private static int s_currentNewIndex;
    
    /// <summary>
    /// You MUST call this function to initialize the loading in arm.
    /// It is used to set its index and subscribe to some internal delegates.
    /// It also sets their position.
    /// </summary>
    public void Initialization() {
        m_indexInArm = s_currentNewIndex;
        s_currentNewIndex++; //We increase
        OnLoadedObjectUnloaded += SetBack;
        SetPosition();
    }

    /// <summary> Call this to unload a beacon from the arm </summary>
    public void Unloading() {
        s_currentNewIndex--;
        OnLoadedObjectUnloaded -= SetBack;
        OnLoadedObjectUnloaded?.Invoke(m_indexInArm);
        Destroy(this);
    }

    /// <summary>
    /// Is called when a beacon is unloaded from the arm
    /// Will change the index and position of this object if the unloaded beacon was before this object in the arm order
    /// </summary>
    /// <param name="p_index">The index in the arm of the beacon that was suppressed</param>
    private void SetBack(int p_index) {
        if (p_index > m_indexInArm) return; //This function i only useful to the beacons that have a smaller index in arm than the one that got unloaded

        m_indexInArm--;
        SetPosition();
    }

    /// <summary> Set the position and parent of this object according to its current index in the arm </summary>
    private void SetPosition() {
        
        if(m_indexInArm > m_synchronizer.m_maxSlotsLoading) Debug.LogWarning("wtf plzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz don't tell me this error occured", this);
        
        Transform reference = m_synchronizer.m_loadingPositions[m_indexInArm];
        transform.position = reference.position;
        transform.SetParent(reference);
    }
}
