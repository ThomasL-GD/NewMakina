using CustomMessages;
using Network;
using UnityEngine;

namespace Synchronizers {
    public class SynchronizeSendVrRig : Synchronizer<SynchronizeSendVrRig> {

        [Header("VR elements to track")]
        [SerializeField] private Transform m_head;
        [SerializeField] private Transform m_leftHand;
        [SerializeField] public Transform m_rightHand;
        
        
        public void Update() {
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
            
            MyNetworkManager.singleton.SendVrData(new VrTransform() {positionHead = m_head.position, rotationHead = m_head.rotation, positionLeftHand = m_leftHand.position, rotationLeftHand = m_leftHand.rotation, positionRightHand = m_rightHand.position, rotationRightHand = m_rightHand.rotation});
        }
    }
}