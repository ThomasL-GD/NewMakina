using System;
using System.Collections;
using Synchronizers;
using UnityEngine;

public class Transition : Synchronizer<Transition> {

    [SerializeField] private MeshRenderer m_rendererThatHaveShader = null;
    [SerializeField, Range(0.1f, 10f)] private float m_transitionDuration = 1f;
    private bool m_isTransitionning = false;

    /// <summary>Is called once a transition is halfway done (thus black screen).<br/>Will be emptied after use.</summary>
    public static Action a_transitionDone;
    private static readonly int Filling = Shader.PropertyToID("_Filling");


    // Start is called before the first frame update
    void Start() {
        if (m_rendererThatHaveShader == null) Debug.LogError("Guess I'll stay cis if you don't want to serialize my transition...", this);
    }

    [ContextMenu("Transition!")]
    public void StartTransition() {
        if (!m_isTransitionning) StartCoroutine(TransitionLoop());
    }

    private IEnumerator TransitionLoop() {

        m_isTransitionning = true;
        float elapsedTime = 0f;
        m_rendererThatHaveShader.sharedMaterial.SetFloat(Filling, 1f);

        while (elapsedTime < (m_transitionDuration / 2f)) { // fade to black
            yield return null;
            elapsedTime += Time.deltaTime;
            
            m_rendererThatHaveShader.sharedMaterial.SetFloat(Filling, 1f-(elapsedTime / (m_transitionDuration / 2f)));
        }
        
        m_rendererThatHaveShader.sharedMaterial.SetFloat(Filling, 0f);
        a_transitionDone?.Invoke();
        a_transitionDone = null;
        elapsedTime = 0f;
        
        while (elapsedTime < (m_transitionDuration / 2f)) { //fade to transparent
            yield return null;
            elapsedTime += Time.deltaTime;
            
            m_rendererThatHaveShader.sharedMaterial.SetFloat(Filling, elapsedTime / (m_transitionDuration / 2f));
        }
        
        m_isTransitionning = false;
        m_rendererThatHaveShader.sharedMaterial.SetFloat(Filling, 1f);
    }
}
