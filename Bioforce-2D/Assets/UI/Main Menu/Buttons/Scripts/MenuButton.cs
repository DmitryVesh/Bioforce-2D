using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Animator))]
public class MenuButton : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
{
    public bool Interactable = true;
    private bool LastInteractibility { get; set; }

    [SerializeField] private UnityEvent OnClickEvent;
    [SerializeField] private bool AppearInMobile = true;

    private Animator Animator { get; set; }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (!Interactable)
            return;

        Animator.SetBool("Pressed", true);
        OnClickEvent?.Invoke();

        SoundMusicManager.PlayMainMenuSFX(MainMenuSFXs.buttonPressed);
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        if (!Interactable)
            return;

        Animator.SetBool("Selected", true);
        SoundMusicManager.PlayMainMenuSFX(MainMenuSFXs.buttonSelected);
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        Animator.SetBool("Selected", false);
        Animator.SetBool("Pressed", false);
        SoundMusicManager.PlayMainMenuSFX(MainMenuSFXs.buttonDeselected);
    }

    private void Awake()
    {
        Animator = GetComponent<Animator>();
        LastInteractibility = Interactable;
    }
    private void Start()
    {
        if (!AppearInMobile && GameManager.Instance.IsMobileSupported)
            gameObject.SetActive(false);
    }
    private void Update()
    {
        if (Interactable != LastInteractibility)
        {
            LastInteractibility = Interactable;
            if (!Interactable)
                OnPointerExit(null);
        }
    }
}
