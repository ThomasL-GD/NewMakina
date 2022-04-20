using System;
using Grabbabble_Type_Beat;
using UnityEngine;

namespace Network.Connexion_Menu {

    public class LocalObjectThrow : GrabbablePhysickedObject {

        [SerializeField] private OVRInput.Button m_buttonToTp = OVRInput.Button.One;
        [SerializeField] [Tooltip("The position this object will be teleported when asked to.\nIf null, will take the original position of this object")] private Transform m_TpPosition = null;

        private void OnEnable() {
            if (m_TpPosition == null) {
                Transform currentTransform = transform;
                m_TpPosition = Instantiate(new GameObject(), currentTransform.position, currentTransform.rotation).transform;
            }

            m_layersThatCollides = (1 << 7); // We change the layerMask for collision to the 7th layer (ConnectButton) only
        }

        protected override void OnFirstTimeTouchingGround(Collision p_other) {
            base.OnFirstTimeTouchingGround(p_other);

            if (p_other.gameObject.TryGetComponent(out AttackSensitiveButton script)) {
                MyNetworkManager.OnConnection += DestroyMyself;
                script.OnBeingActivated();
            }
        }

        private void Update() {

            //OVRInput.Update();
            
            if (m_isCaught || !OVRInput.GetDown(m_buttonToTp)) return;
            
            transform.position = m_TpPosition.position;
            transform.rotation = m_TpPosition.rotation;
                
            m_rb.velocity = Vector3.zero;
        }

        private void DestroyMyself() => Destroy(gameObject);
    }

}