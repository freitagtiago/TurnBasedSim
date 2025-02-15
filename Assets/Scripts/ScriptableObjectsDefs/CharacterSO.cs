using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Character", menuName = "ScriptableObjects/Character", order = 1)]
public class CharacterSO : ScriptableObject
{
    public string _name;
    public string _description;

    public Sprite _portrait;
    public AnimatorOverrideController _animatorOverride;

    [Header("Battle Info")]
    public ElementType _type;
    public EquipmentSO _equipment;
    public List<Stat> _stats = new List<Stat> ();
    public List<SkillSO> _availableSkills = new List<SkillSO> ();
    public List<ItemSO> _availableItens = new List<ItemSO> ();

    public Stat GetStat(Stats statToGet)
    {
        foreach(Stat stat in _stats)
        {
            if(stat._stat == statToGet)
            {
                return stat;
            }
        }

        return null;
    }
}
