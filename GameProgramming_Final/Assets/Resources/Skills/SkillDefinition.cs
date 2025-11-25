using UnityEngine;

[CreateAssetMenu(fileName = "Skill_", menuName = "GameData/SkillDefinition")]
public class SkillDefinition : ScriptableObject
{
    public string skillId; // unique string value
    public string displayName;
    public string description;
    public int maxLevel = 5;
    public int[] costPerLevel;
}