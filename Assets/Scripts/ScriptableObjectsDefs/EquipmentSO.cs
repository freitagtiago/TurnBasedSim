using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Equipment", menuName = "ScriptableObjects/Equipment", order = 1)]
public class EquipmentSO : ScriptableObject
{
    public string _name;
    public string _description;
    public List<Stat> _statModifier = new List<Stat>();
    public SkillSO _basicSkill;
}
