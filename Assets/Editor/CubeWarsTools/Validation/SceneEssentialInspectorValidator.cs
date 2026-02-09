#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneEssentialInspectorValidator : EditorWindow
{
    [MenuItem("CubeWars/Validation/Validate Active Scene Essentials")]
    public static void ValidateActiveSceneEssentials()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid())
        {
            Debug.LogError("[SceneValidation] No active scene loaded.");
            return;
        }

        if (!scene.isLoaded)
            EditorSceneManager.OpenScene(scene.path, OpenSceneMode.Single);

        var issues = new List<string>();

        RequireSingle<GameManager>(issues);
        RequireSingle<TeamStorageManager>(issues);
        RequireSingle<BuildPlacementManager>(issues);
        RequireSingle<JobManager>(issues);

        Team[] teams = Object.FindObjectsOfType<Team>(true);
        if (teams.Length == 0)
            issues.Add("No Team components found in scene.");

        int playerTeams = 0;
        for (int i = 0; i < teams.Length; i++)
        {
            var team = teams[i];
            if (team == null) continue;

            if (team.teamType == TeamType.Player)
                playerTeams++;

            if (team.hqRoot == null) issues.Add($"Team {team.teamID} missing HQ root.");
            if (team.unitsRoot == null) issues.Add($"Team {team.teamID} missing Units root.");
            if (team.buildingsRoot == null) issues.Add($"Team {team.teamID} missing Buildings root.");
        }

        if (playerTeams != 1)
            issues.Add($"Expected exactly 1 player team, found {playerTeams}.");

        Headquarters[] hqs = Object.FindObjectsOfType<Headquarters>(true);
        if (hqs.Length == 0)
            issues.Add("No Headquarters found.");

        for (int i = 0; i < hqs.Length; i++)
        {
            var hq = hqs[i];
            if (hq == null) continue;

            var storage = hq.GetComponentsInChildren<ResourceStorageContainer>(true);
            if (storage.Length == 0)
                issues.Add($"HQ '{hq.name}' (Team {hq.teamID}) has no ResourceStorageContainer in children.");

            var provider = hq.GetComponentsInChildren<ResourceStorageProvider>(true);
            if (provider.Length == 0)
                issues.Add($"HQ '{hq.name}' (Team {hq.teamID}) has no ResourceStorageProvider in children.");
        }

        if (issues.Count == 0)
        {
            Debug.Log($"[SceneValidation] PASS '{scene.name}': essential runtime objects look valid.");
            return;
        }

        Debug.LogWarning($"[SceneValidation] '{scene.name}' found {issues.Count} issue(s):\n - {string.Join("\n - ", issues)}");
    }

    static void RequireSingle<T>(List<string> issues) where T : Object
    {
        var found = Object.FindObjectsOfType<T>(true);
        if (found.Length == 0)
            issues.Add($"Missing required component: {typeof(T).Name}");
        else if (found.Length > 1)
            issues.Add($"Multiple instances of {typeof(T).Name} found: {found.Length}");
    }
}
#endif
