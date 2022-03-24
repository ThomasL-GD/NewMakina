using Network.Connexion_Menu;
using Synchronizers;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Animation {

    namespace AnimationDelegates {

        public delegate void AnimationBoolChange(int p_id, bool p_value);
        public delegate void AnimationIntChange(int p_id, int p_value);
        public delegate void AnimationTrigger(int p_id);

    }
    public class VrHandAnimationController : MonoBehaviour {

        [Header("Animators")]
        [SerializeField, Tooltip("The animators that will be affected by this script")] private Animator[] m_animators;
        
        [Space, Header("Animation Variables")]
        [SerializeField, Tooltip("To make the animator above have animations upon a local laser")] private LocalLaser m_localLaser = null;
        [SerializeField, Tooltip("To make the animator above have animations upon a server laser")] private SynchronizeLaser m_synchronizeLaser = null;
        [SerializeField, Tooltip("To make the animator above have animations upon a vr hand actions")] private VrHandBehaviour m_vrHand = null;
        
        
        // Start is called before the first frame update
        void Awake() {

            if (m_animators.Length < 1) {
                if (TryGetComponent(out Animator animator)) {
                    m_animators = new Animator[1] {animator};
                }
#if UNITY_EDITOR
                else Debug.LogError("No animator serialized here (≖_≖ )", this);
#endif
            }
            
            if(m_localLaser != null) {
                m_localLaser.OnLaserCancel += AnimateAll;
                m_localLaser.OnLaserCharge += AnimateAll;
                m_localLaser.OnLaserShot += AnimateAll;
            }

            if(m_synchronizeLaser != null) {
                m_synchronizeLaser.OnLaserCancel += AnimateAll;
                m_synchronizeLaser.OnLaserCharge += AnimateAll;
                m_synchronizeLaser.OnLaserShot += AnimateAll;
            }

            if (m_vrHand == null) return;
                m_vrHand.OnClosedHandChange += AnimateAll;
                m_vrHand.OnGrabItemChange += AnimateAll;

        }


        #region AnimatorLoops

        /// <summary>Sets the value of a boolean of all the animators at once</summary>
        /// <remarks>This function is overloaded with several animator variable types</remarks>
        /// <param name="p_id">The id of the boolean you want changed</param>
        /// <param name="p_value">The value of the boolean to set</param>
        private void AnimateAll(int p_id, bool p_value) {
            foreach (Animator animator in m_animators) {
                animator.SetBool(p_id, p_value);
            }
        }

        /// <summary>Sets the value of an int of all the animators at once</summary>
        /// <remarks>This function is overloaded with several animator variable types</remarks>
        /// <param name="p_id">The id of the int you want changed</param>
        /// <param name="p_value">The value of the int to set</param>
        private void AnimateAll(int p_id, int p_value) {
            foreach (Animator animator in m_animators) {
                animator.SetInteger(p_id, p_value);
            }
        }

        /// <summary>Triggers a trigger of all the animators at once</summary>
        /// <remarks>This function is overloaded with several animator variable types</remarks>
        /// <param name="p_id">The id of the trigger you want triggered</param>
        private void AnimateAll(int p_id) {
            foreach (Animator animator in m_animators) {
                animator.SetTrigger(p_id);
            }
        }

        #endregion
    }
    
}
