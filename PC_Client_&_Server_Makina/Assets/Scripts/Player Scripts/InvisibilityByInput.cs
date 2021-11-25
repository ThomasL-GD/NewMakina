using CustomMessages;
using Mirror;
using TMPro;
using UnityEngine;

public class InvisibilityByInput : MonoBehaviour {

    [Tooltip("The key the player will have to hold to turn invisible")]
    [SerializeField] private KeyCode m_inputInvisibility = KeyCode.Mouse1;
    
    [Tooltip("The up time of invisibility available to the player")]
    [SerializeField] private float m_invisibilityTime = 10f;
    
    [Tooltip("The TextMeshPro element that will contain the feedback for the player's invisibility")]
    [SerializeField] private TextMeshProUGUI m_timerText;

    ///<summary/> The variable that will conserve the original value of m_invisibilityTime 
    private float m_invisibilityTimeSave;
    
    ///<summary/> The bool that keeps the player's invisibility status 
    private bool m_invisible = false;

    /// <summary>
    /// Update is called before Start()
    /// It is used to add timer reset functions to the OnReceiveHeartBreak & OnReceiveLaser delegates
    /// </summary>
    private void Awake()
    {
        ClientManager.OnReceiveHeartBreak += ResetInvisibilityOnReceiveHeartBreak;
        ClientManager.OnReceiveLaser += ResetInvisibilityOnReceiveLaser;
    }

    /// <summary>
    /// The function called to reset the player's invisibility timer OnReceiveHeartBreak
    /// </summary>
    /// <param name="p_heartBreak"> The HeartBreak Component </param>
    void ResetInvisibilityOnReceiveHeartBreak(HeartBreak p_heartBreak)
    {
        m_invisibilityTime = m_invisibilityTimeSave;
        m_timerText.text = $"Invisibility {(int) m_invisibilityTime} / {m_invisibilityTimeSave}";
    }
    
    /// <summary>
    /// The function called to reset the player's invisibility timer when he get's eliminated
    /// </summary>
    /// <param name="p_laser"> The Laser Component </param>
    private void ResetInvisibilityOnReceiveLaser(Laser p_laser)
    {
        if (!p_laser.hit) return; //Checking if the Laser actually hit the player
        
        m_invisibilityTime = m_invisibilityTimeSave;
        m_timerText.text = $"Invisibility {(int) m_invisibilityTime} / {m_invisibilityTimeSave}";
    }


    /// <summary>
    /// Update is called before the first frame
    /// </summary>
    private void Start()
    {
        // Saving the original timer value
        m_invisibilityTimeSave = m_invisibilityTime;
        
        // Resetting the timer's text
        m_timerText.text = $"Invisibility {(int) m_invisibilityTime} / {m_invisibilityTimeSave}";
    }
    
    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {
        // Checking if the timer has expired
        if (m_invisibilityTime<0f)
        {
            // Checking if the player is invisible
            if(m_invisible)
            {
                // Send the message to the server
                NetworkClient.Send(new PcInvisibility() {isInvisible = false});
                
                // Setting invisibility to false
                m_invisible = false;
            }
            return;
        }
        
        // Checking if the player has the key held down
        if (Input.GetKey(m_inputInvisibility))
        {
            // Counting down th timer
            m_invisibilityTime -= Time.deltaTime;
            
            // Resetting the timer
            m_timerText.text = $"Invisibility {(int) m_invisibilityTime} / {m_invisibilityTimeSave}";
        }
        
        // Checking if this is the frame that the player has pressed down the invisibility key
        if(Input.GetKeyDown(m_inputInvisibility))
        {
            // Setting invisibility to true
            m_invisible = true;
            
            // Send the message to the server
            NetworkClient.Send(new PcInvisibility() {isInvisible = true});
        }
        // Checking if this is the frame that the player has let go of the invisibility key
        else if (Input.GetKeyUp(m_inputInvisibility))
        {
            // Setting invisibility to false
            m_invisible = false;
            
            // Send the message to the server
            NetworkClient.Send(new PcInvisibility() {isInvisible = false});
        }
    }
}
