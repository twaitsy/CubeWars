// =============================================================
// WeaponComponent.cs
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

        if (muzzle == null)
            muzzle = transform;

        if (muzzleFlashFX != null)
            Instantiate(muzzleFlashFX, muzzle.position, muzzle.rotation);

        if (ProjectilePool.Instance != null && projectilePrefab != null)
        {
            Projectile proj = ProjectilePool.Instance.Spawn(projectilePrefab, muzzle.position, muzzle.rotation);
            proj.Init(target, damage, teamID);
            return;
        }

        if (ProjectilePool.Instance == null)
            Debug.LogWarning($"[WeaponComponent] ProjectilePool missing in scene; using runtime fallback projectile for {name}.", this);

        if (projectilePrefab == null)
            Debug.LogWarning($"[WeaponComponent] Projectile prefab missing on {name}; using runtime fallback projectile.", this);

        SpawnFallbackProjectile(muzzle, target, damage, teamID, 25f);
    }

    public static void SpawnFallbackProjectile(Transform origin, Attackable target, float damage, int teamID, float speed)
    {
        if (origin == null || target == null)
            return;

        var go = new GameObject("FallbackProjectile");
        go.transform.position = origin.position;
        go.transform.rotation = origin.rotation;

        var projectile = go.AddComponent<Projectile>();
        projectile.speed = Mathf.Max(1f, speed);
        projectile.maxLifetime = 4f;
        projectile.Init(target, damage, teamID);
    }
}
