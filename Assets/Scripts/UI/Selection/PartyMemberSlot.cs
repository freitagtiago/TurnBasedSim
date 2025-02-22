using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class PartyMemberSlot : MonoBehaviour
{
    [SerializeField] private Character _character;
    [SerializeField] private Image _characterImage;
    public void Setup(Character character)
    {
        _character = character;
        _characterImage.sprite = character._baseCharacter._portrait;
    }

    public void OnClick(int buttonIndex)
    {
        SelectionUIHandler._currentSlot = buttonIndex;
        SelectionUIHandler.OnSelectCharacter.Invoke(_character._baseCharacter);
    }
}
