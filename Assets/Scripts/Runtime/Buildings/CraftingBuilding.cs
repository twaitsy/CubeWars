using System.Collections.Generic;
using UnityEngine;

public class CraftingBuilding : Building
{
    public enum ProductionState
    {
        Idle,
        WaitingForInputs,
        InputsReady,
        InProgress,
        OutputReady,
        WaitingForPickup
    }

    [System.Serializable]
    public class BuildingUpgradeTier
    {
        public string name = "Upgrade";
        public int additionalWorkerSlots;
        public float craftingSpeedMultiplier = 1f;
        public float inputEfficiencyMultiplier = 1f;
        public float outputEfficiencyMultiplier = 1f;
    }

    [Header("Recipe")]
    public ProductionRecipeDefinition recipe;
    [Tooltip("When assigned, crafting outputs must be resources categorized as Refined in this database.")]
    public ResourcesDatabase resourcesDatabase;

    [Header("Workflow Points")]
    public Transform inputSlot;
    public Transform outputSlot;
    public Transform[] workPoints;

    [Header("Throughput")]
    [Min(1)] public int maxWorkers = 2;
    [Min(1)] public int maxInputCapacityPerResource = 50;
    [Min(1)] public int maxOutputCapacityPerResource = 50;
    [Min(0.1f)] public float craftingSpeedModifier = 1f;

    [Header("Logistics")]
    public bool requireHaulerLogistics = true;

    [Header("Assignment")]
    [Range(0, 10)] public int assignmentPriority = 5;
    public bool allowAutomaticAssignment = true;

    [Header("Optional FX")]
    public ParticleSystem[] productionEffects;
    public AudioSource productionLoopAudio;

    [Header("Progress UI")]
    public bool showProgressBar = true;
    public Vector3 progressBarOffset = new Vector3(0f, 2.8f, 0f);
    public float progressBarWidth = 1.4f;
    public float progressBarHeight = 0.12f;

    [Header("Upgrades")]
    public List<BuildingUpgradeTier> upgrades = new List<BuildingUpgradeTier>();

    public ProductionState State => state;
    public float CraftProgress01 => craftDuration <= 0f ? 0f : Mathf.Clamp01(craftTimer / craftDuration);
    public IReadOnlyDictionary<ResourceDefinition, int> InputBuffer => inputBuffer;
    public IReadOnlyDictionary<ResourceDefinition, int> OutputQueue => outputQueue;
    public IReadOnlyList<Civilian> AssignedWorkers => assignedWorkers;

    readonly Dictionary<ResourceDefinition, int> inputBuffer = new Dictionary<ResourceDefinition, int>();
    readonly Dictionary<ResourceDefinition, int> outputQueue = new Dictionary<ResourceDefinition, int>();
    readonly Dictionary<Civilian, int> occupiedWorkpoints = new Dictionary<Civilian, int>();
    readonly Dictionary<Civilian, float> inactiveAssignedWorkerTimers = new Dictionary<Civilian, float>();
    readonly List<Civilian> assignedWorkers = new List<Civilian>();

    ProductionState state;
    float craftTimer;
    float craftDuration;
    int upgradeLevel;
    WorldProgressBar progressBar;


    protected override void ApplyDefinition(BuildingDefinition def)
    {
        base.ApplyDefinition(def);
        if (def == null)
            return;

        if (def.workerSlots > 0)
            maxWorkers = def.workerSlots;

        if (def.recipes != null && def.recipes.Count > 0)
            recipe = def.recipes[0];
    }

    protected override void Start()
    {
        base.Start();
        InitializeResourceMaps();
        state = ProductionState.WaitingForInputs;
        EnsureProgressBar();
        CraftingJobManager.Instance?.RegisterBuilding(this);
    }

    void OnEnable()
    {
        CraftingJobManager.Instance?.RegisterBuilding(this);
    }

    void OnDisable()
    {
        CraftingJobManager.Instance?.UnregisterBuilding(this);
    }

    void Update()
    {
        TickState();
        TickProductionFx();
        TickProgressBar();
    }

