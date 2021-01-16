using System.Collections.Generic;
using UnityEngine;

public class TabGroupPages : TabGroup
{
    [SerializeField] private GameObject PageHolder;

    private List<GameObject> Pages { get; set; } = new List<GameObject>();
    private GameObject SelectedPage { get; set; }

    protected override void SelectTabButton(TabButton tabButton)
    {
        base.SelectTabButton(tabButton);
        int tabButtonIndex = tabButton.transform.GetSiblingIndex();

        if (SelectedPage != null)
            SelectedPage.SetActive(false);
        
        SelectedPage = Pages[tabButtonIndex];
        SelectedPage.SetActive(true);

        ServerMenu.Instance.SetSelectedPage(SelectedPage);
    }
    protected override void Start()
    {
        int pageHolderChildCount = PageHolder.transform.childCount;
        for (int childCount = 0; childCount < pageHolderChildCount; childCount++)
            Pages.Add(PageHolder.transform.GetChild(childCount).gameObject);

        base.Start();
    }
}
