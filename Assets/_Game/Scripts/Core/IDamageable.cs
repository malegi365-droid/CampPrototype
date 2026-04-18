public interface IDamageable
{
    void TakeDamage(float amount, UnitStats sourceStats = null);
    void ReceiveHealing(float amount);
    bool IsDead();
}