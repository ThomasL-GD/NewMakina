using System.Collections;
using CustomMessages;
using Mirror;
using Synchronizers;
using TMPro;
using UnityEngine;

public class SynchroniseReady : Synchronizer<SynchroniseReady>
{
    [SerializeField] private GameObject m_readyHeart;
    [SerializeField] private GameObject m_readyStuff;
    [SerializeField] private GameObject m_bowl;
    [SerializeField][Tooltip("in seconds")] private float m_practiceTime = 120f;
    [SerializeField] private TextMeshProUGUI m_practiceCountdown;
    
    private bool m_practice;
    private void Awake()
    {
        ClientManager.OnReceiveReadyToFace += ReceiveReady;
        ClientManager.OnReceiveInitiateLobby += ReceiveInitiateLobby;
    }

    private void ReceiveInitiateLobby(InitiateLobby p_initiateLobby)
    {
        m_readyStuff.SetActive(true);
        if (p_initiateLobby.trial)
        {
            StartCoroutine(TrialTimmer());
            return;
        }
        m_readyHeart.SetActive(true);
        m_bowl.SetActive(false);
    }

    private void ReceiveReady(ReadyToFace p_message) {
        m_readyStuff.SetActive(true);
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
        m_readyHeart.SetActive(true);
    }
    
    // Update is called once per frame
    public void StartReady()
    {
        Debug.Log("8");
        NetworkClient.Send(new ReadyToGoIntoTheBowl());
        m_readyHeart.SetActive(false);
        m_readyStuff.SetActive(false);
        m_bowl.SetActive(true);
    }
}
