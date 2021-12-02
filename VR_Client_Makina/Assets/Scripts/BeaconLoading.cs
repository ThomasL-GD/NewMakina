using Synchronizers;
using UnityEngine;

[RequireComponent(typeof(BeaconBehavior))]
public class BeaconLoading : MonoBehaviour {

    public static SynchronizeBeacons s_synchronizer;
    
    [HideInInspector] private int m_indexInArm = 0;
    private static int s_currentIndex = 0;
    
    private delegate void UnloadingBeaconDelegator(int p_index);
    /// <summary> Is called when a beacon is unloaded and gives the index of the unloaded beacon </summary>
    private static UnloadingBeaconDelegator OnBeaconUnloaded;
    
    // Start is called before the first frame update
    void OnEnable() {
        m_indexInArm = s_currentIndex;
        s_currentIndex++; //We increase
        OnBeaconUnloaded += SetBack;
        SetPosition();
    }

    /// <summary> Call this to unload a beacon from the arm </summary>
    public void Unloading() {
        s_currentIndex--;
        OnBeaconUnloaded -= SetBack;
        OnBeaconUnloaded?.Invoke(m_indexInArm);
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
        
        if(m_indexInArm > SynchronizeBeacons.maxSlotsForBeacons) Debug.LogWarning("wtf plzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz don't tell me this error occured", this);
        
        Transform reference = s_synchronizer.m_loadingPositions[m_indexInArm];
        transform.position = reference.position;
        transform.SetParent(reference);
    }
}
