using UnityEngine;
using UnityEditor;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

public static class GameDatabaseExporter
{
    private const string DatabasePath = "Assets/Data/Databases/GameDatabase.asset";

    [MenuItem("Tools/Cube Wars/Export Database to JSON")]
    public static void ExportDatabase()
    {
        var db = AssetDatabase.LoadAssetAtPath<GameDatabase>(DatabasePath);
        if (db == null)
        {
            Debug.LogError("GameDatabase.asset not found at: " + DatabasePath);
            return;
        }

        GameDatabaseDTO dto = BuildDto(db);

        string json = JsonConvert.SerializeObject(dto, Formatting.Indented);

        string savePath = EditorUtility.SaveFilePanel(
            "Export Cube Wars Database",
            "",
            "CubeWarsDatabase.json",
            "json"
        );

        if (string.IsNullOrEmpty(savePath))
            return;

        File.WriteAllText(savePath, json);
        Debug.Log("Cube Wars database exported to: " + savePath);
    }

    private static GameDatabaseDTO BuildDto(GameDatabase db)
    {
        var dto = new GameDatabaseDTO
        {
            jobs = BuildJobsDto(db.jobs),
            tools = BuildToolsDto(db.tools),
            buildings = BuildBuildingsDto(db.buildings),
            resources = BuildResourcesDto(db.resources),
            units = BuildUnitsDto(db.units),
            recipes = BuildRecipesDto(db.recipes),
            foods = BuildFoodsDto(db.foods),
            techTree = BuildTechTreeDto(db.techTree)
        };

        return dto;
    }

    // ---------------- RESOURCES ----------------

    private static ResourcesDatabaseDTO BuildResourcesDto(ResourcesDatabase db)
    {
        var dto = new ResourcesDatabaseDTO();
        if (db == null || db.resources == null) return dto;

        foreach (var r in db.resources)
        {
            if (r == null) continue;
            var rDto = new ResourceDefinitionDTO
            {
                id = r.id,
                displayName = r.displayName,
                category = r.category.ToString(),
                weight = r.weight,
                baseValue = r.baseValue
            };
            dto.resources.Add(rDto);
        }

        return dto;
    }

    // ---------------- BUILDINGS ----------------

    private static BuildingsDatabaseDTO BuildBuildingsDto(BuildingsDatabase db)
    {
        var dto = new BuildingsDatabaseDTO();
        if (db == null || db.buildings == null) return dto;

        foreach (var b in db.buildings)
        {
            if (b == null) continue;

            var bDto = new BuildingDefinitionDTO
            {
                id = b.id,
                displayName = b.displayName,
                category = b.category.ToString(),
                subCategory = b.subCategory,
                buildTime = b.buildTime,
                maxHealth = b.maxHealth,
                upkeepInterval = b.upkeepInterval,
                maxResidents = b.maxResidents,
                comfortLevel = b.comfortLevel,
                isProducer = b.isProducer,
                workerSlots = b.workerSlots,
                isStorage = b.isStorage,
                isMilitary = b.isMilitary,
                attackDamage = b.attackDamage,
                attackRange = b.attackRange,
                garrisonSlots = b.garrisonSlots,
                powerConsumption = b.powerConsumption,
                powerGeneration = b.powerGeneration,
                waterConsumption = b.waterConsumption,
                upgradeToBuildingId = b.upgradeTo != null ? b.upgradeTo.id : null
            };

            if (b.constructionCost != null)
            {
                foreach (var c in b.constructionCost)
                {
                    if (c == null || c.resource == null) continue;
                    bDto.constructionCost.Add(new ResourceAmountDTO
                    {
                        resourceId = c.resource.id,
                        amount = c.amount
                    });
                }
            }

            if (b.upkeepCost != null)
            {
                foreach (var u in b.upkeepCost)
                {
                    if (u == null || u.resource == null) continue;
                    bDto.upkeepCost.Add(new ResourceAmountDTO
                    {
                        resourceId = u.resource.id,
                        amount = u.amount
                    });
                }
            }

            if (b.storageSettings != null)
            {
                bDto.storageCapacity = b.storageSettings.capacity;
                if (b.storageSettings.allowedCategories != null)
                {
                    foreach (var cat in b.storageSettings.allowedCategories)
                        bDto.allowedStorageCategories.Add(cat.ToString());
                }
            }

            if (b.recipes != null)
            {
                foreach (var r in b.recipes)
                {
                    if (r == null) continue;
                    bDto.recipeIds.Add(r.recipeName);
                }
            }

            dto.buildings.Add(bDto);
        }

        return dto;
    }

    // ---------------- UNITS ----------------

    private static UnitsDatabaseDTO BuildUnitsDto(UnitsDatabase db)
    {
        var dto = new UnitsDatabaseDTO();
        if (db == null || db.units == null) return dto;

        foreach (var u in db.units)
        {
            if (u == null) continue;

            var uDto = new UnitDefinitionDTO
            {
                id = u.id,
                displayName = u.displayName,
                jobType = u.jobType.ToString(),
                isCombatUnit = u.isCombatUnit,
                maxHealth = u.maxHealth,
                moveSpeed = u.moveSpeed,
                attackDamage = u.attackDamage,
                attackRange = u.attackRange,
                attackCooldown = u.attackCooldown,
                armor = u.armor,
                carryCapacity = u.carryCapacity,
                gatherSpeed = u.gatherSpeed,
                buildSpeed = u.buildSpeed,
                trainingTime = u.trainingTime,
                trainedAtBuildingId = u.trainedAt != null ? u.trainedAt.id : null,
                upgradeToUnitId = u.upgradeTo != null ? u.upgradeTo.id : null
            };

            if (u.trainingCost != null)
            {
                foreach (var c in u.trainingCost)
                {
                    if (c == null || c.resource == null) continue;
                    uDto.trainingCost.Add(new ResourceAmountDTO
                    {
                        resourceId = c.resource.id,
                        amount = c.amount
                    });
                }
            }

            dto.units.Add(uDto);
        }

        return dto;
    }

