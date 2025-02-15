using UnityEngine;

[CreateAssetMenu(fileName = "StatSkill", menuName = "ScriptableObjects/Skills/StatSkill", order = 6)]
public class StatSkillSO : SkillSO
{
    [Header("Stat Skill Related")]
    public bool _isBuff = true;
    public Stats _stat;
    public int _percentEffect;
    public int _minimumTurnCount = 1;
    public int _maximumTurnCount = 3;
}