    void EnsureProgressBar()
    {
        if (!showProgressBar)
            return;

        if (progressBar == null)
            progressBar = GetComponent<WorldProgressBar>();

        if (progressBar == null)
            progressBar = gameObject.AddComponent<WorldProgressBar>();

        progressBar.offset = progressBarOffset;
        progressBar.width = Mathf.Max(0.1f, progressBarWidth);
        progressBar.height = Mathf.Max(0.02f, progressBarHeight);
        progressBar.hideWhenZero = true;
        progressBar.progress01 = 0f;
    }

    void TickProgressBar()
    {
        if (!showProgressBar)
            return;

        if (progressBar == null)
            EnsureProgressBar();

        if (progressBar == null)
            return;

        progressBar.progress01 = state == ProductionState.InProgress ? CraftProgress01 : 0f;
    }

    public string GetProductionBlockerReason()
    {
        if (recipe == null)
            return "No recipe selected.";

        switch (state)
        {
            case ProductionState.WaitingForInputs:
                return $"Missing inputs: {GetMissingInputSummary()}";
            case ProductionState.InputsReady:
                return "Inputs ready, waiting for a worker with the required job.";
            case ProductionState.WaitingForPickup:
                return "Output buffer is full and must be hauled to storage.";
            case ProductionState.Idle:
                return "Building is idle.";
            case ProductionState.InProgress:
                return "Production is running.";
            case ProductionState.OutputReady:
                return "Outputs are ready to collect.";
            default:
                return string.Empty;
        }
    }

    void InitializeResourceMaps()
    {
        inputBuffer.Clear();
        outputQueue.Clear();

        var db = resourcesDatabase != null ? resourcesDatabase : ResourcesDatabase.Instance;

        // 1. Add all resources from the database
        if (db != null && db.resources != null)
        {
            foreach (var def in db.resources)
            {
                if (def == null) continue;
                inputBuffer[def] = 0;
                outputQueue[def] = 0;
            }
        }

        // 2. Ensure all recipe resources are included
        if (recipe != null)
        {
            if (recipe.inputs != null)
            {
                foreach (var entry in recipe.inputs)
                {
                    if (entry?.resource == null) continue;
                    if (!inputBuffer.ContainsKey(entry.resource))
                        inputBuffer[entry.resource] = 0;
                }
            }

            if (recipe.outputs != null)
            {
                foreach (var entry in recipe.outputs)
                {
                    if (entry?.resource == null) continue;
                    if (!outputQueue.ContainsKey(entry.resource))
                        outputQueue[entry.resource] = 0;
                }
            }
        }
    }

    void TickState()
    {
        if (recipe == null)
        {
            state = ProductionState.Idle;
            return;
        }

        if (HasOutputPressure())
        {
            state = ProductionState.WaitingForPickup;
            ProductionNotificationManager.Instance?.NotifyIfReady($"output-full-{GetInstanceID()}", $"{name}: output queue full.");
            return;
        }

        if (!HasRequiredInputs())
        {
            state = ProductionState.WaitingForInputs;
            ProductionNotificationManager.Instance?.NotifyIfReady($"inputs-missing-{GetInstanceID()}", $"{name}: waiting for crafting inputs.");
            return;
        }

        TickInactiveAssignedWorkers();

        int activeWorkers = GetActiveWorkerCount();
        if (activeWorkers <= 0)
        {
            state = ProductionState.InputsReady;
            ProductionNotificationManager.Instance?.NotifyIfReady($"workers-missing-{GetInstanceID()}", $"{name}: no workers assigned.");
            return;
        }

        state = ProductionState.InProgress;

        if (craftDuration <= 0f)
            craftDuration = GetCraftDuration();

        craftTimer += Time.deltaTime * activeWorkers;
        if (craftTimer >= craftDuration)
            CompleteCraft();
    }

    void TickInactiveAssignedWorkers()
    {
        for (int i = assignedWorkers.Count - 1; i >= 0; i--)
        {
            Civilian worker = assignedWorkers[i];
            if (worker == null)
            {
                assignedWorkers.RemoveAt(i);
                continue;
            }

            if (IsWorkerActive(worker))
            {
                inactiveAssignedWorkerTimers[worker] = 0f;
                continue;
            }

            float timer = inactiveAssignedWorkerTimers.TryGetValue(worker, out float value) ? value : 0f;
            timer += Time.deltaTime;
            inactiveAssignedWorkerTimers[worker] = timer;

            if (allowAutomaticAssignment && timer >= 30f)
            {
                Debug.LogWarning($"[{nameof(CraftingBuilding)}] Unassigning inactive worker '{worker.name}' from '{name}'.");
                inactiveAssignedWorkerTimers.Remove(worker);
                worker.ClearCraftingAssignment();
                RemoveWorker(worker);
            }
        }
    }

