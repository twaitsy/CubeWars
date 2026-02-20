using UnityEngine;

public class Projectile : MonoBehaviour
{
    [HideInInspector] public Projectile prefabKey;

    [Header("Movement")]
    public float speed = 25f;
    public float maxLifetime = 5f;
    [Min(0.05f)] public float impactDistance = 0.25f;

    [Header("FX")]
    public GameObject impactFX;

    Attackable target;
    float damage;
    int sourceTeam;
    float lifeTimer;

    public void Init(Attackable target, float damage, int teamID)
    {
        this.target = target;
        this.damage = damage;
        sourceTeam = teamID;
        lifeTimer = 0f;
    }

    void Update()
    {
        lifeTimer += Time.deltaTime;
        if (lifeTimer >= maxLifetime)
        {
            Despawn();
            return;
        }

        if (target == null || !target.IsAlive)
        {
            Despawn();
            return;
        }

        Vector3 targetPos = target.AimPoint != null ? target.AimPoint.position : target.transform.position;
        Vector3 next = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
        Vector3 dir = (targetPos - transform.position);

        transform.position = next;
        if (dir.sqrMagnitude > 0.0001f)
            transform.forward = dir.normalized;

        if (Vector3.Distance(transform.position, targetPos) <= impactDistance)
            HitTarget();
    }

    void HitTarget()
    {
        if (target != null && target.IsAlive && target.teamID != sourceTeam)
            target.TakeDamage(damage);

        if (impactFX != null)
            Instantiate(impactFX, transform.position, Quaternion.identity);

        Despawn();
    }

    void Despawn()
    {
        if (ProjectilePool.Instance != null)
            ProjectilePool.Instance.Despawn(this);
        else
            Destroy(gameObject);
    }
}
