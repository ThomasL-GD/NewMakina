using System.Collections;
using System.IO;
using CustomMessages;
using Mirror;
using UnityEngine;

namespace Network {

    public class MyNetworkManager : NetworkManager {

    
        //Singleton time ! 	(˵ ͡° ͜ʖ ͡°˵)
        public new static MyNetworkManager singleton { get; private set; }

        [HideInInspector] public bool m_canSend { get; private set; } = false;

        [Header("AutoConnexion type beat")]
        [SerializeField] private bool m_localHost = false;
        [SerializeField] private string m_IPadress = "10.31.240.210";
        [SerializeField] [Range(0.01f, 60f)] private float m_autoconnexionTimeInterval = 10f;

        /// <summary>If true, we are playing rn, if not, we're probably in a a menu or in the lobby</summary>
        [HideInInspector] public static bool m_isInRealGame = false;

        #region delegates

        /// <summary/> The delegate that is called when the client's connection is confirmed
        public delegate void ServerDelegator();
        public static ServerDelegator OnConnection;
    

        /// <summary/> Is called when we receive new HeartConquerStart data from server
        public delegate void HeartConquerStartDelegator(HeartConquerStart p_heartConquerStart);
        public static HeartConquerStartDelegator OnReceiveHeartConquerStart;
    

        /// <summary/> Is called when we receive new HeartConquerStop data from server
        public delegate void HeartConquerStopDelegator(HeartConquerStop p_ready);
        public static HeartConquerStopDelegator OnReceiveHeartConquerStop;
    

        /// <summary/> Is called when we receive new Teleported data from server
        public delegate void TeleportedDelegator(Teleported p_ready);
        public static TeleportedDelegator OnReceiveTeleported;
    

        /// <summary/> Is called when we receive new RemoveTp data from server
        public delegate void RemoveTpDelegator(RemoveTp p_ready);
        public static RemoveTpDelegator OnReceiveRemoveTp;
    

        /// <summary/> Is called when we receive new DropTp data from server
        public delegate void DropTpDelegator(DropTp p_ready);
        public static DropTpDelegator OnReceiveDropTp;
    

        /// <summary/> Is called when we receive new ReadyToFaceReadyToFace data from server
        public delegate void ReadyOrNotDelegator(ReadyToFace p_ready);
        public static ReadyOrNotDelegator OnReadyToFace;
    

        /// <summary/> Is called when we receive new ReadyToFaceReadyToFace data from server
        public delegate void ReadyToGoIntoTheBowlDelegator(ReadyToGoIntoTheBowl p_ready);
        public static ReadyToGoIntoTheBowlDelegator OnReadyToGoIntoTheBowl;
    

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
    
        public delegate void HeartBreakDelegator(HeartBreak p_heartBreak);
        public static HeartBreakDelegator OnReceiveHeartBreak;
    
        /// <summary/> The delegates that will be called each time the server updates the beacons
        public delegate void SpawnBeaconDelegator(SpawnBeacon p_spawnBeacon);
        public static SpawnBeaconDelegator OnReceiveSpawnBeacon;
    
        public delegate void DestroyedBeaconsDelegator(DestroyedBeacon p_beaconsPositions);
        public static DestroyedBeaconsDelegator OnReceiveDestroyedBeacon;
    
        public delegate void BeaconDetectionUpdateDelegator(BeaconDetectionUpdate p_beaconsPositions);
        public static BeaconDetectionUpdateDelegator OnReceiveBeaconDetectionUpdate;
    
        public delegate void ActivateBeaconDelegator(ActivateBeacon p_activateBeacon);
        public static ActivateBeaconDelegator OnReceiveActivateBeacon;
    
        /// <summary/> The delegate that will be called each time the server sends a GameEnd message
        public delegate void GameEndDelegator(GameEnd p_gameEnd);
        public static GameEndDelegator OnReceiveGameEnd;
        
        /// <summary/> The delegate that will be called each time the server sends a InitialData message
        public delegate void InitialDataDelegator(InitialData p_initialData);
        public static InitialDataDelegator OnReceiveInitialData;
    
        /// <summary/> The delegates that will be called each time the server updates the beacons
        public delegate void SpawnBombDelegator(SpawnBomb p_spawnBomb);
        public static SpawnBombDelegator OnReceiveSpawnBomb;
    