    void TickProductionFx()
    {
        bool producing = state == ProductionState.InProgress;

        if (productionEffects != null)
        {
            for (int i = 0; i < productionEffects.Length; i++)
            {
                var fx = productionEffects[i];
                if (fx == null) continue;

                if (producing && !fx.isPlaying)
                    fx.Play();
                else if (!producing && fx.isPlaying)
                    fx.Stop();
            }
        }

        if (productionLoopAudio != null)
        {
            if (producing && !productionLoopAudio.isPlaying)
                productionLoopAudio.Play();
            else if (!producing && productionLoopAudio.isPlaying)
                productionLoopAudio.Stop();
        }
    }

    float GetCraftDuration()
    {
        float speed = Mathf.Max(0.1f, craftingSpeedModifier);
        speed *= Mathf.Max(0.1f, GetUpgradeCraftingSpeedMultiplier());
        speed *= Mathf.Max(0.1f, GetWorkerToolSpeedMultiplier());
        return Mathf.Max(0.05f, recipe.craftTimeSeconds / speed);
    }


    float GetWorkerToolSpeedMultiplier()
    {
        float total = 0f;
        int count = 0;

        for (int i = 0; i < assignedWorkers.Count; i++)
        {
            Civilian worker = assignedWorkers[i];
            if (worker == null)
                continue;

            total += Mathf.Max(0.1f, worker.GetCraftingSpeedMultiplierFromTools());
            count++;
        }

        return count > 0 ? total / count : 1f;
    }
    int GetEffectiveInputRequirement(RecipeResourceAmount entry)
    {
        float upgradeMult = Mathf.Max(0.1f, GetUpgradeInputEfficiencyMultiplier());
        float combined = recipe.inputEfficiencyMultiplier * upgradeMult;
        return Mathf.Max(1, Mathf.CeilToInt(entry.amount * recipe.batchSize * combined));
    }

    int GetEffectiveOutputAmount(RecipeResourceAmount entry)
    {
        float upgradeMult = Mathf.Max(0.1f, GetUpgradeOutputEfficiencyMultiplier());
        float combined = recipe.outputEfficiencyMultiplier * upgradeMult;
        return Mathf.Max(1, Mathf.RoundToInt(entry.amount * recipe.batchSize * combined));
    }

    bool IsValidEntry(RecipeResourceAmount entry)
    {
        return entry != null && entry.resource != null;
    }

    bool IsAllowedOutputType(ResourceDefinition type)
    {
        if (resourcesDatabase == null)
            return true;

        return resourcesDatabase.TryGetById(ResourceIdUtility.GetKey(type), out ResourceDefinition definition) &&
               resourcesDatabase.IsCategory(definition, ResourceCategory.Refined);
    }

    public void SetRecipe(ProductionRecipeDefinition newRecipe)
    {
        if (recipe == newRecipe)
            return;

        recipe = newRecipe;
        craftTimer = 0f;
        craftDuration = 0f;
        InitializeResourceMaps();
        state = recipe == null ? ProductionState.Idle : ProductionState.WaitingForInputs;
    }

    public bool NeedsInput(ResourceDefinition type)
    {
        if (recipe?.inputs == null) return false;

        for (int i = 0; i < recipe.inputs.Length; i++)
        {
            var entry = recipe.inputs[i];
            if (!IsValidEntry(entry) || entry.resource != type) continue;

            int needed = GetEffectiveInputRequirement(entry);
            return !inputBuffer.TryGetValue(type, out int buffered) || buffered < needed;
        }

        return false;
    }

