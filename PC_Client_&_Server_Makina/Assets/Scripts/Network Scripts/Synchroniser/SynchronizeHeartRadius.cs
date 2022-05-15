using CustomMessages;
using Synchronizers;
using TMPro;
using UnityEngine;

public class SynchronizeHeartRadius : Synchronizer<SynchronizeHeartRadius>
{
    [SerializeField] private TextMeshProUGUI m_heartRadiusFeedback;
    [SerializeField] private AudioSource m_heartBreakingAudioSource;

    private float m_maxHeartTime;
    // Start is called before the first frame update
    void Awake()
    {
        m_heartRadiusFeedback.enabled = false;
        ClientManager.OnReceiveHeartConquerStop += ReceiveHeartConquerStop;
        ClientManager.OnReceiveHeartConquerStart += ReceiveHeartConquerStart;
        ClientManager.OnReceiveInitialData += ReceiveInitialData;
    }

    private void ReceiveInitialData(InitialData p_initialdata) => m_maxHeartTime = p_initialdata.heartConquerTime;
    

    private void ReceiveHeartConquerStop(HeartConquerStop p_heartconquerstop)
    {
        m_heartRadiusFeedback.enabled = false;
        m_heartRadiusFeedback.text = "";
        m_heartBreakingAudioSource.Stop();
        if(SynchronizeHearts.Instance.m_hearts.Length !=0)
            SynchronizeHearts.Instance.m_hearts[indexOflastTouchedHeart].GetComponent<HeartIdentifier>().StopAnticipation();
    }


    private int indexOflastTouchedHeart = 0;
    private void ReceiveHeartConquerStart(HeartConquerStart p_heartconquerstart)
    {
        if(m_heartRadiusFeedback.enabled == false) m_heartBreakingAudioSource.Play();
        indexOflastTouchedHeart = p_heartconquerstart.index;
        
        if(SynchronizeHearts.Instance.m_hearts.Length !=0)
            SynchronizeHearts.Instance.m_hearts[indexOflastTouchedHeart].GetComponent<HeartIdentifier>().StartAnticipation(m_maxHeartTime);
        
        m_heartRadiusFeedback.enabled = true;
        string timer = p_heartconquerstart.time.ToString();
        string text = "x,y";
        text = text.Replace('x',timer[0]);
        char car = timer.Length > 3 ? timer[2] : '0';
        text = text.Replace('y',car);
        

        m_heartRadiusFeedback.text = $"Breaking Heart : {text}/{m_maxHeartTime}";
    }
}
