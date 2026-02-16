using System;
using System.Collections.Generic;
using UnityEngine;

public class ConstructionRegistry : MonoBehaviour
{
    public static ConstructionRegistry Instance { get; private set; }

    // --- Events -------------------------------------------------------------

    public static event Action<ConstructionSite> OnSiteRegistered;
    public static event Action<ConstructionSite> OnSiteUnregistered;
    public static event Action<ConstructionSite> OnSiteChanged;
    public static event Action<ConstructionSite> OnSiteCompleted;

    // --- Data ---------------------------------------------------------------

    readonly HashSet<ConstructionSite> allSites = new();
    readonly Dictionary<int, List<ConstructionSite>> sitesByTeam = new();

    // --- Lifecycle ----------------------------------------------------------

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple ConstructionRegistry instances detected. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    // --- Registration -------------------------------------------------------

    public void Register(ConstructionSite site)
    {
        if (site == null)
            return;

        if (!allSites.Add(site))
            return;

        if (!sitesByTeam.TryGetValue(site.teamID, out var list))
        {
            list = new List<ConstructionSite>();
            sitesByTeam[site.teamID] = list;
        }

        list.Add(site);

        OnSiteRegistered?.Invoke(site);
    }

    public void Unregister(ConstructionSite site)
    {
        if (site == null)
            return;

        if (allSites.Remove(site))
        {
            if (sitesByTeam.TryGetValue(site.teamID, out var list))
                list.Remove(site);

            OnSiteUnregistered?.Invoke(site);
        }
    }

    // --- Change Notifications ----------------------------------------------

    public void NotifySiteChanged(ConstructionSite site)
    {
        if (site == null)
            return;

        if (site.IsComplete)
        {
            OnSiteCompleted?.Invoke(site);
        }

        OnSiteChanged?.Invoke(site);
    }

    // --- Queries ------------------------------------------------------------

    public IReadOnlyCollection<ConstructionSite> AllSites => allSites;

    public IReadOnlyList<ConstructionSite> GetSitesForTeam(int teamID)
    {
        if (sitesByTeam.TryGetValue(teamID, out var list))
            return list;

        return Array.Empty<ConstructionSite>();
    }
}