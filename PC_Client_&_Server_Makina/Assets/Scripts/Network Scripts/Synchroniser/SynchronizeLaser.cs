using System;
using System.Threading;
using CustomMessages;
using UnityEngine;
using UnityEngine.Rendering;


namespace Synchronizers
{
    public class SynchronizeLaser : Synchronizer<SynchronizeLaser>
    {
        [SerializeField] private LineRenderer m_lazerPreshot;
        [SerializeField] private GameObject m_laserPrefab;

        [SerializeField] private Volume m_volume;
        [SerializeField] private Transform m_rightHand;
        [SerializeField] private Transform m_pcPlayer;
        [SerializeField] private float m_shotSensibleRange = 20f;
        [SerializeField, Tooltip("transition speed in weight per second")] private float m_smoothTransitionSpeed;

        private float m_targetIntensity;

        /// <summary/> Awake is called before Start
        private void Awake()
        {
            // base.Awake();
            ClientManager.OnReceiveLaserPreview += SynchroniseLaserPreshot;
            ClientManager.OnReceiveLaser += SynchroniseLaserPreshot;
            ClientManager.OnReceiveLaser += SynchroniseShot;
            ClientManager.OnReceiveInitialData += ReceiveInitialData;

            m_lazerPreshot.enabled = false;
        }

        private void ReceiveInitialData(InitialData pitialData) {
            m_lazerPreshot.enabled = false;
        }

        /// <summary/> This function checks wether the laser is aiming or shooting and changing it's state based on that
        /// <param name="p_laser"> The message sent by the server </param>
        private void SynchroniseLaserPreshot(Laser p_laser) => m_lazerPreshot.enabled = p_laser.laserState == LaserState.Aiming;


        private void Update()
        {
            float intensity = m_volume.weight;
            intensity = Mathf.MoveTowards(intensity, m_targetIntensity, m_smoothTransitionSpeed * Time.deltaTime);
            m_volume.weight = intensity;
            if (m_lazerPreshot.enabled)
            {
                m_volume.weight = 0;
                return;
            }
            Vector3 startingPoint = m_rightHand.position;
            Vector3 direction = m_rightHand.rotation * Vector3.forward;
            Vector3 playerPos = m_pcPlayer.position + Vector3.up ;
            
            if(Vector3.Dot(playerPos - startingPoint, direction) < 0) return;
            
            float distance = Vector3.Cross(direction, playerPos - startingPoint).magnitude;
            distance = Mathf.Clamp(distance,0, m_shotSensibleRange) / m_shotSensibleRange;

            m_targetIntensity = Mathf.Abs(distance - 1f);
        }
        
        /// <summary/> This function is called when the laser is shooting and instantiates the shot
        /// <param name="p_laser"> The message sent by the server </param>
        private void SynchroniseShot(Laser p_laser)
        {
            // Instantiating the shot
            Instantiate(m_laserPrefab,p_laser.origin,p_laser.rotation);
        }
    }
}
