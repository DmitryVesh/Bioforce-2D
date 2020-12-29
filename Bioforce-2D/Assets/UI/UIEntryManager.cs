using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public abstract class UIEntryManager : MonoBehaviour
{
    [SerializeField] protected float EntryTimeToLive = 8;

    private GameObject Panel { get; set; }
    private List<GameObject> EntryPanels { get; set; }
    private List<TextMeshProUGUI> EntryTexts { get; set; }
    private Queue<(GameObject, Coroutine)> ActiveEntries { get; set; }


    protected virtual void Awake()
    {
        Panel = transform.GetChild(0).gameObject;
        Panel.SetActive(true);

        InitEntryPanels();
        InitEntryTexts();
        ActiveEntries = new Queue<(GameObject, Coroutine)>();
    }
    protected GameObject AddEntry(string text)
    {
        int inactiveEntryIndex = FindIndexOfInactiveEntry();
        GameObject entry = GetEntry(ref inactiveEntryIndex);

        entry.SetActive(true);

        EntryTexts[inactiveEntryIndex].text = text;
        Coroutine removingEntryCoroutine = StartCoroutine(RemoveEntry(entry));

        ActiveEntries.Enqueue((entry, removingEntryCoroutine));

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
        EntryTexts = new List<TextMeshProUGUI>(EntryPanels.Count);
        foreach (GameObject entry in EntryPanels)
            EntryTexts.Add(entry.GetComponentInChildren<TextMeshProUGUI>());
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
