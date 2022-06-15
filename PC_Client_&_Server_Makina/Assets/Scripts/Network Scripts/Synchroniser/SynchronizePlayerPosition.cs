 using Mirror;
 using UnityEngine;
 using CustomMessages;
 
 namespace Synchronizers
 {
     public class SynchronizePlayerPosition : Synchronizer<SynchronizePlayerPosition>
     {
         [SerializeField] public Transform m_player;
         [SerializeField] private Transform m_playerHead;

         [SerializeField] private GameObject[] m_invisbilityFeedbacks = null;

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

         // TODO : moce this to it's own synchronizer ! 
         void UpdateInvisibility(PcInvisibility p_pcInvisibility) 
         {
             foreach (GameObject go in m_invisbilityFeedbacks) go.SetActive(p_pcInvisibility.isInvisible);
         }
     }
 }