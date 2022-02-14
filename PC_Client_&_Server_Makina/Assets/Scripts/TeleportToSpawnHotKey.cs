using Synchronizers;
using UnityEngine;

public class TeleportToSpawnHotKey : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            transform.position = SynchronizeRespawn.Instance.m_spawnPoints[Random.Range(0, SynchronizeRespawn.Instance.m_spawnPoints.Length)].position;
        }
    }
}
