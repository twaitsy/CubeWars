using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CubeWars/Needs/NeedDefinition")]
public class NeedDefinition : ScriptableObject
{
    [Header("Identity")]
    public string id;
    public string displayName;
    public Sprite icon;

    [Header("Core Values")]
    public float maxValue = 100f;
    public float decayRatePerSecond = 1f;
    public float recoveryRatePerSecond = 0f;

    [Tooltip("When the civilian starts seeking to satisfy this need (0–1).")]
    public float seekThreshold01 = 0.5f;

    [Tooltip("When negative effects begin (0–1).")]
    public float criticalThreshold01 = 0.1f;

    [Tooltip("When the need is considered fully satisfied (0–1).")]
    public float satisfiedThreshold01 = 0.9f;

    [Header("Curves")]
    public AnimationCurve decayCurve = AnimationCurve.Linear(0, 1, 1, 1);
    public AnimationCurve recoveryCurve = AnimationCurve.Linear(0, 1, 1, 1);

    [Header("Effects on Civilian")]
    public float workSpeedMultiplier = 1f;
    public float movementSpeedMultiplier = 1f;
    public float combatEffectivenessMultiplier = 1f;
    public float moraleMultiplier = 1f;
    public float healthRegenMultiplier = 1f;
    public float panicChanceMultiplier = 1f;

    [Header("Satisfaction Sources")]
    public List<BuildingDefinition> satisfyingBuildings = new();
    public List<ResourceDefinition> satisfyingItems = new();
    public List<FoodDefinition> satisfyingFood = new();
    public List<string> satisfyingSocialActions = new();

    [Header("Environmental Modifiers")]
    public float heatPenaltyMultiplier = 1f;
    public float coldPenaltyMultiplier = 1f;
    public float weatherPenaltyMultiplier = 1f;

    [Header("AI Behaviour")]
    public float aiPriority = 1f;
    public float aiMaxTravelDistance = 20f;
    public float aiCooldownSeconds = 3f;

    [Header("Custom Logic")]
    public NeedBehaviour behaviour;
}