using System;
using System.Collections.Generic;
using Mirror;
using Mirror.Discovery;
using UnityEngine;

namespace Network.Connexion_Menu { /*
    Documentation: https://mirror-networking.gitbook.io/docs/components/network-discovery
    API Reference: https://mirror-networking.com/docs/api/Mirror.Discovery.NetworkDiscovery.html
*/

    public class DiscoveryRequest : NetworkMessage {
        // Add properties for whatever information you want sent by clients
        // in their broadcast messages that servers will consume.
    }

    public class DiscoveryResponse : NetworkMessage {
        // Add properties for whatever information you want the server to return to
        // clients for them to display or consume for establishing a connection.
    }

    public class MyNetworkDiscovery : NetworkDiscovery {

    
        //Singleton time ! 	(˵ ͡° ͜ʖ ͡°˵)
        public static MyNetworkDiscovery singleton { get; private set; }
        
        /// <summary>
        /// Is that... a singleton setup ?
        /// *Pokédex's voice* A singleton, a pretty common pokécode you can find in a lot of projects, it allows anyone to
        /// call it and ensure there is only one script of this type in the entire scene !
        /// </summary>
        private void Awake() {
            //base.Awake();
            // if the singleton hasn't been initialized yet
            if (singleton != null && singleton != this)
            {
                gameObject.SetActive(this);
                Debug.LogWarning("BROOOOOOOOOOOOOOOOOOO ! There are too many Singletons broda", this);
                return;
            }
 
            singleton = this;
        }
        
        public static readonly Dictionary<long, ServerResponse> DiscoveredServers = new Dictionary<long, ServerResponse>();

        public void CustomDiscoveryConnect(Uri p_uri) {
            StopDiscovery();
            NetworkManager.singleton.StartClient(p_uri);
        }
        
    }

}