using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TargetButtonUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _targetName;
    private Character _target;

    public void SetupTarget(Character target)
    {
        _target = target;
        _targetName.text = _target._name;

        GetComponent<Button>().interactable = !BlockInteraction();
    }

    private bool BlockInteraction()
    {
        SkillSO skill = TurnBasedSystem.Instance._selectedSkill;

        if (skill is HealingSkillSO
            && _target._currentHP == _target._maxHP)
        {
            return true;
        }

        if(skill is StatSkillSO)
        {
            StatSkillSO statSkill = skill as StatSkillSO;

            foreach(StatModifier modifier in _target._statsModifiers)
            {
                if(statSkill._statModifier._modifierFactor > 1
                    && !modifier._isPermanent
                    && statSkill._statModifier._stat == modifier._stat
                    && modifier._stage == 4
                    && modifier._modifierFactor > 1)
                {
                    return true;
                }
                else if (statSkill._statModifier._modifierFactor < 1
                    && !modifier._isPermanent
                    && statSkill._statModifier._stat == modifier._stat
                    && modifier._stage == 4
                    && modifier._modifierFactor < 1)
                {
                    return true;
                }
            }
        }

        if(skill is StatusCondition)
        {
            if(_target._currentStatusCondition != StatusCondition.None)
            {
                return true;
            }
        }
        return false;
    }

    public void SelectTarget()
    {
        if (TurnBasedSystem.Instance._selectedSkill != null)
        {
            TurnBasedSystem.Instance.ApplySkillOnTarget(_target);
        }
        else
        {
            TurnBasedSystem.Instance.ApplyItemOnTarget(_target);
        }
        
    }
}
