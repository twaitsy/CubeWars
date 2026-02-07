// =============================================================
// TurretProjectilePool.cs
//
// DEPENDENCIES:
// - TurretProjectile:
//      * Projectiles spawned and despawned by this pool.
// - Turret / any weapon system using TurretProjectile:
//      * Should call Spawn() and Despawn() instead of Instantiate/Destroy.
//
// NOTES FOR FUTURE MAINTENANCE:
// - This is a generic pool keyed by prefab.
// - If you add different turret projectile types, they can all share this pool.
// - Make sure there is exactly one TurretProjectilePool in the scene.
// =============================================================

using System.Collections.Generic;
using UnityEngine;

public class TurretProjectilePool : MonoBehaviour
{
    public static TurretProjectilePool Instance;

    private readonly Dictionary<TurretProjectile, Stack<TurretProjectile>> pools =
        new Dictionary<TurretProjectile, Stack<TurretProjectile>>();

    void Awake()
    {
        Instance = this;
    }

    public TurretProjectile Spawn(TurretProjectile prefab, Vector3 pos, Quaternion rot)
    {
        if (!pools.TryGetValue(prefab, out var stack))
        {
            stack = new Stack<TurretProjectile>();
            pools[prefab] = stack;
        }

        TurretProjectile obj = (stack.Count > 0) ? stack.Pop() : Instantiate(prefab);
        obj.transform.SetPositionAndRotation(pos, rot);
        obj.gameObject.SetActive(true);

        obj.prefabKey = prefab;

        return obj;
    }

    public void Despawn(TurretProjectile obj)
    {
        if (obj == null) return;

        obj.gameObject.SetActive(false);

        if (obj.prefabKey != null)
        {
            if (!pools.TryGetValue(obj.prefabKey, out var stack))
            {
                stack = new Stack<TurretProjectile>();
                pools[obj.prefabKey] = stack;
            }
            stack.Push(obj);
        }
        else
        {
            Destroy(obj.gameObject);
        }
    }
}