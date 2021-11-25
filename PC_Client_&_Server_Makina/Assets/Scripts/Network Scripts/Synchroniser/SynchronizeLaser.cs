using CustomMessages;
using UnityEngine;


namespace Synchronizers
{
    public class SynchronizeLaser : Synchronizer
    {
        [SerializeField] private LineRenderer m_lazerPreshot;

        [SerializeField] private GameObject m_laserPrefab;

        // Start is called before the first frame update
        private /*new*/ void Awake()
        {
            // base.Awake();
            ClientManager.OnReceiveLaserPreview += SynchroniseLaserPreshot;
            ClientManager.OnReceiveLaser += SynchroniseLaserPreshot;
            ClientManager.OnReceiveLaser += SynchroniseShot;

            m_lazerPreshot.enabled = false;
        }

        private void SynchroniseLaserPreshot(Laser p_laser) => m_lazerPreshot.enabled = p_laser.laserState == LaserState.Aiming;
        

        private void SynchroniseShot(Laser p_laser)
        {

            GameObject instantiate = Instantiate(m_laserPrefab);

            instantiate.transform.position = p_laser.origin;
            instantiate.transform.rotation = p_laser.rotation;
        }
    }
}
