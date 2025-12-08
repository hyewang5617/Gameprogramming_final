using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [Header("References")]
    public Player player;
    public GameManager gameManager;
    public DataManager dataManager;

    [Header("Air Time Score")]
    public float scorePerSecondInAir = 100f;

    [Header("Time Decay Score")]
    public float decayStartTime = 30f; // 이 시간 이후부터 1000점에서 깎기 시작
    public float decayRate = 1f;
    public float startingDecayScore = 1000f;

    float currentScore = 0f;
    float airTimeScore = 0f;
    float timeAttackScore = 0f;
    bool isInAir = false;
    float airTimeStart = 0f;
    bool scoreDecaying = false;
    float gameStartTime = 0f;
    bool gameActive = true;
    int earnedCurrency = 0;

    public float CurrentScore => Mathf.Max(0f, currentScore);
    public int EarnedCurrency => earnedCurrency;
    public int AirPoint => Mathf.RoundToInt(airTimeScore);
    public int TimeAttackPoint => Mathf.RoundToInt(timeAttackScore);
    
    public int GetFinalAirPoint() => Mathf.RoundToInt(airTimeScore);
    public int GetFinalTimeAttackPoint()
    {
        // 골인 시점의 timeAttackScore 반환 (이미 StopScoreDecay()로 멈춘 상태)
        return Mathf.RoundToInt(timeAttackScore);
    }

    void Start()
    {
        if (player == null)
            player = FindObjectOfType<Player>();
        
        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();
        
        if (dataManager == null)
            dataManager = DataManager.Instance;
    }

    void Update()
    {
        if (!gameActive) return;
        if (!gameManager.IsGameStarted()) return;

        HandleAirTimeScore();
        HandleTimeDecayScore();
    }

    void HandleAirTimeScore()
    {
        if (player == null) return;

        bool wasInAir = isInAir;
        bool nowInAir = !player.IsGrounded() && !player.IsOnVehicle();

        if (!wasInAir && nowInAir)
        {
            isInAir = true;
            airTimeStart = Time.time;
        }
        else if (wasInAir && !nowInAir)
        {
            isInAir = false;
            float airTime = Time.time - airTimeStart;
            float scoreGain = airTime * scorePerSecondInAir;
            AddScore(scoreGain);
            airTimeScore += scoreGain;
        }
    }

    void HandleTimeDecayScore()
    {
        if (gameStartTime <= 0f) return; // StartGame()이 호출되지 않았으면 아무것도 하지 않음

        float elapsedTime = Time.time - gameStartTime;
        
        // 지정한 시간(decayStartTime) 이후부터 1000점에서 깎기 시작
        if (elapsedTime >= decayStartTime && !scoreDecaying)
        {
            scoreDecaying = true;
            timeAttackScore = startingDecayScore;
            currentScore += startingDecayScore;
        }

        // 점수 감소 중
        if (scoreDecaying)
        {
            float decayAmount = decayRate * Time.deltaTime;
            timeAttackScore = Mathf.Max(0f, timeAttackScore - decayAmount);
            currentScore = Mathf.Max(0f, currentScore - decayAmount);
        }
    }

    void AddScore(float amount) => currentScore += amount;

    public void StopScoreDecay()
    {
        scoreDecaying = false;
        gameActive = false;
    }

    // ClaimFinalScore는 더 이상 사용되지 않음 (GameManager에서 직접 추가)
    // 하지만 호환성을 위해 유지
    public void ClaimFinalScore()
    {
        // GameManager에서 이미 점수를 추가했으므로 여기서는 초기화만 수행
        currentScore = 0f;
        airTimeScore = 0f;
        timeAttackScore = 0f;
        earnedCurrency = 0;
    }

    public void StartGame()
    {
        currentScore = 0f;
        airTimeScore = 0f;
        timeAttackScore = 0f;
        earnedCurrency = 0;
        scoreDecaying = false;
        gameActive = true;
        gameStartTime = Time.time;
    }
}

