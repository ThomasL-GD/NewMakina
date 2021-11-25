using System.Collections.Generic;
using Mirror;
using Mirror.Discovery;
using UnityEngine;

public class RefreshBroadcastButton : LaserSensitiveButtonBehavior {
    
    private readonly Dictionary<long, ServerResponse> discoveredServers = new Dictionary<long, ServerResponse>();

    [HideInInspector] public NetworkDiscovery networkDiscovery;
    
    // Start is called before the first frame update
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        
    }

    /// <summary>
    /// Overriden to start broadcast when the button is hit
    /// </summary>
    public override void OnBeingShot() {
        discoveredServers.Clear();
        networkDiscovery.StartDiscovery();
    }
    
    
}
