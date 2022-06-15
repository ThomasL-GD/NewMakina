using Mirror;
using UnityEngine;
using CustomMessages;
using Synchronizers;


/// <summary/> The client side manager will handle all of the client side network dealing of the game

public class ClientManager : MonoBehaviour
{
    //Singleton time ! 	(｡◕‿◕｡)
    public static ClientManager singleton { get; private set; }

    #region Delegates
    // (ᵔᴥᵔ)
    
    
    /// <summary/> The delegate that will be called each time the VR player's position is updated 
    public delegate void VrTransformDelegator(VrTransform p_vrTransform);
    public static VrTransformDelegator OnReceiveVrTransform;
    
    /// <summary/> The delegate that will be called each time a laser is called position is updated 
    public delegate void LaserDelegator(Laser p_laser);
    public static LaserDelegator OnReceiveLaser;
    public static LaserDelegator OnReceiveLaserPreview;
    
    /// <summary/> The delegate that will be called each time the pc player's invisibility is updated
    public delegate void InvisibilityDelegator(PcInvisibility p_pcInvisibility);
    public static InvisibilityDelegator OnReceiveInvisibility;

    public delegate void HeartBreakDelegator(HeartBreak p_heartBreak);
    public static HeartBreakDelegator OnReceiveHeartBreak;
    
    /// <summary/> The delegates that will be called each time the server updates the beacons 
    public delegate void BeaconsPositionsDelegator(BeaconsPositions p_beaconsPositions);
    public static BeaconsPositionsDelegator OnReceiveBeaconsPositions;
    
    /// <summary/> The delegates that will be called each time the server updates the bombs 
    public delegate void BombsPositionsDelegator(BombsPositions p_bombsPositions);
    public static BombsPositionsDelegator OnReceiveBombsPositions;
    
    /// <summary/> The delegates that will be called each time the server spawns a bomb
    public delegate void SpawnBombDelegator(SpawnBomb p_spawnBomb);
    public static SpawnBombDelegator OnReceiveSpawnBomb;
    
    public delegate void DestroyedBeaconsDelegator(DestroyedBeacon p_beaconsPositions);
    public static DestroyedBeaconsDelegator OnReceiveDestroyedBeacon;

    public delegate void BeaconDetectionUpdateDelegator(BeaconDetectionUpdate p_beaconsPositions);
    public static BeaconDetectionUpdateDelegator OnReceiveBeaconDetectionUpdate;
    
    /// <summary> The delegate that will be called each time the server sends a GameEnd message </summary>
    public delegate void GameEndDelegator(GameEnd p_gameEnd);
    public static GameEndDelegator OnReceiveGameEnd;
    
    /// <summary> The delegate that will be called each time the server sends a InitialData message </summary>
    public delegate void InitialDataDelegator(InitialData p_initialData);

    public static InitialDataDelegator OnReceiveInitialData;
    
    /// <summary> The delegate that will be called each time the server sends a SpawnBeacon message </summary>
    public delegate void SpawnBeaconDelegator(SpawnBeacon p_spawnBeacon);
    public static SpawnBeaconDelegator OnReceiveSpawnBeacon;
    
    /// <summary> The delegate that will be called each time the server sends a ActivateBeacon message </summary>
    public delegate void SpawnActivateBeacon(ActivateBeacon p_activateBeacon);
    public static SpawnActivateBeacon OnReceiveActivateBeacon;
    
    /// <summary> The delegate that will be called each time the server sends a BombExplosion message </summary>
    public delegate void BombExplosionDelegator(BombExplosion p_activateBeacon);
    public static BombExplosionDelegator OnReceiveBombExplosion;
    
    /// <summary/> The delegate that will be called each time the client receives a BombActivation message
    public delegate void BombActivationDelegator(BombActivation p_bombActivation);
    public static BombActivationDelegator OnReceiveBombActivation;   
    
    /// <summary/> The delegate that will be called each time the client receives a ElevatorActivation message
    public delegate void ElevatorActivationDelegator(ElevatorActivation p_bombActivation);
    public static ElevatorActivationDelegator OnReceiveElevatorActivation;
        
    /// <summary/> The delegate that will be called each time the client receives a ActivateFlair message
    public delegate void ActivateFlairDelegator(ActivateFlair p_activateFlair);
    public static ActivateFlairDelegator OnReceiveActivateFlair;
        
