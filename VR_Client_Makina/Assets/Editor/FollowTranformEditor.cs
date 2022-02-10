using UnityEditor;
using UnityEngine;
using static UnityEditor.EditorGUILayout;

[CustomEditor(typeof(FollowTransform))]
public class FollowTranformEditor : Editor {
    public override void OnInspectorGUI() {
        
        base.OnInspectorGUI();
        
        SerializedProperty transformToFollow = serializedObject.FindProperty("m_transformToFollow");
        
        SerializedProperty positions = serializedObject.FindProperty("m_positions");
        SerializedProperty rotations = serializedObject.FindProperty("m_rotations");
        SerializedProperty scale = serializedObject.FindProperty("m_scale");
        
        
        SerializedProperty pX = serializedObject.FindProperty("m_pX");
        SerializedProperty pY = serializedObject.FindProperty("m_pY");
        SerializedProperty pZ = serializedObject.FindProperty("m_pZ");
        
        
        SerializedProperty rX = serializedObject.FindProperty("m_rX");
        SerializedProperty rY = serializedObject.FindProperty("m_rY");
        SerializedProperty rZ = serializedObject.FindProperty("m_rZ");
        
        
        SerializedProperty sX = serializedObject.FindProperty("m_sX");
        SerializedProperty sY = serializedObject.FindProperty("m_sY");
        SerializedProperty sZ = serializedObject.FindProperty("m_sZ");

        PropertyField(transformToFollow);
        
        if((Transform)transformToFollow.objectReferenceValue == null)  HelpBox("No Transform Serialized !", MessageType.Error);
        
        Space();

        PropertyField(positions);
        if (positions.boolValue) {
            PropertyField(pX);
            PropertyField(pY);
            PropertyField(pZ);
        }
        Space();
        
        PropertyField(rotations);
        if (rotations.boolValue) {
            PropertyField(rX);
            PropertyField(rY);
            PropertyField(rZ);
        }
        Space();
        
        PropertyField(scale);
        if (scale.boolValue) {
            PropertyField(sX);
            PropertyField(sY);
            PropertyField(sZ);
        }
        
        serializedObject.ApplyModifiedProperties();
    }
}