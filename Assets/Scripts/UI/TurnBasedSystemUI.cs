using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class TurnBasedSystemUI : MonoBehaviour
{
    [SerializeField] private CharacterUI _characterUIPrefab;
    [SerializeField] private Transform _sideA;
    [SerializeField] private Transform _sideB;

    public CharacterUI[] _charactersSideAUI = new CharacterUI[4];
    public CharacterUI[] _charactersSideBUI = new CharacterUI[4];

    [SerializeField] private GameObject _actionPanelUI;
    [SerializeField] private GameObject _dialoguePanelUI;
    [SerializeField] private GameObject _skillPanelUI;
    [SerializeField] private GameObject _itenPanelUI;
    [SerializeField] private GameObject _specialPanelUI;
    [SerializeField] private GameObject _targetPanelUI;

    [SerializeField] private TextMeshProUGUI _actionPointsSideA;
    [SerializeField] private TextMeshProUGUI _actionPointsSideB;
    [SerializeField] private GameObject _specialActionButton;
    [SerializeField] private Slider _sliderA;
    [SerializeField] private Slider _sliderB;

    public void SetupCharacterSlot(Character character, int index,int side)
    {
        if (side == 0)
        {
            _charactersSideAUI[index] = Instantiate(_characterUIPrefab, _sideA);
            _charactersSideAUI[index].Setup(character);
        }
        else
        {
            _charactersSideBUI[index] = Instantiate(_characterUIPrefab, _sideB);
            _charactersSideBUI[index].Setup(character);
        }

        
    }
    
    public void EndBattle()
    {
        for (int i = 0; i < _charactersSideAUI.Length; i++)
        {
            Destroy(_charactersSideAUI[i].gameObject);
        }
        for (int i = 0; i < _charactersSideBUI.Length; i++)
        {
            Destroy(_charactersSideBUI[i].gameObject);
        }
        _charactersSideAUI = new CharacterUI[4];
        _charactersSideBUI = new CharacterUI[4];

        _dialoguePanelUI.GetComponent<IPanelUI>().ClosePanel();
        _skillPanelUI.GetComponent<IPanelUI>().ClosePanel();
        _itenPanelUI.GetComponent<IPanelUI>().ClosePanel();
        _specialPanelUI.GetComponent<IPanelUI>().ClosePanel();
        _actionPanelUI.GetComponent<IPanelUI>().ClosePanel();
        _targetPanelUI.GetComponent<IPanelUI>().ClosePanel();
    }

    public void UpdateCharacterSlotUI(int index, int side)
    {
        if (side == 0)
        {
            _charactersSideAUI[index].UpdateUI();
        }
        else
        {
            _charactersSideBUI[index].UpdateUI();
        }
    }

    public void SetupPlayerAction(Character currentActor)
    {
        _dialoguePanelUI.GetComponent<IPanelUI>().ClosePanel();
        _skillPanelUI.GetComponent<IPanelUI>().ClosePanel();
        _itenPanelUI.GetComponent<IPanelUI>().ClosePanel();
        _specialPanelUI.GetComponent<IPanelUI>().ClosePanel();
        _actionPanelUI.GetComponent<IPanelUI>().OpenPanel();

        if(TurnBasedSystem.Instance._currentActionPointsSideA > 100
            && !TurnBasedSystem.Instance._isCharging
            && TurnBasedSystem.Instance.GetCurrentActor() == TurnBasedSystem.Instance._charactersSideA[0])
        {
            _specialActionButton.SetActive(true);
        }
        else
        {
            _specialActionButton.SetActive(false);
        }
    }

    public void SetupEnemyAction(Character currentActor)
    {
        _actionPanelUI.GetComponent<IPanelUI>().ClosePanel();
        _dialoguePanelUI.GetComponent<IPanelUI>().OpenPanel();
        // Desativar UI de interação
        // AGuardar retorno de IA
    }

    public void SetupTargetPanel(SkillSO skill)
    {
        _targetPanelUI.GetComponent<IPanelUI>().OpenPanel();
    }

    public void SetupTargetItemPanel(ItemSO item)
    {
        _targetPanelUI.GetComponent<IPanelUI>().OpenPanel();
    }

    public void CloseTargetPanel()
    {
        _targetPanelUI.GetComponent<IPanelUI>().ClosePanel();
    }

    public void CloseDialoguePanel()
    {
        _dialoguePanelUI.GetComponent<IPanelUI>().ClosePanel();
    }

    public void SetupDialoguePanel(string message, Action action) 
    {
        _dialoguePanelUI.GetComponent<DialogueUI>().SetDialogue(message, action);
    }

    public void BasicAttackActionButton()
    {
        _dialoguePanelUI.GetComponent<IPanelUI>().ClosePanel();
        _skillPanelUI.GetComponent<IPanelUI>().ClosePanel();
        _itenPanelUI.GetComponent<IPanelUI>().ClosePanel();
        _specialPanelUI.GetComponent<IPanelUI>().ClosePanel();
        TurnBasedSystem.Instance.SetBasicAction(true);
    }

    public void SpecialAttackActionButton()
    {
        _specialPanelUI.GetComponent<IPanelUI>().OpenPanel();
    }

    public void SkillsActionButton()
    {
        _skillPanelUI.GetComponent<IPanelUI>().OpenPanel();
    }

    public void ItensActionButton()
    {
        _itenPanelUI.GetComponent<IPanelUI>().OpenPanel();
    }

    public void DefendActionButton()
    {
        _dialoguePanelUI.GetComponent<IPanelUI>().ClosePanel();
        _skillPanelUI.GetComponent<IPanelUI>().ClosePanel();
        _itenPanelUI.GetComponent<IPanelUI>().ClosePanel();
        _specialPanelUI.GetComponent<IPanelUI>().ClosePanel();
        TurnBasedSystem.Instance.SetBasicAction(false);
    }

    public void ExitActionButton()
    {
        _dialoguePanelUI.GetComponent<IPanelUI>().ClosePanel();
        _skillPanelUI.GetComponent<IPanelUI>().ClosePanel();
        _itenPanelUI.GetComponent<IPanelUI>().ClosePanel();
        _specialPanelUI.GetComponent<IPanelUI>().ClosePanel();
        StartCoroutine(TurnBasedSystem.Instance.EndBattle());
    }

    public void UpdateActionPointsUI()
    {
        _actionPointsSideA.text = $"{TurnBasedSystem.Instance._currentActionPointsSideA}/{TurnBasedSystem.Instance._maxActionPoints}";
        _sliderA.value = TurnBasedSystem.Instance._currentActionPointsSideA;
        _actionPointsSideB.text = $"{TurnBasedSystem.Instance._currentActionPointsSideB}/{TurnBasedSystem.Instance._maxActionPoints}";
        _sliderB.value = TurnBasedSystem.Instance._currentActionPointsSideB;
    }
}
