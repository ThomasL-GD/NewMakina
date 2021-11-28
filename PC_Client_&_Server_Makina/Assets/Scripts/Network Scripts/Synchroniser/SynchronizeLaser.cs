using CustomMessages;
using UnityEngine;


namespace Synchronizers
{
    public class SynchronizeLaser : Synchronizer
    {
        [SerializeField] private LineRenderer m_lazerPreshot;

        [SerializeField] private GameObject m_laserPrefab;

        /// <summary/> Awake is called before Start
        private void Awake()
        {
            // base.Awake();
            ClientManager.OnReceiveLaserPreview += SynchroniseLaserPreshot;
            ClientManager.OnReceiveLaser += SynchroniseLaserPreshot;
            ClientManager.OnReceiveLaser += SynchroniseShot;

            m_lazerPreshot.enabled = false;
        }

        /// <summary/> This function checks wether the laser is aiming or shooting and changing it's state based on that
        /// <param name="p_laser"> The message sent by the server </param>
        private void SynchroniseLaserPreshot(Laser p_laser) => m_lazerPreshot.enabled = p_laser.laserState == LaserState.Aiming;
        

        /// <summary/> This function is called when the laser is shooting and instantiates the shot
        /// <param name="p_laser"> The message sent by the server </param>
        private void SynchroniseShot(Laser p_laser)
        {
            // Instantiating the shot
            GameObject instantiate = Instantiate(m_laserPrefab,p_laser.origin,p_laser.rotation);

            // Setting the instances position and rotation
            // instantiate.transform.position = p_laser.origin;
            // instantiate.transform.rotation = p_laser.rotation;
        }
    }
}
