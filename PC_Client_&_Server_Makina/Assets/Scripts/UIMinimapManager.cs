using System;
using System.Collections;
using System.Collections.Generic;
using CustomMessages;
using Synchronizers;
using UnityEngine;

public class UIMinimapManager : MonoBehaviour {

    /// <summary>The serialized value of the  </summary>
    [SerializeField] [Range(1f, 50f)] [Tooltip("How many times the map should be zoomed in\nWarning, if this value is changed in play mode, it will NOT be taken into account")] private float m_mapZoom = 4f;
    
    [SerializeField] [Tooltip("The real size of the bowl, in meters")] private float m_mapRealSize = 500;
    
    [Header("UI elements")]
    [SerializeField] [Tooltip("The object of the map that should move alongside the player")] private RectTransform m_mapElement = null;
    [SerializeField] [Tooltip("The object of the player's character on UI")] private RectTransform m_playerElement = null;
    [SerializeField] [Tooltip("The object of the VR player on UI\nMust be child of the map")] private RectTransform m_vrPlayerElement = null;
    [SerializeField] [Tooltip("The prefab of an UI elevator\nMust have a Rect Transform")] private GameObject m_uiElevatorPrefab = null;

    private void Start() {
        
        ClientManager.OnReceiveInitialData += PlaceIcons;
    }

    private void PlaceIcons(InitialData p_initialData) {
        
        foreach (Vector3 elevatorPos in SynchroniseElevators.Instance.elevatorPositions) {
            
        }
    }

    // Update is called once per frame
    void Update() {
        
        //TODO : Can optimize it if I remove the playerPositionRatio calculation step but it'd be less clear
        Vector3 playerPosition = SynchronizePlayerPosition.Instance.m_player.position;
        Vector2 playerPositionRatio = new Vector2(((playerPosition.x / (m_mapRealSize/2f)) + 1f) / 2f, ((playerPosition.z / (m_mapRealSize/2f)) + 1f) / 2f); //This line gives a vector2 with both x and y being between 0 and 1 and corresponds to how far the player is from the center (0.5 is the center, 0 is the farthest on one side and 1 is the farthest on the opposite side)

        m_mapElement.anchorMin = new Vector2((playerPositionRatio.x * m_mapZoom) - (m_mapZoom / 2f), (playerPositionRatio.y * m_mapZoom) - (m_mapZoom / 2f));
        m_mapElement.anchorMax = new Vector2((playerPositionRatio.x * m_mapZoom) + (m_mapZoom / 2f), (playerPositionRatio.y * m_mapZoom) + (m_mapZoom / 2f));
        
        //m_mapElement.rect.center = new Vector2(playerPositionRatio)

        m_playerElement.localRotation = Quaternion.Euler(0, 0, -SynchronizePlayerPosition.Instance.m_player.rotation.eulerAngles.y + 180f);

        Transform vrTransform = SynchronizeVrTransforms.Instance.m_headVR;
        Vector3 headVRPosition = vrTransform.position;
        m_vrPlayerElement.localPosition = new Vector2(headVRPosition.x / (m_mapRealSize/2f), headVRPosition.z / (m_mapRealSize/2f));
        m_vrPlayerElement.localRotation = Quaternion.Euler(0, 0, -vrTransform.rotation.eulerAngles.y + 180f);
        
        //Debug.Log($"map size : {}", this);

    }
}
