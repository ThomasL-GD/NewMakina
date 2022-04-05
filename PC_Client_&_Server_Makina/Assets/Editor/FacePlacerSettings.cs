using System.Collections.Generic;
using Tools;
using UnityEngine;

[CreateAssetMenu(fileName = "Facade Placer Settings", menuName = "Facade Placer Settings",order = 0)]

public class FacePlacerSettings : ScriptableObject
{
    [SerializeField,HideInInspector] public GameObject facadePrefab;
    [SerializeField,HideInInspector] public GameObject facadePrefabTop;
    [SerializeField,HideInInspector] public GameObject facadePrefabBottom;
    [SerializeField,HideInInspector] public float assetWidth = 3f;
    [SerializeField,HideInInspector] public float assetHeight = 3f;
    [SerializeField,HideInInspector] public bool placeCorners = true;
    [SerializeField,HideInInspector] public Vector3 previewRotationOffset;
    [SerializeField,HideInInspector] public GameObject cornerPrefab;
    [SerializeField,HideInInspector] public float cornerOffsetY = 0f;
    [HideInInspector] public List<float> prefabPlacingProbability;
    [HideInInspector] public List<float> prefabPlacingProbabilityBottom;
    [HideInInspector] public List<float> prefabPlacingProbabilityTop;
    [HideInInspector] public HoudinAllRight houdinAllRight;
    [HideInInspector] public HoudinAllRight houdinAllRightBottom;
    [HideInInspector] public HoudinAllRight houdinAllRightTop;
}
