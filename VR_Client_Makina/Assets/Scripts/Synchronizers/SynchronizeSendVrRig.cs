using CustomMessages;
using Grabbabble_Type_Beat;
using Network;
using UnityEngine;

namespace Synchronizers {
    public class SynchronizeSendVrRig : Synchronizer<SynchronizeSendVrRig> {

        [Header("VR elements to track")]
        [SerializeField] public Transform m_head;
        [SerializeField] private Transform m_leftHand;
        [SerializeField] public Transform m_rightHand;

        private VrHandBehaviour m_leftScript = null;
        private VrHandBehaviour m_rightScript = null;

        private void Start() {
            if (m_leftHand.TryGetComponent(out VrHandBehaviour lScript)) m_leftScript = lScript;
#if UNITY_EDITOR
            else Debug.LogError("The left hand doesn't have any hand script on them !");
#endif
            
            if (m_rightHand.TryGetComponent(out VrHandBehaviour rScript)) m_rightScript = rScript;
#if UNITY_EDITOR
            else Debug.LogError("The left hand doesn't have any hand script on them !");
#endif
        }
        
        
        private void Update() {
            if (m_head == null) {
                Debug.LogError("No VR Head serialized here !", this);
                return;
            }
            if (m_leftHand == null) {
                Debug.LogError("No VR Left Hand serialized here !", this);
                return;
            }
            if (m_rightHand == null) {
                Debug.LogError("No VR Right Hand serialized here !", this);
                return;
            }
            
            MyNetworkManager.singleton.SendVrData(new VrTransform() {positionHead = m_head.position, rotationHead = m_head.rotation, positionLeftHand = m_leftHand.position, rotationLeftHand = m_leftHand.rotation, isLeftHandClenched = m_leftScript.m_isPressingTrigger, positionRightHand = m_rightHand.position, rotationRightHand = m_rightHand.rotation, isRightHandClenched = m_rightScript.m_isPressingTrigger});
        }
    }
}