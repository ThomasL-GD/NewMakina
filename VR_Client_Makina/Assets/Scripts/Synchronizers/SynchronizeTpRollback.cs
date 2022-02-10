using System.Collections;
using CustomMessages;
using Network;
using Synchronizers;
using UnityEngine;

public class SynchronizeTpRollback : Synchronizer<SynchronizeTpRollback> {

    [Header("Tp point")]
    [SerializeField] private GameObject m_prefabTpPoint = null;
    private GameObject m_tpPoint = null;


    [Header("Particles when tp")]
    [SerializeField] [Range(0.1f, 50f)] public float m_particlesSpeed = 1f;
    [SerializeField] private GameObject m_prefabParticles = null;
    
    private void Start() {
#if UNITY_EDITOR
        if(m_prefabTpPoint == null)Debug.LogError("No tp point prefab serialized here !", this);
        if(m_prefabParticles == null)Debug.LogError("No particles prefab serialized here !", this);
#endif
        MyNetworkManager.OnReceiveRemoveTp += Remove;
        MyNetworkManager.OnReceiveDropTp += PlaceTp;
        MyNetworkManager.OnReceiveTeleported += Teleport;
    }

    private void Teleport(Teleported p_teleported) {
        GameObject go = Instantiate(m_prefabParticles, p_teleported.teleportOrigin, Quaternion.LookRotation(p_teleported.teleportDestination - p_teleported.teleportOrigin));
        ParticleSystem.MainModule bob = go.GetComponent<ParticleSystem>().main;
        StartCoroutine(DestroyParticles(bob.duration + bob.startLifetime.constantMax, go));
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

    IEnumerator DestroyParticles(float p_duration, GameObject p_go) {
        yield return new WaitForSeconds(p_duration);
        Destroy(p_go);
    }
}
