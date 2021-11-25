using Mirror;
using UnityEngine;
using CustomMessages;


/// <summary>
/// The client side manager will handle all of the client side network dealing of the game
/// </summary>

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
    
    /// <summary> The delegate that will be called each time the server sends a GameEnd message </summary>
    public delegate void GameEndDelegator(GameEnd p_gameEnd);
    public static GameEndDelegator OnReceiveGameEnd;
    
    #endregion
    
    [SerializeField][Tooltip("The player Object to be enabled on connection")] private GameObject m_player;
    
    private void Awake()
    {
        // Disabling the player to 
        m_player.SetActive(false);

        // if the singleton hasn't been initialized yet
        if (singleton != null && singleton != this)
        {
            gameObject.SetActive(false);
            Debug.LogWarning("BROOOOOOOOOOOOOOOOOOO ! There are too many Singletons broda", this);
        }else singleton = this;

        MyNetworkManager.del_onConnectAsClient += StartClient;
    }
    
    private void Start()
    {
        NetworkClient.RegisterHandler<VrTransform>(ReceiveVrTransform);
        NetworkClient.RegisterHandler<Laser>(ReceiveLaser);
        NetworkClient.RegisterHandler<PcInvisibility>(ReceiveInvisibility);
        NetworkClient.RegisterHandler<ClientConnect>(ReceiveClientConnect);
        NetworkClient.RegisterHandler<HeartTransforms>(ReceiveHeartTranforms);
        NetworkClient.RegisterHandler<HeartBreak>(ReceiveHeartBreak);
        NetworkClient.RegisterHandler<BeaconsPositions>(ReceiveBeaconsPositions);
        NetworkClient.RegisterHandler<DestroyedBeacon>(ReceiveDestroyedBeacon);
        NetworkClient.RegisterHandler<BeaconDetectionUpdate>(ReceiveBeaconDetectionUpdate);
        NetworkClient.RegisterHandler<GameEnd>(ReceiveGameEnd);
    }



    private void StartClient()
    {
        m_player.SetActive(true);
    }
    #region CLIENT HANDLERS

    private void ReceiveGameEnd(GameEnd p_gameEnd) => OnReceiveGameEnd?.Invoke(p_gameEnd);
    private void ReceiveBeaconDetectionUpdate(BeaconDetectionUpdate p_beaconDetectionUpdate) => OnReceiveBeaconDetectionUpdate?.Invoke(p_beaconDetectionUpdate);

    /// <summary>
    /// The function called when the client receives a message of type HeartTransforms
    /// </summary>
    /// <param name="p_heartTransforms"> The message sent by the Server to the Client </param>
    private void ReceiveHeartTranforms(HeartTransforms p_heartTransforms) => OnReceiveHeartTransforms?.Invoke(p_heartTransforms); 
    
    /// <summary>
    /// The function called when the client receives a message of type HeartTransforms
    /// </summary>
    /// <param name="p_heartBreak"> The message sent by the Server to the Client </param>
    private void ReceiveHeartBreak(HeartBreak p_heartBreak) => OnReceiveHeartBreak?.Invoke(p_heartBreak); 
    
    
    /// <summary>
    /// The function called when the client receives a message of type ClientConnect
    /// </summary>
    /// <param name="p_clientConnect"> The message sent by the Server to the Client </param>
    private void ReceiveClientConnect(ClientConnect p_clientConnect) => NetworkClient.Send(new ClientConnect(){client = ClientConnection.PcPlayer});
    
    /// <summary>
    /// The function called when the client receives a message of type VrTransform
    /// </summary>
    /// <param name="p_vrTransform"> The message sent by the Server to the Client </param>
    private void ReceiveVrTransform(VrTransform p_vrTransform) => OnReceiveVrTransform?.Invoke(p_vrTransform);

    
    /// <summary>
    /// The function called when the client receives a message of type Laser
    /// </summary>
    /// <param name="p_laser"> The message sent by the Server to the Client </param>
    private void ReceiveLaser(Laser p_laser)
    {
        switch (p_laser.laserState)
        {
            case LaserState.Shooting:
        
                OnReceiveLaser?.Invoke(p_laser);
                break;
            
            default:
                
                OnReceiveLaserPreview?.Invoke(p_laser);
                break; //Turn off or on the aim
        }
    }
    
    
    /// <summary>
    /// The function called when the client receives a message of type VrTransform
    /// </summary>
    /// <param name="p_pcInvisibility"> The message sent by the Server to the Client </param>
    private void ReceiveInvisibility(PcInvisibility p_pcInvisibility) => OnReceiveInvisibility?.Invoke(p_pcInvisibility);
    
    /// <summary>
    /// The function called when the client receives a message of type BeaconsPositions
    /// </summary>
    /// <param name="p_beaconsPositions"> The message sent by the Server to the Client </param>
    private void ReceiveBeaconsPositions(BeaconsPositions p_beaconsPositions) => OnReceiveBeaconsPositions?.Invoke(p_beaconsPositions); 
    
    /// <summary>
    /// The function called when the client receives a message of type DestroyedBeacon
    /// </summary>
    /// <param name="p_destroyedBeacon"> The message sent by the Server to the Client </param>
    private void ReceiveDestroyedBeacon(DestroyedBeacon p_destroyedBeacon) => OnReceiveDestroyedBeacon?.Invoke(p_destroyedBeacon);

    #endregion
}
