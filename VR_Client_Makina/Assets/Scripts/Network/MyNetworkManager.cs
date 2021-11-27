using CustomMessages;
using Mirror;
using UnityEngine;

namespace Network {

    public class MyNetworkManager : NetworkManager {

    
        //Singleton time ! 	(˵ ͡° ͜ʖ ͡°˵)
        public new static MyNetworkManager singleton { get; private set; }
    
        [Header("ServerVariables")]
        [SerializeField][Tooltip("The prefab of the hearts that will be spawned on server connection")] private GameObject m_heartPrefabs;
        public bool m_canSend = false;

        [Header("AutoConnexion type beat")]
        [SerializeField] private bool m_localHost = false;
        [SerializeField] private string m_IPadress = "10.31.240.210";

        #region delegates

        /// <summary/> The delegate that is called when the client's connection is confirmed
        public delegate void ServerDelegator();
        public static ServerDelegator OnConnection;
    

        /// <summary/> Is called when we receive new PcTransform data from server
        public delegate void PcTransformDelegator(PcTransform p_pcTransform);
        public static PcTransformDelegator OnPcTransformUpdate;
    
        /// <summary/> Is called when we receive new Laser data from server
        public delegate void LaserDelegator(Laser p_laser);
        public static LaserDelegator OnLaserShootingUpdate;
        public static LaserDelegator OnLaserAimingUpdate;
    
        /// <summary/> Is called when we receive new PcInvisibility data from server
        public delegate void InvisibilityDelegator(PcInvisibility p_pcInvisibility);
        public static InvisibilityDelegator OnInvisibilityUpdate;
    
        /// <summary/> The delegates that will be called each time the server updates the hearts
        public delegate void HeartTransformDelegator(HeartTransforms p_heartTransforms);
        public static HeartTransformDelegator OnReceiveHeartTransforms;
    
        public delegate void HeartBreakDelegator(HeartBreak p_heartBreak);
        public static HeartBreakDelegator OnReceiveHeartBreak;
    
        /// <summary/> The delegates that will be called each time the server updates the beacons
        public delegate void BeaconsPositionsDelegator(BeaconsPositions p_beaconsPositions);
        public static BeaconsPositionsDelegator OnReceiveBeaconsPositions;
    
        public delegate void DestroyedBeaconsDelegator(DestroyedBeacon p_beaconsPositions);
        public static DestroyedBeaconsDelegator OnReceiveDestroyedBeacon;
    
        public delegate void BeaconDetectionUpdateDelegator(BeaconDetectionUpdate p_beaconsPositions);
        public static BeaconDetectionUpdateDelegator OnReceiveBeaconDetectionUpdate;
    
        /// <summary/> The delegate that will be called each time the server sends a GameEnd message
        public delegate void GameEndDelegator(GameEnd p_gameEnd);
        public static GameEndDelegator OnReceiveGameEnd;
        
        /// <summary/> The delegate that will be called each time the server sends a InitialData message
        public delegate void InitialDataDelegator(InitialData p_initialData);
        public static InitialDataDelegator OnReceiveInitialData;

        #endregion
        
        /// <summary>
        /// Is that... a singleton setup ?
        /// *Pokédex's voice* A singleton, a pretty common pokécode you can find in a lot of projects, it allows anyone to
        /// call it and ensure there is only one script of this type in the entire scene !
        /// </summary>
        private new void Awake()
        {
            //base.Awake();
            // if the singleton hasn't been initialized yet
            if (singleton != null && singleton != this)
            {
                gameObject.SetActive(this);
                Debug.LogWarning("BROOOOOOOOOOOOOOOOOOO ! There are too many Singletons broda", this);
                return;
            }
 
            singleton = this;
        
            networkAddress = "";
        }

        /// <summary/> Is called by other scripts to send data to the server
        /// <param name="p_vrDataToSend"></param>
        /// <typeparam name="T">Can be anything as long as its a struct deriving from VrData</typeparam>
        public void SendVrData<T>(T p_vrDataToSend) where T : struct, NetworkMessage {
            if (NetworkClient.active && m_canSend) {
            
                //Debug.Log($"I'm sending {p_vrDataToSend}");
                NetworkClient.Send(p_vrDataToSend);
            }
            // else {
            //     Debug.LogWarning("You want to send data to no one, pretty sus", this);
            // }
        }

