using System;
using System.Collections.Generic;
using UnityEngine;

public class ResourceRegistry : MonoBehaviour
{
    public static ResourceRegistry Instance { get; private set; }

    // --- Events -------------------------------------------------------------

    public static event Action<ResourceNode> OnNodeRegistered;
    public static event Action<ResourceNode> OnNodeUnregistered;
    public static event Action<ResourceNode> OnNodeChanged;
    public static event Action<ResourceNode> OnNodeDepleted;

    // --- Data ---------------------------------------------------------------

    readonly Dictionary<string, List<ResourceNode>> nodesById =
        new();

    readonly HashSet<ResourceNode> allNodes = new();
    public IReadOnlyCollection<ResourceNode> AllNodes => allNodes;

    // Tracks which nodes have already fired depletion
    readonly HashSet<ResourceNode> depletedNodes = new();

    // --- Lifecycle ----------------------------------------------------------

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple ResourceRegistry instances detected. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    // --- Registration -------------------------------------------------------

    public void Register(ResourceNode node)
    {
        if (node == null || node.resource == null)
            return;

        string key = ResourceIdUtility.GetKey(node.resource);

        if (!nodesById.TryGetValue(key, out var list))
        {
            list = new List<ResourceNode>();
            nodesById.Add(key, list);
        }

        if (list.Contains(node))
            return;

        list.Add(node);
        allNodes.Add(node);
        depletedNodes.Remove(node);

        OnNodeRegistered?.Invoke(node);
    }

    public void Unregister(ResourceNode node)
    {
        if (node == null || node.resource == null)
            return;

        string key = ResourceIdUtility.GetKey(node.resource);

        if (nodesById.TryGetValue(key, out var list))
        {
            list.Remove(node);

            if (list.Count == 0)
                nodesById.Remove(key);
        }

        allNodes.Remove(node);
        depletedNodes.Remove(node);

        OnNodeUnregistered?.Invoke(node);
    }

    // --- Change Notifications ----------------------------------------------

    public void NotifyNodeChanged(ResourceNode node)
    {
        if (node == null)
            return;

        if (node.amount <= 0 && depletedNodes.Add(node))
        {
            OnNodeDepleted?.Invoke(node);
        }

        OnNodeChanged?.Invoke(node);
    }

    // --- Queries ------------------------------------------------------------

    public IReadOnlyList<ResourceNode> GetNodes(ResourceDefinition resource)
    {
        if (resource == null)
            return Array.Empty<ResourceNode>();

        string key = ResourceIdUtility.GetKey(resource);

        if (!nodesById.TryGetValue(key, out var list))
            return Array.Empty<ResourceNode>();

        return list;
    }

    public ResourceNode GetNearestNode(Vector3 position, ResourceDefinition resource)
    {
        if (resource == null)
            return null;

        if (!nodesById.TryGetValue(ResourceIdUtility.GetKey(resource), out var list))
            return null;

        ResourceNode best = null;
        float bestDist = float.MaxValue;

        for (int i = list.Count - 1; i >= 0; i--)
        {
            var node = list[i];

            if (node == null)
            {
                list.RemoveAt(i);
                continue;
            }

            if (node.amount <= 0)
                continue;

            float dist = (node.transform.position - position).sqrMagnitude;
            if (dist < bestDist)
            {
                bestDist = dist;
                best = node;
            }
        }

        return best;
    }
}
