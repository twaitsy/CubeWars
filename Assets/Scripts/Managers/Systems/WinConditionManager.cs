using UnityEngine;

public class WinConditionManager : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float checkInterval = 1f;

    private float checkTimer;
    private bool hasDeclaredWinner;

    void Update()
    {
        if (hasDeclaredWinner)
            return;

        checkTimer -= Time.unscaledDeltaTime;
        if (checkTimer > 0f)
            return;

        checkTimer = checkInterval;
        CheckVictory();
    }

    void CheckVictory()
    {
        int aliveTeams = 0;
        int lastTeam = -1;

        var headquarters = GameObject.FindObjectsOfType<Headquarters>();
        for (int i = 0; i < headquarters.Length; i++)
        {
            var hq = headquarters[i];
            if (!hq.IsAlive)
                continue;

            aliveTeams++;
            lastTeam = hq.teamID;

            if (aliveTeams > 1)
                return;
        }

        if (aliveTeams <= 1)
            DeclareWinner(lastTeam);
    }

    void DeclareWinner(int teamID)
    {
        if (hasDeclaredWinner)
            return;

        hasDeclaredWinner = true;
        Debug.Log($"TEAM {teamID} WINS!");
        Time.timeScale = 0f;
    }
}
