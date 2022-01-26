using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GrabbableDropDownObject : GrabbableObject {
    /// <summary> Becomes true once this object touches the ground for the first time </summary>
    private bool m_hasTouchedGround = false;

    private Rigidbody m_rb;

    /// <summary> The layers this object will collide with, only layer 8 (ground) is selected by default </summary>
    private LayerMask m_layersThatCollides = 1 << 8;

    public float m_speed = 10f;

    protected override void Start() {
        base.Start();
        m_isPuttableOnlyOnce = true; // We ensure the object cannot be picked up again

        m_rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision p_other) {
        if (((1 << p_other.gameObject.layer) & m_layersThatCollides) == 0) return; //If this object collides with any layer non-specified with m_layersThatCollides, returns
        transform.rotation = Quaternion.Euler(0, 0, 0);
        if (!m_hasTouchedGround) OnFirstTimeTouchingGround(p_other);
    }

    /// <summary> Is called the first time this object touches the ground </summary>
    /// <remarks> Override this to have things done at this moment </remarks>
    protected virtual void OnFirstTimeTouchingGround(Collision p_other) {
        m_hasTouchedGround = true;
    }

    public override void ActualiseParent() {
        base.ActualiseParent();

        if (!m_isCaught && m_hasBeenCaughtInLifetime) {
            StartCoroutine(DroppingDown());
        }
    }


    /// <summary> Will make this object go down until it touches anything </summary>
    IEnumerator DroppingDown() {
        while (!m_hasTouchedGround) {
            m_rb.MovePosition(transform.position + Vector3.down * (m_speed / Time.fixedDeltaTime));

            yield return new WaitForFixedUpdate();
        }
    }
}