using System.Collections;
using UnityEngine;

namespace Player_Scripts.Reloading {

    public class CooldownReload : ReloadingAbstract {
        
        [SerializeField] [Range(0.1f, 120f)] [Tooltip("The time taken to reload\nUnit : seconds")] private float m_cooldownTime;
        private Coroutine m_cooldownCoroutine;

        public override void StartReloading() {
            base.StartReloading();

            m_cooldownCoroutine = StartCoroutine(Cooldown());
        }

        IEnumerator Cooldown() {
            yield return new WaitForSeconds(m_cooldownTime);
            OnReloading?.Invoke();
        }
    }

}
