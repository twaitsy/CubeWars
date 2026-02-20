using UnityEngine;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance;

    [Header("Defaults")]
    public Projectile defaultProjectilePrefab;
    public LayerMask defaultAttackableLayers = ~0;
    public float defaultVisionRange = 16f;
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
    }

    public void ConfigureCombatant(UnitCombatController combat)
    {
        if (combat == null) return;

        combat.attackableLayers = defaultAttackableLayers;
        if (combat.visionRange <= 0f)
            combat.visionRange = defaultVisionRange;

        if (combat.weapon == null)
            combat.weapon = combat.GetComponent<WeaponComponent>();

        if (combat.weapon != null && combat.weapon.projectilePrefab == null && defaultProjectilePrefab != null)
            combat.weapon.projectilePrefab = defaultProjectilePrefab;

        if (attackableLayer >= 0)
        {
            var attackables = combat.GetComponentsInChildren<Attackable>(true);
            foreach (var atk in attackables)
                atk.gameObject.layer = attackableLayer;
        }
    }
}
