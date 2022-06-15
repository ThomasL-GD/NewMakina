using System;
using System.Collections;
using CustomMessages;
using Synchronizers;
using UnityEngine;
using UnityEngine.Serialization;

public class SoundManager : Synchronizer<SoundManager> {

    [Serializable] private struct SoundOptions {
        public AudioSource audioSource;
        [Tooltip("If false, will cut the previous sound of this type before playing a new one")] public bool canBePlayedSuperposed;
        private bool m_isPlaying;

        [HideInInspector] public readonly bool isPlaying => audioSource != null && audioSource.isPlaying;

        public void Play() {
            if (audioSource == null) return;
            if (!canBePlayedSuperposed) audioSource.Stop();
            audioSource.Play();
        }

        public void Stop() {
            if (audioSource == null) return;
            audioSource.Stop();
        }
    }
    
    private bool m_isBreakingAHeart = false;
    private bool m_isInvisible = false;
    private bool m_isInVrAim = false;

    [SerializeField] private float m_loadVolumeAugmentation = 0.1f;
    [Space]
    [SerializeField] private SoundOptions m_inVrLook;
    [SerializeField] private SoundOptions m_inVrAim;
    [SerializeField] private SoundOptions m_staysInvisible;
    [SerializeField] private SoundOptions m_isInBeacon;
    [SerializeField] private SoundOptions m_triggerBeacon;
    [SerializeField] private SoundOptions m_death;
    [SerializeField] private SoundOptions m_respawn;
    [SerializeField] private SoundOptions m_throwADecoy;
    [SerializeField] private SoundOptions m_decoyDeath;
    [SerializeField] private SoundOptions m_teleport;
    [SerializeField] private SoundOptions m_placeTpPoint;
    [SerializeField] private SoundOptions m_reloadAbility;
    [SerializeField] private SoundOptions m_heartBreak;
    [SerializeField] private SoundOptions m_isBreakingHeart;

    // Start is called before the first frame update
    void Start() {
        ClientManager.OnReceiveDestroyLeure += DestroyLeure;
        ClientManager.OnReceiveHeartBreak += HeartBreakSound;
        ClientManager.OnReceiveInvisibility += InvisibilitySounds;
        ClientManager.OnReceiveHeartConquerStart += HeartBreakingSounds;
        ClientManager.OnReceiveHeartConquerStop += HeartBreakingSoundsStop;
    }

    #region HeartBreak
    private void HeartBreakingSoundsStop(HeartConquerStop p_heartconquerstop) {
        m_isBreakingAHeart = false;
    }

    private void HeartBreakingSounds(HeartConquerStart p_heartconquerstart) {
        m_isBreakingAHeart = true;
        StartCoroutine(IsBreakingHeart());
    }

    IEnumerator IsBreakingHeart() {
        m_isBreakingHeart.Play();
        while (m_isBreakingAHeart) {
            yield return null;
        }
        m_isBreakingHeart.Stop();
    }

    private void HeartBreakSound(HeartBreak p_heartBreak) {
        m_heartBreak.Play();
    }
    #endregion
    
    #region Invisibility
    private void InvisibilitySounds(PcInvisibility p_pcInvisibility) {
        if (p_pcInvisibility.isInvisible) {
            if (m_isInvisible) return;
            m_isInvisible = true;
            StartCoroutine(StayInvisible());
        }
        else {
            m_isInvisible = false;
        }
    }

    IEnumerator StayInvisible() {
        m_staysInvisible.Play();
        while (m_isInvisible) {
            yield return null;
        }
        m_staysInvisible.Stop();
    }
    #endregion

    private void DestroyLeure(DestroyLeure p_activateBlind) => m_decoyDeath.Play();
    
    #region VR Aim
    public void VrAim(bool p_isInAim) {
        if (p_isInAim) {
            if (m_isInVrAim) return;
            m_isInVrAim = true;
            StartCoroutine(IsInVrAim());
        }
        else {
            m_isInVrAim = false;
        }
    }

    IEnumerator IsInVrAim() {
        m_inVrAim.Play();
        while (m_isInVrAim) {
            yield return null;
        }
        m_inVrAim.Stop();
    }
    #endregion
    
    public void VrLookSound(bool p_isInVrLook) {
        if (p_isInVrLook) {
            if(!m_inVrLook.isPlaying) m_inVrLook.Play();
        }else {
            if(m_inVrLook.isPlaying) m_inVrLook.Stop();
        }
    }
}