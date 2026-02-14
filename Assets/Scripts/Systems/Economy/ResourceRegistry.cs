using System.Collections.Generic;
using UnityEngine;

public class ResourceRegistry : MonoBehaviour
{
    public static ResourceRegistry Instance;
    readonly Dictionary<string, List<ResourceNode>> nodesById = new Dictionary<string, List<ResourceNode>>();

    void Awake() => Instance = this;

    public void Register(ResourceNode node)
    {
        if (node == null || node.resource == null) return;
        string key = ResourceIdUtility.GetKey(node.resource);
        if (!nodesById.TryGetValue(key, out var list))
        {
            list = new List<ResourceNode>();
            nodesById[key] = list;
        }
        if (!list.Contains(node)) list.Add(node);
    }

    public void Unregister(ResourceNode node)
    {
        if (node == null || node.resource == null) return;
        string key = ResourceIdUtility.GetKey(node.resource);
        if (nodesById.TryGetValue(key, out var list)) list.Remove(node);
    }

    public List<ResourceNode> GetNodes(ResourceDefinition resource)
    {
        string key = ResourceIdUtility.GetKey(resource);
        if (!nodesById.TryGetValue(key, out var list))
        {
            list = new List<ResourceNode>();
            nodesById[key] = list;
        }
        return list;
    }
}
