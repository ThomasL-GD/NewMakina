using System;
using System.Collections;
using System.Collections.Generic;
using CustomMessages;
using Synchronizers;
using UnityEngine;
using UnityEngine.Rendering;

// ReSharper disable CompareOfFloatsByEqualityOperator

public class UIMinimapManager : MonoBehaviour {

    /// <summary>The serialized value of the  </summary>
    [SerializeField] [Range(1f, 50f)] [Tooltip("How many times the map should be zoomed in\nWARNING : if this value is changed in play mode, it will NOT be taken into account for all elements")] private float m_mapZoom = 4f;
    
    [SerializeField] [Tooltip("The real size of the bowl, in meters")] private float m_mapRealSize = 500;

    [Serializable] private enum MinimapShape {
        Square,
        Round,
    }
    [SerializeField] [Tooltip("The shape type of the minimap.\nThe components that shape it are not automatic, contact Blue if you wish to change that.")] private MinimapShape m_mapShape = MinimapShape.Round;
    
    [SerializeField] [Tooltip("Is it the player that rotates on the minimap or the minimap that rotates around the player ?\nFalse for first option and True for the second one")] private bool m_playerRotation = false;
    
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
    [SerializeField] [Tooltip("If true, the elevators will stick to the edges of the minimap")] private bool m_elevatorsStickToTheEdges = true;
    [SerializeField] [Range(0f, 50f)] [Tooltip("Determine the scale down value applied to this element when it is out of bounds (depends on the distance).\nO means there will be no scale down")] private float m_elevatorsEdgeScaleFactor = 1f;
    private UIElementData[] m_elevatorDatas = Array.Empty<UIElementData>();
    
    [Header("VR Hearts")]
    [SerializeField] [Tooltip("The prefab of a Vr Heart\nMust have a Rect Transform")] private GameObject m_uiVrHeartPrefab = null;
    [SerializeField] [Tooltip("The proportion taken by this element\n0 means nothing and 1 means all the size of the map canvas")] private Vector2 m_vrHeartAnchorRatio = new Vector2(0.1f, 0.2f);
    [SerializeField] [Range(0.1f, 10f)] [Tooltip("The time the minimap heart will blink before disappearing when destroyed")] private float m_heartDestroyBlinkTotalTime = 1f;
    [SerializeField] [Range(0.05f, 2f)] [Tooltip("The time the minimap heart will blink before disappearing when destroyed")] private float m_heartDestroyBlinkTiming = 1f;
    [SerializeField] [Tooltip("If true, the hearts will stick to the edges of the minimap")] private bool m_heartsStickToTheEdges = true;
    [SerializeField] [Range(0f, 50f)] [Tooltip("Determine the scale down value applied to this element when it is out of bounds (depends on the distance).\nO means there will be no scale down")] private float m_heartsEdgeScaleFactor = 0f;
    private UIElementData[] m_heartDatas = Array.Empty<UIElementData>();
    
    [Header("Beacons")]
    [SerializeField] [Tooltip("The prefab of a deployed beacon that is NOT detecting any player inside\nMust have a Rect Transform")] private GameObject m_prefabBeaconEmpty = null;
    [SerializeField] [Tooltip("The prefab of a deployed beacon that IS detecting a player inside\nMust have a Rect Transform")] private GameObject m_prefabBeaconDetecting = null;
    /// <summary>Reminder : this anchor ratio does not work like the others</summary>
    private float m_beaconAnchorRatio;
    private List<UIBeaconData> m_beaconDatas = new List<UIBeaconData>();
    
    [Space, Header("Floor Dimming")]
    [SerializeField] [Range(-100f, 1000f)] [Tooltip("The maximum height the player can be to be considered at the bottom floor and he minimal height the player can be to be considered at the middle floor")] private float m_heightBetweenBottomAndMiddleFloor = 30f;
    [SerializeField] [Range(-100f, 1000f)] [Tooltip("The maximum height the player can be to be considered at the middle floor and he minimal height the player can be to be considered at the top floor")] private float m_heightBetweenMiddleAndTopFloor = 60f;

