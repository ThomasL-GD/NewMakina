using System.Collections;
using System.Collections.Generic;
using CustomMessages;
using Synchronizers;
using TMPro;
using UnityEngine;

public class HeartRadiusSynchronizer : Synchronizer<HeartRadiusSynchronizer>
{
    [SerializeField] private TextMeshProUGUI m_heartRadiusFeedback;

    private float m_maxHeartTime;
    // Start is called before the first frame update
    void Awake()
    {
        ClientManager.OnReceiveHeartConquerStop += ReceiveHeartConquerStop;
        ClientManager.OnReceiveHeartConquerStart += ReceiveHeartConquerStart;
        ClientManager.OnReceiveInitialData += ReceiveInitialData;
    }

    private void ReceiveInitialData(InitialData p_initialdata) => m_maxHeartTime = p_initialdata.heartConquerTime;
    

    private void ReceiveHeartConquerStop(HeartConquerStop p_heartconquerstop)=> m_heartRadiusFeedback.enabled = false;
    

    private void ReceiveHeartConquerStart(HeartConquerStart p_heartconquerstart)
    {
        m_heartRadiusFeedback.enabled = true;
        m_heartRadiusFeedback.text = $"Breaking Heart : {((int)(p_heartconquerstart.time * 10f))/10f}/{m_maxHeartTime}";
    }
}
