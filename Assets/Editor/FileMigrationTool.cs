using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class FileMigrationTool : EditorWindow
{
    private static bool dryRun = false; // Set to false to actually move files

    [MenuItem("Tools/CubeWars/Run File Migration")]
    public static void RunMigration()
    {
        Debug.Log("=== CubeWars File Migration Started ===");
        Debug.Log(dryRun ? "Running in DRY RUN mode (no files will be moved)" : "Running in LIVE mode (files will be moved)");

        var fileMap = BuildFileMap();

        foreach (var entry in fileMap)
        {
            MoveFile(entry.Key, entry.Value);
        }

        Debug.Log("=== CubeWars File Migration Complete ===");
    }

    private static Dictionary<string, string> BuildFileMap()
    {
        return new Dictionary<string, string>
        {
            // --- Core ---
            { "Dependencies.cs", "Assets/Scripts/Core/Services" },
            { "GameBootstrap.cs", "Assets/Scripts/Core/GameLoop" },
            { "HQSpawner.cs", "Assets/Scripts/Core/Scene" },
            { "NewBehaviourScript.cs", "Assets/Scripts/Core" },

            // --- AI ---
            { "AIBuilder.cs", "Assets/Scripts/Systems/AI/DecisionMaking" },
            { "AIBuildingPriority.cs", "Assets/Scripts/Systems/AI/DecisionMaking" },
            { "AIDifficulty.cs", "Assets/Scripts/Data/AI" },
            { "AIEconomy.cs", "Assets/Scripts/Systems/AI/DecisionMaking" },
            { "AIManager.cs", "Assets/Scripts/Managers/Systems" },
            { "AIMilitary.cs", "Assets/Scripts/Systems/AI/DecisionMaking" },
            { "AIPersonality.cs", "Assets/Scripts/Data/AI" },
            { "AIPlayer.cs", "Assets/Scripts/Systems/AI/DecisionMaking" },
            { "AIRebuildManager.cs", "Assets/Scripts/Systems/AI/Jobs" },
            { "AIRepairManager.cs", "Assets/Scripts/Systems/AI/Jobs" },
            { "AIResourceManager.cs", "Assets/Scripts/Systems/AI/Jobs" },
            { "AIThreatDetector.cs", "Assets/Scripts/Systems/AI/Targeting" },
            { "MultiTeamAIDirector.cs", "Assets/Scripts/Systems/AI/DecisionMaking" },
            { "Team.cs", "Assets/Scripts/Data/AI" },
            { "TeamAIBuild.cs", "Assets/Scripts/Systems/AI/DecisionMaking" },
            { "TeamColorManager.cs", "Assets/Scripts/Managers/Systems" },
            { "TeamColorUtils.cs", "Assets/Scripts/Utilities/Helpers" },
            { "TeamInventory.cs", "Assets/Scripts/Data/AI" },
            { "TeamResources.cs", "Assets/Scripts/Data/Resources" },
            { "TeamStorageManager.cs", "Assets/Scripts/Managers/Systems" },
            { "TeamVisual.cs", "Assets/Scripts/Runtime/Units" },

            // --- Buildings ---
            { "Barracks.cs", "Assets/Scripts/Runtime/Buildings" },
            { "BuildCatalog.cs", "Assets/Scripts/Data/Buildings" },
            { "BuildCellReservation.cs", "Assets/Scripts/Systems/Buildings" },
            { "BuildGridCell.cs", "Assets/Scripts/Systems/Grid" },
            { "BuildGridManager.cs", "Assets/Scripts/Systems/Grid" },
            { "Building.cs", "Assets/Scripts/Runtime/Buildings" },
            { "BuildItemDefinition.cs", "Assets/Scripts/Data/Buildings" },
            { "BuildItemInstance.cs", "Assets/Scripts/Runtime/Buildings" },
            { "BuildMenuUI.cs", "Assets/Scripts/Systems/UI" },
            { "BuildPlacementManager.cs", "Assets/Scripts/Systems/Buildings" },
            { "BuildTimeSettings.cs", "Assets/Scripts/Data/Buildings" },
            { "ConstructionSite.cs", "Assets/Scripts/Runtime/Buildings" },
            { "CraftingSystem.cs", "Assets/Scripts/Systems/Economy" },
            { "DefenseTurret.cs", "Assets/Scripts/Runtime/Buildings" },
            { "Farm.cs", "Assets/Scripts/Runtime/Buildings" },
            { "Headquarters.cs", "Assets/Scripts/Runtime/Buildings" },
            { "ResourceCapacityEntry.cs", "Assets/Scripts/Data/Resources" },
            { "ResourceCost.cs", "Assets/Scripts/Data/Resources" },
            { "ResourceDropoff.cs", "Assets/Scripts/Runtime/Buildings" },
            { "ResourceNode.cs", "Assets/Scripts/Runtime/Buildings" },
            { "ResourceRegistry.cs", "Assets/Scripts/Systems/Economy" },
            { "ResourceSpawner.cs", "Assets/Scripts/Runtime/Buildings" },
            { "ResourceStorageContainer.cs", "Assets/Scripts/Runtime/Buildings" },
            { "ResourceStorageProvider.cs", "Assets/Scripts/Runtime/Buildings" },
            { "ResourceVisualBuilder.cs", "Assets/Scripts/Systems/UI" },
            { "VehicleFactory.cs", "Assets/Scripts/Runtime/Buildings" },
            { "WeaponsFactory.cs", "Assets/Scripts/Runtime/Buildings" },

            // --- Catalogs ---
            { "ToolItem.cs", "Assets/Scripts/Data/Buildings" },

            // --- Civilians ---
            { "Civilian.cs", "Assets/Scripts/Runtime/Units" },
            { "CivilianRole.cs", "Assets/Scripts/Data/Units" },
            { "CivilianSpawner.cs", "Assets/Scripts/Systems/Units" },
            { "JobManager.cs", "Assets/Scripts/Systems/Civilians" },
            { "SkillType.cs", "Assets/Scripts/Data/Units" },

            // --- Combat ---
            { "Attackable.cs", "Assets/Scripts/Runtime/Units" },
            { "AutoDestroyFX.cs", "Assets/Scripts/Runtime/Effects" },
            { "CombatManager.cs", "Assets/Scripts/Managers/Systems" },
            { "CombatStance.cs", "Assets/Scripts/Data/Units" },
            { "DiplomacyManager.cs", "Assets/Scripts/Managers/Systems" },
            { "IAttackable.cs", "Assets/Scripts/Systems/Combat" },
            { "ICommandable.cs", "Assets/Scripts/Systems/Combat" },
            { "IHasHealth.cs", "Assets/Scripts/Systems/Combat" },
            { "ITargetable.cs", "Assets/Scripts/Systems/Combat" },
            { "Projectile.cs", "Assets/Scripts/Runtime/Projectiles" },
            { "ProjectilePool.cs", "Assets/Scripts/Systems/Combat" },
            { "SkirmishConfig.cs", "Assets/Scripts/Data/Combat" },
            { "Turret.cs", "Assets/Scripts/Runtime/Buildings" },
            { "TurretProjectile.cs", "Assets/Scripts/Runtime/Projectiles" },
            { "TurretProjectilePool.cs", "Assets/Scripts/Systems/Combat" },
            { "Unit.cs", "Assets/Scripts/Runtime/Units" },
            { "UnitCombatController.cs", "Assets/Scripts/Systems/Units" },
            { "UnitCommandController.cs", "Assets/Scripts/Systems/Units" },
            { "UnitManager.cs", "Assets/Scripts/Managers/Systems" },
            { "UnitProductionDefinition.cs", "Assets/Scripts/Data/Units" },
            { "UnitProductionQueue.cs", "Assets/Scripts/Systems/Units" },
            { "WeaponComponent.cs", "Assets/Scripts/Systems/Combat" },

            // --- Core ---
            { "AlertManager.cs", "Assets/Scripts/Managers/Core" },
            { "CharacterStats.cs", "Assets/Scripts/Data/Units" },
            { "EventManager.cs", "Assets/Scripts/Core/Events" },
            { "GameManager.cs", "Assets/Scripts/Managers/Core" },
            { "ParentRenamer.cs", "Assets/Scripts/Utilities/Helpers" },
            { "ResourceType.cs", "Assets/Scripts/Data/Resources" },
            { "RTSCamera.cs", "Assets/Scripts/Core/Scene" },
            { "SaveLoadManager.cs", "Assets/Scripts/Managers/Core" },
            { "SciFiMapBootstrap.cs", "Assets/Scripts/Core/Scene" },
            { "SciFiTeamStyler.cs", "Assets/Scripts/Systems/UI" },
            { "Selectable.cs", "Assets/Scripts/Systems/Units" },
            { "SelfRenamer.cs", "Assets/Scripts/Utilities/Helpers" },
            { "SixTeamBootstrap.cs", "Assets/Scripts/Core/Scene" },
            { "Spawner.cs", "Assets/Scripts/Core/Scene" },
            { "WinConditionManager.cs", "Assets/Scripts/Managers/Systems" },

            // --- Debug ---
            { "FindTeamDuplicates.cs", "Assets/Scripts/Utilities/Debug" },
            { "TeamWatchdog.cs", "Assets/Scripts/Utilities/Debug" },

            // --- Documents ---
            { "CombatSystem.cs", "Assets/Scripts/Documents" },

            // --- UI ---
            { "ConstructionManager.cs", "Assets/Scripts/Systems/UI" },
            { "EconomyUI.cs", "Assets/Scripts/Systems/UI" },
            { "IMGUIInputBlocker.cs", "Assets/Scripts/Systems/UI" },
            { "MainMenuUI.cs", "Assets/Scripts/Systems/UI" },
            { "MapVisualsBootstrap.cs", "Assets/Scripts/Systems/UI" },
            { "Minimap.cs", "Assets/Scripts/Systems/UI" },
            { "NeonRing.cs", "Assets/Scripts/Systems/UI" },
            { "SelectionManager.cs", "Assets/Scripts/Systems/UI" },
            { "SelectionRing.cs", "Assets/Scripts/Systems/UI" },
            { "TaskBoardUI.cs", "Assets/Scripts/Systems/UI" },
            { "TeamBootstrap.cs", "Assets/Scripts/Systems/UI" },
            { "UnitInspectorUI.cs", "Assets/Scripts/Systems/UI" },
            { "WorldHealthBar.cs", "Assets/Scripts/Systems/UI" },
        };
    }

    private static void MoveFile(string fileName, string destinationFolder)
    {
        EnsureFolder(destinationFolder);

        string[] guids = AssetDatabase.FindAssets(Path.GetFileNameWithoutExtension(fileName));

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            if (Path.GetFileName(path) == fileName)
            {
                string destPath = $"{destinationFolder}/{fileName}";

                if (dryRun)
                {
                    Debug.Log($"[DRY RUN] Would move: {path} → {destPath}");
                }
                else
                {
                    string result = AssetDatabase.MoveAsset(path, destPath);
                    if (!string.IsNullOrEmpty(result))
                        Debug.LogError($"Failed to move {fileName}: {result}");
                    else
                        Debug.Log($"Moved: {fileName} → {destinationFolder}");
                }

                return;
            }
        }

        Debug.LogWarning($"File not found: {fileName}");
    }

    private static void EnsureFolder(string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath))
            return;

        string[] parts = folderPath.Split('/');
        string current = parts[0];

        for (int i = 1; i < parts.Length; i++)
        {
            string next = $"{current}/{parts[i]}";
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[i]);
            }
            current = next;
        }
    }
}