using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Synchronizers {
    
    public abstract class SynchronizeLoadedObjectsAbstract : Synchronizer {

        [Header("Loading in Arm")]
        [SerializeField] public Transform[] m_loadingPositions;
        
        public int m_maxSlotsLoading = 1;
        
        //protected static 
        
    }

}
