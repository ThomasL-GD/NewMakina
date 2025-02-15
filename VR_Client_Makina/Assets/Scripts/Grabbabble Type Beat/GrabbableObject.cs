using System.Collections;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Grabbabble_Type_Beat {

    [RequireComponent(typeof(Collider))]
    public abstract class GrabbableObject : MonoBehaviour {

        [Header("Pull Material Change (optional)")]
        [SerializeField] private Material m_materialWhenAimedAt;
        private Material m_defaultMaterial;
        private Renderer m_renderer;
    
        protected bool m_isCaught = false;
        protected bool m_hasBeenCaughtInLifetime = false;
        [HideInInspector] public bool m_isGrabbable = true; 

        protected bool m_isPuttableOnlyOnce = false; // If true, once this object is let go somewhere, it can NOT be picked up again

        private Coroutine m_getGrabbedCoroutine;

        public delegate void DestroyGrabbableDelegator(GrabbableObject p_grabbableObject);
        /// <summary>Is called when this object will be destroyed</summary>
        public DestroyGrabbableDelegator OnDestroyGrabbable;

        [Header("Hand animation")]
        [SerializeField] private float m_timeToGoInHand;

        // Start is called before the first frame update
        protected virtual void Start() {
            if(gameObject.layer is not (0 or 6)) {
                GameObject go = gameObject;
                Debug.LogWarning($"This object was on a weird layer ( {go.layer} ), this code will AUTOMATICALLY put it back to the right layer.\nIf you wanted to mess with the layers of this objects, contact Blue.", go);
            }

            gameObject.layer = 6; //The sixth layer is the one for Catchable Objects

            m_renderer = GetComponent<Renderer>();
            m_defaultMaterial = m_renderer.material;
        }

        /// <summary>
        /// Will destroy this object correctly, don't use plain Destroy on this
        /// </summary>
        public void DestroyMaSoul() {
            OnDestroyGrabbable?.Invoke(this);
            BeingDestroyed();
            Destroy(gameObject);
        }
    
        /// <summary> This is called at the end of DestroyMaSoul but before the gameobject gets destroyed </summary>
        /// <remarks> Override this to do code after the call of OnDestroyGrabbable but before the destruction of this gameobject </remarks>
        protected virtual void BeingDestroyed(){}

        /// <summary>Will set a new parent to this object</summary>
        /// <param name="p_newParent">The new parent you want for thi object</param>
        /// <param name="p_offsetPositionInHand">The offset this item will move to when going towards the hand</param>
        public virtual void BeGrabbed(Transform p_newParent, Vector3 p_offsetPositionInHand) {
            transform.SetParent(p_newParent);
            m_getGrabbedCoroutine = StartCoroutine(GoToHandCenter(m_timeToGoInHand, p_offsetPositionInHand));
        
            m_isCaught = true;
            m_hasBeenCaughtInLifetime = true;
        }

        /// <summary>Will let the item go and be the child of no one</summary>
        public virtual void BeLetGo(OVRInput.Axis1D p_handInput) {
            if(m_getGrabbedCoroutine != null) StopCoroutine(m_getGrabbedCoroutine);
        
            transform.SetParent(null);
        
            m_isCaught = false;

            if (!m_isPuttableOnlyOnce) return;
        
            if (m_hasBeenCaughtInLifetime && !m_isCaught) {
                m_isGrabbable = false;
            }
        }

        private void OnTriggerStay(Collider p_other) {
            if (!m_isGrabbable) return;
        
            if (p_other.gameObject.layer == VrHandBehaviour.s_layer && p_other.TryGetComponent(out VrHandBehaviour script)) {
                if (script.isFree && !m_isCaught) {
                    script.Catch(this);
                }
            }
        }

        private IEnumerator GoToHandCenter(float p_time, Vector3 p_localPosToGo) {
            float elapsedTime = 0f;
            Vector3 originalLocalPos = transform.localPosition;
            Vector3 pathToGo = p_localPosToGo - originalLocalPos;
        
            while (elapsedTime < p_time){
                yield return new WaitForSeconds(Time.fixedDeltaTime);
                elapsedTime += Time.fixedDeltaTime;
                float ratio = elapsedTime / p_time;
                transform.localPosition = originalLocalPos + (pathToGo * ratio);
            }

            transform.localPosition = p_localPosToGo;
        }

        /// <summary>Will Change the material of a GrabbableObject to give feedback</summary>
        /// <param name="p_isActivated">If true, will set the "feedback" material, if false will change back to the original material.<br/>Is true by default</param>
        public void ChangeMaterial(bool p_isActivated = true) {
            m_renderer.material = p_isActivated ? m_materialWhenAimedAt : m_defaultMaterial;
        }
    }

}
