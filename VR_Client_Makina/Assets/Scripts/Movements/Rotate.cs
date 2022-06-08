using System;
using UnityEngine;

public class Rotate : MonoBehaviour {

    [Flags]
    private enum Axis {
        X = 0b0000_0001,
        Y = 0b0000_0010,
        Z = 0b0000_0100,
        None = 0b0000_0000
    }
    
    [SerializeField] private Axis m_axis = Axis.None;
    [SerializeField, Range(0f, 100f)] private float m_speed = 10f;

    // Update is called once per frame
    void Update() {
        
        transform.Rotate(m_axis.HasFlag(Axis.X) ? m_speed : 0f, m_axis.HasFlag(Axis.Y) ? m_speed : 0f, m_axis.HasFlag(Axis.Z) ? m_speed : 0f);
    }
}