        /// <summary/> Is called when we get connected
        public override void OnStartClient() {
            NetworkClient.RegisterHandler<PcTransform>(ReceivePcTransform);
            NetworkClient.RegisterHandler<Laser>(ReceivePcTransform);
            NetworkClient.RegisterHandler<PcInvisibility>(ReceivePcInvisibility);
            NetworkClient.RegisterHandler<ClientConnect>(ReceiveClientConnect);
            NetworkClient.RegisterHandler<HeartTransforms>(ReceiveHeartTranforms);
            NetworkClient.RegisterHandler<HeartBreak>(ReceiveHeartBreak);
            NetworkClient.RegisterHandler<BeaconsPositions>(ReceiveBeaconsPositions);
            NetworkClient.RegisterHandler<DestroyedBeacon>(ReceiveDestroyedBeacon);
            NetworkClient.RegisterHandler<BeaconDetectionUpdate>(ReceiveBeaconDetectionUpdate);
            NetworkClient.RegisterHandler<GameEnd>(ReceiveGameEnd);
            NetworkClient.RegisterHandler<InitialData>(ReceiveInitialData);
        
            OnConnection?.Invoke();
        }

        /// <summary/> The function called when the client receives a message of type InitialData
        /// <param name="p_initialData"> The message sent by the Server to the Client </param>
        private void ReceiveInitialData(InitialData p_initialData) => OnReceiveInitialData?.Invoke(p_initialData);

        /// <summary/> The function called when the client receives a message of type HeartTransforms
        /// <param name="p_heartTransforms"> The message sent by the Server to the Client </param>
        private void ReceiveHeartTranforms(HeartTransforms p_heartTransforms) => OnReceiveHeartTransforms?.Invoke(p_heartTransforms); 
    
        /// <summary/> The function called when the client receives a message of type HeartTransforms
        /// <param name="p_heartBreak"> The message sent by the Server to the Client </param>
        private void ReceiveHeartBreak(HeartBreak p_heartBreak) => OnReceiveHeartBreak?.Invoke(p_heartBreak); 
    
        /// <summary/> Is called when we receive data of type ClientConnect from the server
        /// <param name="p_clientConnect">The actual data we received</param>
        private void ReceiveClientConnect(ClientConnect p_clientConnect) {
            NetworkClient.Send(new ClientConnect(){client = ClientConnection.VrPlayer});
            m_canSend = true;
        } 
    
        /// <summary/> Is called when we receive data of type PcTransform from the server
        /// <param name="p_pcTransform">The actual data we received</param>
        private void ReceivePcTransform(PcTransform p_pcTransform) => OnPcTransformUpdate?.Invoke(p_pcTransform);

        /// <summary/> Is called when we receive data of type Laser from the server
        /// <param name="p_laser">The actual data we received</param>
        private void ReceivePcTransform(Laser p_laser) {

            switch (p_laser.laserState) { //We do different things if the server is telling us that the laser is aiming or shooting
            
                //TODO have two different delegates for those two
                case LaserState.Aiming:
                    OnLaserAimingUpdate?.Invoke(p_laser);//Turn on the aim
                    break;
                case LaserState.CancelAiming:
                    OnLaserAimingUpdate?.Invoke(p_laser);//Turn off the aim
                    break;
            
                case LaserState.Shooting:
                    OnLaserShootingUpdate?.Invoke(p_laser);
                    break;
            }
        }

        /// <summary/> Is called when we receive data of type pcInvisibility from the server
        /// <param name="p_pcInvisibility">The actual data we received</param>
        private void ReceivePcInvisibility(PcInvisibility p_pcInvisibility) => OnInvisibilityUpdate?.Invoke(p_pcInvisibility);
    
        /// <summary/> The function called when the client receives a message of type BeaconsPositions
        /// <param name="p_beaconsPositions"> The message sent by the Server to the Client </param>
        private void ReceiveBeaconsPositions(BeaconsPositions p_beaconsPositions) => OnReceiveBeaconsPositions?.Invoke(p_beaconsPositions); 
    
        /// <summary/> The function called when the client receives a message of type DestroyedBeacon
        /// <param name="p_destroyedBeacon"> The message sent by the Server to the Client </param>
        private void ReceiveDestroyedBeacon(DestroyedBeacon p_destroyedBeacon) => OnReceiveDestroyedBeacon?.Invoke(p_destroyedBeacon);
    
        /// <summary/> The function called when the client receives a message of type BeaconDetectionUpdate
        /// <param name="p_beaconDetectionUpdate"> The message sent by the Server to the Client </param>
        private void ReceiveBeaconDetectionUpdate(BeaconDetectionUpdate p_beaconDetectionUpdate) => OnReceiveBeaconDetectionUpdate?.Invoke(p_beaconDetectionUpdate);

        /// <summary/> The function called when the client receives a message of type GameEnd
        /// <param name="p_gameEnd"> The message sent by the Server to the Client </param>
        private void ReceiveGameEnd(GameEnd p_gameEnd) => OnReceiveGameEnd?.Invoke(p_gameEnd);

        /// <summary>
        /// Connects to the default serialized ip
        /// (can be set to localhost)
        /// </summary>
        public void CustomConnect() {
            networkAddress = m_localHost ? "localhost" : m_IPadress;
            StartClient();
        }
    }

}