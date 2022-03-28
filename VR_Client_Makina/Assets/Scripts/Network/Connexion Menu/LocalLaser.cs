using System;
using System.Collections;
using Animation.AnimationDelegates;
using CustomMessages;
using Synchronizers;
using UnityEngine;
using UnityEngine.Serialization;

namespace Network.Connexion_Menu {

    [RequireComponent(typeof(LineRenderer))]
    public class LocalLaser : MonoBehaviour {
        
        [Header("Line Colors")]
        [SerializeField] private Color m_firstColor = Color.yellow;
        [SerializeField] private Color m_lastColor = Color.white;
        
        [Header("prefabs")]
        [SerializeField] private GameObject m_laserShotPrefab;
        [SerializeField] private GameObject m_prefabParticlesWhenKill;
        [SerializeField] private GameObject m_prefabParticlesWallHit;
        
        private LineRenderer m_line = null;
        
        [Header("Input")]
        [SerializeField] private OVRInput.Axis1D m_input;
        [SerializeField] [Range(0.01f, 1f)]/**/ private float m_upTriggerValue;
        
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

        public static NewTargetDelegator SetNewTargetForAll;
        public static NewSensitiveTargetDelegator SetNewSensitiveTargetForAll;

        public AnimationBoolChange OnLaserLoad;
        public AnimationTrigger OnLaserShot;

        private static readonly int IsLoading = Animator.StringToHash("IsLoading");
        private static readonly int IsShooting = Animator.StringToHash("IsShooting");

        // Start is called before the first frame update
        private void Start() {
            m_line = GetComponent<LineRenderer>();
            m_line.enabled = false;
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
                m_isShooting = false;
                m_line.enabled = false;
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
                    
                    bool hitSmth = Physics.Raycast(handPosition, handForward.normalized, out RaycastHit hitInfo, 10000f, m_whatDoIHitMask, QueryTriggerInteraction.Ignore);

                    bool hit;
                    switch (hitSmth) {
                        case true: {
                            m_shutDown = true;
                            if (hitInfo.transform.gameObject.TryGetComponent(out AttackSensitiveButton script)) {
                                hit = true;
                                script.OnBeingActivated();
                            }
                            else hit = false;

                            m_line.enabled = false;
                            break;
                        }
                        case false:
                            hit = false;
                            break;
                    }

                    StartCoroutine(ShotDownLaser(handPosition, hitInfo.point - handPosition, hit));
                }


                m_shutDown = true;
                m_line.enabled = false;
            }
            
            else if (m_isShooting) { //Holding
                OnLaserLoad?.Invoke(IsLoading, true);
                m_line.enabled = true;
                m_elapsedHoldingTime += Time.deltaTime;
                float ratio = m_elapsedHoldingTime / m_laserLoadingTime;
                m_line.materials[0].color = new Color(((m_firstColor.r * (1-ratio)) + (m_lastColor.r * ratio)), ((m_firstColor.g * (1-ratio)) + (m_lastColor.g * ratio)), ((m_firstColor.b * (1-ratio)) + (m_lastColor.b * ratio)));

                m_line.widthMultiplier = m_initialLaserSize * (1 - ratio) + (m_endLaserSize * ratio);

                //Setting the right length for the laser aiming previsualization
                Vector3 forward = transform.forward;
                Vector3 position = transform.position;
                bool isHitting = Physics.Raycast(position, forward, out RaycastHit ray, Mathf.Infinity, m_whatDoIHitMask);
                m_line.SetPosition(1, position + (forward * (isHitting ? ray.distance : 100000f)));
                m_line.SetPosition(0, position);
            }
            else if (OVRInput.Get (m_input) >= m_upTriggerValue) { // If the player press the trigger hard enough
                OnLaserLoad?.Invoke(IsLoading, true);
                m_isShooting = true;
                m_elapsedHoldingTime = 0f;
            }
            
        }

        IEnumerator ShotDownLaser(Vector3 p_startPos, Vector3 p_distance, bool p_isHitting) {
            
            m_line.enabled = false;

            GameObject instantiate = Instantiate(m_laserShotPrefab);

            instantiate.transform.position = p_startPos;
            LineRenderer line = instantiate.GetComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.SetPosition(0, p_startPos);
            line.SetPosition(1, p_startPos + p_distance);
            
            if(p_isHitting){ //If the player is hit, we make a cool FX coz player rewarding and other arguable design reasons
                GameObject particles = Instantiate(m_prefabParticlesWhenKill, p_startPos + p_distance, new Quaternion(0, 0, 0, 0));
                bool isFound = particles.TryGetComponent(out ParticleSystem comp);
                StartCoroutine(ParticleStopper(particles, isFound ? comp.main.duration : 5f));
            }
            else if (p_distance.magnitude < 10000f && !p_isHitting) {
                GameObject particles = Instantiate(m_prefabParticlesWallHit, p_startPos + p_distance, new Quaternion(0, 0, 0, 0));
                bool isFound = particles.TryGetComponent(out ParticleSystem comp);
                StartCoroutine(ParticleStopper(particles, isFound ? comp.main.duration : 5f));
            }
            
            
            yield return new WaitForSeconds(m_laserDuration);
            
            
            Destroy(instantiate);
            
            m_elapsedHoldingTime = 0f;
            m_isShooting = false;
            m_shutDown = false;
        }

        IEnumerator ParticleStopper(GameObject p_particles, float p_time) {
            yield return new WaitForSeconds(p_time);
            Destroy(p_particles);
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
