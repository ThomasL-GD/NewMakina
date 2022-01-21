using System;
using System.Collections.Generic;
using Mirror.Discovery;
using UnityEngine;

namespace Network.Connexion_Menu {

    public class RefreshBroadcastButton : ConnexionMenuButtonBehavior {
        
        readonly Dictionary<long, ServerResponse> discoveredServers = new Dictionary<long, ServerResponse>();
        
        [SerializeField] [Tooltip("the gameobject that will spawn for every server available.\nMust contain the ServerConnectButton script.")] private GameObject m_prefabServerConnectButton = null;
        [SerializeField] private Vector3 m_positionFirstButton = Vector3.zero;

        public NetworkDiscovery networkDiscovery;

        private List<GameObject> m_spawnedButtons = new List<GameObject>();

        private void Start() {
            if (m_prefabServerConnectButton == null) Debug.LogError("No server button prefab serialized, how do you expect me to work ?!", this);
            
            UnityEditor.Events.UnityEventTools.AddPersistentListener(networkDiscovery.OnServerFound, OnDiscoveredServer);
        }

        private void OnValidate() {
            //UnityEditor.Events.UnityEventTools.AddPersistentListener(networkDiscovery.OnServerFound, OnDiscoveredServer);
        }

        /// <summary>
        /// Overriden to start broadcast when the button is hit
        /// </summary>
        public override void OnBeingActivated() {
            base.OnBeingActivated();
            MyNetworkDiscovery.DiscoveredServers.Clear();
            MyNetworkDiscovery.singleton.StartDiscovery();

            ClearSpawnedButtons();
            
            int i = 0;
            foreach (ServerResponse info in discoveredServers.Values) {
                SpawnServerListElement(m_positionFirstButton + (Vector3.up * 5 * i), info);
                i++;
            }
        }

        /// <summary> Will spawn a ServerConnectButton and attach it a ServerMessage </summary>
        /// <param name="p_position">The world position where the object shall spawn</param>
        /// <param name="p_serverResponse">The ServerMessage we should attach to it</param>
        private void SpawnServerListElement(Vector3 p_position, ServerResponse p_serverResponse) {
            GameObject go = Instantiate(m_prefabServerConnectButton, p_position, Quaternion.Euler(Vector3.zero));
            go.GetComponent<ServerConnectButton>().serverResponse = p_serverResponse;
            m_spawnedButtons.Add(go);
        }

        /// <summary> Will clear the m_spawnedButtons list and destroy every GameObject that is in it </summary>
        private void ClearSpawnedButtons() {
            foreach (GameObject go in m_spawnedButtons) { Destroy(go); }
            m_spawnedButtons.Clear();
        }

        public void OnDiscoveredServer(ServerResponse p_info)
        {
            // Note that you can check the versioning to decide if you can connect to the server or not using this method
            discoveredServers[p_info.serverId] = p_info;
        }

    }
}
