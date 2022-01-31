using System;
using System.Collections.Generic;
using UnityEngine;

namespace Player_Scripts.Reloading {

    public class CollectibleReload : ReloadingAbstract {

        [SerializeField] [Tooltip("The position of each collectible")] private Transform[] m_collectiblesTransforms;
        [SerializeField] [Range(0.1f, 5f)] [Tooltip("The radius of every collectible")] private float m_collectibleRadius = 1f;
        [SerializeField] [Tooltip("If true, will show a wireframe sphere with the gizmos to show the radius of each beacon\nIf false, show nothing")] private bool m_shouldShowRadius = true;
        [SerializeField] [Tooltip("If true, will disable the ameObject of the collectible eaten\nIf false, will let them be")] private bool m_shouldDisableGameObjects = true;

        [Space] [SerializeField] [Tooltip("The reloading method for the collectibles to appear again once they are eaten\nIf this is empty, they will never respawn")] private ReloadingAbstract m_respawnCollectibles = null;
        
        [Serializable] private class Collectible {
            public Transform collectibleTransform;
            public bool isAvailable;
            
            public Collectible(Transform p_collectibleTransform) {
                collectibleTransform = p_collectibleTransform;
                isAvailable = true;
            }
        }

        /// <summary>The position and availability of every collectible for this reload type</summary>
        /// <remarks>I know it's weird for this to be a list but when we'll have to add and remove collectibles in real time, it'll be the way</remarks>
        private List<Collectible> m_collectibles = new List<Collectible>();


        private void Start() {
            if (m_respawnCollectibles != null) {
                m_respawnCollectibles.OnReloading += RespawnAll;
            }

            foreach (Transform tran in m_collectiblesTransforms) {
                m_collectibles.Add(new Collectible(tran));
            }
        }

        private void FixedUpdate() {
            foreach (Collectible col in m_collectibles) {
                if(col.isAvailable) {
                    if (Vector3.Distance(col.collectibleTransform.position, ClientManager.singleton.m_playerObject.transform.position) < m_collectibleRadius) {
                        OnReloading?.Invoke();
                        col.isAvailable = false;
                        if(m_shouldDisableGameObjects) col.collectibleTransform.gameObject.SetActive(false);
                        break;
                    }
                }
            }
        }

        /// <summary>Will respawn every collectible's GameObject if they've been disabled and will make them available again</summary>
        private void RespawnAll() {
            foreach (Collectible col in m_collectibles) {
                if (col.isAvailable) continue;
                
                col.isAvailable = true;
                if(m_shouldDisableGameObjects) col.collectibleTransform.gameObject.SetActive(true);
            }
        }

        private void OnDrawGizmos() {
            if (!m_shouldShowRadius) return;
            
            if (Application.isEditor) {
                foreach (Transform t in m_collectiblesTransforms) {
                    Gizmos.DrawWireSphere(t.position, m_collectibleRadius);
                }
            }
            else if (Application.isPlaying) {
                foreach (Collectible col in m_collectibles) {
                    if(col.isAvailable)
                        Gizmos.DrawWireSphere(col.collectibleTransform.position, m_collectibleRadius);
                }
            }
        }
    }

}