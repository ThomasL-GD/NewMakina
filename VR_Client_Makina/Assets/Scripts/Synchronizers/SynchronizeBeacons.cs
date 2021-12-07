using System;
using System.Collections;
using System.Collections.Generic;
using CustomMessages;
using Network;
using UnityEngine;

namespace Synchronizers {
    public class SynchronizeBeacons : SynchronizeLoadedObjectsAbstract {

        [Header("Beacons")]
        [SerializeField] private GameObject m_prefabBeaconAmmo = null;
        [HideInInspector] public float m_range = 20f;
        [SerializeField] [Range(0f, 1f)] public float m_inflateTime = 0.2f;
        

        [Header("Colors")]
        [SerializeField] private Color m_undetectedColor = Color.red;
        [SerializeField] private Color m_detectedColor = Color.green;
        [SerializeField] private Color m_unactiveColor = Color.blue;

        /// <summary> Contains a gameobject of a beacon and its server ID, additionally contains booleans that explain the beacon state </summary>
        private class BeaconInfo {
            /// <summary>The gameobject of the beacon</summary>
            public readonly GameObject gameObject;
            
            /// <summary> If true, the PC player is in the range of this beacon </summary>
            public bool isDetecting;
            
            /// <summary> If true, the beacon has touched the ground after being thrown, if false, the beacon is either loaded or in a hand of the player or in the air </summary>
            public bool isOnTheGroundAndSetUp;

            /// <summary> The ID the server uses for verifications, basically don't touch this </summary>
            public float serverID;

            /// <summary> The constructor of a beaconInfo class </summary>
            /// <param name="p_go">The GameObject of the beacon</param>
            /// <param name="p_serverID">The server id of the beacon</param>
            public BeaconInfo(GameObject p_go, float p_serverID) {
                isDetecting = false;
                isOnTheGroundAndSetUp = false;
                gameObject = p_go;
                serverID = p_serverID;
            }
        }
        
        private List<BeaconInfo> m_beacons = new List<BeaconInfo>();
        private BeaconBehavior[] m_beaconsInTheArm = null;

        public delegate void BeaconDelegator(BeaconBehavior p_beaconBehavior);

        public static BeaconDelegator OnNewBeacon;
        public static BeaconDelegator OnBeaconSetUp;
        
        
        private static readonly int CodeBeaconColor = Shader.PropertyToID("_Beacon_Color");

        private void Awake() {
            if(m_prefabBeaconAmmo == null) Debug.LogError("You forgot to serialize a beacon prefab here ! (╬ ಠ益ಠ)", this);
        }

        private void Start() {
            MyNetworkManager.OnReceiveInitialData += SetMaxBeaconsSlots;
            MyNetworkManager.OnReceiveSpawnBeacon += CreateBeacon;
            MyNetworkManager.OnReceiveBeaconDetectionUpdate += UpdateDetection;
            MyNetworkManager.OnReceiveDestroyedBeacon += DestroyBeacon;
            MyNetworkManager.OnReceiveInitialData += SetRangeOfBeacons;
        }

        /// <summary>Just sets maxSlotsForBeacons according to the server's InitialData</summary>
        /// <param name="p_initialData">The message sent by the server</param>
        private void SetMaxBeaconsSlots(InitialData p_initialData) {
            m_maxSlotsLoading = p_initialData.maximumBeaconCount;
            m_beaconsInTheArm = new BeaconBehavior[m_maxSlotsLoading];
            if(m_loadingPositions.Length < m_maxSlotsLoading) Debug.LogWarning("There is more possible loaded beacons than loaded beacon position, so please do your game designer's job, we ain't paying you a SMIC for nothing", this);
        }

        private void Update() {
            if (m_beacons.Count < 1) return;
            BeaconData[] positionses = new BeaconData[m_beacons.Count];

            for (int i = 0; i < positionses.Length; i++) {
                positionses[i].position = m_beacons[i].gameObject.transform.position;
                positionses[i].beaconID = m_beacons[i].serverID;
            }
            
            MyNetworkManager.singleton.SendVrData(new BeaconsPositions(){data = positionses});
        }

