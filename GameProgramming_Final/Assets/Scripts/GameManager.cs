using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("플레이어 Transform")]
    public Transform player;
    [Tooltip("리스폰 지점")]
    public Transform spawnPoint;
    
    [Header("Death Settings")]
    public float deathHeight = -10f; // 사망 처리 높이

    void Update()
    {
        if (player == null) return;

        // 바닥 아래로 떨어지면 재시작
        if (player.position.y < deathHeight)
        {
            Respawn();
        }
    }

    public void Respawn()
    {
        if (spawnPoint != null)
        {
            player.position = spawnPoint.position;
            
            // 플레이어 Rigidbody 속도 초기화
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public void LevelComplete()
    {
        Debug.Log("레벨 클리어!");
        // 다음 레벨 또는 메뉴로 이동
    }
}

