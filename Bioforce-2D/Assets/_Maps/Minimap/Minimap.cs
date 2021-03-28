using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public static Minimap Instance { get; private set; }
    private List<MinimapIcon> Icons { get; set; } = new List<MinimapIcon>();
    [SerializeField] private RectTransform MinimapRectTF;
    private Rect MinimapRect { get; set; }
    private Vector2 MinimapCorner { get; set; }
    private Vector2 MinimapScale { get; set; }
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
        minimapIcon.Icon.color = color;
    }
    internal static void Unsubscribe(MinimapIcon minimapIcon) =>
        Instance.Icons.Remove(minimapIcon);

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Debug.Log($"GameManager instance already exists, destroying {gameObject.name}");
            Destroy(gameObject);
        }

        MinimapRect = MinimapRectTF.rect;

        MinimapScale = MinimapRectTF.localScale;

        Vector3[] minimapCorners = new Vector3[4];
        MinimapRectTF.GetWorldCorners(minimapCorners);

        MinimapCorner = minimapCorners[(int)CornderIndex];

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
