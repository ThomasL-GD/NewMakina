using System.Collections;
using Synchronizers;
using UnityEngine;

namespace Network.Connexion_Menu {

    [RequireComponent(typeof(LineRenderer))]
    public class LocalLaser : MonoBehaviour {
        
        [SerializeField] private Color m_firstColor = Color.yellow;
        [SerializeField] private Color m_lastColor = Color.white;
        [SerializeField] private Color m_shotColor = Color.red;
        [SerializeField] private OVRInput.Axis1D m_input;
        [SerializeField] [Range(0.01f,1f)]/**/ private float m_upTriggerValue;
        [SerializeField] [Range(0.1f,5f)] private float m_laserLoadingTime;
        [SerializeField] [Range(0.1f,5f)] private float m_laserDuration;
        
        [SerializeField] [Range(0f,50f)] private float m_initialLaserSize;
        [SerializeField] [Range(0f,50f)] private float m_endLaserSize;
        [SerializeField] [Range(0f,50f)] private float m_shotLaserSize;
        [SerializeField] LayerMask m_mask;

        private LineRenderer m_line = null;
        private bool m_isShooting = false;
        private bool m_shutDown = false;
        private float m_elapsedTime = 0f;

        // Start is called before the first frame update
        void Start() {
            m_line = GetComponent<LineRenderer>();
            m_line.enabled = false;
        }

        // Update is called once per frame
        void Update() {

            if (m_shutDown) return;

            if (OVRInput.Get(m_input) < m_upTriggerValue) {
                m_isShooting = false;
                m_line.enabled = false;
                m_elapsedTime = 0f;
            }

            if (m_elapsedTime > m_laserLoadingTime) {
                m_line.materials[0].color = m_shotColor;
                m_line.widthMultiplier = m_shotLaserSize;
                RaycastHit hit;
                bool hasRaycastHit = Physics.Raycast(transform.position, transform.forward, out hit, 1000f, m_mask);
                if (hasRaycastHit) {
                    m_shutDown = true;
                    if(hit.transform.gameObject.TryGetComponent(out ConnexionMenuButtonBehavior script)) script.OnBeingActivated();
                    m_line.enabled = false;
                }
                
                StartCoroutine(ShotDownLaser(hit));
            }
            else if (m_isShooting) {
                m_line.enabled = true;
                m_elapsedTime += Time.deltaTime;
                float ratio = m_elapsedTime / m_laserLoadingTime;
                m_line.materials[0].color = new Color(((m_firstColor.r * (1-ratio)) + (m_lastColor.r * ratio)), ((m_firstColor.g * (1-ratio)) + (m_lastColor.g * ratio)), ((m_firstColor.b * (1-ratio)) + (m_lastColor.b * ratio)));

                m_line.widthMultiplier = m_initialLaserSize * (1 - ratio) + (m_endLaserSize * ratio);

                //Setting the right length for the laser aiming previsualization
                Vector3 forward = Synchronizer<SynchronizeSendVrRig>.Instance.m_rightHand.forward;
                Vector3 position = Synchronizer<SynchronizeSendVrRig>.Instance.m_rightHand.position;
                bool isHitting = Physics.Raycast(position, forward, out RaycastHit ray, Mathf.Infinity, m_mask);
                m_line.SetPosition(1, position + (forward * (isHitting ? ray.distance : 100000f)));
                m_line.SetPosition(0, position);
            }
            else if (OVRInput.Get (m_input) >= m_upTriggerValue) { // If the player press the trigger hard enough
                m_isShooting = true;
                m_elapsedTime = 0f;
            }
            
        }

        IEnumerator ShotDownLaser(RaycastHit p_hit) {
            yield return new WaitForSeconds(m_laserDuration);
            m_line.enabled = false;
                    
            if (MyNetworkManager.singleton.m_canSend) {
                MyNetworkManager.OnConnection += DestroyMyself;
            }
            else {
                m_elapsedTime = 0f;
                m_isShooting = false;
                m_shutDown = false;
            }
        }

        private void DestroyMyself() => Destroy(gameObject);
    }
}
