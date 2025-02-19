using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class CharacterUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _characterName;
    [SerializeField] private TextMeshProUGUI _healthPoints;
    [SerializeField] private TextMeshProUGUI _speacialPoints;
    [SerializeField] private TextMeshProUGUI _modifierWarning;
    [SerializeField] private TextMeshProUGUI _type;
    [SerializeField] private Slider _hpSlider;
    [SerializeField] private Slider _spSlider;
    [SerializeField] private Image _sprite;
    [SerializeField] private GameObject _shield;
    private Character _character;

    public void Setup(Character character)
    {
        _character = character;
        _characterName.text = character._name;
        _sprite.sprite = character._baseCharacter._portrait;

        _type.color = Utils.HexToColor(Utils.GetHexCodeForElement(_character._type));
        _type.text = _character._type.ToString();

        _hpSlider.maxValue = _character._currentHP;
        _spSlider.maxValue = _character._maxSP;

        _healthPoints.text = $"HP: {_character._currentHP}/{_character._maxHP}";
        _hpSlider.value = _character._currentHP;

        _speacialPoints.text = $"HP: {_character._currentSP}/{_character._maxSP}";
        _spSlider.value = _character._currentSP;

        _modifierWarning.text = GetModifierText();
    }

    public void UpdateUI()
    {
        _healthPoints.text = $"HP: {_character._currentHP}/{_character._maxHP}";
        _hpSlider.value = _character._currentHP;

        _speacialPoints.text = $"HP: {_character._currentSP}/{_character._maxSP}";
        _spSlider.value = _character._currentSP;
        _modifierWarning.text = GetModifierText();

        if (_hpSlider.value == 0)
        {
            _hpSlider.fillRect.gameObject.SetActive(false);
        }
        else
        {
            _hpSlider.fillRect.gameObject.SetActive(true);
        }

        if (_spSlider.value == 0)
        {
            _spSlider.fillRect.gameObject.SetActive(false);
        }
        else
        {
            _spSlider.fillRect.gameObject.SetActive(true);
        }

        _shield.SetActive((_character._inDefensiveState 
                            && _character._currentHP > 0));

       switch (_character._currentStatusCondition)
       {
           case StatusCondition.Poisoned:
               _characterName.color = Color.magenta;
               break;
           case StatusCondition.Paralyzed:
               _characterName.color = Color.yellow;
               break;
           case StatusCondition.Blind:
               _characterName.color = Color.gray;
               break;
           case StatusCondition.Exausted:
               _characterName.color = Color.black;
               break;
           case StatusCondition.Confused:
               _characterName.color = Color.green;
               break;
           case StatusCondition.Burned:
               _characterName.color = Color.red;
               break;
           case StatusCondition.Freezed:
               _characterName.color = Color.cyan;
               break;
           default:
               _characterName.color = Color.white;
               break;
       }
    }

    private string GetModifierText()
    {
        string message = "";

        foreach(StatModifier modifier in _character._statsModifiers)
        {
            if (modifier._isPermanent)
            {
                continue;
                //message += $"{modifier._stat} : {modifier._stage}" + Environment.NewLine;
            }
            else
            {
                if(modifier._value < 0)
                {
                    message += $"<color=#F3BEBE>{modifier._stat} : {modifier._stage}</color>" + Environment.NewLine;
                }
                else
                {
                    message += $"<color=#0000FF>{modifier._stat} : {modifier._stage}</color>" + Environment.NewLine;
                }
            }
        }
        return message;
    }
}
