using System;
using UnityEngine;

public class ChangeMaterialValues : MonoBehaviour
{
    [Serializable]
    public class ValueToChange
    {
        public String valueToChange;
        public float value;
    }
    public void ChangeMaterialVars(ValueToChange[] p_valuesToChange)
    {
        Debug.Log(p_valuesToChange[0].valueToChange);
    }
}
