using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [Header("References")]
    public Player player;
    public GameManager gameManager;
    public DataManager dataManager;

    [Header("Air Time Score")]
    public float scorePerSecondInAir = 100f; // 공중에 1초당 얻는 점수

    [Header("Time Decay Score")]
    public float decayStartTime = 30f; // 몇 초부터 점수 감소 시작
    public float decayRate = 1f; // 초당 감소하는 점수
    public float startingDecayScore = 1000f; // 감소 시작 시 초기 점수

    float currentScore = 0f; // 현재 점수
    float airTimeScore = 0f; // 공중 시간으로 얻은 점수 (누적)
    float timeAttackScore = 0f; // 시간 기반 점수 (1000점에서 감소된 값)
    bool isInAir = false;
    float airTimeStart = 0f;
    bool scoreDecaying = false;
    float gameStartTime = 0f;
    bool gameActive = true;
    int earnedCurrency = 0; // 이번 스테이지에서 얻은 Currency

    public float CurrentScore => Mathf.Max(0f, currentScore); // 현재 점수 반환 (0 이하로 안 가게)
    public int EarnedCurrency => earnedCurrency; // 이번 스테이지에서 얻은 Currency
    public int AirPoint => Mathf.RoundToInt(airTimeScore); // 공중 시간으로 얻은 점수
    public int TimeAttackPoint => Mathf.RoundToInt(timeAttackScore); // 시간 기반 점수
    
    // 골인 시점의 최종 점수 반환 (ClaimFinalScore 호출 전에 사용)
    public int GetFinalAirPoint() => Mathf.RoundToInt(airTimeScore);
    public int GetFinalTimeAttackPoint()
    {
        // 30초가 지나지 않았어도 골인 시 1000점 반환
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

    // 공중 시간 점수 계산
    void HandleAirTimeScore()
    {
        if (player == null) return;

        bool wasInAir = isInAir;
        bool nowInAir = !player.IsGrounded() && !player.IsOnVehicle();

        if (!wasInAir && nowInAir)
        {
            // 공중으로 떠오름
            isInAir = true;
            airTimeStart = Time.time;
        }
        else if (wasInAir && !nowInAir)
        {
            // 착지함
            isInAir = false;
            float airTime = Time.time - airTimeStart;
            float scoreGain = airTime * scorePerSecondInAir;
            AddScore(scoreGain);
            airTimeScore += scoreGain;
            Debug.Log($"[ScoreManager] 공중 시간 {airTime:F2}초, {scoreGain:F0}점 획득. 현재 점수: {Mathf.RoundToInt(CurrentScore)}");
        }
    }

    // 시간 기반 점수 감소
    void HandleTimeDecayScore()
    {
        if (gameStartTime <= 0f)
            gameStartTime = Time.time;

        float elapsedTime = Time.time - gameStartTime;
        
        if (elapsedTime >= decayStartTime && !scoreDecaying)
        {
            scoreDecaying = true;
            timeAttackScore = startingDecayScore; // 1000점으로 시작
            currentScore += startingDecayScore; // 감소 시작 시 1000점 추가
            Debug.Log($"[ScoreManager] 점수 감소 시작! 초기 {startingDecayScore}점 추가. 현재 점수: {Mathf.RoundToInt(CurrentScore)}");
        }

        if (scoreDecaying)
        {
            float decayAmount = decayRate * Time.deltaTime;
            timeAttackScore = Mathf.Max(0f, timeAttackScore - decayAmount); // 시간 기반 점수 감소
            currentScore = Mathf.Max(0f, currentScore - decayAmount); // 총 점수 감소
        }
    }

    // 점수 추가
    void AddScore(float amount)
    {
        currentScore += amount;
    }

    // 점수 감소 중지 (골인 시 호출)
    public void StopScoreDecay()
    {
        scoreDecaying = false;
        gameActive = false;
    }

    // 최종 점수를 Currency로 변환 (골인 시 호출)
    public void ClaimFinalScore()
    {
        if (dataManager != null)
        {
            // 30초가 안 지났어도 골인 시 1000점 추가 (한 번만)
            if (!scoreDecaying)
            {
                scoreDecaying = true; // 중복 추가 방지
                timeAttackScore = startingDecayScore; // 1000점으로 설정
                currentScore += startingDecayScore;
                Debug.Log($"[ScoreManager] 골인! {startingDecayScore}점 추가. 현재 점수: {Mathf.RoundToInt(currentScore)}");
            }
            
            // 현재 점수는 airTimeScore + timeAttackScore와 일치해야 함
            int expectedScore = Mathf.RoundToInt(airTimeScore + timeAttackScore);
            int finalScore = Mathf.RoundToInt(currentScore);
            
            // 디버그: 점수 계산 확인
            Debug.Log($"[ScoreManager] 점수 계산 - Air: {Mathf.RoundToInt(airTimeScore)}, Time Attack: {Mathf.RoundToInt(timeAttackScore)}, 총: {expectedScore}, Current: {finalScore}");
            
            earnedCurrency = finalScore; // 이번 스테이지에서 얻은 Currency 저장
            dataManager.AddCurrency(finalScore);
            Debug.Log($"[ScoreManager] 최종 점수 {finalScore} 획득!");
            currentScore = 0f;
            airTimeScore = 0f;
            timeAttackScore = 0f;
        }
    }

    // 게임 시작 시 호출
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

