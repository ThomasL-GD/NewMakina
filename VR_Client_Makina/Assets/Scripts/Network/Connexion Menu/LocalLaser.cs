using System;
using System.Collections;
using Animation.AnimationDelegates;
using CustomMessages;
using Synchronizers;
using UnityEngine;
using UnityEngine.Serialization;

namespace Network.Connexion_Menu {

    public class LocalLaser : MonoBehaviour{

        [Header("Input")]
        [SerializeField] private OVRInput.Axis1D m_input;
        [SerializeField] [Range(0.01f, 1f)] private float m_upTriggerValue;

        [Header("Feedback")]
        [SerializeField] private LaserVFXHandler m_laserVFXHandler;
        
        [Header("Timings & size")]
        [SerializeField] [Range(0.1f, 5f)] [Tooltip("The time needed to hold before actually shooting")] private float m_laserLoadingTime;
        [SerializeField] [Range(0.1f, 5f)] [Tooltip("The time the laser feedback will stay alive")] private float m_laserDuration;
        [SerializeField] [Range(0f, 50f)] [Tooltip("The radius of the auto-aim")] private float m_laserRadius;
        [SerializeField] [Range(100f, 10000f)] [Tooltip("The max range of a laser shot")] private float m_laserMaxRange = 1000f;

        private bool m_isShooting = false;
        private bool m_shutDown = false;
        private float m_elapsedHoldingTime = 0f;
        

        [Header("Layer Masks")]
        [FormerlySerializedAs("m_mask"), SerializeField, Tooltip("Every Layer that is taken nto account when firing a laser")] LayerMask m_whatDoIHitMask = ~(1 << 7);
        [SerializeField, Tooltip("The layer(s) that are considered as targets")] LayerMask m_targetLayers = ~(1 << 7);

        private bool m_isActive = true;
        
        public delegate void LocalCharge(bool p_isActuallyCancelling, GameObject p_source);
        public static LocalCharge OnLocalCharge;
        
        public AnimationBoolChange OnLaserLoad;
        public AnimationTrigger OnLaserShot;

        private static readonly int IsLoading = Animator.StringToHash("IsAiming");
        private static readonly int IsShooting = Animator.StringToHash("IsShooting");

        // Start is called before the first frame update
        private void Start() {
            MyNetworkManager.OnReceiveInitialData += DestroyMyself;
            MyNetworkManager.OnReceiveGameEnd += ActiveMe;

            m_whatDoIHitMask |= m_targetLayers; //adding the targets to the main layer mask so we don't ignore the target layer in case the game designer didn't put them in
        }

        // Update is called once per frame
        private void Update() {

            if (!m_isActive) return;
            if (m_shutDown) return;

            if (OVRInput.Get(m_input) < m_upTriggerValue) { //Let go
                OnLaserLoad?.Invoke(IsLoading, false);
                OnLocalCharge?.Invoke(true, gameObject);
                if (m_isShooting && m_laserVFXHandler != null) m_laserVFXHandler.m_delegatedAction(new Laser() {laserState = LaserState.CancelAiming}, m_elapsedHoldingTime / m_laserLoadingTime);
                
                m_isShooting = false;
                m_elapsedHoldingTime = 0f;
            }

            
            if (m_elapsedHoldingTime > m_laserLoadingTime) { //shot
                
                OnLaserShot?.Invoke(IsShooting);
                OnLaserLoad?.Invoke(IsLoading, false);
                
                
                Vector3 handForward = transform.forward;
                Vector3 handPosition = transform.position;

                Vector3 direction;
                bool hit;

                bool targetHit = Physics.SphereCast(new Ray(handPosition, handForward.normalized), m_laserRadius, out RaycastHit targetHitInfo, m_laserMaxRange, m_targetLayers);

                if (targetHit) {
                    bool hitAWall = Physics.Raycast(handPosition, (targetHitInfo.point - handPosition).normalized, out RaycastHit wallHitInfo, (targetHitInfo.point - handPosition).magnitude, m_whatDoIHitMask, QueryTriggerInteraction.Ignore);

                    if (!hitAWall || wallHitInfo.collider.gameObject == targetHitInfo.collider.gameObject) { //Hit without any obstacle
                        
                        if(targetHitInfo.transform.gameObject.TryGetComponent(out AttackSensitiveButton script)) script.OnBeingActivated();

                        direction = targetHitInfo.point - handPosition;
                        hit = true;
                    }
                    else { //there is an obstacle on the way
                        direction = wallHitInfo.point - handPosition;
                        hit = false;
                    }
                }
                else { //If there's no target hit
                    direction = handPosition + handForward * m_laserMaxRange;
                    hit = false;
                }
                
                StartCoroutine(ShotDownLaser(handPosition, direction, hit));
                
                m_shutDown = true;
            }
            
            else if (m_isShooting) { //Holding
                OnLaserLoad?.Invoke(IsLoading, true);
                OnLocalCharge?.Invoke(false, gameObject);
                m_elapsedHoldingTime += Time.deltaTime;
                if (m_laserVFXHandler != null) m_laserVFXHandler.m_delegatedAction(new Laser() {laserState = LaserState.Aiming}, m_elapsedHoldingTime);
            }
            else if (OVRInput.Get (m_input) >= m_upTriggerValue) { // If the player press the trigger hard enough
                if (m_laserVFXHandler != null) m_laserVFXHandler.m_delegatedAction(new Laser() {laserState = LaserState.Aiming}, 0f);
                OnLaserLoad?.Invoke(IsLoading, true);
                m_isShooting = true;
                m_elapsedHoldingTime = 0f;
            }
            
        }

        IEnumerator ShotDownLaser(Vector3 p_startPos, Vector3 p_distance, bool p_isHitting) {
            
            if (m_laserVFXHandler != null) m_laserVFXHandler.m_delegatedAction(new Laser() {laserState = LaserState.Shooting, hit = p_isHitting, hitPosition = p_startPos + p_distance, length = p_distance.magnitude, origin = p_startPos}, 0f);
            
            yield return new WaitForSeconds(m_laserDuration);

            m_elapsedHoldingTime = 0f;
            m_isShooting = false;
            m_shutDown = false;
        }

        private void DestroyMyself(InitialData p_initialData) => m_isActive = false;

        private void ActiveMe(GameEnd p_gameEnd) => m_isActive = true;
    }
}