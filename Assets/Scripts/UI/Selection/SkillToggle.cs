using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillToggle : MonoBehaviour
{
    [SerializeField] public Toggle _toggle;
    [SerializeField] private SkillSO _skill;
    [SerializeField] private TextMeshProUGUI _skillName;
    [SerializeField] private TextMeshProUGUI _skillCost;
    [SerializeField] private TextMeshProUGUI _skillType;
    [SerializeField] private TextMeshProUGUI _skillDescription;

    private SelectionUIHandler _uiHandler;

    private void OnEnable()
    {
        _toggle.onValueChanged.AddListener(OnValueChanged);
    }

    private void OnDisable()
    {
        _toggle.onValueChanged.RemoveAllListeners();
    }

    public void Setup(SkillSO skill, SelectionUIHandler uiHandler)
    {
        _uiHandler = uiHandler;
        _skill = skill;
        _skillName.text = skill._name;
        _skillCost.text = skill._cost.ToString();
        _skillType.text = skill._type.ToString();
        _skillDescription.text = skill._description;
    }

    private void OnValueChanged(bool value)
    {
        _uiHandler.ValidateSkill(_skill, value);
    }
}
