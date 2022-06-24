using System;
using System.Collections;
using CustomMessages;
using Mirror;
using Synchronizers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadStuffOnClick : MonoBehaviour
{
    [SerializeField] private GameObject[] m_toEnable;
    [SerializeField] private GameObject m_camera;
    [SerializeField] private float m_fadeTime = 2f;
    [SerializeField] private RawImage m_blackScreen;

    [SerializeField] private AnimationCurve m_curveIn;
    [SerializeField] private AnimationCurve m_curveOut;

    [SerializeField] private bool m_practice;

    [SerializeField] private GameObject m_spotLight;
    [SerializeField] private GameObject m_prompt;

    [SerializeField] private TextMeshPro m_text;
    
    private static bool clicked = false;
    private bool hasClicked = false;
    private bool dontUse = false;
    private bool temp = false;

    private void Awake()
    {
        ClientManager.OnReceiveInitiateLobby += StartFade;
        ClientManager.OnReceiveReadyToFace += StartConnection;
    }

    private void StartConnection(ReadyToFace p_readytoface)
    {
        if(clicked && hasClicked && !dontUse)
        {
            NetworkClient.Send(new InitiateLobby() {trial = m_practice});
            NetworkClient.Send(new ReadyToFace());
        }
    }


    private void OnMouseOver()
    {
        if (clicked) return;
        m_spotLight.SetActive(true);
    }
    

    private void OnMouseExit()
    {
        if (clicked) return;
        m_spotLight.SetActive(false);
    } 
    

    // Update is called once per frame
    void OnMouseDown()
    {
        if (!temp && !m_practice)
        {
            m_prompt.SetActive(true);
            temp = true;
            return;
        }

        if (clicked) return;

        m_text.text = "waiting for VR player";
        
        clicked = true;
        hasClicked = true;
        
        if (!SynchronizeInitialData.vrConnected) return;

        dontUse = true;
        
        NetworkClient.Send(new InitiateLobby(){trial = m_practice});
        NetworkClient.Send(new ReadyToFace());
    }

    private void StartFade(InitiateLobby p_initiateLobby)
    {
        Debug.Log("ready to face !");
        StartCoroutine(FadeToBlack());
    }
    

    IEnumerator FadeToBlack()
    {
        m_text.text = "";
        Color color = m_blackScreen.color;
        float timer = 0f;
        do
        {
            timer += Time.deltaTime / m_fadeTime;
            timer = Mathf.Min(1f, timer);

            color.a = m_curveIn.Evaluate(timer);
            m_blackScreen.color = color;
            yield return null;
        } while (color.a != 1f);
        
        Debug.Log("fade to black button");
        
        foreach (GameObject go in m_toEnable) go.SetActive(true);
        
        m_camera.SetActive(false);

        do
        {
            timer -= Time.deltaTime / m_fadeTime;
            timer = Mathf.Max(0f, timer);

            color.a = m_curveOut.Evaluate(timer);
            
            m_blackScreen.color = color;
            yield return null;
        } while (color.a != 0f);
        
        clicked = false;
    }
}
