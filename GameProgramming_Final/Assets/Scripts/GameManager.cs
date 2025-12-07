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
    public GameObject loadingPanel;
    public UnityEngine.UI.Slider loadingProgressBar;
    
    [Header("Death Settings")]
    public float deathHeight = -10f;

    bool isGameOver = false;
    bool isLevelComplete = false;
    bool gameStarted = false;

    public bool IsGameStarted() => gameStarted; // 다른 스크립트에서 게임 시작 여부 확인용

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(false);
        if (loadingPanel != null)
            loadingPanel.SetActive(true);

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
            if (loadingProgressBar != null && vehicleSpawner != null)
                loadingProgressBar.value = vehicleSpawner.InitialSpawnProgress;
            
            if (vehicleSpawner != null && vehicleSpawner.IsReady)
            {
                gameStarted = true;
                
                if (loadingPanel != null)
                    loadingPanel.SetActive(false);
                
                if (vehicleSpawner != null)
                    vehicleSpawner.ResetAllVehicleSpeeds();
                
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                
                if (player != null)
                {
                    Player playerScript = player.GetComponent<Player>();
                    if (playerScript != null)
                    {
                        playerScript.ReleaseFromStart();
                        playerScript.SetCanMove(true);
                    }
                }
            }
            return;
        }

        if ((isGameOver || isLevelComplete) && Input.anyKeyDown)
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
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    // GoalTrigger에서 호출
    public void LevelComplete()
    {
        if (isLevelComplete) return;
        
        isLevelComplete = true;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(true);
    }

    // UI에서 아무 키나 누르면 호출
    void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