        /// <summary/> The delegates that will be called each time the server updates the beacons
        public delegate void BombExplosionDelegator(BombExplosion p_bombExplosion);
        public static BombExplosionDelegator OnReceiveBombExplosion;


        /// <summary/> The delegate that will be called each time the client receives a ElevatorActivation message
        public delegate void ElevatorActivationDelegator(ElevatorActivation p_bombActivation);
        public static ElevatorActivationDelegator OnReceiveElevatorActivation;
        
        /// <summary/> The delegate that will be called each time the client receives a DeActivateBlind message
        public delegate void DeActivateBlindDelegator(DeActivateBlind p_deActivateBlind);
        public static DeActivateBlindDelegator OnReceiveDeActivateBlind;
        
        /// <summary/> The delegate that will be called each time the client receives a ActivateBlind message
        public delegate void ActivateBlindDelegator(ActivateBlind p_activateBlind);
        public static ActivateBlindDelegator OnReceiveActivateBlind;
        
        /// <summary/> The delegate that will be called each time the client receives a ActivateFlair message
        public delegate void ActivateFlairDelegator(ActivateFlair p_activateFlair);
        public static ActivateFlairDelegator OnReceiveActivateFlair;
        
        /// <summary/> The delegate that will be called each time the client receives a SpawnLeure message
        public delegate void SpawnLeureDelegator(SpawnLeure p_spawnLeure);
        public static SpawnLeureDelegator OnReceiveSpawnLeure;
        
        /// <summary/> The delegate that will be called each time the client receives a DestroyLeure message
        public delegate void DestroyLeureDelegator(DestroyLeure p_destroyLeure);
        public static DestroyLeureDelegator OnReceiveDestroyLeure;
        
        /// <summary/> The delegate that will be called each time the client receives a LeureTransform message
        public delegate void LeureTransformDelegator(LeureTransform p_leureTransform);
        public static LeureTransformDelegator OnReceiveLeureTransform;
        
        /// <summary/> The delegate that will be called each time the client receives a PotentialSpawnPoints message
        public delegate void PotentialSpawnPointsDelegator(PotentialSpawnPoints p_potentialSpawnPoints);
        public static PotentialSpawnPointsDelegator OnReceivePotentialSpawnPoints;
        
        
        public delegate void InitiateLobbyDelegator(InitiateLobby p_initiateLobby);
        public static InitiateLobbyDelegator OnReceiveInitiateLobby;
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
            ReadTextFile();

            StartCoroutine(ForceConnect());
        }

