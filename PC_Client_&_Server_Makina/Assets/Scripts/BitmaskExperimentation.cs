using System;
using UnityEngine;

public class BitmaskExperimentation : MonoBehaviour
{
    private int m_bitmask;

    [ContextMenu("test")]
    void FunctionToDoStuffWith()
    {
        m_bitmask = 0b00000000000;

        m_bitmask |= 1 << 5;
        m_bitmask |= 1 << 9;
        m_bitmask |= 1 << 8;
        m_bitmask &= ~(1 << 8);
        
        Debug.Log(Convert.ToString (m_bitmask, 2));
        
    }
}
