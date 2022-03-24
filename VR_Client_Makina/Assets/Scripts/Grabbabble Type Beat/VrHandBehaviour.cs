using Animation.AnimationDelegates;
using JetBrains.Annotations;
using UnityEngine;

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class VrHandBehaviour : MonoBehaviour {
    
    [SerializeField] private OVRInput.Axis1D m_grabInput = OVRInput.Axis1D.PrimaryHandTrigger;
    
    [SerializeField] [Range(0.01f,1f)]/**/ private float m_triggerGrabSensitivity = 0.2f;
    //[SerializeField] [Range(0.01f,1f)]/**/ private float m_triggerLetGoSensitivity = 0.9f;

    public AnimationBoolChange OnGrabItemChange;
    [CanBeNull] public GrabbableObject m_objectHeld {
        get => _objectHeld;
        private set {
            if (value == _objectHeld) return;

            OnGrabItemChange?.Invoke(IsClosed, value != null);
            _objectHeld = value;
        }
    }
    [CanBeNull] private GrabbableObject _objectHeld = null;
    public bool isFree => m_objectHeld == null && m_isPressingTrigger;

    public AnimationBoolChange OnClosedHandChange;
    public bool m_isPressingTrigger {
        get => _isPressingTrigger;
        private set {
            if (value == _isPressingTrigger) return;

            OnClosedHandChange?.Invoke(IsClosed, value);
            _isPressingTrigger = value;
        }
    }
    private bool _isPressingTrigger = false;

    private static readonly int IsClosed = Animator.StringToHash("IsClosed");
    private static readonly int IsGrabbing = Animator.StringToHash("IsGrabbing");
    public static int s_layer = -1;

    private void Start() {
        
#if UNITY_EDITOR
        //if (m_triggerGrabSensitivity < m_triggerLetGoSensitivity) Debug.LogWarning("What the fuck is that balancing ?!", this);
        if (s_layer != -1 && s_layer != gameObject.layer) Debug.LogError($"Eyo ! Both hands have different layers ?! Or there is more than two hands ???? Anyway, i'll consider {gameObject.layer} as the last correct layer", this);
#endif
        
        s_layer = gameObject.layer;
    }

    private void Update() {
        m_isPressingTrigger = OVRInput.Get(m_grabInput) >= m_triggerGrabSensitivity; //if the trigger is pressed enough, the boolean becomes true

        if (m_objectHeld == null) return; //We keep going if an item is held

        if (m_isPressingTrigger) return; //We keep going if the trigger is NOT PRESSED anymore
        
        m_objectHeld.BeLetGo(m_grabInput);
        m_objectHeld = null;
    }

    /// <summary>Will attach an object to this hand</summary>
    /// <param name="p_whoAmIEvenCatching">The object which is being grabbed by this hand</param>
    public void Catch(GrabbableObject p_whoAmIEvenCatching) {
#if UNITY_EDITOR
        if(m_objectHeld != null) Debug.LogWarning($"The object {m_objectHeld.gameObject.name} is already in this hand but i'm supposed to grab {p_whoAmIEvenCatching.gameObject.name} so what do i do ?!", this);
#endif
        m_objectHeld = p_whoAmIEvenCatching;
        p_whoAmIEvenCatching.BeGrabbed(transform);
    }
}