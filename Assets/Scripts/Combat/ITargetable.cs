public interface ITargetable
{
    int TeamID { get; }
    bool IsAlive { get; }
    void TakeDamage(float damage);
}
