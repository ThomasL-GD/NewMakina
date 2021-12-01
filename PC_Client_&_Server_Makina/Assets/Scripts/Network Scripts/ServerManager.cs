using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using CustomMessages;

/// <summary>
/// The server side manager will handle all of the server side network dealings of the game
/// </summary>

public class ServerManager : MonoBehaviour
{
    //Singleton time ! (╬ ಠ益ಠ)
    public static ServerManager singleton { get; private set; }
    
    /// <summary/> The delegate that will be called on each server Ticks
    private delegate void OnSendDataToClients();
    private OnSendDataToClients onSendDataToClients;

    #region Buffers

    private VrTransform m_vrTransformBuffer = new VrTransform();
    private PcTransform m_pcTransformBuffer = new PcTransform();
    private Laser m_laserBuffer = new Laser();
    private PcInvisibility m_pcInvisibilityBuffer = new PcInvisibility();
    private HeartBreak m_heartBreakBuffer = new HeartBreak();
    private BeaconsPositions m_beaconsPositionsBuffer = new BeaconsPositions();
    private DestroyedBeacon m_destroyedBeaconBuffer = new DestroyedBeacon();
    private VrPlayerInteractWithBeacon m_vrPlayerInteractWithBeaconBuffer = new VrPlayerInteractWithBeacon();
    #endregion

    
    #region Server Data


    [Header("Server Settings")]
    [SerializeField][Range(1,120)][Tooltip("the server's tick rate in Hz")] private int m_tickrate = 30;
    private float m_tickDelta = 1f;
    
    /// <summary/> The custom variables added onto the Network Manager
    [Header("Laser :")]
    [Header("Game Settings")]
    [SerializeField][Tooltip("The radius of the VR laser")] private float m_laserRadius = 20f;
    [SerializeField][Tooltip("The charge time of the VR laser")] private float m_laserChargeTime = 1f;
    

    [Header("Hearts")]
    [Header(" ")]
    [SerializeField][Tooltip("The positions of the hearts on the map")] private Transform[] m_heartTransforms;
    [SerializeField][Tooltip("The amount of hearts that need to be destroyed for the VR player to lose")] private int m_vrPlayerHealth = 3;
    [SerializeField][Tooltip("The amount of times the PC player has to eliminated to lose")] private int m_pcPlayerHealth = 3;
    [SerializeField][Tooltip("The possible spawn positions of the PC player on the map")] private Transform[] m_beaconSpawnPositions;

    
    [Header("Beacons")]
    [Header(" ")]
    [SerializeField][Tooltip("The interval at which the beacons spawn")] private int m_beaconRespawnTime = 30;
    [SerializeField][Tooltip("The lifetime of a beacon")] private int m_beaconLifeTime = 10;
    [SerializeField][Tooltip("The range of the beacons")] private float m_beaconRange = 400;
    
    /// <summary/> The list of beacons that detected the PC player
    private List<bool> m_playerDetected = new List<bool>();
    
    /// <summary/> The positions of the hearts
    private Vector3[] m_heartPositions;
    
    /// <summary/> The rotations of the hearts
    private Quaternion[] m_heartRotations;
    
    /// <summary/> The connection adresses of the players
    private NetworkConnection m_vrNetworkConnection = null;
    private NetworkConnection m_pcNetworkConnection = null;

    #endregion
    
    /// <summary>
    /// Awake is called when the script is enabled
    /// Here we use it to check the singleton conditions and to add functions to the delegates
    /// </summary>
    private void Awake()
    {
        // if the singleton hasn't been initialized yet 
        
        if (singleton != null && singleton != this)
        {
            gameObject.SetActive(false);
            Debug.LogWarning("BROOOOOOOOOOOOOOOOOOO ! There are too many Singletons broda", this);
        }else singleton = this;
        
        //linking NetworkManager functions
        MyNetworkManager.del_onHostServer += OnStartServer;
        MyNetworkManager.del_onClientConnection += OnServerConnect;
        
        //Setting up senders
        onSendDataToClients += SendVrTransform;
        onSendDataToClients += SendPcTransform;
        onSendDataToClients += SendLaser;
        onSendDataToClients += SendPcInvisbility;
        onSendDataToClients += SendBeaconsPositions;
        onSendDataToClients += BeaconDetectionCheck;
        onSendDataToClients += CheckHealths;
    }
    
    
    /// <summary>
    /// Start is called before the first update loop
    /// Here we use it to set the phase of the tickrate so that it can't be changed during runtime
    /// We also use it to register the servers handlers / buffers
    /// </summary>
    private void Start()
    {
        // making the tick delta constant
        m_tickDelta = 1 / m_tickrate;
        
        // Setting up buffers
        NetworkServer.RegisterHandler<VrTransform>(OnServerReceiveVrTransforms);
        NetworkServer.RegisterHandler<PcTransform>(OnServerReceivePctransform);
        NetworkServer.RegisterHandler<VrLaser>(OnServerReceiveVrLaser);
        NetworkServer.RegisterHandler<PcInvisibility>(OnServerReceiveInvisibility);
        NetworkServer.RegisterHandler<ClientConnect>(OnServerReceiveClientConnect);
        NetworkServer.RegisterHandler<HeartBreak>(OnServerReceiveHeartBreak);
        NetworkServer.RegisterHandler<BeaconsPositions>(OnServerReceiveBeaconsPositions);
        NetworkServer.RegisterHandler<DestroyedBeacon>(OnServerReceiveDestroyedBeacon);

        //Unpacking the heart position values from the transforms to send through messages
        List<Vector3> heartPositions = new List<Vector3>();
        List<Quaternion> heartRotations = new List<Quaternion>();

        foreach (Transform pos in m_heartTransforms)
        {
            heartPositions.Add(pos.position);
            heartRotations.Add(pos.rotation);
        }

        m_heartPositions = heartPositions.ToArray();
        m_heartRotations = heartRotations.ToArray();
        
        
        if (m_vrPlayerHealth > m_heartTransforms.Length) Debug.LogWarning("the Vr Player has more health than there are hearts... Big L?",this);
    }