    /// <summary/> The delegate that will be called each time the client receives a DeActivateBlind message
    public delegate void DeActivateBlindDelegator(DeActivateBlind p_deActivateBlind);
    public static DeActivateBlindDelegator OnReceiveDeActivateBlind;
        
    /// <summary/> The delegate that will be called each time the client receives a ActivateBlind message
    public delegate void ActivateBlindDelegator(ActivateBlind p_activateBlind);
    public static ActivateBlindDelegator OnReceiveActivateBlind;
        
    /// <summary/> The delegate that will be called each time the client receives a DestroyLeure message
    public delegate void ReadyDelegator(ReadyToFace p_readyToFace);
    public static ReadyDelegator OnReceiveReadyToFace;
    
    /// <summary/> The delegate that will be called each time the client receives a DestroyLeure message
    public delegate void ReadyToGoIntoTheBowlDelegator(ReadyToGoIntoTheBowl p_readyToGoIntoTheBowl);
    public static ReadyToGoIntoTheBowlDelegator OnReceiveReadyToGoIntoTheBowl;
        
    /// <summary/> The delegate that will be called each time the client receives a DestroyLeure message
    public delegate void DestoyLeureDelegator(DestroyLeure p_activateBlind);
    public static DestoyLeureDelegator OnReceiveDestroyLeure;
    
    /// <summary/> The delegate that will be called each time the client receives a HeartConquerStop message
    public delegate void HeartConquerStopDelegator(HeartConquerStop p_heartConquerStop);
    public static HeartConquerStopDelegator OnReceiveHeartConquerStop;

    /// <summary/> The delegate that will be called each time the client receives a HeartConquerStart message
    public delegate void ReceiveHeartConquerStartDelegator(HeartConquerStart p_heartConquerStart);
    public static ReceiveHeartConquerStartDelegator OnReceiveHeartConquerStart;

    /// <summary/> The delegate that will be called each time the client receives a HeartConquerStart message
    public delegate void ReceiveVrInitialValuesDelegator(VrInitialValues p_vrInitialValues);
    public static ReceiveVrInitialValuesDelegator OnReceiveVrInitialValues;
    
    /// <summary/> The delegate that will be called each time the client receives a InitiateLobby message
    public delegate void InitiateLobbyDelegator(InitiateLobby p_heartConquerStop);
    public static InitiateLobbyDelegator OnReceiveInitiateLobby;
    
    #endregion
    [SerializeField, Tooltip("The player Object to get the player's location")] public GameObject m_playerObject;

    /// <summary>Becomes true when the game starts and is false during th menuing</summary>
    [HideInInspector] public bool m_isInGame = false;
    
    
    /// <summary/> Singleton type beat
    private void Awake()
    {

        // if the singleton hasn't been initialized yet
        if (singleton != null && singleton != this)
        {
            gameObject.SetActive(false);
            Debug.LogWarning("BROOOOOOOOOOOOOOOOOOO ! There are too many Singletons broda", this);
        }else singleton = this;
    }
    
