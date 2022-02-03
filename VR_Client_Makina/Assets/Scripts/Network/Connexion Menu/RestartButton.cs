using CustomMessages;

namespace Network.Connexion_Menu {

    public class RestartButton : AttackSensitiveButton {
        public override void OnBeingActivated() {
            MyNetworkManager.singleton.SendVrData(new RestartGame(){});
        }
    }

}
