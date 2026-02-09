using System;
using System.Collections.Generic;
using UnityEngine;

public class ResourceRegistry : MonoBehaviour
{
    public static ResourceRegistry Instance;

    private readonly Dictionary<ResourceType, List<ResourceNode>> nodesByType = new Dictionary<ResourceType, List<ResourceNode>>();

    void Awake()
    {
        Instance = this;

        // Pre-create lists for all enum values so lookups are cheap
        foreach (ResourceType t in Enum.GetValues(typeof(ResourceType)))
            nodesByType[t] = new List<ResourceNode>();
    }

    public void Register(ResourceNode node)
    {
        if (node == null) return;
        if (!nodesByType.TryGetValue(node.type, out var list))
        {
            list = new List<ResourceNode>();
            nodesByType[node.type] = list;
        }
        if (!list.Contains(node)) list.Add(node);
    }

    public void Unregister(ResourceNode node)
    {
        if (node == null) return;
        if (nodesByType.TryGetValue(node.type, out var list))
            list.Remove(node);
    }

    public List<ResourceNode> GetNodes(ResourceType type)
    {
        if (!nodesByType.TryGetValue(type, out var list))
        {
            list = new List<ResourceNode>();
            nodesByType[type] = list;
        }
        return list;
    }
}
