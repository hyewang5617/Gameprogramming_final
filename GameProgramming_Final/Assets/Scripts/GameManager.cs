using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("플레이어 Transform")]
    public Transform player;
    
    [Header("UI")]
    [Tooltip("게임오버 패널")]
    public GameObject gameOverPanel;
    [Tooltip("레벨 클리어 패널")]
    public GameObject levelCompletePanel;
    
    [Header("Death Settings")]
    [Tooltip("사망 처리 높이")]
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
        Debug.Log("[GameManager] LevelComplete() 호출됨!");
        
        if (isLevelComplete)
        {
            Debug.Log("[GameManager] 이미 클리어 상태임");
            return;
        }
        
        isLevelComplete = true;
        Time.timeScale = 0f; // 게임 정지
        
        Debug.Log($"[GameManager] levelCompletePanel null? {levelCompletePanel == null}");
        
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);
            Debug.Log("[GameManager] levelCompletePanel 활성화됨!");
        }
        else
        {
            Debug.LogWarning("[GameManager] levelCompletePanel이 할당되지 않았습니다!");
        }
    }

    void RestartLevel()
    {
        Time.timeScale = 1f; // 게임 재개
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

