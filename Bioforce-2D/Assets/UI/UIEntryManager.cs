﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class UIEntryManager : MonoBehaviour
{
    [SerializeField] protected float EntryTimeToLive = 8;

    private GameObject Panel { get; set; }
    private List<GameObject> EntryPanels { get; set; }
    private List<(TextMeshProUGUI, TextMeshProUGUI)> EntryTexts { get; set; }
    private List<Image> EntryImage { get; set; }
    private Queue<(GameObject, Coroutine)> ActiveEntries { get; set; }


    protected virtual void Awake()
    {
        Panel = transform.GetChild(0).gameObject;
        Panel.SetActive(true);

        InitEntryPanels();
        InitEntryTexts();
        ActiveEntries = new Queue<(GameObject, Coroutine)>();
    }
    protected (GameObject, int) AddEntry(string text1, string text2, Sprite image)
    {
        int inactiveEntryIndex = FindIndexOfInactiveEntry();
        GameObject entry = GetEntry(ref inactiveEntryIndex);

        entry.SetActive(true);

        EntryTexts[inactiveEntryIndex].Item1.text = text1;
        EntryTexts[inactiveEntryIndex].Item2.text = text2;
        EntryImage[inactiveEntryIndex].sprite = image;
        Coroutine removingEntryCoroutine = StartCoroutine(RemoveEntry(entry));

        ActiveEntries.Enqueue((entry, removingEntryCoroutine));

        return (entry, inactiveEntryIndex);
    }
    protected GameObject AddEntry(string killerName, string diedName, Sprite deathSprite, Color imageColor, Color killerColor, Color diedColor)
    {
        (GameObject entry, int entryIndex) = AddEntry(killerName, diedName, deathSprite);
        EntryTexts[entryIndex].Item1.color = killerColor;
        EntryTexts[entryIndex].Item2.color = diedColor;
        EntryImage[entryIndex].color = imageColor;

        return entry;
    }

    private IEnumerator RemoveEntry(GameObject entry)
    {
        yield return new WaitForSeconds(EntryTimeToLive);
        //TODO: make disappering decreasing in alpha, then deactivating
        entry.SetActive(false);
        ActiveEntries.Dequeue();
        SetSiblingIndexLast(entry);
    }

    private void SetSiblingIndexLast(GameObject entry)
    {
        entry.transform.SetAsLastSibling();
    }

    private int FindIndexOfInactiveEntry()
    {
        for (int count = 0; count < EntryPanels.Count; count++)
        {
            GameObject entry = EntryPanels[count];
            if (!entry.activeInHierarchy)
                return count;
        }
        return -1;
    }
    private GameObject GetEntry(ref int inactiveEntryIndex)
    {
        GameObject entry;
        if (inactiveEntryIndex == -1)
        {
            entry = GetLongestActiveEntry();
            inactiveEntryIndex = entry.transform.GetSiblingIndex();
        }
        else
            entry = EntryPanels[inactiveEntryIndex];
        SetSiblingIndexLast(entry);
        return entry;
    }

    private GameObject GetLongestActiveEntry()
    {
        (GameObject entry, Coroutine removeCoroutine) = ActiveEntries.Dequeue();
        StopCoroutine(removeCoroutine);
        entry.SetActive(false);
        return entry;
    }
    private void InitEntryTexts()
    {
        EntryTexts = new List<(TextMeshProUGUI, TextMeshProUGUI)>(EntryPanels.Count);
        EntryImage = new List<Image>(EntryPanels.Count);
        foreach (GameObject entry in EntryPanels)
        {
            EntryTexts.Add((entry.transform.GetChild(1).GetComponent<TextMeshProUGUI>(),
                entry.transform.GetChild(2).GetComponent<TextMeshProUGUI>()));
            EntryImage.Add(entry.transform.GetChild(3).GetComponent<Image>());
        }

    }
    private void InitEntryPanels()
    {
        Transform panelTransform = Panel.transform;
        int numPanels = panelTransform.childCount;
        EntryPanels = new List<GameObject>(numPanels);
        for (int count = 0; count < numPanels; count++)
        {
            EntryPanels.Add(panelTransform.GetChild(count).gameObject);
            EntryPanels[count].SetActive(false);
        }
    }
}
