using System;
using UnityEngine;
using Random = UnityEngine.Random;


public class Teleport : MonoBehaviour {

    [SerializeField] [Range(0f,10f)] [Tooltip("The minimal distance between the player and this object have to be for the teleportation to start")] private float m_rangeOfEntry = 1f;
    [SerializeField] [Tooltip("The possible exits of the teleportation, will pick randomly between all")] private Transform[] m_exits = null;

    // Update is called once per frame
    void Update() {
        if(Vector3.Distance(ClientManager.singleton.m_player.transform.position, transform.position) > m_rangeOfEntry) return;

        ClientManager.singleton.m_player.transform.position = m_exits[Random.Range(0,m_exits.Length)].position;
    }

    private void OnDrawGizmos() {
        Gizmos.DrawWireSphere(transform.position, m_rangeOfEntry);
    }
}
