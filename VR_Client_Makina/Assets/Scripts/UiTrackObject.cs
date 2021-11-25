using System;
using UnityEngine;
using UnityEngine.UI;

public class UiTrackObject : MonoBehaviour
{
    [SerializeField] private GameObject m_objectToTrack;
    [SerializeField] private RawImage m_uiElement;
    [SerializeField] private Camera m_uiCamera;

    [SerializeField] private Canvas m_canvas;
    // Update is called once per frame
    private void Start()
    {
        m_uiElement.transform.parent = m_objectToTrack.transform;
        transform.position = new Vector3(0, 0, 0);
    }

    void LateUpdate()
    {
        m_uiElement.transform.LookAt(m_uiCamera.transform.position);
        m_uiElement.transform.Rotate(Vector3.up * 180f);
    }
}
