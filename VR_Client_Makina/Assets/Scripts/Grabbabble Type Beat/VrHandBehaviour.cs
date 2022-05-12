using System.Collections;
using Animation.AnimationDelegates;
using JetBrains.Annotations;
using UnityEditor.TextCore.Text;
using UnityEngine;

namespace Grabbabble_Type_Beat {

    [RequireComponent(typeof(Collider), typeof(Rigidbody))]
    public class VrHandBehaviour : MonoBehaviour {
    
        [SerializeField] private OVRInput.Axis1D m_grabInput = OVRInput.Axis1D.PrimaryHandTrigger;
    
        [SerializeField] [Range(0.01f,1f)]/**/ private float m_triggerGrabSensitivity = 0.2f;
        //[SerializeField] [Range(0.01f,1f)]/**/ private float m_triggerLetGoSensitivity = 0.9f;
    
        [SerializeField] private Vector3 m_grabbedItemsPositionInHand;

        [Header("Pull")]
        [SerializeField] private bool m_pull = true;
        [SerializeField] [Range(10f,100000f)] private float m_pullMaxDistance;
        [SerializeField] [Range(0.1f,100f)] private float m_pullSpeed;
        [SerializeField] [Range(0.1f,100f)] private float m_pullRadius;
        [SerializeField] private LayerMask m_layersThatPulls;
        private bool m_isThereAnObjectPulled = false;
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
        private static readonly int ActivatableFeedback = Shader.PropertyToID("_ActivatableFeedback");

        private void Start() {
        
#if UNITY_EDITOR
            //if (m_triggerGrabSensitivity < m_triggerLetGoSensitivity) Debug.LogWarning("What the fuck is that balancing ?!", this);
            if (s_layer != -1 && s_layer != gameObject.layer) Debug.LogError($"Eyo ! Both hands have different layers ?! Or there is more than two hands ???? Anyway, i'll consider {gameObject.layer} as the last correct layer", this);
#endif
        
            s_layer = gameObject.layer;
        }

        private void Update() {
            m_isPressingTrigger = OVRInput.Get(m_grabInput) >= m_triggerGrabSensitivity; //if the trigger is pressed enough, the boolean becomes true

            if (m_objectHeld == null && !m_isThereAnObjectPulled) { // If no item is held nor pulled

                if (!m_pull) return; //We can disable the entire feature if we want to
                
                Transform transform1 = transform;
                Vector3 position = transform1.position;
                RaycastHit[] hitResult = Physics.SphereCastAll(position, m_pullRadius, transform1.forward, m_pullMaxDistance, m_layersThatPulls);
#if UNITY_EDITOR
                Debug.DrawRay(position, transform.forward * m_pullMaxDistance, Color.cyan);
#endif

                if (hitResult.Length > 0) {

                    RaycastHit bestPick = default; 
                    float bestDotSoFar = -1;
                    foreach (RaycastHit raycastHit in hitResult) { //We check which target hit is the most centered one
                        
                        if (!(Vector3.Dot(transform.forward, (raycastHit.transform.position - transform.position).normalized) > bestDotSoFar)) continue;
                        var transform2 = transform;
                        bestDotSoFar = Vector3.Dot(transform2.forward, (raycastHit.transform.position - transform2.position).normalized);
                        bestPick = raycastHit;
                    }
                    
                    
                    // ReSharper disable once CommentTypo
                    if (!bestPick.transform.TryGetComponent(out GrabbableObject script)) return; //If a grabbable object is in the aimline of this hand
                    Shader.SetGlobalVector(ActivatableFeedback, script.transform.position);
                            
                    if(m_isPressingTrigger) StartCoroutine(Pull(script)); //If the trigger is pressed, we start the pulling
                    
                }
                else {
                    Shader.SetGlobalVector(ActivatableFeedback, Vector4.zero);
                }
            }
            else if (m_objectHeld != null) { //We keep going if an item is held
                
                Shader.SetGlobalVector(ActivatableFeedback, Vector4.zero);

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

                if (p_objectPulled == null) {
                    Debug.LogWarning($"The object targeted does not exist anymore, I quit", this);
                    yield break;
                }
                
                elapsedTime += Time.deltaTime;
                Vector3 position1 = p_objectPulled.transform.position;
                Shader.SetGlobalVector(ActivatableFeedback, position1);

                Transform transform1;
                (transform1 = p_objectPulled.transform).Translate((transform.position - position1).normalized * m_pullSpeed * Time.deltaTime);
                
                Shader.SetGlobalVector(ActivatableFeedback, transform1.position);

                if (m_objectHeld != null) {
                    isPulling = false;
                }

                if (m_isPressingTrigger && (!(m_objectHeld != null & m_objectHeld != p_objectPulled))) continue;
                    isPulling = false;
                    isPushing = true;
            }

            m_isThereAnObjectPulled = false;
            Shader.SetGlobalVector(ActivatableFeedback, Vector4.zero);
            
            // p_objectPulled.BeGrabbed(p_objectPulled.transform.parent, Vector3.zero);
            // p_objectPulled.BeLetGo(m_grabInput);

            while (isPushing) {
            
                yield return null;
                elapsedTime -= Time.deltaTime; //Here we go reverse in elapsed time
                
                p_objectPulled.transform.Translate((originalPos - p_objectPulled.transform.position).normalized * m_pullSpeed * Time.deltaTime);
            
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