using System;
using UnityEngine;

[Serializable]
public struct SkillProgress
{
    public int level;
    public float xp;
}

[Serializable]
public enum CharacterSkillType
{
    Mining,
    Building,
    Farming,
    Hauling,
    Combat,
    Driving
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
            float mult = GetSkillMultiplier(CharacterSkillType.Mining);
            float tool = (equippedTool != null) ? equippedTool.harvestMultiplier : 1f;
            float v = baseHarvestPerTick * mult * tool;
            return Mathf.Max(1, Mathf.RoundToInt(v));
        }
    }

    public float BuildWorkMultiplier
    {
        get
        {
            float mult = GetSkillMultiplier(CharacterSkillType.Building);
            float tool = (equippedTool != null) ? equippedTool.buildMultiplier : 1f;
            return Mathf.Max(0.05f, baseBuildMultiplier * mult * tool);
        }
    }

    public float DamageMultiplier
    {
        get
        {
            float mult = GetSkillMultiplier(CharacterSkillType.Combat);
            float tool = (equippedTool != null) ? equippedTool.damageMultiplier : 1f;
            return Mathf.Max(0.05f, baseDamageMultiplier * mult * tool);
        }
    }

    public float GetSkillMultiplier(CharacterSkillType t)
    {
        SkillProgress sp = GetSkill(t);
        // Simple curve: +5% per level
        return 1f + (sp.level * 0.05f);
    }

    public void AddXP(CharacterSkillType t, float amount)
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

    public SkillProgress GetSkill(CharacterSkillType t)
    {
        switch (t)
        {
            case CharacterSkillType.Mining: return mining;
            case CharacterSkillType.Building: return building;
            case CharacterSkillType.Farming: return farming;
            case CharacterSkillType.Hauling: return hauling;
            case CharacterSkillType.Combat: return combat;
            case CharacterSkillType.Driving: return driving;
            default: return mining;
        }
    }

    public void SetSkill(CharacterSkillType t, SkillProgress sp)
    {
        switch (t)
        {
            case CharacterSkillType.Mining: mining = sp; break;
            case CharacterSkillType.Building: building = sp; break;
            case CharacterSkillType.Farming: farming = sp; break;
            case CharacterSkillType.Hauling: hauling = sp; break;
            case CharacterSkillType.Combat: combat = sp; break;
            case CharacterSkillType.Driving: driving = sp; break;
        }
    }
}
