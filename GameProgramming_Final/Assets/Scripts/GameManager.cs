using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public VehicleSpawner vehicleSpawner;
    
    [Header("UI")]
    public GameObject gameOverPanel;
    public GameObject levelCompletePanel;
    
    [Header("Death Settings")]
    public float deathHeight = -10f;

    bool isGameOver = false;
    bool isLevelComplete = false;
    bool gameStarted = false;

    void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(false);

        if (vehicleSpawner == null)
            vehicleSpawner = FindObjectOfType<VehicleSpawner>();

        if (player != null)
        {
            Player playerScript = player.GetComponent<Player>();
            if (playerScript != null)
                playerScript.SetCanMove(false);
        }
    }

    void Update()
    {
        if (!gameStarted)
        {
            if (vehicleSpawner != null && vehicleSpawner.IsReady)
            {
                gameStarted = true;
                if (player != null)
                {
                    Player playerScript = player.GetComponent<Player>();
                    if (playerScript != null)
                        playerScript.SetCanMove(true);
                }
            }
            return;
        }

        if (isGameOver && Input.anyKeyDown)
        {
            RestartLevel();
            return;
        }

        if (isLevelComplete && Input.anyKeyDown)
        {
            RestartLevel();
            return;
        }

        if (player != null && player.position.y < deathHeight)
        {
            GameOver();
        }
    }

    void GameOver()
    {
        if (isGameOver) return;
        
        isGameOver = true;
        Time.timeScale = 0f; // 게임 정지
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    public void LevelComplete()
    {
        if (isLevelComplete) return;
        
        isLevelComplete = true;
        Time.timeScale = 0f;
        
        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(true);
    }

    void RestartLevel()
    {
        Time.timeScale = 1f; // 게임 재개
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

