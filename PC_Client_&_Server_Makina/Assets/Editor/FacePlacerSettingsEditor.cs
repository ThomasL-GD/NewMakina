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

        FacePlacerSettings facePlacerSettings = target as FacePlacerSettings;
        
        Space();
        m_showPrefabProbability = Foldout(m_showPrefabProbability, "Prefab Probability", true, EditorStyles.foldoutHeader);
        
        facePlacerSettings.houdinAllRight = facePlacerSettings.facadePrefab.GetComponent<HoudinAllRight>();
        facePlacerSettings.houdinAllRight.Refresh();

        
        for (int i = 0; i < facePlacerSettings.houdinAllRight.m_children.Length; i++)
        {
            string buttonName = facePlacerSettings.houdinAllRight.m_children[i].name;
            buttonName = buttonName.Replace("M_", "");
            buttonName = buttonName.Replace("P_", "");
            buttonName = buttonName.Replace('_', ' ');
            buttonName = buttonName.Replace("V ", "");
            buttonName = buttonName.Replace("O ", "");

            if (facePlacerSettings.prefabPlacingProbability.Count <= i) facePlacerSettings.prefabPlacingProbability.Add(.5f);
            
            if (m_showPrefabProbability) {
                LabelField(buttonName + " probability");
                facePlacerSettings.prefabPlacingProbability[i] = Slider(facePlacerSettings.prefabPlacingProbability[i], 0f, 1f);
                Space(8);
            }
        }

        if (facePlacerSettings.prefabPlacingProbability.Count > facePlacerSettings.houdinAllRight.m_children.Length)
            for (int i = facePlacerSettings.houdinAllRight.m_children.Length; i < facePlacerSettings.prefabPlacingProbability.Count; i++)
                facePlacerSettings.prefabPlacingProbability.RemoveAt(i);
        


        m_showPrefabProbabilityBottom = Foldout(m_showPrefabProbabilityBottom, "Prefab Probability bottom", true, EditorStyles.foldoutHeader);

        facePlacerSettings.houdinAllRightBottom = facePlacerSettings.facadePrefabBottom.GetComponent<HoudinAllRight>();
        facePlacerSettings.houdinAllRightBottom.Refresh();

        for (int i = 0; i < facePlacerSettings.houdinAllRightBottom.m_children.Length; i++)
        {
            string buttonName = facePlacerSettings.houdinAllRightBottom.m_children[i].name;
            buttonName = buttonName.Replace("M_", "");
            buttonName = buttonName.Replace("P_", "");
            buttonName = buttonName.Replace('_', ' ');
            buttonName = buttonName.Replace("V ", "");
            buttonName = buttonName.Replace("O ", "");

            if (facePlacerSettings.prefabPlacingProbabilityBottom.Count <= i) facePlacerSettings.prefabPlacingProbabilityBottom.Add(.5f);

            if (m_showPrefabProbabilityBottom)
            {
                LabelField(buttonName + " probability");
                facePlacerSettings.prefabPlacingProbabilityBottom[i] = Slider(facePlacerSettings.prefabPlacingProbabilityBottom[i], 0f, 1f);
                Space(8);
            }
        }

        if (facePlacerSettings.prefabPlacingProbabilityBottom.Count > facePlacerSettings.houdinAllRightBottom.m_children.Length)
            for (int i = facePlacerSettings.houdinAllRightBottom.m_children.Length; i < facePlacerSettings.prefabPlacingProbabilityBottom.Count; i++)
                facePlacerSettings.prefabPlacingProbabilityBottom.RemoveAt(i);
        
        
        
        
        
        
        m_showPrefabProbabilityTop = Foldout(m_showPrefabProbabilityTop, "Prefab Probability top", true, EditorStyles.foldoutHeader);

        facePlacerSettings.houdinAllRightTop = facePlacerSettings.facadePrefabTop.GetComponent<HoudinAllRight>();
        facePlacerSettings.houdinAllRightTop.Refresh();

        for (int i = 0; i < facePlacerSettings.houdinAllRightTop.m_children.Length; i++)
        {
            string buttonName = facePlacerSettings.houdinAllRightTop.m_children[i].name;
            buttonName = buttonName.Replace("M_", "");
            buttonName = buttonName.Replace("P_", "");
            buttonName = buttonName.Replace('_', ' ');
            buttonName = buttonName.Replace("V ", "");
            buttonName = buttonName.Replace("O ", "");

            if (facePlacerSettings.prefabPlacingProbabilityTop.Count <= i) facePlacerSettings.prefabPlacingProbabilityTop.Add(.5f);

            if (m_showPrefabProbabilityTop)
            {
                LabelField(buttonName + " probability");
                facePlacerSettings.prefabPlacingProbabilityTop[i] = Slider(facePlacerSettings.prefabPlacingProbabilityTop[i], 0f, 1f);
                Space(8);
            }
        }

        if (facePlacerSettings.prefabPlacingProbabilityTop.Count > facePlacerSettings.houdinAllRightTop.m_children.Length)
            for (int i = facePlacerSettings.houdinAllRightTop.m_children.Length; i < facePlacerSettings.prefabPlacingProbabilityTop.Count; i++)
                facePlacerSettings.prefabPlacingProbabilityTop.RemoveAt(i);
        
    }
}
