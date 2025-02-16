using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SkillButtonUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _name;
    [SerializeField] private TextMeshProUGUI _type;
    [SerializeField] private TextMeshProUGUI _power;
    [SerializeField] private TextMeshProUGUI _cost;
    private SkillSO _skill;
    public void Setup(SkillSO skill)
    {
        _skill = skill;
        _name.text = _skill._name;
        _type.text = _skill._type.ToString();
        _power.text = _skill._baseForce.ToString();
        _cost.text = _skill._cost.ToString();

        if(TurnBasedSystem.Instance.GetCurrentActor()._currentSP < _skill._cost)
        {
            GetComponent<Button>().interactable = false;
        }

    }

    public void SelectSkill()
    {
        TurnBasedSystem.Instance.SetSelectedSkill(_skill);
    }
}
