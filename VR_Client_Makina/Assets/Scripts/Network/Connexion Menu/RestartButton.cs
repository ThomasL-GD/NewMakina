using CustomMessages;

namespace Network.Connexion_Menu {

    public class RestartButton : AttackSensitiveButton {
        public override void OnBeingActivated() {
            MyNetworkManager.OnReadyToFace -= DeactivateMySelf;
            MyNetworkManager.OnReadyToFace += DeactivateMySelf;
            MyNetworkManager.singleton.SendVrData(new RestartGame(){});
        }

        private void DeactivateMySelf(ReadyToFace p_r) {
            gameObject.SetActive(false);
        }
    }

}
