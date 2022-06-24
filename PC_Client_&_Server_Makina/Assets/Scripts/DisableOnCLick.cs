using UnityEngine;

public class DisableOnCLick : MonoBehaviour
{
    private void OnMouseDown()
    {
        gameObject.SetActive(false);
    }
}
