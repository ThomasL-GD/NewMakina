using UnityEngine;

namespace Player_Scripts.Reloading {

    public abstract class ReloadingAbstract : MonoBehaviour {

        public delegate void ReloadingDelegator();

        /// <summary>Is invoked when the reloading conditions are true </summary>
        /// <remarks>Subscribe to this te re-enable your ability</remarks>
        public ReloadingDelegator OnReloading;

        /// <summary>Call this function to start the reloading process
        /// When it's done, it will invoke OnReloading</summary>
        /// <remarks>You probably want to call that once your ability is used</remarks>
        public virtual void StartReloading() { }
    
    }

}
