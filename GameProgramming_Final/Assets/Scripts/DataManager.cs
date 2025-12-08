using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// - SkillDefinition ScriptableObject는 Assets/Resources/Skills/에 위치
/// - Stage numbering은 1부터 시작. stageRecords 리스트의 인덱스 = stage - 1
/// </summary>

[Serializable]
public class SkillData
{
    public string skillId;
    public int level; // 0 == 미해금
}

[Serializable]
public class StageRecord
{
    public int bestScore;
    public float bestTime;
    public string bestGrade;
}

[Serializable]
public class PlayerSaveData
{
    public int currency = 0;
    public List<SkillData> skills = new List<SkillData>();
    public int unlockedStage = 1;
    public List<StageRecord> stageRecords = new List<StageRecord>(); // index = stage - 1
}

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    private const string SAVE_KEY = "player_save_data";
    public PlayerSaveData Data = new PlayerSaveData();

    // 런타임 캐시(간단 조회용)
    private Dictionary<string, int> skillDict = new Dictionary<string, int>();

    // SkillDefinition 로드
    private SkillDefinition[] skillDefs;

    [Header("Debug")]
    [SerializeField] private bool unlockAllStagesOnStart = false; // 시작 시 모든 스테이지 해금 (개발용)
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // 필요한 초기화
        LoadSkillDefinitions();
        Load();
        
        // 개발용: 모든 스테이지 해금
        if (unlockAllStagesOnStart)
        {
            UnlockAllStages();
            Debug.Log("[DataManager] 모든 스테이지 해금됨");
        }
        
        // 강제로 Stage 2 이상 해금 (기존 저장 데이터 무시)
        if (Data.unlockedStage < 2)
        {
            Data.unlockedStage = 4;
            Save();
            Debug.Log("[DataManager] Stage 2 이상 강제 해금");
        }
    }

    #region SaveLoad
    public void Save()
    {
        Data.skills = skillDict.Select(kv => new SkillData { skillId = kv.Key, level = kv.Value }).ToList();

        try
        {
            string json = JsonUtility.ToJson(Data, true);
            PlayerPrefs.SetString(SAVE_KEY, json);
            PlayerPrefs.Save();
        }
        catch (Exception e)
        {
            Debug.LogError($"DataManager.Save failed: {e}");
        }
    }

    public void Load()
    {
        try
        {
            if (PlayerPrefs.HasKey(SAVE_KEY))
            {
                string json = PlayerPrefs.GetString(SAVE_KEY);
                var loaded = JsonUtility.FromJson<PlayerSaveData>(json);
                Data = loaded ?? new PlayerSaveData();
            }
            else
            {
                CreateDefaultData();
                Save();
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"DataManager.Load failed: {e}. Using defaults.");
            CreateDefaultData();
        }

        RebuildCaches();
    }

    private void CreateDefaultData()
    {
        Data = new PlayerSaveData
        {
            currency = 0,
            unlockedStage = 4, // 모든 스테이지 기본 해금 (테스트용)
            skills = new List<SkillData>(),
            stageRecords = new List<StageRecord>()
        };
    }

    private void RebuildCaches()
    {
        // 스킬 정보 언팩
        skillDict.Clear();
        if (Data.skills != null)
        {
            foreach (var s in Data.skills)
            {
                if (string.IsNullOrEmpty(s.skillId)) continue;
                skillDict[s.skillId] = Math.Max(0, s.level);
            }
        }

        // stageRecords는 Data 클래스 내 리스트 그대로 사용 (index 기반)
        if (Data.stageRecords == null) Data.stageRecords = new List<StageRecord>();
    }
    #endregion

    #region Currency
    public int GetCurrency() => Data.currency;

    public void AddCurrency(int amount)
    {
        Data.currency += amount;
        Save();
    }

    public bool SpendCurrency(int amount)
    {
        if (Data.currency < amount) return false;
        Data.currency -= amount;
        Save();
        return true;
    }
    #endregion

    #region Skill
    private void LoadSkillDefinitions()
    {
        skillDefs = Resources.LoadAll<SkillDefinition>("Skills");
    }

    private SkillDefinition GetSkillDef(string skillId)
    {
        if (skillDefs == null || skillDefs.Length == 0) LoadSkillDefinitions();
        return skillDefs?.FirstOrDefault(d => d != null && d.skillId == skillId); // 없으면 null
    }

    public SkillDefinition GetSkillDefinition(string skillId)
    {
        return GetSkillDef(skillId);
    }

    public SkillDefinition[] GetAllSkillDefinitions()
    {
        if (skillDefs == null || skillDefs.Length == 0) LoadSkillDefinitions();
        return skillDefs;
    }

    public int GetSkillLevel(string skillId)
    {
        if (string.IsNullOrEmpty(skillId)) return 0;
        if (skillDict.TryGetValue(skillId, out var lvl)) return lvl;
        return 0;
    }

    public bool UpgradeSkill(string skillId)
    {
        var def = GetSkillDef(skillId);
        if (def == null)
        {
            Debug.LogWarning($"TryUpgradeSkill: SkillDefinition not found for '{skillId}'.");
            return false;
        }

        int cur = GetSkillLevel(skillId);
        if (cur >= def.maxLevel) return false;

        int nextIndex = cur; // costPerLevel[0] => level 1 cost
        if (def.costPerLevel == null || nextIndex < 0 || nextIndex >= def.costPerLevel.Length)
        {
            Debug.LogWarning($"TryUpgradeSkill: cost undefined for '{skillId}' level {cur + 1}");
            return false;
        }

        int cost = def.costPerLevel[nextIndex];
        if (Data.currency < cost) return false;

        Data.currency -= cost;
        skillDict[skillId] = cur + 1;
        Save();
        return true;
    }
    #endregion

    #region StageRecord
    private void EnsureStageIndex(int stage)
    {
        if (stage < 1) throw new ArgumentException("stage must be >= 1");
        int idx = stage - 1;
        while (Data.stageRecords.Count <= idx)
        {
            Data.stageRecords.Add(new StageRecord()); // 기본값
        }
    }

    public bool IsStageUnlocked(int stage) => stage <= Data.unlockedStage;

    public void UnlockStage(int stage)
    {
        if (stage > Data.unlockedStage)
        {
            Data.unlockedStage = stage;
            Save();
        }
    }

    public StageRecord GetStageRecord(int stage)
    {
        if (stage < 1) return null;
        int idx = stage - 1;
        if (idx >= 0 && idx < Data.stageRecords.Count) return Data.stageRecords[idx];
        return null;
    }

    public void RecordStageClear(int stage, int score, float time, string grade)
    {
        if (stage < 1) return;
        EnsureStageIndex(stage);
        int idx = stage - 1;
        var existing = Data.stageRecords[idx];

        bool shouldReplace = false;
        if (existing == null) shouldReplace = true;
        else
        {
            if (score > existing.bestScore) shouldReplace = true;
            else if (score == existing.bestScore)
            {
                if (existing.bestTime <= 0f) shouldReplace = true;
                else if (time > 0f && time < existing.bestTime) shouldReplace = true;
            }
        }

        if (shouldReplace)
        {
            Data.stageRecords[idx] = new StageRecord { bestScore = score, bestTime = time, bestGrade = grade };
            Save();
        }
    }
    #endregion

    #region Utilities (개발용)
    public void ClearAllData()
    {
        PlayerPrefs.DeleteKey(SAVE_KEY);
        CreateDefaultData();
        skillDict.Clear();
        Save();
    }

    // 모든 스테이지 해금 (개발/테스트용)
    public void UnlockAllStages()
    {
        Data.unlockedStage = 999; // 충분히 큰 수로 설정
        Save();
    }
    #endregion
}
