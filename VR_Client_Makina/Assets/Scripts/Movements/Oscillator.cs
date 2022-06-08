using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Oscillator : MonoBehaviour
{
    public enum Axe
    {
        X,
        Y,
        Z
    }

    public Axe axe;

    [Range(0.0f, 100f)]
    public float speed = 1f;
    [Range(0.0f, 100f)]
    public float amplitude = 1f;
    [Range(0.0f, 1f)]
    public float phase = 0f;
    // Start is called before the first frame update

    private Vector3 initialPosition;

    private float totalTime = 0;
    void Start()
    {
        initialPosition = this.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        float oscillation = Mathf.Cos(totalTime + (phase*(Mathf.PI*2))) * amplitude;
        Vector3 targetPosition;
        switch (axe)
        {
            case Axe.X:
                targetPosition = new Vector3(initialPosition.x + oscillation, initialPosition.y, initialPosition.z);
                break;
            case Axe.Y:
                targetPosition = new Vector3(initialPosition.x, initialPosition.y + oscillation, initialPosition.z);
                break;
            default:
                targetPosition = new Vector3(initialPosition.x, initialPosition.y, initialPosition.z + oscillation);
                break;
        }

        
        this.transform.position = targetPosition;

        totalTime += (Time.deltaTime*speed);
    }
}
