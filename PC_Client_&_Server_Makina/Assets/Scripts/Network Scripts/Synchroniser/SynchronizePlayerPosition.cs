 using System;
 using Mirror;
 using UnityEngine;
 using CustomMessages;
 using UnityEngine.Rendering;
 using UnityEngine.Rendering.Universal;
 using UnityEngine.Serialization;

 namespace Synchronizers
 {
     public class SynchronizePlayerPosition : Synchronizer
     {
         [SerializeField] private Transform m_player;
         [SerializeField] private Transform m_playerHead;

         [SerializeField] private GameObject[] m_invisbilityFeedbacks = null;
         [SerializeField] private AudioSource m_invisibiltySound;

         private void Start() {
             ClientManager.OnReceiveInvisibility += UpdateInvisibility;
             
             UpdateInvisibility(new PcInvisibility(){isInvisible = false});
         }

         // Update is called once per frame
         void Update()
         {
             if (!NetworkClient.ready) return;
             NetworkClient.Send(new PcTransform() {position = m_player.position, rotation = m_playerHead.rotation});
         }

         void UpdateInvisibility(PcInvisibility p_pcInvisibility) 
         {
             foreach (GameObject go in m_invisbilityFeedbacks) go.SetActive(p_pcInvisibility.isInvisible);
             
             if(p_pcInvisibility.isInvisible) m_invisibiltySound.Play();
         }
     }
 }