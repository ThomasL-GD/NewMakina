using System;
using System.Collections.Generic;
using CustomMessages;
using UnityEngine;

namespace Synchronizers {
    public class SynchronizeBeacons : Synchronizer {

        [SerializeField][Tooltip("The beacon prefab that will be instantiated for each beacon")] private GameObject m_prefabBeaconActive = null;
        [SerializeField][Tooltip("The beacon prefab that will be instantiated for each beacon")] private GameObject m_prefabBeaconInactive = null;

        [SerializeField][Tooltip("The color of the beacon when the player is undetected")] private Color m_undetectedColor = Color.red;
        [SerializeField][Tooltip("The color of the beacon when the player is detected")] private Color m_detectedColor = Color.green;
        
        /// <summary/> The beacons of the player that are stored away
        private List<Beacons> m_beacons = new List<Beacons>();

        [Serializable]
        private struct Beacons
        {
            public GameObject beaconPrefabInstance;
            public float ID;

            public Beacons(GameObject p_beaconPrefabInstance, float p_id, Vector3? p_position = null)
            {
                beaconPrefabInstance = Instantiate(p_beaconPrefabInstance);
                beaconPrefabInstance.transform.position = p_position?? Vector3.zero;
                
                ID = p_id;
            }
        }
        private float m_beaconRange;

        
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
            ClientManager.OnReceiveInitialData += UpdateBeaconRange;
            ClientManager.OnReceiveActivateBeacon += UpdateBeaconActivation;
        }
        
        /// <summary/> Updating a beacon to activate
        /// <param name="p_activatebeacon"> the beacon data to activate </param>
        private void UpdateBeaconActivation(ActivateBeacon p_activatebeacon)
        {
            int? index = FindBeaconFromID(p_activatebeacon.index, p_activatebeacon.beaconID);

            if (index == null)
            {
                Debug.LogError("BEACON DETECTION UPDATE ID SEARCH FAILED");
                return;
            }

            GameObject oldBeacon = m_beacons[index ?? 0].beaconPrefabInstance;
            m_beacons[index ?? 0] = new Beacons(m_prefabBeaconActive,m_beacons[index ?? 0].ID,oldBeacon.transform.position);
            Destroy(oldBeacon);
        }



        /// <summary/> Updating the beacon range
        /// <param name="p_initialdata"></param>
        private void UpdateBeaconRange(InitialData p_initialdata) {
            m_prefabBeaconActive.transform.localScale = Vector3.one * (p_initialdata.beaconRange * 2f);
        }

        /// <summary/> Updating the beacon positions
        /// <param name="p_beaconsPositions"></param>
        private void UpdatePositions(BeaconsPositions p_beaconsPositions)
        {
            for (int i = 0; i < m_beacons.Count; i++)
            {
                BeaconData data = p_beaconsPositions.data[i];
                
                int? index = FindBeaconFromID(i, data.beaconID);

                if (index == null)
                {
                    Debug.LogError("BEACON DETECTION UPDATE ID SEARCH FAILED");
                    return;
                }

                m_beacons[index ?? 0].beaconPrefabInstance.transform.position = data.position;
            }
        }

        /// <summary/> Updating the beacon's detections statuses
        /// <param name="p_beaconDetectionUpdate"></param>
        private void UpdateDetection(BeaconDetectionUpdate p_beaconDetectionUpdate)
        {
            int? index = FindBeaconFromID(p_beaconDetectionUpdate.index, p_beaconDetectionUpdate.beaconID);

            if (index == null)
            {
                Debug.LogError("BEACON DETECTION UPDATE ID SEARCH FAILED");
                return;
            }
            

            Material mat = m_beacons[index ?? 0].beaconPrefabInstance.GetComponent<MeshRenderer>().material;
            mat.SetColor("_Beacon_Color", p_beaconDetectionUpdate.playerDetected?m_detectedColor:m_undetectedColor);
        }

        /// <summary/> Destroying the beacon based in the server info
        /// <param name="p_destroyedBeacon"></param>
        private void DestroyBeacon(DestroyedBeacon p_destroyedBeacon)
        {

            int? index = FindBeaconFromID(p_destroyedBeacon.index, p_destroyedBeacon.beaconID);
            float id = p_destroyedBeacon.beaconID;

            if (index == null)
            {
                Debug.LogError("DESTROY BEACON ID SEARCH FAILED");
                return;
            }
            
            Destroy(m_beacons[index??0].beaconPrefabInstance);
            m_beacons.RemoveAt(index??0);
        }

        /// <summary/> A function to find the index of the beacon that matches the given ID
        /// <param name="p_index"> the estimated index of the wanted beacon </param>
        /// <param name="p_beaconID"> the ID of the wanted beacon </param>
        /// <returns> returns the index of the beacon with the right ID if none are found, returns null </returns>
        private int? FindBeaconFromID(int p_index, float p_beaconID)
        {
            int index = p_index;
            float ID = p_beaconID;
            if ( index < m_beacons.Count || m_beacons[index].ID == ID) return index;

            for (int i = 0; i < m_beacons.Count; i++) if (m_beacons[i].ID == ID) return i;

            Debug.LogWarning("I couldn't find the index matching this ID brother",this);
            return null;
        }
    }
}
