using System;
using System.Collections;
using CustomMessages;
using UnityEngine;
using UnityEngine.VFX;

public class LaserVFXHandler : MonoBehaviour
{
    //------------------------- PUBLIC VARIABLES -------------------------//
    public Action<Laser, float> m_delegatedAction;
    
    //------------------------- PRIVATE VARIABLES -------------------------//
    [SerializeField] private VisualEffect m_laserTrail;
    [SerializeField] private VisualEffect m_laserBall;
    [SerializeField] private  string m_start = "Ss";
    [SerializeField] private string m_stop = "Stop";
    [SerializeField] private string m_chargeLevel = "ChargeLevel";
    [SerializeField] private float m_ballSpeed = 200f;
    [SerializeField] private float m_fadeOutSpeed = 4f;
    [SerializeField] private Transform m_parent = null;
    [SerializeField] private VisualEffect m_explosionEffect;
    [SerializeField] private VisualEffect m_killEffect;
    
    private Coroutine m_yeetRoutine;
    
    //------------------------- METHODS -------------------------//
    private void OnEnable() => m_delegatedAction += HandleLaserVFX;
    
    private void OnDisable() => m_delegatedAction -= HandleLaserVFX;

    [ContextMenu("test")]
    void testFunction() => StartCoroutine(TestCoco());

    IEnumerator TestCoco()
    {
        m_delegatedAction -= HandleLaserVFX;
        m_delegatedAction += HandleLaserVFX;
        float waitTime = 6f;
        float timer = 0f;
        
        while (timer < waitTime)
        {
            timer += Time.deltaTime;
            m_delegatedAction(new Laser(){laserState = LaserState.Aiming}, timer / waitTime);
            yield return null;
        }
        
        m_delegatedAction(new Laser(){laserState = LaserState.Shooting, hitPosition = new Vector3(-0.0500000007f,77.3000031f,241f), hit = true}, timer / waitTime);
    }
    
    /// <summary/> Method to handle the VFX for the laser
    /// <param name="p_laser"> The laser sent by the player</param>
    /// <param name="p_chargedLvl"> The charged amount of the laser from 0 to 1</param>
    private void HandleLaserVFX(Laser p_laser, float p_chargedLvl)
    {
        switch (p_laser.laserState)
        {
            case LaserState.Aiming:
                SetParent(m_parent);
                SetFloatInVFXs(m_chargeLevel, p_chargedLvl);
                SendEventInVFXs(m_start);
                break;
            case LaserState.Shooting:
                SetParent(null);
                m_laserTrail.SendEvent(m_stop);
                SetFloatInVFXs(m_chargeLevel, 1f);
                if(m_yeetRoutine != null) StopCoroutine(m_yeetRoutine);
                m_yeetRoutine = StartCoroutine(YeetBall(p_laser.hitPosition, p_laser.hit));
                break;
            case LaserState.CancelAiming:
                SetParent(null);
                StartCoroutine(InverseLerp(m_chargeLevel, p_chargedLvl));
                SendEventInVFXs(m_stop);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    void SetFloatInVFXs(String p_paramName, float p_value)
    {
        m_laserBall.SetFloat(p_paramName, p_value);
        m_laserTrail.SetFloat(p_paramName, p_value);
    }

    void SendEventInVFXs(String p_eventName)
    {
        m_laserBall.SendEvent(p_eventName);
        m_laserTrail.SendEvent(p_eventName);
    }

    void SetParent(Transform p_parent)
    {
        m_laserBall.transform.parent = p_parent;
        m_laserTrail.transform.parent = p_parent;
        
        if(p_parent == null) return;

        Transform transform1 = m_laserBall.transform;
        transform1.position = p_parent.position;
        transform1.rotation = p_parent.rotation;

        Transform transform2 = m_laserTrail.transform;
        transform2.position = p_parent.position;
        transform2.rotation = p_parent.rotation;
    }
    
    IEnumerator YeetBall(Vector3 p_hitPosition, bool p_hit)
    {
        Vector3 initalPosition = m_laserBall.transform.position;
        while (m_laserBall.transform.position != p_hitPosition)
        {
            m_laserBall.transform.position = Vector3.MoveTowards(m_laserBall.transform.position, p_hitPosition, m_ballSpeed * Time.deltaTime);
            yield return null;
        }

        m_explosionEffect.transform.position = p_hitPosition;
        m_explosionEffect.SendEvent("Explode");

        
        m_laserBall.transform.position = initalPosition;
        SetFloatInVFXs(m_chargeLevel, 0f);
        SendEventInVFXs(m_stop);
        if (!p_hit) yield break;
        m_killEffect.transform.position = p_hitPosition;
        StartCoroutine(KillEffect());
    }

    IEnumerator KillEffect()
    {
        m_killEffect.SendEvent("Start");
        
        float timer = 0f;
        while (timer<1f)
        {
            m_killEffect.SetVector3("Hand", m_parent.position);
            yield return null;
            timer += Time.deltaTime;
        }
        
        m_killEffect.SendEvent("Stop");
    }
    
    IEnumerator InverseLerp(String p_paramName, float p_initialValue)
    {
        float value = p_initialValue;
        while (value != 0f)
        {
            yield return null;
            value -= Time.deltaTime * m_fadeOutSpeed;
            value = Mathf.Max(value, 0f);
            SetFloatInVFXs(p_paramName, value);
        }
    }
}