    public bool NeedsAnyInput()
    {
        if (recipe?.inputs == null || recipe.inputs.Length == 0)
            return false;

        for (int i = 0; i < recipe.inputs.Length; i++)
        {
            var entry = recipe.inputs[i];
            if (!IsValidEntry(entry))
                continue;

            int needed = GetEffectiveInputRequirement(entry);
            if (!inputBuffer.TryGetValue(entry.resource, out int buffered) || buffered < needed)
                return true;
        }

        return false;
    }

    public bool HasAnyOutputQueued()
    {
        if (recipe?.outputs == null || recipe.outputs.Length == 0)
            return false;

        for (int i = 0; i < recipe.outputs.Length; i++)
        {
            var entry = recipe.outputs[i];
            if (!IsValidEntry(entry) || !IsAllowedOutputType(entry.resource)) continue;
            if (outputQueue.TryGetValue(entry.resource, out int queued) && queued > 0)
                return true;
        }

        return false;
    }

    bool HasRequiredInputs()
    {
        if (recipe?.inputs == null || recipe.inputs.Length == 0)
            return true;

        for (int i = 0; i < recipe.inputs.Length; i++)
        {
            var entry = recipe.inputs[i];
            if (!IsValidEntry(entry))
                continue;

            int need = GetEffectiveInputRequirement(entry);
            if (!inputBuffer.TryGetValue(entry.resource, out int buffered) || buffered < need)
                return false;
        }

        return true;
    }

    bool HasOutputPressure()
    {
        if (recipe?.outputs == null) return false;

        for (int i = 0; i < recipe.outputs.Length; i++)
        {
            var entry = recipe.outputs[i];
            if (!IsValidEntry(entry) || !IsAllowedOutputType(entry.resource)) continue;
            if (outputQueue.TryGetValue(entry.resource, out int queued) && queued >= maxOutputCapacityPerResource)
                return true;
        }

        return false;
    }

    public int GetActiveWorkerCount()
    {
        int count = 0;
        for (int i = assignedWorkers.Count - 1; i >= 0; i--)
        {
            var worker = assignedWorkers[i];
            if (worker == null)
            {
                assignedWorkers.RemoveAt(i);
                continue;
            }

            if (IsWorkerActive(worker))
                count++;
        }
        return count;
    }

    bool IsWorkerActive(Civilian worker)
    {
        if (worker == null)
            return false;

        if (!worker.IsAtAssignedCraftingWorkPoint)
            return false;

        float workerRange = Mathf.Max(1f, worker.stopDistance * 1.5f);
        return (worker.transform.position - transform.position).sqrMagnitude <= workerRange * workerRange
               || occupiedWorkpoints.ContainsKey(worker)
               || IsWorkerInOperatingRange(worker);
    }

    bool IsWorkerInOperatingRange(Civilian worker)
    {
        if (worker == null) return false;

        Vector3 anchor = transform.position;
        if (workPoints != null)
        {
            for (int i = 0; i < workPoints.Length; i++)
            {
                if (workPoints[i] == null) continue;
                if ((worker.transform.position - workPoints[i].position).sqrMagnitude <= 2.25f)
                    return true;
            }
        }

        return (worker.transform.position - anchor).sqrMagnitude <= 4f;
    }

    void CompleteCraft()
    {
        craftTimer = 0f;
        craftDuration = 0f;

        ConsumeInputs();
        ProduceOutputs();

        state = HasOutputPressure() ? ProductionState.WaitingForPickup : ProductionState.OutputReady;
    }

    void ConsumeInputs()
    {
        if (recipe?.inputs == null) return;

        for (int i = 0; i < recipe.inputs.Length; i++)
        {
            var entry = recipe.inputs[i];
            if (!IsValidEntry(entry))
                continue;

            int need = GetEffectiveInputRequirement(entry);
            int current = inputBuffer.TryGetValue(entry.resource, out int buffered) ? buffered : 0;
            inputBuffer[entry.resource] = Mathf.Max(0, current - need);
        }
    }

    void ProduceOutputs()
    {
        if (recipe?.outputs == null) return;

        for (int i = 0; i < recipe.outputs.Length; i++)
        {
            var entry = recipe.outputs[i];
            if (!IsValidEntry(entry))
                continue;

            if (!IsAllowedOutputType(entry.resource))
            {
                Debug.LogWarning($"[{nameof(CraftingBuilding)}] Ignoring output {entry.resource} on {name}: only Refined resources are allowed.");
                continue;
            }

            int produce = GetEffectiveOutputAmount(entry);
            int current = outputQueue.TryGetValue(entry.resource, out int queued) ? queued : 0;
            outputQueue[entry.resource] = Mathf.Min(maxOutputCapacityPerResource, current + produce);
        }
    }

