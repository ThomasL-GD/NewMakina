using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowPOsitions : MonoBehaviour {
    

    // Update is called once per frame
    void Update() {
        var transform1 = transform;
        Debug.Log($"my POSITION : {transform1.position}   LOCAL POS : {transform1.localPosition}");
    }
}
