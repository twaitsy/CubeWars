using UnityEngine;
using UnityEditor;
using System.Linq;

public class FindTeamDuplicates : MonoBehaviour
{
    [MenuItem("Tools/Find Team Duplicates")]
    static void FindTeams()
    {
        var scripts = Resources.FindObjectsOfTypeAll<MonoScript>()
            .Where(s => s != null && s.text.Contains("class Team"))
            .ToArray();

        foreach (var s in scripts)
            Debug.Log("FOUND TEAM CLASS IN: " + AssetDatabase.GetAssetPath(s));
    }
}