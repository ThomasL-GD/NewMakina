using System;
using UnityEngine;

public class BitmaskExperimentation : MonoBehaviour
{
    private int m_bitmask;

    [ContextMenu("test")]
    void FunctionToDoStuffWith()
    {
        m_bitmask = 0b00000000000;
        int m_bitmask2 = 0b00000000010;

        Debug.Log(Convert.ToString (m_bitmask, 2));
        m_bitmask |= 1 << 5;
        Debug.Log(Convert.ToString (m_bitmask, 2));
        m_bitmask |= 1 << 6;
        Debug.Log(Convert.ToString (m_bitmask, 2));
        m_bitmask &= ~(1 << 5);
        Debug.Log(Convert.ToString (m_bitmask, 2));
        
        Debug.Log((m_bitmask & 1<<5) == 0 && (m_bitmask & 1<<2) == 0);
    }
}
