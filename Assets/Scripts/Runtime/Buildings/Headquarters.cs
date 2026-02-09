using UnityEngine;

public class Headquarters : Building
{
    protected override void Die()
    {
        Debug.Log($"Team {teamID} Headquarters destroyed!");

        // TODO (later): trigger victory / defeat logic here

        base.Die();
    }
}
