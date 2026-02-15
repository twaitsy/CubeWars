using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class SceneRequiredScripts : MonoBehaviour
{
    [SerializeField] bool checkOnAwake = true;

    static readonly Type[] RequiredSceneScripts =
    {
        typeof(GameManager),
        typeof(TeamStorageManager),
        typeof(TeamResources),
        typeof(DiplomacyManager),
        typeof(BuildPlacementManager),
        typeof(BuildGridManager),
        typeof(ResourceRegistry),
        typeof(JobManager),
        typeof(WorkerTaskDispatcher),
        typeof(WorkerTaskGenerationSystem),
        typeof(CraftingJobManager),
        typeof(ProductionNotificationManager),
        typeof(ProjectilePool),
        typeof(SelectionManager),
        typeof(UnitInspectorUI),
        typeof(BuildMenuUI),
        typeof(Minimap),
        typeof(RTSCamera),
    };

    void Awake()
    {
        if (checkOnAwake)
            ValidateScene();
    }

    [ContextMenu("Validate Required Scene Scripts")]
    public void ValidateScene()
    {
        var missingRequiredTypes = new List<string>();

        for (int i = 0; i < RequiredSceneScripts.Length; i++)
        {
            Type requiredType = RequiredSceneScripts[i];
            if (FindObjectOfType(requiredType) == null)
                missingRequiredTypes.Add(requiredType.Name);
        }

        if (FindObjectOfType<TeamBootstrap>() == null &&
            FindObjectOfType<SixTeamBootstrap>() == null &&
            FindObjectOfType<HQSpawner>() == null)
        {
            missingRequiredTypes.Add("TeamBootstrap OR SixTeamBootstrap OR HQSpawner");
        }

        int missingScriptComponents = CountMissingScriptComponentsInActiveScene();

        if (missingRequiredTypes.Count == 0 && missingScriptComponents == 0)
        {
            Debug.Log("[SceneRequiredScripts] Validation passed: all required scene scripts are present and no missing script components were found.", this);
            return;
        }

        if (missingRequiredTypes.Count > 0)
        {
            Debug.LogError("[SceneRequiredScripts] Missing required scene scripts: " + string.Join(", ", missingRequiredTypes), this);
        }

        if (missingScriptComponents > 0)
        {
            Debug.LogError("[SceneRequiredScripts] Found " + missingScriptComponents + " missing script component(s) in the active scene hierarchy.", this);
        }
    }

    static int CountMissingScriptComponentsInActiveScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
            return 0;

        int missingCount = 0;
        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            Transform[] allTransforms = roots[i].GetComponentsInChildren<Transform>(true);
            for (int j = 0; j < allTransforms.Length; j++)
            {
                Component[] components = allTransforms[j].GetComponents<Component>();
                for (int k = 0; k < components.Length; k++)
                {
                    if (components[k] == null)
                        missingCount++;
                }
            }
        }

        return missingCount;
    }
}
