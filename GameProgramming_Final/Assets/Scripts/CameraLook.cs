using UnityEngine;

public class CameraLook : MonoBehaviour
{
    [Header("Look Settings")]
    public float sensitivity = 120f;
    
    float pitch = 0f; // X축 회전 각도 (위아래)

    void Update()
    {
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null && !gameManager.IsGameStarted()) return;

        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -90f, 90f);

        transform.localRotation = Quaternion.Euler(pitch, 0, 0);
    }
}