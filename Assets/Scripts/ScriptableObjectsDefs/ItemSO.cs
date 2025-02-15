using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "ScriptableObjects/Item", order = 1)]
public class ItemSO : ScriptableObject
{
    public string _name;
    public string _description;

    public bool _affetEntireParty = false;

    public bool _isHealingItem = false;
    public bool _canRevive = false;
    public bool _cureAllConditions = false;
    public int _hpValueToCure;
    public int _spValueToCure;
    public bool _removeDebuff = false;
    
    public bool _cureStatus = false;
    public StatusCondition _statusConditionToCure;
}
