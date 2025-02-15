using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TargetButtonUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _targetName;
    private Character _target;

    public void SetupTarget(Character target)
    {
        _target = target;
        _targetName.text = _target._name;
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
