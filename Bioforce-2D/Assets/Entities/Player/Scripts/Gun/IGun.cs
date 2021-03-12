using UnityEngine;

public interface IGun
{
    void ShootBullet(Vector2 position, Quaternion rotation);
    void SetOwnerClientID(int iD);
    void Disable(TypeOfDeath typeOfDeath);
    void Enable();
    void SetColor(Color playerColor);
    void SetOwnerCollider(Collider2D ownCollider);
}
