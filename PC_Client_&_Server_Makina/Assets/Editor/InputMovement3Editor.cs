using ICSharpCode.NRefactory.Ast;
using UnityEditor;
using UnityEngine;
using static UnityEditor.EditorGUILayout;

[CustomEditor(typeof(InputMovement3))]
public class InputMovement3Editor : Editor
{
    Material m_mat;
    bool showInputMetrics;
    bool showInputCurves;
    bool showLookMetrics;
    bool showGravitySlope;
    bool showJump;
    bool showHeadBob;
    bool showInfo;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        // Fetching the serialized properties
        // Input Metrics
        SerializedProperty movementSpeed = serializedObject.FindProperty("m_maxMovementSpeed");
        SerializedProperty accelerationTime = serializedObject.FindProperty("m_accelerationTime");
        SerializedProperty decelerationTime = serializedObject.FindProperty("m_decelerationTime");
        SerializedProperty sustainDirectionChangeSpeed = serializedObject.FindProperty("m_sustainDirectionChangeSpeed");

        // Acceleration Curves
        SerializedProperty accelerationCurve = serializedObject.FindProperty("m_accelerationBehaviorCurve");
        SerializedProperty decelerationCurve = serializedObject.FindProperty("m_decelerationBehaviorCurve");
        SerializedProperty curvePositionX = serializedObject.FindProperty("s_curvePositionX");
        
        // Look Metrics
        SerializedProperty mouseSensitivity = serializedObject.FindProperty("m_mouseSensitivity");
        SerializedProperty lockCursor = serializedObject.FindProperty("m_lockCursor");
        
        //Gravity & Slopes
        SerializedProperty gravityAcceleration = serializedObject.FindProperty("m_gravityAcceleration");
        SerializedProperty slideAcceleration = serializedObject.FindProperty("m_slideAcceleration");
        SerializedProperty maxSlideSpeed = serializedObject.FindProperty("m_maxSlideSpeed");
        SerializedProperty maxFallSpeed = serializedObject.FindProperty("m_maxFallSpeed");
        SerializedProperty groundCheckRange = serializedObject.FindProperty("m_groundCheckRange");
        SerializedProperty maxSlope = serializedObject.FindProperty("m_maxSlope");
        SerializedProperty slideDeceleration = serializedObject.FindProperty("m_slideDeceleration");
        
        //Jump
        SerializedProperty playerJumpHeight = serializedObject.FindProperty("m_playerJumpHeight");
        SerializedProperty playerJumpToleranceTime = serializedObject.FindProperty("m_playerJumpToleranceTime");
        SerializedProperty jumpUsingGroundNormal = serializedObject.FindProperty("m_jumpUsingGroundNormal");
        SerializedProperty jumpOnSlope = serializedObject.FindProperty("m_jumpOnSlope");
        SerializedProperty jumpOnSlopeUsingGroundNormal = serializedObject.FindProperty("m_jumpOnSlopeUsingGroundNormal");
        SerializedProperty jumpKey = serializedObject.FindProperty("m_jumpKey");
        
        //Head Bob
        SerializedProperty headBob = serializedObject.FindProperty("m_headBob");
        SerializedProperty headBobSpeed = serializedObject.FindProperty("m_headBobSpeed");
        SerializedProperty headBobIntensity = serializedObject.FindProperty("m_headBobIntensity");
        SerializedProperty headBobAnimationCurve = serializedObject.FindProperty("m_headBobAnimationCurve");
        SerializedProperty headBobCurvePositionX = serializedObject.FindProperty("s_headBobCurvePositionX");
        
        // Info
        SerializedProperty speed = serializedObject.FindProperty("s_speed");
        SerializedProperty gravity = serializedObject.FindProperty("m_gravity");
        SerializedProperty slideVelocity = serializedObject.FindProperty("m_slideVelocity");
        SerializedProperty movementState = serializedObject.FindProperty("m_movementState");
        SerializedProperty groundTouchingState = serializedObject.FindProperty("s_groundTouchingState");
        SerializedProperty jump = serializedObject.FindProperty("m_jump");
        SerializedProperty inputAngle = serializedObject.FindProperty("s_inputAngle");
        SerializedProperty inputVelocityAngle = serializedObject.FindProperty("s_inputVelocityAngle");
        
        // Character Controller
        SerializedProperty controller = serializedObject.FindProperty("m_controller");
        