    /// <summary/> Registering all the client side handlers
    private void Start()
    {
        NetworkClient.RegisterHandler<VrTransform>(ReceiveVrTransform);
        NetworkClient.RegisterHandler<Laser>(ReceiveLaser);
        NetworkClient.RegisterHandler<PcInvisibility>(ReceiveInvisibility);
        NetworkClient.RegisterHandler<ClientConnect>(ReceiveClientConnect);
        NetworkClient.RegisterHandler<HeartBreak>(ReceiveHeartBreak);
        NetworkClient.RegisterHandler<BeaconsPositions>(ReceiveBeaconsPositions);
        NetworkClient.RegisterHandler<DestroyedBeacon>(ReceiveDestroyedBeacon);
        NetworkClient.RegisterHandler<BeaconDetectionUpdate>(ReceiveBeaconDetectionUpdate);
        NetworkClient.RegisterHandler<GameEnd>(ReceiveGameEnd);
        NetworkClient.RegisterHandler<InitialData>(ReceiveInitialData);
        NetworkClient.RegisterHandler<SpawnBeacon>(ReceiveSpawnBeacon);
        NetworkClient.RegisterHandler<ActivateBeacon>(ReceiveActivateBeacon);
        NetworkClient.RegisterHandler<BombExplosion>(ReceiveBombExplosion);
        NetworkClient.RegisterHandler<BombsPositions>(ReceiveBombsPositions);
        NetworkClient.RegisterHandler<SpawnBomb>(ReceiveSpawnBomb);
        NetworkClient.RegisterHandler<BombActivation>(ReceiveBombActivation);
        NetworkClient.RegisterHandler<ElevatorActivation>(ReceiveElevatorActivation);
        NetworkClient.RegisterHandler<ActivateFlair>(ReceiveActivateFlair);
        NetworkClient.RegisterHandler<ActivateBlind>(ReceiveActivateBlind);
        NetworkClient.RegisterHandler<DeActivateBlind>(ReceiveDeActivateBlind);
        NetworkClient.RegisterHandler<ReadyToFace>(ReceiveReadyMessage);
        NetworkClient.RegisterHandler<ReadyToGoIntoTheBowl>(ReceiveReadyToGoIntoTheBowl);
        NetworkClient.RegisterHandler<DestroyLeure>(ReceiveDestroyLeure);
        NetworkClient.RegisterHandler<HeartConquerStart>(ReceiveHeartConquerStart);
        NetworkClient.RegisterHandler<HeartConquerStop>(ReceiveHeartConquerStop);
        NetworkClient.RegisterHandler<VrInitialValues>(ReceiveVrInitialValues);
        NetworkClient.RegisterHandler<InitiateLobby>(ReceiveInitiateLobby);
    }

    private void ReceiveReadyToGoIntoTheBowl(ReadyToGoIntoTheBowl p_readyToGoIntoBowl)=>OnReceiveReadyToGoIntoTheBowl?.Invoke(p_readyToGoIntoBowl);

    private void ReceiveInitiateLobby(InitiateLobby p_initiateLobby)=>OnReceiveInitiateLobby?.Invoke(p_initiateLobby);

    private void ReceiveHeartConquerStop(HeartConquerStop p_heartConquerStop)=>OnReceiveHeartConquerStop?.Invoke(p_heartConquerStop);

    private void ReceiveHeartConquerStart(HeartConquerStart p_heartConquerStart)=>OnReceiveHeartConquerStart?.Invoke(p_heartConquerStart);

    private void ReceiveDestroyLeure(DestroyLeure p_destroyLeureMessage) => OnReceiveDestroyLeure?.Invoke(p_destroyLeureMessage);

    private void ReceiveReadyMessage(ReadyToFace p_readyMessage) => OnReceiveReadyToFace?.Invoke(p_readyMessage);
    

    private void ReceiveActivateFlair(ActivateFlair p_activateFlair) => OnReceiveActivateFlair?.Invoke(p_activateFlair);
    
    private void ReceiveActivateBlind(ActivateBlind p_activateBlind) => OnReceiveActivateBlind?.Invoke(p_activateBlind);

    private void ReceiveDeActivateBlind(DeActivateBlind p_deActivteBlind) => OnReceiveDeActivateBlind?.Invoke(p_deActivteBlind);

    private void ReceiveElevatorActivation(ElevatorActivation p_elevatorActivation) => OnReceiveElevatorActivation?.Invoke(p_elevatorActivation);
    
    private void ReceiveBombActivation(BombActivation p_bombActivation) => OnReceiveBombActivation?.Invoke(p_bombActivation);

    private void ReceiveActivateBeacon(ActivateBeacon p_activateBeacon) => OnReceiveActivateBeacon?.Invoke(p_activateBeacon);

    private void ReceiveSpawnBeacon(SpawnBeacon p_spawnBeacon) => OnReceiveSpawnBeacon?.Invoke(p_spawnBeacon);
    #region CLIENT HANDLERS

    /// <summary/> The function called when the client receives a message of type InitialData
    /// <param name="p_initialData"> The message sent by the Server to the Client </param>
    private void ReceiveInitialData(InitialData p_initialData) {
        m_isInGame = true;
        Debug.LogWarning("$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$ INITIAL DATA");
        OnReceiveInitialData?.Invoke(p_initialData);
    }


    /// <summary/> The function called when the client receives a message of type GameEnd
    /// <param name="p_gameEnd"> The message sent by the Server to the Client </param>
    private void ReceiveGameEnd(GameEnd p_gameEnd) {
        m_isInGame = false;
        Debug.LogWarning("$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$ END GAME");
        OnReceiveGameEnd?.Invoke(p_gameEnd);
    }


