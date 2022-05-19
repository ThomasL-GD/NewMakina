using System;
using System.Collections.Generic;
using UnityEngine;
using CustomMessages;
using Network;
using Network.Connexion_Menu;
using Synchronizers;

public class HapticsManager : Synchronizer<HapticsManager> {

    [SerializeField] private LaserVFXHandler m_laserVFXHandler = null;
    
    [Header("Laser Charge (must last as long as the charging time")]
    [SerializeField] private AnimationCurve m_laserChargeStrength;
    [SerializeField] private AnimationCurve m_laserChargeFrequency;
    private HapticCommand m_chargeCommand = null;

    [Header("Laser Shot And Miss")]
    [SerializeField] private AnimationCurve m_laserShotAndMissStrength;
    [SerializeField] private AnimationCurve m_laserShotAndMissFrequency;
    
    [Header("Laser Shot And Kill")]
    [SerializeField] private AnimationCurve m_laserShotAndKillStrength;
    [SerializeField] private AnimationCurve m_laserShotAndKillFrequency;
    
    [Header("Elevator")]
    [SerializeField] private AnimationCurve m_elevatorStrength;
    [SerializeField] private AnimationCurve m_elevatorFrequency;
    
    [Header("Beacon")]
    [SerializeField] private AnimationCurve m_beaconActivationStrength;
    [SerializeField] private AnimationCurve m_beaconActivationFrequency;

#if UNITY_EDITOR
    [Header("Debug")]
    [SerializeField] private OVRInput.Button m_buttonToTest;
    [SerializeField] private AnimationCurve m_buttonStrength;
    [SerializeField] private AnimationCurve m_buttonFrequency;
#endif

    private class HapticCommand {
        public AnimationCurve strength;
        public AnimationCurve frequency;
        public float time = 0f;
        public OVRInput.Controller controllersAffected;
    }

    private List<HapticCommand> m_commandList = new List<HapticCommand>();
    /// <summary>For optimization of LateUpdate, is true when all haptics were turned of last frame</summary>
    private bool m_wasOffLastFrame = true;
    
    // Start is called before the first frame update
    void Start() {
        //MyNetworkManager.OnLaserShootingUpdate += NetworkShotHaptic;
        MyNetworkManager.OnLaserAimingUpdate += NetworkChargeHaptic;
        MyNetworkManager.OnReceiveBeaconDetectionUpdate += NetworkBeaconActivationHaptic;
        //LocalLaser.OnLocalShot += ShotHaptic;
        LocalLaser.OnLocalCharge += ChargeHaptic;
        ElevatorBehavior.OnElevatorLocalActivation += ElevatorHaptic;
        m_laserVFXHandler.m_deepImpact += ShotHaptic;
    }

    private void NetworkChargeHaptic(Laser p_laser) {
        ChargeHaptic(p_laser.laserState == LaserState.CancelAiming, MyNetworkManager.singleton.gameObject);
    }

    private void ChargeHaptic(bool p_isCancelingCharge, GameObject p_source) {
        if (p_isCancelingCharge) { //If the charging is canceled
            m_commandList.Remove(m_chargeCommand);
            m_chargeCommand = null;
        }
        else if (m_chargeCommand == null) { //If the charging is started
            m_chargeCommand = new HapticCommand() {controllersAffected = OVRInput.Controller.RTouch, frequency = m_laserChargeFrequency, strength = m_laserChargeStrength, time = 0f};
            m_commandList.Add(m_chargeCommand);
        }
    }

    /// <summary>Add a command of beacon detection haptic from network</summary>
    private void NetworkBeaconActivationHaptic(BeaconDetectionUpdate p_beaconDetection) {
        if (!p_beaconDetection.playerDetected) return;
        m_commandList.Add(new HapticCommand(){strength = m_beaconActivationStrength, frequency = m_beaconActivationFrequency, time = 0f, controllersAffected = OVRInput.Controller.RTouch | OVRInput.Controller.LTouch});
    }

    /// <summary>Will add an elevator trigger haptic command</summary>
    private void ElevatorHaptic(OVRInput.Controller p_handUsed) {
        m_commandList.Add(new HapticCommand(){strength = m_elevatorStrength, frequency = m_elevatorFrequency, time = 0f, controllersAffected = p_handUsed});
    }

