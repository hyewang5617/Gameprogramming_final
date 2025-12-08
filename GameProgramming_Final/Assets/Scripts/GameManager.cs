using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public VehicleSpawner vehicleSpawner;
    public TutorialManager tutorialManager;
    public ScoreManager scoreManager;
    
    [Header("UI")]
    public GameObject gameOverPanel;
    public GameObject levelCompletePanel;
    public GameObject loadingPanel;
    public UnityEngine.UI.Slider loadingProgressBar;
    public TextMeshProUGUI totalCurrencyText; // 총 Currency 표시 텍스트
    public TextMeshProUGUI earnedPointText; // Earned Point 표시 텍스트 (Air Point와 Time Attack Point를 순차적으로 표시)
    public float pointDisplayDelay = 2f; // Air Point 표시 후 Time Attack Point 표시까지의 딜레이
    
    [Header("Death Settings")]
    public float deathHeight = -10f;

    bool isGameOver = false;
    bool isLevelComplete = false;
    bool gameStarted = false;

    public bool IsGameStarted() => gameStarted; // 다른 스크립트에서 게임 시작 여부 확인용

    void Start()
    {
        Cursor.lockState = CursorLockMode.Confined; // 화면 안에서만 이동, 클릭 인식 가능
        Cursor.visible = false; // 커서는 숨김
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(false);
        if (loadingPanel != null)
            loadingPanel.SetActive(true);
        if (earnedPointText != null)
            earnedPointText.gameObject.SetActive(false);

        if (vehicleSpawner == null)
            vehicleSpawner = FindObjectOfType<VehicleSpawner>();
        
        if (tutorialManager == null)
            tutorialManager = FindObjectOfType<TutorialManager>();
        
        if (scoreManager == null)
            scoreManager = FindObjectOfType<ScoreManager>();

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
                
                Cursor.lockState = CursorLockMode.Confined;
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

                if (scoreManager != null)
                    scoreManager.StartGame();

                if (tutorialManager != null)
                    tutorialManager.StartTutorial();
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
        
        // 패널을 먼저 활성화
        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(true);
        
        if (scoreManager != null)
        {
            scoreManager.StopScoreDecay();
        }
        
        // 현재 스테이지 번호 확인 및 다음 스테이지 언락
        UnlockNextStage();
        
        // Earned Point 순차적으로 표시 (점수를 먼저 표시한 후 Currency 변환)
        StartCoroutine(ShowEarnedPointsSequentially());
    }

    // Currency 표시 업데이트
    void UpdateCurrencyDisplay()
    {
        DataManager dm = DataManager.Instance;
        
        // Instance가 null이면 씬에서 찾기
        if (dm == null)
        {
            dm = FindObjectOfType<DataManager>();
            if (dm == null)
            {
                Debug.LogError("[GameManager] DataManager를 찾을 수 없습니다! 씬에 DataManager 컴포넌트가 있는 GameObject가 있는지 확인해주세요.");
                return;
            }
        }

        int totalCurrency = dm.GetCurrency();

        if (totalCurrencyText != null)
        {
            totalCurrencyText.text = $"Total Currency: {totalCurrency}";
        }
        else
        {
            Debug.LogWarning("[GameManager] totalCurrencyText is null! Inspector에서 할당해주세요.");
        }
    }

    // Earned Point를 순차적으로 표시하는 코루틴
    IEnumerator ShowEarnedPointsSequentially()
    {
        if (earnedPointText == null || scoreManager == null)
        {
            Debug.LogWarning("[GameManager] earnedPointText 또는 scoreManager가 null입니다!");
            yield break;
        }

        // 점수 값을 먼저 저장 (ClaimFinalScore 호출 전)
        // GetFinalTimeAttackPoint()는 30초가 안 지났어도 1000점 반환
        int airPoint = scoreManager.GetFinalAirPoint();
        int timeAttackPoint = scoreManager.GetFinalTimeAttackPoint();
        
        Debug.Log($"[GameManager] 점수 저장 - Air: {airPoint}, Time Attack: {timeAttackPoint}");

        // 점수를 Currency로 변환 (값이 저장된 후)
        scoreManager.ClaimFinalScore();
        
        // Currency 정보 업데이트 (초기 상태)
        DataManager dm = DataManager.Instance ?? FindObjectOfType<DataManager>();
        if (dm == null)
        {
            Debug.LogError("[GameManager] DataManager를 찾을 수 없습니다!");
            yield break;
        }
        
        int currentTotalCurrency = dm.GetCurrency() - airPoint - timeAttackPoint; // 추가되기 전 총 Currency
        UpdateCurrencyText(currentTotalCurrency);

        // Air Point 카운트업 애니메이션 (1.5초 동안)
        earnedPointText.gameObject.SetActive(true);
        yield return StartCoroutine(CountUpScore("Air Point: +", 0, airPoint, 1.5f));
        
        // Air Point를 Total Currency에 더하면서 자연스럽게 올라가는 모션
        yield return StartCoroutine(CountUpCurrency(currentTotalCurrency, currentTotalCurrency + airPoint, 0.5f));
        currentTotalCurrency += airPoint;
        
        Debug.Log($"[GameManager] Air Point 표시 완료: +{airPoint}");

        // 1초 대기 (Air Point에서 Time Attack Point로 넘어가는 속도)
        yield return new WaitForSecondsRealtime(1f);

        // Time Attack Point 카운트업 애니메이션 (1.5초 동안)
        yield return StartCoroutine(CountUpScore("Time Attack Point: +", 0, timeAttackPoint, 1.5f));
        
        // Time Attack Point를 Total Currency에 더하면서 자연스럽게 올라가는 모션
        yield return StartCoroutine(CountUpCurrency(currentTotalCurrency, currentTotalCurrency + timeAttackPoint, 0.5f));
        
        Debug.Log($"[GameManager] Time Attack Point 표시 완료: +{timeAttackPoint}");
    }

    // 점수 카운트업 애니메이션 코루틴
    IEnumerator CountUpScore(string prefix, int startValue, int targetValue, float duration)
    {
        if (duration <= 0f) duration = 0.1f; // 최소 시간 보장
        
        float elapsedTime = 0f;
        int currentValue = startValue;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = elapsedTime / duration;
            
            // 부드러운 증가를 위한 easing 함수 사용 (easeOut)
            float easedT = 1f - Mathf.Pow(1f - t, 3f);
            currentValue = Mathf.RoundToInt(Mathf.Lerp(startValue, targetValue, easedT));
            
            earnedPointText.text = $"{prefix}{currentValue}";
            
            yield return null;
        }
        
        // 마지막에 정확한 값으로 설정
        earnedPointText.text = $"{prefix}{targetValue}";
    }

    // Total Currency 카운트업 애니메이션 코루틴
    IEnumerator CountUpCurrency(int startValue, int targetValue, float duration)
    {
        if (totalCurrencyText == null || duration <= 0f)
        {
            UpdateCurrencyText(targetValue);
            yield break;
        }
        
        float elapsedTime = 0f;
        int currentValue = startValue;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = elapsedTime / duration;
            
            // 부드러운 증가를 위한 easing 함수 사용 (easeOut)
            float easedT = 1f - Mathf.Pow(1f - t, 3f);
            currentValue = Mathf.RoundToInt(Mathf.Lerp(startValue, targetValue, easedT));
            
            totalCurrencyText.text = $"Total Currency: {currentValue}";
            
            yield return null;
        }
        
        // 마지막에 정확한 값으로 설정
        totalCurrencyText.text = $"Total Currency: {targetValue}";
    }

    // Total Currency 텍스트만 업데이트 (애니메이션 없이)
    void UpdateCurrencyText(int currency)
    {
        if (totalCurrencyText != null)
        {
            totalCurrencyText.text = $"Total Currency: {currency}";
        }
    }

    void RestartLevel()
    {
        Time.timeScale = 1f;
        // Stage 선택 화면으로 돌아가야 함을 표시 (PlayerPrefs 사용)
        PlayerPrefs.SetInt("returnToStageSelect", 1);
        PlayerPrefs.Save();
        // Main 씬으로 이동 (UIManager에서 플래그 확인 후 Stage 선택 화면으로 전환)
        SceneManager.LoadScene("Main");
    }

    // 현재 스테이지를 확인하고 다음 스테이지 언락
    void UnlockNextStage()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        int currentStage = GetStageNumberFromSceneName(currentSceneName);
        
        if (currentStage > 0)
        {
            DataManager dm = DataManager.Instance ?? FindObjectOfType<DataManager>();
            if (dm != null)
            {
                // 현재 스테이지의 다음 스테이지 언락
                int nextStage = currentStage + 1;
                dm.UnlockStage(nextStage);
                Debug.Log($"[GameManager] Stage {currentStage} 완료! Stage {nextStage} 언락됨.");
            }
        }
    }

    // 씬 이름에서 스테이지 번호 추출 (예: "Stage 1" -> 1, "Stage1" -> 1)
    int GetStageNumberFromSceneName(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return 0;
        
        // "Stage 1", "Stage1", "Stage 2" 등의 패턴 매칭
        Match match = Regex.Match(sceneName, @"Stage\s*(\d+)", RegexOptions.IgnoreCase);
        if (match.Success && match.Groups.Count > 1)
        {
            if (int.TryParse(match.Groups[1].Value, out int stageNumber))
            {
                return stageNumber;
            }
        }
        
        return 0;
    }
}
