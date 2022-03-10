using System;
using Network;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tutorial {

    public class TutorialManager : MonoBehaviour {

        [SerializeField] private Scene m_sceneToLoadWhenDone;

        /// <summary>The height at will theoretical invisible object will spawn</summary>
        [Header("Emerging values"), Range(-1000f, 100f)] public float heightThatIsConsideredUnderground;
        [SerializeField] [Range(0f, 2f)] public float emergingTime = 0.5f;
        [SerializeField] public AnimationCurve speedToGoUp = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        
        [Header("Beacons")]
        [SerializeField] [Range(0, 20)] private int m_firststepWithBeacons = 4;
        [SerializeField] [Range(5f, 100f)] public float beaconRange = 50f;
        [SerializeField] [Range(0.1f, 20f)] public float beaconLifetime = 5f;
        [SerializeField] private GameObject m_prefabBeacon;
        [SerializeField] public GameObject prefabDeployedBeacon;
        [SerializeField] private Transform[] m_beaconsSpawnPos;
        
        
        [Serializable] public class Step {
            public Emerge[] objectsOfThisStep;
        }
        
        [Header("Iterations")]
        [SerializeField, Tooltip("Each cell represent a step contains an array.\nIn each array, put every object you want to make appear and give them the Emerge script.\nThe number of cells will determine the number of steps")] private Step[] m_gameObjectsToActivateForEachStep;

        private int m_currentStep = 0;


        #region singleton
        //Singleton time ! 	(˵ ͡° ͜ʖ ͡°˵)
        public new static TutorialManager singleton { get; private set; }

        public float heightThatIsConsideredUnderground1 {
            get => heightThatIsConsideredUnderground;
            set => heightThatIsConsideredUnderground = value;
        }

        /// <summary>
        /// Is that... a singleton setup ?
        /// *Pokédex's voice* A singleton, a pretty common pokécode you can find in a lot of projects, it allows anyone to
        /// call it and ensure there is only one script of this type in the entire scene !
        /// </summary>
        private void Awake() {
            if (singleton != null && singleton != this) {
#if UNITY_EDITOR
                Debug.LogWarning("Too many tutorial singletons for some reason", this);
#endif
                gameObject.SetActive(false);
                return;
            }
            singleton = this;
        }
        #endregion


        private void Start() {
            StartTutorial();
        }


        public void StartTutorial() {
#if UNITY_EDITOR
            if(m_currentStep != 0) Debug.LogWarning("Tutorial has already started but you are trying to set it up again ?!");
#endif
            //MyNetworkManager.singleton.SendVrData(new CustomMessages.Tutorial(){isInTutorial = true});
            m_currentStep = -1;
            NextStep();
        }

        public void SpawnBeacon(int p_index) {
            GameObject go = Instantiate(m_prefabBeacon, m_beaconsSpawnPos[p_index].position, Quaternion.Euler(Vector3.zero));
            go.transform.SetParent(m_beaconsSpawnPos[p_index]);
            go.GetComponent<LocalBeaconBehaviour>().m_index = p_index;
        }

        public void NextStep() {
            
            Debug.LogWarning("Next step !");

            if (m_currentStep >= 0) {
                foreach (Emerge script in m_gameObjectsToActivateForEachStep[m_currentStep].objectsOfThisStep) {
                    script.StartDemerging();
                }
            }
            
            m_currentStep++;
            if (m_currentStep >= m_gameObjectsToActivateForEachStep.Length) {
                EndTutorial();
                return;
            }

            if (m_currentStep == m_firststepWithBeacons) {
                for (var index = 0; index < m_beaconsSpawnPos.Length; index++) {
                    SpawnBeacon(index);
                }
            }

            foreach (Emerge script in m_gameObjectsToActivateForEachStep[m_currentStep].objectsOfThisStep) {
                script.gameObject.SetActive(true);
                script.StartEmerging();
            }
            
        }

        private void EndTutorial() {
            singleton = null;
            SceneManager.LoadScene(m_sceneToLoadWhenDone.buildIndex);
            Destroy(this);
        }
        
        
    }

}