using UnityEngine;

namespace Tutorial {
    
    public class LocalBeaconBehaviour : GrabbablePhysickedObject {

        [HideInInspector] public int m_index;

        // Start is called before the first frame update
        protected override void Start() {
            base.Start();

            m_rb.isKinematic = true;
        }

        protected override void OnFirstTimeTouchingGround(Collision p_other) {
            base.OnFirstTimeTouchingGround(p_other);
            var transform1 = transform;
            GameObject go = Instantiate(TutorialManager.singleton.prefabDeployedBeacon, transform1.position, transform1.rotation);
            go.GetComponent<InflateToSize>().m_targetScale = TutorialManager.singleton.beaconRange;
            go.GetComponent<LocalBeaconFeedback>().m_index = m_index;
            Destroy(gameObject);
        }
        
        public override void BeGrabbed(Transform p_parent) {

            // if (!m_hasBeenCaughtInLifetime) {
            //     m_rb.isKinematic = false;
            // }
        
            base.BeGrabbed(p_parent);
        }
    }

}
