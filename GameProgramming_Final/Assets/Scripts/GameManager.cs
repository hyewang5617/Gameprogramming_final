using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    
    [Header("UI")]
    public GameObject gameOverPanel;
    public GameObject levelCompletePanel;
    
    [Header("Death Settings")]
    public float deathHeight = -10f;

    bool isGameOver = false;
    bool isLevelComplete = false;

    void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(false);
    }

    void Update()
    {
        // 게임오버 상태에서 아무 키나 누르면 재시작
        if (isGameOver && Input.anyKeyDown)
        {
            RestartLevel();
            return;
        }

        // 레벨 클리어 상태에서 아무 키나 누르면 재시작
        if (isLevelComplete && Input.anyKeyDown)
        {
            RestartLevel();
            return;
        }

        // 플레이어 사망 체크
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

