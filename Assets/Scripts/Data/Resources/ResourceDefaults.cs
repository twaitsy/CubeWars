using System;

public static class ResourceDefaults
{
    public const int DefaultStoragePerResource = 1000;

    public static ResourceType[] AllTypes =>
        (ResourceType[])Enum.GetValues(typeof(ResourceType));
}
