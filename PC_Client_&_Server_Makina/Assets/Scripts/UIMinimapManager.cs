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
    private RectTransform m_elevatorParent = null;
    
    [Header("VR Hearts")]
    [SerializeField] [Tooltip("The prefab of a Vr Heart\nMust have a Rect Transform")] private GameObject m_uiVrHeartPrefab = null;
    [SerializeField] [Tooltip("The proportion taken by this element\n0 means nothing and 1 means all the size of the map canvas")] private Vector2 m_vrHeartAnchorRatio = new Vector2(0.1f, 0.2f);
    [SerializeField] [Range(0.1f, 10f)] [Tooltip("The time the minimap heart will blink before disappearing when destroyed")] private float m_heartDestroyBlinkTotalTime = 1f;
    [SerializeField] [Range(0.05f, 2f)] [Tooltip("The time the minimap heart will blink before disappearing when destroyed")] private float m_heartDestroyBlinkTiming = 1f;
    private UIHeartData[] m_heartDatas = Array.Empty<UIHeartData>();
    
    [Header("Beacons")]
    [SerializeField] [Tooltip("The prefab of a deployed beacon that is NOT detecting any player inside\nMust have a Rect Transform")] private GameObject m_prefabBeaconEmpty = null;
    [SerializeField] [Tooltip("The prefab of a deployed beacon that IS detecting a player inside\nMust have a Rect Transform")] private GameObject m_prefabBeaconDetecting = null;
    /// <summary>Reminder : this anchor ratio does not work like the others</summary>
    private float m_beaconAnchorRatio;
    private List<UIBeaconData> m_beaconDatas = new List<UIBeaconData>();

    private bool m_isInGame = false;
    
    private struct UIBeaconData {
        public float ID;
        public RectTransform undetectingBeacon;
        public RectTransform detectingBeacon;
    }

    private struct UIHeartData {
        public RectTransform rectTransform;
        public Vector2 originalRatioOnMap;
    }

    private void Start() {
        
        ClientManager.OnReceiveInitialData += PlaceIcons;
        ClientManager.OnReceiveActivateBeacon += PlaceABeacon;
        ClientManager.OnReceiveBeaconDetectionUpdate += DetectionUpdate;
        ClientManager.OnReceiveDestroyedBeacon += DestroyBeacon;
        ClientManager.OnReceiveHeartBreak += DestroyUIHeart;
    }

    /// <summary>Will place every object once the game starts, is called when we receive InitialData</summary>
    private void PlaceIcons(InitialData p_initialData) {
        
        #region Annihilation
        Destroy(m_elevatorParent);
        foreach (UIBeaconData beaconData in m_beaconDatas) {
            Destroy(beaconData.detectingBeacon.gameObject);
            Destroy(beaconData.undetectingBeacon.gameObject);
        }
        foreach (UIHeartData heartData in m_heartDatas) Destroy(heartData.rectTransform.gameObject); 
        
        m_beaconDatas = new List<UIBeaconData>();
        m_heartDatas = new UIHeartData[p_initialData.heartPositions.Length];
        #endregion

        m_beaconAnchorRatio = 1 / (m_mapRealSize / p_initialData.beaconRange);

        //Instantiate and place every elevator
        m_elevatorParent = Instantiate(new GameObject(), m_mapElement).AddComponent<RectTransform>();
        m_elevatorParent.position = Vector3.zero;
        m_elevatorParent.anchorMin = Vector2.zero;
        m_elevatorParent.anchorMax = Vector2.one;
        m_elevatorParent.anchoredPosition = Vector2.zero;
        m_elevatorParent.localPosition = Vector3.zero;
        m_elevatorParent.position = Vector3.zero;
        m_elevatorParent.offsetMin = Vector2.zero;
        m_elevatorParent.offsetMax = Vector2.zero;
        m_elevatorParent.anchoredPosition = Vector2.zero;
        m_elevatorParent.gameObject.name = "UI Elevators";
        foreach (Vector3 elevatorPos in SynchroniseElevators.Instance.elevatorPositions) {
            Debug.Log($"elevator instantiated : {RatioOnMap(elevatorPos)}");
            PlaceAnchors(Instantiate(m_uiElevatorPrefab, m_elevatorParent).GetComponent<RectTransform>(), RatioOnMap(elevatorPos), m_elevatorsAnchorRatio);
        }
        
        
        //Instantiate and place every heart
        for (int i = 0; i < p_initialData.heartPositions.Length; i++) {
            RectTransform heartRect = Instantiate(m_uiVrHeartPrefab, m_mapElement).GetComponent<RectTransform>();
            Vector2 heartRatioOnMap = RatioOnMap(p_initialData.heartPositions[i]);

            m_heartDatas[i] = new UIHeartData(){rectTransform = heartRect, originalRatioOnMap = heartRatioOnMap};
            PlaceAnchors(heartRect, heartRatioOnMap, m_vrHeartAnchorRatio);
        }

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

        m_playerElement.localRotation = Quaternion.Euler(0, 0, -SynchronizePlayerPosition.Instance.m_player.rotation.eulerAngles.y);
        #endregion
        
        playerPositionRatio = Vector2.one - playerPositionRatio; // We get a more correct player ratio for other elements that will read it

        if (!m_isInGame) return;
        
        #region Hearts
        foreach (UIHeartData heartData in m_heartDatas) {

            if (heartData.rectTransform == null) continue; //If the heart is already destroyed
            
            bool positiveOutOfBoundsX = heartData.originalRatioOnMap.x > playerPositionRatio.x + ((1f / m_mapZoom) / 2f);
            bool negativeOutOfBoundsX = heartData.originalRatioOnMap.x < playerPositionRatio.x - ((1f / m_mapZoom) / 2f);
            bool positiveOutOfBoundsY = heartData.originalRatioOnMap.y > playerPositionRatio.y + ((1f / m_mapZoom) / 2f);
            bool negativeOutOfBoundsY = heartData.originalRatioOnMap.y < playerPositionRatio.y - ((1f / m_mapZoom) / 2f);
            bool inRatioX = !positiveOutOfBoundsX && !negativeOutOfBoundsX;
            bool inRatioY = !positiveOutOfBoundsY && !negativeOutOfBoundsY;

            if (inRatioX && inRatioY) { //If the heart is visible, we simply place it correctly
                PlaceAnchors(heartData.rectTransform, heartData.originalRatioOnMap, m_vrHeartAnchorRatio);
            }
            else {
                float horizontalShift = heartData.originalRatioOnMap.x - playerPositionRatio.x;
                float verticalShift = heartData.originalRatioOnMap.y - playerPositionRatio.y;
                
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

                // the shift between the player and the heart on X or Y depending which one is bigger
                
                float neededDisplacement = minorShift / (Mathf.Abs(majorShift) / (0.5f / m_mapZoom));
                
                PlaceAnchors(heartData.rectTransform, playerPositionRatio + (directionMajorShift * (0.5f / m_mapZoom)) + directionMinorShift * neededDisplacement, m_vrHeartAnchorRatio);
            }
        }
        #endregion

        #region Vr Head
        Transform vrTransform = SynchronizeVrTransforms.Instance.m_headVR;
        Vector3 headVRPosition = vrTransform.position;

        PlaceAnchors(m_vrPlayerElement, RatioOnMap(headVRPosition), m_vrAnchorRatio);
        
        m_vrPlayerElement.localRotation = Quaternion.Euler(0, 0, -vrTransform.rotation.eulerAngles.y + 90f);
        #endregion

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
