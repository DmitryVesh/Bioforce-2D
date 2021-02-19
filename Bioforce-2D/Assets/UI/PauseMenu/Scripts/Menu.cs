using UnityEngine;
using UnityEngine.EventSystems;

public class Menu : MonoBehaviour
{
    [SerializeField] private string MenuTitle;
    [SerializeField] GameObject DefaultButton;

    public void ActivateMenu() =>
        Activate();
    public string Activate()
    {
        gameObject.SetActive(true);
        EventSystem.current.SetSelectedGameObject(DefaultButton);
        return MenuTitle;
    }
    public void Deactivate()
    {
        gameObject.SetActive(false);
    }
}
