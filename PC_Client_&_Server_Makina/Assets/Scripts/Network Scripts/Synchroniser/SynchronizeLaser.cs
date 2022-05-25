using System;
using System.Threading;
using CustomMessages;
using UnityEngine;
using UnityEngine.Rendering;


namespace Synchronizers {
    public class SynchronizeLaser : Synchronizer<SynchronizeLaser> {

        [SerializeField] private LaserVFXHandler m_laserVFXHandler;
        [SerializeField] private Volume m_volume;
        [SerializeField] private Transform m_rightHand;
        [SerializeField] private Transform m_pcPlayer;
        [SerializeField] private float m_shotSensibleRange = 5f;
        [SerializeField] private float m_supposedChargeTime = 1.5f; //TODO : That is so trash, move that to initial data for the love of zombie jesus
        [SerializeField, Tooltip("transition speed in weight per second")] private float m_smoothTransitionSpeed;

        private float m_targetIntensity;
        private float m_elapsedChargingTime = 0f;
        private float m_lastTimeLaserReceived = 0f;

        /// <summary/> Awake is called before Start
        private void Awake() {
            // base.Awake();
            ClientManager.OnReceiveLaserPreview += SynchroniseLaserPreshot;
            ClientManager.OnReceiveLaser += SynchroniseShot;
            ClientManager.OnReceiveInitialData += ReceiveInitialData;
        }

        private void ReceiveInitialData(InitialData p_initialData) {
            
        }

        /// <summary/> This function checks wether the laser is aiming or shooting and changing it's state based on that
        /// <param name="p_laser"> The message sent by the server </param>
        private void SynchroniseLaserPreshot(Laser p_laser) {
            switch (p_laser.laserState) {
                case LaserState.Aiming: {
                    if (m_elapsedChargingTime == 0f) m_lastTimeLaserReceived = Time.time;
                    m_elapsedChargingTime += Time.time - m_lastTimeLaserReceived;
                    m_laserVFXHandler.m_delegatedAction?.Invoke(new Laser(){laserState = LaserState.Aiming}, m_elapsedChargingTime / m_supposedChargeTime);
                    break;
                }
                case LaserState.CancelAiming when m_elapsedChargingTime != 0f:
                    m_laserVFXHandler.m_delegatedAction?.Invoke(new Laser(){laserState = LaserState.CancelAiming}, m_elapsedChargingTime / m_supposedChargeTime);
                    m_elapsedChargingTime = 0f;
                    break;
                case LaserState.Shooting:
                default:
                    Debug.LogWarning("How ?! How could I receive a laserAim delegate with a shooting laserState ?!", this);
                    break;
            }
        }


        private void Update() {
            
            float intensity = m_volume.weight;
            intensity = Mathf.MoveTowards(intensity, m_targetIntensity, m_smoothTransitionSpeed * Time.deltaTime);
            m_volume.weight = intensity;
            if (m_elapsedChargingTime == 0f) {
                m_volume.weight = 0f;
                m_targetIntensity = 0f;
                return;
            }
            Vector3 startingPoint = m_rightHand.position;
            Vector3 direction = (m_rightHand.rotation * Vector3.forward).normalized;
            Vector3 playerPos = m_pcPlayer.position + Vector3.up ;
            
            if(Vector3.Dot((playerPos - startingPoint).normalized, direction) < 0f) return;
            
            float distance = Vector3.Cross(direction, playerPos - startingPoint).magnitude;
            distance = Mathf.Clamp(distance, 0f,m_shotSensibleRange) / m_shotSensibleRange;

            m_targetIntensity = Mathf.Max(1f - distance,0f);
        }
        
        /// <summary/> This function is called when the laser is shooting and gives the message to the VFX
        /// <param name="p_laser"> The message sent by the server </param>
        private void SynchroniseShot(Laser p_laser) {
            m_elapsedChargingTime = 0f;
            m_laserVFXHandler.m_delegatedAction?.Invoke(p_laser, 0f);
        }
    }
}
