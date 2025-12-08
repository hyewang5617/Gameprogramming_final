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
    public float decayStartTime = 30f;
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
        if (!scoreDecaying)
            return Mathf.RoundToInt(startingDecayScore);
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
        
        StartGame();
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
        if (gameStartTime <= 0f)
            gameStartTime = Time.time;

        float elapsedTime = Time.time - gameStartTime;
        
        if (elapsedTime >= decayStartTime && !scoreDecaying)
        {
            scoreDecaying = true;
            timeAttackScore = startingDecayScore;
            currentScore += startingDecayScore;
        }

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

    public void ClaimFinalScore()
    {
        if (dataManager != null)
        {
            if (!scoreDecaying)
            {
                scoreDecaying = true;
                timeAttackScore = startingDecayScore;
                currentScore += startingDecayScore;
            }
            
            int finalScore = Mathf.RoundToInt(currentScore);
            earnedCurrency = finalScore;
            dataManager.AddCurrency(finalScore);
            
            currentScore = 0f;
            airTimeScore = 0f;
            timeAttackScore = 0f;
        }
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

