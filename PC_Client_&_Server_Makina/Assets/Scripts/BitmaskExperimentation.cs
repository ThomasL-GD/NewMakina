using System;
using UnityEngine;

public class BitmaskExperimentation : MonoBehaviour
{
    private int m_bitmask;

    [ContextMenu("test")]
    void FunctionToDoStuffWith()
    {
        m_bitmask = 0b1111111111;

        for (int i = 0; i < 7; i++)
        {
            Debug.Log(m_bitmask);
        }
        
    }
}
