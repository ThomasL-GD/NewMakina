using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsGrabScaleTestScriptSkibidip : GrabbablePhysickedObject {
    
    // Start is called before the first frame update
    protected override void Start() {
        
        base.Start();

        //We just get a random number of values stored and change the color according to that
        int rand = Random.Range(1, 21);

        m_lastCoordinates = new AmputatedTransform[rand];

        float frand = rand / 21f;
        GetComponent<MeshRenderer>().material.color = new Color(frand, frand, frand);

    }
    
}
