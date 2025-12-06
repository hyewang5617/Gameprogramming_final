using UnityEngine;

public class CameraLook : MonoBehaviour
{
    [Header("Look Settings")]
    public float sensitivity = 120f;
    
    float pitch = 0f;

    void Update()
    {
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -90f, 90f);
        transform.localRotation = Quaternion.Euler(pitch, 0, 0);
    }
}