using System;
using System.Collections;
using Animation.AnimationDelegates;
using CustomMessages;
using Synchronizers;
using UnityEngine;
using UnityEngine.Serialization;

namespace Network.Connexion_Menu {

    [RequireComponent(typeof(LineRenderer))]
    public class LocalLaser : MonoBehaviour{

        [Header("Input")]
        [SerializeField] private OVRInput.Axis1D m_input;
        [SerializeField] [Range(0.01f, 1f)]/**/ private float m_upTriggerValue;

        [Header("Feedback")]
        [SerializeField] private LaserVFXHandler m_laserVFXHandler;
        
        [Header("Timings & size")]
        [SerializeField] [Range(0.1f, 5f)] [Tooltip("The time needed to hold before actually shooting")] private float m_laserLoadingTime;
        [SerializeField] [Range(0.1f, 5f)] [Tooltip("The time the laser feedback will stay alive")] private float m_laserDuration;
        [SerializeField] [Range(0f, 50f)] [Tooltip("The size of the laser at the very beginning of loading")] private float m_initialLaserSize;
        [SerializeField] [Range(0f, 50f)] [Tooltip("The size of the laser at the very beginning of loading")] private float m_endLaserSize;
        [SerializeField] [Range(0f, 50f)] [Tooltip("The radius of the auto-aim")] private float m_laserRadius;

        private bool m_isShooting = false;
        private bool m_shutDown = false;
        private float m_elapsedHoldingTime = 0f;

        [HideInInspector] public AttackSensitiveButton m_targetThatIsSensitive = null;
        [HideInInspector] public Transform m_target = null;
        private Transform m_transformTarget => (m_targetThatIsSensitive == null) ? m_target : m_targetThatIsSensitive.transform;
        

        [Header("Layer Masks")]
        [FormerlySerializedAs("m_mask"), SerializeField] LayerMask m_whatDoIHitMask = ~(1 << 7);

        private bool m_isActive = true;

        public delegate void NewTargetDelegator(Transform p_newTarget);
        public delegate void NewSensitiveTargetDelegator(AttackSensitiveButton p_newTarget);
        
        public delegate void LocalCharge(bool p_isActuallyCancelling, GameObject p_source);
        public static LocalCharge OnLocalCharge;

        public static NewTargetDelegator SetNewTargetForAll;
        public static NewSensitiveTargetDelegator SetNewSensitiveTargetForAll;

        public AnimationBoolChange OnLaserLoad;
        public AnimationTrigger OnLaserShot;

        private static readonly int IsLoading = Animator.StringToHash("IsLoading");
        private static readonly int IsShooting = Animator.StringToHash("IsShooting");

        // Start is called before the first frame update
        private void Start() {
            MyNetworkManager.OnReceiveInitialData += DestroyMyself;
            MyNetworkManager.OnReceiveGameEnd += ActiveMe;
            SetNewTargetForAll += SetNewTarget;
            SetNewSensitiveTargetForAll += SetNewSensitiveTarget;
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
                
                if(m_transformTarget != null) {

                    //Setting the right length for the laser aiming previsualization
                    Vector3 targetPos = m_transformTarget.position;
                    Vector3 laserCriticalPath = targetPos - handPosition;

                    // Hitboxes Verification (blame Blue)
                    bool hitAWall = Physics.Raycast(handPosition, laserCriticalPath.normalized, out RaycastHit wallHitInfo, laserCriticalPath.magnitude, m_whatDoIHitMask, QueryTriggerInteraction.Ignore);
                
                    
                    bool hitSmth = Physics.Raycast(handPosition, handForward.normalized, out RaycastHit hitInfo, 10000f, m_whatDoIHitMask, QueryTriggerInteraction.Ignore);
                    
                    bool hitTheTarget;
                    Vector3 direction;
                    switch (hitAWall) {
                        case false : { //If there's no wall between
                            // So we measure the distance of the player's position from the line of the laser
                            float distance = Vector3.Cross(handForward, targetPos - handPosition).magnitude;

                            //if the distance between the line of fire and the player's position is blablablbalalboom... he ded
                            hitTheTarget = distance <= m_laserRadius && Vector3.Angle(targetPos - handPosition, handForward) < 90f;

                            //Giving the 
                            direction = hitTheTarget ? (laserCriticalPath) : (hitSmth ? hitInfo.point - handPosition : handForward * 10000f);

                            break; }

                        case true : { //If there's a wall between the target and the hand
                            hitTheTarget = false;
                            direction = hitSmth ? hitInfo.point - handPosition : handForward * 10000f;
                            break; }
                    }
                    
                    StartCoroutine(ShotDownLaser(handPosition, direction, hitTheTarget));
                    
                    if (!hitTheTarget) return;
                        if (m_targetThatIsSensitive != null) m_targetThatIsSensitive.OnBeingActivated();
                        else{
                            if(hitInfo.transform.gameObject.TryGetComponent(out AttackSensitiveButton script)) script.OnBeingActivated();
#if UNITY_EDITOR
                            else Debug.LogError("No script found on target ???");
#endif
                        }
                        
                }
                else if (m_transformTarget == null) {
                    
                    bool hitSmth = Physics.Raycast(handPosition, handForward.normalized, out RaycastHit hitInfo, 100000f, m_whatDoIHitMask, QueryTriggerInteraction.Ignore);

                    bool hit;
                    switch (hitSmth) {
                        case true: {
                            m_shutDown = true;
                            if (hitInfo.transform.gameObject.TryGetComponent(out AttackSensitiveButton script)) {
                                hit = true;
                                script.OnBeingActivated();
                            }
                            else hit = false;

                            break;
                        }
                        case false:
                            hit = false;
                            break;
                    }

                    StartCoroutine(ShotDownLaser(handPosition, hitInfo.point == Vector3.zero ? handPosition + handForward.normalized * 10000f : hitInfo.point - handPosition, hit));
                }


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

        private void SetNewTarget(Transform p_newTarget) {
            m_target = p_newTarget;
        }

        private void SetNewSensitiveTarget(AttackSensitiveButton p_newTarget) {
            m_targetThatIsSensitive = p_newTarget;
        }

        private void DestroyMyself(InitialData p_initialData) => m_isActive = false;

        private void ActiveMe(GameEnd p_gameEnd) => m_isActive = true;

        private void OnDestroy() {
            SetNewTargetForAll -= SetNewTarget;
            SetNewSensitiveTargetForAll -= SetNewSensitiveTarget;
        }
    }
}