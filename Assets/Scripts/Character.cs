using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Character
{
    [SerializeField] public CharacterSO _baseCharacter { get; private set; }
    [SerializeField] private Animator _animator;

    [SerializeField] public StatusCondition _currentStatusCondition { get; private set; }
    [SerializeField] public int _remainingTurnsStatusCondition { get; private set; }

    [SerializeField] public string _name { get; private set; }
    [SerializeField] public string _description { get; private set; }
    [SerializeField] public  ElementType _type { get; private set; }
    [SerializeField] public EquipmentSO _equipment { get; private set; }

    [SerializeField] public int _currentHP = 0;
    [SerializeField] public int _currentSP = 0;

    [SerializeField] public List<Stat> _statsModifiers { get; private set; } = new List<Stat>();
    [SerializeField] public List<Stat> _stats { get; private set; } = new List<Stat>();

    [SerializeField] public List<SkillSO> _skillList { get; private set; } = new List<SkillSO>();
    [SerializeField] public List<ItemSO> _itemsList { get; private set; } = new List<ItemSO>();

    public int _side = 0;

    public void SetupCharacter(CharacterSO baseCharacter, int side)
    {
        _baseCharacter = baseCharacter;
        _name = baseCharacter._name;
        _description = baseCharacter._description;
        _type = baseCharacter._type;
        _stats = _baseCharacter._stats;
        _skillList = baseCharacter._availableSkills;
        _itemsList = baseCharacter._availableItens;
        _equipment = baseCharacter._equipment;

        _side = side;
        foreach (Stat stat in _equipment._statModifier)
        {
            ApplyStatModifier(stat);
        }

        _currentHP = _baseCharacter.GetStat(Stats.HealthPoints)._value + GetStatModifier(Stats.HealthPoints);
        _currentSP = _baseCharacter.GetStat(Stats.SpecialPoints)._value;
    }

   public void ApplyStatusCondition(StatusCondition condition)
    {
        if(_currentStatusCondition != StatusCondition.None
            && _currentStatusCondition != condition)
        {
            return;
        }
        _currentStatusCondition = condition;

        _remainingTurnsStatusCondition = Mathf.Clamp(6 - (GetStat(Stats.Luck) / 10), 1,4);
    }

    public void ApplyStatModifier(Stat stat)
    {
        bool statFound = false;
        foreach (Stat statModifier in _statsModifiers)
        {
            if(statModifier._stat == stat._stat)
            {
                statModifier._value += stat._value;
                statFound = true;
                break;
            }
        }

        if (!statFound)
        {
            _statsModifiers.Add(stat);
        }
    }

    public int GetStat(Stats statToGet)
    {
        int baseStatValue = 0;
        int modifierStatValue = 0;

        for(int i = 0; i < _stats.Count; i++)
        {
            if (_stats[i]._stat == statToGet)
            {
                baseStatValue = _stats[i]._value;
                break;
            }
        }

        for (int i = 0; i < _statsModifiers.Count; i++)
        {
            if (_statsModifiers[i]._stat == statToGet)
            {
                modifierStatValue = _statsModifiers[i]._value;
                break;
            }
        }
        return baseStatValue + modifierStatValue;
    }

    private int GetStatModifier(Stats stat)
    {
        int statModifierValue = 0;
        foreach(Stat modifier in _statsModifiers)
        {
            if(modifier._stat == stat)
            {
                statModifierValue += modifier._value;
            }
        }

        return statModifierValue;
    }

    public void ApplyDamage(int damage)
    {
        int maxHP = _baseCharacter.GetStat(Stats.HealthPoints)._value + GetStatModifier(Stats.HealthPoints);
        _currentHP = Mathf.Clamp(_currentHP + damage, 0, maxHP);
    }

    public bool HandleStatusCondition()
    {
        bool blockAction = false;
        if(_currentStatusCondition == StatusCondition.None)
        {
            return blockAction;
        }

        _remainingTurnsStatusCondition--;

        if(_remainingTurnsStatusCondition == 0)
        {
            return blockAction;
        }

        if (_remainingTurnsStatusCondition == 0)
        {
             _currentStatusCondition = StatusCondition.None;
        }

        switch (_currentStatusCondition)
        {
            case StatusCondition.Poisoned:
                ApplyDamage((int)Mathf.Round(_currentHP * 0.1f * -1));
                break;
            case StatusCondition.Paralyzed:
                blockAction = Random.Range(0, 101) < 50;
                break;
            case StatusCondition.Blind:
                break;
            case StatusCondition.Exausted:
                break;
            case StatusCondition.Confused:
                blockAction = Random.Range(0, 101) < 50;
                if (blockAction)
                {
                    ApplyDamage(_equipment._basicSkill._baseForce);
                }
                break;
            case StatusCondition.Burned:

                ApplyDamage((int)Mathf.Round(_currentHP * (Random.Range(0.05f,0.21f) * -1)));
                break;
            case StatusCondition.Freezed:
                ApplyDamage((int)Mathf.Round(_currentHP * 0.05f * -1));
                blockAction = true;
                break;
        }
        return blockAction;
    }
}
