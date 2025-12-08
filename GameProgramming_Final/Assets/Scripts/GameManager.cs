using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public VehicleSpawner vehicleSpawner;
    public TutorialManager tutorialManager;
    public ScoreManager scoreManager;
    
    [Header("Stage Settings")]
    public string[] stageSceneNames;
    
    [Header("UI")]
    public GameObject gameOverPanel;
    public GameObject levelCompletePanel;
    public GameObject loadingPanel;
    public UnityEngine.UI.Slider loadingProgressBar;
    public TextMeshProUGUI totalCurrencyText;
    public TextMeshProUGUI earnedPointText;
    public float pointDisplayDelay = 1f;
    
    bool isGameOver = false;
    bool isLevelComplete = false;
    bool gameStarted = false;

    public bool IsGameStarted() => gameStarted;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
        
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
        if (loadingPanel != null) loadingPanel.SetActive(true);
        if (earnedPointText != null) earnedPointText.gameObject.SetActive(false);

        if (vehicleSpawner == null) vehicleSpawner = FindObjectOfType<VehicleSpawner>();
        if (tutorialManager == null) tutorialManager = FindObjectOfType<TutorialManager>();
        if (scoreManager == null) scoreManager = FindObjectOfType<ScoreManager>();

        Player playerScript = player?.GetComponent<Player>();
        if (playerScript != null) playerScript.SetCanMove(false);
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
                if (loadingPanel != null) loadingPanel.SetActive(false);
                if (vehicleSpawner != null) vehicleSpawner.ResetAllVehicleSpeeds();
                
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = false;
                
                Player playerScript = player?.GetComponent<Player>();
                if (playerScript != null)
                {
                    playerScript.ReleaseFromStart();
                    playerScript.SetCanMove(true);
                }

                if (scoreManager != null) scoreManager.StartGame();
                if (tutorialManager != null) tutorialManager.StartTutorial();
            }
            return;
        }

        if ((isGameOver || isLevelComplete) && Input.anyKeyDown)
        {
            RestartLevel();
            return;
        }
    }

    public void GameOver()
    {
        if (isGameOver) return;
        
        isGameOver = true;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    public void LevelComplete()
    {
        if (isLevelComplete) return;
        
        isLevelComplete = true;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        if (levelCompletePanel != null) levelCompletePanel.SetActive(true);
        if (scoreManager != null) scoreManager.StopScoreDecay();
        
        UnlockNextStage();
        StartCoroutine(ShowEarnedPointsSequentially());
    }

    void UpdateCurrencyDisplay()
    {
        DataManager dm = DataManager.Instance ?? FindObjectOfType<DataManager>();
        if (dm == null) return;

        int totalCurrency = dm.GetCurrency();
        if (totalCurrencyText != null)
            totalCurrencyText.text = $"Total Currency: {totalCurrency}";
    }

    IEnumerator ShowEarnedPointsSequentially()
    {
        if (earnedPointText == null || scoreManager == null) yield break;

        int airPoint = scoreManager.GetFinalAirPoint();
        int timeAttackPoint = scoreManager.GetFinalTimeAttackPoint();
        
        DataManager dm = DataManager.Instance ?? FindObjectOfType<DataManager>();
        if (dm == null) yield break;
        
        int currentTotalCurrency = dm.GetCurrency();
        UpdateCurrencyText(currentTotalCurrency);

        earnedPointText.gameObject.SetActive(true);
        yield return StartCoroutine(CountUpScore("Air Point: +", 0, airPoint, 1.5f));
        yield return StartCoroutine(CountUpCurrency(currentTotalCurrency, currentTotalCurrency + airPoint, 0.5f));
        currentTotalCurrency += airPoint;

        yield return new WaitForSecondsRealtime(pointDisplayDelay);
        yield return StartCoroutine(CountUpScore("Time Attack Point: +", 0, timeAttackPoint, 1.5f));
        yield return StartCoroutine(CountUpCurrency(currentTotalCurrency, currentTotalCurrency + timeAttackPoint, 0.5f));
        
        scoreManager.ClaimFinalScore();
    }

    IEnumerator CountUpScore(string prefix, int startValue, int targetValue, float duration)
    {
        if (duration <= 0f) duration = 0.1f;
        
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = elapsedTime / duration;
            float easedT = 1f - Mathf.Pow(1f - t, 3f);
            int currentValue = Mathf.RoundToInt(Mathf.Lerp(startValue, targetValue, easedT));
            earnedPointText.text = $"{prefix}{currentValue}";
            yield return null;
        }
        earnedPointText.text = $"{prefix}{targetValue}";
    }

    IEnumerator CountUpCurrency(int startValue, int targetValue, float duration)
    {
        if (totalCurrencyText == null || duration <= 0f)
        {
            UpdateCurrencyText(targetValue);
            yield break;
        }
        
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = elapsedTime / duration;
            float easedT = 1f - Mathf.Pow(1f - t, 3f);
            int currentValue = Mathf.RoundToInt(Mathf.Lerp(startValue, targetValue, easedT));
            totalCurrencyText.text = $"Total Currency: {currentValue}";
            yield return null;
        }
        totalCurrencyText.text = $"Total Currency: {targetValue}";
    }

    void UpdateCurrencyText(int currency)
    {
        if (totalCurrencyText != null)
            totalCurrencyText.text = $"Total Currency: {currency}";
    }

    void RestartLevel()
    {
        Time.timeScale = 1f;
        PlayerPrefs.SetInt("returnToStageSelect", 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene("Main");
    }

    void UnlockNextStage()
    {
        DataManager dm = DataManager.Instance ?? FindObjectOfType<DataManager>();
        if (dm == null)
        {
            Debug.LogError("[GameManager] DataManager를 찾을 수 없습니다!");
            return;
        }
        
        if (stageSceneNames == null || stageSceneNames.Length == 0)
        {
            Debug.LogWarning("[GameManager] stageSceneNames 배열이 비어있습니다!");
            return;
        }
        
        string currentSceneName = SceneManager.GetActiveScene().name;
        int currentStage = -1;
        
        for (int i = 0; i < stageSceneNames.Length; i++)
        {
            if (string.IsNullOrEmpty(stageSceneNames[i])) continue;
            
            string sceneName = stageSceneNames[i].Trim();
            if (sceneName == currentSceneName || 
                sceneName.Contains(currentSceneName) || 
                currentSceneName.Contains(sceneName))
            {
                currentStage = i + 1;
                break;
            }
        }
        
        if (currentStage < 0)
        {
            Debug.LogWarning($"[GameManager] 현재 씬 '{currentSceneName}'을 stageSceneNames 배열에서 찾을 수 없습니다!");
            return;
        }
        
        int nextStage = currentStage + 1;
        dm.UnlockStage(nextStage);
        Debug.Log($"[GameManager] Stage {currentStage} 클리어 → Stage {nextStage} 해금 완료!");
    }
}
