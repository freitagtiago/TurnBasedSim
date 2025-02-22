using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ItemButtonUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _name;
    [SerializeField] private TextMeshProUGUI _effect;
    private ItemSO _item;
    public void Setup(ItemSO item)
    {
        _item = item;
        _name.text = _item._name;
        _effect.text = _item._description.ToString();
    }

    public void SelectItem()
    {
        TurnBasedSystem.Instance.SetSelectedItem(_item);
    }
}
