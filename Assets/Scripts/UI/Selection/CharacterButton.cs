using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class CharacterButton : MonoBehaviour
{
    [SerializeField] private Image _partyMemberImage;
    [SerializeField] private TextMeshProUGUI _partyMemberName;
    private CharacterSO _character;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(LoadCharacter);
    }

    public void Setup(CharacterSO character)
    {
        _character = character;
        _partyMemberImage.sprite = character._portrait;
        _partyMemberName.text = character._name;
    }

    private void LoadCharacter()
    {
        SelectionUIHandler.OnSelectCharacter.Invoke(_character);
    }
}
