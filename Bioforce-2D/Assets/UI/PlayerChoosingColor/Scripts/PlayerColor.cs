using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerColor : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image ColorChoiceImage;
    [SerializeField] private GameObject TakenCrossObject;
    public bool IsFree { get; private set; }

    public Color Color { get => ColorChoiceImage.color; }

    public void SetIsFree(bool free)
    {
        IsFree = free;
        ColorChoiceImage.raycastTarget = IsFree;
        TakenCrossObject.SetActive(!IsFree);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        PlayerChooseColor.Instance.ActivatedColor(ColorChoiceImage.color);
        SetIsFree(false);
    }

    private void Start()
    {
        SetIsFree(true);
    }
}
