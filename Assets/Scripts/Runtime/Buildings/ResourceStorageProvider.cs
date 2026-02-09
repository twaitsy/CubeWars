using UnityEngine;

public class ResourceStorageProvider : MonoBehaviour
{
    public int teamID;

    [Header("Capacities provided by this building")]
    public ResourceCapacityEntry[] capacities;

    private bool started;
    private bool registered;

    // ---------------------------------------------------------
    // DEPENDENCIES:
    // - TeamStorageManager: must implement AddCapacity(teamID, type, amount)
    //                       and RemoveCapacity(teamID, type, amount)
    // - ResourceStorageContainer: actual storage objects
    // - Building: teamID must be set before Start()
    // ---------------------------------------------------------

    void Start()
    {
        started = true;
        Register();
    }

    void OnEnable()
    {
        if (started) Register();
    }

    void OnDisable()
    {
        if (registered) Unregister();
    }

    void Register()
    {
        if (registered) return;
        if (TeamStorageManager.Instance == null) return;

        if (capacities != null)
        {
            for (int i = 0; i < capacities.Length; i++)
            {
                int cap = Mathf.Max(0, capacities[i].capacity);
                if (cap > 0)
                    TeamStorageManager.Instance.AddCapacity(teamID, capacities[i].type, cap);
            }
        }

        registered = true;
    }

    void Unregister()
    {
        if (TeamStorageManager.Instance == null) return;

        if (capacities != null)
        {
            for (int i = 0; i < capacities.Length; i++)
            {
                int cap = Mathf.Max(0, capacities[i].capacity);
                if (cap > 0)
                    TeamStorageManager.Instance.RemoveCapacity(teamID, capacities[i].type, cap);
            }
        }

        registered = false;
    }

    public void SetTeamID(int newTeamID)
    {
        if (teamID == newTeamID) return;
        teamID = newTeamID;
        RefreshRegistration();
    }

    public void RefreshRegistration()
    {
        if (!started) return;

        if (registered)
            Unregister();

        Register();
    }
}