﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class UIItemListingManager : MonoBehaviour
{
    [SerializeField] protected GameObject ItemListingPrefab; //Set in inspector
    protected List<(int, bool)> IndexesToCompare { get; set; }

    protected abstract void SetIndexesToCompare(List<(int, bool)> indexesToCompare);
    protected abstract void SortTransformsItemListingsDictionary();

    protected Dictionary<int, IUIItemListing> MergeSortItemListings(Dictionary<int, IUIItemListing> unsortedList, List<(int, bool)> itemCompareIndexes)
    {
        if (unsortedList.Count <= 1) // terminal case
            return unsortedList;

        Dictionary<int, IUIItemListing> leftEntries = new Dictionary<int, IUIItemListing>();
        Dictionary<int, IUIItemListing> rightEntries = new Dictionary<int, IUIItemListing>();

        int middleIndex = unsortedList.Count / 2;
        int count = 0;
        foreach (KeyValuePair<int, IUIItemListing> keyValuePair in unsortedList)
        {
            if (count < middleIndex)
                leftEntries.Add(keyValuePair.Key, keyValuePair.Value);
            else
                rightEntries.Add(keyValuePair.Key, keyValuePair.Value);
            count++;
        }

        leftEntries = MergeSortItemListings(leftEntries, itemCompareIndexes);
        rightEntries = MergeSortItemListings(rightEntries, itemCompareIndexes);

        return MergeItemListings(leftEntries, rightEntries, itemCompareIndexes);

    }
    private Dictionary<int, IUIItemListing> MergeItemListings(Dictionary<int, IUIItemListing> leftEntries, Dictionary<int, IUIItemListing> rightEntries, List<(int, bool)> itemCompareIndexes)
    {
        Dictionary<int, IUIItemListing> merged = new Dictionary<int, IUIItemListing>();

        while (leftEntries.Count > 0 || rightEntries.Count > 0)
        {
            KeyValuePair<int, IUIItemListing> left;
            KeyValuePair<int, IUIItemListing> right;

            if (leftEntries.Count > 0 && rightEntries.Count > 0) //Both lists have entries to merge
            {
                left = leftEntries.First();
                right = rightEntries.First();
                bool addedToMerge = false;

                foreach ((int, bool) itemIndexToCompare in itemCompareIndexes)
                {
                    IComparable leftItem = left.Value.GetItemInList(itemIndexToCompare.Item1);
                    IComparable rightItem = right.Value.GetItemInList(itemIndexToCompare.Item1);
                    bool sortByAscending = itemIndexToCompare.Item2;

                    int comparisonResult = leftItem.CompareTo(rightItem);
                    // returns x<0 if left item smaller
                    // returns x==0 if items same
                    // returns x>0 if left item bigger

                    if (comparisonResult < 0)
                    {
                        if (sortByAscending)
                            AddToMerged(left, ref leftEntries, ref merged);
                        else
                            AddToMerged(right, ref rightEntries, ref merged);
                        addedToMerge = true;
                        break;
                    }
                    else if (comparisonResult > 0)
                    {
                        if (sortByAscending)
                            AddToMerged(right, ref rightEntries, ref merged);
                        else
                            AddToMerged(left, ref leftEntries, ref merged);
                        addedToMerge = true;
                        break;
                    }
                    else // Same value, have to sort by number the next itemCompareIndex given
                        continue;
                }                  

                if (!addedToMerge) //If all indexesToCompare were equal, then just add the left entry
                    AddToMerged(left, ref leftEntries, ref merged);
            }
            else if (leftEntries.Count > 0)
            {
                left = leftEntries.First();
                AddToMerged(left, ref leftEntries, ref merged);
            }
            else if (rightEntries.Count > 0)
            {
                right = rightEntries.First();
                AddToMerged(right, ref rightEntries, ref merged);
            }
        }
        return merged;
    }
    private void AddToMerged(KeyValuePair<int, IUIItemListing> entry, ref Dictionary<int, IUIItemListing> entries, ref Dictionary<int, IUIItemListing> merged)
    {
        merged.Add(entry.Key, entry.Value);
        entries.Remove(entry.Key);
    }
}