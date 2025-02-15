using CustomMessages;
using Network;
using Synchronizers;
using UnityEngine;
using UnityEngine.VFX;

public class SynchonizeLeure : Synchronizer<SynchonizeLeure> {
    
    [SerializeField] private GameObject m_leure;

    [SerializeField] private VisualEffect m_deathFX;
    // Start is called before the first frame update
    void Awake()
    {
        MyNetworkManager.OnReceiveSpawnLeure += ReceiveSpawnLeure;
        MyNetworkManager.OnReceiveDestroyLeure += ReceiveDestroyLeure;
        MyNetworkManager.OnReceiveLeureTransform += UpdateLeurePosition;
    }

    // Update is called once per frame
    void ReceiveSpawnLeure(SpawnLeure p_spawnLeure) {
        
        m_leure.transform.position = Vector3.down * 1000f;
        m_leure.SetActive(true);
    }

    void ReceiveDestroyLeure(DestroyLeure p_destroyLeure) {
        
        m_deathFX.transform.position = m_leure.transform.position;
        Debug.Log($"CLOUDS ON : {m_leure.transform.position}");
        m_deathFX.SendEvent("StartLeureDeath");
        m_leure.SetActive(false);
        m_leure.transform.position = Vector3.down * 1000f;
    }

    void UpdateLeurePosition(LeureTransform p_leureTransform) {
        
        if(!m_leure.activeSelf) return;
        m_leure.transform.SetPositionAndRotation(p_leureTransform.position, p_leureTransform.rotation);
    }
}