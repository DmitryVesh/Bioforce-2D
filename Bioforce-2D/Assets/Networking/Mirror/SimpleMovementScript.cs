using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class SimpleMovementScript : NetworkBehaviour
{
    float MovementSpeed { get => movementSpeed; }
    [SerializeField] float movementSpeed = 1f;

    Rigidbody2D RigidBody { get => rigidbody; }
    [SerializeField] Rigidbody2D rigidbody;

    void HandleHorizontal()
    {
        if (!isLocalPlayer) 
            return;

        float xMove = Input.GetAxis("Horizontal");        

        if (xMove == 0)
            return;

        Vector3 move = new Vector3(xMove, 0, 0) * MovementSpeed * Time.fixedDeltaTime;
        transform.position += move;       
    }
    private void HandleVertical()
    {
        if (!isLocalPlayer)
            return;

        bool jumped = Input.GetButtonDown("Jump");
        if (!jumped)
            return;

        float JumpForce = 5f;
        RigidBody.velocity = new Vector2(RigidBody.velocity.x, JumpForce);        
    }

    private void FixedUpdate()
    {
        HandleHorizontal();
        HandleVertical();
    }

}
