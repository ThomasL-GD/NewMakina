using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableAbilityOnKeyPress : MonoBehaviour
{
    [SerializeField,Tooltip("the mechanic to enable on trigger")] private AbstractMechanic m_mechanic;

    [SerializeField] private KeyCode m_key = KeyCode.E;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (Input.GetKeyDown(m_key) && m_mechanic.enabled)
        {
            m_mechanic.enabled = false;
            Destroy(this);
        }
    }
}
