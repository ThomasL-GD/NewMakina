using CustomMessages;

namespace Network.Connexion_Menu {

    public class RestartButton : AttackSensitiveButton {
        public override void OnBeingActivated() {
            MyNetworkManager.OnReadyToPlay += DeactivateMySelf;
            MyNetworkManager.singleton.SendVrData(new RestartGame(){});
        }

        private void DeactivateMySelf(ReadyToPlay p_r) {
            gameObject.SetActive(false);
        }
    }

}
