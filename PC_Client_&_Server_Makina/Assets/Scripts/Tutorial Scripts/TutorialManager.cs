using System;
using UnityEngine;

namespace Tutorial_Scripts {

    public class TutorialManager : MonoBehaviour {

        /// <summary>Contains all information needed on one step of the tutorial</summary>
        [Serializable] struct TutorialStep {
            //trigger
            public Transform[] gameobjectsOfTheStep;
        }

        [SerializeField] [Tooltip("Each new step, the player will start here")] private Transform m_spawnpoint;
        
        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
