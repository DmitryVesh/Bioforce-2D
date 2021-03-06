using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Singleton;
using UnityEngine.UI;

public class PlayerChooseColor : MonoBehaviour
{
    public static PlayerChooseColor Instance { get => instance; }
    private static PlayerChooseColor instance;

    [SerializeField] private GameObject Panel;

    [SerializeField] private Image ExamplePlayerModelBody;
    [SerializeField] private Image ExamplePlayerModelArms;
    public Color ChosenColor { get; private set; } = Color.black;
    private int ChosenColorIndex { get; set; }

    [SerializeField] private PlayerColor[] PlayerColors;

    internal Color GetColorFromIndex(int playerColorIndex) =>
        PlayerColors[playerColorIndex].Color;
    public void ClickedPlay() //Subscribed through Inspector
    {
        Deactivate();
        GameManager.Instance.PlayerChoseColor(ChosenColor, ChosenColorIndex);
    }

    public void Activate() =>
        SetActive(true);
    public void Deactivate() =>
        SetActive(false);
    public void SetActive(bool active)
    {
        Panel.SetActive(active);
    }
    
    public void ActivatedColor(Color color)
    {
        int colorToFree = -1, colorToTake = -1;
        for (int colorCount = 0; colorCount < PlayerColors.Length; colorCount++)
        {
            PlayerColor playerColor = PlayerColors[colorCount];
            Color currentColor = playerColor.Color;
            
            if (currentColor == color)
                colorToTake = colorCount;
            else if (currentColor == ChosenColor)
            {
                colorToFree = colorCount;
                playerColor.SetIsFree(true);
            }
        }

        ChosenColor = color;
        ChosenColorIndex = colorToTake;
        ExamplePlayerModelBody.color = ChosenColor;
        ExamplePlayerModelArms.color = ChosenColor;

        if (ChosenColorIndex != -1)
        {
            PlayerColors[ChosenColorIndex].SetIsFree(false);
        }
        if (colorToFree != -1)
        {
            PlayerColors[colorToFree].SetIsFree(true);
        }
        ClientSend.ColorToFreeAndToTaken(colorToFree, colorToTake);
    }
    
    internal void SetTakenColors(List<int> takenColors)
    {
        for (int colorCount = 0; colorCount < PlayerColors.Length; colorCount++)
        {
            PlayerColor playerColor = PlayerColors[colorCount];
            if (takenColors.Contains(colorCount))
                playerColor.SetIsFree(false);
            else
                playerColor.SetIsFree(true);
        }
    }

    internal void FreeColor(int colorToFree)
    {
        PlayerColors[colorToFree].SetIsFree(true);
    }
    internal void TakeColor(int colorToTake)
    {
        PlayerColors[colorToTake].SetIsFree(false);
    }
    
    internal void SetDefaultColor()
    {
        foreach (PlayerColor color in PlayerColors)
        {
            if (color.IsFree)
            {
                color.OnPointerClick(null);
                break;
            }
        }
    }

    private void Awake()
    {
        Singleton.Init(ref instance, this);

        GameStateManager.GameEnded += Deactivate;
        GameStateManager.GameRestarting += Activate;
    }
    private void OnDestroy()
    {
        GameStateManager.GameEnded -= Deactivate;
        GameStateManager.GameRestarting -= Activate;
    }

}