    public bool TryAssignWorker(Civilian civilian, bool manual = false)
    {
        if (civilian == null || recipe == null) return false;
        if (!allowAutomaticAssignment && !manual) return false;
        if (assignedWorkers.Contains(civilian)) return true;
        if (assignedWorkers.Count >= GetMaxWorkers()) return false;
        if (recipe.requiredJobType != CivilianJobType.Generalist && civilian.JobType != recipe.requiredJobType) return false;

        assignedWorkers.Add(civilian);
        return true;
    }

    public void RemoveWorker(Civilian civilian)
    {
        if (civilian == null) return;
        assignedWorkers.Remove(civilian);
        occupiedWorkpoints.Remove(civilian);
        inactiveAssignedWorkerTimers.Remove(civilian);
    }

    public bool TryReserveWorkPoint(Civilian civilian, out Transform workPoint)
    {
        workPoint = null;
        if (civilian == null || workPoints == null || workPoints.Length == 0)
            return false;

        for (int i = 0; i < workPoints.Length; i++)
        {
            var point = workPoints[i];
            if (point == null) continue;

            if (occupiedWorkpoints.TryGetValue(civilian, out int existingIndex) && existingIndex == i)
            {
                workPoint = point;
                return true;
            }

            bool occupied = false;
            foreach (var kv in occupiedWorkpoints)
            {
                if (kv.Key == civilian) continue;
                if (kv.Value == i)
                {
                    occupied = true;
                    break;
                }
            }

            if (occupied) continue;

            occupiedWorkpoints[civilian] = i;
            workPoint = point;
            return true;
        }

        return false;
    }

    public void ReleaseWorkPoint(Civilian civilian)
    {
        if (civilian == null) return;
        occupiedWorkpoints.Remove(civilian);
    }

    public bool TryDeliverInput(ResourceDefinition type, int amount, out int accepted)
    {
        accepted = 0;
        if (amount <= 0) return false;

        int current = inputBuffer.TryGetValue(type, out int existing) ? existing : 0;
        int free = Mathf.Max(0, maxInputCapacityPerResource - current);
        accepted = Mathf.Min(amount, free);
        if (accepted <= 0) return false;

        inputBuffer[type] += accepted;
        return true;
    }

    public int CollectOutput(ResourceDefinition type, int amount)
    {
        if (amount <= 0) return 0;
        int available = outputQueue.TryGetValue(type, out int queued) ? queued : 0;
        int taken = Mathf.Min(amount, available);
        outputQueue[type] = available - taken;
        return taken;
    }

    // ---------------------------------------------------------
    // UPDATED METHOD  OPTION A: Haulers fill to full capacity
    // ---------------------------------------------------------
    public bool TryGetInputRequest(
        Vector3 workerPosition,
        out ResourceDefinition neededType,
        out int amount,
        out ResourceStorageContainer nearestStorage)
    {
        neededType = default;
        amount = 0;
        nearestStorage = null;

        if (recipe?.inputs == null || TeamStorageManager.Instance == null)
            return false;

        for (int i = 0; i < recipe.inputs.Length; i++)
        {
            var entry = recipe.inputs[i];
            if (!IsValidEntry(entry))
                continue;

            var type = entry.resource;

            // NEW BEHAVIOR:
            // Request enough to fill the entire input buffer.
            int current = inputBuffer.TryGetValue(type, out int existing) ? existing : 0;
            int missing = Mathf.Max(0, maxInputCapacityPerResource - current);

            if (missing <= 0)
                continue;

            var storage = TeamStorageManager.Instance.FindNearestStorageWithStored(
                teamID,
                type,
                workerPosition);

            if (storage == null)
                continue;

            neededType = type;
            amount = missing;
            nearestStorage = storage;
            return true;
        }

        return false;
    }

