using UnityEngine;

public class HealthComponent : MonoBehaviour, IHasHealth
{
    [SerializeField] private int teamID;
    [SerializeField] private float maxHealth;
    [SerializeField] private float currentHealth;

    public int TeamID => teamID;
    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public bool IsAlive => currentHealth > 0f;

    public void SetTeam(int team)
    {
        teamID = team;
    }

    public void Initialize()
    {
        currentHealth = maxHealth;
    }

    public void SetMaxHealth(float value)
    {
        maxHealth = value;
        currentHealth = value;
    }

    public void TakeDamage(float amount)
    {
        currentHealth = Mathf.Max(0f, currentHealth - amount);
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
    }
}