    [SerializeField] [Tooltip("The mask that will show when the player is NOT at the bottom floor")] private RectTransform m_bottomFloorDimmer = null;
    [SerializeField] [Tooltip("The mask that will show when the player is NOT at the middle floor")] private RectTransform m_middleFloorDimmer = null;
    [SerializeField] [Tooltip("The mask that will show when the player is NOT at the top floor")] private RectTransform m_topFloorDimmer = null;
    

    private bool m_isInGame = false;
    
    private struct UIBeaconData {
        /// <summary>The server ID of this beacon </summary>
        // ReSharper disable once InconsistentNaming
        public float ID;
        
        /// <summary>The rest transform of this beacon with the undetecting sprite </summary>
        public RectTransform undetectingBeacon;
        
        /// <summary>The rest transform of this beacon with the detecting sprite </summary>
        public RectTransform detectingBeacon;
    }

    private struct UIElementData {
        /// <summary>The rest transform of this element </summary>
        public RectTransform rectTransform;
        
        /// <summary>The ratio on map of this element at spawn </summary>
        public Vector2 originalRatioOnMap;
    }

    private void Start() {
        ClientManager.OnReceiveInitialData += PlaceIcons;
        ClientManager.OnReceiveActivateBeacon += PlaceABeacon;
        ClientManager.OnReceiveBeaconDetectionUpdate += DetectionUpdate;
        ClientManager.OnReceiveDestroyedBeacon += DestroyBeacon;
        ClientManager.OnReceiveHeartBreak += DestroyUIHeart;
        ClientManager.OnReceiveGameEnd += ReceiveGameEnd;
        
        m_playerElement.gameObject.SetActive(false);
    }

    private void ReceiveGameEnd(GameEnd p_gameEnd) {
        m_playerElement.gameObject.SetActive(false);
    }

    /// <summary>Will place every object once the game starts, is called when we receive InitialData</summary>
    private void PlaceIcons(InitialData p_initialData) {
        
        #region Annihilation
        foreach (UIBeaconData beaconData in m_beaconDatas) {
            Destroy(beaconData.detectingBeacon.gameObject);
            Destroy(beaconData.undetectingBeacon.gameObject);
        }
        foreach (UIElementData heartData in m_heartDatas) Destroy(heartData.rectTransform.gameObject); 
        foreach (UIElementData elevatorData in m_elevatorDatas) Destroy(elevatorData.rectTransform.gameObject); 
        
        m_beaconDatas = new List<UIBeaconData>();
        m_heartDatas = new UIElementData[p_initialData.heartPositions.Length];
        m_elevatorDatas = new UIElementData[SynchroniseElevators.Instance.elevatorPositions.Length];
        
        if(m_bottomFloorDimmer != null)m_bottomFloorDimmer.gameObject.SetActive(false);
        if(m_middleFloorDimmer != null)m_middleFloorDimmer.gameObject.SetActive(false);
        if(m_topFloorDimmer != null)m_topFloorDimmer.gameObject.SetActive(false);
        #endregion

        m_beaconAnchorRatio = 1 / (m_mapRealSize / p_initialData.beaconRange);
        
        //Instantiate and place every elevator
        for (byte i = 0; i < SynchroniseElevators.Instance.elevatorPositions.Length; i++) {
            RectTransform elevatorRect = Instantiate(m_uiElevatorPrefab, m_mapElement).GetComponent<RectTransform>();
            Vector2 elevatorRatioOnMap = RatioOnMap(SynchroniseElevators.Instance.elevatorPositions[i]);

            m_elevatorDatas[i] = new UIElementData() {rectTransform = elevatorRect, originalRatioOnMap = elevatorRatioOnMap};
            PlaceAnchors(elevatorRect, elevatorRatioOnMap, m_elevatorsAnchorRatio);
        }
        
        //Instantiate and place every heart
        for (byte i = 0; i < p_initialData.heartPositions.Length; i++) {
            RectTransform heartRect = Instantiate(m_uiVrHeartPrefab, m_mapElement).GetComponent<RectTransform>();
            Vector2 heartRatioOnMap = RatioOnMap(p_initialData.heartPositions[i]);

            m_heartDatas[i] = new UIElementData(){rectTransform = heartRect, originalRatioOnMap = heartRatioOnMap};
            PlaceAnchors(heartRect, heartRatioOnMap, m_vrHeartAnchorRatio);
        }
        
        m_playerElement.gameObject.SetActive(true);

        m_isInGame = true;
    }
    
