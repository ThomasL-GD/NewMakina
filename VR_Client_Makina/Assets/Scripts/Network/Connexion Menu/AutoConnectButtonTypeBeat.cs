namespace Network.Connexion_Menu {

    public class AutoConnectButtonTypeBeat : AttackSensitiveButton{
        public override void OnBeingActivated() {
            base.OnBeingActivated();
            MyNetworkManager.singleton.CustomConnect();
        }
    }

}
