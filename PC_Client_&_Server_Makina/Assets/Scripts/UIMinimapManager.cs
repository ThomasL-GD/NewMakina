using System.Collections.Generic;
using CustomMessages;
using Synchronizers;
using UnityEngine;
// ReSharper disable CompareOfFloatsByEqualityOperator

public class UIMinimapManager : MonoBehaviour {

    /// <summary>The serialized value of the  </summary>
    [SerializeField] [Range(1f, 50f)] [Tooltip("How many times the map should be zoomed in\nWarning, if this value is changed in play mode, it will NOT be taken into account")] private float m_mapZoom = 4f;
    
    [SerializeField] [Tooltip("The real size of the bowl, in meters")] private float m_mapRealSize = 500;
    
    [Space]
    [Header("UI elements")]
    [Space]
    [SerializeField] [Tooltip("The object of the map that should move alongside the player")] private RectTransform m_mapElement = null;
    [SerializeField] [Tooltip("The object of the player's character on UI")] private RectTransform m_playerElement = null;
    
    [Header("Vr Head")]
    [SerializeField] [Tooltip("The object of the VR player on UI\nMust be child of the map")] private RectTransform m_vrPlayerElement = null;
    [SerializeField] [Tooltip("The proportion taken by this element\n0 means nothing and 1 means all the size of the map canvas")] private Vector2 m_vrAnchorRatio = new Vector2(0.2f, 0.2f);
    
    [Header("Elevators")]
    [SerializeField] [Tooltip("The prefab of an UI elevator\nMust have a Rect Transform")] private GameObject m_uiElevatorPrefab = null;
    [SerializeField] [Tooltip("The proportion taken by this element\n0 means nothing and 1 means all the size of the map canvas")] private Vector2 m_elevatorsAnchorRatio = new Vector2(0.1f, 0.2f);
    
    [Header("VR Hearts")]
    [SerializeField] [Tooltip("The prefab of a Vr Heart\nMust have a Rect Transform")] private GameObject m_uiVrHeartPrefab = null;
    [SerializeField] [Tooltip("The proportion taken by this element\n0 means nothing and 1 means all the size of the map canvas")] private Vector2 m_vrHeartAnchorRatio = new Vector2(0.1f, 0.2f);
    
    [Header("Beacons")]
    [SerializeField] [Tooltip("The prefab of a deployed beacon that is NOT detecting any player inside\nMust have a Rect Transform")] private GameObject m_prefabBeaconEmpty = null;
    [SerializeField] [Tooltip("The prefab of a deployed beacon that IS detecting a player inside\nMust have a Rect Transform")] private GameObject m_prefabBeaconDetecting = null;
    /// <summary>Reminder : this anchor ratio does not work like the others</summary>
    private float m_beaconAnchorRatio;
    private List<BeaconData> m_beaconDatas;
    
    private struct BeaconData {
        public float ID;
        public RectTransform undetectingBeacon;
        public RectTransform detectingBeacon;
    }

    private void Start() {
        
        ClientManager.OnReceiveInitialData += PlaceIcons;
        ClientManager.OnReceiveActivateBeacon += PlaceABeacon;
        ClientManager.OnReceiveBeaconDetectionUpdate += DetectionUpdate;
        ClientManager.OnReceiveDestroyedBeacon += DestroyBeacon;
    }

    /// <summary>Will place every object once the game starts, is called when we receive InitialData</summary>
    private void PlaceIcons(InitialData p_initialData) {

        m_beaconAnchorRatio = 1 / (m_mapRealSize / p_initialData.beaconRange);

        //Instantiate and place every elevator
        foreach (Vector3 elevatorPos in SynchroniseElevators.Instance.elevatorPositions) {
            PlaceAnchors(Instantiate(m_uiElevatorPrefab, m_mapElement).GetComponent<RectTransform>(), RatioOnMap(elevatorPos), m_elevatorsAnchorRatio);
        }
        
        //Instantiate and place every heart
        foreach (Vector3 heartPos in p_initialData.heartPositions) {
            PlaceAnchors(Instantiate(m_uiVrHeartPrefab, m_mapElement).GetComponent<RectTransform>(), RatioOnMap(heartPos), m_vrHeartAnchorRatio);
        }
    }