    #region Beacons
    /// <summary>Will create a Beacon and add it to m_beaconDatas</summary>
    private void PlaceABeacon(ActivateBeacon p_activateBeacon) {

        RectTransform detectingBeacon = Instantiate(m_prefabBeaconDetecting, m_mapElement).GetComponent<RectTransform>();
        RectTransform undetectingBeacon = Instantiate(m_prefabBeaconEmpty, m_mapElement).GetComponent<RectTransform>();

        Vector3 beaconWorldPos = SynchronizeBeacons.Instance.GetBeaconPosition(p_activateBeacon.index, p_activateBeacon.beaconID);
        
        //place anchors of both of them
        Vector2 anchorMin = RatioOnMap(beaconWorldPos) - new Vector2(m_beaconAnchorRatio, m_beaconAnchorRatio);
        Vector2 anchorMax = RatioOnMap(beaconWorldPos) + new Vector2(m_beaconAnchorRatio, m_beaconAnchorRatio);
        detectingBeacon.anchorMin = anchorMin;
        undetectingBeacon.anchorMin = anchorMin;
        detectingBeacon.anchorMax = anchorMax;
        undetectingBeacon.anchorMax = anchorMax;
        
        detectingBeacon.gameObject.SetActive(false);//We set the beacon not detecting by default
        
        m_beaconDatas.Add(new UIBeaconData(){detectingBeacon = detectingBeacon, undetectingBeacon = undetectingBeacon, ID = p_activateBeacon.beaconID});
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
    #endregion

    #region Destroy Heart
    /// <summary>Destroys a UI heart </summary>
    private void DestroyUIHeart(HeartBreak p_heartBreak) {
        StartCoroutine(BlinkHeart(p_heartBreak.index));
    }

    /// <summary>Makes a UI heart blink then destroys it</summary>
    /// <param name="p_index">The index of the heart</param>
    private IEnumerator BlinkHeart(int p_index) {
        float elapsedTime = 0f;
        while (elapsedTime < m_heartDestroyBlinkTotalTime) {
            m_heartDatas[p_index].rectTransform.gameObject.SetActive(!m_heartDatas[p_index].rectTransform.gameObject.activeInHierarchy);
            elapsedTime += m_heartDestroyBlinkTiming;
            yield return new WaitForSeconds(m_heartDestroyBlinkTiming);
        }

        Destroy(m_heartDatas[p_index].rectTransform.gameObject);
    }
    #endregion

    // Update is called once per frame
    void Update() {
        
        #region Map position thus player position
        Vector3 playerPosition = SynchronizePlayerPosition.Instance.m_player.position;
        Vector2 playerPositionRatio = Vector2.one - RatioOnMap(playerPosition);
            
        //Operation order can be changed to be shorter but it basically does calculate the anchors depending on the zoom value
        m_mapElement.anchorMin = new Vector2(((playerPositionRatio.x * m_mapZoom) - (0.5f * (m_mapZoom - 1f))) - (m_mapZoom / 2f), ((playerPositionRatio.y * m_mapZoom) - (0.5f * (m_mapZoom - 1f))) - (m_mapZoom / 2f)); //TODO : ask Schreiner if I can optimize this by making the math of (0.5f * (m_mapZoom - 1)) only once at the cost of one variable
        m_mapElement.anchorMax = new Vector2(((playerPositionRatio.x * m_mapZoom) - (0.5f * (m_mapZoom - 1f))) + (m_mapZoom / 2f), ((playerPositionRatio.y * m_mapZoom) - (0.5f * (m_mapZoom - 1f))) + (m_mapZoom / 2f));

        float playerRotationY = SynchronizePlayerPosition.Instance.m_player.rotation.eulerAngles.y;
        if (m_playerRotation) {
            m_playerElement.localRotation = Quaternion.Euler(0, 0, -playerRotationY);
        }else {
            m_mapElement.pivot = Vector2.one - playerPositionRatio;
            m_mapElement.localRotation = Quaternion.Euler(0, 0, playerRotationY);
        }
        #endregion

        #region Floor Dimmers
            if(m_bottomFloorDimmer != null)m_bottomFloorDimmer.gameObject.SetActive(playerPosition.y < m_heightBetweenBottomAndMiddleFloor);
            if(m_middleFloorDimmer != null)m_middleFloorDimmer.gameObject.SetActive(playerPosition.y >= m_heightBetweenBottomAndMiddleFloor && playerPosition.y <= m_heightBetweenMiddleAndTopFloor);
            if(m_topFloorDimmer != null)m_topFloorDimmer.gameObject.SetActive(playerPosition.y > m_heightBetweenMiddleAndTopFloor);
        #endregion
        
        playerPositionRatio = Vector2.one - playerPositionRatio; // We get a more correct player ratio for other elements that will read it

        if (!m_isInGame) return;
        
        #region Glue & Rotations
        foreach (UIElementData heartData in m_heartDatas) {
            if (heartData.rectTransform == null) continue; //If the heart is already destroyed
            if(m_heartsStickToTheEdges) StickToEdges(playerPositionRatio, heartData, m_vrHeartAnchorRatio, m_heartsEdgeScaleFactor);
            if(!m_playerRotation) heartData.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -playerRotationY);
        }

        foreach (UIElementData elevatorData in m_elevatorDatas) {
            if (m_elevatorsStickToTheEdges) StickToEdges(playerPositionRatio, elevatorData, m_elevatorsAnchorRatio, m_elevatorsEdgeScaleFactor);
            if(!m_playerRotation) elevatorData.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -playerRotationY);
        }
        #endregion