    // ---------------- JOBS ----------------

    private static JobsDatabaseDTO BuildJobsDto(JobsDatabase db)
    {
        var dto = new JobsDatabaseDTO();
        if (db == null || db.jobs == null) return dto;

        foreach (var j in db.jobs)
        {
            if (j == null) continue;

            var jDto = new JobDefinitionDTO
            {
                id = j.id,
                displayName = j.displayName
            };

            if (j.requiredTools != null)
            {
                foreach (var t in j.requiredTools)
                {
                    if (t == null) continue;
                    jDto.requiredToolIds.Add(t.id);
                }
            }

            dto.jobs.Add(jDto);
        }

        return dto;
    }

    // ---------------- TOOLS ----------------

    private static ToolsDatabaseDTO BuildToolsDto(ToolsDatabase db)
    {
        var dto = new ToolsDatabaseDTO();
        if (db == null || db.tools == null) return dto;

        foreach (var t in db.tools)
        {
            if (t == null) continue;

            dto.tools.Add(new ToolDefinitionDTO
            {
                id = t.id,
                displayName = t.displayName,
                durability = t.durability,
                efficiency = t.efficiency
            });
        }

        return dto;
    }

    // ---------------- RECIPES ----------------

    private static RecipesDatabaseDTO BuildRecipesDto(RecipesDatabase db)
    {
        var dto = new RecipesDatabaseDTO();
        if (db == null || db.recipes == null) return dto;

        foreach (var r in db.recipes)
        {
            if (r == null) continue;

            var rDto = new ProductionRecipeDTO
            {
                id = r.recipeName,
                recipeName = r.recipeName,
                craftTimeSeconds = r.craftTimeSeconds,
                batchSize = r.batchSize,
                requiredJobType = r.requiredJobType.ToString(),
                outputEfficiencyMultiplier = r.outputEfficiencyMultiplier,
                inputEfficiencyMultiplier = r.inputEfficiencyMultiplier,
                requiresPower = r.requiresPower,
                requiresFuel = r.requiresFuel
            };

            if (r.inputs != null)
            {
                foreach (var input in r.inputs)
                {
                    if (input == null || input.resource == null) continue;
                    rDto.inputs.Add(new ResourceAmountDTO
                    {
                        resourceId = input.resource.id,
                        amount = input.amount
                    });
                }
            }

            if (r.outputs != null)
            {
                foreach (var output in r.outputs)
                {
                    if (output == null || output.resource == null) continue;
                    rDto.outputs.Add(new ResourceAmountDTO
                    {
                        resourceId = output.resource.id,
                        amount = output.amount
                    });
                }
            }

            dto.recipes.Add(rDto);
        }

        return dto;
    }

    // ---------------- FOOD ----------------

    private static FoodDatabaseDTO BuildFoodsDto(FoodDatabase db)
    {
        var dto = new FoodDatabaseDTO();
        if (db == null || db.foods == null) return dto;

        foreach (var f in db.foods)
        {
            if (f == null || f.resource == null) continue;

            dto.foods.Add(new FoodDefinitionDTO
            {
                resourceId = f.resource.id,
                hungerRestore = f.hungerRestore,
                eatTime = f.eatTime,
                requiresCooking = f.requiresCooking,
                perishable = f.perishable,
                spoilTime = f.spoilTime
            });
        }

        return dto;
    }

    // ---------------- TECH TREE ----------------

    private static TechTreeDatabaseDTO BuildTechTreeDto(TechTreeDatabase db)
    {
        var dto = new TechTreeDatabaseDTO();
        if (db == null || db.techNodes == null) return dto;

        foreach (var node in db.techNodes)
        {
            if (node == null) continue;

            var nDto = new TechNodeDTO
            {
                id = node.id,
                displayName = node.displayName,
                description = node.description,
                researchTime = node.researchTime,
                globalProductionSpeedMultiplier = node.globalProductionSpeedMultiplier,
                globalCombatDamageMultiplier = node.globalCombatDamageMultiplier,
                workerEfficiencyMultiplier = node.workerEfficiencyMultiplier
            };

            if (node.researchCost != null)
            {
                foreach (var cost in node.researchCost)
                {
                    if (cost == null || cost.resource == null) continue;
                    nDto.researchCost.Add(new ResourceAmountDTO
                    {
                        resourceId = cost.resource.id,
                        amount = cost.amount
                    });
                }
            }

            if (node.prerequisites != null)
            {
                foreach (var pre in node.prerequisites)
                {
                    if (pre == null) continue;
                    nDto.prerequisiteIds.Add(pre.id);
                }
            }

            if (node.requiredBuilding != null)
                nDto.requiredBuildingId = node.requiredBuilding.id;

            if (node.unlockBuildings != null)
            {
                foreach (var b in node.unlockBuildings)
                {
                    if (b == null) continue;
                    nDto.unlockBuildingIds.Add(b.id);
                }
            }

            if (node.unlockUnits != null)
            {
                foreach (var u in node.unlockUnits)
                {
                    if (u == null) continue;
                    nDto.unlockUnitIds.Add(u.id);
                }
            }

            if (node.unlockRecipes != null)
            {
                foreach (var r in node.unlockRecipes)
                {
                    if (r == null) continue;
                    nDto.unlockRecipeIds.Add(r.recipeName);
                }
            }

            dto.techNodes.Add(nDto);
        }

        return dto;
    }
}