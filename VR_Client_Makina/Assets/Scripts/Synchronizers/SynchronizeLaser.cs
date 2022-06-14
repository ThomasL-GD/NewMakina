using System;
using Animation.AnimationDelegates;
using CustomMessages;
using Network;
using Network.Connexion_Menu;
using UnityEngine;

namespace Synchronizers {
    public class SynchronizeLaser : Synchronizer<SynchronizeLaser> {
        
        [SerializeField] private LayerMask m_layersThatCollidesWithLaser;
        
        [Header("Input")]
        [SerializeField] [Range(0.01f,1f)]/**/ private float m_upTriggerValue;
        [SerializeField] private OVRInput.Axis1D m_input;
        private bool m_isTriggerPressed = false;
        
        [Header("Timings")]
        [SerializeField] [Range(0.1f,5f)] private float m_laserLoadingTime; //TODO : move that to initial data
        private float m_elapsedTime = 0f;

        [Header("Sounds")]
        [SerializeField] private AudioSource m_audioSource;
        [SerializeField] [Tooltip("If true, nice shot :)\nIf false, crippling emptiness...")] private bool m_niceShotQuestionMark = true;
        [SerializeField] private AudioClip m_niceShotSound = null;

        [Header("Feedback")]
        [SerializeField] private LaserVFXHandler m_laserVFXHandler = null;

        private bool m_isLoading = false;

        public AnimationBoolChange OnLaserLoad;
        public AnimationTrigger OnLaserShot;

        private static readonly int IsLoading = Animator.StringToHash("IsAiming");
        private static readonly int IsShooting = Animator.StringToHash("IsShooting");

        // Start is called before the first frame update
        private void Awake() {

            MyNetworkManager.OnReceiveInitialData += ReceiveInitialData;
            MyNetworkManager.OnLaserAimingUpdate += SynchroniseLaserAiming;
            MyNetworkManager.OnLaserShootingUpdate += SynchroniseLaserAiming;
            MyNetworkManager.OnLaserShootingUpdate += SynchroniseShot;
        }

        /// <summary/> Function called when the client receives the initial data
        /// <param name="p_initialData"> The message sent by the server</param>
        void ReceiveInitialData(InitialData p_initialData) {
            //TODO light be annoying if the menuing is fluid
            m_elapsedTime = 0f;
            m_isTriggerPressed = false;
            m_isLoading = false;
        }

        /// <summary>
        /// Called when the server tell us that we're aiming or shooting, displays or not the aiming line
        /// </summary>
        /// <param name="p_laser">The message sent by the server</param>
        private void SynchroniseLaserAiming(Laser p_laser) {
            
            switch (p_laser.laserState) {
                case LaserState.Aiming:
                    break;
                case LaserState.Shooting:
                    break;
                case LaserState.CancelAiming:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        /// <summary>
        /// Called when the server tell us we shot
        /// Will display various feedback regarding the shot
        /// </summary>
        /// <param name="p_laser">The message sent by the server</param>
        private void SynchroniseShot(Laser p_laser) {
            if (p_laser.length == 0f) p_laser.length = 10000f;
            
            Debug.Log($"hit position : {p_laser.hitPosition}    hit : {p_laser.hit}", this);
            if(m_laserVFXHandler != null)m_laserVFXHandler.m_delegatedAction(new Laser(){hit = p_laser.hit, hitPosition = p_laser.hitPosition, laserState = p_laser.laserState}, 0f);
            OnLaserLoad?.Invoke(IsLoading, false);
            OnLaserShot?.Invoke(IsShooting);
            
            if(p_laser.hit){ //If the player is hit, we make a cool FX coz player rewarding and other arguable design reasons
                
                if (m_niceShotQuestionMark) { // Audio Feedback
                    m_audioSource.Stop();
                    m_audioSource.clip = m_niceShotSound;
                    m_audioSource.Play();
                }
                
                SynchronizeInitialData.instance.LosePcHealth();
            }

            bool hasRaycastHit = Physics.Raycast(p_laser.origin, p_laser.hitPosition - p_laser.origin, out RaycastHit hit, 1000f, 1 << 7);
            if (hasRaycastHit && hit.transform.gameObject.TryGetComponent(out AttackSensitiveButton script)) script.OnBeingActivated();
        }

        private void Update() {

            if (!MyNetworkManager.singleton.m_canSend) return;
        
            if (!m_isTriggerPressed && !m_isLoading && OVRInput.Get(m_input) >= m_upTriggerValue) { // If the laser is OFF and the player press the trigger hard enough
            
                MyNetworkManager.singleton.SendVrData<VrLaser>(new VrLaser(){laserState = LaserState.Aiming});
                m_isLoading = true;
                m_isTriggerPressed = true;
                m_elapsedTime = 0f;
                if(m_laserVFXHandler != null)m_laserVFXHandler.m_delegatedAction(new Laser() {laserState = LaserState.Aiming}, 0f);
                OnLaserLoad?.Invoke(IsLoading, true);
            }

            if (OVRInput.Get(m_input) < m_upTriggerValue) { //let go of the trigger
                
                MyNetworkManager.singleton.SendVrData<VrLaser>(new VrLaser(){laserState = LaserState.CancelAiming});
                OnLaserLoad?.Invoke(IsLoading, false);
                
                if(m_isTriggerPressed && m_laserVFXHandler != null) m_laserVFXHandler.m_delegatedAction(new Laser() {laserState = LaserState.CancelAiming}, m_elapsedTime / m_laserLoadingTime);
                
                m_isTriggerPressed = false;
                m_isLoading = false;
                m_elapsedTime = 0f;
            }

            if (!m_isLoading || !m_isTriggerPressed) return; // Holding
                m_elapsedTime += Time.deltaTime;
                if(m_laserVFXHandler != null)m_laserVFXHandler.m_delegatedAction(new Laser() {laserState = LaserState.Aiming}, m_elapsedTime / m_laserLoadingTime);

                if (!(m_elapsedTime > m_laserLoadingTime)) return; // shooting
            
                    m_elapsedTime = 0f;
                    
                    MyNetworkManager.singleton.SendVrData<VrLaser>(new VrLaser(){laserState = LaserState.Shooting, origin = SynchronizeSendVrRig.Instance.m_fingertipLaser.position, rotation = SynchronizeSendVrRig.Instance.m_fingertipLaser.rotation});
                    m_isLoading = false;
        }
    }
}
