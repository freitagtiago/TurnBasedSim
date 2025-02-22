using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueUI : MonoBehaviour, IPanelUI
{
    [SerializeField] private List<GameObject> _panelsToClose = new List<GameObject>();
    [SerializeField] private GameObject _panel;
    [SerializeField] private TextMeshProUGUI _dialogueText;
    [SerializeField] private float _dialogueTime;

    public void ClosePanel()
    {
        _panel.SetActive(false);
    }

    public void OpenPanel()
    {
        CloseOtherPanels();
       // SetDialogue(text, actionToExecute);
        _panel.SetActive(true);
    }

    public void CloseOtherPanels()
    {
        foreach (GameObject panel in _panelsToClose)
        {
            panel.GetComponent<IPanelUI>()?.ClosePanel();
        }
    }

    public void SetDialogue(string textToShow, Action actionToExecute)
    {
        CloseOtherPanels();
        _dialogueText.text = "";
        _panel.SetActive(true);
        StartCoroutine(DialogueRoutine(textToShow, actionToExecute));
    }

    private IEnumerator DialogueRoutine(string message, Action actionToExecute)
    {
        Debug.Log($"AÇÃO: {message}");
        _dialogueText.text = message;
        _panel.SetActive(true);

        yield return new WaitForSecondsRealtime(_dialogueTime);
        
        _panel.SetActive(false);
        _dialogueText.text = "";
        actionToExecute?.Invoke();
    }
}
