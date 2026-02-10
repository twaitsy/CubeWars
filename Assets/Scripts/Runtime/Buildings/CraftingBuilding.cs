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
    public ProductionRecipe recipe;

    [Header("Workflow Points")]
    public Transform inputSlot;
    public Transform outputSlot;
    public Transform[] workPoints;

    [Header("Throughput")]
    [Min(1)] public int maxWorkers = 2;
    [Min(1)] public int maxInputCapacityPerResource = 50;
    [Min(1)] public int maxOutputCapacityPerResource = 50;
    [Min(0.1f)] public float craftingSpeedModifier = 1f;

    [Header("Assignment")]
    [Range(0, 10)] public int assignmentPriority = 5;
    public bool allowAutomaticAssignment = true;

    [Header("Optional FX")]
    public ParticleSystem[] productionEffects;
    public AudioSource productionLoopAudio;

    [Header("Upgrades")]
    public List<BuildingUpgradeTier> upgrades = new List<BuildingUpgradeTier>();

    public ProductionState State => state;
    public float CraftProgress01 => craftDuration <= 0f ? 0f : Mathf.Clamp01(craftTimer / craftDuration);
    public IReadOnlyDictionary<ResourceType, int> InputBuffer => inputBuffer;
    public IReadOnlyDictionary<ResourceType, int> OutputQueue => outputQueue;
    public IReadOnlyList<Civilian> AssignedWorkers => assignedWorkers;

    readonly Dictionary<ResourceType, int> inputBuffer = new Dictionary<ResourceType, int>();
    readonly Dictionary<ResourceType, int> outputQueue = new Dictionary<ResourceType, int>();
    readonly Dictionary<Civilian, int> occupiedWorkpoints = new Dictionary<Civilian, int>();
    readonly List<Civilian> assignedWorkers = new List<Civilian>();

    ProductionState state;
    float craftTimer;
    float craftDuration;
    int upgradeLevel;

    protected override void Start()
    {
        base.Start();
        InitializeResourceMaps();
        state = ProductionState.WaitingForInputs;
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
    }

    void InitializeResourceMaps()
    {
        foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
        {
            inputBuffer[type] = 0;
            outputQueue[type] = 0;
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

        if (GetActiveWorkerCount() <= 0)
        {
            state = ProductionState.InputsReady;
            ProductionNotificationManager.Instance?.NotifyIfReady($"workers-missing-{GetInstanceID()}", $"{name}: no workers assigned.");
            return;
        }

        state = ProductionState.InProgress;

        if (craftDuration <= 0f)
            craftDuration = GetCraftDuration();

        craftTimer += Time.deltaTime;
        if (craftTimer >= craftDuration)
            CompleteCraft();
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
        return Mathf.Max(0.05f, recipe.craftTimeSeconds / speed);
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

    public bool NeedsInput(ResourceType type)
    {
        if (recipe?.inputs == null) return false;

        for (int i = 0; i < recipe.inputs.Length; i++)
        {
            var entry = recipe.inputs[i];
            if (entry.resourceType != type) continue;

            int needed = GetEffectiveInputRequirement(entry);
            return inputBuffer[type] < needed;
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
            int need = GetEffectiveInputRequirement(entry);
            if (inputBuffer[entry.resourceType] < need)
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
            if (outputQueue[entry.resourceType] >= maxOutputCapacityPerResource)
                return true;
        }

        return false;
    }

    int GetActiveWorkerCount()
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

            if (occupiedWorkpoints.ContainsKey(worker))
                count++;
        }
        return count;
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
            int need = GetEffectiveInputRequirement(entry);
            inputBuffer[entry.resourceType] = Mathf.Max(0, inputBuffer[entry.resourceType] - need);
        }
    }

    void ProduceOutputs()
    {
        if (recipe?.outputs == null) return;

        for (int i = 0; i < recipe.outputs.Length; i++)
        {
            var entry = recipe.outputs[i];
            int produce = GetEffectiveOutputAmount(entry);
            outputQueue[entry.resourceType] = Mathf.Min(maxOutputCapacityPerResource, outputQueue[entry.resourceType] + produce);
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

    public bool TryDeliverInput(ResourceType type, int amount, out int accepted)
    {
        accepted = 0;
        if (amount <= 0) return false;

        int free = Mathf.Max(0, maxInputCapacityPerResource - inputBuffer[type]);
        accepted = Mathf.Min(amount, free);
        if (accepted <= 0) return false;

        inputBuffer[type] += accepted;
        return true;
    }

    public int CollectOutput(ResourceType type, int amount)
    {
        if (amount <= 0) return 0;
        int taken = Mathf.Min(amount, outputQueue[type]);
        outputQueue[type] -= taken;
        return taken;
    }

    public bool TryGetInputRequest(Vector3 workerPosition, out ResourceType neededType, out int amount, out ResourceStorageContainer nearestStorage)
    {
        neededType = default;
        amount = 0;
        nearestStorage = null;

        if (recipe?.inputs == null || TeamStorageManager.Instance == null)
            return false;

        for (int i = 0; i < recipe.inputs.Length; i++)
        {
            var entry = recipe.inputs[i];
            int need = GetEffectiveInputRequirement(entry);
            int missing = Mathf.Max(0, need - inputBuffer[entry.resourceType]);
            if (missing <= 0) continue;

            var storage = TeamStorageManager.Instance.FindNearestStorageWithStored(teamID, entry.resourceType, workerPosition);
            if (storage == null) continue;

            neededType = entry.resourceType;
            amount = Mathf.Min(missing, maxInputCapacityPerResource);
            nearestStorage = storage;
            return true;
        }

        return false;
    }

    public bool TryGetOutputRequest(Vector3 workerPosition, out ResourceType outputType, out int amount, out ResourceStorageContainer nearestStorage)
    {
        outputType = default;
        amount = 0;
        nearestStorage = null;

        if (recipe?.outputs == null || TeamStorageManager.Instance == null)
            return false;

        for (int i = 0; i < recipe.outputs.Length; i++)
        {
            var entry = recipe.outputs[i];
            int available = outputQueue[entry.resourceType];
            if (available <= 0) continue;

            var storage = TeamStorageManager.Instance.FindNearestStorageWithFree(teamID, entry.resourceType, workerPosition);
            if (storage == null) continue;

            outputType = entry.resourceType;
            amount = available;
            nearestStorage = storage;
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
            int needed = GetEffectiveInputRequirement(entry);
            int missing = Mathf.Max(0, needed - inputBuffer[entry.resourceType]);
            if (missing > 0)
                parts.Add($"{entry.resourceType}:{missing}");
        }

        if (parts.Count == 0) return "None";
        return string.Join(", ", parts);
    }
}
