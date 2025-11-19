using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    public float sensitivity = 120f;
    float yaw = 0f;

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        yaw += mouseX;
        transform.rotation = Quaternion.Euler(0, yaw, 0);
    }
}