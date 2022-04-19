using System;
using System.Collections;
using System.Collections.Generic;
using CustomMessages;
using Network;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Synchronizers {
    public class SynchronizeBeacons : SynchronizeLoadedObjectsAbstract {

        [Header("Beacons")]
        [SerializeField] [Tooltip("Must have the BeaconBehavior script attached")] private GameObject m_prefabBeaconAmmo = null;
        [SerializeField] [Tooltip("Must have the InflateToSize script attached")] private GameObject m_prefabDeployedBeacon = null;
        [HideInInspector] public float m_range = 20f;
        [SerializeField] [Range(0f, 1f)] public float m_inflateTime = 0.2f;
        

        [Header("Colors")]
        [SerializeField] private Color m_undetectedColor = Color.red;
        [SerializeField] private Color m_detectedColor = Color.green;
        //[SerializeField] private Color m_unactiveColor = Color.blue;
        
        [Header("Sounds")]
        [SerializeField, Tooltip("Must contain an AudioSource component")] private AudioClip m_beaconDetectSoundPrefab;

        
        private int m_beaconBitMaskDetected = 0;
        
        private static readonly int m_beaconColorProperty = Shader.PropertyToID("_Beacon_Color");
        private static readonly int m_beaconRangeShaderID = Shader.PropertyToID("_BeaconRange");
        private static readonly int m_beaconBitMaskShaderID = Shader.PropertyToID("_BeaconBitMask");
        private static readonly int m_beaconDetectionBitMaskShaderID = Shader.PropertyToID("_BeaconDetectionBitMask");
        
        /// <summary> Contains a gameobject of a beacon and its server ID, additionally contains booleans that explain the beacon state </summary>
        private class BeaconInfo {
            /// <summary>The gameobject of the beacon to grab</summary>
            public readonly BeaconBehavior beaconScript;
            
            /// <summary>The gameobject of the deployed beacon</summary>
            public readonly InflateToSize deployedBeaconScript;
            
            /// <summary> If true, the PC player is in the range of this beacon </summary>
            public bool isDetecting;
            
            /// <summary> If true, the beacon has touched the ground after being thrown, if false, the beacon is either loaded or in a hand of the player or in the air </summary>
            public bool isOnTheGroundAndSetUp;

            /// <summary> The ID the server uses for verifications, basically don't touch this </summary>
            public float serverID;
            public int bitMaskIndex;

            /// <summary> The constructor of a beaconInfo class </summary>
            /// <param name="p_grabScript">The GameObject of the beacon to grab</param>
            /// <param name="p_serverID">The server id of the beacon</param>
            /// <param name="p_deployedBeaconScript">The gameobject of the deployed beacon</param>
            public BeaconInfo(BeaconBehavior p_grabScript, InflateToSize p_deployedBeaconScript, float p_serverID, int p_BitMaskIndex) {
                isDetecting = false;
                isOnTheGroundAndSetUp = false;
                beaconScript = p_grabScript;
                serverID = p_serverID;
                deployedBeaconScript = p_deployedBeaconScript;
                bitMaskIndex = p_BitMaskIndex;
            }
        }
        
        private List<BeaconInfo> m_beacons = new List<BeaconInfo>();

        public delegate void BeaconDelegator(BeaconBehavior p_beaconBehavior);

        public static BeaconDelegator OnNewBeacon;
        public static BeaconDelegator OnBeaconSetUp;
        
        
        private static readonly int CodeBeaconColor = Shader.PropertyToID("_Beacon_Color");
        
        [Header("DropDown")]
        [SerializeField] [Tooltip("If true, nice shot :)\nIf false, crippling emptiness...")] private GameObject m_prefabDropDownFeedback;

        private void Awake() {
            if(m_prefabBeaconAmmo == null) Debug.LogError("You forgot to serialize a beacon prefab here ! (╬ ಠ益ಠ)", this);
        }

        protected override void Start() {
            base.Start();
            
            MyNetworkManager.OnReceiveInitialData += ReceiveInitialData;
            MyNetworkManager.OnReceiveSpawnBeacon += CreateBeacon;
            MyNetworkManager.OnReceiveBeaconDetectionUpdate += UpdateDetection;
            MyNetworkManager.OnReceiveDestroyedBeacon += DestroyBeacon;
            MyNetworkManager.OnReceiveInitialData += SetRangeOfBeacons;
            MyNetworkManager.OnReceiveGameEnd += Reset;
        }

        /// <summary>Is called by the OnReceiveGameEnd and destroys every beacon to be ready to launch a new game </summary>
        /// <param name="p_p_gameend">The message sent by the server</param>
        private void Reset(GameEnd p_p_gameend) {
            
            //Destroying every beacon ψ(` ͜ʖ´)ψ
            for(int i = 0; i < m_beacons.Count; i++) {
                
                //If the beacon is dep^loyed, we can't destroy it because it is already destroyed
                if(!m_beacons[i].isOnTheGroundAndSetUp) m_beacons[i].beaconScript.DestroyMaSoul();
                
                Destroy(m_beacons[FindBeaconFromID(i, m_beacons[i].serverID)??0].deployedBeaconScript.gameObject);
            }

            m_beaconBitMaskDetected = 0;
            
            Shader.SetGlobalInt(m_beaconBitMaskShaderID,0b00000000000);
            Shader.SetGlobalInt(m_beaconDetectionBitMaskShaderID,0b00000000000);
            m_beacons.Clear();
        }

        /// <summary>Just sets maxSlotsForBeacons according to the server's InitialData</summary>
        /// <param name="p_initialData">The message sent by the server</param>
        protected override void ReceiveInitialData(InitialData p_initialData) {
            base.ReceiveInitialData(p_initialData);
            m_maxSlotsLoading = p_initialData.maximumBeaconCount;
            
            m_beaconBitMaskDetected = 0;
            
            Shader.SetGlobalInt(m_beaconBitMaskShaderID,0b00000000000);
            Shader.SetGlobalInt(m_beaconDetectionBitMaskShaderID,0b00000000000);
        }

        private void Update() {
            if (m_beacons.Count < 1) return;
            BeaconData[] positionses = new BeaconData[m_beacons.Count];

            for (int i = 0; i < positionses.Length; i++) {
                switch (m_beacons[i].isOnTheGroundAndSetUp) { //If the beacon is not deployed yet, we take the position of the grabbable item, else, we take the deployed beacon's position coz the grabbableBeacon has been destroyed
                    case true :
                        positionses[i].position = m_beacons[i].deployedBeaconScript.gameObject.transform.position;
                    break;
                    
                    case false :
                        positionses[i].position = m_beacons[i].beaconScript.gameObject.transform.position;
                    break;
                }
                
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
            BeaconBehavior beaconScript = go.GetComponent<BeaconBehavior>();
            
            GameObject daChild = Instantiate(m_prefabDeployedBeacon, go.transform);
            InflateToSize deployScript = daChild.GetComponent<InflateToSize>();
            
            m_beacons.Add(new BeaconInfo(beaconScript, deployScript, p_spawnBeacon.beaconID,0));

            m_beacons[m_beacons.Count - 1].bitMaskIndex = m_beacons.Count - 1;
            
            beaconScript.m_index = m_beacons.Count - 1;
            beaconScript.m_serverID = p_spawnBeacon.beaconID;
            beaconScript.m_synchronizer = this;
            beaconScript.m_mustDropDown = m_dropDown;
            beaconScript.m_prefabDropDownFeedback = m_prefabDropDownFeedback;

            OnNewBeacon?.Invoke(beaconScript);
        }

        /// <summary> Will load a beacon in a random available position </summary>
        /// <param name="p_script">The ObjectLoading script of the beacon you want to load</param>
        public void LoadBeaconRandomly(ObjectLoading p_script) {
            List<int> currentAvailablePositions = new List<int>();
            for(int i = 0; i < m_availblePositions.Length; i++) {
                if (m_availblePositions[i]) {
                    currentAvailablePositions.Add(i);
                }
            }

            int random = Random.Range(0, currentAvailablePositions.Count);
            LoadObjectFromIndex(p_script, currentAvailablePositions[random]);
        }
        
        
        /// <summary> Is called when we receive a DestroyedBeacon message from server </summary>
        /// <param name="p_destroyedBeacon">The message from the server</param>
        private void DestroyBeacon(DestroyedBeacon p_destroyedBeacon) {
            
            int? index = FindBeaconFromID(p_destroyedBeacon.index, p_destroyedBeacon.beaconID);
            if (index == null) {
                Debug.LogError($"What the fuck is that, horrible {p_destroyedBeacon.index} c'est und des selling point la du jeu {p_destroyedBeacon.beaconID}");
                return;
            }
            
            
            int bitmask = Shader.GetGlobalInt(m_beaconBitMaskShaderID);
            bitmask &= ~(1 << m_beacons[index??0].bitMaskIndex);
            Shader.SetGlobalInt(m_beaconBitMaskShaderID,bitmask);
            
            m_beaconBitMaskDetected &= ~(1 << m_beacons[index??0].bitMaskIndex);
            Shader.SetGlobalInt(m_beaconDetectionBitMaskShaderID,m_beaconBitMaskDetected);
            
            
            GameObject go = m_beacons[index ?? 0].deployedBeaconScript.gameObject;
            m_beacons.RemoveAt(index??0);
            Destroy(go);
        }
        
        /// <summary>
        /// Is called when we receive a BeaconDetectionUpdate message from server
        /// Will update the state and the color of a beacon depending on if they are in range or not according to the server
        /// </summary>
        /// <param name="p_beaconDetectionUpdate">The message from the server</param>
        private void UpdateDetection(BeaconDetectionUpdate p_beaconDetectionUpdate) {
            
            int? index = FindBeaconFromID(p_beaconDetectionUpdate.index, p_beaconDetectionUpdate.beaconID);
            if (index == null) {
                Debug.LogError($"Index null for a beacon {p_beaconDetectionUpdate.index} : {p_beaconDetectionUpdate.beaconID}");
                return;
            }

            if (!m_beacons[index ?? 0].isDetecting && p_beaconDetectionUpdate.playerDetected) {
                if (m_beacons[index ?? 0].beaconScript.gameObject.TryGetComponent(out AudioSource audioSource)) {
                    audioSource.clip = m_beaconDetectSoundPrefab;
                    audioSource.Play();
                }else {
                    AudioSource createdAudioSource = m_beacons[index ?? 0].beaconScript.gameObject.AddComponent<AudioSource>();
                    createdAudioSource.Play();
                }
            }
            m_beacons[index??0].isDetecting = p_beaconDetectionUpdate.playerDetected;
            //Debug.Log($"Is player detected ? actually the {p_beaconDetectionUpdate.index} is {(p_beaconDetectionUpdate.playerDetected? "REALLY" : "NOT" )} detecting", this);
            
            ActualiseColorOfBeacon(index??0);
        }

        /// <summary> Will actualise the color of a beacon according to what it should display according to the beacon state </summary>
        /// <param name="p_index">The index of the beacon</param>
        private void ActualiseColorOfBeacon(int p_index) {

            Color newColor;
        
            switch (m_beacons[p_index].isDetecting) {
                case true:
                    newColor = m_detectedColor;
                    
                    m_beaconBitMaskDetected |= 1 << m_beacons[p_index].bitMaskIndex;
                    Shader.SetGlobalInt(m_beaconDetectionBitMaskShaderID,m_beaconBitMaskDetected);
                    break;
                case false:
                    newColor = m_undetectedColor;
                    m_beaconBitMaskDetected &= ~(1 << m_beacons[p_index].bitMaskIndex);
                    Shader.SetGlobalInt(m_beaconDetectionBitMaskShaderID,m_beaconBitMaskDetected);
                    break;
            }
            
            GameObject go = m_beacons[p_index].deployedBeaconScript.gameObject;
            Material mat = go.GetComponent<MeshRenderer>().material;
            mat.SetColor(CodeBeaconColor, newColor);
        }

        /// <summary> Changes the range of the beacon feedback in the prefab itself </summary>
        /// <param name="p_initialData">The message from the server</param>
        private void SetRangeOfBeacons(InitialData p_initialData) {
            m_range = p_initialData.beaconRange;
            
            Shader.SetGlobalFloat(m_beaconRangeShaderID, m_range);

            InflateToSize script = m_prefabDeployedBeacon.GetComponent<InflateToSize>();
            script.m_targetScale = (m_range * 2f) /* / m_prefabBeaconAmmo.transform.localScale.x*/;
            script.m_inflationTime = m_inflateTime;
            
            m_beaconBitMaskDetected = 0;
        }

        /// <summary> Send to the network manager the ActivateBeacon message </summary>
        /// <param name="p_index">The index of the beacon that gets activated</param>
        /// <param name="p_serverID">The server ID of the beacon that gets activated</param>
        public void SendBeaconActivation(int p_index, float p_serverID) {
            
            int? index = FindBeaconFromID(p_index, p_serverID);
            if (index == null) {
#if UNITY_EDITOR
                Debug.LogError($"What the fuck is that, horrible {p_index} c'est und des selling point la du jeu {p_serverID}");
#endif
                return;
            }
            
            m_beacons[index??0].deployedBeaconScript.gameObject.transform.SetParent(null, true);
            m_beacons[index??0].deployedBeaconScript.StartInflating();
            m_beacons[index??0].beaconScript.DestroyMaSoul();
            
            int beaconBitMask = Shader.GetGlobalInt(m_beaconBitMaskShaderID);
            int bitMaskIndex = -2;
            
            for (int i = 0; i < 10; i++)
            {
                if ((beaconBitMask & 1 << i) != 1 << i)
                {
                    m_beacons[index??0].bitMaskIndex = i;
                    beaconBitMask |= 1 << m_beacons[index??0].bitMaskIndex;
                    Shader.SetGlobalInt(m_beaconBitMaskShaderID, beaconBitMask);

                    Vector3 pos = m_beacons[index??0].deployedBeaconScript.gameObject.transform.position;
                    
                    Shader.SetGlobalVector($"_beaconPosition_{i}", new Vector4(pos.x,pos.y,pos.z,0));
                    Shader.SetGlobalFloat($"_beaconTimer_{i}", Time.time);
                    break;
                }
            }
            
            SetActivationOfABeacon(index??0, true);
            OnBeaconSetUp?.Invoke(m_beacons[index??0].beaconScript);
            MyNetworkManager.singleton.SendVrData(new ActivateBeacon(){index = index??0, beaconID = m_beacons[index??0].serverID});
        }

        /// <summary> Change the value of the isOnTheGroundAndSetUp of a beacon according to its index </summary>
        /// <param name="p_index">The index of the beacon</param>
        /// <param name="p_isOnTheGroundAndSetUp">If the beacon is loaded or not</param>
        private void SetActivationOfABeacon(int p_index, bool p_isOnTheGroundAndSetUp) {
            m_beacons[p_index].isOnTheGroundAndSetUp = p_isOnTheGroundAndSetUp;
        }
        
        /// <summary/> A function to find the index of the beacon that matches the given ID
        /// <param name="p_index"> the estimated index of the wanted beacon </param>
        /// <param name="p_beaconID"> the ID of the wanted beacon </param>
        /// <returns> returns the index of the beacon with the right ID if none are found, returns null </returns>
        private int? FindBeaconFromID(int p_index, float p_beaconID)
        {
            int index = p_index;
            float ID = p_beaconID;
            List<BeaconInfo> data = m_beacons;
            if ( index < data.Count && data[index].serverID == ID) return index;

            for (int i = 0; i < data.Count; i++) if (data[i].serverID == ID) return i;

#if UNITY_EDITOR
            Debug.LogWarning("I couldn't find the index matching this ID brother",this);
#endif
            return null;
        }
    }
}