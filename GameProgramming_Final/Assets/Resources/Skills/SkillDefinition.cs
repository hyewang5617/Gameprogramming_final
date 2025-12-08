using UnityEngine;

[CreateAssetMenu(fileName = "Skill_", menuName = "GameData/SkillDefinition")]
public class SkillDefinition : ScriptableObject
{
    public string skillId; // unique string value
    public string displayName;
    public string description;
    public int maxLevel = 5;
    public int[] costPerLevel;

    [Header("Gauge (optional)")]
    public bool usesGauge;
    public float[] gaugeMaxPerLevel;
    public float[] drainPerSecPerLevel;
    public float[] regenPerSecPerLevel;

    [Header("Optional per-skill params")]
    public float[] timeScalePerLevel;   // TimeSlow 전용
    public float[] forcePerLevel;       // JetPack 전용
    public int[] extraJumpPerLevel;     // DoubleJump 전용
}