    /// <summary/> The Server Loop
    IEnumerator ServerTick()
    {
        while (NetworkServer.active)
        {
            onSendDataToClients?.Invoke();
            yield return new WaitForSeconds(m_tickDelta);
        }
    }

    
    /// <summary/> Spawns the beacons
    IEnumerator SpawnBeacon()
    {
        while (NetworkServer.active)
        {
            yield return new WaitForSeconds(m_beaconRespawnTime);
            
            List<Vector3> beaconPositionList = new List<Vector3>();
            
            if(m_beaconsPositionsBuffer.positions != null) beaconPositionList = m_beaconsPositionsBuffer.positions.ToList();
            
            beaconPositionList.Add(m_beaconSpawnPositions[Random.Range(0,m_beaconSpawnPositions.Length)].position);
            m_beaconsPositionsBuffer.positions = beaconPositionList.ToArray();
            // Debug.Log($"spawn : { m_beaconsPositionsBuffer.positions.Length}");
            m_playerDetected.Add(false);
            if(m_beaconDestroyIndexModification.Count < m_beaconsPositionsBuffer.positions.Length)
                m_beaconDestroyIndexModification.Add(0);
            StartCoroutine(DespawnBeacon(m_beaconsPositionsBuffer.positions.Length-1));
            m_beaconDestroyIndexModification[m_beaconsPositionsBuffer.positions.Length - 1] = 0;
        }
    }

    private List<int> m_beaconDestroyIndexModification = new List<int>();

    /// <summary/> Despawns the beacon at the selected index with an offset defined by the previous despawns
    /// <param name="p_index"/> the selected index
    IEnumerator DespawnBeacon(int p_index)
    {
        int index = p_index - m_beaconDestroyIndexModification[p_index];
        yield return new WaitForSeconds(m_beaconLifeTime);
        List<Vector3> poses = m_beaconsPositionsBuffer.positions.ToList();
        poses.RemoveAt(index);
        m_playerDetected.RemoveAt(index);
        
        m_beaconsPositionsBuffer.positions = poses.ToArray();
        SendToBothClients(new DestroyedBeacon(){index = index});

        for (int i = index+1; i < m_beaconDestroyIndexModification.Count; i++)
            m_beaconDestroyIndexModification[i]++;
    }

    #region SERVER ACTIONS

    
    /// <summary/> The function that is called when the server is hosted.
    private void OnStartServer()
    {
        StartCoroutine(SpawnBeacon());
        StartCoroutine(ServerTick());
    }


    /// <summary>
    /// The function that is called each time a player connects to the server.
    /// We assume that the latest player to connect is the VR player.
    /// </summary>
    /// <param name="p_conn"> The NetworkConnection of the newly connected client </param>
    private void OnServerConnect(NetworkConnection p_conn)
    {
        p_conn.Send( new ClientConnect());
    }

    
    /// <summary/>The function that will be called to check the detection of all the beacons
    private void BeaconDetectionCheck()
    {
        if (m_beaconsPositionsBuffer.positions == null) return;
        for (int i = 0; i < m_beaconsPositionsBuffer.positions.Length; i++)
        {
            bool detected;
            detected = Vector3.Distance(m_pcTransformBuffer.position, m_beaconsPositionsBuffer.positions[i]) < m_beaconRange;
            if (m_playerDetected[i] != detected)
            {
                m_playerDetected[i] = detected;
                SendToBothClients(new BeaconDetectionUpdate(){playerDetected = detected, index = i});
            }
        }
    }

