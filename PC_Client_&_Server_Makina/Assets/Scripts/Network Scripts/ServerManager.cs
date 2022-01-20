using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using CustomMessages;
using JetBrains.Annotations;

/// <summary/> The server side manager will handle all of the server side network dealings of the game

public class ServerManager : MonoBehaviour
{
    //Singleton time ! (╬ ಠ益ಠ)
    public static ServerManager singleton { get; private set; }
    
    /// <summary/> The delegate that will be called on each server Ticks
    private delegate void OnSendDataToClients();
    private OnSendDataToClients OnServerTick;

    #region Buffers

    private VrTransform m_vrTransformBuffer = new VrTransform();
    private PcTransform m_pcTransformBuffer = new PcTransform();
    private Laser m_laserBuffer = new Laser();
    private PcInvisibility m_pcInvisibilityBuffer = new PcInvisibility();
    private HeartBreak m_heartBreakBuffer = new HeartBreak();
    private BeaconsPositions m_beaconsPositionsBuffer = new BeaconsPositions();
    private BombsPositions m_bombsPositionsBuffer = new BombsPositions();
    private bool m_newPositions = false;
    private DestroyedBeacon m_destroyedBeaconBuffer = new DestroyedBeacon();
    private ElevatorActivation m_elevatorActivationBuffer;

    #endregion
    
    
    #region Server Data


    [Header("Server Settings")]
    [SerializeField, Range(1,120), Tooltip("the server's tick rate in Hz")] private int m_tickrate = 30;
    private float m_tickDelta = 1f;
    
    /// <summary/> The custom variables added onto the Network Manager
    [Header("Laser :")]
    [Header("Game Settings")]
    [SerializeField, Tooltip("The radius of the VR laser")] private float m_laserRadius = 20f;
    [SerializeField, Tooltip("The speed of the elevators in m/s")] private float m_elevatorSpeed = 5.8f;
    [SerializeField, Tooltip("The speed of the elevators in m/s")] private float m_elevatorWaitTime = 1f;
    

    [Header("Hearts")]
    [Header(" ")]
    [SerializeField, Tooltip("The positions of the hearts on the map")] private Transform[] m_heartTransforms;
    [SerializeField, Tooltip("The amount of hearts that need to be destroyed for the VR player to lose")] private int m_vrPlayerHealth = 3;
    [SerializeField, Tooltip("The amount of times the PC player has to eliminated to lose")] private int m_pcPlayerHealth = 3;
    [SerializeField, Tooltip("The possible spawn positions of the PC player on the map")] private Transform[] m_beaconSpawnPositions;

    
    [Header("Beacons")]
    [Header(" ")]
    [SerializeField, Tooltip("The amount of beacons that will spawn at the start of the game")]private int m_initialBeacons = 2;
    [SerializeField, Tooltip("The time after which the initial beacons will spawn")]private float m_initialBeaconSpawnDelay = 10f;
    [Header(" ")]
    [SerializeField, Tooltip("The interval at which the beacons spawn")] private float m_beaconRespawnTime = 30f;
    [SerializeField, Tooltip("The lifetime of a beacon")] private float m_beaconLifeTime = 10f;
    [SerializeField, Tooltip("The maximum amount of beacons")] private int m_maxBeacons = 3; 
    private int m_currentBeaconAmount = 0;
    [Header(" ")]
    [SerializeField, Tooltip("The range of the beacons")] private float m_beaconRange = 400f;

    [Header(" ")] [Header("Bombs")] [Header(" ")]
    [SerializeField, Tooltip("The interval at which the bomb spawn"), Range(0f, 120f)] private float m_bombRespawnTime = 30f;
    [SerializeField, Tooltip("The maximum amount of bombs"), Range(1, 5)] private int m_maxBombs = 1;
    [SerializeField, Tooltip("The range of the bomb's explosion"), Range(1f, 100f)] private float m_bombExplosionRange = 50f;
    private int m_currentBombAmount = 0;
    

    /// <summary/> The positions of the hearts
    private Vector3[] m_heartPositions;
    
    /// <summary/> The rotations of the hearts
    private Quaternion[] m_heartRotations;
    
    /// <summary/> The connection addresses of the players
    private NetworkConnection m_vrNetworkConnection = null;
    private NetworkConnection m_pcNetworkConnection = null;
    