        #region Vr Head
        Transform vrTransform = SynchronizeVrTransforms.Instance.m_headVR;
        Vector3 headVRPosition = vrTransform.position;

        PlaceAnchors(m_vrPlayerElement, RatioOnMap(headVRPosition), m_vrAnchorRatio);
        
        m_vrPlayerElement.localRotation = Quaternion.Euler(0, 0, -vrTransform.rotation.eulerAngles.y + 90f);
        #endregion

    }

    /// <summary>Will Make a UI element stick to the edges of the map in case it is out of the minimap bounds</summary>
    /// <param name="p_playerPositionRatio">The current player position ratio, will not be modified</param>
    /// <param name="p_elementData">The UI element you want to stick to edges, will not be modified</param>
    /// <param name="p_anchorRatio">The anchor Ratio of the element you want to place</param>
    /// <param name="p_scaleFactor">The scale applied to this element the farther it gets out of bounds</param>
    private void StickToEdges(Vector2 p_playerPositionRatio, UIElementData p_elementData, Vector2 p_anchorRatio, float p_scaleFactor) {

        switch (m_mapShape) {
            case MinimapShape.Square:

                bool positiveOutOfBoundsX = p_elementData.originalRatioOnMap.x > p_playerPositionRatio.x + ((1f / m_mapZoom) / 2f);
                bool negativeOutOfBoundsX = p_elementData.originalRatioOnMap.x < p_playerPositionRatio.x - ((1f / m_mapZoom) / 2f);
                bool positiveOutOfBoundsY = p_elementData.originalRatioOnMap.y > p_playerPositionRatio.y + ((1f / m_mapZoom) / 2f);
                bool negativeOutOfBoundsY = p_elementData.originalRatioOnMap.y < p_playerPositionRatio.y - ((1f / m_mapZoom) / 2f);
                bool inRatioX = !positiveOutOfBoundsX && !negativeOutOfBoundsX;
                bool inRatioY = !positiveOutOfBoundsY && !negativeOutOfBoundsY;

                if (inRatioX && inRatioY) { //If the heart is visible, we simply place it correctly
                    PlaceAnchors(p_elementData.rectTransform, p_elementData.originalRatioOnMap, p_anchorRatio);
                }
                else {
                    float horizontalShift = p_elementData.originalRatioOnMap.x - p_playerPositionRatio.x;
                    float verticalShift = p_elementData.originalRatioOnMap.y - p_playerPositionRatio.y;

                    Vector2 directionMajorShift;
                    Vector2 directionMinorShift;
                    float majorShift;
                    float minorShift;
                    if (Mathf.Abs(horizontalShift) >= Mathf.Abs(verticalShift)) { //Horizontal is Major
                        majorShift = horizontalShift;
                        minorShift = verticalShift;

                        directionMinorShift = Vector2.up;
                        directionMajorShift = majorShift > 0 ? Vector2.right : Vector2.left;
                    }
                    else { //Vertical is Major
                        majorShift = verticalShift;
                        minorShift = horizontalShift;

                        directionMinorShift = Vector2.right;
                        directionMajorShift = majorShift > 0 ? Vector2.up : Vector2.down;
                    }

                    // the shift between the player and the heart on X or Y depending which one is smaller
                    float neededDisplacement = minorShift / (Mathf.Abs(majorShift) / (0.5f / m_mapZoom));
                    
                    float rescaleFactor = (1f - (((p_elementData.originalRatioOnMap - p_playerPositionRatio).magnitude - ((1f/(2f*m_mapZoom)) + neededDisplacement)) * p_scaleFactor));
                    if (rescaleFactor > 0) p_anchorRatio *= rescaleFactor; //Rescale this element the farther it gets depending on p_scaleFactor
                    else p_anchorRatio = Vector2.zero;
                    PlaceAnchors(p_elementData.rectTransform, p_playerPositionRatio + (directionMajorShift * (0.5f / m_mapZoom)) + directionMinorShift * neededDisplacement, p_anchorRatio);
                }

                break;
            
            case MinimapShape.Round :
                if ((p_elementData.originalRatioOnMap - p_playerPositionRatio).magnitude < 1f / (m_mapZoom * 2f)) PlaceAnchors(p_elementData.rectTransform, p_elementData.originalRatioOnMap, p_anchorRatio);
                else {
                    float rescaleFactor = (1f - (((p_elementData.originalRatioOnMap - p_playerPositionRatio).magnitude - (1f/(2f*m_mapZoom))) * p_scaleFactor)); //Rescale this element the farther it gets depending on p_scaleFactor
                    if (rescaleFactor > 0f) p_anchorRatio *= rescaleFactor;
                    else p_anchorRatio = Vector2.zero;
                    PlaceAnchors(p_elementData.rectTransform, p_playerPositionRatio + ((p_elementData.originalRatioOnMap - p_playerPositionRatio).normalized * (1f / (m_mapZoom * 2f))), p_anchorRatio);
                }
                break;
            
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    #region RationOnMap
    /// <summary>Gives a vector2 with both x and y being between 0 and 1 and corresponds to how far this object is from the center (0.5 is the center, 0 is the farthest on one side and 1 is the farthest on the opposite side) given its world position</summary>
    /// <param name="p_worldPosition">Its 3D coordinates</param>
    /// <returns>The ratio on the map</returns>
    private Vector2 RatioOnMap(Vector3 p_worldPosition) => RatioOnMap(new Vector2(p_worldPosition.x, p_worldPosition.z));

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