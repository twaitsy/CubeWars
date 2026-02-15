using UnityEngine;

public class AIThreatDetector : MonoBehaviour
{
    public int teamID;
    public float threatRadius = 18f;

    public Attackable DetectThreatNear(Vector3 pos)
    {
        Attackable[] all = GameObject.FindObjectsOfType<Attackable>();
        float threatRadiusSqr = threatRadius * threatRadius;
        DiplomacyManager diplomacy = DiplomacyManager.Instance;

        foreach (var a in all)
        {
            if (!a.IsAlive) continue;
            if (a.teamID == teamID) continue;
            if (diplomacy == null || !diplomacy.AreAtWar(teamID, a.teamID)) continue;

            float sqrDistance = (a.transform.position - pos).sqrMagnitude;
            if (sqrDistance <= threatRadiusSqr)
                return a;
        }

        return null;
    }
}
