using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobileLocalPlayerMovement : LocalPlayerMovement
{
    private Joystick JoystickScript { get; set; }
    [SerializeField] private float RunThreshold = 0.4f; 
    [SerializeField] private float SprintThreshold = 0.85f;
    
    [SerializeField] private float JumpThreshold = 0.4f;
    [SerializeField] private float TimeBetweenJumps = 0.65f;
    private float currentTimeBetweenJumps = 0;
    private bool canJump = true;

    protected override void Awake()
    {
        base.Awake();
        JoystickScript = MobileJoystick.Instance.GetComponent<Joystick>();
        MobileJoystick.Instance.SetActive(true);
    }
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (!canJump)
        {
            currentTimeBetweenJumps -= Time.fixedDeltaTime;
            canJump = currentTimeBetweenJumps < 0;
        }
    }
    protected override float GetMoveInputX()
    {
        float speedX = JoystickScript.Horizontal;
        if (speedX >= RunThreshold)
            return 1;
        else if (speedX <= -RunThreshold)
            return -1;
        return 0;
    }
    protected override bool GetSprintInput()
    {
        float speedX = JoystickScript.Horizontal;
        if (speedX >= SprintThreshold || speedX <= -SprintThreshold)
            return true;
        return false;
    }


    protected override bool GetJumpInputPressed()
    {
        if (JoystickScript.Vertical >= JumpThreshold && canJump)
        {
            canJump = false;
            currentTimeBetweenJumps = TimeBetweenJumps;
            return true;
        }
        return false;
    }
    protected override bool GetJumpInputReleased()
    {
        if (JoystickScript.Vertical < JumpThreshold)
        {
            canJump = true;
            return true;
        }
        return false;
    }
    
    protected override float GetVerticalInput()
    {
        return JoystickScript.Vertical;
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
        MobileJoystick.Instance.SetActive(false);
    }
}
