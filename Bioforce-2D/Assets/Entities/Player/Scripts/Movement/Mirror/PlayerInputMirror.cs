using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class PlayerInputMirror : NetworkBehaviour
{
    //public float MoveInputX { get; private set; }
    //public bool Sprinting { get; private set; }
    //public bool JumpPressed { get; private set; }
    //public bool JumpReleased { get; private set; }
    //public float MoveInputY { get; private set; }

    [SerializeField] public float MoveInputX;
    [SerializeField] public bool Sprinting;
    [SerializeField] public bool JumpPressed;
    [SerializeField] public bool JumpReleased;
    [SerializeField] public float MoveInputY;

    

    [ClientCallback]
    private void Update()
    {  
        if (!isLocalPlayer) 
            return;

        float moveInputX = GetMoveInputX();
        bool sprinting = GetSprintInput();
        bool jumpPressed = GetJumpInputPressed();
        bool jumpReleased = GetJumpInputReleased();
        float moveInputY = GetVerticalInput();

        CmdSyncInputs(moveInputX, sprinting, jumpPressed, jumpReleased, moveInputY);
    }

    [Command]
    private void CmdSyncInputs(float moveInputX, bool sprinting, bool jumpPressed, bool jumpReleased, float moveInputY)
    {
        moveInputX = NormaliseInput(moveInputX);
        moveInputY = NormaliseInput(moveInputY);

        MoveInputX = moveInputX;
        Sprinting = sprinting;
        JumpPressed = jumpPressed;
        JumpReleased = jumpReleased;
        MoveInputY = moveInputY;

        float NormaliseInput(float floatVal)
        {
            floatVal = floatVal > 1 ? 1 : floatVal;
            floatVal = floatVal < -1 ? -1 : floatVal;

            return floatVal;
        }
    }

    

    protected virtual float GetMoveInputX() =>
        Input.GetAxisRaw("Horizontal"); // Get user input in x direction
    protected virtual bool GetSprintInput() =>
        Input.GetButton("Sprint");

    protected virtual bool GetJumpInputPressed() =>
        Input.GetButtonDown("Jump");
    protected virtual bool GetJumpInputReleased() =>
        Input.GetButtonUp("Jump");
    protected virtual float GetVerticalInput() =>
        Input.GetAxis("Vertical");

    
}
