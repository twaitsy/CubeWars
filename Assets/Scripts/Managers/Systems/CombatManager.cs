using UnityEngine;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance;

    [Header("Defaults")]
    public Projectile defaultProjectilePrefab;
    public LayerMask defaultAttackableLayers = ~0;

    [Tooltip("Layer assigned to Attackable objects. Set to -1 to keep current layers.")]
    public int attackableLayer = -1;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Debug.Log("CombatManager initialized");
    }

    public void ConfigureCombatant(UnitCombatController combat)
    {
        if (combat == null) return;

        if (combat.weapon == null)
            combat.weapon = combat.GetComponent<WeaponComponent>();

        combat.attackableLayers = defaultAttackableLayers;

        if (combat.weapon != null)
        {
            if (combat.weapon.projectilePrefab == null && defaultProjectilePrefab != null)
                combat.weapon.projectilePrefab = defaultProjectilePrefab;

            if (combat.weapon.muzzle == null)
                combat.weapon.muzzle = combat.weapon.transform;
        }

        if (attackableLayer >= 0 && attackableLayer < 32)
        {
            var attackables = combat.GetComponentsInChildren<Attackable>(true);
            for (int i = 0; i < attackables.Length; i++)
                attackables[i].gameObject.layer = attackableLayer;
        }
    }
}
