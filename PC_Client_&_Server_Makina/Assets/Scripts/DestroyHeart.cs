using CustomMessages;
using Mirror;
using TMPro;
using UnityEngine;

public class DestroyHeart : MonoBehaviour
{

    [SerializeField] private LayerMask m_layerMask;

    [SerializeField] private Transform m_cameraTransform;
    
    [SerializeField] private TextMeshProUGUI m_text;
    [SerializeField] private KeyCode m_destroyKey;
    
    [SerializeField] private float m_range = 20f;
    
    // Update is called once per frame
    void Update()
    {
        // Checking the raycast for objects of the heart layer
        bool lookingAtHeart = Physics.Raycast(m_cameraTransform.position, m_cameraTransform.forward, out RaycastHit hit, m_range, m_layerMask);
        m_text.enabled = lookingAtHeart;

        //If the item is in sight and range and the key is down
        if (lookingAtHeart && Input.GetKeyDown(m_destroyKey))
        {
            // Tell the server to destroy it
            if(hit.transform.gameObject.TryGetComponent(out HeartIdentifier hi))
            {
                int heartIndex = hi.heartIndex;
                NetworkClient.Send(new HeartBreak(){index = heartIndex});
            }
            else
            {
                Debug.Log("there is a gameobject that is on the heart layer that doesn't have the heart identifier class ಠ_ಠ", hit.transform.gameObject);
            }
        }
    }
}