    /// <summary/> The function called when the client receives a message of type BeaconDetectionUpdate
    /// <param name="p_beaconDetectionUpdate"> The message sent by the Server to the Client </param>
    private void ReceiveBeaconDetectionUpdate(BeaconDetectionUpdate p_beaconDetectionUpdate) => OnReceiveBeaconDetectionUpdate?.Invoke(p_beaconDetectionUpdate);


    /// <summary/> The function called when the client receives a message of type HeartTransforms
    /// <param name="p_heartBreak"> The message sent by the Server to the Client </param>
    private void ReceiveHeartBreak(HeartBreak p_heartBreak) => OnReceiveHeartBreak?.Invoke(p_heartBreak); 
    
    
    /// <summary/> The function called when the client receives a message of type ClientConnect
    /// <param name="p_clientConnect"> The message sent by the Server to the Client </param>
    private void ReceiveClientConnect(ClientConnect p_clientConnect) => NetworkClient.Send(new ClientConnect(){client = ClientConnection.PcPlayer});
    
    
    /// <summary/> The function called when the client receives a message of type VrTransform
    /// <param name="p_vrTransform"> The message sent by the Server to the Client </param>
    private void ReceiveVrTransform(VrTransform p_vrTransform) => OnReceiveVrTransform?.Invoke(p_vrTransform);
    
    
    /// <summary/> The function called when the client receives a message of type Laser
    /// <param name="p_laser"> The message sent by the Server to the Client </param>
    private void ReceiveLaser(Laser p_laser)
    {
        switch (p_laser.laserState)
        {
            case LaserState.Shooting:
        
                OnReceiveLaser?.Invoke(p_laser);
                if (p_laser.hit) SynchronizeRespawn.OnPlayerDeath?.Invoke();
                break;
            
            default:
                
                OnReceiveLaserPreview?.Invoke(p_laser);
                break; //Turn off or on the aim
        }
    }
    
    
    /// <summary/> The function called when the client receives a message of type BombExplosion
    /// <param name="p_bombExplosion"> The message sent by the Server to the Client </param>
    private void ReceiveBombExplosion(BombExplosion p_bombExplosion) {
        
        OnReceiveBombExplosion?.Invoke(p_bombExplosion); //TODO : put something in this
        if (p_bombExplosion.hit) SynchronizeRespawn.OnPlayerDeath?.Invoke();
    }
    
    
    /// <summary/> The function called when the client receives a message of type VrTransform
    /// <param name="p_pcInvisibility"> The message sent by the Server to the Client </param>
    private void ReceiveInvisibility(PcInvisibility p_pcInvisibility) => OnReceiveInvisibility?.Invoke(p_pcInvisibility);
    
    
    /// <summary/>The function called when the client receives a message of type BeaconsPositions
    /// <param name="p_beaconsPositions"> The message sent by the Server to the Client </param>
    private void ReceiveBeaconsPositions(BeaconsPositions p_beaconsPositions) => OnReceiveBeaconsPositions?.Invoke(p_beaconsPositions); 
    
    
    /// <summary/>The function called when the client receives a message of type BombsPositions
    /// <param name="p_bombsPositions"> The message sent by the Server to the Client </param>
    private void ReceiveBombsPositions(BombsPositions p_bombsPositions) => OnReceiveBombsPositions?.Invoke(p_bombsPositions);
    
    
    /// <summary/>The function called when the client receives a message of type SpawnBomb
    /// <param name="p_spawnBomb"> The message sent by the Server to the Client </param>
    private void ReceiveSpawnBomb(SpawnBomb p_spawnBomb) => OnReceiveSpawnBomb?.Invoke(p_spawnBomb);
    
    
    /// <summary/> The function called when the client receives a message of type DestroyedBeacon
    /// <param name="p_destroyedBeacon"> The message sent by the Server to the Client </param>
    private void ReceiveDestroyedBeacon(DestroyedBeacon p_destroyedBeacon) => OnReceiveDestroyedBeacon?.Invoke(p_destroyedBeacon);
    
    
    /// <summary/> The function called when the client receives a message of type VrInitialValues
    /// <param name="p_vrInitialValues"> The message sent by the Server to the Client </param>
    private void ReceiveVrInitialValues(VrInitialValues p_vrInitialValues) => OnReceiveVrInitialValues?.Invoke(p_vrInitialValues);

    #endregion
}
