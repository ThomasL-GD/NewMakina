using System;
using System.Collections.Generic;
using System.Globalization;
using CustomMessages;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

namespace Synchronizers {
    public class SynchronizeBeacons : Synchronizer<SynchronizeBeacons> {

        [SerializeField][Tooltip("The beacon prefab that will be instantiated for each beacon")] private GameObject m_prefabBeaconActive = null;
        [SerializeField][Tooltip("The beacon prefab that will be instantiated for each beacon")] private GameObject m_prefabBeaconInactive = null;

        [SerializeField][Tooltip("The color of the beacon when the player is undetected")] private Color m_undetectedColor = Color.red;
        [SerializeField][Tooltip("The color of the beacon when the player is detected")] private Color m_detectedColor = Color.green;

        /// <summary/> The beacons of the player that are stored away
        private List<Beacons> m_beacons = new List<Beacons>();

        [Serializable]
        private class Beacons
        {
            public GameObject beaconPrefabInstance;
            public float ID;
            public bool detected;
            public int bitMaskIndex;

            public Beacons(GameObject p_beaconPrefabInstance, float p_id, Vector3? p_position = null, bool p_detected = false, int p_BitMaskIndex = -1)
            {
                p_beaconPrefabInstance.name += p_id.ToString();
                beaconPrefabInstance = Instantiate(p_beaconPrefabInstance);
                beaconPrefabInstance.transform.position = p_position?? Vector3.zero;
                
                ID = p_id;
                detected = p_detected;
                bitMaskIndex = p_BitMaskIndex;
            }
        }
        
        private int m_beaconBitMaskDetected = 0;
        
        private float m_beaconRange;
        private static readonly int m_beaconColorProperty = Shader.PropertyToID("_Beacon_Color");
        private static readonly int m_beaconRangeShaderID = Shader.PropertyToID("_BeaconRange");
        private static readonly int m_beaconBitMaskShaderID = Shader.PropertyToID("_BeaconBitMask");
        private static readonly int m_beaconDetectionBitMaskShaderID = Shader.PropertyToID("_BeaconDetectionBitMask");


        /// <summary/> Spawning a beacon and adding it to the array
        /// <param name="p_spawnbeacon"> the message sent by the server </param>
        private void SpawnBeacons(SpawnBeacon p_spawnbeacon)
        {
            m_beacons.Add(new Beacons(m_prefabBeaconInactive, p_spawnbeacon.beaconID));
        }
        
        /// <summary/> Initiating the class by adding the right functions to the Client delegates
        private void Awake() {
            // Adding the right functions to the delegate
            ClientManager.OnReceiveSpawnBeacon += SpawnBeacons; 
            ClientManager.OnReceiveBeaconsPositions += UpdatePositions;
            ClientManager.OnReceiveDestroyedBeacon += DestroyBeacon;
            ClientManager.OnReceiveBeaconDetectionUpdate += UpdateDetection;
            ClientManager.OnReceiveInitialData += ReceiveInitialData;
            ClientManager.OnReceiveActivateBeacon += UpdateBeaconActivation;
            ClientManager.OnReceiveGameEnd += Reset;
        }

        /// <summary>Will destroy all the beacons to be ready to launch another game </summary>
        /// <param name="p_gameend">The message sent by the server</param>
        private void Reset(GameEnd p_gameend) {
            foreach (Beacons beacon in m_beacons) {
                Destroy(beacon.beaconPrefabInstance);
            }

            m_beaconBitMaskDetected = 0b00000000000;
            
            Shader.SetGlobalFloat(m_beaconBitMaskShaderID,0b00000000000);
            Shader.SetGlobalFloat(m_beaconDetectionBitMaskShaderID,0b00000000000);
            
            m_beacons.Clear();
        }

        /// <summary/> Updating a beacon to activate
        /// <param name="p_activatebeacon"> the beacon data to activate </param>
        private void UpdateBeaconActivation(ActivateBeacon p_activatebeacon)
        {
            int? index = FindBeaconFromID(p_activatebeacon.index, p_activatebeacon.beaconID,"Update Beacon Activation");

            if (index == null)
            {
                return;
            }

            Beacons oldBeacon = m_beacons[index ?? 0];
            int beaconBitMask = Shader.GetGlobalInt(m_beaconBitMaskShaderID);
            int bitMaskIndex = -2;
            
            for (int i = 0; i < 10; i++)
            {
                if ((beaconBitMask & 1 << i) != 1 << i)
                {
                    beaconBitMask |= 1 << i;
                    Shader.SetGlobalInt(m_beaconBitMaskShaderID, beaconBitMask);

                    Vector3 pos = oldBeacon.beaconPrefabInstance.transform.position;
                    
                    Shader.SetGlobalVector($"_beaconPosition_{i}", new Vector4(pos.x,pos.y,pos.z,0));
                    Shader.SetGlobalFloat($"_beaconTimer_{i}", Time.time);
                    bitMaskIndex = i;
                    m_beacons[index ?? 0] = new Beacons(m_prefabBeaconActive,oldBeacon.ID,oldBeacon.beaconPrefabInstance.transform.position,oldBeacon.detected,bitMaskIndex);
                    Destroy(oldBeacon.beaconPrefabInstance);
                    return;
                }
            }
            
            m_beacons[index ?? 0] = new Beacons(m_prefabBeaconActive,oldBeacon.ID,oldBeacon.beaconPrefabInstance.transform.position,oldBeacon.detected,bitMaskIndex);
            Destroy(oldBeacon.beaconPrefabInstance);
        }



