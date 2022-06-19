using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using TMPro;
using UnityEngine;

public class CreaditsZoomIn : MonoBehaviour
{

    [SerializeField] private Transform m_camera;
    [SerializeField] private Transform m_target;
    [SerializeField] private SpriteRenderer m_sprite;

    [SerializeField] private GameObject m_light;
    
    [SerializeField] private GameObject[] m_toDisable;

    [SerializeField] private AnimationCurve m_curve;
    [SerializeField] private float m_movingTime = 1.5f;
    
    [SerializeField] private TextMeshPro m_textElement;
    [SerializeField] private Collider m_backCollider;
    // Start is called before the first frame update
    void OnMouseOver()
    {
        m_light.SetActive(true);
        m_sprite.color = new Color(0.6914f, 0.6914f, 0.6914f, 1f);
    }

    // Update is called once per frame
    void OnMouseExit()
    {
        m_light.SetActive(false);
        m_sprite.color = new Color(0.2914f, 0.2914f, 0.2914f, 1f);
    }
    

    private void OnMouseDown() => StartCoroutine(MoveCamera());
    

    IEnumerator MoveCamera()
    {
        Debug.Log("once !");
        GetComponent<Collider>().enabled = false;
        m_textElement.enabled = false;

        float timer = 0f;
        float speed = 1f / m_movingTime;

        Vector3 ogPos = m_camera.position;
        Quaternion ogRotation = m_camera.rotation;

        while (timer != 1f)
        {
            m_light.SetActive(true);
            m_sprite.color = new Color(0.6914f, 0.6914f, 0.6914f, 1f);
            timer += speed * Time.deltaTime;

            timer = Mathf.Min(timer, 1f);

            float interpolation = m_curve.Evaluate(timer);

            m_camera.position = Vector3.Lerp(ogPos,m_target.position, interpolation);
            
            m_camera.rotation = Quaternion.Slerp(ogRotation,m_target.rotation , interpolation);
            
            yield return null;
        }

        m_backCollider.enabled = true;
    }
}
