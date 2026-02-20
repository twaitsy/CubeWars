using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ResourceStorageProvider))]
[RequireComponent(typeof(ResourceStorageContainer))]

[DisallowMultipleComponent]
public class StorageFacility : Building
{
    [Header("Database")]
    public ResourcesDatabase resourcesDatabase;

    [Header("Storage Defaults")]
    [Min(1)] public int defaultCapacityPerResource = 500;
    public bool allowAllResourceCategories = true;
    public List<ResourceCategory> allowedCategories = new()
    {
        ResourceCategory.Raw,
        ResourceCategory.Refined,
        ResourceCategory.Food,
        ResourceCategory.Fuel,
        ResourceCategory.Luxury,
        ResourceCategory.Construction,
        ResourceCategory.Military,
        ResourceCategory.Research,
        ResourceCategory.Currency,
        ResourceCategory.Other,
        ResourceCategory.Tool,
        ResourceCategory.Misc
    };
    ResourceStorageProvider storageProvider;
    ResourceStorageContainer storageContainer;
    protected void Awake()
    {
        ConfigureStorageFromDatabase();
    }
    protected override void Start()
    {
        base.Start();
        ConfigureStorageFromDatabase();
    }

    [ContextMenu("Configure Storage From Database")]
    public void ConfigureStorageFromDatabase()
    {
        ResolveStorageComponents();
        if (storageProvider == null)
            return;

        ResourcesDatabase db = resourcesDatabase != null ? resourcesDatabase : ResourcesDatabase.Instance;
        if (db == null || db.resources == null)
            return;

        int perResourceCapacity = Mathf.Max(defaultCapacityPerResource, ResolveDefinitionCapacityFallback());
        var capacities = new List<ResourceCapacityEntry>();
        var flowEntries = new List<ResourceStorageContainer.ResourceFlowEntry>();

        for (int i = 0; i < db.resources.Count; i++)
        {
            ResourceDefinition resource = db.resources[i];
            if (resource == null || !IsAllowed(resource))
                continue;

            capacities.Add(new ResourceCapacityEntry
            {
                resource = resource,
                capacity = perResourceCapacity
            });

            flowEntries.Add(new ResourceStorageContainer.ResourceFlowEntry
            {
                resource = resource,
                flowMode = ResourceStorageContainer.ResourceFlowMode.ReceiveAndSupply
            });
        }

        storageProvider.capacities = capacities.ToArray();

        if (storageContainer != null)
            storageContainer.resourceFlow = flowEntries;

        storageProvider.RefreshRegistration();
    }

    bool IsAllowed(ResourceDefinition resource)
    {
        return allowAllResourceCategories || allowedCategories.Contains(resource.category);
    }

    int ResolveDefinitionCapacityFallback()
    {
        var loaded = GameDatabaseLoader.Loaded;
        if (loaded == null || string.IsNullOrWhiteSpace(buildingDefinitionId))
            return 0;

        if (!loaded.TryGetBuildingById(buildingDefinitionId, out BuildingDefinition definition) || definition == null)
            return 0;

        return definition.storageSettings != null ? Mathf.Max(0, definition.storageSettings.capacity) : 0;
    }

    void ResolveStorageComponents()
    {
        if (storageProvider == null)
            storageProvider = GetComponent<ResourceStorageProvider>() ?? GetComponentInChildren<ResourceStorageProvider>();

        if (storageContainer == null)
            storageContainer = GetComponent<ResourceStorageContainer>() ?? GetComponentInChildren<ResourceStorageContainer>();
    }
}
