using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddToChildrenWhenInTrigger : MonoBehaviour
{
    [SerializeField] private LayerMask m_layersToParentise;

    // Update is called once per frame
    private void OnTriggerEnter(Collider p_other)
    {
        if (m_layersToParentise != (m_layersToParentise | (1 << p_other.gameObject.layer))) return;
        
        p_other.gameObject.transform.SetParent(transform);
    }
    private void OnTriggerExit(Collider p_other)
    {
        {
            if (m_layersToParentise != (m_layersToParentise | (1 << p_other.gameObject.layer))) return;

            p_other.gameObject.transform.SetParent(null);
        }
    }
}
