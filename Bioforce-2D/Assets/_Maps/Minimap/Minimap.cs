using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Output;
using UnityEngine.Rendering;
using UnityEngine.Singleton;

public enum CornderIndex
{
    bottomLeft,
    topLeft,
    topRight,
    bottomRight
}
public class Minimap : MonoBehaviour
{
    [SerializeField] private Camera MinimapCamera;

    public static Minimap Instance { get => instance; }
    private static Minimap instance;

    private List<MinimapIcon> Icons { get; set; } = new List<MinimapIcon>();
    [SerializeField] private RectTransform MinimapRectTF;
    private Rect MinimapRect { get; set; }
    //private Vector2 MinimapCorner { get; set; }
    //private Vector2 MinimapScale { get; set; }
    private int MultiplierX { get; set; }
    private int MultiplierY { get; set; }

    [SerializeField] private Transform MinimapMaskTF;
    [SerializeField] private CornderIndex CornderIndex;

    [SerializeField] private float MaxX = 0.9375f;
    [SerializeField] private float MaxY = 0.9375f;

    internal static void SubscribeIcon(MinimapIcon minimapIcon, Color color)
    {
        Instance.Icons.Add(minimapIcon);
        minimapIcon.Icon = Instantiate(minimapIcon.Icon, Instance.MinimapMaskTF);
        int numIcons = Instance.transform.childCount;
        for (int iconCount = 2; iconCount < numIcons; iconCount++)
        {
            int otherIconSortOrder = Instance.transform.GetChild(iconCount).GetComponent<SortingGroup>().sortingOrder;
            if (otherIconSortOrder <= minimapIcon.SortOrder)
                continue;

            minimapIcon.Icon.transform.SetSiblingIndex(iconCount);
            break;
        }
        minimapIcon.Icon.color = color;
    }
    internal static void Unsubscribe(MinimapIcon minimapIcon)
    {
        Instance.Icons.Remove(minimapIcon);
    }

    private void Awake()
    {
        Singleton.Init(ref instance, this);

        MinimapRect = MinimapRectTF.rect;        

        Vector3[] minimapCorners = new Vector3[4];
        MinimapRectTF.GetWorldCorners(minimapCorners);

        //MinimapCorner = minimapCorners[(int)CornderIndex];
        //MinimapScale = MinimapRectTF.localScale;

        MakeXAndYMultipliers();
    }

    void Update()
    {
        DrawIcons();
    }

    private void DrawIcons()
    {
        foreach (MinimapIcon icon in Icons)
        {
            Vector3 screenPos = MinimapCamera.WorldToViewportPoint(icon.transform.position);

            screenPos.x = CorrectTheOrientation(screenPos.x, MultiplierX, MinimapRect.width, icon.ShouldClamp, MaxX);
            screenPos.y = CorrectTheOrientation(screenPos.y, MultiplierY, MinimapRect.height, icon.ShouldClamp, MaxY);
            screenPos.z = 0f;

            icon.Icon.transform.localPosition = screenPos;
        }
    }

    
    private float CorrectTheOrientation(float position, int multiplier, float miniMapRectDim, bool clamp, float max)
    {
        if (multiplier == -1)
            position = 1 - position;

        if (clamp)
            position = Mathf.Clamp(position, 1 - max, max);

        position = multiplier * (position * miniMapRectDim);
        return position;
    }

    private void MakeXAndYMultipliers()
    {
        //CornerIndex determines if the val for x/y should be negative or not
        // 0 -> x,y
        // 1 -> x, -y
        // 2 -> -x, -y
        // 3 -> -x ,y

        //Setting default bottom left
        MultiplierX = 1;
        MultiplierY = 1;

        switch (CornderIndex)
        {
            case CornderIndex.topRight:
                MultiplierX = -1;
                MultiplierY = -1;
                break;
            case CornderIndex.bottomRight:
                MultiplierX = -1;
                break;
            case CornderIndex.topLeft:
                MultiplierY = -1;
                break;
        }
    }
}
