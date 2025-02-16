using UnityEngine;

[CreateAssetMenu(fileName = "Skill", menuName = "ScriptableObjects/Skills/Skill", order = 1)]
public class SkillSO : ScriptableObject
{
    [Header ("General Info")]
    public string _name;
    public string _description;
    public ElementType _type;
    public int _cost;
    public int _accuracy;
    public int _baseForce;
    public bool _affectAll = false;
    public bool _isBasicSkill = false;

    [Header ("Status Condition Chance")]
    public bool _causeStatusCondition = false;
    public StatusCondition _statusCondition;
    public int _statusConditionChance = 0;

    [Header("Debuff Condition Chance")]
    public bool _causeDebuff = false;
    public Stats _statDebuff;
    public int _debuffChance = 0;
    public StatModifier _statModifier;
}