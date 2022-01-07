using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(InputMovement3))]
public class InputMovement3Editor : Editor
{
    Material mat;
    protected static bool showInputMetrics = false;
    protected static bool showLookMetrics = false;
    protected static bool showInputInfo = false;
    public override void OnInspectorGUI()
    {
        EditorGUILayout.LabelField("Yeaaaaa Boiiiii");
        
        SerializedProperty controler = serializedObject.FindProperty("m_controler");
        
        // Input Metrics
        SerializedProperty movementSpeed = serializedObject.FindProperty("m_movementSpeed");
        SerializedProperty accelerationSpeed = serializedObject.FindProperty("m_accelerationSpeed");
        SerializedProperty decelerationSpeed = serializedObject.FindProperty("m_decelerationSpeed");
        SerializedProperty sustainDirectionChangeSpeed = serializedObject.FindProperty("m_sustainDirectionChangeSpeed");

        // Look Metrics
        SerializedProperty mouseSensitivity = serializedObject.FindProperty("m_mouseSensitivity");
        SerializedProperty lockCursor = serializedObject.FindProperty("m_lockCursor");
        
        // Info
        SerializedProperty speed = serializedObject.FindProperty("s_speed");
        SerializedProperty movementState = serializedObject.FindProperty("m_movementState");
        
        SerializedProperty inputAngle = serializedObject.FindProperty("s_inputAngle");
        SerializedProperty inputVelocityAngle = serializedObject.FindProperty("s_inputVelocityAngle");
        SerializedProperty lookAngle = serializedObject.FindProperty("s_lookAngle");

        showInputMetrics = EditorGUILayout.Foldout(showInputMetrics, "Input Metrics", true, EditorStyles.boldLabel);
        if (showInputMetrics)
        {
            EditorGUILayout.PropertyField(movementSpeed);
            EditorGUILayout.PropertyField(accelerationSpeed);
            EditorGUILayout.PropertyField(decelerationSpeed);
            EditorGUILayout.PropertyField(sustainDirectionChangeSpeed);
        }

        showLookMetrics = EditorGUILayout.Foldout(showLookMetrics, "Look Metrics", true,EditorStyles.boldLabel);
        if (showLookMetrics)
        {
            EditorGUILayout.PropertyField(mouseSensitivity);
            EditorGUILayout.PropertyField(lockCursor);
        }

        showInputInfo = EditorGUILayout.Foldout(showInputInfo, "Input Info", true,EditorStyles.boldLabel);
        if (showInputInfo)
        {
            EditorGUILayout.LabelField($"Input speed : {speed.floatValue.ToString()} m/s");
            EditorGUILayout.LabelField($"State : {movementState.enumNames[movementState.enumValueIndex]}");
            
            
            Rect rect = GUILayoutUtility.GetRect(10f, 1000f, 200f, 200f);
            if (Event.current.type == EventType.Repaint)
            {
                Color color = Color.red;
                DrawLine(rect, inputAngle.floatValue, color);
                
                color = Color.Lerp(Color.green, Color.red, Mathf.Abs(inputVelocityAngle.floatValue / 180f) - Mathf.Abs(inputAngle.floatValue / 180f));
                DrawLine(rect, inputVelocityAngle.floatValue, color);
            }

        }
        
        EditorGUILayout.PropertyField(controler);
        serializedObject.ApplyModifiedProperties();
        Repaint();
    }

    private void DrawLine(Rect p_rect, float p_angle, Color color)
    {
        GUI.BeginClip(p_rect);
        mat.SetPass(0);
         
        GL.Begin(GL.LINES);
        GL.Color(color);

        float angle = Mathf.Deg2Rad * p_angle;
                
        float x = 100f + 100f * Mathf.Sin(angle);
        float y = 100f - 100f * Mathf.Cos(angle);
                
        GL.Vertex3(100f, 100f, 0);
        GL.Vertex3(x, y, 0);
        
        GL.End();
        GUI.EndClip();
    }
    
    private void OnEnable()
    {
        var shader = Shader.Find("Hidden/Internal-Colored");
        mat = new Material(shader);
    }
}