using System.Runtime.CompilerServices;
using CustomMessages;
using Mirror;
using TMPro;
using UnityEngine;

public class InvisibilityByInput : MonoBehaviour {

    [Tooltip("The key the player will have to hold to turn invisible"),SerializeField]
    private KeyCode m_inputInvisibility = KeyCode.Mouse1;
    
    [Tooltip("The up time of invisibility available to the player"),SerializeField]
    private float m_invisibilityTime = 10f;
    
    [Tooltip("The TextMeshPro element that will contain the feedback for the player's invisibility"),SerializeField]
    private TextMeshProUGUI m_timerText;

    [Tooltip("The TextMeshPro element that will contain the feedback for the player's invisibility"), SerializeField]
    private float m_speedMultiplier = 1.5f;

    public static float m_maxSpeedMultipler = 1f;
    
    ///<summary/> The variable that will conserve the original value of m_invisibilityTime 
    private float m_invisibilityTimeSave;
    
    ///<summary/> The bool that keeps the player's invisibility status 
    private bool m_invisible = false;

    public bool m_canTurnInvisible = true;
    
    /// <summary>
    /// Update is called before Start()
    /// It is used to add timer reset functions to the OnReceiveHeartBreak & OnReceiveLaser delegates
    /// </summary>
    private void Awake()
    {
        ClientManager.OnReceiveHeartBreak += ResetInvisibilityOnReceiveHeartBreak;
        ClientManager.OnReceiveLaser += ResetInvisibilityOnReceiveAttack;
        ClientManager.OnReceiveBombExplosion += ResetInvisibilityOnReceiveAttack;
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
    private void ResetInvisibilityOnReceiveAttack(Laser p_laser)
    {
        if (!p_laser.hit) return; //Checking if the Laser actually hit the player
        
        m_invisibilityTime = m_invisibilityTimeSave;
        m_timerText.text = $"Invisibility {(int) m_invisibilityTime} / {m_invisibilityTimeSave}";
    }
    
    /// <summary>
    /// The function called to reset the player's invisibility timer when he get's eliminated
    /// </summary>
    /// <param name="p_bomb"> The Laser Component </param>
    private void ResetInvisibilityOnReceiveAttack(BombExplosion p_bomb)
    {
        if (!p_bomb.hit) return; //Checking if the Laser actually hit the player
        
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
        if (!m_canTurnInvisible) return;
        
        // Checking if the timer has expired
        if (m_invisibilityTime<0f)
        {
            // Checking if the player is invisible
            if(m_invisible) SendInvisibility(false);
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
            SendInvisibility(true);
            return;
        }
        
        // Checking if this is the frame that the player has let go of the invisibility key
        if (Input.GetKeyUp(m_inputInvisibility)) SendInvisibility(false);
    }
    private void SendInvisibility(bool p_isInvisible)
    {
            
        // Setting invisibility to false
        m_invisible = p_isInvisible;
        m_maxSpeedMultipler = p_isInvisible?m_speedMultiplier:1f;
            
        // Send the message to the server
        NetworkClient.Send(new PcInvisibility() {isInvisible = p_isInvisible});
    }
}
