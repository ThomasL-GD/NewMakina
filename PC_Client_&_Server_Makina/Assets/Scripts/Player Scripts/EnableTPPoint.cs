using UnityEngine;

public class EnableTPPoint : MonoBehaviour
{
    [HideInInspector] public TeleportRollBack m_parentScript; 
    private void OnCollisionEnter(Collision other)
    {
        foreach (ContactPoint contactPoint in other.contacts)
            if(Vector3.Dot(contactPoint.normal, Vector3.up) < .8f) return;
        
        Debug.Log($"Woo ?");
        
        m_parentScript.SetTPoint(transform.position);

        Rigidbody rb = GetComponent<Rigidbody>();
        
        rb.useGravity = false;
        rb.velocity = Vector3.zero;
    }
}
