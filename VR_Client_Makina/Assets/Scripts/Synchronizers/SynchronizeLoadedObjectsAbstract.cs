using System;
using System.Collections;
using System.Collections.Generic;
using CustomMessages;
using Network;
using UnityEngine;

namespace Synchronizers {
    
    public abstract class SynchronizeLoadedObjectsAbstract : Synchronizer<SynchronizeLoadedObjectsAbstract> {

        [Header("Loading in Arm")]
        [SerializeField] public Transform[] m_loadingPositions;

        [SerializeField, Tooltip("If true, the object will become child of its spawnpoint's transform until it is picked up\nIf false, will spawn normally and wait to be picked up without moving even tough it spawn point moved")] public bool m_stickToSpawnPosition = false;

        protected bool[] m_availblePositions = null;
        
        [HideInInspector] public int m_maxSlotsLoading = 1;
        
        [SerializeField] [Tooltip("If true, the only physics applied to be applied on this object on drop will be a straight down force.\nIf false, it can be thrown around")] protected bool m_dropDown = false;

        protected virtual void Start() {
            Initialize();
            MyNetworkManager.OnReceiveInitialData += Initialize;
            MyNetworkManager.OnReceiveInitiateLobby += Initialize;
            MyNetworkManager.OnReadyToGoIntoTheBowl += Initialize;
        }

        private void Initialize(InitialData p_p) => Initialize();
        private void Initialize(InitiateLobby p_p) => Initialize();
        private void Initialize(ReadyToGoIntoTheBowl p_p) => Initialize();

        protected void Initialize() {
            m_availblePositions = new bool[m_loadingPositions.Length];
            for (int i = 0; i < m_availblePositions.Length; i++) m_availblePositions[i] = true; //We set the entire array to true 
            if(m_loadingPositions.Length < m_maxSlotsLoading) Debug.LogError("There is more possible loaded beacons than loaded beacon position, so please do your game designer's job, we ain't paying you a SMIC for nothing", this);
        }

        /// <summary> Will call the Initialization method of a given script and lock the position taken. </summary>
        /// <param name="p_script">The ObjectLoading script that is on the object you want to load</param>
        /// <param name="p_index">The index in the possible loading array you want to set the object in</param>
        protected void LoadObjectFromIndex(ObjectLoading p_script, int p_index) {
#if UNITY_EDITOR
            if(p_index < 0 || p_index > m_availblePositions.Length - 1) Debug.LogError($"Why would you have an index ({p_index}) out of bounds of the array ≖_≖", this);
            if(m_availblePositions[p_index] == false) Debug.LogError($"You messed it up, the position n°{p_index} is unavailable", this);
#endif
            m_availblePositions[p_index] = false;
            p_script.Initialization(p_index);
        }

        public void RestoreAvailability(int p_index) {
            m_availblePositions[p_index] = true;
        }
        
        //TODO : improve this coz there is a lot of copy/paste in bombs & beacons
        
    }

}
