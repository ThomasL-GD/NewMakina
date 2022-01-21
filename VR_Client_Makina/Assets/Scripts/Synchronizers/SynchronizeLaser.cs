using System;
using System.Collections;
using CustomMessages;
using Network;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Synchronizers {
    public class SynchronizeLaser : Synchronizer {
        
        [SerializeField] private LineRenderer m_laserAiming;
        
        [Header("Colors")]
        [SerializeField] private Color m_firstColor = Color.yellow;
        [SerializeField] private Color m_lastColor = Color.white;
        
        [Header("Size")]
        [SerializeField] [Range(0f,50f)] private float m_initialLaserSize;
        [SerializeField] [Range(0f,50f)] private float m_endLaserSize;
        
        [Header("Input")]
        [SerializeField] [Range(0.01f,1f)]/**/ private float m_upTriggerValue;
        [SerializeField] private OVRInput.Axis1D m_input;
        private bool m_isTriggerPressed = false;
        
        [Header("Timings")]
        [SerializeField] [Range(0.1f,5f)] private float m_laserLoadingTime;
        private float m_elapsedTime = 0f;
        
        [Header("Prefabs")]
        [SerializeField] private GameObject m_laserPrefab;

        [SerializeField] private GameObject m_prefabParticlesWhenKill = null;
        [SerializeField] private GameObject m_prefabParticlesWallHit = null;

        [Header("Sounds")]
        [SerializeField] private AudioSource m_audioSource;
        [SerializeField] [Tooltip("If true, nice shot :)\nIf false, crippling emptiness...")] private bool m_niceShotQuestionMark = true;
        [SerializeField] private AudioClip m_niceShotSound = null;

        private bool m_isLoading = false;

        // Start is called before the first frame update
        private void Awake()
        {
            MyNetworkManager.OnLaserAimingUpdate += SynchroniseLaserAiming;
            MyNetworkManager.OnLaserShootingUpdate += SynchroniseLaserAiming;
            MyNetworkManager.OnLaserShootingUpdate += SynchroniseShot;

            m_laserAiming.enabled = false;
        }

        /// <summary>
        /// Called when the server tell us that we're aiming or shooting, displays or not the aiming line
        /// </summary>
        /// <param name="p_laser">The message sent by the server</param>
        private void SynchroniseLaserAiming(Laser p_laser) => m_laserAiming.enabled = p_laser.laserState == LaserState.Aiming;


        /// <summary>
        /// Called when the server tell us we shot
        /// Will display various feedback regarding the shot
        /// </summary>
        /// <param name="p_laser">The message sent by the server</param>
        private void SynchroniseShot(Laser p_laser) {
            if (p_laser.length == 0f) p_laser.length = 10000f;
            
            GameObject instantiate = Instantiate(m_laserPrefab);

            instantiate.transform.position = p_laser.origin;
            instantiate.transform.rotation = p_laser.hit? Quaternion.LookRotation(p_laser.hitPosition - p_laser.origin, Vector3.up) : p_laser.rotation;
            instantiate.GetComponent<LineRenderer>().SetPosition(1,  Vector3.forward*p_laser.length);

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (p_laser.length != 10000f && !p_laser.hit) {
                GameObject particles = Instantiate(m_prefabParticlesWallHit, p_laser.origin + (instantiate.transform.rotation * Vector3.forward *p_laser.length ), new Quaternion(0, 0, 0, 0));
                StartCoroutine(ParticleStopper(particles));
            }
            
            if(p_laser.hit){ //If the player is hit, we make a cool FX coz player rewarding and other arguable design reasons
                GameObject particles = Instantiate(m_prefabParticlesWhenKill, p_laser.hitPosition, new Quaternion(0, 0, 0, 0));
                StartCoroutine(ParticleStopper(particles));
                
                if (m_niceShotQuestionMark) { // Audio Feedback
                    m_audioSource.Stop();
                    m_audioSource.clip = m_niceShotSound;
                    m_audioSource.Play();
                }
                
                SynchronizeInitialData.instance.LosePcHealth();
            }
        }

        private void Update() {

            if (!MyNetworkManager.singleton.m_canSend) return;
        
            if (!m_isTriggerPressed && !m_isLoading && OVRInput.Get(m_input) >= m_upTriggerValue) { // If the laser is OFF and the player press the trigger hard enough
            
                MyNetworkManager.singleton.SendVrData<VrLaser>(new VrLaser(){laserState = LaserState.Aiming});
                m_isLoading = true;
                m_isTriggerPressed = true;
                m_elapsedTime = 0f;
            }

            if (OVRInput.Get(m_input) < m_upTriggerValue) {
                m_isTriggerPressed = false;
                m_isLoading = false;
                m_laserAiming.enabled = false;
                m_elapsedTime = 0f;
            }
            
            if (m_isLoading && m_isTriggerPressed){
            
                m_laserAiming.enabled = true;
                m_elapsedTime += Time.deltaTime;
                float ratio = m_elapsedTime / m_laserLoadingTime;
                ratio = Mathf.Clamp(ratio, 0f, 1f);
                m_laserAiming.materials[0].color = new Color(((m_firstColor.r * (1-ratio)) + (m_lastColor.r * ratio)), ((m_firstColor.g * (1-ratio)) + (m_lastColor.g * ratio)), ((m_firstColor.b * (1-ratio)) + (m_lastColor.b * ratio)));

                m_laserAiming.widthMultiplier = m_initialLaserSize * (1 - ratio) + (m_endLaserSize * ratio);
            

                if (m_elapsedTime > m_laserLoadingTime) {
                    
                    m_laserAiming.enabled = false;
                    m_elapsedTime = 0f;
                    MyNetworkManager.singleton.SendVrData<VrLaser>(new VrLaser(){laserState = LaserState.Shooting});
                    m_isLoading = false;

                }
                
            }
        }

        /// <summary>
        /// Auto destroying particles when they're done
        /// </summary>
        /// <param name="p_particleSystem">The gameobject containing the particles system</param>
        IEnumerator ParticleStopper(GameObject p_particleSystem) {
            yield return new WaitForSeconds(p_particleSystem.GetComponent<ParticleSystem>().main.startLifetime.constant);
            Destroy(p_particleSystem);
        }
    }
}
