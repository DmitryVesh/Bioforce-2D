using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TabGroup : MonoBehaviour
{
    [SerializeField] private Color IdleColor;
    [SerializeField] private Color HoverColor;
    [SerializeField] private Color SelectedColor;

    private List<TabButton> TabButtons { get; set; }
    private TabButton SelectedTabButton { get; set; }

    public void OnTabButtonHover(TabButton tabButton)
    {
        ResetNonSelectedButtons();
        if (tabButton != SelectedTabButton)
            tabButton.SetBackgroundColor(HoverColor);
    }
    public void OnTabButtonSelect(TabButton tabButton)
    {
        SelectTabButton(tabButton);
        ResetNonSelectedButtons();
    }
    public void OnTabButtonExit(TabButton tabButton)
    {
        ResetNonSelectedButtons();
    }

    protected virtual void Start()
    {
        TabButtons = GetComponentsInChildren<TabButton>(true).ToList();
        foreach (TabButton button in TabButtons)
        {
            button.SetParentTabGroup(this);
            ResetButton(button);
        }
        SetDefaultSelectedButton();
    }
    protected virtual void SelectTabButton(TabButton tabButton)
    {
        tabButton.SetBackgroundColor(SelectedColor);
        SelectedTabButton = tabButton;
    }

    private void SetDefaultSelectedButton() =>
        SelectTabButton(TabButtons[0]);
    private void ResetNonSelectedButtons()
    {
        foreach (TabButton tabButton in TabButtons)
        {
            if (tabButton != SelectedTabButton) 
                ResetButton(tabButton);
        }
    }
    private void ResetButton(TabButton tabButton) =>
        tabButton.SetBackgroundColor(IdleColor);
}
