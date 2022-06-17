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

    [SerializeField] private TextMeshPro m_text;
    
    private bool clicked = false;

    private void Awake() => ClientManager.OnReceiveInitiateLobby += StartFade;


    private void OnMouseOver()
    {
        if (!SynchronizeInitialData.vrConnected) return;
        m_spotLight.SetActive(true);
    }
    

    private void OnMouseExit()
    {
        if (!SynchronizeInitialData.vrConnected) return;
        m_spotLight.SetActive(false);
    } 
    

    // Update is called once per frame
    void OnMouseDown()
    {
        if (!SynchronizeInitialData.vrConnected) return;
        if (clicked) return;

        m_text.text = "";
        
        clicked = true;
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
