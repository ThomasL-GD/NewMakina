using UnityEngine;

namespace Grabbabble_Type_Beat {

    public class CageBehavior : GrabbablePhysickedObject {
        
        

        protected override void OnFirstTimeTouchingGround(Collision p_other) {
            base.OnFirstTimeTouchingGround(p_other);
        
            m_isGrabbable = false;
            m_rb.isKinematic = true;
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }

}
