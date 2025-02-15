using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TooltipUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _tooltipText;
    [SerializeField] private GameObject _tooltipObject;

    public void EnableToolTip(string tooltipDescription)
    {
        _tooltipText.text = tooltipDescription;
        _tooltipObject.SetActive(true);
    }

    public void DisableToolTip()
    {
        _tooltipText.text = "";
        _tooltipObject.SetActive(false);
    }
}
