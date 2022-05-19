using UnityEngine;

public class RotateAndBob : MonoBehaviour
{
    [SerializeField,Tooltip("the speed at which the object will rotate in °/s")]float m_rotationSpeed = 20f;
    [SerializeField,Tooltip("the speed at which the object will bob in oscillations per second")]float m_bobSpeed = 1f;
    [SerializeField,Tooltip("The intensity of the bob in m")]float m_bobIntensity = .5f;

    private Vector3 m_ogPos;

    private void Start()
    {
        m_ogPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up * (m_rotationSpeed * Time.deltaTime));
        transform.position = m_ogPos + Vector3.up * Mathf.Cos((Time.time / Mathf.PI) * m_bobSpeed) * m_bobIntensity;
    }
}
