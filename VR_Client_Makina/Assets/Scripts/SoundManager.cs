using System;
using System.Collections;
using CustomMessages;
using Network;
using Synchronizers;
using UnityEngine;

public class SoundManager : Synchronizer<SoundManager> {

    [Serializable] private struct SoundOptions {
        public AudioSource audioSource;
        [Tooltip("If false, will cut the previous sound of this type before playing a new one")] public bool canBePlayedSuperposed;

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

    public static Action<LaserState, bool> a_laser;
    private bool m_isChargingLaser;

    [SerializeField] private float m_loadVolumeAugmentation = 0.1f;
    [Space]
    [SerializeField] private SoundOptions m_heartBreak;
    [SerializeField] private SoundOptions m_laserShot;
    [SerializeField] private SoundOptions m_laserLoad;
    [SerializeField] private SoundOptions m_kill;
    [SerializeField] private SoundOptions m_killDecoy;
    [SerializeField] private SoundOptions m_teleport;
    [SerializeField] private SoundOptions m_beaconGetsActive;
    [SerializeField] private SoundOptions m_beaconHitGround;

    // Start is called before the first frame update
    void Start() {
        MyNetworkManager.OnReceiveHeartBreak += HeartBreakSound;
        MyNetworkManager.OnReceiveDestroyLeure += DestroyLeureSound;
        MyNetworkManager.OnReceiveTeleported += TeleportSound;
    }
    
    private void OnEnable() => a_laser += LaserSounds;
    private void OnDisable() => a_laser -= LaserSounds;

    private void HeartBreakSound(HeartBreak p_heartBreak) {
        m_heartBreak.Play();
    }

    private void LaserSounds(LaserState p_laserState, bool p_hit) {
        switch (p_laserState) {
            
            case LaserState.Aiming:
                if (m_isChargingLaser) break;
                m_isChargingLaser = true;
                StartCoroutine(ChargeSound());
                break;
            case LaserState.Shooting:
                m_laserShot.Play();
                if(p_hit) m_kill.Play();
                break;
            case LaserState.CancelAiming:
                m_isChargingLaser = false;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    IEnumerator ChargeSound() {
        m_laserLoad.Play();
        float initialVolume = m_laserLoad.audioSource.volume;
        while (m_isChargingLaser) {
            yield return null;
            m_laserLoad.audioSource.volume += m_loadVolumeAugmentation * Time.deltaTime;
        }
        m_laserLoad.audioSource.volume = initialVolume;
        m_laserLoad.Stop();
    }

    private void DestroyLeureSound(DestroyLeure p_destroyLeure) {
        m_killDecoy.Play();
    }

    private void TeleportSound(Teleported p_teleported) {
        m_teleport.Play();
    }

    public void BeaconDetectSound() {
        m_beaconGetsActive.Play();
    }

    public void BeaconHitGround() {
        m_beaconHitGround.Play();
    }
}