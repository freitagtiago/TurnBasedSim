using UnityEngine;

[CreateAssetMenu(fileName = "StatusConditionSkill", menuName = "ScriptableObjects/Skills/StatusConditionSkill", order = 5)]
public class StatusConditionSkillSO : SkillSO
{
    [Header("Status Condition Skill Related")]
    public int _minimumTurnCount = 1;
    public int _maximumTurnCount = 3;

}