using Network.Connexion_Menu;
using Synchronizers;

public class GoBackToMenuOnShot : AttackSensitiveButton {
    
    public override void OnBeingActivated() {
        base.OnBeingActivated();
        SynchronizeReadyOrNot.Instance.GoFromEndScreenToMainMenu();
    }
}
