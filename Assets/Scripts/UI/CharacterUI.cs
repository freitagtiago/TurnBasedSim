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
    [SerializeField] private Image _sprite;
    private Character _character;

    public void Setup(Character character)
    {
        _character = character;
        _characterName.text = character._name;
        _sprite.sprite = character._baseCharacter._portrait;
        _healthPoints.text = "HP: " + character._currentHP.ToString();
        _speacialPoints.text = "SP: " + character._currentSP.ToString();
    }

    public void UpdateUI()
    {
        _healthPoints.text = "HP: " + _character._currentHP.ToString();
        _speacialPoints.text = "SP: " + _character._currentSP.ToString();


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
}
