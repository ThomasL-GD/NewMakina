using System;
using System.Collections;
using System.Collections.Generic;
using CustomMessages;
using Network;
using Synchronizers;
using UnityEngine;

namespace Synchronizers {
    public class SynchronizeExplorer : Synchronizer<SynchronizeExplorer>
    {

        [SerializeField] private Transform m_explorerTransform = null;
        [SerializeField] private Transform m_explorerHeadTransform = null;

        private void Start() {
            if(m_explorerTransform == null) Debug.LogError("No Explorer serialized here !", this);
            if(m_explorerHeadTransform == null) Debug.LogError("No Explorer Head serialized here !", this);
        }

        private void OnEnable() {
            MyNetworkManager.OnPcTransformUpdate += MoveMyself;
        }

        private void OnDisable() {
            MyNetworkManager.OnPcTransformUpdate -= MoveMyself;
        }

        /// <summary>
        /// Will move the gameObject it's attached to accordingly to the other player's position and rotation 
        /// </summary>
        /// <param name="p_pcTransform">The actual data we received</param>
        private void MoveMyself(PcTransform p_pcTransform) {
            m_explorerTransform.position = p_pcTransform.position;

            Quaternion rotation = m_explorerTransform.rotation;
            rotation = Quaternion.Euler(rotation.eulerAngles.x, p_pcTransform.rotation.eulerAngles.y, rotation.eulerAngles.z);
            m_explorerTransform.rotation = rotation;
            m_explorerHeadTransform.rotation = Quaternion.Euler(p_pcTransform.rotation.eulerAngles.x, m_explorerHeadTransform.rotation.eulerAngles.y, p_pcTransform.rotation.eulerAngles.z);
        }
    }
}