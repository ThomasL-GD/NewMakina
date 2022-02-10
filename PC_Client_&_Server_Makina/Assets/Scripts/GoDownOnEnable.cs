using UnityEngine;

public class GoDownOnEnable : MonoBehaviour
{
    void OnEnable()
    {
        Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit);
        transform.position = hit.point;
    }
}
