using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections;

/// <summary>
/// 액티브 스킬 관리 및 게이지/입력 처리.
/// - 스킬 해금 여부는 DataManager의 스킬 레벨로 판단
/// - 더블점프: 플레이어에 추가 점프 허용
/// - 타임슬로우: timeScale 변경 + 게이지 소모/회복
/// - 제트팩: 점프 키 유지 시 상승 힘 적용 + 게이지 소모/회복
/// </summary>
public class SkillManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Player player;
    [SerializeField] private DataManager dataManager;

    [Header("Time Slow")]
    [SerializeField] private KeyCode timeSlowKey = KeyCode.Q;
    [SerializeField] private bool useMouseButton = false; // 마우스 버튼 사용 여부
    [SerializeField] private int timeSlowMouseButton = 0; // 0=왼쪽, 1=오른쪽
    [SerializeField] private float defaultTimeSlowScale = 0.3f;
    [SerializeField] private float defaultTimeSlowGaugeMax = 5f;
    [SerializeField] private float defaultTimeSlowDrainPerSec = 1f;
    [SerializeField] private float defaultTimeSlowRegenPerSec = 0.5f;
    [SerializeField] private Image timeSlowGaugeFill;
    [SerializeField] private GameObject timeSlowGaugeBackground;
    [SerializeField] private CanvasGroup timeSlowGaugeGroup;
    [SerializeField] private CanvasGroup timeSlowOverlay;
    [SerializeField] private float timeSlowOverlayFadeDuration = 0.25f;

    [Header("Jetpack")]
    [SerializeField] private KeyCode jetpackKey = KeyCode.Space;
    [SerializeField] private bool useJetpackMouseButton = false; // 마우스 버튼 사용 여부
    [SerializeField] private int jetpackMouseButton = 1; // 0=왼쪽, 1=오른쪽
    [SerializeField] private float defaultJetpackForce = 15f;
    [SerializeField] private float defaultJetpackGaugeMax = 5f;
    [SerializeField] private float defaultJetpackDrainPerSec = 1f;
    [SerializeField] private float defaultJetpackRegenPerSec = 0.75f;
    [SerializeField] private Image jetpackGaugeFill;
    [SerializeField] private GameObject jetpackGaugeBackground;
    [SerializeField] private CanvasGroup jetpackGaugeGroup;

    [Header("Double Jump")]
    [SerializeField] private int defaultExtraJumpCount = 1;

    private bool hasDoubleJump;
    private bool hasTimeSlow;
    private bool hasJetpack;

    private float timeSlowScale;
    private float timeSlowGaugeMax;
    private float timeSlowDrainPerSec;
    private float timeSlowRegenPerSec;

    private float jetpackForce;
    private float jetpackGaugeMax;
    private float jetpackDrainPerSec;
    private float jetpackRegenPerSec;

    private int extraJumpCount;

    private float timeSlowGauge;
    private float jetpackGauge;
    private bool timeSlowActive;
    private float originalFixedDeltaTime = 0.02f;
    private Coroutine overlayFadeRoutine;
    [SerializeField] private float gaugeFadeSpeed = 3f;
    [SerializeField, Range(0f, 1f)] private float minRechargePercent = 0.15f;
    private bool timeSlowWasFull = true;
    private bool jetpackWasFull = true;
    private bool timeSlowDepleted = false;
    private bool jetpackDepleted = false;

    private void Awake()
    {
        if (player == null) player = FindObjectOfType<Player>();
        if (dataManager == null) dataManager = DataManager.Instance;
        originalFixedDeltaTime = Time.fixedDeltaTime;
        if (timeSlowOverlay != null)
        {
            timeSlowOverlay.alpha = 0f;
            timeSlowOverlay.blocksRaycasts = false;
            timeSlowOverlay.interactable = false;
        }

        CacheSkillUnlocks();
        RecalculateSkillParams();
        InitializeGauges();
        ApplyPersistentSkillEffects();
    }

    private void CacheSkillUnlocks()
    {
        hasDoubleJump = dataManager != null && dataManager.GetSkillLevel("DoubleJump") > 0;
        hasTimeSlow = dataManager != null && dataManager.GetSkillLevel("TimeSlow") > 0;
        hasJetpack = dataManager != null && dataManager.GetSkillLevel("JetPack") > 0;
    }

    private void RecalculateSkillParams()
    {
        int djLevel = dataManager != null ? dataManager.GetSkillLevel("DoubleJump") : 0;
        int tsLevel = dataManager != null ? dataManager.GetSkillLevel("TimeSlow") : 0;
        int jpLevel = dataManager != null ? dataManager.GetSkillLevel("JetPack") : 0;

        var defDouble = dataManager != null ? dataManager.GetSkillDefinition("DoubleJump") : null;
        var defTime = dataManager != null ? dataManager.GetSkillDefinition("TimeSlow") : null;
        var defJet = dataManager != null ? dataManager.GetSkillDefinition("JetPack") : null;

        extraJumpCount = GetIntByLevel(defDouble?.extraJumpPerLevel, djLevel, defaultExtraJumpCount);

        timeSlowGaugeMax = GetByLevel(defTime?.gaugeMaxPerLevel, tsLevel, defaultTimeSlowGaugeMax);
        timeSlowDrainPerSec = GetByLevel(defTime?.drainPerSecPerLevel, tsLevel, defaultTimeSlowDrainPerSec);
        timeSlowRegenPerSec = GetByLevel(defTime?.regenPerSecPerLevel, tsLevel, defaultTimeSlowRegenPerSec);
        timeSlowScale = GetByLevel(defTime?.timeScalePerLevel, tsLevel, defaultTimeSlowScale);

        jetpackForce = GetByLevel(defJet?.forcePerLevel, jpLevel, defaultJetpackForce);
        jetpackGaugeMax = GetByLevel(defJet?.gaugeMaxPerLevel, jpLevel, defaultJetpackGaugeMax);
        jetpackDrainPerSec = GetByLevel(defJet?.drainPerSecPerLevel, jpLevel, defaultJetpackDrainPerSec);
        jetpackRegenPerSec = GetByLevel(defJet?.regenPerSecPerLevel, jpLevel, defaultJetpackRegenPerSec);
    }

    private void InitializeGauges()
    {
        timeSlowGauge = timeSlowGaugeMax;
        jetpackGauge = jetpackGaugeMax;
        UpdateGaugeUI(timeSlowGaugeFill, timeSlowGauge, timeSlowGaugeMax, timeSlowGaugeBackground, timeSlowGaugeGroup, ref timeSlowWasFull);
        UpdateGaugeUI(jetpackGaugeFill, jetpackGauge, jetpackGaugeMax, jetpackGaugeBackground, jetpackGaugeGroup, ref jetpackWasFull);
    }

    private void ApplyPersistentSkillEffects()
    {
        if (hasDoubleJump && player != null)
        {
            player.SetMaxExtraJumps(extraJumpCount);
        }
        else if (player != null)
        {
            player.SetMaxExtraJumps(0);
        }
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        HandleTimeSlow(dt);
        HandleJetpack(dt);
        UpdateGaugeUI(timeSlowGaugeFill, timeSlowGauge, timeSlowGaugeMax, timeSlowGaugeBackground, timeSlowGaugeGroup, ref timeSlowWasFull);
        UpdateGaugeUI(jetpackGaugeFill, jetpackGauge, jetpackGaugeMax, jetpackGaugeBackground, jetpackGaugeGroup, ref jetpackWasFull);
    }

    private void HandleTimeSlow(float dt)
    {
        if (!hasTimeSlow) return;

        bool wantTimeSlow = useMouseButton 
            ? Input.GetMouseButton(timeSlowMouseButton) 
            : Input.GetKey(timeSlowKey);
        float minReady = timeSlowGaugeMax > 0f ? timeSlowGaugeMax * minRechargePercent : 0f;
        if (timeSlowDepleted && timeSlowGauge < minReady)
        {
            wantTimeSlow = false; // 충전 대기 상태에서는 입력을 무시
        }

        if (wantTimeSlow && timeSlowGauge > 0.1f)
        {
            if (!timeSlowActive && !timeSlowDepleted && timeSlowGauge >= minReady)
                EnableTimeSlow();
        }
        else
        {
            if (timeSlowActive)
                DisableTimeSlow();
        }

        if (timeSlowActive)
        {
            timeSlowGauge = Mathf.Max(0f, timeSlowGauge - timeSlowDrainPerSec * dt);
            if (timeSlowGauge <= 0f)
            {
                DisableTimeSlow();
                timeSlowDepleted = true;
            }
        }
        else
        {
            timeSlowGauge = Mathf.Min(timeSlowGauge + timeSlowRegenPerSec * dt, timeSlowGaugeMax);
            if (timeSlowDepleted && timeSlowGauge >= minReady)
            {
                timeSlowDepleted = false;
            }
        }
    }

    private void EnableTimeSlow()
    {
        timeSlowActive = true;
        Time.timeScale = timeSlowScale;
        Time.fixedDeltaTime = originalFixedDeltaTime * timeSlowScale;
        FadeOverlay(1f);
    }

    private void DisableTimeSlow()
    {
        if (!timeSlowActive) return;
        timeSlowActive = false;
        Time.timeScale = 1f;
        Time.fixedDeltaTime = originalFixedDeltaTime;
        FadeOverlay(0f);
    }

    private void HandleJetpack(float dt)
    {
        if (!hasJetpack || player == null) return;

        float minReady = jetpackGaugeMax > 0f ? jetpackGaugeMax * minRechargePercent : 0f;
        bool keyPressed = useJetpackMouseButton 
            ? Input.GetMouseButton(jetpackMouseButton) 
            : Input.GetKey(jetpackKey);
        bool useJetpack = keyPressed && jetpackGauge > 0f;
        if (jetpackDepleted && jetpackGauge < minReady)
        {
            useJetpack = false; // 충전 대기 상태에서는 입력을 무시
        }
        if (useJetpack)
        {
            Rigidbody rb = player.GetRigidbody();
            if (rb != null)
            {
                rb.AddForce(Vector3.up * jetpackForce, ForceMode.Acceleration);
            }
            jetpackGauge = Mathf.Max(0f, jetpackGauge - jetpackDrainPerSec * dt);
            if (jetpackGauge <= 0f)
            {
                jetpackDepleted = true;
            }
        }
        else
        {
            jetpackGauge = Mathf.Min(jetpackGauge + jetpackRegenPerSec * dt, jetpackGaugeMax);
            if (jetpackDepleted && jetpackGauge >= minReady)
            {
                jetpackDepleted = false;
            }
        }
    }

    private void UpdateGaugeUI(Image fill, float current, float max, GameObject background, CanvasGroup group, ref bool wasFull)
    {
        if (fill != null)
        {
            if (max > 0f)
                fill.fillAmount = Mathf.Clamp01(current / max);
            else
                fill.fillAmount = 0f;
        }

        if (group != null)
        {
            bool full = max > 0f && current >= max - 0.001f;
            if (wasFull && !full)
            {
                // 시작 시 페이드 인 효과를 주기 위해 0에서 시작
                group.alpha = 0f;
            }
            wasFull = full;

            float target = full ? 0f : 1f;
            float alpha = Mathf.MoveTowards(group.alpha, target, gaugeFadeSpeed * Time.unscaledDeltaTime);
            group.alpha = alpha;
            group.blocksRaycasts = alpha > 0.01f;
            group.interactable = alpha > 0.01f;
        }

        if (background != null)
        {
            // 배경은 페이드 중에도 활성 유지, 알파가 충분히 낮으면 비활성
            float currentAlpha = group != null ? group.alpha : 1f;
            bool show = max > 0f && (current < max - 0.001f || currentAlpha > 0.05f);
            if (background.activeSelf != show)
                background.SetActive(show);
        }
    }

    private void OnDisable()
    {
        DisableTimeSlow();
    }

    private void FadeOverlay(float targetAlpha)
    {
        if (timeSlowOverlay == null) return;
        if (overlayFadeRoutine != null) StopCoroutine(overlayFadeRoutine);
        overlayFadeRoutine = StartCoroutine(FadeOverlayCoroutine(targetAlpha));
    }

    private IEnumerator FadeOverlayCoroutine(float targetAlpha)
    {
        float duration = Mathf.Max(0.01f, timeSlowOverlayFadeDuration);
        float start = timeSlowOverlay.alpha;
        float t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float lerp = Mathf.Clamp01(t / duration);
            timeSlowOverlay.alpha = Mathf.Lerp(start, targetAlpha, lerp);
            yield return null;
        }

        timeSlowOverlay.alpha = targetAlpha;
        bool enable = targetAlpha > 0.001f;
        timeSlowOverlay.blocksRaycasts = enable;
        timeSlowOverlay.interactable = enable;
        overlayFadeRoutine = null;
    }

    private float GetByLevel(float[] arr, int level, float fallback)
    {
        if (arr == null || arr.Length == 0 || level <= 0) return fallback;
        int idx = Mathf.Clamp(level - 1, 0, arr.Length - 1);
        return arr[idx];
    }

    private int GetIntByLevel(int[] arr, int level, int fallback)
    {
        if (arr == null || arr.Length == 0 || level <= 0) return fallback;
        int idx = Mathf.Clamp(level - 1, 0, arr.Length - 1);
        return arr[idx];
    }
}
