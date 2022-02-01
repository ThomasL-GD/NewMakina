using CustomMessages;
using Network;
using Synchronizers;
using UnityEngine;

public class SynchonizeLeure : Synchronizer<SynchonizeLeure>
{
    [SerializeField] private GameObject m_leure;
    // Start is called before the first frame update
    void Awake()
    {
        MyNetworkManager.OnReceiveSpawnLeure += ReceiveSpawnLeure;
        MyNetworkManager.OnReceiveDestroyLeure += ReceiveDestroyLeure;
        MyNetworkManager.OnReceiveLeureTransform += UpdateLeurePosition;
    }

    // Update is called once per frame
    void ReceiveSpawnLeure(SpawnLeure p_spawnLeure)
    {
        m_leure.transform.position = Vector3.down * 1000f;
        m_leure.SetActive(true);
    }
    void ReceiveDestroyLeure(DestroyLeure p_destroyLeure)  => m_leure.SetActive(false);

    void UpdateLeurePosition(LeureTransform p_leureTransform) => m_leure.transform.SetPositionAndRotation(p_leureTransform.position,p_leureTransform.rotation);
}
