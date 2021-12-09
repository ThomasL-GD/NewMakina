using System.Collections;
using System.Collections.Generic;
using Synchronizers;
using UnityEngine;

public class BombBehavior : GrabbablePhysickedObject {

    private ObjectLoading m_bombLoading = null;
    public SynchronizeBombs m_synchronizer = null;

    protected override void Start() {
        base.Start();
        //m_isPuttableOnlyOnce = true;

        if (gameObject.TryGetComponent(out ObjectLoading script)) {
            m_bombLoading = script;
        }
        else {
            Debug.LogWarning("No ObjectLoading script attached on this object but don't worry i gotchu fam ( ͡° ͜ʖ ͡°)", this);
            m_bombLoading = gameObject.AddComponent<ObjectLoading>();
        }

        //m_bombLoading.m_synchronizer = m_synchronizer;
        m_synchronizer.LoadBombRandomly(m_bombLoading);
    }

    protected override void OnFirstTimeTouchingGround() {
        base.OnFirstTimeTouchingGround();
        
        m_synchronizer.ExplodeLol(transform.position);
    }
}
