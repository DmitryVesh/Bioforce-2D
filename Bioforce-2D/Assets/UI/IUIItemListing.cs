using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUIItemListing
{
    void SetArrayList(ArrayList itemList);
    IComparable GetItemInList(int itemListIndex);
    GameObject GetGameObject();
    void AddToItemIndex(int itemListIndex, int toAdd);
}
