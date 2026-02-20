// ============================================================================
// DiplomacyManager.cs
// ============================================================================

using System.Collections.Generic;
using UnityEngine;

public class DiplomacyManager : MonoBehaviour
{
    public static DiplomacyManager Instance;

    [Header("Startup")]
    [Tooltip("If enabled, all known teams start at war with each other.")]
    public bool initializeAllTeamsAtWar = true;

    private readonly Dictionary<int, HashSet<int>> warMatrix =
        new();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (initializeAllTeamsAtWar)
            DeclareWarForAllKnownTeams();
    }

    public bool AreAtWar(int a, int b)
    {
        if (a == b) return false;
        return warMatrix.ContainsKey(a) && warMatrix[a].Contains(b);
    }

    public void DeclareWar(int a, int b)
    {
        if (a == b) return;

        Ensure(a);
        Ensure(b);
        warMatrix[a].Add(b);
        warMatrix[b].Add(a);
    }

    public void SetWarState(int a, int b, bool atWar)
    {
        if (atWar) DeclareWar(a, b);
        else MakePeace(a, b);
    }

    public void MakePeace(int a, int b)
    {
        if (warMatrix.ContainsKey(a)) warMatrix[a].Remove(b);
        if (warMatrix.ContainsKey(b)) warMatrix[b].Remove(a);
    }

    public List<int> GetKnownTeams()
    {
        var teams = new HashSet<int>();

        foreach (var kv in warMatrix)
            teams.Add(kv.Key);

        foreach (var t in GameObject.FindObjectsByType<Team>(FindObjectsSortMode.None))
            teams.Add(t.teamID);

        foreach (var a in GameObject.FindObjectsByType<Attackable>(FindObjectsSortMode.None))
            teams.Add(a.teamID);

        var list = new List<int>(teams);
        list.Sort();
        return list;
    }

    public void DeclareWarForAllKnownTeams()
    {
        var teamIDs = GetKnownTeams();

        for (int i = 0; i < teamIDs.Count; i++)
        {
            for (int j = i + 1; j < teamIDs.Count; j++)
            {
                DeclareWar(teamIDs[i], teamIDs[j]);
            }
        }

 //       Debug.Log($"[DiplomacyManager] Initialized diplomacy. Teams at war count: {teamIDs.Count}", this);
    }

    private void Ensure(int team)
    {
        if (!warMatrix.ContainsKey(team))
            warMatrix[team] = new HashSet<int>();
    }
}
