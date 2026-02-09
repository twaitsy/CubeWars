// =============================================================
// WeaponComponent.cs
//
// PURPOSE:
// - Shared firing logic for Units, Turrets, and any future ranged entity.
// - Handles projectile pooling, muzzle FX, fire cooldowns.
//
// DEPENDENCIES:
// - ProjectilePool:
//      * Spawns/despawns projectiles.
// - Projectile:
//      * Unified projectile logic.
// - Attackable:
//      * Target interface for damage application.
// - UnitCombatController:
//      * Calls FireAtTarget() when ready to shoot.
//
// NOTES FOR FUTURE MAINTENANCE:
// - If you add different weapon types (burst, beam, AoE), extend this class.
// - If you add ammo or overheating, add checks before firing.
// - If you add sound FX, trigger them here.
// =============================================================

using UnityEngine;

public class WeaponComponent : MonoBehaviour
{
    [Header("Weapon Stats")]
    public float damage = 10f;
    public float fireCooldown = 0.75f;
    public float range = 8f;

    [Header("Projectile")]
    public Projectile projectilePrefab;
    public Transform muzzle;

    [Header("FX")]
    public GameObject muzzleFlashFX;

    private float fireTimer;

    public bool CanFire => fireTimer <= 0f;

    void Update()
    {
        fireTimer -= Time.deltaTime;
    }

    public void FireAtTarget(Attackable target, int teamID)
    {
        if (!CanFire || target == null || !target.IsAlive)
            return;

        fireTimer = fireCooldown;

        if (muzzleFlashFX != null)
            Instantiate(muzzleFlashFX, muzzle.position, muzzle.rotation);

        Projectile proj = ProjectilePool.Instance.Spawn(projectilePrefab, muzzle.position, muzzle.rotation);
        proj.Init(target, damage, teamID);
    }
}