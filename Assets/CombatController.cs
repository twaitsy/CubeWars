using UnityEngine;

public class CombatController : MonoBehaviour
{
    [SerializeField] private int attackDamage;
    [SerializeField] private float attackRange;
    [SerializeField] private float attackCooldown;
    [SerializeField] private int armor;

    public void SetCombatStats(int damage, float range, float cooldown, int armorValue)
    {
        attackDamage = damage;
        attackRange = range;
        attackCooldown = cooldown;
        armor = armorValue;
    }
}