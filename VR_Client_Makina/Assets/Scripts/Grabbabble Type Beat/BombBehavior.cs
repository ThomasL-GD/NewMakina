using System.Collections;
using System.Collections.Generic;
using Synchronizers;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class BombBehavior : GrabbablePhysickedObject {

    private ObjectLoading m_bombLoading = null;
    [HideInInspector] public SynchronizeBombs m_synchronizer = null;

    [HideInInspector] public int m_index;
    [HideInInspector] public float m_serverID;

    public delegate void DestroyBombDelegator(BombBehavior p_bombBehavior);
    private static DestroyBombDelegator OnDestroyBomb;

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

        m_bombLoading.m_synchronizer = m_synchronizer;
        m_synchronizer.LoadBombRandomly(m_bombLoading);

        OnDestroyBomb += ActualiseIndex;
    }

    protected override void OnFirstTimeTouchingGround() {
        base.OnFirstTimeTouchingGround();
        
        m_synchronizer.ExplodeLol(m_index, m_serverID);
            
        OnDestroyBomb?.Invoke(this);
        Destroy(gameObject);
    }

    /// <summary> </summary>
    /// <param name="p_grabbableObject"></param>
    private void ActualiseIndex(BombBehavior p_grabbableObject) {
        if (m_index > p_grabbableObject.m_index) m_index--;
    }
}
