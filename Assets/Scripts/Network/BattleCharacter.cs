using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BattleCharacter
{
    public string _baseCharacterSOName;
    public List<SkillSO> _skillList = new List<SkillSO>();
    public List<ItemSO> _itemsList = new List<ItemSO>();

    public CharacterSO GetBaseCharacter()
    {
        CharacterSO character;
        character = Resources.Load<CharacterSO>("Characters/" + _baseCharacterSOName);
        return character;
    }
}
