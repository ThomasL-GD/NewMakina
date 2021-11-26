using System.Collections.Generic;
using Mirror.Discovery;
using UnityEngine;

namespace Network {

    public class RefreshBroadcastButton : LaserSensitiveButtonBehavior {

        [SerializeField] private MyNetworkDiscovery m_networkDiscovery = null;
        
        // Start is called before the first frame update
        void Start() {
            if(m_networkDiscovery == null)Debug.LogError("No network discorvery serialized", this);
        }
        

        /// <summary>
        /// Overriden to start broadcast when the button is hit
        /// </summary>
        public override void OnBeingShot() {
            MyNetworkDiscovery.DiscoveredServers.Clear();
            m_networkDiscovery.StartDiscovery();
        }
        
        
    }
}
