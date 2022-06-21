using UnityEngine;

public class showDevelopperConsole : MonoBehaviour
{
    
    void Update() {
        if (Input.GetKeyDown(KeyCode.P)) Debug.developerConsoleVisible = !Debug.developerConsoleVisible;
    }
}
