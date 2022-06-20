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
    [SerializeField] private GameObject[] m_bowl;
    [SerializeField] private GameObject[] m_lobby;
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
            StartCoroutine(TrialTimmer(p_initiateLobby.trialTime));
            return;
        }
        m_readyHeart.SetActive(true);

        foreach (GameObject obj in m_bowl) obj.SetActive(false);
        foreach (GameObject obj in m_lobby) obj.SetActive(true);
    }

    private void ReceiveReady(ReadyToFace p_message) {
        m_readyStuff.SetActive(true);
    }
    
    IEnumerator TrialTimmer(float p_time)
    {
        float startTime = Time.time;
        
        while (Time.time - startTime < p_time)
        {
            Debug.Log("Hey");
            m_practiceCountdown.text = $"Practice ends in {Mathf.CeilToInt(p_time - (Time.time - startTime))} seconds";
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
        
        foreach (GameObject obj in m_bowl) obj.SetActive(true);
        foreach (GameObject obj in m_lobby) obj.SetActive(false);
    }
}
