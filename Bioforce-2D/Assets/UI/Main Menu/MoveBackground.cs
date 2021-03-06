using UnityEngine;
using System.Collections;

public class MoveBackground : MonoBehaviour
{
    
    [SerializeField] private GameObject BackgroundToMove = null;
    [SerializeField] private float MoveSpeed = 1;
    [SerializeField] private float CurrentMoveSpeed;

    [SerializeField] private Camera Camera = null;

    [SerializeField] private float RecoverySpeed = 2;
    private const float WaitTime = 0.01f;

    private static float WaitCheckTimer = 5f;
    private float CurrentTimer;
    private bool Running;

    private void Awake()
    {
        CurrentMoveSpeed = MoveSpeed;
        CurrentTimer = WaitCheckTimer;
        Running = false;
    }

    private void FixedUpdate()
    {
        SetDirection();

        Transform toMove = BackgroundToMove.transform;
        toMove.position = new Vector3(toMove.position.x + (-CurrentMoveSpeed * Time.fixedDeltaTime), toMove.position.y, toMove.position.z);
        
    }

    private void SetDirection()
    {
        if (Running)
        {
            CurrentTimer -= Time.fixedDeltaTime;
            if (CurrentTimer < 0)
            {
                Running = false;
                CurrentTimer = WaitCheckTimer;
            }
            return;
        }

        RaycastHit2D hit = Physics2D.Raycast(Camera.transform.position, Vector3.forward, 30);
        if (!hit) 
        {
            bool goingRight;
            if (MoveSpeed < 0) 
                goingRight = false;
            else 
                goingRight = true;
            MoveSpeed *= -1;

            StartCoroutine(DampenSpeed(goingRight));
            Running = true;
        }       
    }

    private IEnumerator DampenSpeed(bool goingRight)
    {
        float addSpeed;
        if (goingRight)
            addSpeed = -RecoverySpeed; //Should go left
        else
            addSpeed = RecoverySpeed; //Should go right
        addSpeed *= WaitTime;

        goingRight = !goingRight;
        while ((CurrentMoveSpeed < MoveSpeed && goingRight) || (CurrentMoveSpeed > MoveSpeed && !goingRight))
        {
            CurrentMoveSpeed += addSpeed;
            yield return new WaitForSeconds(WaitTime);
        }
    }

}
