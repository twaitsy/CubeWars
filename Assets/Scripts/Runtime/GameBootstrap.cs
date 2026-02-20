using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject uiPrefab;
    public GameObject cameraRigPrefab;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        // ROOTS
        GameObject worldRoot = new("World");
        GameObject systemsRoot = new("Systems");
        GameObject teamsRoot = new("Teams");

        // WORLD SUBROOTS
        new GameObject("Terrain").transform.SetParent(worldRoot.transform);
        new GameObject("ResourceNodes").transform.SetParent(worldRoot.transform);
        new GameObject("NeutralFactions").transform.SetParent(worldRoot.transform);
        new GameObject("MapBounds").transform.SetParent(worldRoot.transform);

        // TEAMS
        for (int i = 1; i <= 8; i++)
        {
            GameObject team = new($"Team_{i}");
            team.transform.SetParent(teamsRoot.transform);

            new GameObject("HQ").transform.SetParent(team.transform);
            new GameObject("Units").transform.SetParent(team.transform);
            new GameObject("Buildings").transform.SetParent(team.transform);
        }

        // SYSTEMS (created at runtime)
        CreateSystem<BuildGridManager>(systemsRoot);
        CreateSystem<BuildPlacementManager>(systemsRoot);
        CreateSystem<ConstructionManager>(systemsRoot);
        CreateSystem<TeamResources>(systemsRoot);
        CreateSystem<JobManager>(systemsRoot);
        CreateSystem<AIManager>(systemsRoot);
        CreateSystem<CombatManager>(systemsRoot);
        CreateSystem<ProjectilePool>(systemsRoot);
        CreateSystem<UnitManager>(systemsRoot);
        CreateSystem<EventManager>(systemsRoot);
        CreateSystem<WinConditionManager>(systemsRoot);

        // GAME MANAGER
        CreateSystem<GameManager>(null);

        // UI + CAMERA
        Instantiate(uiPrefab).name = "UI";
        Instantiate(cameraRigPrefab).name = "CameraRig";
    }

    void CreateSystem<T>(GameObject parent) where T : Component
    {
        GameObject obj = new(typeof(T).Name);
        obj.AddComponent<T>();
        if (parent != null)
            obj.transform.SetParent(parent.transform);
    }
}