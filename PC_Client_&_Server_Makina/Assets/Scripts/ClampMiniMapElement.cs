using UnityEngine;

public class ClampMiniMapElement : MonoBehaviour
{
    private Vector3 m_ogPosition;
    private Vector3 m_ogScale;
    [SerializeField, Min(0)] private float m_margin = 5f;
    [SerializeField, Min(0)] private float m_maxDistance = 400f;
    private void Start()
    {
        m_ogPosition = transform.position;
        m_ogScale = transform.localScale;
    }
    

    void Update()
    {
        Camera mapsCam = CameraAndUISingleton.mapCamera;
        Vector2 position2D = new Vector2(m_ogPosition.x,m_ogPosition.z);
        Vector3 mapCamPosition = mapsCam.transform.position;
        Vector2 mapCam2D = new Vector2(mapCamPosition.x,mapCamPosition.z);

        float mapSize = mapsCam.orthographicSize - m_margin;
        
        position2D.x = Mathf.Clamp(position2D.x, mapCam2D.x - mapSize, mapCam2D.x + mapSize);
        position2D.y = Mathf.Clamp(position2D.y, mapCam2D.y - mapSize, mapCam2D.y + mapSize);

        float factor = Mathf.Max(m_maxDistance - Mathf.Max(Vector2.Distance(position2D, mapCam2D) - mapSize, 0),m_maxDistance/5f)/m_maxDistance;
        transform.localScale = new Vector3(m_ogScale.x*factor,m_ogScale.y,m_ogScale.z*factor);
        transform.position = new Vector3(position2D.x, m_ogPosition.y, position2D.y);
    }
}
