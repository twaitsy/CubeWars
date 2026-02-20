using UnityEngine;

public static class ResourceIdUtility
{
    public static string GetKey(ResourceDefinition resource)
    {
        if (resource == null)
            return string.Empty;

        if (!string.IsNullOrWhiteSpace(resource.id))
            return resource.id.Trim().ToLowerInvariant();

        if (!string.IsNullOrWhiteSpace(resource.displayName))
            return resource.displayName.Trim().ToLowerInvariant();

        return string.Empty;
    }
}