        /// <summary/> Is called by other scripts to send data to the server
        /// <param name="p_vrDataToSend"></param>
        /// <typeparam name="T">Can be anything as long as its a struct deriving from VrData</typeparam>
        public void SendVrData<T>(T p_vrDataToSend) where T : struct, NetworkMessage {
            if (NetworkClient.active && m_canSend) {
#if UNITY_EDITOR
                if(p_vrDataToSend is DestroyedBeacon) Debug.LogError($"WTFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
                //if(p_vrDataToSend is ReadyToGoIntoTheBowl) Debug.LogError($"Send ready to go into the bowl");
                //Debug.Log($"I'm sending {p_vrDataToSend}");
#endif
                NetworkClient.Send(p_vrDataToSend);
            }
            // else {
            //     Debug.LogWarning("You want to send data to no one, pretty sus", this);
            // }
        }

        /// <summary/> Is called when we get connected
        public override void OnStartClient() {
            NetworkClient.RegisterHandler<PcTransform>(ReceivePcTransform);
            NetworkClient.RegisterHandler<Laser>(ReceiveLaser);
            NetworkClient.RegisterHandler<PcInvisibility>(ReceivePcInvisibility);
            NetworkClient.RegisterHandler<ClientConnect>(ReceiveClientConnect);
            NetworkClient.RegisterHandler<HeartBreak>(ReceiveHeartBreak);
            NetworkClient.RegisterHandler<SpawnBeacon>(ReceiveSpawnBeacon);
            NetworkClient.RegisterHandler<DestroyedBeacon>(ReceiveDestroyedBeacon);
            NetworkClient.RegisterHandler<BeaconDetectionUpdate>(ReceiveBeaconDetectionUpdate);
            NetworkClient.RegisterHandler<ActivateBeacon>(ReceiveActivateBeacon);
            NetworkClient.RegisterHandler<GameEnd>(ReceiveGameEnd);
            NetworkClient.RegisterHandler<InitialData>(ReceiveInitialData);
            NetworkClient.RegisterHandler<SpawnBomb>(ReceiveSpawnBomb);
            NetworkClient.RegisterHandler<BombExplosion>(ReceiveBombExplosion);
            NetworkClient.RegisterHandler<ElevatorActivation>(ReceiveElevatorActivation);
            NetworkClient.RegisterHandler<ActivateFlair>(ReceiveActivateFlair);
            NetworkClient.RegisterHandler<ActivateBlind>(ReceiveActivateBlind);
            NetworkClient.RegisterHandler<DeActivateBlind>(ReceiveDeActivateBlind);
            NetworkClient.RegisterHandler<SpawnLeure>(ReceiveSpawnLeure);
            NetworkClient.RegisterHandler<DestroyLeure>(ReceiveDestroyLeure);
            NetworkClient.RegisterHandler<LeureTransform>(ReceiveLeureTransform);
            NetworkClient.RegisterHandler<ReadyToFace>(ReceiveReadyToPlay);
            NetworkClient.RegisterHandler<ReadyToGoIntoTheBowl>(ReceiveReadyToGoIntoTheBowl);
            NetworkClient.RegisterHandler<DropTp>(ReceiveDropTp);
            NetworkClient.RegisterHandler<RemoveTp>(ReceiveRemoveTp);
            NetworkClient.RegisterHandler<Teleported>(ReceiveTeleported);
            NetworkClient.RegisterHandler<HeartConquerStart>(ReceiveHeartConquerStart);
            NetworkClient.RegisterHandler<HeartConquerStop>(ReceiveHeartConquerStop);
            NetworkClient.RegisterHandler<PotentialSpawnPoints>(ReceivePotentialSpawnPoints);
            NetworkClient.RegisterHandler<InitiateLobby>(ReceiveInitiateLobby);
        
            OnConnection?.Invoke();
        }

        private void ReceiveHeartConquerStart(HeartConquerStart p_conquerStart) => OnReceiveHeartConquerStart?.Invoke(p_conquerStart);
        
        private void ReceiveHeartConquerStop(HeartConquerStop p_conquerStop) => OnReceiveHeartConquerStop?.Invoke(p_conquerStop);

        private void ReceiveTeleported(Teleported p_teleported) => OnReceiveTeleported?.Invoke(p_teleported);

        private void ReceiveRemoveTp(RemoveTp p_removeTp) => OnReceiveRemoveTp?.Invoke(p_removeTp);

        private void ReceiveDropTp(DropTp p_dropTp) => OnReceiveDropTp?.Invoke(p_dropTp);

        private void ReceiveReadyToPlay(ReadyToFace p_readyToFace) => OnReadyToFace?.Invoke(p_readyToFace);
        
        private void ReceiveReadyToGoIntoTheBowl(ReadyToGoIntoTheBowl p_readyToGoIntoTheBowl) {
            m_isInRealGame = true;
            OnReadyToGoIntoTheBowl?.Invoke(p_readyToGoIntoTheBowl);
        }

        private void ReceiveActivateFlair(ActivateFlair p_activateFlair) => OnReceiveActivateFlair?.Invoke(p_activateFlair);

        private void ReceiveSpawnLeure(SpawnLeure p_spawnLeure) => OnReceiveSpawnLeure?.Invoke(p_spawnLeure);

        private void ReceiveDestroyLeure(DestroyLeure p_destroyLeure){
            OnReceiveDestroyLeure?.Invoke(p_destroyLeure);
        }

        private void ReceiveLeureTransform(LeureTransform p_leureTransform) => OnReceiveLeureTransform?.Invoke(p_leureTransform);

        private void ReceiveActivateBlind(ActivateBlind p_activateBlind) => OnReceiveActivateBlind?.Invoke(p_activateBlind);
        private void ReceiveDeActivateBlind(DeActivateBlind p_deActivateBlind) => OnReceiveDeActivateBlind?.Invoke(p_deActivateBlind);

        private void ReceiveElevatorActivation(ElevatorActivation p_elevatorActivation) => OnReceiveElevatorActivation?.Invoke(p_elevatorActivation);
        
        /// <summary/> The function called when the client receives a message of type InitialData
        /// <param name="p_initialData"> The message sent by the Server to the Client </param>
        private void ReceiveInitialData(InitialData p_initialData) => OnReceiveInitialData?.Invoke(p_initialData);
    
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
        private void ReceiveLaser(Laser p_laser) {

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
    
        /// <summary/> The function called when the client receives a message of type SpawnBeacon
        /// <param name="p_spawnBeacon"> The message sent by the Server to the Client </param>
        private void ReceiveSpawnBeacon(SpawnBeacon p_spawnBeacon) => OnReceiveSpawnBeacon?.Invoke(p_spawnBeacon); 
    
        /// <summary/> The function called when the client receives a message of type DestroyedBeacon
        /// <param name="p_destroyedBeacon"> The message sent by the Server to the Client </param>
        private void ReceiveDestroyedBeacon(DestroyedBeacon p_destroyedBeacon) => OnReceiveDestroyedBeacon?.Invoke(p_destroyedBeacon);
    
        /// <summary/> The function called when the client receives a message of type BeaconDetectionUpdate
        /// <param name="p_beaconDetectionUpdate"> The message sent by the Server to the Client </param>
        private void ReceiveBeaconDetectionUpdate(BeaconDetectionUpdate p_beaconDetectionUpdate) => OnReceiveBeaconDetectionUpdate?.Invoke(p_beaconDetectionUpdate);
    
        /// <summary/> The function called when the client receives a message of type ActivateBeacon
        /// <param name="p_activateBeacon"> The message sent by the Server to the Client </param>
        private void ReceiveActivateBeacon(ActivateBeacon p_activateBeacon) => OnReceiveActivateBeacon?.Invoke(p_activateBeacon);

        /// <summary/> The function called when the client receives a message of type GameEnd
        /// <param name="p_gameEnd"> The message sent by the Server to the Client </param>
        private void ReceiveGameEnd(GameEnd p_gameEnd) => OnReceiveGameEnd?.Invoke(p_gameEnd);
    
        /// <summary/> The function called when the client receives a message of type SpawnBeacon
        /// <param name="p_spawnBomb"> The message sent by the Server to the Client </param>
        private void ReceiveSpawnBomb(SpawnBomb p_spawnBomb) {
            //Debug.LogWarning("Bomb supposed to spawn");
            OnReceiveSpawnBomb?.Invoke(p_spawnBomb);
        }
    
        /// <summary/> The function called when the client receives a message of type BombExplosion
        /// <param name="p_bombExplosion"> The message sent by the Server to the Client </param>
        private void ReceiveBombExplosion(BombExplosion p_bombExplosion) => OnReceiveBombExplosion?.Invoke(p_bombExplosion); 
        
        private void ReceivePotentialSpawnPoints(PotentialSpawnPoints p_potentialSpawnPoints) => OnReceivePotentialSpawnPoints?.Invoke(p_potentialSpawnPoints);
        
        private void ReceiveInitiateLobby(InitiateLobby p_initiateLobby) {
            m_isInRealGame = false;
            OnReceiveInitiateLobby?.Invoke(p_initiateLobby);
        }


        private IEnumerator ForceConnect() {
            while (!m_canSend) {
                CustomConnect();
                yield return new WaitForSeconds(m_autoconnexionTimeInterval);
            }
        }

        /// <summary>
        /// Connects to the default serialized ip
        /// (can be set to localhost)
        /// </summary>
        [ContextMenu("Custom Connect")]
        public void CustomConnect() {
            networkAddress = m_localHost ? "localhost" : m_IPadress;
            StartClient();
        }
        
        void ReadTextFile() {
#if UNITY_EDITOR
            StreamReader inp_stm = new StreamReader("Assets/StreamingAssets/ip.txt");
#else
            StreamReader inp_stm = new StreamReader(Application.dataPath + "/StreamingAssets/ip.txt");
#endif

            while(!inp_stm.EndOfStream) {
                string inp_ln = inp_stm.ReadLine( );

                m_IPadress = inp_ln;
            }

            inp_stm.Close( );  
        }
    }

}