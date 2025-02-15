using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ItenPanelUI : MonoBehaviour, IPanelUI
{
    [SerializeField] private List<GameObject> _panelsToClose = new List<GameObject>();

    [SerializeField] private GameObject _panel;
    [SerializeField] private ItemButtonUI _itemButtonPrefab;
    [SerializeField] private Transform _itemContent;

    public void ClosePanel()
    {
        _panel.SetActive(false);
    }

    public void OpenPanel()
    {
        CloseOtherPanels();
        LoadItems();
        _panel.SetActive(true);
    }

    public void CloseOtherPanels()
    {
        foreach (GameObject panel in _panelsToClose)
        {
            panel.GetComponent<IPanelUI>()?.ClosePanel();
        }
    }

    private void LoadItems()
    {
        Character currentActor = TurnBasedSystem.Instance.GetCurrentActor();

        DeleteItemButtons();

        foreach (ItemSO item in currentActor._itemsList)
        {
            ItemButtonUI itemButton = Instantiate(_itemButtonPrefab, _itemContent);
            itemButton.Setup(item);
        }
    }

    private void DeleteItemButtons()
    {
        foreach (ItemButtonUI button in _itemContent.GetComponentsInChildren<ItemButtonUI>())
        {
            Destroy(button.gameObject);
        }
    }
}
