using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayerChooseColor : MonoBehaviour
{
    public static PlayerChooseColor Instance { get; private set; }

    [SerializeField] private GameObject Panel;

    [SerializeField] private Image ExamplePlayerModelBody;
    [SerializeField] private Image ExamplePlayerModelArms;
    public Color ChosenColor { get; private set; } = Color.black;
    private int ChosenColorIndex { get; set; }

    [SerializeField] private PlayerColor[] PlayerColors;
    const int NumColors = 16;

    internal Color GetColorFromIndex(int playerColorIndex) =>
        PlayerColors[playerColorIndex].Color;
    public void ClickedPlay()
    {
        SetActivate(false);
        GameManager.Instance.PlayerChoseColor(ChosenColor, ChosenColorIndex);
    }
    public void SetActivate(bool active)
    {
        Panel.SetActive(active);
    }
    public void ActivatedColor(Color color)
    {
        int colorToFree = -1, colorToTake = -1;
        for (int colorCount = 0; colorCount < NumColors; colorCount++)
        {
            PlayerColor playerColor = PlayerColors[colorCount];
            Color currentColor = playerColor.Color;

            if (currentColor == ChosenColor)
            {
                colorToFree = colorCount;
                playerColor.SetIsFree(true);
            }
            else if (currentColor == color)
                colorToTake = colorCount;
        }

        ChosenColor = color;
        ChosenColorIndex = colorToTake;
        ExamplePlayerModelBody.color = ChosenColor;
        ExamplePlayerModelArms.color = ChosenColor;
        //Send message to server that this color is to be taken!
        ClientSend.ColorToFreeAndToTaken(colorToFree, colorToTake);
    }
    
    internal void SetTakenColors(List<int> playerColors)
    {
        foreach (PlayerColor playerColor in PlayerColors)
            playerColor.SetIsFree(true);

        foreach (int color in playerColors)
            PlayerColors[color].SetIsFree(false);
    }
   
    internal void FreeColor(int colorToFree) =>
        PlayerColors[colorToFree].SetIsFree(true);
    internal void TakeColor(int colorToTake) =>
        PlayerColors[colorToTake].SetIsFree(false);
    
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
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Debug.Log($"PlayerChooseColor instance already exists, destroying {gameObject.name}");
            Destroy(gameObject);
        }
    }

    
}
