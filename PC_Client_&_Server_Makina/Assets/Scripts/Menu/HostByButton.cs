using Mirror;
using UnityEngine;

namespace Menu {

    [RequireComponent(typeof(MakiButton))]
    public class HostByButton : MonoBehaviour {
        private void Start() {
            GetComponent<MakiButton>().OnClick += StartHosting;
        }

        private static void StartHosting() {
            if (NetworkClient.active) return;
            if (Application.platform == RuntimePlatform.WebGLPlayer) return;
            
            MyNetworkManager.singleton.StartHost();
        }
    }

}
