
using UnityEngine;

namespace Tutorial {
    
    public class LocalBeaconBehaviour : GrabbablePhysickedObject {
    
        // Start is called before the first frame update
        protected override void Start() {
            base.Start();
        }

        protected override void OnFirstTimeTouchingGround(Collision p_other) {
            base.OnFirstTimeTouchingGround(p_other);
            //TODO : INstantiate prefab stored in the manager
        }
    }

}
