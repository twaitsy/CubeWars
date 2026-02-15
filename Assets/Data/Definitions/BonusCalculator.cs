public static class BonusCalculator
{
    public static float GetFinalMultiplier(
        JobDefinition job,
        ToolDefinition tool,
        ResourceDefinition resource)
    {
        float result = job.baseSkill * tool.baseEfficiency;

        foreach (var bonus in tool.bonuses)
        {
            switch (bonus.targetType)
            {
                case BonusTargetType.Resource:
                    if (bonus.resource == resource)
                        result *= bonus.multiplier;
                    break;

                case BonusTargetType.Category:
                    if (bonus.category == resource.category)
                        result *= bonus.multiplier;
                    break;
            }
        }

        return result;
    }
}