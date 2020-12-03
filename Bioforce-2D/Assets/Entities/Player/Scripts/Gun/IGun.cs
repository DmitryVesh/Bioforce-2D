using UnityEngine;

public interface IGun
{
    void ShootBullet(Vector2 position, Quaternion rotation);
    void SetOwnerClientID(int iD);
    void Disable();
    void Enable();
}