    /// <summary/> the function called to check the winning conditions
    private void CheckHealths()
    {
        if (m_pcPlayerHealth <= 0)
        {
            SendToBothClients(new GameEnd(){winningClient = ClientConnection.VrPlayer});
            return;
        }
        
        if (m_vrPlayerHealth <= 0)
            SendToBothClients(new GameEnd(){winningClient = ClientConnection.PcPlayer});
    }
    
    #endregion
    

    #region SERVER BUFFERS


    /// <summary>
    /// function called when the server receives a message of type ClientConnect
    /// </summary>
    /// <param name="p_conn"> The connection from which originated the message </param>
    /// <param name="p_vrTransform"> The message sent by the Client to the Server  </param>
    private void OnServerReceiveClientConnect(NetworkConnection p_conn, ClientConnect client)
    {
        if (client.client == ClientConnection.VrPlayer) m_vrNetworkConnection = p_conn;
        else if (client.client == ClientConnection.PcPlayer) m_pcNetworkConnection = p_conn;

        p_conn.Send(new HeartTransforms(){positions = m_heartPositions, rotations = m_heartRotations});

        
        InitialData initialData = new InitialData() {
            healthPcPlayer = m_pcPlayerHealth,
            healthVrPlayer = m_vrPlayerHealth,
            beaconRange = m_beaconRange
        };
        
        p_conn.Send(initialData);
        
        if(m_beaconsPositionsBuffer.positions != null)p_conn.Send(m_beaconsPositionsBuffer);
    }

    /// <summary>
    /// function called when the server receives a message of type VrTransform
    /// </summary>
    /// <param name="p_conn"> The connection from which originated the message </param>
    /// <param name="p_vrTransform"> The message sent by the Client to the Server  </param>
    private void OnServerReceiveVrTransforms(NetworkConnection p_conn,VrTransform p_vrTransform)
    {
        m_vrTransformBuffer = p_vrTransform;
    }

    /// <summary>
    /// function called when the server receives a message of type PcTransform
    /// </summary>
    /// <param name="p_conn"> The connection from which originated the message </param>
    /// <param name="p_pcTransform"> The message sent by the Client to the Server  </param>
    private void OnServerReceivePctransform(NetworkConnection p_conn,PcTransform p_pcTransform)
    {
        m_pcTransformBuffer = p_pcTransform;
    }

    /// <summary>
    /// function called when the server receives a message of type VrLaser
    /// it calculate whether the laser hit the pc player then sends the result back
    /// it calculates this by checking whether the player's pivot point is within the range of the laser's radius 
    /// </summary>
    /// <param name="p_conn"> The connection from which originated the message </param>
    /// <param name="p_vrlaser"> The message sent by the Client to the Server </param>
    private void OnServerReceiveVrLaser(NetworkConnection p_conn, VrLaser p_vrlaser)
    {
        m_laserBuffer = new Laser();
        m_laserBuffer.laserState = p_vrlaser.laserState;
        
        //TODO bring this verification to the server loop
        
        // If the laser is being aimed forgo the collision calculation and just send the message 
        if (m_laserBuffer.laserState == LaserState.Shooting)
        {

            // Or une parallel line distance operations to check wether the player's pivot point is within the range of the laser's radius

            // Mafs
            Vector3 startingPoint = m_vrTransformBuffer.positionRightHand;
            Vector3 direction = m_vrTransformBuffer.rotationRightHand * Vector3.forward;
            Vector3 playerPos = m_pcTransformBuffer.position;

            // So we measure the distance of the player's position from the line of the laser
            float distance = Vector3.Cross(direction, playerPos - startingPoint).magnitude;

            //if the distance between the line of fire and the player's position is blablablbalalboom... he ded
            bool hit = distance <= m_laserRadius && Vector3.Angle(playerPos - startingPoint, direction) < 90f;

            if (hit) m_pcPlayerHealth--;
            
            // Packing the vallues in a neat little message
            m_laserBuffer.origin = startingPoint;
            m_laserBuffer.rotation = m_vrTransformBuffer.rotationRightHand;
            m_laserBuffer.hitPosition = m_pcTransformBuffer.position;
            m_laserBuffer.hit = hit;
        }
        //preparing to send the message
        onSendDataToClients -= SendLaser;
        onSendDataToClients += SendLaser;
    }

    /// <summary>
    /// function called when the server receives a message of type Invisbility
    /// </summary>
    /// <param name="p_conn"> The connection from which originated the message </param>
    /// <param name="p_pcInvisibility"> The message sent by the Client to the Server  </param>
    private void OnServerReceiveInvisibility(NetworkConnection p_conn, PcInvisibility p_pcInvisibility) {

        m_pcInvisibilityBuffer = p_pcInvisibility;
        onSendDataToClients -= SendPcInvisbility;
        onSendDataToClients += SendPcInvisbility;
    }

