using System;
using System.Collections;
using System.Collections.Generic;
using CustomMessages;
using Network;
using UnityEngine;

namespace Synchronizers {
    public class SynchronizeBeacons : Synchronizer {

        [SerializeField] private GameObject m_prefabBeacon = null;
        private float m_range = 20f;

        [Header("Colors")]
        [SerializeField] private Color m_undetectedColor = Color.red;
        [SerializeField] private Color m_detectedColor = Color.green;
        [SerializeField] private Color m_grabbedColor = Color.blue;

        private class BeaconInfo {
            public readonly GameObject gameObject;
            public bool isDetecting;
            public bool isGrabbed;
            public bool isLoaded;
            public bool isUnactive => isGrabbed || isLoaded;

            public float serverID;

            public BeaconInfo(GameObject p_go, float p_id) {
                isDetecting = false;
                isGrabbed = false;
                isLoaded = false;
                gameObject = p_go;
                serverID = p_id;
            }
        }
        
        private List<BeaconInfo> m_beacons = new List<BeaconInfo>();
        private BeaconBehavior[] m_beaconsInTheArm = null;

        public delegate void NewBeaconDelegator(BeaconBehavior p_beaconBehavior);

        public static NewBeaconDelegator OnNewBeacon;
        private static readonly int CodeBeaconColor = Shader.PropertyToID("_Beacon_Color");


        [Header("Loading in Arm")]
        [SerializeField] public Transform[] m_loadingPositions;
        
        public static int maxSlotsForBeacons = 1;

        private void Awake() {
            if(m_prefabBeacon == null) Debug.LogError("You forgot to serialize a beacon prefab here ! (╬ ಠ益ಠ)", this);
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
            maxSlotsForBeacons = p_initialData.maximumBeaconCount;
            m_beaconsInTheArm = new BeaconBehavior[maxSlotsForBeacons];
            if(m_loadingPositions.Length < maxSlotsForBeacons) Debug.LogWarning("There is more possible loaded beacons than loaded beacon position, so please do your game designer's job, we ain't paying you a SMIC for nothing", this);
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
        /// TODO this
        /// </summary>
        /// <param name="p_spawnBeacon"></param>
        private void CreateBeacon(SpawnBeacon p_spawnBeacon) {

            BeaconLoading.s_synchronizer = this;
            
            GameObject go = Instantiate(m_prefabBeacon);
            
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
            Debug.Log(m_beacons.Count);
            int index = p_destroyedBeacon.index;
            m_beacons[index].gameObject.GetComponent<GrabbableObject>().DestroyMaSoul();
            m_beacons.RemoveAt(index);
            Debug.Log(m_beacons.Count);
        }
        
        /// <summary>
        /// Is called when we receive a DestroyedBeacon message from server
        /// Will update the state and the color of a beacon depending on if they are in range or not according to the server
        /// </summary>
        /// <param name="p_beaconDetectionUpdate">The message from the server</param>
        private void UpdateDetection(BeaconDetectionUpdate p_beaconDetectionUpdate) {
            
            m_beacons[p_beaconDetectionUpdate.index].isDetecting = p_beaconDetectionUpdate.playerDetected;
            
            Debug.Log($"Is player detected ? actually the {p_beaconDetectionUpdate.index} is {(p_beaconDetectionUpdate.playerDetected? "REALLY" : "NOT" )} detecting", this);
            
            ActualiseColorOfBeacon(p_beaconDetectionUpdate.index);
        }

        /// <summary>Is called by a beacon when it gets grabbed to change the color of itself </summary>
        /// <param name="p_beaconIndex">The index of the beacon</param>
        public void BeaconGrabbed(int p_beaconIndex) {

            m_beacons[p_beaconIndex].isGrabbed = true;
            ActualiseColorOfBeacon(p_beaconIndex);
        }

        /// <summary> Is called by a beacon when it gets let go to change the color of itself according to its state </summary>
        /// <param name="p_beaconIndex">The index of the beacon</param>
        public void BeaconLetGo(int p_beaconIndex) {

            m_beacons[p_beaconIndex].isGrabbed = false;
            ActualiseColorOfBeacon(p_beaconIndex);
        }
        
        /// <summary> Will actualise the color of a beacon according to what it should display according to the beacon state </summary>
        /// <param name="p_index">The index of the beacon</param>
        private void ActualiseColorOfBeacon(int p_index) {

            Color newColor;

            if (m_beacons[p_index].isUnactive) newColor = m_grabbedColor;
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
            foreach (Transform child in m_prefabBeacon.transform) {
                child.localScale = new Vector3(m_range, m_range, m_range);
            }
        }
    }
}