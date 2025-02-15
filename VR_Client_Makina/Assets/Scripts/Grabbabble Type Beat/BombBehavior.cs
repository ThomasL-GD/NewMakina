using System.Collections;
using System.Collections.Generic;
using CustomMessages;
using Grabbabble_Type_Beat;
using Network;
using Synchronizers;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class BombBehavior : GrabbablePhysickedObject {

    private ObjectLoading m_bombLoading = null;
    [HideInInspector] public SynchronizeBombs m_synchronizer = null;

    [HideInInspector] public int m_index;
    [HideInInspector] public float m_serverID;
    [HideInInspector] public float m_explosionTimeOnceOnGround = 0f;

    private delegate void DestroyBombDelegator(BombBehavior p_bombBehavior);
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

    public override void BeGrabbed(Transform p_parent, Vector3 p_offsetPositionInHand) {

        if (!m_hasBeenCaughtInLifetime) {
            m_bombLoading.Unloading();
        }
        
        base.BeGrabbed(p_parent, p_offsetPositionInHand);
    }

    protected override void OnFirstTimeTouchingGround(Collision p_other) {
        base.OnFirstTimeTouchingGround(p_other);
        
        m_isGrabbable = false;
        m_rb.isKinematic = true;

        m_synchronizer.ChangeMaterialOfABomb(m_index, m_serverID);
        //Debug.Log("hooyeaaa");
        MyNetworkManager.singleton.SendVrData(new BombActivation(){index = m_index,bombID = m_serverID});
        
        StartCoroutine(ExplodeAfterTime());
    }

    IEnumerator ExplodeAfterTime() {
        yield return new WaitForSeconds(m_explosionTimeOnceOnGround);
        
        m_synchronizer.ExplodeLol(m_index, m_serverID);
        
        OnDestroyBomb?.Invoke(this);
        DestroyMaSoul();
        //Destroy(gameObject);
    }

    /// <summary> </summary>
    /// <param name="p_grabbableObject"></param>
    private void ActualiseIndex(BombBehavior p_grabbableObject) {
        if (m_index > p_grabbableObject.m_index) m_index--;
    }
}