    /// <summary>
    /// function called when the server receives a message of type HeartBreak
    /// </summary>
    /// <param name="p_conn"> The connection from which originated the message </param>
    /// <param name="p_heartBreak"> The message sent by the Client to the Server  </param>
    private void OnServerReceiveHeartBreak(NetworkConnection p_conn, HeartBreak p_heartBreak)
    {
        //TODO bring this to the server loop
        
        m_vrPlayerHealth--;
        
        m_heartBreakBuffer = p_heartBreak;
        onSendDataToClients -= SendHeartBreak;
        onSendDataToClients += SendHeartBreak;
    }

    /// <summary>
    /// function called when the server receives a message of type BeaconsPositions
    /// </summary>
    /// <param name="p_conn">The connection from which originated the message</param>
    /// <param name="p_beaconsPositions">The message sent by the Client to the Server</param>
    private void OnServerReceiveBeaconsPositions(NetworkConnection p_conn, BeaconsPositions p_beaconsPositions)
    {
        int length = p_beaconsPositions.positions.Length < m_beaconsPositionsBuffer.positions.Length ? p_beaconsPositions.positions.Length : m_beaconsPositionsBuffer.positions.Length;
        
        for (int i=0; i<length;i++)
            m_beaconsPositionsBuffer.positions[i] = p_beaconsPositions.positions[i];
    }

    /// <summary>
    /// function called when the server receives a message of type DestroyedBeacon
    /// </summary>
    /// <param name="p_conn"> The connection from which originated the message </param>
    /// <param name="p_destroyedBeacon"> The message sent by the Client to the Server  </param>
    private void OnServerReceiveDestroyedBeacon(NetworkConnection p_conn, DestroyedBeacon p_destroyedBeacon) {
        m_destroyedBeaconBuffer = p_destroyedBeacon;
        onSendDataToClients -= SendDestroyedBeacon;
        onSendDataToClients += SendDestroyedBeacon;
    }

    //TODO use this?
    private void OnServerReceiveVrPlayerInteractWithBeacon(NetworkConnection p_conn, VrPlayerInteractWithBeacon p_vrPlayerInteractWithBeacon)
    {
        m_vrPlayerInteractWithBeaconBuffer = p_vrPlayerInteractWithBeacon;
    }
    
    #endregion


    #region SERVER SENDERS

    /// <summary>
    /// The function that send the VrTransform to the pc client
    /// </summary>
    private void SendVrTransform() => m_pcNetworkConnection?.Send(m_vrTransformBuffer);
    
    /// <summary>
    /// The function that send the PcTransform to the vr client
    /// </summary>
    private void SendPcTransform()
    {
        m_vrNetworkConnection?.Send(m_pcTransformBuffer);
    }

    /// <summary>
    /// The function that send the Laser to the clients
    /// </summary>
    private void SendLaser()
    {
        SendToBothClients(m_laserBuffer);
        onSendDataToClients -= SendLaser;
    }

    /// <summary>
    /// The function that send the PcInvisibility to the clients
    /// </summary>
    private void SendPcInvisbility()
    {
        SendToBothClients(m_pcInvisibilityBuffer);
        onSendDataToClients -= SendPcInvisbility;
    }
    
    /// <summary>
    /// The function that send the SendHeartBreak to the clients
    /// </summary>
    private void SendHeartBreak()
    {
        SendToBothClients(m_heartBreakBuffer);
        onSendDataToClients -= SendHeartBreak;
    }

    /// <summary>
    /// The function that send the BeaconsPositions (if not empty) to the pc client
    /// </summary>
    private void SendBeaconsPositions() {
        if(m_beaconsPositionsBuffer.positions != null && m_beaconsPositionsBuffer.positions.Length > 0)
        {
            SendToBothClients(m_beaconsPositionsBuffer);
        }
    }
    
    /// <summary>
    /// The function that send the destroyedBeacon to the clients
    /// </summary>
    private void SendDestroyedBeacon() {
        m_pcNetworkConnection?.Send(m_destroyedBeaconBuffer);
        onSendDataToClients -= SendDestroyedBeacon;
    }
    
    #endregion


    /// <summary>
    /// A function to send a message to both clients
    /// </summary>
    /// <param name="p_msg"> the message </param>
    /// <typeparam name="T"> a parameter that can only be a NetworkMessage struct </typeparam>
    private void SendToBothClients<T>(T p_msg) where T : struct,NetworkMessage
    {
        m_pcNetworkConnection?.Send(p_msg);
        m_vrNetworkConnection?.Send(p_msg);
    }
}