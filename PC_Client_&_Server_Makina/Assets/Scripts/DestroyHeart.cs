using CustomMessages;
using Mirror;
using TMPro;
using UnityEngine;

public class DestroyHeart : MonoBehaviour
{

    [SerializeField][Tooltip("the hearts layer mask")] private LayerMask m_layerMask;

    [SerializeField][Tooltip("the Pc player's camera Transform")] private Transform m_cameraTransform;
    
    [SerializeField][Tooltip("the UI feedback when the players can interact with a heart")] private TextMeshProUGUI m_text;
    [SerializeField][Tooltip("the Key the players should press to destroy a heart")] private KeyCode m_destroyKey;
    
    [SerializeField][Tooltip("the range at which the players can break a heart")] private float m_range = 20f;

    [SerializeField] [Tooltip("The strength at which the player will be thrown from a heart being destroyed")]
    private float m_yeetStrength = 20f;

    [SerializeField] [Tooltip("The strength at which the player will be thrown upwards from a heart being destroyed")]
    private float m_upYeetStrength = 20f;
    
    /// <summary/> Checking on the update if the player can break a heart and doing so if he can
    void Update()
    {
        // is the player looking at a heart in range?
        bool lookingAtHeart = true;
        
        // The heart the player is looking at
        GameObject heart;
        
        // Checking if the player has a heart in range and is looking at it
        if (Physics.Raycast(m_cameraTransform.position, m_cameraTransform.forward, out RaycastHit hit, m_range, m_layerMask))
        {
            heart = hit.transform.gameObject;
        }else
        {
            m_text.enabled = false;
            return;
        }
        
        // Checking if there are nop obstacle between the player and the heart
        Physics.Raycast(m_cameraTransform.position, m_cameraTransform.forward, out hit, m_range);

        
        // Setting the feedback in accordance to wether the player is looking at a heart or not
        lookingAtHeart = hit.transform.gameObject == heart;
        m_text.enabled = lookingAtHeart;
        
        // If the player is not looking at a heart GTFO
        if(!lookingAtHeart) return;

        //If the heart is in sight and range and the key is down
        if (Input.GetKeyDown(m_destroyKey))
            if(heart.TryGetComponent(out HeartIdentifier hi))
            {
                // Tell the server to destroy it
                int heartIndex = hi.heartIndex;
                NetworkClient.Send(new HeartBreak(){index = heartIndex});
                
                // Yeet the player
                //InputMovement3.instance.m_velocity += (transform.position - heart.transform.position).normalized * m_yeetStrength + Vector3.up * m_upYeetStrength;
            }
            else Debug.LogWarning("There is a gameobject that is on the heart layer that doesn't have the heart identifier class ಠ_ಠ", hit.transform.gameObject);
    }
}