    /// <summary>Will add a shot haptic command</summary>
    /// <param name="p_hit">true if the shot kills and false if it missed</param>
    private void ShotHaptic(bool p_hit) {
        m_commandList.Add(p_hit  ?  new HapticCommand(){strength = m_laserShotAndKillStrength, frequency = m_laserShotAndKillFrequency, time = 0f, controllersAffected = OVRInput.Controller.RTouch}  :  new HapticCommand(){strength = m_laserShotAndMissStrength, frequency = m_laserShotAndMissFrequency, time = 0f, controllersAffected = OVRInput.Controller.RTouch});
    }

    /// <summary>Add a command of laser shot from network</summary>
    /// <param name="p_laser">The message sent by the server<br/>We only use the hit boolean</param>
    private void NetworkShotHaptic(Laser p_laser) => ShotHaptic(p_laser.hit);

#if UNITY_EDITOR
    private void Update() {
        if (OVRInput.GetDown(m_buttonToTest)) {
            m_commandList.Add(new HapticCommand(){controllersAffected = OVRInput.Controller.RTouch | OVRInput.Controller.LTouch, time = 0f, frequency = m_buttonFrequency, strength = m_buttonStrength});
        }
    }
#endif

    private void LateUpdate() {
        if (m_commandList.Count < 1) {
            
            if (m_wasOffLastFrame) return;
            
            OVRInput.SetControllerVibration(0f, 0f, OVRInput.Controller.All);
            m_wasOffLastFrame = true;
            return;
        }

        m_wasOffLastFrame = false;

        List<byte> indexesToRemove = new List<byte>();
        float leftBiggestStrengthSoFar = 0f;
        float rightBiggestStrengthSoFar = 0f;
        byte leftBiggestStrengthSoFarIndex = 0;
        byte rightBiggestStrengthSoFarIndex = 0;
        for (byte i = 0; i < m_commandList.Count; i++) {
            
            HapticCommand haptic = m_commandList[i];
            float strength = haptic.strength.Evaluate(haptic.time);
            
            if (strength < 0.0001f) { // if a haptic is close enough to 0 strength, we consider it done
                indexesToRemove.Add(i);
            }
            
            //We are trying to find the strongest haptic and remember it
            if (haptic.controllersAffected.HasFlag(OVRInput.Controller.LTouch) && strength > leftBiggestStrengthSoFar) {
                leftBiggestStrengthSoFar = strength;
                leftBiggestStrengthSoFarIndex = i;
            }
            if (haptic.controllersAffected.HasFlag(OVRInput.Controller.RTouch) && strength > rightBiggestStrengthSoFar) {
                rightBiggestStrengthSoFar = strength;
                rightBiggestStrengthSoFarIndex = i;
            }

            haptic.time += Time.deltaTime;
        }
        
        OVRInput.SetControllerVibration(m_commandList[leftBiggestStrengthSoFarIndex].frequency.Evaluate(m_commandList[leftBiggestStrengthSoFarIndex].time), m_commandList[leftBiggestStrengthSoFarIndex].strength.Evaluate(m_commandList[leftBiggestStrengthSoFarIndex].time), OVRInput.Controller.LTouch);
        OVRInput.SetControllerVibration(m_commandList[rightBiggestStrengthSoFarIndex].frequency.Evaluate(m_commandList[rightBiggestStrengthSoFarIndex].time), m_commandList[rightBiggestStrengthSoFarIndex].strength.Evaluate(m_commandList[rightBiggestStrengthSoFarIndex].time), OVRInput.Controller.RTouch);
        
        if(indexesToRemove.Count < 1) return; //Then, we remove from the list every terminated hapticCommand

        for (int i = indexesToRemove.Count - 1; i >= 0; i--) { // We go through the list backwards so we can remove the biggest indexes first thus avoiding index shifting issues
            if (m_commandList[indexesToRemove[i]] == m_chargeCommand) m_chargeCommand = null;
            m_commandList.RemoveAt(indexesToRemove[i]);
        }
    }
}
