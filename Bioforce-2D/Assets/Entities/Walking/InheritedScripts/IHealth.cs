
public interface IHealth
{
    byte GetOwnerClientID();
    void TakeDamage(int damage, byte bulletOwnerID);
    void Die(byte bulletOwnerID);
    void Respawn();
    void FallDie();

    float GetMaxHealth();
    float GetCurrentHealth();

    bool CanTakeDamage();
}
