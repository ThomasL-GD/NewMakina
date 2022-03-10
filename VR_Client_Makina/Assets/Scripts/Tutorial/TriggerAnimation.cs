using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerAnimation : MonoBehaviour {

    private Animator m_animator;

    [SerializeField, Tooltip("Between 0 and 1 only plz")] private AnimationCurve m_triggerPosition;

    [SerializeField] private bool m_animateTrigger = true;
    [SerializeField] private bool m_animateGrip = false;

    private Coroutine m_coroutine;
    private bool m_mustRun = true;
    private static readonly int Trigger = Animator.StringToHash("Trigger");
    private static readonly int Grip = Animator.StringToHash("Grip");

    // Start is called before the first frame update
    void Start() {
        m_animator = GetComponent<Animator>();
        m_coroutine = StartCoroutine(LoopAnim());
    }

    IEnumerator LoopAnim() {
        float elapsedTime = 0f;
        float maxTime = m_triggerPosition.keys[m_triggerPosition.length - 1].time;
        while (m_mustRun) {
            yield return new WaitForFixedUpdate();
            elapsedTime += Time.fixedDeltaTime;
            if (elapsedTime > maxTime) elapsedTime -= maxTime; //Loop

            if (m_animateTrigger)m_animator.SetFloat(Trigger, m_triggerPosition.Evaluate(elapsedTime));
            if (m_animateGrip)m_animator.SetFloat(Grip, m_triggerPosition.Evaluate(elapsedTime));
        }
    }

    private void OnDestroy() {
        m_mustRun = false;
        StopCoroutine(m_coroutine);
    }
}
