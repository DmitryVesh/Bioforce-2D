using TMPro;
using UnityEngine;
using UnityEngine.Singleton;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get => instance; }
    private static PauseManager instance;

    public bool Paused { get; set; }


    [SerializeField] private TextMeshProUGUI TitleText;

    [SerializeField] private Menu MenuPause;
    [SerializeField] private Menu MenuSettings;
    [SerializeField] private Menu MenuAudioSettings;

    [SerializeField] private GameObject Panel;
    private Menu CurrentMenu { get; set; }
    private Menu LastMenu { get; set; }

    private bool ClickedOdd { get; set; }
    [SerializeField] private GameObject MobileSettingsButton;

    [SerializeField] private GameObject LostConnectionPanel;
    [SerializeField] private GameObject NormalPausePanel;

    internal void SetNormalPauseActive(bool active)
    {
        Paused = active;
        Panel.SetActive(active);

        NormalPausePanel.SetActive(active);
        LostConnectionPanel.SetActive(false);

        if (GameManager.Instance.IsMobileSupported)
            MobileSettingsButton.SetActive(!active);

        if (active)
        {
            DeactivateAllMenus();
            ActivateMenu(Instance.MenuPause);
        }        
    }
    internal void SetLostonnectionPause(bool lostConnection)
    {
        Panel.SetActive(Paused || lostConnection);
        NormalPausePanel.SetActive(Paused && !lostConnection);
        LostConnectionPanel.SetActive(lostConnection);
    }
    public void OnMobileSettings()
    {
        ClickedOdd = !ClickedOdd;
        if (ClickedOdd)
            GameManager.Instance.InvokePauseEvent(!Paused);
    }

    public void OnBackToGameButton() =>
        GameManager.Instance.InvokePauseEvent(false);
    public void OnSettingsButton() =>
        ActivateMenu(MenuSettings);
    public void OnQuitButton() =>
        Client.Instance.Disconnect();

    public void OnMainMenuButton() =>
        ActivateMenu(MenuPause);
    public void OnAudioSettingsButton() =>
        ActivateMenu(MenuAudioSettings);
    public void OnBackToLastMenu() =>
        ActivateMenu(LastMenu);


    private void DeactivateAllMenus()
    {
        MenuPause.Deactivate();
        MenuSettings.Deactivate();
        MenuAudioSettings.Deactivate();
    }

    private void ActivateMenu(Menu menu)
    {
        LastMenu = CurrentMenu;
        if (CurrentMenu != null)
            CurrentMenu.Deactivate();
        TitleText.text = menu.Activate();
        CurrentMenu = menu;
    }

    

    private void Awake()
    {
        Singleton.Init(ref instance, this);

        SetNormalPauseActive(false);
        MobileSettingsButton.SetActive(GameManager.Instance.IsMobileSupported);

    }
    private void Start()
    {
        GameManager.Instance.OnPauseEvent += SetNormalPauseActive;
        GameManager.Instance.OnLostConnectionEvent += SetLostonnectionPause;
    }
    private void OnDestroy()
    {
        GameManager.Instance.OnPauseEvent -= SetNormalPauseActive;
        GameManager.Instance.OnLostConnectionEvent -= SetLostonnectionPause;
    }

}
