using CustomMessages;
using Network;
using UnityEngine;

public class ChangeMaterialOnHeartbreak : MonoBehaviour {

    [SerializeField] private int m_heartID;
    [SerializeField] private Material[] m_materialWhenDestroyed;
    private Material[] m_baseMaterial;
    private bool m_isAlreadyChanged = false;
    
    // Start is called before the first frame update
    void Start() {
        MyNetworkManager.OnReceiveHeartBreak += Change;
        MyNetworkManager.OnReceiveInitialData += Reset;
        MyNetworkManager.OnReadyToGoIntoTheBowl += Reset;
        m_baseMaterial = GetComponent<MeshRenderer>().materials;
    }

    private void Reset(ReadyToGoIntoTheBowl p_p_ready) => Reset();
    private void Reset(InitialData p_initialData) => Reset();

    private void Reset() {
        GetComponent<MeshRenderer>().materials = m_baseMaterial;
        m_isAlreadyChanged = false;
    }

    private void Change(HeartBreak p_heartBreak) {
        if(m_isAlreadyChanged || p_heartBreak.index != m_heartID) return;

        m_isAlreadyChanged = true;
        GetComponent<MeshRenderer>().materials = m_materialWhenDestroyed;
    }
}
