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

    [SerializeField] public bool m_practice = false;
    
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
    private LeureTransform m_leureBuffer;
    private bool m_isLeureAliveBuffer = false;
    private ActivateFlair m_flairBuffer;
    private ActivateBlind m_activateBlindBuffer;
    private DropTp m_dropTpBuffer;
    private Teleported m_teleporttedBuffer;
    private PotentialSpawnPoints m_potentialSpawnPointsBuffer;
    
    private bool m_vrReadyBuffer;
    private bool m_pcReadyBuffer;
    
    #endregion
    
    
    #region Server Data

    [Header("Server Settings")]
    [SerializeField, Range(1,120), Tooltip("the server's tick rate in Hz")] private int f_tickrate = 30;
    private int m_tickrate = 30;
    private float m_tickDelta = 1f;

    [SerializeField] [Tooltip("Do not edit this value, it id for debug only.\nYou can remove this SerializeField if everything is working fine")] private bool m_isInTutorial = false;
    
    /// <summary/> The custom variables added onto the Network Manager
    [Header("Laser")]
    [SerializeField, Tooltip("The radius of the VR laser")] private float f_laserRadius = 20f;
    private float m_laserRadius = 20f;
    
    [SerializeField, Tooltip("The offset of the raycast shot to the player to check if the lazer hit")] private float f_laserCheckOffset = 2f;
    private float m_laserCheckOffset = 2f;

    [SerializeField] private LayerMask f_playerLayers;
    private LayerMask m_playerLayers;

    /// <summary>The local position of the child of the right hand that determines the point from where the laser is shot </summary>
    private Vector3 m_fingertipOffset;
    
    
    [Header("Elevators")]
    
    [SerializeField, Tooltip("The speed of the elevators in m/s")] private float f_elevatorSpeed = 5.8f;
    private float m_elevatorSpeed = 5.8f;
    
    [SerializeField, Tooltip("The speed of the elevators in m/s")] private float f_elevatorWaitTime = 1f;
    private float m_elevatorWaitTime = 1f;
    

    [Header("Hearts")]
    [Header(" ")]
    [SerializeField, Tooltip("The positions of the hearts on the map")] private Transform[] f_heartTransforms;
    [ItemCanBeNull] private Transform[] m_heartTransforms;
    [SerializeField, Tooltip("The amount of hearts that need to be destroyed for the VR player to lose")] private int f_vrPlayerHealth = 3;
    private int m_vrPlayerHealth = 3;
    [SerializeField, Tooltip("The amount of times the PC player has to eliminated to lose")] private int f_pcPlayerHealth = 3;
    private int m_pcPlayerHealth = 3;
    
    [Header("Heart Radius")] [Header(" ")]
    [SerializeField, Range(0f,100f)]private float f_heartZoneRadius = 5f;
    private float m_heartZoneRadius = 0f;
    
    private float m_heartTimer = 0f;
    
    [SerializeField, Range(0f,100f)] private float f_heartDestroyTime = 5f;
    private float m_heartDestroyTime = 0f;

    
    [Header("Beacons")]
    [Header(" ")]
    [SerializeField, Tooltip("The amount of beacons that will spawn at the start of the game")]private int f_initialBeacons = 2;
    private int m_initialBeacons = 2;
    [SerializeField, Tooltip("The time after which the initial beacons will spawn")]private float f_initialBeaconSpawnDelay = 10f;
    private float m_initialBeaconSpawnDelay = 10f;
    [Header(" ")]
    [SerializeField, Tooltip("The interval at which the beacons spawn")] private float f_beaconRespawnTime = 30f;
    private float m_beaconRespawnTime = 30f;
    [SerializeField, Tooltip("The lifetime of a beacon")] private float f_beaconLifeTime = 10f;
    private float m_beaconLifeTime = 10f;
    [SerializeField, Tooltip("The maximum amount of beacons")] private int f_maxBeacons = 3;
    private int m_maxBeacons = 3; 
    private int m_currentBeaconAmount = 0;
    [Header(" ")]
    [SerializeField, Tooltip("The range of the beacons")] private float f_beaconRange = 400f;
    private float m_beaconRange = 400f;

    [Header(" ")] [Header("Bombs")] [Header(" ")]
    [SerializeField, Tooltip("If false, will play the game without the bombs feature")] private bool f_isUsingBombs = true;
    private bool m_isUsingBombs = true;
    
    [SerializeField, Tooltip("The interval at which the bomb spawn"), Range(0f, 120f)] private float f_bombRespawnTime = 30f;
    private float m_bombRespawnTime = 30f;
    [SerializeField, Tooltip("The maximum amount of bombs"), Range(1, 5)] private int f_maxBombs = 1;
    private int m_maxBombs = 1;
    [SerializeField, Tooltip("The range of the bomb's explosion"), Range(1f, 100f)] private float f_bombExplosionRange = 50f;
    private float m_bombExplosionRange = 50f;
    
    [SerializeField, Tooltip("The time the bomb will take to explode"), Range(1f, 100f)] private float f_bomDetonationTime = 1f;
    private float m_bomDetonationTime = 1f;
    
    [Header(" ")] [Header("Flair")] [Header(" ")]
    [SerializeField, Tooltip("The speed at which the flair will rise"), Range(0f, 120f)] private float f_flairRaiseSpeed = 30f;
    private float m_flairRaiseSpeed = 30f;
    [SerializeField, Tooltip("The amount of time before the flair detonates"), Range(1, 15)] private float f_flairDetonationTime = 2f;
    private float m_flairDetonationTime = 2f;
    [SerializeField, Tooltip("The amount of time before the flair detonates"), Range(1, 15)] private float f_flashDuration = 5f;
    private float m_flashDuration = 5f;
    [SerializeField, Tooltip("the minimum and maximum dot product from the look angle to clamp")]private Vector2 f_flashClamp;
    private Vector2 m_flashClamp;
    private int m_currentBombAmount = 0;
    
    [Header("Respawn")]
    [SerializeField, Range(0f,100f)] private ushort f_numberOfSpawnPointsToDisplay = 3;
    private ushort m_numberOfSpawnPointsToDisplay = 3;

    

    /// <summary/> The positions of the hearts
    private Vector3[] m_heartPositions;
    
    /// <summary/> The rotations of the hearts
    private Quaternion[] m_heartRotations;
    
    /// <summary/> The connection addresses of the players
    private NetworkConnection m_vrNetworkConnection = null;
    private NetworkConnection m_pcNetworkConnection = null;
    
    private bool m_bombCoroutineRunning;

    private Coroutine m_severTick;
    private Coroutine m_spawnInitialBeacons;
    private Coroutine m_spawnBombs;
    
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
        // MyNetworkManager.del_onHostServer += StartGame;
        MyNetworkManager.del_onClientConnection += OnServerConnect;
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
        NetworkServer.RegisterHandler<ActivateBeacon>(OnServerReceiveActivateBeacon);
        NetworkServer.RegisterHandler<BombsPositions>(OnServerReceiveBombsPositions);
        NetworkServer.RegisterHandler<BombExplosion>(OnServerReceiveBombExplosion);
        NetworkServer.RegisterHandler<BombActivation>(OnReceiveBombActivation);
        NetworkServer.RegisterHandler<ElevatorActivation>(OnElevatorActivation);
        NetworkServer.RegisterHandler<ActivateFlair>(OnActivateFlair);
        NetworkServer.RegisterHandler<SpawnLeure>(OnSpawnLeure);
        NetworkServer.RegisterHandler<DestroyLeure>(OnDestroyLeure);
        NetworkServer.RegisterHandler<LeureTransform>(OnLeureTransform);
        NetworkServer.RegisterHandler<RestartGame>(OnRestartGame);
        NetworkServer.RegisterHandler<ReadyToPlay>(OnReadyMessage);
        NetworkServer.RegisterHandler<DropTp>(OnDropTp);
        NetworkServer.RegisterHandler<RemoveTp>(OnRemoveTp);
        NetworkServer.RegisterHandler<Teleported>(OnTeleported);
        NetworkServer.RegisterHandler<Tutorial>(OnReceiveTutorial);
        NetworkServer.RegisterHandler<PotentialSpawnPoints>(OnReceivePotentialSpawnpoints);
        NetworkServer.RegisterHandler<VrInitialValues>(OnReceiveVRInitialValues);
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
    
    /// <summary/> The Server Loop
    IEnumerator Flair(float p_detonationTime, float p_flashDuration)
    {
        yield return new WaitForSeconds(p_detonationTime);
        Vector3 detonationPoint = m_flairBuffer.startPosition + Vector3.up * (m_flairRaiseSpeed * m_flairDetonationTime);

        Vector3 vrHeadForward = m_vrTransformBuffer.rotationHead * Vector3.forward;

        
        
        Vector3 headToDetonationAngle = (detonationPoint - m_vrTransformBuffer.positionHead).normalized;
        
        float flashStrength = (Vector3.Dot(vrHeadForward, headToDetonationAngle) + 1f)/2f;

        if (flashStrength < m_flashClamp.x) flashStrength = 0f;
        else if (flashStrength > m_flashClamp.y) flashStrength = 1f;
        
        if(m_flashCoroutine != null)StopCoroutine(m_flashCoroutine);
        m_flashCoroutine = StartCoroutine(Flash(p_flashDuration * flashStrength));
    }

    IEnumerator Flash(float p_flashDuration)
    {
        m_activateBlindBuffer.blindIntensity = p_flashDuration;
        SendActivateBlind();
        yield return new WaitForSeconds(p_flashDuration);
        SendDeActivateBlind();
    }

    private void Update()
    {
        Debug.DrawRay(Vector3.zero, m_vrTransformBuffer.rotationHead * Vector3.forward * 300f);
    }

   

    #region SERVER ACTIONS

    private void SendReady()
    {
        SendToBothClients(new ReadyToPlay());
    }
    
    /// <summary/> DONT FUCKING TOUCH THIS
    private bool m_firstTime = true;

    private bool m_gameEnded;
    private Coroutine m_flashCoroutine;

    /// <summary>
    /// The function that is called when the server is hosted.
    /// This function takes the serialized properties and applies them to their modifiable counterpart thereby conserving the initial values.
    /// </summary>
    private void StartGame()
    {
        
        m_isUsingBombs = f_isUsingBombs;
        
        
        //Setting up senders
        OnServerTick += SendVrTransform;
        OnServerTick += SendPcTransform;
        OnServerTick += SendLaser;
        OnServerTick += SendPcInvisbility;
        OnServerTick += SendBeaconsPositions;
        OnServerTick += BeaconDetectionCheck;
        if(m_isUsingBombs)OnServerTick += SendBombPositions;
        OnServerTick += CheckHealths;
        OnServerTick += CheckHeartDestroyRange;
        
        #region Members assignment
        //Resetting the serialized values
        m_tickrate = f_tickrate;
        m_laserRadius = f_laserRadius;
        m_elevatorSpeed = f_elevatorSpeed;
        m_elevatorWaitTime = f_elevatorWaitTime;
        m_heartTransforms = f_heartTransforms;

        foreach (var tr in m_heartTransforms) tr.localScale = Vector3.one;
        
        m_vrPlayerHealth = f_vrPlayerHealth;
        m_pcPlayerHealth = f_pcPlayerHealth;
        m_initialBeacons = f_initialBeacons;
        m_initialBeaconSpawnDelay = f_initialBeaconSpawnDelay;
        m_beaconRespawnTime = f_beaconRespawnTime;
        m_beaconLifeTime = f_beaconLifeTime;
        m_maxBeacons = f_maxBeacons;
        m_beaconRange = f_beaconRange;
        m_bombRespawnTime = f_bombRespawnTime;
        m_maxBombs = f_maxBombs;
        m_bombExplosionRange = f_bombExplosionRange;
        m_bomDetonationTime = f_bomDetonationTime;
        m_flairRaiseSpeed = f_flairRaiseSpeed;
        m_flairDetonationTime = f_flairDetonationTime;
        m_flashDuration = f_flashDuration;
        m_flashClamp = f_flashClamp;
        m_laserCheckOffset = f_laserCheckOffset;
        m_playerLayers = f_playerLayers;

        m_heartZoneRadius = f_heartZoneRadius;
        m_heartDestroyTime = f_heartDestroyTime;

        m_numberOfSpawnPointsToDisplay = f_numberOfSpawnPointsToDisplay;
        
        //Resetting the beacon values
        m_currentBeaconAmount = 0;
        m_currentBombAmount = 0;
        m_bombCoroutineRunning = false;
        
        m_pcReadyBuffer = false;
        m_vrReadyBuffer = false;

        m_gameEnded = false;
        
        // Doing a small checkup
        if (f_vrPlayerHealth > f_heartTransforms.Length) Debug.LogWarning("the Vr Player has more health than there are hearts... Big L?",this);
        
        //Unpacking the heart position values from the transforms to send through messages
        List<Vector3> heartPositions = new List<Vector3>();
        List<Quaternion> heartRotations = new List<Quaternion>();

        foreach (Transform pos in f_heartTransforms)
        {
            heartPositions.Add(pos.position);
            heartRotations.Add(pos.rotation);
        }
        
        m_heartPositions = heartPositions.ToArray();
        m_heartRotations = heartRotations.ToArray();
        
        //Resetting the buffers
        m_vrTransformBuffer = new VrTransform();
        m_pcTransformBuffer = new PcTransform();
        m_laserBuffer = new Laser();
        m_pcInvisibilityBuffer = new PcInvisibility();
        m_heartBreakBuffer = new HeartBreak();
        m_beaconsPositionsBuffer = new BeaconsPositions();
        m_bombsPositionsBuffer = new BombsPositions();
        m_newPositions = false;
        m_destroyedBeaconBuffer = new DestroyedBeacon();
        m_elevatorActivationBuffer = new ElevatorActivation();
        m_leureBuffer = new LeureTransform();
        m_flairBuffer = new ActivateFlair();
        m_activateBlindBuffer = new ActivateBlind();

        #endregion
        
        InitialData initialData = new InitialData() {
            healthPcPlayer = m_pcPlayerHealth,
            healthVrPlayer = m_vrPlayerHealth,
            beaconRange = m_beaconRange,
            maximumBeaconCount = m_maxBeacons,
            maximumBombsCount = m_maxBombs,
            elevatorSpeed = m_elevatorSpeed,
            elevatorWaitTime = m_elevatorWaitTime,
            flairRaiseSpeed = m_flairRaiseSpeed,
            flairDetonationTime = m_flairDetonationTime,
            heartPositions = m_heartPositions,
            heartRotations = m_heartRotations,
            bombDetonationTime = m_bomDetonationTime,
            bombExplosionRange = m_bombExplosionRange,
            heartRange = m_heartZoneRadius,
            heartConquerTime = m_heartDestroyTime,
            numberOfSpawnPointsToDisplay = m_numberOfSpawnPointsToDisplay
        };
        
        SendToBothClients(initialData);
        
        
        
        if(m_firstTime) {
            m_severTick = StartCoroutine(ServerTick());
            m_firstTime = false;
        }
        m_spawnInitialBeacons = StartCoroutine(SpawnInitialBeacons());
        if(m_isUsingBombs)m_spawnBombs = StartCoroutine(BombSpawnTimer());
    }

    private void EndGame(ClientConnection p_winner = ClientConnection.PcPlayer)
    {
        m_gameEnded = true;
        
        SendToBothClients( new GameEnd(){winningClient = p_winner});
        
        StopCoroutine(m_spawnInitialBeacons);
        StopCoroutine(m_spawnBombs);
        //Setting up senders
        OnServerTick = null;
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
            if (m_isLeureAliveBuffer)detected = detected || Vector3.Distance(data.position, m_leureBuffer.position) < m_beaconRange;

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
        if (m_gameEnded) return;
        if (m_pcPlayerHealth <= 0) {
            EndGame(ClientConnection.VrPlayer);
            return;
        }
        
        if (m_vrPlayerHealth <= 0)
            EndGame(ClientConnection.PcPlayer);
    }
    private void CheckHeartDestroyRange()
    {
        Vector2 horizontalPlayerPosition = new Vector2(m_pcTransformBuffer.position.x, m_pcTransformBuffer.position.z);
        float playerHeight = m_pcTransformBuffer.position.y;
        for (int i = 0; i < m_heartTransforms.Length; i++)
        {
            if(m_heartTransforms[i].localScale == Vector3.zero) continue;
            
            if(Mathf.Abs(playerHeight - m_heartTransforms[i].position.y) > 10f) continue;
            
            Vector2 horizontalheartPosition = new Vector2(m_heartTransforms[i].position.x,m_heartTransforms[i].position.z);
            if (Vector2.Distance(horizontalPlayerPosition, horizontalheartPosition) < m_heartZoneRadius)
            {
                m_heartTimer += m_tickDelta;
                SendToBothClients(new HeartConquerStart() {time = m_heartTimer, index =  i});
                
                if (m_heartTimer>m_heartDestroyTime)
                {
                    m_heartTransforms[i].localScale = Vector3.zero;
                    SendToBothClients(new HeartBreak() {index = i});
                    m_vrPlayerHealth--;
                }
                return;
            }
        }

        if (m_heartTimer == 0) return;
        
        SendToBothClients(new HeartConquerStop());
        m_heartTimer = 0f;
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
        List<BeaconData> beaconList = (m_beaconsPositionsBuffer.data == null) ? new List<BeaconData>() : m_beaconsPositionsBuffer.data.ToList() ;
        
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

    private void OnLeureTransform(NetworkConnection arg1, LeureTransform p_leureTransform)
    {
        OnServerTick -= SendLeureTransform;
        OnServerTick += SendLeureTransform;

        m_leureBuffer = p_leureTransform;
    }
    
    private void OnRestartGame(NetworkConnection arg1, RestartGame p_restartGame)
    {
        if(m_vrNetworkConnection != null && m_pcNetworkConnection !=null) {
            EndGame();
            SendReady();
        }
    }

    private void OnDestroyLeure(NetworkConnection arg1, DestroyLeure arg2)
    {
        m_isLeureAliveBuffer = false;
        OnServerTick -= SendDestroyLeure;
        OnServerTick += SendDestroyLeure;
    }
    
    private void OnSpawnLeure(NetworkConnection arg1, SpawnLeure arg2)
    {
        m_isLeureAliveBuffer = true;
        OnServerTick -= SendSpawnLeure;
        OnServerTick += SendSpawnLeure;
    }
    
    private void OnActivateFlair(NetworkConnection arg1, ActivateFlair p_message)
    {
        m_flairBuffer = p_message;
        OnServerTick -= SendActivateFlair;
        OnServerTick += SendActivateFlair;
    }
    
    private void OnActivateBlind(NetworkConnection arg1, ActivateBlind p_activateBlind)
    {
        m_activateBlindBuffer = p_activateBlind;
        OnServerTick -= SendActivateBlind;
        OnServerTick += SendActivateBlind;
    }
    
    private void OnDeActivateBlind(NetworkConnection arg1, DeActivateBlind arg2)
    {
        OnServerTick -= SendDeActivateBlind;
        OnServerTick += SendDeActivateBlind;
    }


    /// <summary>
    /// function called when the server receives a message of type ClientConnect
    /// </summary>
    /// <param name="p_conn"> The connection from which originated the message </param>
    /// <param name="p_client"> The message sent by the Client to the Server  </param>
    private void OnServerReceiveClientConnect(NetworkConnection p_conn, ClientConnect p_client)
    {
        if (p_client.client == ClientConnection.VrPlayer) m_vrNetworkConnection = p_conn;
        else if (p_client.client == ClientConnection.PcPlayer) m_pcNetworkConnection = p_conn;

        if (m_vrNetworkConnection != null && m_pcNetworkConnection != null)
        {
            SendReady();
        }
    }

    private void OnReadyMessage(NetworkConnection p_connection, ReadyToPlay p_message)
    {
        if (p_connection == m_vrNetworkConnection) m_vrReadyBuffer = true;
        else if (p_connection == m_pcNetworkConnection) m_pcReadyBuffer = true;
        
        if(m_vrReadyBuffer && m_pcReadyBuffer) StartGame();
    }
    
    
    private void OnTeleported(NetworkConnection p_connection, Teleported p_message)
    {
        m_teleporttedBuffer = p_message;
        OnServerTick -= SendTeleported;
        OnServerTick += SendTeleported;
    }

    private void OnRemoveTp(NetworkConnection p_connection, RemoveTp p_message)
    {
        OnServerTick -= SendRemoveTp;
        OnServerTick += SendRemoveTp;
    }
    private void OnDropTp(NetworkConnection p_connection, DropTp p_message)
    {
        m_dropTpBuffer = p_message;
        OnServerTick -= SendDropTp;
        OnServerTick += SendDropTp;
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
    private void OnServerReceivePctransform(NetworkConnection p_conn,PcTransform p_pcTransform) {
#if UNITY_EDITOR
        //TODO : maybe cancel the receive ? Only if we really want this to be cheat-proof
        if(p_conn == m_vrNetworkConnection && !m_isInTutorial)Debug.LogWarning("I just received PC position from the Vr side ﴾͡๏̯͡๏﴿");
#endif
        m_pcTransformBuffer = p_pcTransform;
    }

    /// <summary>
    /// function called when the server receives a message of type VrLaser
    /// it calculate whether the laser hit the pc player then sends the result back
    /// it calculates this by checking whether the player's pivot point is within the range of the laser's radius 
    /// </summary>
    /// <param name="p_conn"> The connection from which originated the message </param>
    /// <param name="p_vrlaser"> The message sent by the Client to the Server </param>
    private void OnServerReceiveVrLaser(NetworkConnection p_conn, VrLaser p_vrlaser) {
        
        m_laserBuffer = new Laser();
        m_laserBuffer.laserState = p_vrlaser.laserState;
        
        //TODO bring this verification to the server loop
        
        // If the laser is being aimed forgo the collision calculation and just send the message 
        if (m_laserBuffer.laserState == LaserState.Shooting) {

            // Or une parallel line distance operations to check wether the player's pivot point is within the range of the laser's radius

            // Mafs
            // Vector3 startingPoint = m_vrTransformBuffer.positionRightHand + (m_vrTransformBuffer.rotationRightHand * m_fingertipOffset);
            // Vector3 direction = m_vrTransformBuffer.rotationRightHand * Vector3.forward;
            Vector3 startingPoint = p_vrlaser.origin;
            Vector3 direction = p_vrlaser.rotation * Vector3.forward;
            Vector3 playerPos = m_pcTransformBuffer.position + Vector3.up * m_laserCheckOffset/2f;
            Vector3 laserCriticalPath = (playerPos + Vector3.up * m_laserCheckOffset / 2f) - startingPoint;
            Debug.DrawLine(playerPos,playerPos + Vector3.up * m_laserCheckOffset/2f,Color.red,5f);
            
            // Hitboxes Verification (blame Blue)
            bool hitAWall = Physics.Raycast(startingPoint, laserCriticalPath.normalized,out RaycastHit hited, laserCriticalPath.magnitude,m_playerLayers,QueryTriggerInteraction.Ignore);
            
#if UNITY_EDITOR
            Debug.DrawRay(startingPoint, laserCriticalPath, hitAWall ? Color.green : Color.red,15f);
#endif
            
            RaycastHit rayHit;
            bool hitSmth = Physics.Raycast(startingPoint, direction.normalized, out rayHit, 10000f, m_playerLayers, QueryTriggerInteraction.Ignore);

            //Debug.DrawRay(startingPoint, laserCriticalPath, hitAWall ? Color.green : Color.red, 15f);
            
            bool hit;
            const float valueIfNoHit = 0f; //BE WARY THE VR NEEDS TO KNOW WHICH VALUE THIS IS AND WE DON'T SEND IT THROUGH NETWORK!
            float distanceFromHitToTarget = 0f;

            switch (hitAWall) {
                case false : { //If there's no wall between
                    // So we measure the distance of the player's position from the line of the laser
                    distanceFromHitToTarget = Vector3.Cross(direction, playerPos - startingPoint).magnitude;

                    //if the distance between the line of fire and the player's position is blablablbalalboom... he ded
                    hit = distanceFromHitToTarget <= m_laserRadius && Vector3.Angle(playerPos - startingPoint, direction) < 90f;

                    if (hit) {
                        // todo : yeah it's not a feature Blue.. nice try..
                        m_pcPlayerHealth--;
                        m_laserBuffer.length = laserCriticalPath.magnitude;
                    }
                    else {
                        m_laserBuffer.length = hitSmth ? rayHit.distance : valueIfNoHit;
                    }

                    break; }

                case true : {
                    m_laserBuffer.length = hitSmth ? rayHit.distance : valueIfNoHit;
                    hit = false;
                    
                    break; }
            }

            bool hitLeure = false;
            if(!hit)
            {
                Vector3 leurePosition = m_leureBuffer.position + Vector3.up * m_laserCheckOffset / 2f;
                laserCriticalPath = (leurePosition + Vector3.up * m_laserCheckOffset / 2f) - startingPoint;

                distanceFromHitToTarget = Vector3.Cross(direction, leurePosition - startingPoint).magnitude;
                hitLeure = distanceFromHitToTarget <= m_laserRadius && Vector3.Angle(leurePosition - startingPoint, direction) < 90f;

                
                if (hitLeure && !Physics.Raycast(startingPoint, laserCriticalPath.normalized, out hited, laserCriticalPath.magnitude, m_playerLayers, QueryTriggerInteraction.Ignore)) 
                {
                    if(m_flashCoroutine != null)StopCoroutine(m_flashCoroutine);
                    m_flashCoroutine = StartCoroutine(Flash(m_flashDuration));
                    Debug.Log("Vr killed the DECOY");
                    
                    m_isLeureAliveBuffer = false;
                    SendToBothClients(new DestroyLeure());
                }
            }
            
            Debug.Log($"Laser shot : start pos {startingPoint} (detail : {m_vrTransformBuffer.positionRightHand} + {m_vrTransformBuffer.rotationRightHand * m_fingertipOffset}),  direction : {direction}");
            
            // Packing the values in a neat little message
            m_laserBuffer.origin = startingPoint;
            m_laserBuffer.rotation = m_vrTransformBuffer.rotationRightHand;
            //TODO : Next line ain't clean...
            m_laserBuffer.hitPosition = hit ? /*direction.normalized * rayHit.distance*/ rayHit.point : hitSmth ? rayHit.point: direction.normalized * 500f;
            m_laserBuffer.hit = hit;
        }
        //preparing to send the message
        OnServerTick -= SendLaser;
        OnServerTick += SendLaser;
    }

    /// <summary>
    /// function called when the server receives a message of type PcInvisibility
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
        if(m_beaconsPositionsBuffer.data == null) return;
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
    private void OnServerReceiveBombExplosion(NetworkConnection p_conn, BombExplosion p_bombExplosion)
    {

        Vector2 pcFlatPosition = new Vector2(m_pcTransformBuffer.position.x,m_pcTransformBuffer.position.z);
        Vector2 bombFlatPosition = new Vector2(p_bombExplosion.position.x,p_bombExplosion.position.z);
        bool hit = Vector2.Distance(pcFlatPosition, bombFlatPosition) < m_bombExplosionRange;
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
        OnServerTick -= SendElevatorActivation;
        OnServerTick += SendElevatorActivation;
    }

    private void OnReceiveTutorial(NetworkConnection p_conn, Tutorial p_tutorial) => m_isInTutorial = p_tutorial.isInTutorial;

    /// <summary/> function called when the server receives a message of type PotentialSpawnPoints
    /// <param name="p_conn">The connection from which originated the message</param>
    /// <param name="p_potentialSpawnPoints">The message sent by the Client to the Server</param>
    private void OnReceivePotentialSpawnpoints(NetworkConnection p_conn, PotentialSpawnPoints p_potentialSpawnPoints) {
        m_potentialSpawnPointsBuffer = p_potentialSpawnPoints;
        OnServerTick += SendPotentialSpawnPoints;
    }

    /// <summary/> function called when the server receives a message of type VrInitialValues
    /// <param name="p_conn">The connection from which originated the message</param>
    /// <param name="p_vrInitialValues">The message sent by the Client to the Server</param>
    private void OnReceiveVRInitialValues(NetworkConnection p_conn, VrInitialValues p_vrInitialValues) {
        m_fingertipOffset = p_vrInitialValues.fingertipOffset;
        m_pcNetworkConnection?.Send(new VrInitialValues(){fingertipOffset = m_fingertipOffset});
    }

    #endregion


    #region SERVER SENDERS
    
    private void SendTeleported()
    {
        m_vrNetworkConnection.Send(m_teleporttedBuffer);
        OnServerTick -= SendTeleported;
    }
    
    private void SendRemoveTp()
    {
        m_vrNetworkConnection?.Send(new RemoveTp());
        OnServerTick -= SendRemoveTp;
    } 
    
    private void SendDropTp()
    {
        m_vrNetworkConnection?.Send(m_dropTpBuffer);
        OnServerTick -= SendDropTp;
    }
    
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
    private void SendElevatorActivation() {
        OnServerTick -= SendElevatorActivation;
        SendToBothClients(m_elevatorActivationBuffer);
    }

    private void SendActivateFlair()
    {
        OnServerTick -= SendActivateFlair;
        SendToBothClients(m_flairBuffer);
        StartCoroutine(Flair(m_flairDetonationTime, m_flashDuration));
    }

    private void SendActivateBlind()
    {
        OnServerTick -= SendActivateBlind;
        SendToBothClients(m_activateBlindBuffer);
    }

    private void SendDeActivateBlind()
    {
        OnServerTick -= SendDeActivateBlind;
        SendToBothClients(new DeActivateBlind());
    }
    
    private void SendLeureTransform()
    {
        OnServerTick -= SendLeureTransform;
        m_vrNetworkConnection?.Send(m_leureBuffer);
    }
    private void SendDestroyLeure() {
        OnServerTick -= SendDestroyLeure;
        m_vrNetworkConnection?.Send(new DestroyLeure());
    }
    private void SendSpawnLeure()
    {
        m_vrNetworkConnection?.Send(new SpawnLeure());
        OnServerTick -= SendSpawnLeure;
    }

    private void SendPotentialSpawnPoints() {
        OnServerTick -= SendPotentialSpawnPoints;
        m_vrNetworkConnection?.Send(m_potentialSpawnPointsBuffer);
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