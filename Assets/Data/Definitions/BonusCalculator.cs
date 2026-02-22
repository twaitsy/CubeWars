using System;
using UnityEngine;

public static class BonusCalculator
{
    public static float GetFinalMultiplier(
        JobDefinition job,
        ToolDefinition tool,
        ResourceDefinition resource,
        CivilianDefinition worker,
        ProductionRecipeDefinition recipe,
        BuildingDefinition station)
    {
        float result = 1f;

        // ---------------------------------------------------------
        // 1. Job base skill
        // ---------------------------------------------------------
        if (job != null)
            result *= job.baseSkill;

        // ---------------------------------------------------------
        // 2. Worker training level
        // ---------------------------------------------------------
        if (worker != null)
            result *= GetWorkerTrainingMultiplier(worker);

        // ---------------------------------------------------------
        // 3. Tool base efficiency
        // ---------------------------------------------------------
        if (tool != null)
            result *= tool.baseEfficiency;

        // ---------------------------------------------------------
        // 4. Tool bonuses (resource, job, recipe, station)
        // ---------------------------------------------------------
        if (tool != null && tool.bonuses != null)
        {
            foreach (var bonus in tool.bonuses)
            {
                if (bonus == null)
                    continue;

                CivilianJobType workerJob = worker != null ? worker.jobType : CivilianJobType.Generalist;
                string stationId = station != null ? station.id : null;

                if (bonus.Matches(resource, workerJob, recipe, stationId))
                {
                    ApplyBonus(ref result, bonus);
                }
            }
        }

        // ---------------------------------------------------------
        // 5. Tool recipe bonuses
        // ---------------------------------------------------------
        if (tool != null && tool.recipeBonuses != null)
        {
            foreach (var bonus in tool.recipeBonuses)
            {
                if (bonus == null)
                    continue;

                CivilianJobType workerJob = worker != null ? worker.jobType : CivilianJobType.Generalist;
                string stationId = station != null ? station.id : null;

                if (bonus.Matches(resource, workerJob, recipe, stationId))
                {
                    ApplyBonus(ref result, bonus);
                }
            }
        }

        // ---------------------------------------------------------
        // 6. Tool station bonuses
        // ---------------------------------------------------------
        if (tool != null && tool.stationBonuses != null)
        {
            foreach (var bonus in tool.stationBonuses)
            {
                if (bonus == null)
                    continue;

                CivilianJobType workerJob = worker != null ? worker.jobType : CivilianJobType.Generalist;
                string stationId = station != null ? station.id : null;

                if (bonus.Matches(resource, workerJob, recipe, stationId))
                {
                    ApplyBonus(ref result, bonus);
                }
            }
        }

        // ---------------------------------------------------------
        // 7. Resource difficulty
        // ---------------------------------------------------------
        if (resource != null)
            result *= 1f / Mathf.Max(0.1f, resource.gatherDifficulty);

        // ---------------------------------------------------------
        // 8. Recipe difficulty
        // ---------------------------------------------------------
        if (recipe != null)
            result *= recipe.workerSpeedMultiplier;

        // ---------------------------------------------------------
        // 9. Station efficiency
        // ---------------------------------------------------------
        if (station != null)
            result *= station.productionSpeedMultiplier;

        // ---------------------------------------------------------
        // 10. Worker personality modifiers
        // ---------------------------------------------------------
        if (worker != null)
            result *= GetPersonalityModifier(worker);

        // ---------------------------------------------------------
        // 11. Worker tool effectiveness
        // ---------------------------------------------------------
        if (worker != null)
            result *= worker.toolEffectivenessMultiplier;

        // ---------------------------------------------------------
        // Clamp to avoid zero or negative multipliers
        // ---------------------------------------------------------
        return Mathf.Max(0.1f, result);
    }

    // ============================================================
    // Helper: Apply stacking mode
    // ============================================================
    private static void ApplyBonus(ref float result, BonusDefinition bonus)
    {
        switch (bonus.stackingMode)
        {
            case BonusStackingMode.Multiply:
                result *= bonus.multiplier;
                break;

            case BonusStackingMode.Add:
                result += bonus.additiveBonus;
                break;

            case BonusStackingMode.Override:
                result = bonus.multiplier;
                break;
        }
    }

    // ============================================================
    // Helper: Worker training level
    // ============================================================
    private static float GetWorkerTrainingMultiplier(CivilianDefinition worker)
    {
        return worker.trainingLevel switch
        {
            JobTrainingLevel.Novice => 0.8f,
            JobTrainingLevel.Adept => 1.0f,
            JobTrainingLevel.Expert => 1.2f,
            JobTrainingLevel.Master => 1.4f,
            _ => 1f
        };
    }

    // ============================================================
    // Helper: Personality modifiers
    // ============================================================
    private static float GetPersonalityModifier(CivilianDefinition worker)
    {
        float modifier = 1f;

        modifier *= Mathf.Lerp(0.9f, 1.1f, worker.discipline);
        modifier *= Mathf.Lerp(0.95f, 1.05f, worker.curiosity);
        modifier *= Mathf.Lerp(1.05f, 0.95f, worker.stubbornness);

        return modifier;
    }
}