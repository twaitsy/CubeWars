using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class ResourceSpawner : MonoBehaviour
{
    [Serializable]
    public class ResourceSpawnConfig
    {
        public ResourceType type;
        public GameObject prefab;
        [Min(0)] public int count = 10;

        [Header("Node Contents")]
        public Vector2Int amountRange = new Vector2Int(50, 200);
        public Vector2Int valueRange = new Vector2Int(1, 5);

        [Header("Scale by Amount")]
        public bool scaleHeightByAmount = true;
        public int referenceAmount = 200;
        public float minHeightScale = 0.6f;
        public float maxHeightScale = 2.0f;
    }

    // ---------------------------------------------------------
    // DEPENDENCIES:
    // - ResourceNode: must expose `remaining` and `type`
    // - ResourceNode.amount is read-only → must assign to `remaining`
    // - Prefabs must contain ResourceNode
    // ---------------------------------------------------------

    [Header("Spawn Area (world space)")]
    public Vector3 resourceSpawnCenter = Vector3.zero;
    public Vector2 resourceSpawnSize = new Vector2(80f, 80f);
    public float spawnHeight = 20f;

    [Header("Grounding")]
    public bool snapToGround = true;
    public LayerMask groundMask = ~0;
    public float groundRayLength = 200f;

    [Header("Spawn Rules")]
    public float avoidRadius = 1.2f;
    public int maxAttemptsPerNode = 25;

    [Header("Resource Configs")]
    [Tooltip("Only resources categorized as Raw in this database are spawned.")]
    public ResourcesDatabase resourcesDatabase;
    public List<ResourceSpawnConfig> configs = new List<ResourceSpawnConfig>();

    [Header("Lifecycle")]
    public bool spawnOnStart = true;
    public bool clearPreviousChildren = true;

    void Start()
    {
        if (spawnOnStart)
            SpawnAllResources();
    }

    [ContextMenu("Spawn All Resources")]
    public void SpawnAllResources()
    {
        if (clearPreviousChildren)
            ClearChildren();

        int spawnedTotal = 0;

        foreach (var cfg in configs)
        {
            if (cfg == null || cfg.count <= 0 || cfg.prefab == null) continue;
            if (!IsRawResource(cfg.type))
            {
                Debug.LogWarning($"[ResourceSpawner] Skipping {cfg.type}: only Raw resources are allowed for spawning.");
                continue;
            }

            for (int i = 0; i < cfg.count; i++)
            {
                if (TrySpawnOne(cfg, out GameObject spawned))
                    spawnedTotal++;
            }
        }

        Debug.Log($"[ResourceSpawner] Spawned total nodes: {spawnedTotal}");
    }

    void ClearChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);
    }

    bool IsRawResource(ResourceType type)
    {
        if (resourcesDatabase == null)
            return true;

        return ResourcesDatabase.IsCategory(resourcesDatabase, type, ResourceCategory.Raw);
    }

    bool TrySpawnOne(ResourceSpawnConfig cfg, out GameObject spawned)
    {
        spawned = null;

        for (int attempt = 0; attempt < maxAttemptsPerNode; attempt++)
        {
            Vector3 pos = RandomPointInArea();

            if (snapToGround && !TrySnapToGround(ref pos))
                continue;

            if (avoidRadius > 0f && Physics.CheckSphere(pos, avoidRadius))
                continue;

            spawned = Instantiate(cfg.prefab, pos, Quaternion.identity, transform);

            var node = spawned.GetComponent<ResourceNode>();
            if (node == null)
            {
                Debug.LogWarning("[ResourceSpawner] Spawned prefab has no ResourceNode. Destroying spawned object.");
                Destroy(spawned);
                spawned = null;
                return false;
            }

            // Assign type and random amount
            node.type = cfg.type;

            // FIX: ResourceNode.amount is read-only → assign to remaining
            node.remaining = UnityEngine.Random.Range(cfg.amountRange.x, cfg.amountRange.y + 1);

            // Optional: assign value if ResourceNode has a compatible field
            TrySetValueOnNode(node, cfg.valueRange);

            // Scale object height based on amount
            if (cfg.scaleHeightByAmount)
                ScaleHeightByAmount(spawned.transform, node.remaining, cfg.referenceAmount, cfg.minHeightScale, cfg.maxHeightScale);

            return true;
        }

        return false;
    }

    Vector3 RandomPointInArea()
    {
        float halfX = resourceSpawnSize.x * 0.5f;
        float halfZ = resourceSpawnSize.y * 0.5f;

        float x = resourceSpawnCenter.x + UnityEngine.Random.Range(-halfX, halfX);
        float z = resourceSpawnCenter.z + UnityEngine.Random.Range(-halfZ, halfZ);

        return new Vector3(x, resourceSpawnCenter.y + spawnHeight, z);
    }

    bool TrySnapToGround(ref Vector3 pos)
    {
        Vector3 origin = pos + Vector3.up * 0.1f;
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, groundRayLength + 0.1f, groundMask, QueryTriggerInteraction.Ignore))
        {
            pos.y = hit.point.y;
            return true;
        }
        return false;
    }

    void ScaleHeightByAmount(Transform t, int amount, int referenceAmount, float minScale, float maxScale)
    {
        if (referenceAmount <= 0) referenceAmount = 1;

        float k = Mathf.Clamp01(amount / (float)referenceAmount);
        float heightScale = Mathf.Lerp(minScale, maxScale, k);

        Vector3 oldScale = t.localScale;
        t.localScale = new Vector3(oldScale.x, oldScale.y * heightScale, oldScale.z);

        float deltaY = oldScale.y * (heightScale - 1f);
        t.position += new Vector3(0f, deltaY * 0.5f, 0f);
    }

    void TrySetValueOnNode(ResourceNode node, Vector2Int valueRange)
    {
        int value = UnityEngine.Random.Range(valueRange.x, valueRange.y + 1);
        string[] candidates = { "valuePerUnit", "value", "nodeValue" };
        Type type = node.GetType();

        foreach (string name in candidates)
        {
            FieldInfo f = type.GetField(name, BindingFlags.Public | BindingFlags.Instance);
            if (f != null && f.FieldType == typeof(int))
            {
                f.SetValue(node, value);
                return;
            }

            PropertyInfo p = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (p != null && p.PropertyType == typeof(int) && p.CanWrite)
            {
                p.SetValue(node, value, null);
                return;
            }
        }
    }
}