using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class PhysicsGrabScaleTestScriptSkibidip : GrabbablePhysickedObject {

    [SerializeField] [Range(2, 50)] private int m_unSliderDInt = 7;
    
    // Start is called before the first frame update
    protected override void Start() {
        
        base.Start();

            //We just get a random number of values stored and change the color according to that
        int rand = Random.Range(2, 50);

        m_lastCoordinates = new AmputatedTransform[rand];

        float frand = (rand - 2) / 48f;
        Color grayscale = new Color(frand, frand, frand);
        GetComponent<MeshRenderer>().material.color = grayscale;

    }

    /// <summary>Fuck you </summary>
    public override void ActualiseParent() {
        base.ActualiseParent();

        if (!m_rb.isKinematic) {
            Debug.DrawLine(m_lastCoordinates[m_lastCoordinates.Length - 1].position, m_lastCoordinates[0].position, Color.cyan, 15f);
        }
    }

    [ContextMenu("Vegeta no")]
    private void VegetaYes() {
        m_lastCoordinates = new AmputatedTransform[m_unSliderDInt];
        GetComponent<MeshRenderer>().material.color = new Color((m_unSliderDInt - 2) / 48f, (m_unSliderDInt - 2) / 48f, (m_unSliderDInt - 2) / 48f);
        transform.position = new Vector3(0f, 50f, 0f);
    }
}