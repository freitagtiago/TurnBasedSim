using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Skill", menuName = "ScriptableObjects/Skills/Skill", order = 1)]
public class SkillSO : ScriptableObject
{
    [Header ("General Info")]
    public string _name;
    [TextArea]
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
    public bool _applyStatModifier = false;
    public Stats _statDebuff;
    public int _modifierChance = 0;
    public StatModifier _statModifier;

    [Header("Special Skill")]
    public bool _isSpecialSkill = false;
    public float _restoreHPFactor = 0;
    public bool _removeDebuff = false;
    public bool _cureAllStatusConditions = false;
    public List<StatModifier> _modifiersToAdd = new List<StatModifier>();
}