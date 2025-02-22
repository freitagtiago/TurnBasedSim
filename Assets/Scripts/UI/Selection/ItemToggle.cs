using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemToggle : MonoBehaviour
{
    [SerializeField] public Toggle _toggle;
    [SerializeField] private ItemSO _item;
    [SerializeField] private TextMeshProUGUI _itemName;
    [SerializeField] private TextMeshProUGUI _itemDescription;

    private SelectionUIHandler _uiHandler;

    private void OnEnable()
    {
        _toggle.onValueChanged.AddListener(OnValueChanged);
    }

    private void OnDisable()
    {
        _toggle.onValueChanged.RemoveAllListeners();
    }

    public void Setup(ItemSO item, SelectionUIHandler uiHandler)
    {
        _uiHandler = uiHandler;
        _item = item;
        _itemName.text = _item._name;
        _itemDescription.text = _item._description;
    }

    private void OnValueChanged(bool value)
    {
        _uiHandler.ValidateItem(_item, value);
    }
}
