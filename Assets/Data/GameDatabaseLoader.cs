using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class GameDatabaseLoader : MonoBehaviour
{
    public static GameDatabase Loaded;   // The ScriptableObject reference

    [Header("Assign the GameDatabase asset here")]
    public GameDatabase database;

    void OnEnable()
    {
        AssignLoaded();
    }

    void Awake()
    {
        AssignLoaded();
    }

    public static GameDatabase ResolveLoaded()
    {
        if (Loaded != null)
            return Loaded;

        GameDatabaseLoader[] loaders = Resources.FindObjectsOfTypeAll<GameDatabaseLoader>();
        for (int i = 0; i < loaders.Length; i++)
        {
            GameDatabaseLoader loader = loaders[i];
            if (loader == null || loader.database == null)
                continue;

            Loaded = loader.database;
            return Loaded;
        }

        return null;
    }

    void AssignLoaded()
    {
        if (database == null)
            return;

        Loaded = database;
    }
}
