using UnityEngine;
using CustomMessages;
using Network;

public class SyncronizeInvisibility : MonoBehaviour
{
    [SerializeField] private MeshRenderer[] m_playerMeshRenderers;

    [SerializeField] private AudioSource m_audioSource;
    [SerializeField] private AudioClip m_soundOn = null;
    [SerializeField] private AudioClip m_soundOff = null;

    private void Awake()
    {
        MyNetworkManager.OnInvisibilityUpdate += OnReceiveInvisibility;
    }

    /// <summary>
    /// Called when the server sends a PcInvisbility type message (▀̿Ĺ̯▀̿ ̿) ▄︻̷̿┻̿═━一
    /// </summary>
    /// <param name="p_pcInvisibility"> the message </param>
    void OnReceiveInvisibility(PcInvisibility p_pcInvisibility)
    {
        bool invisible = p_pcInvisibility.isInvisible;

        foreach (MeshRenderer meshRenderer in m_playerMeshRenderers)
            meshRenderer.enabled = !invisible;

        //TODO: Replace audio "player invisible" if needed 
        // m_audioSource.Stop();
        // m_audioSource.clip = invisible ? m_soundOn : m_soundOff;
        // m_audioSource.Play();
        
    }
}
