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
        [SerializeField] private Transform m_headVR;

        [SerializeField] private Transform m_leftHandVR;
        [SerializeField] private Transform m_rightHandVR;

        /// <summary>
        /// Awake is called before the first frame update.
        /// Adding the Synchronise Vr function to the delegate called each time the server receives a VrTransform message
        /// </summary>
        private /*new*/ void Awake() {
            // base.Awake();
            ClientManager.OnReceiveVrTransform += SynchroniseVrTransform;
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

            m_rightHandVR.position = p_vrTransform.positionRightHand;
            m_rightHandVR.rotation = p_vrTransform.rotationRightHand;
        }
    }
}
