using UnityEngine;

public class WinConditionManager : MonoBehaviour
{
    void Update()
    {
        CheckVictory();
    }

    void CheckVictory()
    {
        int aliveTeams = 0;
        int lastTeam = -1;

        foreach (var hq in GameObject.FindObjectsOfType<Headquarters>())
        {
            if (!hq.IsAlive) continue;
            aliveTeams++;
            lastTeam = hq.teamID;
        }

        if (aliveTeams <= 1)
            DeclareWinner(lastTeam);
    }

    void DeclareWinner(int teamID)
    {
        Debug.Log($"TEAM {teamID} WINS!");
        Time.timeScale = 0f;
    }
}
