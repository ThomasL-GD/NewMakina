using System;
using System.Collections.Generic;
using CustomMessages;
using UnityEngine;

namespace Synchronizers {
    public class SynchronizeBeacons : Synchronizer {

        [SerializeField][Tooltip("The beacon prefab that will be instantiated for each beacon")] private GameObject m_prefabBeacon = null;

        [SerializeField][Tooltip("The color of the beacon when the player is undetected")] private Color m_undetectedColor = Color.red;
        [SerializeField][Tooltip("The color of the beacon when the player is detected")] private Color m_detectedColor = Color.green;
        
        /// <summary/> The beacons of the player that are stored away
        private List<Beacons> m_beacons = new List<Beacons>();

        [Serializable]
        private struct Beacons
        {
            public GameObject gameObject;
            public float ID;
        }
        private float m_beaconRange;

        /// <summary/> Initiating the class by adding the right functions to the Client delegates
        private void Awake() {
            // Adding the right functions to the delegate
            ClientManager.OnReceiveSpawnBeacon += SpawnBeacons; 
            ClientManager.OnReceiveBeaconsPositions += UpdatePositions;
            ClientManager.OnReceiveDestroyedBeacon += DestroyBeacon;
            ClientManager.OnReceiveBeaconDetectionUpdate += UpdateDetection;
            ClientManager.OnReceiveInitialData += UpdateBeaconRange;
        }

        private void SpawnBeacons(SpawnBeacon p_spawnbeacon)
        {
            GameObject bc  = Instantiate(m_prefabBeacon);
            bc.transform.localScale *= m_beaconRange;
            m_beacons.Add(new Beacons(){gameObject = bc,ID = p_spawnbeacon.beaconID});
        }

        /// <summary/> Updating the beacon range
        /// <param name="p_initialdata"></param>
        private void UpdateBeaconRange(InitialData p_initialdata) => m_beaconRange = p_initialdata.beaconRange;

        /// <summary/> Updating the beacon positions
        /// <param name="p_beaconsPositions"></param>
        private void UpdatePositions(BeaconsPositions p_beaconsPositions) {

            Debug.Log("hey");
            
            for (int i = 0; i < m_beacons.Count; i++)
            {
                if(m_beacons[i].ID == p_beaconsPositions.data[i].beaconID)
                    m_beacons[i].gameObject.transform.position = p_beaconsPositions.data[i].position;

                for (int j = 0; j < m_beacons.Count; j++)
                {
                    if(m_beacons[j].ID == p_beaconsPositions.data[i].beaconID)
                        m_beacons[j].gameObject.transform.position = p_beaconsPositions.data[i].position;
                }
            }
        }

        /// <summary/> Updating the beacon's detections statuses
        /// <param name="p_beaconDetectionUpdate"></param>
        private void UpdateDetection(BeaconDetectionUpdate p_beaconDetectionUpdate)
        {
            return;
            int index = p_beaconDetectionUpdate.index;
            float ID = p_beaconDetectionUpdate.beaconID;
            if ( index < m_beacons.Count|| m_beacons[index].ID == ID)
            {
                Material mat = m_beacons[p_beaconDetectionUpdate.index].gameObject.GetComponent<MeshRenderer>().material;
                mat.SetColor("_Beacon_Color", p_beaconDetectionUpdate.playerDetected?m_detectedColor:m_undetectedColor);
                return;
            }

            for (int i = 0; i < m_beacons.Count; i++)
            {
                if (m_beacons[i].ID == ID)
                {
                    Material mat = m_beacons[p_beaconDetectionUpdate.index].gameObject.GetComponent<MeshRenderer>().material;
                    mat.SetColor("_Beacon_Color", p_beaconDetectionUpdate.playerDetected?m_detectedColor:m_undetectedColor);
                    return;
                }
            }
            
            // How did you get here
            Debug.LogWarning("I couldn't find the ID brother",this);
        }

        /// <summary/> Destroying the beacon based in the server info
        /// <param name="p_destroyedBeacon"></param>
        private void DestroyBeacon(DestroyedBeacon p_destroyedBeacon)
        {
            int index = p_destroyedBeacon.index;
            Destroy(m_beacons[index].gameObject);
            m_beacons.RemoveAt(index);
        }
    }
}
