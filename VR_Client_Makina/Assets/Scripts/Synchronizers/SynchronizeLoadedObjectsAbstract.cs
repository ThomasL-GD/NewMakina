using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Synchronizers {
    
    public abstract class SynchronizeLoadedObjectsAbstract : Synchronizer {

        [Header("Loading in Arm")]
        [SerializeField] public Transform[] m_loadingPositions;

        private bool[] m_availblePositions = null;
        
        public int m_maxSlotsLoading = 1;

        protected virtual void Start() {
            m_availblePositions = new bool[m_loadingPositions.Length];
            for (int i = 0; i < m_availblePositions.Length; i++) m_availblePositions[i] = true;
            if(m_loadingPositions.Length < m_maxSlotsLoading) Debug.LogWarning("There is more possible loaded beacons than loaded beacon position, so please do your game designer's job, we ain't paying you a SMIC for nothing", this);
        }
        
    }

}
