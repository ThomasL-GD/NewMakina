using System.Collections;
using System.Collections.Generic;
using Network;
using UnityEngine;

public class AutoConnectButtonTypeBeat : LaserSensitiveButtonBehavior{
    public override void OnBeingShot() {
        base.OnBeingShot();
        MyNetworkManager.singleton.CustomConnect();
    }
}