    private bool m_bombCoroutineRunning;

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
        OnServerTick += SendVrTransform;
        OnServerTick += SendPcTransform;
        OnServerTick += SendLaser;
        OnServerTick += SendPcInvisbility;
        OnServerTick += SendBeaconsPositions;
        OnServerTick += CheckHealths;
        OnServerTick += BeaconDetectionCheck;
        OnServerTick += SendBombPositions;
    }
    
    
    /// <summary>
    /// Start is called before the first update loop
    /// Here we use it to set the phase of the tickrate so that it can't be changed during runtime
    /// We also use it to register the servers handlers / buffers
    /// </summary>
    private void Start()
    {
        // making the tick delta constant
        m_tickDelta = 1f / m_tickrate;
        
        // Setting up buffers
        NetworkServer.RegisterHandler<VrTransform>(OnServerReceiveVrTransforms);
        NetworkServer.RegisterHandler<PcTransform>(OnServerReceivePctransform);
        NetworkServer.RegisterHandler<VrLaser>(OnServerReceiveVrLaser);
        NetworkServer.RegisterHandler<PcInvisibility>(OnServerReceiveInvisibility);
        NetworkServer.RegisterHandler<ClientConnect>(OnServerReceiveClientConnect);
        NetworkServer.RegisterHandler<HeartBreak>(OnServerReceiveHeartBreak);
        NetworkServer.RegisterHandler<BeaconsPositions>(OnServerReceiveBeaconsPositions);
        //NetworkServer.RegisterHandler<DestroyedBeacon>(OnServerReceiveDestroyedBeacon);
        NetworkServer.RegisterHandler<ActivateBeacon>(OnServerReceiveActivateBeacon);
        NetworkServer.RegisterHandler<BombsPositions>(OnServerReceiveBombsPositions);
        NetworkServer.RegisterHandler<BombExplosion>(OnServerReceiveBombExplosion);
        NetworkServer.RegisterHandler<BombActivation>(OnReceiveBombActivation);
        NetworkServer.RegisterHandler<ElevatorActivation>(OnElevatorActivation);

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
            OnServerTick?.Invoke();
            yield return new WaitForSeconds(m_tickDelta);
        }
    }

    IEnumerator SpawnInitialBeacons()
    {
        yield return new WaitForSeconds(m_initialBeaconSpawnDelay);
        for (int i = 0; i < m_initialBeacons; i++)
        {
            SpawnBeacon(-i-1);
        }
        
        StartCoroutine(BeaconSpawnTimer());
    }
    
    /// <summary/> Spawns the beacons
    IEnumerator BeaconSpawnTimer()
    {
        if (/*!NetworkServer.active ||*/ m_currentBeaconAmount >= m_maxBeacons) yield break;
        
        yield return new WaitForSeconds(m_beaconRespawnTime);

        
        if (/*!NetworkServer.active ||*/ m_currentBeaconAmount >= m_maxBeacons) yield break;

        SpawnBeacon();
        
        if(m_currentBeaconAmount < m_maxBeacons) StartCoroutine(BeaconSpawnTimer());
    }
    
    /// <summary/> Despawns the beacon at the selected index with an offset defined by the previous despawns
    /// <param name="p_index"/> the beacon index
    /// <param name="p_beaconID"/> the beacon ID
    IEnumerator DespawnBeacon(int p_index, float p_beaconID)
    {
        yield return new WaitForSeconds(m_beaconLifeTime);

        int? index = FindBeaconFromID(p_index,p_beaconID);
        
        if (index == null)
        {
            Debug.LogWarning("SERVER RECEIVE DESPAWN BEACON ID SEARCH FAILED");
            yield break;
        }
        
        GetRidOfBeacon(index??0, p_beaconID);
    }

    /// <summary>
    /// Destroys the beacon and iterates the m_currentBeaconAmount variable and sends the message of destruction to the clients
    /// </summary>
    /// <param name="p_index"> the index indication of the beacon </param>
    /// <param name="p_beaconID"> the beaconID od the beacon </param>
    private void GetRidOfBeacon(int p_index, float p_beaconID)
    {
        List<BeaconData> datas = m_beaconsPositionsBuffer.data.ToList();
        datas.RemoveAt(p_index);
        
        m_beaconsPositionsBuffer.data = datas.ToArray();
        m_currentBeaconAmount--;
        
        SendToBothClients(new DestroyedBeacon(){index = p_index,beaconID = p_beaconID});
        StartCoroutine(BeaconSpawnTimer());
    }

    IEnumerator BombSpawnTimer() {
        
        m_bombCoroutineRunning = true;
        if (/*!NetworkServer.active ||*/ m_currentBombAmount >= m_maxBombs) yield break;
        
        yield return new WaitForSeconds(m_bombRespawnTime);

        
        if (/*!NetworkServer.active ||*/ m_currentBombAmount >= m_maxBombs) yield break;

        SpawnBomb();
        
        if(m_currentBombAmount < m_maxBombs) StartCoroutine(BombSpawnTimer());
        m_bombCoroutineRunning = false;
    }

    #region SERVER ACTIONS

    
    /// <summary/> The function that is called when the server is hosted.
    private void OnStartServer()
    {
        StartCoroutine(ServerTick());
        StartCoroutine(SpawnInitialBeacons());
        StartCoroutine(BombSpawnTimer());
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
        if (m_beaconsPositionsBuffer.data == null) return;

        // Debug.Log(m_beaconsPositionsBuffer.data.Length);
        for (int i = 0; i < m_beaconsPositionsBuffer.data.Length; i++)
        {
            BeaconData data = m_beaconsPositionsBuffer.data[i];
            if(!data.isActive)continue;
            
            bool detected = Vector3.Distance(data.position, m_pcTransformBuffer.position) < m_beaconRange;

            if (data.detectingPlayer != detected)
            {
                SendToBothClients(new BeaconDetectionUpdate(){beaconID = data.beaconID,index = i, playerDetected = detected});
                
                m_beaconsPositionsBuffer.data[i].detectingPlayer = detected;
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
    
    /// <summary/> The function called when the server has to spawn a beacon
    /// <param name="p_customID"/> Only set a custom ID if you have to, if you don't it will by default be Time.time
    void SpawnBeacon(float? p_customID = null)
    {
        m_currentBeaconAmount++;
        SpawnBeacon spawnBeacon = new SpawnBeacon() {beaconID = p_customID ?? Time.time};
        
        AddBeacon(spawnBeacon.beaconID);
        SendToBothClients(spawnBeacon);
    }

    /// <summary/> Function called to add a beacon to the servers local list of beacons
    /// <param name="p_ID"> The unique ID of the beacon </param>
    void AddBeacon(float p_ID)
    {
        List<BeaconData> beaconList = (m_beaconsPositionsBuffer.data == null) ? new List<BeaconData>() :m_beaconsPositionsBuffer.data.ToList() ;
        
        beaconList.Add(new BeaconData(){beaconID = p_ID,detectingPlayer = false,isActive = false});

        m_beaconsPositionsBuffer.data = beaconList.ToArray();
    }
    
    /// <summary/> The function called when the server has to spawn a bomb
    /// <param name="p_customID"/> Only set a custom ID if you have to, if you don't it will by default be Time.time
    void SpawnBomb(float? p_customID = null)
    {
        m_currentBombAmount++;
        SpawnBomb spawnBomb = new SpawnBomb() {bombID = p_customID ?? Time.time};
        
        AddBomb(spawnBomb.bombID);
        SendToBothClients(spawnBomb);
    }

    /// <summary/> Function called to add a bomb to the servers local list of bombs
    /// <param name="p_ID"> The unique ID of the beacon </param>
    void AddBomb(float p_ID)
    {
        List<BombData> bombList = (m_bombsPositionsBuffer.data == null) ? new List<BombData>() : m_bombsPositionsBuffer.data.ToList() ;
        
        bombList.Add(new BombData(){bombID = p_ID});

        m_bombsPositionsBuffer.data = bombList.ToArray();
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
            beaconRange = m_beaconRange,
            maximumBeaconCount = m_maxBeacons,
            maximumBombsCount = m_maxBombs,
            elevatorSpeed = m_elevatorSpeed,
            elevatorWaitTime = m_elevatorWaitTime
        };
        
        p_conn.Send(initialData);
        
        if(m_beaconsPositionsBuffer.data != null)
            foreach (BeaconData data in m_beaconsPositionsBuffer.data)
                p_conn.Send(new SpawnBeacon() {beaconID = data.beaconID});
        
        if(m_bombsPositionsBuffer.data != null)
            foreach (BombData data in m_bombsPositionsBuffer.data)
            {
                p_conn.Send(new SpawnBomb() {bombID = data.bombID});
            }
    }
    
    /// <summary/> function called when the server receives a message of type ActivateBeacon
    /// <param name="p_conn"> The connection from which originated the message </param>
    /// <param name="p_ativateBeacon"> The message sent by the Client to the Server  </param>
    private void OnServerReceiveActivateBeacon(NetworkConnection p_conn, ActivateBeacon p_ativateBeacon)
    {

        int? index = FindBeaconFromID(p_ativateBeacon.index,p_ativateBeacon.beaconID);
        
        if (index == null)
        {
            Debug.LogError("SERVER RECEIVE ACTIVATE BEACON ID SEARCH FAILED");
            return;
        }
        
        m_beaconsPositionsBuffer.data[index??0].isActive = true;
        // TODO faire la coroutine propre #indexe memoire
        StartCoroutine(DespawnBeacon(index??0,m_beaconsPositionsBuffer.data[index??0].beaconID));
        
        //TODO Add this to server loop
        SendToBothClients(p_ativateBeacon);
    }

    /// <summary/> function called when the server receives a message of type VrTransform
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
            Vector3 laserCriticalPath = playerPos - startingPoint;
            
            // Hitboxes Verification (blame Blue)
            bool hitAWall = Physics.Raycast(startingPoint, laserCriticalPath.normalized, laserCriticalPath.magnitude, /*Ignoring the player and Vr layers, and yes it could be slightly more optimized :-Þ*/~((1 << 13) | (1 << 3)), QueryTriggerInteraction.Ignore);
            
            RaycastHit rayHit;
            bool hitSmth = Physics.Raycast(startingPoint, direction.normalized, out rayHit, 10000f, /*Ignoring the player and Vr layers, and yes it could be slightly more optimized :-Þ*/~((1 << 13) | (1 << 3)), QueryTriggerInteraction.Ignore);

            Debug.DrawRay(startingPoint, laserCriticalPath, hitAWall ? Color.green : Color.red, 15f);
            
            Debug.LogWarning($"The laser hit ? {hitAWall}", this);
            
            bool hit;
            switch (hitAWall) {
                case false : { //If there's no wall between
                    // So we measure the distance of the player's position from the line of the laser
                    float distance = Vector3.Cross(direction, playerPos - startingPoint).magnitude;

                    //if the distance between the line of fire and the player's position is blablablbalalboom... he ded
                    hit = distance <= m_laserRadius && Vector3.Angle(playerPos - startingPoint, direction) < 90f;

                    if (hit) {
                        m_pcPlayerHealth--;
                        m_laserBuffer.length = laserCriticalPath.magnitude;
                    }
                    else {
                        m_laserBuffer.length = hitSmth ? rayHit.distance : 10000f;
                    }

                break; }

                case true : {
                    m_laserBuffer.length = hitSmth ? rayHit.distance : 10000f;
                    
                    hit = false;
                break; }
            }
            
            // Packing the vallues in a neat little message
            m_laserBuffer.origin = startingPoint;
            m_laserBuffer.rotation = m_vrTransformBuffer.rotationRightHand;
            m_laserBuffer.hitPosition = m_pcTransformBuffer.position;
            m_laserBuffer.hit = hit;
        }
        //preparing to send the message
        OnServerTick -= SendLaser;
        OnServerTick += SendLaser;
    }

    /// <summary>
    /// function called when the server receives a message of type Invisbility
    /// </summary>
    /// <param name="p_conn"> The connection from which originated the message </param>
    /// <param name="p_pcInvisibility"> The message sent by the Client to the Server  </param>
    private void OnServerReceiveInvisibility(NetworkConnection p_conn, PcInvisibility p_pcInvisibility) {

        m_pcInvisibilityBuffer = p_pcInvisibility;
        OnServerTick -= SendPcInvisbility;
        OnServerTick += SendPcInvisbility;
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
        OnServerTick -= SendHeartBreak;
        OnServerTick += SendHeartBreak;
    }

    /// <summary>
    /// function called when the server receives a message of type BeaconsPositions
    /// </summary>
    /// <param name="p_conn">The connection from which originated the message</param>
    /// <param name="p_beaconsPositions">The message sent by the Client to the Server</param>
    private void OnServerReceiveBeaconsPositions(NetworkConnection p_conn, BeaconsPositions p_beaconsPositions)
    {
        //m_beaconsPositionsBuffer = p_beaconsPositions; FUUUUUUUUUUUUUCK
        int count = m_beaconsPositionsBuffer.data.Length;

        for (int i = 0; i < count; i++)
        {
            if(i>=p_beaconsPositions.data.Length) break;
            m_beaconsPositionsBuffer.data[i].position = p_beaconsPositions.data[i].position;
            m_beaconsPositionsBuffer.data[i].beaconID = p_beaconsPositions.data[i].beaconID;
        }

        m_newPositions = true;
    }

    /// <summary>
    /// function called when the server receives a message of type BombsPositions
    /// </summary>
    /// <param name="p_conn">The connection from which originated the message</param>
    /// <param name="p_bombsPositions">The message sent by the Client to the Server</param>
    private void OnServerReceiveBombsPositions(NetworkConnection p_conn, BombsPositions p_bombsPositions) {
        
        int count = m_bombsPositionsBuffer.data.Length;

        for (int i = 0; i < count; i++) { // We get only the position and ID in our buffers
            if(i>=p_bombsPositions.data.Length) break;
            m_bombsPositionsBuffer.data[i].position = p_bombsPositions.data[i].position;
            m_bombsPositionsBuffer.data[i].bombID = p_bombsPositions.data[i].bombID;
        }

        m_newPositions = true;
    }
    
    

    /// <summary>
    /// function called when the server receives a message of type BombExplosion
    /// </summary>
    /// <param name="p_conn">The connection from which originated the message</param>
    /// <param name="p_bombExplosion">The message sent by the Client to the Server</param>
    private void OnServerReceiveBombExplosion(NetworkConnection p_conn, BombExplosion p_bombExplosion) {

        bool hit = Vector3.Distance(p_bombExplosion.position, m_pcTransformBuffer.position) < m_bombExplosionRange;
        m_currentBombAmount--;
        if (hit) m_pcPlayerHealth--;
        
        SendToBothClients(new BombExplosion(){position = p_bombExplosion.position, index = p_bombExplosion.index, bombID = p_bombExplosion.bombID, hit = hit});
        if(!m_bombCoroutineRunning)StartCoroutine(BombSpawnTimer());
    }

    private void OnReceiveBombActivation(NetworkConnection p_conn, BombActivation p_bombActivation) => m_pcNetworkConnection.Send(p_bombActivation);
    
    /// <summary/> function called when the server receives a message of type ElevatorActivation
    /// <param name="p_conn">The connection from which originated the message</param>
    /// <param name="p_elevatorActivation">The message sent by the Client to the Server</param>
    private void OnElevatorActivation(NetworkConnection p_conn, ElevatorActivation p_elevatorActivation)
    {
        m_elevatorActivationBuffer = p_elevatorActivation;
        OnServerTick -= UpdateElevatorActivation;
        OnServerTick += UpdateElevatorActivation;
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
        OnServerTick -= SendLaser;
    }

    /// <summary>
    /// The function that send the PcInvisibility to the clients
    /// </summary>
    private void SendPcInvisbility()
    {
        SendToBothClients(m_pcInvisibilityBuffer);
        OnServerTick -= SendPcInvisbility;
    }
    
    /// <summary>
    /// The function that send the SendHeartBreak to the clients
    /// </summary>
    private void SendHeartBreak()
    {
        SendToBothClients(m_heartBreakBuffer);
        OnServerTick -= SendHeartBreak;
    }

    /// <summary>
    /// The function that send the BeaconsPositions (if not empty) to the pc client
    /// </summary>
    private void SendBeaconsPositions() {
        if(m_newPositions && m_beaconsPositionsBuffer.data != null && m_beaconsPositionsBuffer.data.Length > 0)
        {
            m_pcNetworkConnection.Send(m_beaconsPositionsBuffer);
            m_newPositions = false;
        }
    }
    
    /// <summary>
    /// The function that send the BeaconsPositions (if not empty) to the pc client
    /// </summary>
    private void SendBombPositions() {
        if(m_bombsPositionsBuffer.data != null && m_bombsPositionsBuffer.data.Length > 0)
        {
            m_pcNetworkConnection.Send(m_bombsPositionsBuffer);
        }
    }
    
    /// <summary/> The function to call when an elevator gets activated
    private void UpdateElevatorActivation() {
        SendToBothClients(m_elevatorActivationBuffer);
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
    
    /// <summary/> A function to find the index of the beacon that matches the given ID
    /// <param name="p_index"> the estimated index of the wanted beacon </param>
    /// <param name="p_beaconID"> the ID of the wanted beacon </param>
    /// <returns> returns the index of the beacon with the right ID if none are found, returns null </returns>
    private int? FindBeaconFromID(int p_index, float p_beaconID)
    {
        int index = p_index;
        float ID = p_beaconID;
        BeaconData[] data = m_beaconsPositionsBuffer.data;
        if ( index < data.Length && data[index].beaconID == ID) return index;

        for (int i = 0; i < data.Length; i++) if (data[i].beaconID == ID) return i;

        #if UNITY_EDITOR
        Debug.LogWarning("I couldn't find the index matching this ID brother",this);
        #endif
        return null;
    }
}