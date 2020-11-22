using System;
using TMPro;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private int ID;
    [SerializeField] private string Username;

    private GameObject PlayerModelObject { get; set; }
    private TextMeshProUGUI UsernameText { get; set; }

    public Vector2 Velocity { get; set; }
    private float RunSpeed { get; set; }
    private float SprintSpeed { get; set; }

    // Delegate and event used to notify when movement stats are read from server
    public delegate void PlayerMovementStats(float runSpeed);
    public event PlayerMovementStats PlayerMovementStatsChanged;

    public void Initialise(int iD, string username) 
    {
        (ID, Username) = (iD, username);
        SetUsername();
    }
    public void SetPosition(Vector3 position)
    {
        PlayerModelObject.transform.position = position;
    }
    public void SetRotation(Quaternion rotation)
    {
        PlayerModelObject.transform.rotation = rotation;
    }
    public void SetPlayerMovementStats(float runSpeed, float sprintSpeed)
    {
        RunSpeed = runSpeed;
        SprintSpeed = sprintSpeed;
        PlayerMovementStatsChanged?.Invoke(runSpeed);
    }
    public void Disconnect()
    {
        Destroy(gameObject);
    }


    private void Awake()
    {
        PlayerModelObject = transform.GetChild(0).gameObject;
        UsernameText = PlayerModelObject.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
        SetUsername("");
    }
    
    private void SetUsername()
    {
        UsernameText.text = Username;
    }
    private void SetUsername(string username)
    {
        UsernameText.text = username;
    }

    
}
