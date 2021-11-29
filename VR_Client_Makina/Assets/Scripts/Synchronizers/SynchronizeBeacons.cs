using System;
using System.Collections;
using System.Collections.Generic;
using CustomMessages;
using Network;
using UnityEngine;

namespace Synchronizers {
    public class SynchronizeBeacons : Synchronizer {

        [SerializeField] private GameObject m_prefabBeacon = null;

        [SerializeField] private Color m_undetectedColor = Color.red;
        [SerializeField] private Color m_detectedColor = Color.green;
        [SerializeField] private Color m_grabbedColor = Color.blue;

        private class BeaconInfo {
            public GameObject gameObject;
            public bool isDetecting;
            public bool isGrabbed;
            public BeaconInfo(GameObject p_go) {
                isDetecting = false;
                isGrabbed = false;
                gameObject = p_go;
            }
        }
        
        private List<BeaconInfo> m_beacons = new List<BeaconInfo>();

        public delegate void NewBeaconDelegator(BeaconBehavior p_beaconBehavior);

        public static NewBeaconDelegator OnNewBeacon;

        private void Awake() {
            if(m_prefabBeacon == null) Debug.LogError("You forgot to serialize a beacon prefab here ! (╬ ಠ益ಠ)", this);

            MyNetworkManager.OnReceiveBeaconsPositions += UpdatePositions;
        }

        private void Start() {
            MyNetworkManager.OnReceiveBeaconDetectionUpdate += UpdateDetection;
            MyNetworkManager.OnReceiveDestroyedBeacon += DestroyBeacon;
        }

        private void Update() {
            if (m_beacons.Count < 1) return;
            Vector3[] positionses = new Vector3[m_beacons.Count];

            for (int i = 0; i < positionses.Length; i++) {
                positionses[i] = m_beacons[i].gameObject.transform.position;
            }
            
            MyNetworkManager.singleton.SendVrData(new BeaconsPositions(){positions = positionses});
        }

        private void UpdatePositions(BeaconsPositions p_beaconsPositions) {
            //
            // for(int i = 0; i < m_beacons.Count; i++) {
            //     GameObject beacon = m_beacons[i];
            //     if(beacon.transform.position != p_beaconsPositions.positions[i]) beacon.transform.position = p_beaconsPositions.positions[i];
            // }
            
            
            //We add new beacons if we find new ones in the data
            if (p_beaconsPositions.positions.Length > m_beacons.Count) {
                int prevCount = m_beacons.Count;
                for (int i = 0; i < p_beaconsPositions.positions.Length - prevCount; i++) {
                    GameObject go = Instantiate(m_prefabBeacon, p_beaconsPositions.positions[prevCount + i], new Quaternion(0, 0, 0, 0));
                    m_beacons.Add(new BeaconInfo(go));
                    BeaconBehavior script = go.GetComponent<BeaconBehavior>();
                    script.m_index = m_beacons.Count - 1;
                    script.m_synchronizer = this;
                    OnNewBeacon?.Invoke(go.GetComponent<BeaconBehavior>());
                }
            }
        }
        
        private void DestroyBeacon(DestroyedBeacon p_destroyedBeacon)
        {
            Debug.Log(m_beacons.Count);
            int index = p_destroyedBeacon.index;
            m_beacons[index].gameObject.GetComponent<GrabbableObject>().DestroyMaSoul();
            m_beacons.RemoveAt(index);
            Debug.Log(m_beacons.Count);
        }
        
        private void UpdateDetection(BeaconDetectionUpdate p_beaconDetectionUpdate) {
            
            m_beacons[p_beaconDetectionUpdate.index].isDetecting = p_beaconDetectionUpdate.playerDetected;

            if (m_beacons[p_beaconDetectionUpdate.index].isGrabbed) return;

            Color color = p_beaconDetectionUpdate.playerDetected ? m_detectedColor : m_undetectedColor;
            ChangeColorOfBeacon(p_beaconDetectionUpdate.index, color);
        }

        public void BeaconGrabbed(int p_beaconIndex) {

            m_beacons[p_beaconIndex].isGrabbed = true;
            ChangeColorOfBeacon(p_beaconIndex, m_grabbedColor);
        }

        public void BeaconLetGo(int p_beaconIndex) {

            m_beacons[p_beaconIndex].isGrabbed = false;
            ChangeColorOfBeacon(p_beaconIndex, m_beacons[p_beaconIndex].isDetecting ? m_detectedColor : m_undetectedColor);
        }
        
        private void ChangeColorOfBeacon(int p_index, Color p_color){
            
            GameObject child = m_beacons[p_index].gameObject.transform.GetChild(0).gameObject;
            Material mat = child.GetComponent<MeshRenderer>().material;
            mat.SetColor("_Beacon_Color", p_color);

            m_beacons[p_index].gameObject.GetComponent<MeshRenderer>().material.color = p_color;
            
        }
    }
}