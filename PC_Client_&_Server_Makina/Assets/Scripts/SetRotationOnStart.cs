using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetRotationOnStart : MonoBehaviour
{
    [SerializeField] private Vector3 m_rotation;
    // Start is called before the first frame update
    void Start()
    {
        transform.rotation = Quaternion.Euler(m_rotation);
    }
}
