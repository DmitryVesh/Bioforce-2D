
public interface IHealth
{
    int GetOwnerClientID();
    void TakeDamage(int damage, int bulletOwnerID);
    void Die(int bulletOwnerID);
    void Respawn();
    void FallDie();

    float GetMaxHealth();
    float GetCurrentHealth();
}
