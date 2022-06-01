using System;
using UnityEngine;
using CustomMessages;

namespace Synchronizers
{
    
    /// <summary>
    /// This component will synchronise the transforms of the VR player with it's representation on the PC side
    /// these transforms belong to VR player's :  head, right hand, and left hand
    /// </summary>

    public class SynchronizeVrTransforms : Synchronizer<SynchronizeVrTransforms>
    {
        /// <summary>
        /// The Transforms of Pc Representations of the VR player
        /// </summary>
        [SerializeField] public Transform m_headVR;

        [SerializeField] private Transform m_leftHandVR;
        [SerializeField] private Transform m_rightHandVR;
        
        /// <summary>The local position of the child of the right hand that determines the point from where the laser is shot </summary>
        [SerializeField] private Transform m_fingertipLaser;
        
        private VrHand m_leftScript;
        private VrHand m_rightScript;
        private static readonly int IsGrabbing = Animator.StringToHash("IsGrabbing");

        /// <summary>
        /// Awake is called before the first frame update.
        /// Adding the Synchronise Vr function to the delegate called each time the server receives a VrTransform message
        /// </summary>
        private /*new*/ void Awake() {
            // base.Awake();
            ClientManager.OnReceiveVrTransform += SynchroniseVrTransform;
            ClientManager.OnReceiveVrInitialValues += PlaceFingertip;
        }

        private void PlaceFingertip(VrInitialValues p_vrInitialValues) {
            m_fingertipLaser.localPosition = p_vrInitialValues.fingertipOffset;
        }

        private void Start() {
            if (m_leftHandVR.TryGetComponent(out VrHand lScript)) m_leftScript = lScript;
#if UNITY_EDITOR
            else Debug.LogError("The left hand doesn't have any hand script on them !");
#endif
            
            if (m_rightHandVR.TryGetComponent(out VrHand rScript)) m_rightScript = rScript;
#if UNITY_EDITOR
            else Debug.LogError("The left hand doesn't have any hand script on them !");
#endif
        }

        /// <summary>
        /// Update is called once per frame
        /// </summary>
        /// <param name="p_vrTransform"></param>
        void SynchroniseVrTransform(VrTransform p_vrTransform)
        {
            m_headVR.position = p_vrTransform.positionHead;
            m_headVR.rotation = p_vrTransform.rotationHead;

            m_leftHandVR.position = p_vrTransform.positionLeftHand;
            m_leftHandVR.rotation = p_vrTransform.rotationLeftHand;
            m_leftScript.m_animator.SetBool(IsGrabbing, p_vrTransform.isLeftHandClenched);

            m_rightHandVR.position = p_vrTransform.positionRightHand;
            m_rightHandVR.rotation = p_vrTransform.rotationRightHand;;
            m_rightScript.m_animator.SetBool(IsGrabbing, p_vrTransform.isRightHandClenched);
        }
    }
}
