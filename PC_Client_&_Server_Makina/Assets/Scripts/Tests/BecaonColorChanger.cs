using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BecaonColorChanger : MonoBehaviour
{
    [ContextMenu("ChangeColor")]
    void ChangeColor()
    {
        Material mat = GetComponent<MeshRenderer>().material;
        mat.SetColor("_Beacon_Color", new Color(Random.Range(0f,1f),Random.Range(0f,1f),Random.Range(0f,1f))); 
    }
}
