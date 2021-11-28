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
    
    /// <summary/> Checking on the update if the player can break a heart and doing so if he can
    void Update()
    {
        // Checking the raycast
        bool lookingAtHeart = Physics.Raycast(m_cameraTransform.position, m_cameraTransform.forward, out RaycastHit hit, m_range);

        GameObject heart = hit.transform.gameObject;
        
        // Checking if the hit object is a heart
        if (lookingAtHeart) lookingAtHeart = heart.layer == (heart.layer | (1 << m_layerMask));
        
        //Enabling or Disabling the feedback
        m_text.enabled = lookingAtHeart;
        
        if (!lookingAtHeart) return;

        //If the item is in sight and range and the key is down
        if (lookingAtHeart && Input.GetKeyDown(m_destroyKey))
            // Tell the server to destroy it
            if(heart.TryGetComponent(out HeartIdentifier hi))
            {
                int heartIndex = hi.heartIndex;
                NetworkClient.Send(new HeartBreak(){index = heartIndex});
            }
            else Debug.LogWarning("there is a gameobject that is on the heart layer that doesn't have the heart identifier class ಠ_ಠ", hit.transform.gameObject);
    }
}
