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
        Time.timeScale = 0f;
        
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
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

