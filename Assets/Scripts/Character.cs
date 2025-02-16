using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

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

    [SerializeField] public List<StatModifier> _statsModifiers { get; private set; } = new List<StatModifier>();
    [SerializeField] public List<Stat> _stats { get; private set; } = new List<Stat>();

    [SerializeField] public List<SkillSO> _skillList { get; private set; } = new List<SkillSO>();
    [SerializeField] public List<ItemSO> _itemsList { get; private set; } = new List<ItemSO>();

    public int _side = 0;
    public bool _inDefensiveState = false;

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
        foreach (StatModifier stat in _equipment._statModifier)
        {
            ApplyStatModifier(stat);
        }

        _currentHP = _baseCharacter.GetStat(Stats.HealthPoints)._value + GetStatModifier(Stats.HealthPoints);
        _currentSP = _baseCharacter.GetStat(Stats.SpecialPoints)._value;
    }

   public void ApplyStatusCondition(StatusCondition condition)
    {
        if(_currentStatusCondition != StatusCondition.None
            || _currentStatusCondition == condition)
        {
            return;
        }
        _currentStatusCondition = condition;

        if(condition == StatusCondition.Freezed)
        {
            _remainingTurnsStatusCondition = 10;
        }
        else
        {
            _remainingTurnsStatusCondition = Mathf.Clamp(6 - (GetStat(Stats.Luck) / 10), 1, 4);
        }
    }

    public void ApplyStatModifier(StatModifier statModifier)
    {
        if (!statModifier._isPermanent)
        {
            statModifier._remainingTurns = Mathf.Clamp(statModifier._maximumTurns * 2 - (GetStat(Stats.Luck) / 10), statModifier._minumumTurns, statModifier._maximumTurns);

            if(statModifier._modifierValue != 0)
            {
                statModifier._value = statModifier._modifierValue;
            }
            else
            {
                statModifier._value = (GetStat(statModifier._stat) * statModifier._modifierPercent) / 100;
            }
        }
        _statsModifiers.Add(statModifier);
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
        foreach(StatModifier modifier in _statsModifiers)
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
            _currentStatusCondition = StatusCondition.None;
            return blockAction;
        }

        if (_remainingTurnsStatusCondition == 0)
        {
             _currentStatusCondition = StatusCondition.None;
        }

        switch (_currentStatusCondition)
        {
            case StatusCondition.Poisoned:
                ApplyDamage((int)Mathf.Round(_currentHP * 0.15f * -1));
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
                ApplyDamage((int)Mathf.Round(_currentHP * 0.1f * -1));

                bool defrostChance = Random.Range(0, 11) >= _remainingTurnsStatusCondition;

                if (defrostChance)
                {
                    _remainingTurnsStatusCondition = 1;
                }

                blockAction = true;
                break;
        }
        return blockAction;
    }

    public void HandleStatModifier()
    {
        for (int i = _statsModifiers.Count - 1; i >= 0; i--)
        {
            if (_statsModifiers[i]._isPermanent)
            {
                continue;
            }

            _statsModifiers[i]._remainingTurns--;

            if (_statsModifiers[i]._remainingTurns == 0) 
            {
                _statsModifiers.RemoveAt(i);
            }
        }
    }

    public void ApplyItem(ItemSO item)
    {
        if (item._healHP)
        {
            ApplyDamage(item._hpValueToCure);
        }

        if (item._cureSP)
        {
            RecoverSP(item._spValueToCure);
        }

        if (item._removeDebuff)
        {
            RemoveAllDebuffs();
        }

        if (item._cureStatusCondition)
        {
            ApplyStatusCondition(StatusCondition.None);
        }
    }

    public void ReduceSP(int cost)
    {
        _currentSP = _currentSP - cost;
    }

    public void RemoveAllDebuffs()
    {
        for (int i = _statsModifiers.Count - 1; i >= 0; i--)
        {
            if (_statsModifiers[i]._isPermanent
                && _statsModifiers[i]._value < 0)
            {
                _statsModifiers.RemoveAt(i);
            }
        }
    }

    public void RecoverSP(int valueToRecover)
    {
        int maxSP = _baseCharacter.GetStat(Stats.SpecialPoints)._value + GetStatModifier(Stats.SpecialPoints);
        _currentSP = Mathf.Clamp( _currentSP + valueToRecover, 0, maxSP);
    }
}
