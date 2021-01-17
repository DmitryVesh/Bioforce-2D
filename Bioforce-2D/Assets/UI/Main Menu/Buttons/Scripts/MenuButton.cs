using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Animator))]
public class MenuButton : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
{
    private Animator Animator { get; set; }

    public void OnPointerClick(PointerEventData eventData)
    {
        Animator.SetBool("Pressed", true);
        MainMenu.Instance.PlayMainMenuSFX(MainMenuSFXs.buttonPressed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Animator.SetBool("Selected", true);
        MainMenu.Instance.SetButtonSelected(this);
        MainMenu.Instance.PlayMainMenuSFX(MainMenuSFXs.buttonSelected);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Animator.SetBool("Selected", false);
        Animator.SetBool("Pressed", false);
        MainMenu.Instance.PlayMainMenuSFX(MainMenuSFXs.buttonDeselected);
    }

    private void Awake()
    {
        Animator = GetComponent<Animator>();
    }
}
