using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SkillPanelUI : MonoBehaviour, IPanelUI
{
    [SerializeField] private List<GameObject> _panelsToClose = new List<GameObject>();

    [SerializeField] private GameObject _panel;
    [SerializeField] private SkillButtonUI _skillButtonPrefab;
    [SerializeField] private Transform _skillContent;
    public void OpenPanel()
    {
        CloseOtherPanels();
        LoadSkills();
        _panel.SetActive(true);
    }

    public void ClosePanel()
    {
        _panel.SetActive(false);
    }

    public void CloseOtherPanels()
    {
        foreach (GameObject panel in _panelsToClose)
        {
            panel.GetComponent<IPanelUI>()?.ClosePanel();
        }
    }

    private void LoadSkills()
    {
        Character currentActor = TurnBasedSystem.Instance.GetCurrentActor();

        DeleteSkillButtons();

        foreach(SkillSO skill in currentActor._skillList)
        {
            SkillButtonUI skillButton = Instantiate(_skillButtonPrefab, _skillContent);
            skillButton.Setup(skill);
        }
    }

    private void DeleteSkillButtons()
    {
        foreach(SkillButtonUI button in _skillContent.GetComponentsInChildren<SkillButtonUI>())
        {
            Destroy(button.gameObject);
        }
    }
}
