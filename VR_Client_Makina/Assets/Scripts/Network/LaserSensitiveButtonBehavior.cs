using UnityEngine;

namespace Network {

    [RequireComponent(typeof(Collider))]
    public abstract class LaserSensitiveButtonBehavior : MonoBehaviour {
        
        
        void OnEnable() {
            if (gameObject.layer != 7) {
                Debug.LogWarning("This object is not at the 7th layer, even tough it's supposed to be, I put it back automatically then", this);
                gameObject.layer = 7;
            }
        }

        /// <summary>
        /// Is called when the script LocalLaser will shot this object
        /// </summary>
        /// <remarks>
        /// Override this function if you want something to happen when this object is being shot
        /// </remarks>
        public virtual void OnBeingShot() { }
    }
}