        /// <summary/> Receiving initial data
        /// <param name="p_initialData"></param>
        private void ReceiveInitialData(InitialData p_initialData) {
            m_prefabBeaconActive.transform.localScale = Vector3.one * (p_initialData.beaconRange * 2f);
            Shader.SetGlobalFloat(m_beaconRangeShaderID,p_initialData.beaconRange);

            m_beaconBitMaskDetected = 0b00000000000;
            
            Shader.SetGlobalFloat(m_beaconBitMaskShaderID,0b00000000000);
            Shader.SetGlobalFloat(m_beaconDetectionBitMaskShaderID,0b00000000000);
        }

        /// <summary/> Updating the beacon positions
        /// <param name="p_beaconsPositions"></param>
        private void UpdatePositions(BeaconsPositions p_beaconsPositions)
        {
            for (int i = 0; i < m_beacons.Count; i++)
            {
                BeaconData data = p_beaconsPositions.data[i];
                
                int? index = FindBeaconFromID(i, data.beaconID, "Update Beacon Position");

                if (index == null) return;

                m_beacons[index ?? 0].beaconPrefabInstance.transform.position = data.position;
            }
        }
        
        /// <summary/> Updating the beacon's detections statuses
        /// <param name="p_beaconDetectionUpdate"></param>
        private void UpdateDetection(BeaconDetectionUpdate p_beaconDetectionUpdate)
        {
            int? index = FindBeaconFromID(p_beaconDetectionUpdate.index, p_beaconDetectionUpdate.beaconID, "Update Beacon Detection");

            if (index == null) return;

            bool detected = p_beaconDetectionUpdate.playerDetected;
            Beacons beacon = m_beacons[index ?? 0];
            
            //Material mat = beacon.beaconPrefabInstance.GetComponent<MeshRenderer>().material;
            //mat.SetColor(m_beaconColorProperty, detected?m_detectedColor:m_undetectedColor);
            beacon.detected = detected;
            
            if(detected)
            {
                // Debug.Log(Convert.ToString (m_beaconBitMaskDetected, 2));
                m_beaconBitMaskDetected |= 1 << beacon.bitMaskIndex;
                Shader.SetGlobalInteger(m_beaconDetectionBitMaskShaderID,m_beaconBitMaskDetected);
                //Debug.Log(Convert.ToString (beaconBitMaskDetected, 2));
                return;
            }
            
            m_beaconBitMaskDetected &= ~(1 << beacon.bitMaskIndex);
            Shader.SetGlobalInteger(m_beaconDetectionBitMaskShaderID,m_beaconBitMaskDetected);
            //Debug.Log(Convert.ToString (beaconBitMaskDetected, 2));

            foreach (var beaconData in m_beacons) if (beaconData.detected) return;
        }

        /// <summary/> Destroying the beacon based in the server info
        /// <param name="p_destroyedBeacon"></param>
        private void DestroyBeacon(DestroyedBeacon p_destroyedBeacon)
        {

            int? index = FindBeaconFromID(p_destroyedBeacon.index, p_destroyedBeacon.beaconID, "Destroy Beacon");

            if (index == null) return;

            
            int bitmask = Shader.GetGlobalInt(m_beaconBitMaskShaderID);
            bitmask &= ~(1 << m_beacons[index??0].bitMaskIndex);
            Shader.SetGlobalInt(m_beaconBitMaskShaderID,bitmask);
            
            m_beaconBitMaskDetected &= ~(1 << m_beacons[index??0].bitMaskIndex);
            Shader.SetGlobalInteger(m_beaconDetectionBitMaskShaderID,m_beaconBitMaskDetected);
            
            Destroy(m_beacons[index??0].beaconPrefabInstance);
            m_beacons.RemoveAt(index??0);

            
            foreach (var beacon in m_beacons)
            {
                if (beacon.detected) return;
            }
        }

        /// <summary/> A function to find the index of the beacon that matches the given ID
        /// <param name="p_index"> the estimated index of the wanted beacon </param>
        /// <param name="p_beaconID"> the ID of the wanted beacon </param>
        /// <returns> returns the index of the beacon with the right ID if none are found, returns null </returns>
        private int? FindBeaconFromID(int p_index, float p_beaconID, string context = "")
        {
            int index = p_index;
            float ID = p_beaconID;
            if ( index < m_beacons.Count && m_beacons[index].ID == ID) return index;

            for (int i = 0; i < m_beacons.Count; i++) if (m_beacons[i].ID == ID) return i;

            #if UNITY_EDITOR
            if (context != null)
                Debug.LogWarning(
                    $"I couldn't find the index matching this ID ({p_beaconID}) brother - { context }", this);
            #endif
            
            return null;
        }
    }
}
