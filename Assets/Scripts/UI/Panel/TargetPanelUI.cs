using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class TargetPanelUI : MonoBehaviour, IPanelUI
{
    [SerializeField] private List<GameObject> _panelsToClose;
    [SerializeField] private GameObject _panel;
    [SerializeField] private string _defaultTitle = "Escolha o alvo de {0}";
    [SerializeField] private TextMeshProUGUI _title;
    [SerializeField] private TargetButtonUI _targetButtonPrefab;
    [SerializeField] private Transform _targetButtonsParent;
    [SerializeField] private TurnBasedSystemUI _turnBaseSystemUI;
    public void CloseOtherPanels()
    {
        foreach (GameObject panel in _panelsToClose)
        {
            panel.GetComponent<IPanelUI>()?.ClosePanel();
        }
    }

    public void ClosePanel()
    {
        _title.text = "";
        _panel.SetActive(false);
    }

    public void OpenPanel()
    {
        _title.text = _defaultTitle;
        CloseOtherPanels();
        DeleteAllButtons();
        SetupTargetList();
        _panel.SetActive(true);
    }

    public void ReturnToActionPanel()
    {
        ClosePanel();
        _turnBaseSystemUI.SetupPlayerAction(TurnBasedSystem.Instance.GetCurrentActor());
    }

    private void DeleteAllButtons()
    {
        foreach (TargetButtonUI button in _targetButtonsParent.GetComponentsInChildren<TargetButtonUI>())
        {
            Destroy(button.gameObject);
        }
    }

    private void SetupTargetList()
    {
        SkillSO skill = TurnBasedSystem.Instance._selectedSkill;

        if(skill != null)
        {
            _title.text = string.Format(_defaultTitle, skill._name);

            if (skill is HealingSkillSO)
            {
                bool canRevive = (skill as HealingSkillSO)._canRevive;
                if (TurnBasedSystem.Instance.GetCurrentActor()._side == 0)
                {
                    foreach (Character character in TurnBasedSystem.Instance._charactersSideA)
                    {
                        if(character._currentHP > 0 
                            || canRevive)
                        {
                            TargetButtonUI targetButton = Instantiate(_targetButtonPrefab, _targetButtonsParent);
                            targetButton.SetupTarget(character);
                        }
                    }
                }
                else
                {
                    foreach (Character character in TurnBasedSystem.Instance._charactersSideB)
                    {
                        if (character._currentHP > 0
                            || canRevive)
                        {
                            TargetButtonUI targetButton = Instantiate(_targetButtonPrefab, _targetButtonsParent);
                            targetButton.SetupTarget(character);
                        }
                    }
                }
            }
            else if (skill is MagicalSkillSO
                      || skill is PhysicalSkillSO
                      || skill is StatSkillSO
                      || skill is StatusConditionSkillSO)
            {
                if (TurnBasedSystem.Instance.GetCurrentActor()._side == 0)
                {
                    foreach (Character character in TurnBasedSystem.Instance._charactersSideB)
                    {
                        if(character._currentHP > 0)
                        {
                            TargetButtonUI targetButton = Instantiate(_targetButtonPrefab, _targetButtonsParent);
                            targetButton.SetupTarget(character);
                        }
                    }
                }
                else
                {
                    foreach (Character character in TurnBasedSystem.Instance._charactersSideA)
                    {
                        if(character._currentHP > 0)
                        {
                            TargetButtonUI targetButton = Instantiate(_targetButtonPrefab, _targetButtonsParent);
                            targetButton.SetupTarget(character);
                        }
                    }
                }
            }
            else
            {
                Debug.Log("Tipo de skill genérico");
            }
        }
        else
        {
            ItemSO item = TurnBasedSystem.Instance._selectedItem;
            _title.text = string.Format(_defaultTitle, item._name);

            if (TurnBasedSystem.Instance.GetCurrentActor()._side == 0)
            {
                foreach (Character character in TurnBasedSystem.Instance._charactersSideA)
                {
                    if(character._currentHP > 0
                       || item._canRevive)
                    {
                        TargetButtonUI targetButton = Instantiate(_targetButtonPrefab, _targetButtonsParent);
                        targetButton.SetupTarget(character);
                    }
                }
            }
            else
            {
                foreach (Character character in TurnBasedSystem.Instance._charactersSideB)
                {
                    if (character._currentHP > 0
                       || item._canRevive)
                    {
                        TargetButtonUI targetButton = Instantiate(_targetButtonPrefab, _targetButtonsParent);
                        targetButton.SetupTarget(character);
                    }
                }
            }
        }
    }
}
