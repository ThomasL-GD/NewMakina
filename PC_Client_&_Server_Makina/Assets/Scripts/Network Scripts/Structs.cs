using Mirror;
using UnityEngine;

/// <summary>The structs derived from the NetworkMessage interface of Mirror that wil be sent between clients and servers as messages. </summary>

namespace CustomMessages
{
    /// <summary>
    /// The position and rotation of the pc player
    /// </summary>
    public struct PcTransform : NetworkMessage
    {
        public Vector3 position;
        public Quaternion rotation;
    }

    /// <summary>
    /// The initial data that will went to the clients as they connect
    /// </summary>
    public struct InitialData : NetworkMessage
    {
        public int healthPcPlayer;
        public int healthVrPlayer;
        public float beaconRange;
    }
    
    /// <summary>
    /// The ping sent to the server telling it if the players is visible or invisible
    /// </summary>
    public struct PcInvisibility : NetworkMessage
    {
        public bool isInvisible;
    }
    
    /// <summary>
    /// The position and rotations of all three of the Vr player's limbs
    /// </summary>
    public struct VrTransform : NetworkMessage
    {
        public Vector3 positionHead;
        public Quaternion rotationHead;

        public Vector3 positionLeftHand;
        public Quaternion rotationLeftHand;

        public Vector3 positionRightHand;
        public Quaternion rotationRightHand;
    }
    
    /// <summary>
    /// The position and rotations of all the Hearts in the game 
    /// </summary>
    public struct HeartTransforms : NetworkMessage
    {
        public Vector3[] positions;
        public Quaternion[] rotations;
    }
    
    /// <summary> // TODO Blue Sorry m8 I fixed it niggaaaaaa
    /// The index of a destroyed heart
    /// </summary>
    public struct HeartBreak : NetworkMessage
    {
        public int index;
    }
    
    /// <summary>
    /// The position every beacon in the game 
    /// </summary>
    public struct BeaconsPositions : NetworkMessage {
        public Vector3[] positions;
    }

    /// <summary>
    /// The index of the destroyed Beacon
    /// </summary>
    public struct DestroyedBeacon : NetworkMessage
    {
        public int index;
    }

    public enum VrPlayerHand
    {
        rightHand,
        leftHand
    }
    
    /// <summary>
    /// The message sent when the Vr player Grabs or drops a beacon
    /// </summary>
    public struct VrPlayerInteractWithBeacon : NetworkMessage
    {
        public VrPlayerHand hand;
        public bool grabbed;
        public int index;
        public Vector3 offset;
    }
    
    /// <summary>
    /// The message sent when the beacon detects or stops detecting the pc player
    /// </summary>
    public struct BeaconDetectionUpdate : NetworkMessage
    {
        public bool playerDetected;
        public int index;
    }

    public enum ClientConnection
    {
        PcPlayer,
        VrPlayer
    }
    
    /// <summary>
    /// The ping sent to the server telling it the a client has connected
    /// </summary>
    public struct ClientConnect : NetworkMessage
    {
        public ClientConnection client;
    }

    public struct GameEnd : NetworkMessage
    {
        public ClientConnection winningClient;
    }
    
    public enum LaserState {
        Aiming,
        Shooting,
        CancelAiming
    }

    
    /// <summary>
    /// The ping sent to the server telling it the Vr player has interacted with its laser
    /// </summary>
    public struct VrLaser : NetworkMessage {

        public LaserState laserState;
    }

    /// <summary>
    /// The message sent to the clients giving them laser information updates 
    /// </summary>
    public struct Laser : NetworkMessage
    {
        public LaserState laserState;
        
        public Vector3 origin;
        public Quaternion rotation;
        public Vector3 hitPosition;

        public bool hit;
    }
}