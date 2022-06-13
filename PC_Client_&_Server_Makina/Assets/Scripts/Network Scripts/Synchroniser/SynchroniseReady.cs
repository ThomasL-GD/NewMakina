using System.Collections;
using CustomMessages;
using Mirror;
using Synchronizers;
using TMPro;
using UnityEngine;

public class SynchroniseReady : Synchronizer<SynchroniseReady>
{
    [SerializeField] private GameObject m_readyStuff;
    [SerializeField][Tooltip("in seconds")] private float m_practiceTime = 120f;
    [SerializeField] private TextMeshProUGUI m_practiceCountdown;
    
    private bool m_practice;
    private void Awake()
    {
        ClientManager.OnReceiveReadyToPlay += ReceiveReady;
        ClientManager.OnReceiveInitiateLobby += ReceiveInitiateLobby;
    }

    private void ReceiveInitiateLobby(InitiateLobby p_initiateLobby)
    {
        if (p_initiateLobby.practice)
        {
            StartCoroutine(TrialTimmer());
            return;
        }
        m_readyStuff.SetActive(true);
    }


    private void ReceiveReady(ReadyToPlay p_message) {
        //m_readyStuff.SetActive(true);
    }
    
    IEnumerator TrialTimmer()
    {
        float startTime = Time.time;
        
        while (Time.time - startTime < m_practiceTime)
        {
            Debug.Log("Hey");
            m_practiceCountdown.text = $"Practice ends in {Mathf.CeilToInt(m_practiceTime - (Time.time - startTime))} seconds";
            yield return null;
        }

        m_practiceCountdown.text = "";
        m_readyStuff.SetActive(true);
    }
    
    // Update is called once per frame
    public void StartReady()
    {
        NetworkClient.Send(new ReadyToPlay());
        m_readyStuff.SetActive(false);
    }
}
