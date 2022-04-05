using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OpenMap : MonoBehaviour
{
    [SerializeField] private RectTransform m_mapUIElement;
    [SerializeField] private KeyCode m_openMapKey = KeyCode.M;
    [SerializeField] private float m_targetMapRange =300f;
    
    private Camera m_mapCamera;
    private bool m_mapOpened = false;

    private Vector4 m_anchorPoints;
    private float m_ogMapRange;
    // Start is called before the first frame update
    void Start()
    {
        m_mapCamera = CameraAndUISingleton.mapCamera;
        m_anchorPoints = new Vector4(m_mapUIElement.anchorMin.x,
            m_mapUIElement.anchorMin.y,
            m_mapUIElement.anchorMax.x,
            m_mapUIElement.anchorMax.y);
        m_ogMapRange = m_mapCamera.orthographicSize;
    } 
    

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(m_openMapKey))
        {
            if (m_mapOpened)
            {
                m_mapUIElement.anchorMin = new Vector2(m_anchorPoints.x,m_anchorPoints.y);
                m_mapUIElement.anchorMax = new Vector2(m_anchorPoints.z,m_anchorPoints.w);
                m_mapOpened = false;
                m_mapCamera.orthographicSize = m_ogMapRange;
            }else
            {
                m_mapUIElement.anchorMin = new Vector2(.1f, .1f);
                m_mapUIElement.anchorMax = new Vector2(.9f, .9f);
                m_mapOpened = true;
                m_mapCamera.orthographicSize = m_targetMapRange;
            }
        }
    }
}
