using UnityEngine;

[CreateAssetMenu(fileName = "HeallingSkill", menuName = "ScriptableObjects/Skills/HeallingSkill", order = 4)]
public class HealingSkillSO : SkillSO
{
    [Header("Healing Skill Related")]
    public bool _canRevive = false;
    public float _cureValue;
    public bool _cureStatusCondition;
    public bool _removeDebuffs = false;
}