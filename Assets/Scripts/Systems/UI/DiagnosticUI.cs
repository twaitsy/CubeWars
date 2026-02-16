using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class DiagnosticUI : MonoBehaviour
{
    [Header("Toggle")]
    public bool show;
    public KeyCode toggleKey = KeyCode.D;

    [Header("Layout")]
    public int margin = 10;
    public int headerHeight = 32;
    public int fontSizeBoost = 1;

    [Header("Sections")]
    public bool showDatabases = true;
    public bool showResourcesAndRecipes = true;
    public bool showJobsAndWorkers = true;
    public bool showSceneSystems = true;

    Vector2 scroll;
    readonly StringBuilder line = new(256);

    void Update()
    {
        if (!Input.GetKeyDown(toggleKey))
            return;
        bool next = !show;
        if (next)
            CloseOtherUi();
        show = next;
    }

    void CloseOtherUi()
    {
        MainMenuUI[] mainMenus = FindObjectsByType<MainMenuUI>(FindObjectsSortMode.None);
        for (int i = 0; i < mainMenus.Length; i++)
            mainMenus[i].show = false;
        BuildMenuUI[] buildMenus = FindObjectsByType<BuildMenuUI>(FindObjectsSortMode.None);
        for (int i = 0; i < buildMenus.Length; i++)
            buildMenus[i].SetVisible(false);
        TaskBoardUI[] taskBoards = FindObjectsByType<TaskBoardUI>(FindObjectsSortMode.None);
        for (int i = 0; i < taskBoards.Length; i++)
            taskBoards[i].show = false;
        UnitInspectorUI[] inspectors = FindObjectsByType<UnitInspectorUI>(FindObjectsSortMode.None);
        for (int i = 0; i < inspectors.Length; i++)
            inspectors[i].show = false;
        Minimap[] minimaps = FindObjectsByType<Minimap>(FindObjectsSortMode.None);
        for (int i = 0; i < minimaps.Length; i++)
            minimaps[i].show = false;
    }

    void OnGUI()
    {
        if (!show)
            return;

        Rect panel = new(margin, margin, Screen.width - margin * 2f, Screen.height - margin * 2f);
        IMGUIInputBlocker.Register(panel);

        int oldLabel = GUI.skin.label.fontSize;
        int oldBox = GUI.skin.box.fontSize;
        int oldButton = GUI.skin.button.fontSize;
        GUI.skin.label.fontSize = Mathf.Max(10, oldLabel + fontSizeBoost);
        GUI.skin.box.fontSize = Mathf.Max(10, oldBox + fontSizeBoost);
        GUI.skin.button.fontSize = Mathf.Max(10, oldButton + fontSizeBoost);

        GUILayout.BeginArea(panel, GUI.skin.window);
        DrawHeader();
        scroll = GUILayout.BeginScrollView(scroll);

        DrawDatabases();
        DrawResourcesAndRecipes();
        DrawJobsAndWorkers();
        DrawSceneSystems();

        GUILayout.EndScrollView();
        GUILayout.EndArea();

        GUI.skin.label.fontSize = oldLabel;
        GUI.skin.box.fontSize = oldBox;
        GUI.skin.button.fontSize = oldButton;
    }

    void DrawHeader()
    {
        GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.Height(headerHeight));
        GUILayout.Label("DIAGNOSTICS | Press D to close", GUILayout.ExpandWidth(true));
        if (GUILayout.Button("Close", GUILayout.Width(90)))
            show = false;
        GUILayout.EndHorizontal();
    }

    void DrawDatabases()
    {
        string headerText = (showDatabases ? "▼ " : "▶ ") + "Databases";
        if (GUILayout.Button(headerText, GUI.skin.box))
            showDatabases = !showDatabases;

        if (showDatabases)
        {
            GameDatabase loaded = GameDatabaseLoader.ResolveLoaded();
            if (loaded == null)
            {
                GUILayout.Label("- GameDatabase: NOT LOADED");
            }
            else
            {
                GUILayout.Label($"- GameDatabase: {loaded.name}");
                DrawSubDatabase("Jobs", loaded.jobs, loaded.jobs != null ? loaded.jobs.jobs.Count : 0);
                DrawSubDatabase("Tools", loaded.tools, loaded.tools != null ? loaded.tools.tools.Count : 0);
                DrawSubDatabase("Buildings", loaded.buildings, loaded.buildings != null ? loaded.buildings.buildings.Count : 0);
                DrawSubDatabase("Resources", loaded.resources, loaded.resources != null ? loaded.resources.resources.Count : 0);
                DrawSubDatabase("Units", loaded.units, loaded.units != null ? loaded.units.units.Count : 0);
                DrawSubDatabase("Recipes", loaded.recipes, loaded.recipes != null ? loaded.recipes.recipes.Count : 0);
                DrawSubDatabase("Foods", loaded.foods, loaded.foods != null ? loaded.foods.foods.Count : 0);
                DrawSubDatabase("TechTree", loaded.techTree, loaded.techTree != null ? loaded.techTree.techNodes.Count : 0);

                if (loaded.resources != null && loaded.resources.resources != null)
                {
                    GUILayout.Label(" Resources list:");
                    for (int i = 0; i < loaded.resources.resources.Count; i++)
                    {
                        ResourceDefinition resource = loaded.resources.resources[i];
                        if (resource == null)
                            continue;
                        GUILayout.Label($" • {resource.displayName} [{resource.id}] ({resource.category})");
                    }
                }

                if (loaded.recipes != null && loaded.recipes.recipes != null)
                {
                    GUILayout.Label(" Recipes list:");
                    for (int i = 0; i < loaded.recipes.recipes.Count; i++)
                    {
                        ProductionRecipeDefinition recipe = loaded.recipes.recipes[i];
                        if (recipe == null)
                            continue;
                        line.Clear();
                        line.Append(" • ").Append(recipe.recipeName).Append(" | inputs: ");
                        if (recipe.inputs != null && recipe.inputs.Length > 0)
                        {
                            for (int inputIndex = 0; inputIndex < recipe.inputs.Length; inputIndex++)
                            {
                                if (inputIndex > 0)
                                    line.Append(", ");
                                RecipeResourceAmount input = recipe.inputs[inputIndex];
                                string inputName = input != null && input.resource != null ? input.resource.id : "<none>";
                                line.Append(inputName).Append(" x").Append(input != null ? input.amount : 0);
                            }
                        }
                        else
                        {
                            line.Append("none");
                        }
                        line.Append(" | outputs: ");
                        if (recipe.outputs != null && recipe.outputs.Length > 0)
                        {
                            for (int outputIndex = 0; outputIndex < recipe.outputs.Length; outputIndex++)
                            {
                                if (outputIndex > 0)
                                    line.Append(", ");
                                RecipeResourceAmount output = recipe.outputs[outputIndex];
                                string outputName = output != null && output.resource != null ? output.resource.id : "<none>";
                                line.Append(outputName).Append(" x").Append(output != null ? output.amount : 0);
                            }
                        }
                        else
                        {
                            line.Append("none");
                        }
                        GUILayout.Label(line.ToString());
                    }
                }
            }
        }
    }

    void DrawSubDatabase(string label, UnityEngine.Object asset, int count)
    {
        string status = asset == null ? "MISSING" : asset.name;
        GUILayout.Label($" - {label}: {status} | entries: {count}");
    }

    void DrawResourcesAndRecipes()
    {
        string headerText = (showResourcesAndRecipes ? "▼ " : "▶ ") + "Runtime Resource + Production State";
        if (GUILayout.Button(headerText, GUI.skin.box))
            showResourcesAndRecipes = !showResourcesAndRecipes;

        if (showResourcesAndRecipes)
        {
            ResourceNode[] nodes = FindObjectsByType<ResourceNode>(FindObjectsSortMode.None);
            GUILayout.Label($"- Resource Nodes in Scene: {nodes.Length}");
            for (int i = 0; i < nodes.Length; i++)
            {
                ResourceNode node = nodes[i];
                if (node == null)
                    continue;
                string resourceName = node.resource != null ? node.resource.displayName : "<none>";
                GUILayout.Label($" • {node.name} | {resourceName} | remaining: {node.remaining}/{node.amount} | gatherers: {node.ActiveGatherers}/{Mathf.Max(1, node.maxGatherers)}");
            }

            CraftingBuilding[] buildings = FindObjectsByType<CraftingBuilding>(FindObjectsSortMode.None);
            GUILayout.Label($"- Crafting Buildings in Scene: {buildings.Length}");
            for (int i = 0; i < buildings.Length; i++)
            {
                CraftingBuilding building = buildings[i];
                if (building == null)
                    continue;
                string recipeId = building.recipe != null ? building.recipe.recipeName : "<none>";
                GUILayout.Label($" • {building.name} | team {building.teamID} | recipe: {recipeId} | progress: {building.CraftProgress01:0.00}");
            }
        }
    }

    void DrawJobsAndWorkers()
    {
        string headerText = (showJobsAndWorkers ? "▼ " : "▶ ") + "Jobs + Workers";
        if (GUILayout.Button(headerText, GUI.skin.box))
            showJobsAndWorkers = !showJobsAndWorkers;

        if (showJobsAndWorkers)
        {
            CivilianJobType[] jobTypes = (CivilianJobType[])Enum.GetValues(typeof(CivilianJobType));
            GUILayout.Label($"- Job Types ({jobTypes.Length}): {string.Join(", ", jobTypes.Select(j => j.ToString()).ToArray())}");

            JobsDatabase jobs = GameDatabaseLoader.ResolveLoaded() != null ? GameDatabaseLoader.ResolveLoaded().jobs : null;
            if (jobs != null && jobs.jobs != null)
            {
                GUILayout.Label($"- Available Jobs ({jobs.jobs.Count})");
                for (int i = 0; i < jobs.jobs.Count; i++)
                {
                    JobDefinition job = jobs.jobs[i];
                    if (job == null)
                        continue;
                    GUILayout.Label($" • {job.id} | {job.displayName} | default type: {job.defaultJobType} | legacy role: {job.legacyRole}");
                }
            }

            Civilian[] workers = FindObjectsByType<Civilian>(FindObjectsSortMode.None);
            GUILayout.Label($"- Workers in Scene: {workers.Length}");
            var waitingWorkers = new List<Civilian>();
            var byState = new Dictionary<string, int>();
            for (int i = 0; i < workers.Length; i++)
            {
                Civilian worker = workers[i];
                if (worker == null)
                    continue;
                string state = worker.CurrentState;
                if (!byState.ContainsKey(state))
                    byState[state] = 0;
                byState[state]++;
                if (state == "Idle" || state.StartsWith("Searching"))
                    waitingWorkers.Add(worker);
            }
            foreach (var kvp in byState.OrderBy(k => k.Key))
                GUILayout.Label($" • State {kvp.Key}: {kvp.Value}");
            GUILayout.Label($"- Individual workers waiting for jobs: {waitingWorkers.Count}");
            for (int i = 0; i < waitingWorkers.Count; i++)
            {
                Civilian worker = waitingWorkers[i];
                GUILayout.Label($" • {worker.name} | team {worker.teamID} | role {worker.role} | job {worker.JobType}");
            }

            WorkerTaskDispatcher dispatcher = WorkerTaskDispatcher.Instance;
            if (dispatcher != null)
            {
                List<WorkerTaskRequest> queued = dispatcher.GetQueuedTasksSnapshot();
                GUILayout.Label($"- Queued Worker Tasks: {queued.Count}");
                var byTaskType = new Dictionary<WorkerTaskType, int>();
                for (int i = 0; i < queued.Count; i++)
                {
                    WorkerTaskType type = queued[i].taskType;
                    if (!byTaskType.ContainsKey(type))
                        byTaskType[type] = 0;
                    byTaskType[type]++;
                }
                foreach (var kvp in byTaskType.OrderBy(k => k.Key.ToString()))
                    GUILayout.Label($" • {kvp.Key}: {kvp.Value}");
                for (int i = 0; i < queued.Count; i++)
                {
                    WorkerTaskRequest task = queued[i];
                    GUILayout.Label($" • Task[{i}] type={task.taskType} team={task.teamID} target={ResolveTaskTarget(task)}");
                }
            }
        }
    }

    void DrawSceneSystems()
    {
        string headerText = (showSceneSystems ? "▼ " : "▶ ") + "Scene Systems Snapshot";
        if (GUILayout.Button(headerText, GUI.skin.box))
            showSceneSystems = !showSceneSystems;

        if (showSceneSystems)
        {
            JobManager jm = JobManager.Instance;
            if (jm != null)
            {
                Civilian[] workers = FindObjectsByType<Civilian>(FindObjectsSortMode.None);
                var teams = workers.Select(w => w.teamID).Distinct().OrderBy(id => id).ToArray();
                for (int i = 0; i < teams.Length; i++)
                {
                    int teamId = teams[i];
                    Dictionary<CivilianRole, int> counts = jm.GetRoleCounts(teamId);
                    string parts = string.Join(", ", counts.Select(k => $"{k.Key}:{k.Value}").ToArray());
                    GUILayout.Label($"- Team {teamId} role counts: {parts}");
                }
            }

            TeamResources resources = TeamResources.Instance;
            if (resources != null)
                GUILayout.Label("- TeamResources: present");
            else
                GUILayout.Label("- TeamResources: missing");

            ResourceStorageContainer[] storages =   FindObjectsByType<ResourceStorageContainer>(FindObjectsSortMode.None);
            GUILayout.Label($"- Storage Containers: {storages.Length}");
            for (int i = 0; i < storages.Length; i++)
            {
                ResourceStorageContainer storage = storages[i];
                if (storage == null)
                    continue;
                GUILayout.Label($" • {storage.name} | team {storage.teamID}");
            }
        }
    }

    static string ResolveTaskTarget(WorkerTaskRequest task)
    {
        if (task.resourceNode != null)
            return task.resourceNode.name;
        if (task.constructionSite != null)
            return task.constructionSite.name;
        if (task.craftingBuilding != null)
            return task.craftingBuilding.name;
        return "none";
    }
}