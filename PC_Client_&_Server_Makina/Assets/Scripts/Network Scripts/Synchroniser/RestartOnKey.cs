using CustomMessages;
using Mirror;
using UnityEngine;

public class RestartOnKey : MonoBehaviour {

    [SerializeField, Tooltip("nbvds nbfdxvhfdxw")] private KeyCode m_keyToPress = KeyCode.F8;

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(m_keyToPress)) {
            NetworkClient.Send(new RestartGame());
        }
    }
}
