using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class TabButton : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
{
    private TabGroup ParentTabGroup { get; set; }
    private Image ButtonBackground { get; set; }

    public void SetParentTabGroup(TabGroup tabGroup) =>
        ParentTabGroup = tabGroup;
    public void SetBackgroundColor(Color backgroundColor) =>
        ButtonBackground.color = backgroundColor;

    private void Awake()
    {
        ButtonBackground = GetComponent<Image>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ParentTabGroup.OnTabButtonSelect(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ParentTabGroup.OnTabButtonHover(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ParentTabGroup.OnTabButtonExit(this);
    }

}
