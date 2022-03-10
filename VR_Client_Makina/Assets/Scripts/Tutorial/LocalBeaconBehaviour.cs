
using UnityEngine;

namespace Tutorial {
    
    public class LocalBeaconBehaviour : GrabbablePhysickedObject {
    
    
    
        // Start is called before the first frame update
        protected override void Start() {
            base.Start();
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        protected override void OnFirstTimeTouchingGround(Collision p_other) {
            base.OnFirstTimeTouchingGround(p_other);
        }
    }

}