        //Creation the foldouts
        //Input Metrics
        showInputMetrics = Foldout(showInputMetrics, "Input Metrics", true, EditorStyles.foldoutHeader);
        if (showInputMetrics)
        {
            PropertyField(movementSpeed);
            PropertyField(accelerationTime);
            PropertyField(decelerationTime);
            PropertyField(sustainDirectionChangeSpeed);
        }
        
        //Input Curves
        showInputCurves = Foldout(showInputCurves, "Input Curves", true, EditorStyles.foldoutHeader);
        if (showInputCurves)
        {
            float height = Screen.width/1.5f;
            LabelField("Acceleration Curve");
            Rect accelerationCurveContainer = GUILayoutUtility.GetRect(10f, 1000f, 220f, height);

            Color accelerationCurveColor = Color.gray;
            if (movementState.enumValueIndex == 1) accelerationCurveColor = Color.green;
            accelerationCurve.animationCurveValue = EditorGUI.CurveField(accelerationCurveContainer, accelerationCurve.animationCurveValue,accelerationCurveColor, new Rect());
            Space();
            Space();
            
            LabelField("Deceleration Curve");
            //decelerationCurve.animationCurveValue = CurveField(decelerationCurve.animationCurveValue , GUILayout.MinWidth(10f), GUILayout.Height(size));
            Rect decelerationCurveContainer = GUILayoutUtility.GetRect(10f, 1000f, 220f, height);
            
            Color decelerationCurveColor = Color.gray;
            if (movementState.enumValueIndex == 3) decelerationCurveColor = Color.green;
            decelerationCurve.animationCurveValue = EditorGUI.CurveField(decelerationCurveContainer ,decelerationCurve.animationCurveValue,decelerationCurveColor,new Rect());

            if (Event.current.type == EventType.Repaint)
            {
                float curvePosition = curvePositionX.floatValue;
                if(movementState.enumValueIndex == 1){
                    DrawLine(accelerationCurveContainer, new Vector2(curvePosition, 0f), new Vector2(curvePosition, 1f),
                        Color.red);
                }

                if (movementState.enumValueIndex == 3)
                {
                    curvePosition = -curvePosition + 1f;
                    DrawLine(decelerationCurveContainer, new Vector2(curvePosition, 0f), new Vector2(curvePosition, 1f),
                        Color.red);
                }
            }
        }
        
        //Look Metrics
        showLookMetrics = Foldout(showLookMetrics, "Look Metrics", true,EditorStyles.foldoutHeader);
        if (showLookMetrics)
        {
            PropertyField(mouseSensitivity);
            PropertyField(lockCursor);
        }
        
        //Gravity & slopes
        showGravitySlope = Foldout(showGravitySlope, "Gravity & Slopes", true,EditorStyles.foldoutHeader);
        if (showGravitySlope)
        {
            PropertyField(gravityAcceleration);
            PropertyField(slideAcceleration);
            PropertyField(maxSlideSpeed);
            PropertyField(maxFallSpeed);
            PropertyField(groundCheckRange);
            PropertyField(maxSlope);
            PropertyField(slideDeceleration);
        }

        //Jump
        showJump= Foldout(showJump, "Jump", true,EditorStyles.foldoutHeader);
        if (showJump)
        {
            PropertyField(jumpKey);
            Space();
            PropertyField(playerJumpHeight);
            PropertyField(playerJumpToleranceTime);
            PropertyField(jumpUsingGroundNormal);
            PropertyField(jumpOnSlope);
            if(jumpOnSlope.boolValue) PropertyField(jumpOnSlopeUsingGroundNormal);
        }
        
        // Head Bob
        showHeadBob= Foldout(showHeadBob, "Head Bob", true,EditorStyles.foldoutHeader);
        if (showHeadBob)
        {
            PropertyField(headBob);
            if (headBob.boolValue)
            {
                PropertyField(headBobSpeed);
                PropertyField(headBobIntensity);
                
                float height = Screen.width/1.5f;
                Rect headBobCurveContainer = GUILayoutUtility.GetRect(10f, 1000f, 220f, height);

                bool active = movementState.enumValueIndex != 0 && groundTouchingState.enumValueIndex == 0;
                
                              Color headBobCurveColor = Color.gray;
                if (active ) headBobCurveColor = Color.green;
                
                headBobAnimationCurve.animationCurveValue = EditorGUI.CurveField(headBobCurveContainer, headBobAnimationCurve.animationCurveValue,headBobCurveColor, new Rect());

                if (active)
                {
                    float curvePosition = headBobCurvePositionX.floatValue;
                    DrawLine(headBobCurveContainer, new Vector2(curvePosition, 0f), new Vector2(curvePosition, 1.5f), Color.red);
                }
            }
        }

