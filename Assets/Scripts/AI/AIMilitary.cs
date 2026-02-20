using UnityEngine;

public class AIMilitary : MonoBehaviour
{
    public int teamID;
    public AIPersonality personality;

    [Header("Attack Rules")]
    public int attackThreshold = 4;
    public float attackRange = 80f;

    public void DefendLocation(Vector3 pos)
    {
        Unit[] units = GameObject.FindObjectsByType<Unit>(FindObjectsSortMode.None);

        foreach (var u in units)
        {
            if (u.teamID != teamID) continue;
            if (!u.combatEnabled) continue;

            u.MoveTo(pos);
            return;
        }
    }

    public void Tick()
    {
        UnitCombatController[] units = GameObject.FindObjectsByType<UnitCombatController>(FindObjectsSortMode.None);
        int myUnits = 0;

        foreach (var u in units)
            if (u.teamID == teamID)
                myUnits++;

        if (myUnits < attackThreshold)
            return;

        Attackable target = FindEnemyTarget();
        if (target == null)
        {
            Debug.Log($"[AIMilitary] Team {teamID} found no enemy target in range {attackRange}.", this);
            return;
        }

        foreach (var unit in units)
        {
            if (unit.teamID != teamID) continue;

            unit.SetManualTarget(target);

            var mover = unit.GetComponent<Unit>();
            if (mover != null)
                mover.MoveTo(target.transform.position);
        }
    }

    Attackable FindEnemyTarget()
    {
        Attackable[] all = GameObject.FindObjectsByType<Attackable>(FindObjectsSortMode.None);
        Attackable best = null;
        float bestDist = float.MaxValue;

        foreach (var a in all)
        {
            if (!a.IsAlive) continue;
            if (a.teamID == teamID) continue;

            if (DiplomacyManager.Instance != null && !DiplomacyManager.Instance.AreAtWar(teamID, a.teamID))
                continue;

            float score = Vector3.Distance(transform.position, a.transform.position);

            if (personality == AIPersonality.Aggressive && a.isCivilian)
                score *= 0.7f;

            if (personality == AIPersonality.Defensive && !a.isBuilding)
                score *= 1.3f;

            if (score < bestDist && score <= attackRange)
            {
                bestDist = score;
                best = a;
            }
        }

        return best;
    }
}
