using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum ServersSortBy
{
    name,
    players,
    mapName,
    ping
}
public class TabGroupServerSort : TabGroup
{   
    ServersSortBy SelectedSort { get; set; } 
    bool Ascending { get; set; }
    protected override void SelectTabButton(TabButton tabButton)
    {
        base.SelectTabButton(tabButton);
        int tabButtonIndex = tabButton.transform.GetSiblingIndex();
        ServersSortBy sort = (ServersSortBy)tabButtonIndex;

        if (sort != SelectedSort) //First time selected must be ascending
            Ascending = false;
        else
            Ascending = !Ascending;
        
        SelectedSort = sort; 
        ServerMenu.Instance.SelectedServersPage.SortServerEntriesBy(SelectedSort, Ascending);
    }

    protected override void Start()
    {
        SelectedSort = ServersSortBy.mapName;
        Ascending = false;

        base.Start(); //Must start later as the default is selected
    }
}