    /// <summary>Will create a Beacon and add it to m_beaconDatas</summary>
    private void PlaceABeacon(ActivateBeacon p_activateBeacon) {

        RectTransform detectingBeacon = Instantiate(m_prefabBeaconDetecting, m_mapElement).GetComponent<RectTransform>();
        detectingBeacon.gameObject.SetActive(false);
        RectTransform undetectingBeacon = Instantiate(m_prefabBeaconEmpty, m_mapElement).GetComponent<RectTransform>();

        Vector3 beaconWorldPos = SynchronizeBeacons.Instance.GetBeaconPosition(p_activateBeacon.index, p_activateBeacon.beaconID);
        
        //TODO : place anchors of both of them
        Vector2 anchorMin = RatioOnMap(beaconWorldPos);
        
        m_beaconDatas.Add(new BeaconData(){detectingBeacon = detectingBeacon, undetectingBeacon = undetectingBeacon, ID = p_activateBeacon.beaconID});
    }

    /// <summary>Update the color of a beacon element</summary>
    private void DetectionUpdate(BeaconDetectionUpdate p_beaconDetection) {
        
        int? index = FindBeaconFromID(p_beaconDetection.index, p_beaconDetection.beaconID);
        if (index == null) return;
        
        m_beaconDatas[index??0].detectingBeacon.gameObject.SetActive(p_beaconDetection.playerDetected);
        m_beaconDatas[index??0].undetectingBeacon.gameObject.SetActive(!p_beaconDetection.playerDetected);
    }

    /// <summary>Erase a beacon from m_beaconDatas</summary>
    private void DestroyBeacon(DestroyedBeacon p_destroyedBeacon) {
        
        int? index = FindBeaconFromID(p_destroyedBeacon.index, p_destroyedBeacon.beaconID);
        if (index == null) return;
        
        Destroy(m_beaconDatas[index??0].detectingBeacon.gameObject);
        Destroy(m_beaconDatas[index??0].undetectingBeacon.gameObject);
        m_beaconDatas.RemoveAt(index??0);
    }

    // Update is called once per frame
    void Update() {
        
        #region Map position thus player position
        //TODO : Can optimize it if I remove the playerPositionRatio calculation step but it'd be less clear
        Vector3 playerPosition = SynchronizePlayerPosition.Instance.m_player.position;
        Vector2 playerPositionRatio = RatioOnMap(playerPosition);
            
        m_mapElement.anchorMin = new Vector2(((playerPositionRatio.x * m_mapZoom) - (0.5f * (m_mapZoom - 1f))) - (m_mapZoom / 2f), ((playerPositionRatio.y * m_mapZoom) - (0.5f * (m_mapZoom - 1f))) - (m_mapZoom / 2f)); //TODO : ask Schreiner if I can optimize this by making the math of (0.5f * (m_mapZoom - 1)) only once at the cost of one variable
        m_mapElement.anchorMax = new Vector2(((playerPositionRatio.x * m_mapZoom) - (0.5f * (m_mapZoom - 1f))) + (m_mapZoom / 2f), ((playerPositionRatio.y * m_mapZoom) - (0.5f * (m_mapZoom - 1f))) + (m_mapZoom / 2f));

        m_playerElement.localRotation = Quaternion.Euler(0, 0, -SynchronizePlayerPosition.Instance.m_player.rotation.eulerAngles.y + 180f);
        #endregion

        #region Vr Head
        Transform vrTransform = SynchronizeVrTransforms.Instance.m_headVR;
        Vector3 headVRPosition = vrTransform.position;

        PlaceAnchors(m_vrPlayerElement, RatioOnMap(headVRPosition), m_vrAnchorRatio);
        
        m_vrPlayerElement.localRotation = Quaternion.Euler(0, 0, -vrTransform.rotation.eulerAngles.y + 180f);
        #endregion

    }

