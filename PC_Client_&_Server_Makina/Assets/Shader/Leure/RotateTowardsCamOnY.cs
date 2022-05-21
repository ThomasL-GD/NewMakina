using UnityEngine;

public class RotateTowardsCamOnY : MonoBehaviour
{
    void Update()
    {
        Vector3 oldEuler = transform.rotation.eulerAngles;
        transform.LookAt(CameraAndUISingleton.camera.transform.position);
        transform.rotation = Quaternion.Euler(new Vector3(oldEuler.x,transform.rotation.eulerAngles.y,oldEuler.z));
    }
}
