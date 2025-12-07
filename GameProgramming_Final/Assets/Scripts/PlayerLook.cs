using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    [Header("Look Settings")]
    public float sensitivity = 120f;
    
    float yaw = 0f; // Y축 회전 각도

    void Update()
    {
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null && !gameManager.IsGameStarted()) return;

        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        yaw += mouseX;
        transform.rotation = Quaternion.Euler(0, yaw, 0);
    }
}