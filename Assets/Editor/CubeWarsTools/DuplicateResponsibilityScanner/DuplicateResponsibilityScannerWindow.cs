using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class DuplicateResponsibilityScannerWindow : EditorWindow
{
    private const string ScriptsRoot = "Assets/Scripts/";
    private const float SimilarityThreshold = 0.65f;

    private Vector2 scroll;
    private bool scanCompleted = false;

    private List<ScriptData> scripts = new List<ScriptData>();
    private List<Cluster> clusters = new List<Cluster>();

    private class ScriptData
    {
        public string name;
        public string path;
        public string code;

        public HashSet<string> tokens = new HashSet<string>();
    }

    private class Cluster
    {
        public float similarity;
        public List<ScriptData> members = new List<ScriptData>();

        // UI foldout state
        public bool foldout = true;
    }

    [MenuItem("Tools/CubeWars/Duplicate Responsibility Scanner")]
    public static void Open()
    {
        var window = GetWindow<DuplicateResponsibilityScannerWindow>();
        window.titleContent = new GUIContent("Duplicate Responsibility Scanner");
        window.minSize = new Vector2(900, 600);
        window.Show();
    }

    private void OnEnable()
    {
        EditorApplication.delayCall += RunScan;
    }

    private void RunScan()
    {
        scanCompleted = false;
        scripts.Clear();
        clusters.Clear();

        try
        {
            LoadScripts();
            TokenizeScripts();
            BuildClusters();
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        scanCompleted = true;
        Repaint();
    }

    // ---------------------------------------------------------
    // Load scripts from Assets/Scripts/
    // ---------------------------------------------------------
    private void LoadScripts()
    {
        EditorUtility.DisplayProgressBar("Scanning", "Loading scripts...", 0.1f);

        string[] guids = AssetDatabase.FindAssets("t:MonoScript");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            if (!path.StartsWith(ScriptsRoot, StringComparison.OrdinalIgnoreCase))
                continue;

            string code = File.ReadAllText(path);

            scripts.Add(new ScriptData
            {
                name = Path.GetFileNameWithoutExtension(path),
                path = path,
                code = code
            });
        }
    }

    // ---------------------------------------------------------
    // Tokenize scripts (hybrid structural + semantic)
    // ---------------------------------------------------------
    private void TokenizeScripts()
    {
        EditorUtility.DisplayProgressBar("Scanning", "Tokenizing scripts...", 0.3f);

        foreach (var script in scripts)
        {
            var tokens = new HashSet<string>();

            // Extract class name
            var classMatch = Regex.Match(script.code, @"class\s+(\w+)");
            if (classMatch.Success)
                tokens.Add(classMatch.Groups[1].Value);

            // Extract base class
            var baseMatch = Regex.Match(script.code, @"class\s+\w+\s*:\s*(\w+)");
            if (baseMatch.Success)
                tokens.Add(baseMatch.Groups[1].Value);

            // Extract interfaces
            foreach (Match m in Regex.Matches(script.code, @"class\s+\w+\s*:\s*\w+,\s*(\w+)"))
                tokens.Add(m.Groups[1].Value);

            // Extract method names
            foreach (Match m in Regex.Matches(script.code, @"\b(\w+)\s*\("))
                tokens.Add(m.Groups[1].Value);

            // Extract field names
            foreach (Match m in Regex.Matches(script.code, @"\b(\w+)\s+\w+\s*(=|;)"))
                tokens.Add(m.Groups[1].Value);

            // Extract comments
            foreach (Match m in Regex.Matches(script.code, @"//(.*)"))
                foreach (var word in m.Groups[1].Value.Split(' '))
                    tokens.Add(word.Trim().ToLower());

            // Extract XML docs
            foreach (Match m in Regex.Matches(script.code, @"///(.*)"))
                foreach (var word in m.Groups[1].Value.Split(' '))
                    tokens.Add(word.Trim().ToLower());

            script.tokens = tokens;
        }
    }

    // ---------------------------------------------------------
    // Build clusters based on similarity
    // ---------------------------------------------------------
    private void BuildClusters()
    {
        EditorUtility.DisplayProgressBar("Scanning", "Building clusters...", 0.6f);

        var unclustered = new HashSet<ScriptData>(scripts);

        while (unclustered.Count > 0)
        {
            var seed = unclustered.First();
            unclustered.Remove(seed);

            var cluster = new Cluster();
            cluster.members.Add(seed);

            foreach (var other in unclustered.ToList())
            {
                float sim = ComputeSimilarity(seed, other);

                if (sim >= SimilarityThreshold)
                {
                    cluster.members.Add(other);
                    unclustered.Remove(other);
                }
            }

            if (cluster.members.Count > 1)
            {
                cluster.similarity = ComputeClusterSimilarity(cluster);
                clusters.Add(cluster);
            }
        }
    }

    private float ComputeSimilarity(ScriptData a, ScriptData b)
    {
        var intersection = a.tokens.Intersect(b.tokens).Count();
        var union = a.tokens.Union(b.tokens).Count();

        if (union == 0) return 0f;

        return (float)intersection / union;
    }

    private float ComputeClusterSimilarity(Cluster cluster)
    {
        float total = 0f;
        int count = 0;

        for (int i = 0; i < cluster.members.Count; i++)
        {
            for (int j = i + 1; j < cluster.members.Count; j++)
            {
                total += ComputeSimilarity(cluster.members[i], cluster.members[j]);
                count++;
            }
        }

        return count > 0 ? total / count : 0f;
    }

    // ---------------------------------------------------------
    // UI
    // ---------------------------------------------------------
    private void OnGUI()
    {
        GUILayout.Space(10);

        if (!scanCompleted)
        {
            GUILayout.Label("Scanning scripts...", EditorStyles.boldLabel);
            return;
        }

        scroll = GUILayout.BeginScrollView(scroll);

        if (clusters.Count == 0)
        {
            GUILayout.Label("No duplicate responsibilities detected.", EditorStyles.boldLabel);
            GUILayout.EndScrollView();
            return;
        }

        int clusterIndex = 1;

        foreach (var cluster in clusters)
        {
            GUILayout.BeginVertical("box");

            cluster.foldout = EditorGUILayout.Foldout(
                cluster.foldout,
                $"Cluster {clusterIndex} — Similarity {cluster.similarity:F2}"
            );

            if (cluster.foldout)
            {
                foreach (var script in cluster.members)
                {
                    GUILayout.BeginHorizontal();

                    GUILayout.Label(script.name, GUILayout.Width(250));
                    GUILayout.Label(script.path, GUILayout.Width(450));

                    if (GUILayout.Button("Ping", GUILayout.Width(50)))
                        EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<MonoScript>(script.path));

                    if (GUILayout.Button("Open", GUILayout.Width(50)))
                        AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<MonoScript>(script.path));

                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.EndVertical();
            GUILayout.Space(5);

            clusterIndex++;
        }

        GUILayout.EndScrollView();
    }
}