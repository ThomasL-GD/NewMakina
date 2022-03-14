using System;
using Player_Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class TutorialPrompt : MonoBehaviour
{
    [SerializeField,Tooltip("The amount of time it will take for the postprocess to fully apply (seconds)")] private float m_postProcessingInTime = .5f;
    [SerializeField,Tooltip("The amount of time it will take for the postprocess to fully disappear (seconds)")] private float m_postProcessingOutTime = .2f;
    [SerializeField,Tooltip("The postprocess volume to be applied when the player enters the prompt mode")] private Volume m_postProcess;
    [SerializeField,Tooltip("The text to appear when the player enters the prompt mode")] private TextMeshProUGUI m_text;
    
    /// <summary/> the target postprocessing weight of the element
    private float m_targetWeight;
    
    /// <summary/> the speed at which the post process volume's weight will go upwards
    private float m_weightTransitionSpeedIn;
    
    /// <summary/> the speed at which the the post process volume's weight will go downwards
    private float m_weightTransitionSpeedOut;

    /// <summary/> is the prompt currently fading in
    private bool m_fadeIn = false;
    
    /// <summary/> is the prompt currently fading out
    private bool m_fadeOut = false;

    private KeyCode? m_promptEndKey;

    
    public delegate void PromptDelegator(String p_promptText,KeyCode? p_promptEndKey = null);
    public static PromptDelegator OnPrompt;
    
    private void Awake()
    {
        OnPrompt += StartPrompt;
    }

    /// <summary/> Start is called before the first frame update
    private void Start()
    {
        m_targetWeight = m_postProcess.weight;
        m_postProcess.weight = 0f;
        m_weightTransitionSpeedIn = m_targetWeight / m_postProcessingInTime;
        m_weightTransitionSpeedOut = m_targetWeight / m_postProcessingOutTime;
        m_text.alpha = 0;
        m_postProcess.enabled = true;
    }

    /// <summary/> starts the prompt
    /// <param name="p_promptEndKey"> the key that the player has to press to end the prompt </param>
    /// <param name="p_promptText"> the text that will show up when the player presses the prompt </param>
    void StartPrompt(String p_promptText,KeyCode? p_promptEndKey = null)
    {
        m_promptEndKey = p_promptEndKey;
        m_text.text = p_promptText;
        InputMovement3.cancelInput = true;
        m_fadeIn = true;
    }
    
    /// <summary/> stops the prompt
    void StopPrompt()
    {
        InputMovement3.cancelInput = false;
        m_fadeOut = true;
        m_promptEndKey = null;
        m_fadeIn = false;
    }

    /// <summary/> Update is called once per frame
    void Update()
    {
        if (m_promptEndKey != null){ if (Input.GetKeyDown((KeyCode) m_promptEndKey)) StopPrompt(); }
        else if (Input.anyKeyDown) StopPrompt();

        // while the prompt is fading in
        if (m_fadeIn)
        {
            // update the text's opacity to rise from 0 to 1
            m_text.alpha += 1 / m_postProcessingInTime * Time.deltaTime;
            m_text.alpha = Mathf.Clamp(m_text.alpha, 0, 1);

            // update the post process's weight to rise from 0 to the target weight 
            m_postProcess.weight = Mathf.Min(m_targetWeight,m_postProcess.weight + m_weightTransitionSpeedIn * Time.deltaTime);
            
            // Resetting the fade in state 
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (m_postProcess.weight == m_targetWeight) m_fadeIn = false; 
        }

        if (m_fadeOut)
        {
            // update the text's opacity to lower from 1 to 0
            m_text.alpha -= (1 / m_postProcessingOutTime) * Time.deltaTime;
            m_text.alpha = Mathf.Clamp(m_text.alpha, 0, 1);
            
            // update the post process's weight to lower from the target weight to 0
            m_postProcess.weight = Mathf.Max(0f,m_postProcess.weight - m_weightTransitionSpeedOut * Time.deltaTime);
            
            // Resetting the fade in state 
            if (m_postProcess.weight == 0f) m_fadeOut = false;
        }
    }
}
