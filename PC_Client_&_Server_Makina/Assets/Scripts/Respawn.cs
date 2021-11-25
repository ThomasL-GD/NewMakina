using CustomMessages;
using UnityEngine;

public class Respawn : MonoBehaviour
{
    
    // Start is called before the first frame update
    [SerializeField]private Transform[] m_spawnPoints;
    
    void Awake()
    {
        ClientManager.OnReceiveLaser += Die;
    }

    void Die(Laser p_laser)
    {
        if (!p_laser.hit) return;
        int index = Random.Range(0, m_spawnPoints.Length);
        transform.position = m_spawnPoints[index].position;
    }
}
