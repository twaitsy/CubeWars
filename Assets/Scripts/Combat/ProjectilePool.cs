// =============================================================
// ProjectilePool.cs
//
// PURPOSE:
// - Unified projectile pooling for all weapons.
// - Replaces TurretProjectilePool.
//
// DEPENDENCIES:
// - Projectile:
//      * Must have a prefabKey reference for pooling.
//
// NOTES FOR FUTURE MAINTENANCE:
// - Supports multiple projectile types.
// - If you add explosive projectiles, they can still use this pool.
// =============================================================

using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : MonoBehaviour
{
    public static ProjectilePool Instance;

    private readonly Dictionary<Projectile, Stack<Projectile>> pools =
        new Dictionary<Projectile, Stack<Projectile>>();

    void Awake()
    {
        Instance = this;
    }

    public Projectile Spawn(Projectile prefab, Vector3 pos, Quaternion rot)
    {
        if (!pools.TryGetValue(prefab, out var stack))
        {
            stack = new Stack<Projectile>();
            pools[prefab] = stack;
        }

        Projectile obj = (stack.Count > 0) ? stack.Pop() : Instantiate(prefab);
        obj.prefabKey = prefab;
        obj.transform.SetPositionAndRotation(pos, rot);
        obj.gameObject.SetActive(true);
        return obj;
    }

    public void Despawn(Projectile obj)
    {
        if (obj == null) return;

        obj.gameObject.SetActive(false);

        if (obj.prefabKey != null)
        {
            if (!pools.TryGetValue(obj.prefabKey, out var stack))
            {
                stack = new Stack<Projectile>();
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