using System.Collections;
using Animation.AnimationDelegates;
using JetBrains.Annotations;
using UnityEngine;

namespace Grabbabble_Type_Beat {

    [RequireComponent(typeof(Collider), typeof(Rigidbody))]
    public class VrHandBehaviour : MonoBehaviour {
    
        [SerializeField] private OVRInput.Axis1D m_grabInput = OVRInput.Axis1D.PrimaryHandTrigger;
    
        [SerializeField] [Range(0.01f,1f)]/**/ private float m_triggerGrabSensitivity = 0.2f;
        //[SerializeField] [Range(0.01f,1f)]/**/ private float m_triggerLetGoSensitivity = 0.9f;
    
        [SerializeField] private Vector3 m_grabbedItemsPositionInHand;
    
        [Header("Pull")]
        [SerializeField] [Range(10f,100000f)] private float m_pullMaxDistance;
        [SerializeField] [Range(0.1f,100f)] private float m_pullSpeed;
        [SerializeField] private LayerMask m_layersThatPulls;
        private bool m_isThereAnObjectPulled = false;
        private Coroutine m_pullCoroutine; 
        private Vector3 m_pulledObjectOriginalPos; 

        public AnimationBoolChange OnGrabItemChange;
        [CanBeNull] public GrabbableObject m_objectHeld {
            get => _objectHeld;
            private set {
                if (value == _objectHeld) return;

                OnGrabItemChange?.Invoke(IsGrabbing, value != null);
                _objectHeld = value;
            }
        }
        // ReSharper disable once InconsistentNaming
        [CanBeNull] private GrabbableObject _objectHeld = null;
        public bool isFree => m_objectHeld == null && m_isPressingTrigger;

        public AnimationBoolChange OnClosedHandChange;
        public bool m_isPressingTrigger {
            get => _isPressingTrigger;
            private set {
                if (value == _isPressingTrigger) return;

                OnClosedHandChange?.Invoke(IsClosed, value);
                _isPressingTrigger = value;
            }
        }
        // ReSharper disable once InconsistentNaming
        private bool _isPressingTrigger = false;

        private static readonly int IsClosed = Animator.StringToHash("IsClosed");
        private static readonly int IsGrabbing = Animator.StringToHash("IsGrabbing");
        public static int s_layer = -1;

        private void Start() {
        
#if UNITY_EDITOR
            //if (m_triggerGrabSensitivity < m_triggerLetGoSensitivity) Debug.LogWarning("What the fuck is that balancing ?!", this);
            if (s_layer != -1 && s_layer != gameObject.layer) Debug.LogError($"Eyo ! Both hands have different layers ?! Or there is more than two hands ???? Anyway, i'll consider {gameObject.layer} as the last correct layer", this);
#endif
        
            s_layer = gameObject.layer;
        }

        private void Update() {
            m_isPressingTrigger = OVRInput.Get(m_grabInput) >= m_triggerGrabSensitivity; //if the trigger is pressed enough, the boolean becomes true

            if (m_objectHeld == null && !m_isThereAnObjectPulled && m_isPressingTrigger) { // If no item is held nor pulled and the trigger is pressed

                Transform transform1 = transform;
                Vector3 position = transform1.position;
                bool hitSmth = Physics.Raycast(position, transform1.forward, out RaycastHit hitData, m_pullMaxDistance, m_layersThatPulls);
#if UNITY_EDITOR
                Debug.DrawRay(position, transform.forward * m_pullMaxDistance, Color.cyan);
#endif

                if (hitSmth) {
                    // ReSharper disable once CommentTypo
                    if (hitData.transform.TryGetComponent(out GrabbableObject script)) { //If a grabbable object is in the aimline of this hand
                        m_pullCoroutine = StartCoroutine(Pull(script));
                    }
                }

            }
            else if (m_objectHeld != null) { //We keep going if an item is held

                if (m_isPressingTrigger) return; //We keep going if the trigger is NOT PRESSED anymore
            
                m_objectHeld.BeLetGo(m_grabInput);
                m_objectHeld = null;
            }
        }

        /// <summary>Will progressively attract a Grabbable object towards the hand while the appropriate trigger is pressed </summary>
        /// <param name="p_objectPulled">The object that is pulled</param>
        IEnumerator Pull(GrabbableObject p_objectPulled) {

            m_isThereAnObjectPulled = true;
            Vector3 position = p_objectPulled.transform.position;
            Vector3 originalPos = position;
            float estimatedTime = (transform.position - position).magnitude / m_pullSpeed;

            float elapsedTime = 0f;
            
            bool isPushing = false;
            bool isPulling = true;
            while (isPulling) {
                
                yield return null;
                elapsedTime += Time.deltaTime;
                
                p_objectPulled.transform.Translate((transform.position - p_objectPulled.transform.position).normalized * m_pullSpeed / Time.deltaTime);

                if (elapsedTime > estimatedTime || m_objectHeld == p_objectPulled) {
                    isPulling = false;
                }

                if (m_isPressingTrigger && (!(m_objectHeld != null & m_objectHeld != p_objectPulled))) continue;
                    isPulling = false;
                    isPushing = true;
            }

            while (isPushing) {

                yield return null;
                elapsedTime -= Time.deltaTime; //Here we go reverse in elapsed time
                
                p_objectPulled.transform.Translate((originalPos - p_objectPulled.transform.position).normalized * m_pullSpeed / Time.deltaTime);

                if (!(elapsedTime < 0f)) continue;
                    p_objectPulled.transform.position = originalPos;
                    isPushing = false;
            }
        }

        /// <summary>Will attach an object to this hand</summary>
        /// <param name="p_whoAmIEvenCatching">The object which is being grabbed by this hand</param>
        public void Catch(GrabbableObject p_whoAmIEvenCatching) {
#if UNITY_EDITOR
            if(m_objectHeld != null) Debug.LogWarning($"The object {m_objectHeld.gameObject.name} is already in this hand but i'm supposed to grab {p_whoAmIEvenCatching.gameObject.name} so what do i do ?!", this);
#endif
            m_objectHeld = p_whoAmIEvenCatching;
            p_whoAmIEvenCatching.BeGrabbed(transform, m_grabbedItemsPositionInHand);
        }
    }

}