        //Info
        showInfo = Foldout(showInfo, "Info", true,EditorStyles.foldoutHeader);
        if (showInfo)
        {
            LabelField($"Input speed : {speed.floatValue.ToString()} m/s");
            LabelField($"Falling speed : {gravity.vector3Value.y.ToString()} m/s");
            LabelField($"Sliding speed : {slideVelocity.floatValue.ToString()} m/s");
            LabelField($"Acceleration State : {movementState.enumNames[movementState.enumValueIndex]}");
            LabelField($"Ground Touching State : {groundTouchingState.enumNames[groundTouchingState.enumValueIndex]}");
            LabelField(jump.boolValue? "Won't Jump" :"Will Jump" );
            Space();
            GUIStyle s = new GUIStyle(EditorStyles.boldLabel);
            s.normal.textColor = Color.green;
            
            LabelField("Movement direction",s);
            s.normal.textColor = Color.blue;
            LabelField("Input direction",s);
            
            Rect rect = GUILayoutUtility.GetRect (10f,1000f, 220f,Screen.width/1.5f);
            
            // Drawing lines based of the player's acceleration
            if (Event.current.type == EventType.Repaint)
            {
                m_mat.SetPass(0);
                GUI.BeginClip(rect);
         
                GL.Begin(GL.QUADS);
                GL.Color(Color.grey);

                GL.Vertex3(0f, 0f, 0);
                GL.Vertex3(rect.width, 0f, 0);
                GL.Vertex3(rect.width, rect.height, 0);
                GL.Vertex3(0, rect.height, 0);
        
                GL.End();
                GUI.EndClip();
                
                Color color = Color.blue;
                DrawLineInCenterFromAngle(rect, inputAngle.floatValue, color);
                
                color = Color.green;
                DrawLineInCenterFromAngle(rect, inputVelocityAngle.floatValue, color, .65f);
            }
        }
        
        PropertyField(controller);
        serializedObject.ApplyModifiedProperties();
        Repaint();
    }

    /// <summary/> Function to draw a line from the center of the rectangle based of an angle
    /// <param name="p_rect"> the rect object that will contain the line </param>
    /// <param name="p_angle"> the angle of the line</param>
    /// <param name="p_color"> the color of the line </param>
    /// <param name="p_length"> the length of the line based on the height of the rect /2 </param>
    private void DrawLineInCenterFromAngle(Rect p_rect, float p_angle, Color p_color, float p_length = .9f)
    {
        GUI.BeginClip(p_rect);
         
        GL.Begin(GL.LINES);
        GL.Color(p_color);

        float angle = Mathf.Deg2Rad * p_angle;

        
        Vector2 center = new Vector2(p_rect.width / 2f, p_rect.height/2);
        float length = center.y * p_length;
            
        float x = center.x + length * Mathf.Sin(angle);
        float y = center.y - length * Mathf.Cos(angle);
                
        GL.Vertex3(center.x, center.y, 0);
        GL.Vertex3(x, y, 0);
        
        GL.End();
        GUI.EndClip();
    }

    /// <summary/> Draws a line from a point A to a point B
    /// <param name="p_rect"> the container of the lines </param>
    /// <param name="p_A"> The A position coordinates from 0 to 1 </param>
    /// <param name="p_B"> The B position coordinates from 0 to 1 </param>
    /// <param name="p_color"> The color of the line </param>
    private void DrawLine(Rect p_rect, Vector2 p_A, Vector2 p_B, Color p_color)
    {
        GUI.BeginClip(p_rect);
         
        GL.Begin(GL.LINES);
        GL.Color(p_color);

        GL.Vertex3(p_A.x * p_rect.width, p_A.y * p_rect.height, 0);
        GL.Vertex3(p_B.x * p_rect.width, p_B.y * p_rect.height, 0);
        
        GL.End();
        GUI.EndClip();
    }
    
    private void OnEnable()
    {
        var shader = Shader.Find("Hidden/Internal-Colored");
        m_mat = new Material(shader);
    }
}