    #region RationOnMap
    /// <summary>Gives a vector2 with both x and y being between 0 and 1 and corresponds to how far this object is from the center (0.5 is the center, 0 is the farthest on one side and 1 is the farthest on the opposite side) given its world position</summary>
    /// <param name="p_worldPosition">Its 3D coordinates</param>
    /// <returns>The ratio on the map</returns>
    private Vector2 RatioOnMap(Vector3 p_worldPosition) {
        return RatioOnMap(new Vector2(p_worldPosition.x, p_worldPosition.z));
    }
    
    /// <summary>Gives a vector2 with both x and y being between 0 and 1 and corresponds to how far this object is from the center (0.5 is the center, 0 is the farthest on one side and 1 is the farthest on the opposite side) given its world position</summary>
    /// <param name="p_worldPositionVector2ified">Its 2D top view coordinates</param>
    /// <returns>The ratio on the map</returns>
    // ReSharper disable once InconsistentNaming
    private Vector2 RatioOnMap(Vector2 p_worldPositionVector2ified) {
        //This line gives a vector2 with both x and y being between 0 and 1 and corresponds to how far the player is from the center (0.5 is the center, 0 is the farthest on one side and 1 is the farthest on the opposite side)
        return new Vector2(((p_worldPositionVector2ified.x / (m_mapRealSize/2f)) + 1f) / 2f, ((p_worldPositionVector2ified.y / (m_mapRealSize/2f)) + 1f) / 2f); 
    }
    #endregion

    #region PlaceAnchors
    /// <summary>Will place all four anchors of an element child of the map to give it the wanted position and size</summary>
    /// <param name="p_uiElement">The element you want to place</param>
    /// <param name="p_ratioOnMap">Its map ratio position (can be calculated with RatioOnMap</param>
    /// <param name="p_anchorsRatio">The size of the element (must be between 0 an 1, corresponds to a map canvas percentage space)</param>
    /// <remarks>Is only applicable to elements that are child of the map</remarks>
    private void PlaceAnchors(RectTransform p_uiElement, Vector2 p_ratioOnMap, Vector2 p_anchorsRatio) {
        p_uiElement.anchorMin = new Vector2(p_ratioOnMap.x - (p_anchorsRatio.x/(2f * m_mapZoom)), p_ratioOnMap.y - (p_anchorsRatio.y/(2f * m_mapZoom)));
        p_uiElement.anchorMax = new Vector2(p_ratioOnMap.x + (p_anchorsRatio.x/(2f * m_mapZoom)), p_ratioOnMap.y + (p_anchorsRatio.y/(2f * m_mapZoom)));
    }

    /// <summary>Will place all four anchors of an element child of the map to give it the wanted position and size</summary>
    /// <param name="p_uiElement">The element you want to place</param>
    /// <param name="p_ratioOnMap">Its map ratio position (can be calculated with RatioOnMap</param>
    /// <param name="p_anchorsRatio">The size of the element (must be between 0 an 1, corresponds to a map canvas percentage space)</param>
    /// <remarks>Is only applicable to elements that are child of the map</remarks>
    private void PlaceAnchors(RectTransform p_uiElement, Vector2 p_ratioOnMap, float p_anchorsRatio) {
        PlaceAnchors(p_uiElement, p_ratioOnMap, new Vector2(p_anchorsRatio, p_anchorsRatio));
    }
    #endregion

    /// <summary/> A function to find the index of the beacon that matches the given ID
    /// <param name="p_index"> the estimated index of the wanted beacon </param>
    /// <param name="p_beaconID"> the ID of the wanted beacon </param>
    /// <returns> returns the index of the beacon with the right ID if none are found, returns null </returns>
    private int? FindBeaconFromID(int p_index, float p_beaconID) {
        
        int index = p_index;
        float ID = p_beaconID;
        if ( index < m_beaconDatas.Count && m_beaconDatas[index].ID == ID) return index;

        for (int i = 0; i < m_beaconDatas.Count; i++) if (m_beaconDatas[i].ID == ID) return i;

#if UNITY_EDITOR
        Debug.LogWarning($"I couldn't find the index matching this ID ({p_beaconID}) brother - context : minimap", this);
#endif
            
        return null;
    }
}
