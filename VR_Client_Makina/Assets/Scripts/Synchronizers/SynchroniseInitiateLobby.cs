using CustomMessages;
using Network;
using UnityEngine;

namespace Synchronizers {

    public class SynchroniseInitiateLobby : Synchronizer<SynchroniseInitiateLobby> {
    
        // Start is called before the first frame update
        void Start() {
            MyNetworkManager.OnReceiveInitiateLobby += ReceiveInitiateLobby;
        }

        private void ReceiveInitiateLobby(InitiateLobby p_initiateLobby) {
            Debug.Log($"initiate lobby received : {p_initiateLobby.trial}");
        }
    }
}
