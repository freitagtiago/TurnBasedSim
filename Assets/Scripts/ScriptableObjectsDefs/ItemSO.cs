using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "ScriptableObjects/Item", order = 1)]
public class ItemSO : ScriptableObject
{
    public string _name;
    [TextArea]
    public string _description;

    public bool _affetEntireParty = false;

    [Header("HP")]
    public bool _healHP = false;
    public bool _canRevive = false;
    public int _hpValueToCure;

    [Header("SP")]
    public bool _cureSP = false;
    public int _spValueToCure;

    [Header("Stats")]
    public bool _removeDebuff = false;

    [Header("")]
    public bool _cureStatusCondition = false;
    public bool _cureAllConditions = false;
    public StatusCondition _statusConditionToCure;
}
