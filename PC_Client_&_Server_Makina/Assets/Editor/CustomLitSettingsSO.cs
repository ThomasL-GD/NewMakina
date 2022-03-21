using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomLitSettingsSO :ScriptableObject
{
    [HideInInspector]public float shadowTiling;
    [HideInInspector]public float lightIntensityMultiplier;
    [HideInInspector]public float defaultOcclusionValue;
    [HideInInspector]public float globalIllumination;
    [HideInInspector]public float triplanarBlendSharpness;
    [HideInInspector]public Vector2 smoothStepR;
    [HideInInspector]public Vector2 smoothStepG;
    [HideInInspector]public Vector2 smoothStepB;
    [HideInInspector]public Texture shadowHashTexture;
}
