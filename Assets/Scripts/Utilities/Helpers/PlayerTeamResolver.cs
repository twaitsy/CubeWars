using UnityEngine;

public static class PlayerTeamResolver
{
    private static int lastFrameResolved = -1;
    private static bool lastResolved;
    private static int lastTeamID;

    public static bool TryGetPlayerTeamID(out int teamID)
    {
        if (Time.frameCount == lastFrameResolved)
        {
            teamID = lastTeamID;
            return lastResolved;
        }

        lastFrameResolved = Time.frameCount;
        lastResolved = false;
        lastTeamID = 0;

        var gm = Object.FindObjectOfType<GameManager>();
        if (gm == null)
        {
            teamID = 0;
            return false;
        }

        if (gm.playerTeam == null)
        {
            Team[] teams = Object.FindObjectsOfType<Team>();
            for (int i = 0; i < teams.Length; i++)
            {
                if (teams[i] != null && teams[i].teamType == TeamType.Player)
                {
                    gm.playerTeam = teams[i];
                    break;
                }
            }
        }

        if (gm.playerTeam == null)
        {
            teamID = 0;
            return false;
        }

        lastTeamID = gm.playerTeam.teamID;
        lastResolved = true;
        teamID = lastTeamID;
        return true;
    }
}
