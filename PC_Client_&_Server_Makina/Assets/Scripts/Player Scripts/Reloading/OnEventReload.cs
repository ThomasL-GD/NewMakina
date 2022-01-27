using System;
using CustomMessages;
using UnityEngine;

namespace Player_Scripts.Reloading {

    public class OnEventReload : ReloadingAbstract {

        [SerializeField] [Tooltip("If true, this will reload when a heart is destroyed")] private bool m_reloadOnHeartDestroyed = false;
        [SerializeField] [Tooltip("If true, this will reload when the player dies")] private bool m_reloadOnDeath = false;

        private void Start() {
            if(m_reloadOnDeath) {
                ClientManager.OnReceiveLaser += ReceiveLaser;
                ClientManager.OnReceiveBombExplosion += ReceiveBomb;
            }

            if (m_reloadOnHeartDestroyed)
                ClientManager.OnReceiveHeartBreak += ReceiveHeartBreak;
        }

        private void ReceiveLaser(Laser p_laser) { if (p_laser.hit) Reload(); }
        private void ReceiveBomb(BombExplosion p_explosion) { if (p_explosion.hit) Reload(); }
        private void ReceiveHeartBreak(HeartBreak p_heartBreak) => Reload();

        private void Reload() {
            OnReloading?.Invoke();
        }
    }

}
