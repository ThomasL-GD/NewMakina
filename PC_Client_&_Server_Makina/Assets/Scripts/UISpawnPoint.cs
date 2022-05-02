using System;
using Synchronizers;
using UnityEngine;

public class UISpawnPoint : MonoBehaviour {

    private ushort m_index = 0;
    private SynchronizeRespawn m_synchronizer;

    private void OnMouseOver() {
        if (Input.GetMouseButtonDown(0)) {
            m_synchronizer.ChooseSpawnPoint(m_index);
        }
    }

    /// <summary>Insert the values that are necessary for this to work </summary>
    /// <param name="p_index">The index of the spawnpoint in the synchroniser array</param>
    /// <param name="p_synchroniser">The SynchronizeRespawn calling this function</param>
    public void InsertValues(ushort p_index, SynchronizeRespawn p_synchroniser) {
        m_index = p_index;
        m_synchronizer = p_synchroniser;
    }
}
