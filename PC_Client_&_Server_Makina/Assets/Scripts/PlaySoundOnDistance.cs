using UnityEngine;

public class PlaySoundOnDistance : MonoBehaviour {

    [SerializeField, Tooltip("The source of the sound")] private AudioSource m_audioSource;
    [SerializeField, Tooltip("The audioclips you want to be played\nOne will be taken randomly in the list each time a sound has to be played")] private AudioClip[] m_audioClip;
    [SerializeField, Tooltip("The range at which a sound shall start playing"), Range(1f, 100f)] private float m_distance = 50f;
    [SerializeField, Tooltip("If false, will only play a sound when the player enters the zone.\nIf true, will play a sound both when they enter and leave.")] private bool m_shouldPlayBothWays = false;
    private bool m_isInDistance = false;
    
    
}
