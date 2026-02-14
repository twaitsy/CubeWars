using UnityEngine;

public class GameDatabaseLoader : MonoBehaviour
{
    public static GameDatabase Loaded;   // The ScriptableObject reference

    [Header("Assign the GameDatabase asset here")]
    public GameDatabase database;

    private void Awake()
    {
        Loaded = database;
    }
}