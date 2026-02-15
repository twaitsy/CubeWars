public static class BonusCalculator
{
    public static float GetFinalMultiplier(
        JobDefinition job,
        ToolDefinition tool,
        ResourceDefinition resource)
    {
        float baseSkill = job != null ? job.baseSkill : 1f;
        float toolEfficiency = tool != null ? tool.baseEfficiency : 1f;
        float result = baseSkill * toolEfficiency;

        if (tool == null || tool.bonuses == null || tool.bonuses.Count == 0 || resource == null)
            return result;

        foreach (var bonus in tool.bonuses)
        {
            if (bonus == null)
                continue;

            if (bonus.Matches(resource))
                result *= bonus.multiplier;
        }

        return result;
    }
}
