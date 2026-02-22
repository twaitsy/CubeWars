using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BuildingInteractionPointController))]

public class Headquarters : Building
{
    [Header("Starting Resources")]
    [SerializeField] private ResourceAmount[] startingResources;

    protected override void Start()
    {
        base.Start();
        StartCoroutine(ApplyStartingResourcesWhenReady());
    }

    IEnumerator ApplyStartingResourcesWhenReady()
    {
        if (startingResources == null || startingResources.Length == 0)
            yield break;

        var remaining = new int[startingResources.Length];
        for (int i = 0; i < startingResources.Length; i++)
            remaining[i] = Mathf.Max(0, startingResources[i].amount);

        int maxFrames = 600;
        int frame = 0;

        while (frame < maxFrames)
        {
            bool anyPending = false;

            if (TeamResources.Instance != null)
            {
                for (int i = 0; i < startingResources.Length; i++)
                {
                    var resource = startingResources[i].resource;
                    if (resource == null || remaining[i] <= 0)
                        continue;

                    int accepted = TeamResources.Instance.Deposit(teamID, resource, remaining[i]);
                    remaining[i] = Mathf.Max(0, remaining[i] - accepted);
                    anyPending |= remaining[i] > 0;
                }

                if (!anyPending)
                    yield break;
            }
            else
            {
                anyPending = true;
            }

            frame++;
            yield return null;
        }

        for (int i = 0; i < startingResources.Length; i++)
        {
            if (startingResources[i].resource == null || remaining[i] <= 0)
                continue;

       //     Debug.LogWarning($"Headquarters team {teamID} could not apply full starting resources for {startingResources[i].resource.displayName}. Remaining: {remaining[i]}", this);
        }
    }

    protected override void Die()
    {
        Debug.Log($"Team {teamID} Headquarters destroyed!");
        // TODO (later): trigger victory / defeat logic here
        base.Die();
    }
}
