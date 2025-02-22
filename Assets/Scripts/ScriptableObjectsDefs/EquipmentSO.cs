using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Equipment", menuName = "ScriptableObjects/Equipment", order = 1)]
public class EquipmentSO : ScriptableObject
{
    public string _name;
    [TextArea]
    public string _description;
    public List<StatModifier> _statModifier = new List<StatModifier>();
    public WeaponDamageType _weaponDamageType;
    public SkillSO _basicSkill;
}
