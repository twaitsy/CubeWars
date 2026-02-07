// =============================================================
// Projectile.cs (Unified + Pooling Compatible)
//
// DEPENDENCIES:
// - Attackable:
//      * Must expose: teamID, IsAlive, TakeDamage(float)
// - ProjectilePool:
//      * Handles pooling.
// - WeaponComponent:
//      * Spawns this projectile.
//
// NOTES FOR FUTURE MAINTENANCE:
// - This is a homing projectile. For ballistic or physics-based,
//   replace Update() with Rigidbody logic.
// - If you add armor/resistance, wrap TakeDamage() in a damage system.
// =============================================================

using UnityEngine;

public class Projectile : MonoBehaviour
{
    [HideInInspector] public Projectile prefabKey;

    [Header("Movement")]
    public float speed = 25f;
    public float maxLifetime = 5f;

    [Header("FX")]
    public GameObject impactFX;

    private Attackable target;
    private float damage;
    private int sourceTeam;
    private float lifeTimer;

    public void Init(Attackable target, float damage, int teamID)
    {
        this.target = target;
        this.damage = damage;
        this.sourceTeam = teamID;
        lifeTimer = 0f;
    }

    void Update()
    {
        lifeTimer += Time.deltaTime;
        if (lifeTimer >= maxLifetime)
        {
            ProjectilePool.Instance.Despawn(this);
            return;
        }

        if (target == null || !target.IsAlive)
        {
            ProjectilePool.Instance.Despawn(this);
            return;
        }

        Vector3 dir = target.transform.position - transform.position;
        float dist = dir.magnitude;

        if (dist < 0.2f)
        {
            HitTarget();
            return;
        }

        transform.position += dir.normalized * speed * Time.deltaTime;
        transform.forward = dir.normalized;
    }

    void HitTarget()
    {
        if (target != null && target.IsAlive && target.teamID != sourceTeam)
            target.TakeDamage(damage);

        if (impactFX != null)
            Instantiate(impactFX, transform.position, Quaternion.identity);

        ProjectilePool.Instance.Despawn(this);
    }
}