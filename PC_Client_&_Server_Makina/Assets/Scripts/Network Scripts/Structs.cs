using Mirror;
using UnityEngine;

/// <summary/>The structs derived from the NetworkMessage interface of Mirror that wil be sent between clients and servers as messages.

namespace CustomMessages
{
    #region Transforms
    
    /// <summary/> The position and rotation of the pc player
    public struct PcTransform : NetworkMessage
    {
        public Vector3 position;
        public Quaternion rotation;
    }

    /// <summary/> The position and rotations of all three of the Vr player's limbs
    public struct VrTransform : NetworkMessage
    {
        public Vector3 positionHead;
        public Quaternion rotationHead;

        public Vector3 positionLeftHand;
        public Quaternion rotationLeftHand;

        public Vector3 positionRightHand;
        public Quaternion rotationRightHand;
    }
    
    #endregion

    #region Misc Data
    
    public enum ClientConnection
    {
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
    }
    
    //TODO add this to that ^
    /// <summary/> The position and rotations of all the Hearts in the game
    public struct HeartTransforms : NetworkMessage
    {
        public Vector3[] positions;
        public Quaternion[] rotations;
    }
    
    /// <summary/> The index of a destroyed heart
    public struct HeartBreak : NetworkMessage
    {
        public int index;
    }
    
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
}