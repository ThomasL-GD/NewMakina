using CustomMessages;
using Network;
using Synchronizers;
using UnityEngine;

public class SynchronizeTpRollback : Synchronizer<SynchronizeTpRollback> {

    [SerializeField] private GameObject m_prefabTpPoint = null;
    private GameObject m_tpPoint = null;
    
    private void Start() {
#if UNITY_EDITOR
        if(m_prefabTpPoint == null)Debug.LogError("No tp point prefab serialized here !", this);
#endif
        MyNetworkManager.OnReceiveRemoveTp += Remove;
        MyNetworkManager.OnReceiveDropTp += PlaceTp;
    }

    private void PlaceTp(DropTp p_tpDrop) {
        m_tpPoint = Instantiate(m_prefabTpPoint, p_tpDrop.tpPosition, Quaternion.Euler(Vector3.zero));
    }

    private void Remove(RemoveTp p_ready) {
#if UNITY_EDITOR
        if(m_tpPoint == null) Debug.LogError("wtf no way check this non existant transgform", this);
#endif
        Destroy(m_tpPoint);
        m_tpPoint = null;
    }
}
