namespace Network.Connexion_Menu {

    public class AutoConnectButtonTypeBeat : ConnexionMenuButtonBehavior{
        public override void OnBeingActivated() {
            base.OnBeingActivated();
            MyNetworkManager.singleton.CustomConnect();
        }
    }

}
