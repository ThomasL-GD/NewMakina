using System;
using CustomMessages;
using Mirror;
using Player_Scripts.Reloading;
using Synchronizers;
using TMPro;
using UnityEngine;

public class TeleportRollBack : AbstractMechanic {
    
    [SerializeField] private KeyCode m_placeOrTeleportKey;
    [SerializeField] private GameObject m_teleportLocationPrefab;

    private GameObject m_teleportLocation;
    private bool m_placed;
    private bool m_canUse = true;

    [SerializeField] private ReloadingAbstract m_coolDownScript;

    [SerializeField] private bool m_isTutorial = false;

    [SerializeField] private float m_yeetForce = 20f;

    [SerializeField] private TextMeshProUGUI m_teleportTutorialText;
     
    public static Action a_onPlaceTpPoint;
    public static Action a_onTeleportBack;
    
    private void Awake()
    {
        m_coolDownScript.OnReloading += Reset;
        m_coolDownScript.OnReloading += SoundManager.Instance.ReloadSound;
        ClientManager.OnReceiveReadyToFace += Reset;
        ClientManager.OnReceiveReadyToGoIntoTheBowl += Reset;
        SynchronizeRespawn.OnPlayerRespawn += Reset;
        ClientManager.OnReceiveInitialData += Reset;
    }

    private void Reset(InitialData p_initialdata) => Reset();
    private void Reset(ReadyToFace p_activateblind) => Reset();
    private void Reset(ReadyToGoIntoTheBowl p_activateblind) => Reset();
    private void Reset()
    {
        UIManager.Instance.ResetTeleportRollbackCooldown();
        if(m_placed) NetworkClient.Send(new RemoveTp());
        m_placed = false;
        m_thrown = false;
        m_canUse = true;
        if(m_teleportLocation != null) Destroy(m_teleportLocation);
    }

    private bool m_thrown = false;
    
    // Update is called once per frame
    void Update()
    {
        if(!SynchronizeInitialData.vrConnected && !m_isTutorial) return;
        if (m_canUse && Input.GetKeyDown(m_placeOrTeleportKey))
        {
            if (m_placed)
            {
                Teleport(m_teleportLocation.transform.position);
                return;
            }

            if(m_thrown == false) ThrowTP();
        }
    }
    
    private void ThrowTP() {
        m_thrown = true;
        Transform camera = CameraAndUISingleton.camera.transform;
        
        m_teleportLocation = Instantiate(m_teleportLocationPrefab, camera.position, camera.rotation);
        m_teleportLocation.transform.RotateAround(Vector3.up, 90f);
        m_teleportLocation.GetComponent<EnableTPPoint>().m_parentScript = this;
        Physics.IgnoreCollision(m_teleportLocation.GetComponent<Collider>() , gameObject.GetComponent<Collider>());
        
        m_teleportLocation.GetComponent<Rigidbody>().AddForce(new Vector3(camera.forward.x,0f,camera.forward.z) * m_yeetForce,ForceMode.Impulse);
        
        a_onPlaceTpPoint?.Invoke();
    }

    public void SetTPoint(Vector3 p_position) {
        NetworkClient.Send(new DropTp() {tpPosition = p_position});
        UIManager.Instance.PlacedTeleporter();
        m_teleportTutorialText.text = "Teleport back to placed point";
        m_placed = true;
    }
    
    private void Teleport(Vector3 p_destination)
    {
        m_teleportTutorialText.text = "Place a teleport point";
        NetworkClient.Send(new Teleported(){teleportDestination = p_destination, teleportOrigin = transform.position});
        UIManager.Instance.TeleportRollback();
        transform.position = p_destination;
        m_canUse = false;
        m_coolDownScript.StartReloading();
        if(m_placed) NetworkClient.Send(new RemoveTp());
        m_placed = false;
        
        Destroy(m_teleportLocation);
        
        a_onTeleportBack?.Invoke();
    }
}
