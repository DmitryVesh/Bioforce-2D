using TMPro;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private int ID;
    [SerializeField] private string Username;
    private Rigidbody2D RB { get; set; }
    private GameObject PlayerModelObject { get; set; }
    private TextMeshProUGUI UsernameText { get; set; }

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
    public void AddVelocity(Vector3 velocity)
    {
        RB.AddForce(velocity);
    }
    public void Disconnect()
    {
        Destroy(gameObject);
    }

    private void Awake()
    {
        PlayerModelObject = transform.GetChild(0).gameObject;
        RB = PlayerModelObject.GetComponent<Rigidbody2D>();
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
