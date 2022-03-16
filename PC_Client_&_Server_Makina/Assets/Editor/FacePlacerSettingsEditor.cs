using System.Collections.Generic;
using Tools;
using UnityEditor;
using UnityEngine;
using static UnityEditor.EditorGUILayout;

[CustomEditor(typeof(FacePlacerSettings))]
public class FacePlacerSettingsEditor : Editor
{
    private bool m_showPrefabProbability;
    private bool m_showPrefabProbabilityBottom;
    private bool m_showPrefabProbabilityTop;
    public override void OnInspectorGUI()
    {
        SerializedProperty facadePrefab = serializedObject.FindProperty("facadePrefab");
        SerializedProperty facadePrefabTop = serializedObject.FindProperty("facadePrefabTop");
        SerializedProperty facadePrefabBottom = serializedObject.FindProperty("facadePrefabBottom");
        SerializedProperty assetWidth = serializedObject.FindProperty("assetWidth");
        SerializedProperty assetHeight = serializedObject.FindProperty("assetHeight");
        SerializedProperty placeCorners = serializedObject.FindProperty("placeCorners");
        SerializedProperty previewRotationOffset = serializedObject.FindProperty("previewRotationOffset");
        SerializedProperty cornerPrefab = serializedObject.FindProperty("cornerPrefab");
        SerializedProperty cornerOffsetY = serializedObject.FindProperty("cornerOffsetY");

        PropertyField(facadePrefab);
        PropertyField(facadePrefabTop);
        PropertyField(facadePrefabBottom);

        Space();
        PropertyField(assetWidth);
        PropertyField(assetHeight);
        Space();
        PropertyField(placeCorners);
        if(placeCorners.boolValue)
        {
            PropertyField(cornerPrefab);
            PropertyField(cornerOffsetY);
        }
        Space();
        PropertyField(previewRotationOffset);
        
        serializedObject.ApplyModifiedProperties();

        FacePlacerSettings prefab = target as FacePlacerSettings;
        
        Space();
        m_showPrefabProbability = Foldout(m_showPrefabProbability, "Prefab Probability", true, EditorStyles.foldoutHeader);
        
        prefab.houdinAllRight = prefab.facadePrefab.GetComponent<HoudinAllRight>();
        prefab.houdinAllRight.Refresh();

        for (int i = 0; i < prefab.houdinAllRight.m_children.Length; i++)
        {
            string buttonName = prefab.houdinAllRight.m_children[i].name;
            buttonName = buttonName.Replace("M_", "");
            buttonName = buttonName.Replace("P_", "");
            buttonName = buttonName.Replace('_', ' ');
            buttonName = buttonName.Replace("V ", "");
            buttonName = buttonName.Replace("O ", "");

            if (prefab.prefabPlacingProbability.Count <= i) prefab.prefabPlacingProbability.Add(.5f);
            
            if (m_showPrefabProbability) {
                LabelField(buttonName + " probability");
                prefab.prefabPlacingProbability[i] = Slider(prefab.prefabPlacingProbability[i], 0f, 1f);
                Space(8);
            }
        }

        if (prefab.prefabPlacingProbability.Count > prefab.houdinAllRight.m_children.Length)
            for (int i = prefab.prefabPlacingProbability.Count; i < prefab.prefabPlacingProbability.Count; i++)
                prefab.prefabPlacingProbability.RemoveAt(i);
        
        
        m_showPrefabProbabilityBottom = Foldout(m_showPrefabProbabilityBottom, "Prefab Probability bottom", true, EditorStyles.foldoutHeader);
        
        
        prefab.houdinAllRightBottom = prefab.facadePrefabBottom.GetComponent<HoudinAllRight>();
        prefab.houdinAllRightBottom.Refresh();

        for (int i = 0; i < prefab.houdinAllRightBottom.m_children.Length; i++)
        {
            string buttonName = prefab.houdinAllRightBottom.m_children[i].name;
            buttonName = buttonName.Replace("M_", "");
            buttonName = buttonName.Replace("P_", "");
            buttonName = buttonName.Replace('_', ' ');
            buttonName = buttonName.Replace("V ", "");
            buttonName = buttonName.Replace("O ", "");

            if (prefab.prefabPlacingProbabilityBottom.Count <= i) prefab.prefabPlacingProbabilityBottom.Add(.5f);

            if (m_showPrefabProbabilityBottom)
            {
                LabelField(buttonName + " probability");
                prefab.prefabPlacingProbabilityBottom[i] = Slider(prefab.prefabPlacingProbabilityBottom[i], 0f, 1f);
                Space(8);
            }
        }

        if (prefab.prefabPlacingProbabilityBottom.Count > prefab.houdinAllRightBottom.m_children.Length)
            for (int i = prefab.prefabPlacingProbabilityBottom.Count; i < prefab.prefabPlacingProbabilityBottom.Count; i++)
                prefab.prefabPlacingProbabilityBottom.RemoveAt(i);
        
        
        
        m_showPrefabProbabilityTop = Foldout(m_showPrefabProbabilityTop, "Prefab Probability top", true, EditorStyles.foldoutHeader);

        prefab.houdinAllRightTop = prefab.facadePrefabTop.GetComponent<HoudinAllRight>();
        prefab.houdinAllRightTop.Refresh();

        for (int i = 0; i < prefab.houdinAllRightTop.m_children.Length; i++)
        {
            string buttonName = prefab.houdinAllRightTop.m_children[i].name;
            buttonName = buttonName.Replace("M_", "");
            buttonName = buttonName.Replace("P_", "");
            buttonName = buttonName.Replace('_', ' ');
            buttonName = buttonName.Replace("V ", "");
            buttonName = buttonName.Replace("O ", "");

            if (prefab.prefabPlacingProbabilityTop.Count <= i) prefab.prefabPlacingProbabilityTop.Add(.5f);

            if (m_showPrefabProbabilityTop)
            {
                LabelField(buttonName + " probability");
                prefab.prefabPlacingProbabilityTop[i] = Slider(prefab.prefabPlacingProbabilityTop[i], 0f, 1f);
                Space(8);
            }
        }

        if (prefab.prefabPlacingProbabilityTop.Count > prefab.houdinAllRightTop.m_children.Length)
            for (int i = prefab.prefabPlacingProbabilityTop.Count; i < prefab.prefabPlacingProbabilityTop.Count; i++)
                prefab.prefabPlacingProbabilityTop.RemoveAt(i);
        
    }
}
