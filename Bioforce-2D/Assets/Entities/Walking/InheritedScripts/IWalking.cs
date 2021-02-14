using UnityEngine;

public interface IWalking
{
    bool GetGrounded();
    void LerpPosition();
    void LerpRotation();
}
