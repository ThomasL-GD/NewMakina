using CustomMessages;
using Mirror;
using Player_Scripts.Reloading;
using Synchronizers;
using UnityEngine;
using UnityEngine.UI;

public class TeleportRollBack : AbstractMechanic
{
    [SerializeField] private KeyCode m_placeOrTeleportKey;
    [SerializeField] private GameObject m_teleportLocationPrefab;

    [SerializeField] private RawImage m_podIcon;
    [SerializeField] private RawImage m_teleportIcon;
    
    private GameObject m_teleportLocation;
    private bool m_placed;
    private bool m_canUse = true;

    [SerializeField] private ReloadingAbstract m_coolDownScript;
    
    private void Awake()
    {
        m_coolDownScript.OnReloading += Reset;
        ClientManager.OnReceiveReadyToPlay += Reset;
        SynchronizeRespawn.OnPlayerRespawn += Reset;
        ClientManager.OnReceiveInitialData += Reset;
    }

    private void Reset(InitialData p_initialdata) => Reset();
    private void Reset(ReadyToPlay p_activateblind) => Reset();
    private void Reset()
    {
        if(m_placed) NetworkClient.Send(new RemoveTp());
        m_placed = false;
        m_canUse = true;
        m_teleportIcon.enabled = false;
        m_podIcon.enabled = true;
        if(m_teleportLocation != null) Destroy(m_teleportLocation);
    }

    // Update is called once per frame
    void Update()
    {
        if (m_canUse && Input.GetKeyDown(m_placeOrTeleportKey))
        {
            if (m_placed)
            {
                Teleport(m_teleportLocation.transform.position);
                return;
            }

            Vector3 position = transform.position;

            m_teleportLocation = Instantiate(m_teleportLocationPrefab, position, transform.rotation);
            NetworkClient.Send(new DropTp() {tpPosition = position});
            m_teleportIcon.enabled = true;
            m_podIcon.enabled = false;
            m_placed = true;
        }
    }
    
    private void Teleport(Vector3 p_destination)
    {
        NetworkClient.Send(new Teleported(){teleportDestination = p_destination, teleportOrigin = transform.position});
        transform.position = p_destination;
        m_canUse = false;
        m_coolDownScript.StartReloading();
        m_teleportIcon.enabled = false;
        m_podIcon.enabled = false;
        if(m_placed) NetworkClient.Send(new RemoveTp());
        m_placed = false;
        Destroy(m_teleportLocation);
    }
}
