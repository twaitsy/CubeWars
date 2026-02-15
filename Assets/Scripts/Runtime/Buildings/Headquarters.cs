using UnityEngine;

public class Headquarters : Building
{
    [Header("Starting Resources")]
    [SerializeField] private ResourceAmount[] startingResources;

    protected override void Start()
    {
        base.Start();

        // Deposit starting resources for this team (assumes one HQ per team; adjust if needed)
        foreach (var amount in startingResources)
        {
            if (amount.resource != null && amount.amount > 0) // Null check since ResourceDefinition is a struct but drawer may leave it default
            {
                TeamResources.Instance.Deposit(teamID, amount.resource, amount.amount);
            }
        }
    }

    protected override void Die()
    {
        Debug.Log($"Team {teamID} Headquarters destroyed!");
        // TODO (later): trigger victory / defeat logic here
        base.Die();
    }
}