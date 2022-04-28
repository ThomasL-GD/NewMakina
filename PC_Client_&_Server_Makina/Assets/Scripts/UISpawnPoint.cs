using System;
using Synchronizers;
using UnityEngine;

public class UISpawnPoint : MonoBehaviour {

    private byte m_index = 0;
    private SynchronizeRespawn m_synchronizer;

    private void OnMouseOver() {
        if (Input.GetMouseButtonDown(0)) {
            m_synchronizer.ChooseSpawnPoint(m_index);
        }
    }
}
