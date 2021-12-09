using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Derives from ObjectLoading class.
/// Will make sure all the objects that have this script will be placed in order.
/// </summary>
public class ObjectLoadingInOrder : ObjectLoading {
    
    private delegate void UnloadingLoadedItemDelegator(int p_index);
    /// <summary> Is called when a beacon is unloaded and gives the index of the unloaded beacon </summary>
    private static UnloadingLoadedItemDelegator OnLoadedObjectUnloaded;

    private static int s_currentNewIndex;
    
    /// <summary>
    /// Alternate function from the one that requires an int as parameter.
    /// Will set the new loaded object at the nearest position available and set everything up for the order to be preserved.
    /// </summary>
    public void Initialization() {
        m_indexInArm = s_currentNewIndex;
        s_currentNewIndex++; //We increase
        OnLoadedObjectUnloaded += SetBack;
        SetPosition();
    }

    public override void Unloading() {
        s_currentNewIndex--;
        OnLoadedObjectUnloaded -= SetBack;
        OnLoadedObjectUnloaded?.Invoke(m_indexInArm);
        base.Unloading();
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
}
