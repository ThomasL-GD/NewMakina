using UnityEngine;

namespace Synchronizers {
    public abstract class Synchronizer : MonoBehaviour {

        //TODO Make the sttic apply dependin of the children type
        
        /*private static int m_howManyMe = 0;

        protected void Awake() {
            Debug.Log("I wanna check something out", this);
            m_howManyMe++;

            if (m_howManyMe != 1) {
                Debug.LogWarning($"There are duplicates ", this);
                this.enabled = false;
            }
        }*/
    }
}
