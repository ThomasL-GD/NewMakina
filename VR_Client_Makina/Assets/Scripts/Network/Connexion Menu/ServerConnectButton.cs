using Mirror.Discovery;

namespace Network.Connexion_Menu {
    public class ServerConnectButton : ConnexionMenuButtonBehavior {

        public ServerResponse serverResponse;

        /// <summary>Override OnBeingActivated to start the connection </summary>
        public override void OnBeingActivated() {
            base.OnBeingActivated();
            MyNetworkDiscovery.singleton.CustomDiscoveryConnect(serverResponse.uri);
        }
        
    }
}