using UnityEngine;
using UnityEngine.UI;using UnityEngine.SceneManagement;

public class StageSelectController : BaseStateController
{
    [System.Serializable]
    private struct StageButtonEntry
    {
        public int stageNumber;
        public string sceneName;
        public Button button;
        public GameObject lockOverlay;
    }

    [Header("Stage Buttons (assign in Inspector)")]
    [SerializeField] private StageButtonEntry[] stageButtons;
    
    // 기본 씬 이름 (Inspector에서 설정하지 않으면 사용)
    private string GetDefaultSceneName(int stageNumber)
    {
        return $"Stage {stageNumber}";
    }

    private DataManager Data => DataManager.Instance;

    private void Awake()
    {
        WireStageButtons();
    }

    protected override void Reset()
    {
        base.Reset();
    }

    public override void Enter(object param = null)
    {
        base.Enter(param);
        RefreshStageButtons();
    }

    public override void Exit()
    {
        base.Exit();
    }

    private void WireStageButtons()
    {
        if (stageButtons == null) return;
        foreach (var entry in stageButtons)
        {
            if (entry.button == null) continue;
            int stageNum = entry.stageNumber;
            string scene = entry.sceneName;
            entry.button.onClick.AddListener(() => OnStageButtonClicked(stageNum, scene));
        }
    }

    private void RefreshStageButtons()
    {
        if (stageButtons == null) return;

        for (int i = 0; i < stageButtons.Length; i++)
        {
            var entry = stageButtons[i];
            if (entry.button == null) continue;

            bool unlocked = Data == null || entry.stageNumber <= 0 || Data.IsStageUnlocked(entry.stageNumber);
            entry.button.interactable = unlocked;
            if (entry.lockOverlay != null)
            {
                entry.lockOverlay.SetActive(!unlocked);
            }
        }
    }

    private void OnStageButtonClicked(int stageNumber, string sceneName)
    {
        // 씬 이름이 비어있으면 기본 씬 이름 사용
        if (string.IsNullOrEmpty(sceneName))
        {
            sceneName = GetDefaultSceneName(stageNumber);
            Debug.LogWarning($"StageSelectController: scene name is empty. Using default: {sceneName}");
        }

        if (Data != null && stageNumber > 0 && !Data.IsStageUnlocked(stageNumber))
        {
            Debug.LogWarning($"StageSelectController: stage {stageNumber} is locked.");
            return;
        }

        Debug.Log($"Loading scene: {sceneName} (Stage {stageNumber})");
        SceneManager.LoadScene(sceneName);
        base.Exit();
    }
}
