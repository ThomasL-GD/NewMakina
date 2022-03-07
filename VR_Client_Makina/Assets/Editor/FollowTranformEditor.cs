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

        EditorGUIUtility.labelWidth = 60f;
        PropertyField(positions);
        if (positions.boolValue) {
            BeginHorizontal("positions");
            EditorGUIUtility.labelWidth = 20f;
            //EditorGUI.indentLevel += 2;
            PropertyField(pX);
            PropertyField(pY);
            PropertyField(pZ);
            //EditorGUI.indentLevel -= 2;
            EndHorizontal();
        }
        Space();
        
        EditorGUIUtility.labelWidth = 60f;
        PropertyField(rotations);
        if (rotations.boolValue) {
            BeginHorizontal("rotations");
            EditorGUIUtility.labelWidth = 20f;
            //EditorGUI.indentLevel += 2;
            PropertyField(rX);
            PropertyField(rY);
            PropertyField(rZ);
            //EditorGUI.indentLevel -= 2;
            EndHorizontal();
        }
        Space();
        
        EditorGUIUtility.labelWidth = 60f;
        PropertyField(scale);
        if (scale.boolValue) {
            BeginHorizontal("scale");
            EditorGUIUtility.labelWidth = 20f;
            //EditorGUI.indentLevel += 2;
            PropertyField(sX);
            PropertyField(sY);
            PropertyField(sZ);
            //EditorGUI.indentLevel -= 2;
            EndHorizontal();
        }
        
        serializedObject.ApplyModifiedProperties();
    }
}