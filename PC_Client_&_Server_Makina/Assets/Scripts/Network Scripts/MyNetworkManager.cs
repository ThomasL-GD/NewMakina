using Mirror;
using UnityEngine;

/// <summary>
/// This NetworkManger is a derivative from the mirror NetworkManager component.
/// It is made to accomodate a Client/Server architecture on a cross platform project through the use of Mirror's messages.
/// </summary>
public class MyNetworkManager : NetworkManager
{
    //Singleton time ! 	(˵ ͡° ͜ʖ ͡°˵)
    public new static MyNetworkManager singleton { get; private set; }


    /// <summary> The delegate that will be called when the local player connects </summary>
    public delegate void OnConnectAsClient();
    public static OnConnectAsClient del_onConnectAsClient;
    
    /// <summary> The delegate that will be called when the local player connects </summary>
    public delegate void OnHostServer();
    public static OnHostServer del_onHostServer;
    
    /// <summary> The delegate that will be called when the local player connects </summary>
    public delegate void OnClientConnection( NetworkConnection p_conn );
    public static OnClientConnection del_onClientConnection;


    /// <summary>
    /// Is that... a singleton setup ?
    /// *Pokédex's voice* A singleton, a pretty common pokécode you can find in a lot of projects, it allows anyone to
    /// call it and ensure there is only one script of this type in the entire scene !
    /// </summary>
    private new void Awake()
    {
        // if the singleton hasn't been initialized yet
        if (singleton != null && singleton != this)
        {
            gameObject.SetActive(false);
            Debug.LogWarning("BROOOOOOOOOOOOOOOOOOO ! There are too many Singletons broda", this);
        }else singleton = this;
    }

    private new void Start() => StartHost();

    /// <summary>
    /// The function that is called each time a player connects to the server.
    /// We assume that the latest player to connect is the VR player.
    /// </summary>
    /// <param name="p_conn"> The NetworkConnection of the newly connected client </param>
    public override void OnServerConnect(NetworkConnection p_conn) => del_onClientConnection?.Invoke(p_conn);


    /// <summary>
    /// The function that is called each time the local player connects to any server.
    /// We assume that the player to connect is the PC player.
    /// </summary>
    public override void OnStartClient() => del_onConnectAsClient?.Invoke();

    
    /// <summary>
    /// The function that is called when the server is hosted.
    /// </summary>
    public override void OnStartServer() => del_onHostServer?.Invoke();

}