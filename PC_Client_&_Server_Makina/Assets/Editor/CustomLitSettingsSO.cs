using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomLitSettingsSO :ScriptableObject
{
    public float shadowTiling;
    public float lightIntensityMultiplier;
    public float defaultOcclusionValue;
    public float globalIllumination;
    public float triplanarBlendSharpness;
    public Vector2 smoothStepR;
    public Vector2 smoothStepG;
    public Vector2 smoothStepB;
    public Texture2D shadowHashTexture;
}
