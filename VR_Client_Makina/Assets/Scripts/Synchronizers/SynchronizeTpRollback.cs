using System.Collections;
using CustomMessages;
using Network;
using Synchronizers;
using UnityEngine;
using UnityEngine.VFX;

public class SynchronizeTpRollback : Synchronizer<SynchronizeTpRollback> {

    [Header("Tp point")]
    [SerializeField] private GameObject m_prefabTpPoint = null;
    private GameObject m_tpPoint = null;


    [Header("Particles when tp")]
    [SerializeField] private VisualEffect m_visualEffect = null;

    [Header("Sound")]
    [SerializeField] private AudioSource m_soundWhenRollback;
    
    private void Start() {
#if UNITY_EDITOR
        if(m_prefabTpPoint == null)Debug.LogError("No tp point prefab serialized here !", this);
        if(m_visualEffect == null)Debug.LogError("No particles prefab serialized here !", this);
#endif
        MyNetworkManager.OnReceiveRemoveTp += Remove;
        MyNetworkManager.OnReceiveDropTp += PlaceTp;
        MyNetworkManager.OnReceiveTeleported += Teleport;
    }

    private void Teleport(Teleported p_teleported)
    {
        m_visualEffect.transform.position = p_teleported.teleportOrigin + Vector3.up * 3f;
        Debug.Log(p_teleported.teleportDestination);
        m_visualEffect.SetVector3("Teleport Position",p_teleported.teleportDestination);
        m_visualEffect.SendEvent("StartTP");
    }

    [ContextMenu("test")]
    private void test()
    {
        m_visualEffect.SetVector3("Teleport Position",new Vector3(0,200,0));
    }

    private void PlaceTp(DropTp p_tpDrop) {
        m_tpPoint = Instantiate(m_prefabTpPoint, p_tpDrop.tpPosition, Quaternion.Euler(Vector3.zero));
    }

    private void Remove(RemoveTp p_remove) {
#if UNITY_EDITOR
        if(m_tpPoint == null) Debug.LogError("wtf no way check this non-existant transgform", this);
#endif
        Destroy(m_tpPoint);
        m_tpPoint = null;
    }
}
