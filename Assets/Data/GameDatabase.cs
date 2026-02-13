using UnityEngine;

public class GameDatabase : MonoBehaviour
{
    public static GameDatabase Instance;

    public JobsDatabase jobs;
    public ToolsDatabase tools;
    public BuildingsDatabase buildings;
    public ResourcesDatabase resources;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}