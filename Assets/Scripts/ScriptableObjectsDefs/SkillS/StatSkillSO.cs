using UnityEngine;

[CreateAssetMenu(fileName = "StatSkill", menuName = "ScriptableObjects/Skills/StatSkill", order = 6)]
public class StatSkillSO : SkillSO
{
    [Header("Stat Skill Related")]
    public StatModifier _statModifier;
}