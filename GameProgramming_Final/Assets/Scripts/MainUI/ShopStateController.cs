using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class ShopStateController : BaseStateController
{
    [System.Serializable]
    private struct SkillButtonEntry
    {
        public string skillId;
        public Button button;
    }

    [Header("Skill UI References")]
    [SerializeField] private SkillButtonEntry[] skillButtons;
    [SerializeField] private TMP_Text skillTitleText;
    [SerializeField] private TMP_Text skillDescText;
    [SerializeField] private TMP_Text skillCostText;
    [SerializeField] private TMP_Text skillLevelText;
    [SerializeField] private TMP_Text currencyText;
    [SerializeField] private Button buyButton;

    private string selectedSkillId;
    private DataManager Data => DataManager.Instance;

    private void Awake()
    {
        WireSkillButtons();
        if (buyButton != null)
            buyButton.onClick.AddListener(TryBuySelected);
    }

    protected override void Reset()
    {
        base.Reset();
    }

    public override void Enter(object param = null)
    {
        base.Enter(param);
        RefreshCurrencyUI();
        ClearSelection();
    }

    public override void Exit()
    {
        base.Exit();
    }

    private void WireSkillButtons()
    {
        if (skillButtons == null) return;
        foreach (var entry in skillButtons)
        {
            if (entry.button == null || string.IsNullOrEmpty(entry.skillId)) continue;
            string capturedId = entry.skillId;
            entry.button.onClick.AddListener(() => OnSkillButtonClicked(capturedId));
        }
    }

    private void OnSkillButtonClicked(string skillId)
    {
        selectedSkillId = skillId;
        RefreshSelectionUI();
    }

    private void RefreshCurrencyUI()
    {
        if (currencyText == null) return;
        currencyText.text = Data != null ? $"{Data.GetCurrency()} Points" : "-";
    }

    private void ClearSelection()
    {
        selectedSkillId = null;
        if (skillTitleText != null) skillTitleText.text = "";
        if (skillDescText != null) skillDescText.text = "";
        if (skillCostText != null) skillCostText.text = "";
        if (skillLevelText != null) skillLevelText.text = "";
        if (buyButton != null) buyButton.interactable = false;
    }

    private void RefreshSelectionUI()
    {
        if (string.IsNullOrEmpty(selectedSkillId))
        {
            ClearSelection();
            return;
        }

        var def = FindSkillDefinition(selectedSkillId);
        int level = Data != null ? Data.GetSkillLevel(selectedSkillId) : 0;

        if (skillTitleText != null) skillTitleText.text = def != null ? def.displayName : selectedSkillId;
        if (skillDescText != null) skillDescText.text = def != null ? def.description : "";
        if (skillLevelText != null)
        {
            if (def != null)
                skillLevelText.text = $"Lv. {level} / {def.maxLevel}";
            else
                skillLevelText.text = $"Lv. {level}";
        }

        int nextCost = GetNextCost(def, level);
        if (skillCostText != null)
        {
            skillCostText.text = nextCost >= 0 ? $"Cost: {nextCost} P" : "MAX";
        }

        UpdateBuyButtonState(def, level, nextCost);
    }

    private SkillDefinition FindSkillDefinition(string skillId)
    {
        if (Data == null || string.IsNullOrEmpty(skillId)) return null;
        return Data.GetSkillDefinition(skillId);
    }

    private int GetNextCost(SkillDefinition def, int currentLevel)
    {
        if (def == null || def.costPerLevel == null) return -1;
        if (currentLevel >= def.maxLevel) return -1;
        int index = currentLevel; // costPerLevel[0] => level 1 cost
        if (index < 0 || index >= def.costPerLevel.Length) return -1;
        return def.costPerLevel[index];
    }

    private void UpdateBuyButtonState(SkillDefinition def, int currentLevel, int nextCost)
    {
        if (buyButton == null)
            return;

        bool canBuy = def != null && Data != null && currentLevel < def.maxLevel && nextCost >= 0 && Data.GetCurrency() >= nextCost;
        buyButton.interactable = canBuy;
    }

    private void TryBuySelected()
    {
        if (string.IsNullOrEmpty(selectedSkillId) || Data == null)
            return;

        bool success = Data.UpgradeSkill(selectedSkillId);
        if (!success)
        {
            // 실패 시에도 UI 상태는 재확인
            RefreshSelectionUI();
            RefreshCurrencyUI();
            return;
        }

        RefreshCurrencyUI();
        RefreshSelectionUI();
    }
}
