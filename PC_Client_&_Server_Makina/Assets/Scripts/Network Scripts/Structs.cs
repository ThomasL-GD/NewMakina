using Mirror;
using UnityEngine;

/// <summary/>The structs derived from the NetworkMessage interface of Mirror that wil be sent between clients and servers as messages.

namespace CustomMessages {
    
    #region Transforms
    
    /// <summary/> The position and rotation of the pc player
    public struct PcTransform : NetworkMessage {
        public Vector3 position;
        public Quaternion rotation;
    }

    /// <summary/> The position and rotations of all three of the Vr player's limbs
    public struct VrTransform : NetworkMessage {
        public Vector3 positionHead;
        public Quaternion rotationHead;

        public Vector3 positionLeftHand;
        public Quaternion rotationLeftHand;
        public bool isLeftHandClenched;

        public Vector3 positionRightHand;
        public Quaternion rotationRightHand;
        public bool isRightHandClenched;
    }

    public struct PotentialSpawnPoints : NetworkMessage {
        public Vector3[] position;
    }
    
    #endregion
    
    #region Tp_Rollback

    public struct Teleported : NetworkMessage {
        public Vector3 teleportOrigin;
        public Vector3 teleportDestination;
    }

    public struct DropTp : NetworkMessage {
        public Vector3 tpPosition;
    }

    public struct RemoveTp : NetworkMessage { }

    #endregion

    #region Misc Data

    public struct ActivateFlair : NetworkMessage {
        public Vector3 startPosition;
    }

    public struct ActivateBlind : NetworkMessage {
        public float blindIntensity;
    }
    public struct DeActivateBlind : NetworkMessage {}

    public enum ClientConnection {
        PcPlayer,
        VrPlayer
    }
    
    /// <summary/> The ping sent to the server telling it the a client has connected
    public struct ClientConnect : NetworkMessage
    {
        public ClientConnection client;
    }

    public struct GameEnd : NetworkMessage
    {
        public ClientConnection winningClient;
    }
    
    /// <summary/> The initial data that will went to the clients as they connect
    public struct InitialData : NetworkMessage
    {
        public int healthPcPlayer;
        public int healthVrPlayer;
        public float beaconRange;
        public int maximumBeaconCount;
        public int maximumBombsCount;
        public float elevatorSpeed;
        public float elevatorWaitTime;
        public float flairRaiseSpeed;
        public float flairDetonationTime;
        public Vector3[] heartPositions;
        public Quaternion[] heartRotations;
        public float bombDetonationTime;
        public float bombExplosionRange;
        public float heartRange;
        public float heartConquerTime;
        public ushort numberOfSpawnPointsToDisplay;
    }
    
    public struct RestartGame : NetworkMessage
    {
        
    }
    
    public struct ReadyToPlay : NetworkMessage {
        
    }

    public struct Tutorial : NetworkMessage {
        public bool isInTutorial;
    }

    public struct ElevatorActivation : NetworkMessage
    {
        public int index;
    }
    
    /// <summary/> The index of a destroyed heart
    public struct HeartBreak : NetworkMessage
    {
        public int index;
    }
    
    /// <summary/> The index of a destroyed heart
        public struct HeartConquerStart : NetworkMessage
    {
        public float time;
    }
    
    /// <summary/> The index of a destroyed heart
    public struct HeartConquerStop : NetworkMessage {}
    
    /// <summary/> The ping sent to the server telling it if the players is visible or invisible
    public struct PcInvisibility : NetworkMessage
    {
        public bool isInvisible;
    }

    #endregion

    #region Lazer

    public enum LaserState {
        Aiming,
        Shooting,
        CancelAiming
    }

    
    /// <summary/> The ping sent to the server telling it the Vr player has interacted with its laser
    public struct VrLaser : NetworkMessage {

        public LaserState laserState;
    }

    /// <summary/> The message sent to the clients giving them laser information updates
    public struct Laser : NetworkMessage
    {
        public LaserState laserState;
        
        public Vector3 origin;
        public Quaternion rotation;
        public float length;
        public Vector3 hitPosition;

        public bool hit;
    }

    #endregion

    #region Beacons

    /// <summary/> Sent when a new beacon spawns
    public struct SpawnBeacon : NetworkMessage
    {
        public float beaconID;
    }
    
    /// <summary/> The index of the destroyed Beacon
    public struct BeaconData
    {
        public Vector3 position;
        public float beaconID;
        public bool detectingPlayer; //TODO this for Blue
        public bool isActive;
    }
    
    /// <summary/> The position of every beacon in the game
    public struct BeaconsPositions : NetworkMessage {
        public BeaconData[] data;
    }
    
    /// <summary/> Sent when a beacon is newly activated
    public struct ActivateBeacon : NetworkMessage
    {
        public int index;
        public float beaconID;
    }

    /// <summary/> The message sent when the beacon detects or stops detecting the pc player
    public struct BeaconDetectionUpdate : NetworkMessage
    {
        public bool playerDetected;
        public int index;
        public float beaconID;
    }
    
    /// <summary/> The message sent when a beacon gets destroyed
    public struct DestroyedBeacon : NetworkMessage
    {
        public int index;
        public float beaconID;
    }

    #endregion

    #region Bombs

    /// <summary/> Sent when a new bomb spawns
    public struct SpawnBomb : NetworkMessage {
        public float bombID;
    }
    
    /// <summary/> The index of the destroyed bomb
    public struct BombData {
        
        public Vector3 position;
        public Quaternion rotation;
        public float bombID;
    }
    
    /// <summary/> The position of every bomb in the game
    public struct BombsPositions : NetworkMessage {
        public BombData[] data;
    }
    
    /// <summary/> Sent when a bomb is exploding, then sent back to know if the explosion killed or not
    public struct BombExplosion : NetworkMessage {

        public Vector3 position;
        public int index;
        public float bombID;
        /// <summary>Is sent by server only</summary>
        public bool hit;
    }

    /// <summary/> Sent when a bomb is activated to enable feedback on the PC side
    public struct BombActivation : NetworkMessage
    {
        public int index;
        public float bombID;
    }

    #endregion

    #region Leure
    public struct SpawnLeure : NetworkMessage {}

    public struct LeureTransform : NetworkMessage
    {
        public Vector3 position;
        public Quaternion rotation;
    }
    public struct DestroyLeure : NetworkMessage {}

    #endregion
}