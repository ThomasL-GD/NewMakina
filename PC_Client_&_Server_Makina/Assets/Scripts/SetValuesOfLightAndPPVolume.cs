using UnityEngine;
using UnityEngine.Rendering;

public class SetValuesOfLightAndPPVolume : MonoBehaviour
{
    [SerializeField] private Volume m_volume;
    [SerializeField] private Light m_light;
    // Start is called before the first frame update
    void OnEnable()
    {
        float scale = transform.lossyScale.x/2f;
        m_volume.blendDistance = scale;
        m_light.range = scale;
    }
}
