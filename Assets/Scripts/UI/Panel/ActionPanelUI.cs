using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionPanelUI : MonoBehaviour, IPanelUI
{
    [SerializeField] private List<GameObject> _panelsToClose = new List<GameObject>();
    [SerializeField] private GameObject _panel;

    public void ClosePanel()
    {
        _panel.SetActive(false);
    }

    public void OpenPanel()
    {
        CloseOtherPanels();
        _panel.SetActive(true);
    }

    public void CloseOtherPanels()
    {
        foreach (GameObject panel in _panelsToClose)
        {
            panel.GetComponent<IPanelUI>()?.ClosePanel();
        }
    }
}
