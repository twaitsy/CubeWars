using System;
using UnityEngine;

[Serializable]
public struct SkillProgress
{
    public int level;
    public float xp;
}

public class CharacterStats : MonoBehaviour
{
    [Header("Base Stats")]
    public float baseMoveSpeed = 2.5f;
    public int baseCarryCapacity = 30;
    public int baseHarvestPerTick = 5;
    public float baseBuildMultiplier = 1f;

    [Header("Combat (for military later)")]
    public float baseDamageMultiplier = 1f;

    [Header("Skills")]
    public SkillProgress mining;
    public SkillProgress building;
    public SkillProgress farming;
    public SkillProgress hauling;
    public SkillProgress combat;
    public SkillProgress driving;

    [Header("Equipment")]
    public ToolItem equippedTool;

    public float MoveSpeed
    {
        get
        {
            float v = baseMoveSpeed;
            if (equippedTool != null) v += equippedTool.moveSpeedBonus;
            return Mathf.Max(0.1f, v);
        }
    }

    public int CarryCapacity
    {
        get
        {
            int v = baseCarryCapacity;
            if (equippedTool != null) v += equippedTool.carryCapacityBonus;
            return Mathf.Max(1, v);
        }
    }

    public int HarvestPerTick
    {
        get
        {
            float mult = GetSkillMultiplier(SkillType.Mining);
            float tool = (equippedTool != null) ? equippedTool.harvestMultiplier : 1f;
            float v = baseHarvestPerTick * mult * tool;
            return Mathf.Max(1, Mathf.RoundToInt(v));
        }
    }

    public float BuildWorkMultiplier
    {
        get
        {
            float mult = GetSkillMultiplier(SkillType.Building);
            float tool = (equippedTool != null) ? equippedTool.buildMultiplier : 1f;
            return Mathf.Max(0.05f, baseBuildMultiplier * mult * tool);
        }
    }

    public float DamageMultiplier
    {
        get
        {
            float mult = GetSkillMultiplier(SkillType.Combat);
            float tool = (equippedTool != null) ? equippedTool.damageMultiplier : 1f;
            return Mathf.Max(0.05f, baseDamageMultiplier * mult * tool);
        }
    }

    public float GetSkillMultiplier(SkillType t)
    {
        SkillProgress sp = GetSkill(t);
        // Simple curve: +5% per level
        return 1f + (sp.level * 0.05f);
    }

    public void AddXP(SkillType t, float amount)
    {
        if (amount <= 0f) return;

        SkillProgress sp = GetSkill(t);
        sp.xp += amount;

        // XP curve: 10 * (level+1)^2
        while (sp.xp >= XpToNext(sp.level))
        {
            sp.xp -= XpToNext(sp.level);
            sp.level++;
        }

        SetSkill(t, sp);
    }

    float XpToNext(int level)
    {
        int n = Mathf.Max(0, level) + 1;
        return 10f * n * n;
    }

    public SkillProgress GetSkill(SkillType t)
    {
        switch (t)
        {
            case SkillType.Mining: return mining;
            case SkillType.Building: return building;
            case SkillType.Farming: return farming;
            case SkillType.Hauling: return hauling;
            case SkillType.Combat: return combat;
            case SkillType.Driving: return driving;
            default: return mining;
        }
    }

    public void SetSkill(SkillType t, SkillProgress sp)
    {
        switch (t)
        {
            case SkillType.Mining: mining = sp; break;
            case SkillType.Building: building = sp; break;
            case SkillType.Farming: farming = sp; break;
            case SkillType.Hauling: hauling = sp; break;
            case SkillType.Combat: combat = sp; break;
            case SkillType.Driving: driving = sp; break;
        }
    }
}