        /// <summary>
        /// Call this to create a new beacon.
        /// The process of adding the beacon to every database that needs it is automated here.
        /// </summary>
        /// <param name="p_spawnBeacon">The server message of the new beacon</param>
        private void CreateBeacon(SpawnBeacon p_spawnBeacon) {
            
            GameObject go = Instantiate(m_prefabBeaconAmmo);
            
            m_beacons.Add(new BeaconInfo(go, p_spawnBeacon.beaconID));
            
            BeaconBehavior script = go.GetComponent<BeaconBehavior>();
            script.m_index = m_beacons.Count - 1;
            script.m_synchronizer = this;

            OnNewBeacon?.Invoke(script);
        }
        
        
        /// <summary> Is called when we receive a DestroyedBeacon message from server </summary>
        /// <param name="p_destroyedBeacon">The message from the server</param>
        private void DestroyBeacon(DestroyedBeacon p_destroyedBeacon)
        {
            int index = p_destroyedBeacon.index;
            m_beacons[index].gameObject.GetComponent<GrabbableObject>().DestroyMaSoul();
            m_beacons.RemoveAt(index);
        }
        
        /// <summary>
        /// Is called when we receive a DestroyedBeacon message from server
        /// Will update the state and the color of a beacon depending on if they are in range or not according to the server
        /// </summary>
        /// <param name="p_beaconDetectionUpdate">The message from the server</param>
        private void UpdateDetection(BeaconDetectionUpdate p_beaconDetectionUpdate) {
            
            m_beacons[p_beaconDetectionUpdate.index].isDetecting = p_beaconDetectionUpdate.playerDetected;
            
            //Debug.Log($"Is player detected ? actually the {p_beaconDetectionUpdate.index} is {(p_beaconDetectionUpdate.playerDetected? "REALLY" : "NOT" )} detecting", this);
            
            ActualiseColorOfBeacon(p_beaconDetectionUpdate.index);
        }
        
        /// <summary> Will actualise the color of a beacon according to what it should display according to the beacon state </summary>
        /// <param name="p_index">The index of the beacon</param>
        public void ActualiseColorOfBeacon(int p_index) {

            Color newColor;

            if (!m_beacons[p_index].isOnTheGroundAndSetUp) newColor = m_unactiveColor;
            else switch (m_beacons[p_index].isDetecting) {
                case true:
                    newColor = m_detectedColor;
                    break;
                case false:
                    newColor = m_undetectedColor;
                    break;
            }
            
            GameObject child = m_beacons[p_index].gameObject.transform.GetChild(0).gameObject;
            Material mat = child.GetComponent<MeshRenderer>().material;
            mat.SetColor(CodeBeaconColor, newColor);

            m_beacons[p_index].gameObject.GetComponent<MeshRenderer>().material.color = newColor;
            
        }

        /// <summary> Changes the range of the beacon feedback in the prefab itself </summary>
        /// <param name="p_initialData">The message from the server</param>
        private void SetRangeOfBeacons(InitialData p_initialData) {
            m_range = p_initialData.beaconRange;
            foreach (Transform child in m_prefabBeaconAmmo.transform) {
                if (child.gameObject.TryGetComponent(out InflateToSize script)) {
                    script.m_targetScale = (m_range * 2f) / m_prefabBeaconAmmo.transform.localScale.x;
                    script.m_inflationTime = m_inflateTime;
                }
            }
        }

        /// <summary> Send to the network manager the ActivateBeacon message </summary>
        /// <param name="p_index">The index of the beacon that gets activated</param>
        public void SendBeaconActivation(int p_index) {
            SetActivationOfABeacon(p_index, true);
            OnBeaconSetUp?.Invoke(m_beacons[p_index].gameObject.GetComponent<BeaconBehavior>());
            MyNetworkManager.singleton.SendVrData(new ActivateBeacon(){index = p_index, beaconID = m_beacons[p_index].serverID});
        }

        /// <summary> Change the value of the isOnTheGroundAndSetUp of a beacon according to its index </summary>
        /// <param name="p_index">The index of the beacon</param>
        /// <param name="p_isOnTheGroundAndSetUp">If the beacon is loaded or not</param>
        private void SetActivationOfABeacon(int p_index, bool p_isOnTheGroundAndSetUp) {
            m_beacons[p_index].isOnTheGroundAndSetUp = p_isOnTheGroundAndSetUp;
        }
    }
}