using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class GrabbableObject : MonoBehaviour {
    
    protected bool m_isCaught = false;
    protected bool m_hasBeenCaughtInLifetime = false;
    [HideInInspector] public bool m_isGrabbable = true; 

    protected bool m_isPuttableOnlyOnce = false; // If true, once this object is let go somewhere, it can NOT be picked up again

    public delegate void DestroyGrabbableDelegator(GrabbableObject p_grabbableObject);
    /// <summary>Is called when this object will be destroyed</summary>
    public DestroyGrabbableDelegator OnDestroyGrabbable;

    // Start is called before the first frame update
    protected virtual void Start() {
        if(!(gameObject.layer == 0 || gameObject.layer == 6)) {
            GameObject go = gameObject;
            Debug.LogWarning($"This object was on a weird layer ( {go.layer} ), this code will AUTOMATICALLY put it back to the right layer.\nIf you wanted to mess with the layers of this objects, contact Blue.", go);
        }

        gameObject.layer = 6; //The sixth layer is the one for Catchable Objects
    }

    /// <summary>
    /// Will destroy this object correctly, don't use plain Destroy on this
    /// </summary>
    public void DestroyMaSoul() {
        OnDestroyGrabbable?.Invoke(this);
        BeingDestroyed();
        Destroy(gameObject);
    }
    
    /// <summary> This is called at the end of DestroyMaSoul but before the gameobject gets destroyed </summary>
    /// <remarks> Override this to do code after the call of OnDestroyGrabbable but before the destruction of this gameobject </remarks>
    protected virtual void BeingDestroyed(){}

    /// <summary>
    /// Will change the parent of this object to the latest correct one
    /// </summary>
    public virtual void ActualiseParent() {
        transform.SetParent(m_originalParent[m_originalParent.Count - 1]);

        //If there is only one cell in the array, it means the object is back to its original parent and thus, is not grabbed
        if (m_originalParent.Count <= 1) m_isCaught = false;
        else if (m_originalParent.Count > 1) {
            m_isCaught = true;
            m_hasBeenCaughtInLifetime = true;
        }


        if (!m_isPuttableOnlyOnce) return;
        
        if (m_hasBeenCaughtInLifetime && !m_isCaught) {
            m_isGrabbable = false;
        }
    }

    private void OnTriggerStay(Collider p_other) {
        if (p_other.gameObject.layer == VrHandBehaviour.s_layer && p_other.TryGetComponent(out VrHandBehaviour script)) {
            if (script.isFree) {
                script.Catch(this);
            }
        }
    }
}
