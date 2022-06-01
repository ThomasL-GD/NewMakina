using CustomMessages;
using Network;
using UnityEngine;

public class SetReadyFunction : MonoBehaviour {

    [SerializeField] private GameObject m_waitForOtherToBeReadyGameObject = null;

    private void Start() {
#if UNITY_EDITOR
        if (m_waitForOtherToBeReadyGameObject == null) Debug.LogError("Do your job and serialize ! (^∇^) ( ^∇)(　^)(　　)(^　)(∇^ )(^∇^)", this);
#endif
        m_waitForOtherToBeReadyGameObject.SetActive(false);
    }

    [ContextMenu("SetReady")]
    public void SetReady() {
        m_waitForOtherToBeReadyGameObject.SetActive(true);
        MyNetworkManager.OnReceiveInitialData += EraseFeedback;
        MyNetworkManager.singleton.SendVrData(new ReadyToPlay(){});
    }

    private void EraseFeedback(InitialData p_initialdata) {
        m_waitForOtherToBeReadyGameObject.SetActive(false);
    }
}
