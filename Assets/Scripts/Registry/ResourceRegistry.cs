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

    private readonly Dictionary<string, List<ResourceNode>> nodesById = new();
    private readonly HashSet<ResourceNode> allNodes = new();
    public IReadOnlyCollection<ResourceNode> AllNodes => allNodes;

    // Tracks which nodes have already fired depletion
    private readonly HashSet<ResourceNode> depletedNodes = new();

    // --- Lifecycle ----------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple ResourceRegistry instances detected. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
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

        if (!list.Contains(node))
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

        // Updated: use node.IsDepleted instead of node.amount
        if (node.IsDepleted && depletedNodes.Add(node))
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

            // Updated: use node.IsDepleted instead of node.amount <= 0
            if (node.IsDepleted)
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