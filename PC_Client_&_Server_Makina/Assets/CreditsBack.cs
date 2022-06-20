using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CreditsBack : MonoBehaviour
{
    private bool m_clicked;
    [SerializeField]private float m_movingTime = 2f;
    [SerializeField]private Transform m_camera;
    [SerializeField]private AnimationCurve m_curve;
    [SerializeField]private Transform m_target;
    [SerializeField]private TextMeshPro m_textElement;
    [SerializeField]private Collider m_crdaitsCollider;

    [SerializeField]private TextMeshPro m_textBack;

    void OnMouseOver()
    {
        m_textBack.color = Color.gray;
    }

    // Update is called once per frame
    void OnMouseExit()
    {
        m_textBack.color = Color.black;
    }
    
    private void OnMouseDown() => StartCoroutine(MoveCamera());
    
    IEnumerator MoveCamera()
    {
        Debug.Log("starts");
        GetComponent<Collider>().enabled = false;
        m_clicked = true;

        float timer = 0f;
        float speed = 1f / m_movingTime;

        Vector3 ogPos = m_camera.position;
        Quaternion ogRotation = m_camera.rotation;

        while (timer != 1f)
        {
            timer += speed * Time.deltaTime;

            timer = Mathf.Min(timer, 1f);

            float interpolation = m_curve.Evaluate(timer);

            m_camera.position = Vector3.Lerp(ogPos,m_target.position, interpolation);
            
            m_camera.rotation = Quaternion.Lerp(ogRotation,m_target.rotation , interpolation);
            
            yield return null;
        }

        m_textElement.enabled = true;
        m_crdaitsCollider.enabled = true;
    }
}