    public bool TryGetOutputRequest(Vector3 workerPosition, out ResourceDefinition outputType, out int amount, out ResourceStorageContainer nearestStorage)
    {
        outputType = default;
        amount = 0;
        nearestStorage = null;

        if (recipe?.outputs == null)
            return false;

        for (int i = 0; i < recipe.outputs.Length; i++)
        {
            var entry = recipe.outputs[i];
            if (!IsValidEntry(entry))
                continue;

            if (!IsAllowedOutputType(entry.resource)) continue;

            int available = outputQueue.TryGetValue(entry.resource, out int queued) ? queued : 0;
            if (available <= 0) continue;

            var storage = FindNearestExternalStorageWithFree(entry.resource);
            if (storage == null) continue;

            outputType = entry.resource;
            amount = available;
            nearestStorage = storage;
            return true;
        }

        return false;
    }

    ResourceStorageContainer FindNearestExternalStorageWithFree(ResourceDefinition type)
    {
        var storages = FindObjectsOfType<ResourceStorageContainer>();
        var localStorages = GetComponentsInChildren<ResourceStorageContainer>(true);

        ResourceStorageContainer best = null;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < storages.Length; i++)
        {
            var storage = storages[i];
            if (storage == null) continue;
            if (storage.teamID != teamID) continue;
            if (!storage.CanReceive(type)) continue;
            if (storage.GetFree(type) <= 0) continue;

            bool isLocal = false;
            for (int j = 0; j < localStorages.Length; j++)
            {
                if (storage == localStorages[j])
                {
                    isLocal = true;
                    break;
                }
            }

            if (isLocal) continue;

            float distance = (storage.transform.position - transform.position).sqrMagnitude;
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = storage;
            }
        }

        return best;
    }

    public bool HasAssignedHauler()
    {
        for (int i = assignedWorkers.Count - 1; i >= 0; i--)
        {
            var worker = assignedWorkers[i];
            if (worker == null)
            {
                assignedWorkers.RemoveAt(i);
                continue;
            }

            if (worker.role == CivilianRole.Hauler)
                return true;
        }

        return false;
    }

    public int GetMaxWorkers()
    {
        return Mathf.Max(1, maxWorkers + GetUpgradeWorkerSlots());
    }

    public int GetUpgradeWorkerSlots()
    {
        int sum = 0;
        for (int i = 0; i < upgradeLevel && i < upgrades.Count; i++)
            sum += Mathf.Max(0, upgrades[i].additionalWorkerSlots);
        return sum;
    }

    float GetUpgradeCraftingSpeedMultiplier()
    {
        float value = 1f;
        for (int i = 0; i < upgradeLevel && i < upgrades.Count; i++)
            value *= Mathf.Max(0.1f, upgrades[i].craftingSpeedMultiplier);
        return value;
    }

    float GetUpgradeInputEfficiencyMultiplier()
    {
        float value = 1f;
        for (int i = 0; i < upgradeLevel && i < upgrades.Count; i++)
            value *= Mathf.Max(0.1f, upgrades[i].inputEfficiencyMultiplier);
        return value;
    }

    float GetUpgradeOutputEfficiencyMultiplier()
    {
        float value = 1f;
        for (int i = 0; i < upgradeLevel && i < upgrades.Count; i++)
            value *= Mathf.Max(0.1f, upgrades[i].outputEfficiencyMultiplier);
        return value;
    }

    public bool ApplyUpgrade()
    {
        if (upgradeLevel >= upgrades.Count)
            return false;

        upgradeLevel++;
        return true;
    }

    public int GetWorkPointOccupancy()
    {
        return occupiedWorkpoints.Count;
    }

    public string GetMissingInputSummary()
    {
        if (recipe?.inputs == null) return "No recipe";

        List<string> parts = new List<string>();
        for (int i = 0; i < recipe.inputs.Length; i++)
        {
            var entry = recipe.inputs[i];
            if (!IsValidEntry(entry))
                continue;

            int needed = GetEffectiveInputRequirement(entry);

            int current = inputBuffer.TryGetValue(entry.resource, out int buffered) ? buffered : 0;
            int missing = Mathf.Max(0, needed - current);
            if (missing > 0)
                parts.Add($"{entry.resource}:{missing}");
        }

        if (parts.Count == 0) return "None";
        return string.Join(", ", parts);
    }
}
