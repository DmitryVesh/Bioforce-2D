using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private int ID;
    [SerializeField] private string Username;
    private Rigidbody2D RB;
    
    public void Initialise(int iD, string username) => (ID, Username) = (iD, username);
    public void SetPosition(Vector3 position)
    {
        transform.position = position;
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
        RB = GetComponent<Rigidbody2D>();
    }
    
}
