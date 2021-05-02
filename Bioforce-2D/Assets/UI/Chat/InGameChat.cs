using System;
using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Output;
using UnityEngine.Singleton;

public class InGameChat : UIEntryManager
{
    public static InGameChat Instance { get => instance; }
    private static InGameChat instance;

    [SerializeField] private byte NumMaxCharsInLine = 32;
    private bool ClickedOdd { get; set; } = false;
    private bool ChatLogIsActive { get; set; } = false;

    [SerializeField] private GameObject ChatLogAndWrite;
    [SerializeField] private TMP_InputField ChatEntryInputField;
    private bool ShouldSendChatEntry { get; set; }

    [SerializeField] private TextMeshProUGUI EntireChatLog;
    [SerializeField] private ScrollRect ScrollRect;

    public void AddInGameChatEntry(string text, Color messageColor)
    {
        bool firstSpaceIndexAfterColon = false;
        bool reachedColon = false;
        int secondSpaceIndexAfterEndBracket = -1;

        for (int i = 0; i < text.Length; i++)
        {
            char character = text[i];
            bool isSpace = character == ' ';
            if (isSpace && firstSpaceIndexAfterColon)
            {
                secondSpaceIndexAfterEndBracket = i;
                break;
            }
            else if (isSpace && reachedColon)
                firstSpaceIndexAfterColon = true;
            else if (character == ':')
                reachedColon = true;
        }

        //Make so can add up to 100 chars in a text, but when displaying as a normal/inherited Entry use ...
        // But then you can view the whole message when pressing the chat button.

        AddTextToChatLog(text);

        if ((secondSpaceIndexAfterEndBracket == -1 || secondSpaceIndexAfterEndBracket > NumMaxCharsInLine) && text.Length > NumMaxCharsInLine)
            text = text.Insert(NumMaxCharsInLine, " -");

        //AddEntry(text, null, null);
        AddEntry(text, messageColor);
    }
    private void AddTextToChatLog(string text)
    {
        StartCoroutine(ScrollToBottom());

        if (EntireChatLog.text == "")
        {
            EntireChatLog.text = text;
            return;
        }

        EntireChatLog.text = string.Concat(EntireChatLog.text, "\n", text);
    }

    private IEnumerator ScrollToBottom()
    {
        yield return new WaitForEndOfFrame();
        ScrollRect.verticalNormalizedPosition = 0f;
    }

    public void ClickedChatButton() //Subscribed to by chat button in inspector
    {
        ClickedOdd = !ClickedOdd;
        if (ClickedOdd)
            ActivateChatLogMenu(!ChatLogIsActive);
    }
    private void ActivateChatLogMenu(bool active)
    {
        ChatLogIsActive = active;

        ChatLogAndWrite.SetActive(ChatLogIsActive);
        if (active)
            StartCoroutine(ScrollToBottom());

        EntryPanel.SetActive(!ChatLogIsActive); // Hides/Shows the temporary chat entries


        if (!ChatLogIsActive && ChatEntryInputField.text != "")
        {
            OnEndEdit(ChatEntryInputField.text);
        }
    }

    public void EnteredText(string text) //Subscribed to by input text field
    {
        TextInputValidator.SanitiseText(ref text);
        ChatEntryInputField.text = text;
        ShouldSendChatEntry = text.Length > 0;
    }
    public void OnEndEdit(string text) //Subscribed to by input text field
    {
        EnteredText(text);

        if (ShouldSendChatEntry)
        {
            //TODO: make a cooldown, so can't spam sending chat messages like in Call Of Duty Mobile
            ChatEntryInputField.text = TextInputValidator.TrimAllSpaces(ChatEntryInputField.text);

            ClientSend.ChatMessage(ChatEntryInputField.text);
            ChatEntryInputField.text = "";

            //TODO: Maybe make a setting so player can either have the chat log open or closed after entering message
            //Closed
            ActivateChatLogMenu(false); //Hides the chat log after sending message
            ClickedOdd = true;

            //Open
            //ClickedOdd = false;
        }
    }
    
    protected override void Awake()
    {
        Singleton.Init(ref instance, this);
        base.Awake();

        //Clear the log of existing text and hide it
        EntireChatLog.text = ""; 
        ChatLogAndWrite.SetActive(false);
    }
    private void Start()
    {
        GameManager.Instance.OnPlayerConnected += PlayerConnectedMessage;
        GameManager.Instance.OnPlayerDisconnected += PlayerDisconnectedMessage;
    }

    private void PlayerConnectedMessage(byte iD, string username, bool justJoined)
    {
        string message;
        if (justJoined)
            message = "joined";
        else
            message = "is in game";
        AddInGameChatEntry($"Player \"{username}\" {message}", Color.white);
    }
    private void PlayerDisconnectedMessage(byte iD, string username) =>
        AddInGameChatEntry($"Player \"{username}\" left", Color.white);

    private void OnDestroy()
    {
        GameManager.Instance.OnPlayerConnected -= PlayerConnectedMessage;
        GameManager.Instance.OnPlayerDisconnected -= PlayerDisconnectedMessage;
    }

}
