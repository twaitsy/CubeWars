using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class StatsCheckerWindow : EditorWindow
{
    private const string OutputFolder = "Assets/Documentation/StatsChecker/";
    private const string IgnoredStatsFile = OutputFolder + "ignored_stats.txt";
    private const string IgnoredScriptsFile = OutputFolder + "ignored_scripts.txt";
    private const string ReportFile = OutputFolder + "StatsReport.txt";

    private Dictionary<string, List<string>> missingStats = new Dictionary<string, List<string>>();
    private HashSet<string> ignoredStats = new HashSet<string>();
    private HashSet<string> ignoredScripts = new HashSet<string>();

    private Vector2 scroll;

    [MenuItem("Tools/Stats Checker")]
    public static void Open()
    {
        StatsCheckerWindow window = GetWindow<StatsCheckerWindow>("Stats Checker");
        window.Show();
    }

    private void OnEnable()
    {
        EnsureFolders();
        LoadIgnoreLists();
    }

    private void EnsureFolders()
    {
        if (!Directory.Exists(OutputFolder))
            Directory.CreateDirectory(OutputFolder);
    }

    private void LoadIgnoreLists()
    {
        ignoredStats = LoadList(IgnoredStatsFile);
        ignoredScripts = LoadList(IgnoredScriptsFile);
    }

    private HashSet<string> LoadList(string path)
    {
        if (!File.Exists(path))
            return new HashSet<string>();

        string[] lines = File.ReadAllLines(path);
        HashSet<string> set = new HashSet<string>();

        foreach (string line in lines)
        {
            if (!string.IsNullOrEmpty(line.Trim()))
                set.Add(line.Trim());
        }

        return set;
    }

    private void SaveList(string path, HashSet<string> list)
    {
        File.WriteAllLines(path, list.OrderBy(x => x).ToArray());
    }

    private void OnGUI()
    {
        GUILayout.Space(10);

        if (GUILayout.Button("Scan Now", GUILayout.Height(30)))
        {
            ScanProject();
        }

        GUILayout.Space(10);

        scroll = GUILayout.BeginScrollView(scroll);

        if (missingStats.Count == 0)
        {
            GUILayout.Label("No missing stats detected.");
        }
        else
        {
            foreach (KeyValuePair<string, List<string>> kvp in missingStats)
            {
                string scriptName = kvp.Key;
                GUILayout.Label(scriptName, EditorStyles.boldLabel);

                foreach (string stat in kvp.Value)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(" - " + stat);

                    if (GUILayout.Button("Ignore Stat", GUILayout.Width(100)))
                    {
                        ignoredStats.Add(scriptName + ":" + stat);
                        SaveList(IgnoredStatsFile, ignoredStats);
                        ScanProject();
                        return;
                    }

                    if (GUILayout.Button("Ignore Script", GUILayout.Width(110)))
                    {
                        ignoredScripts.Add(scriptName);
                        SaveList(IgnoredScriptsFile, ignoredScripts);
                        ScanProject();
                        return;
                    }

                    GUILayout.EndHorizontal();
                }

                GUILayout.Space(10);
            }
        }

        GUILayout.EndScrollView();

        if (GUILayout.Button("Export Report", GUILayout.Height(25)))
        {
            ExportReport();
        }
    }

    private void ScanProject()
    {
        missingStats.Clear();

        Dictionary<string, List<string>> allStats = ScanAllStats();
        HashSet<string> displayedStats = ScanInspectorDisplayedStats();

        foreach (KeyValuePair<string, List<string>> kvp in allStats)
        {
            string scriptName = kvp.Key;

            if (ignoredScripts.Contains(scriptName))
                continue;

            foreach (string stat in kvp.Value)
            {
                string ignoreKey = scriptName + ":" + stat;
                if (ignoredStats.Contains(ignoreKey))
                    continue;

                if (!displayedStats.Contains(scriptName + ":" + stat))
                {
                    if (!missingStats.ContainsKey(scriptName))
                        missingStats[scriptName] = new List<string>();

                    missingStats[scriptName].Add(stat);
                }
            }
        }

        Repaint();
    }

    private Dictionary<string, List<string>> ScanAllStats()
    {
        Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();

        Assembly assembly = Assembly.Load("Assembly-CSharp");
        Type[] types = assembly.GetTypes();

        foreach (Type type in types)
        {
            if (!type.IsClass || type.IsAbstract)
                continue;

            string scriptName = type.Name + " (" + type.Name + ".cs)";

            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            PropertyInfo[] props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            List<string> stats = new List<string>();

            foreach (FieldInfo f in fields)
            {
                if (IsNumericType(f.FieldType))
                    stats.Add(f.Name);
            }

            foreach (PropertyInfo p in props)
            {
                if (p.CanRead && IsNumericType(p.PropertyType))
                    stats.Add(p.Name);
            }

            if (stats.Count > 0)
                result[scriptName] = stats.Distinct().ToList();
        }

        return result;
    }

    private bool IsNumericType(Type t)
    {
        return t == typeof(int) ||
               t == typeof(float) ||
               t == typeof(double) ||
               t == typeof(long) ||
               t == typeof(short) ||
               t == typeof(bool) ||
               t.IsEnum;
    }

    private HashSet<string> ScanInspectorDisplayedStats()
    {
        HashSet<string> result = new HashSet<string>();

        string inspectorPath = FindInspectorScriptPath();
        if (inspectorPath == null)
            return result;

        string code = File.ReadAllText(inspectorPath);

        string[] methodsToParse = new string[]
        {
            "DrawOverview",
            "DrawStats",
            "DrawSkillsAndTools",
            "DrawStorage",
            "DrawProduction",
            "DrawCombat",
            "DrawBuildingStats",
            "DrawNeeds",
            "DrawInventory",
            "DrawTrainingQueue",
            "DrawConstructionStatus"
        };

        foreach (string method in methodsToParse)
        {
            string pattern = "void " + method + @"\\s*\\([^)]*\\)\\s*\\{([\\s\\S]*?)\\}";
            Match match = Regex.Match(code, pattern);

            if (!match.Success)
                continue;

            string body = match.Groups[1].Value;

            MatchCollection fieldMatches = Regex.Matches(body, @"([A-Za-z0-9_]+)\.([A-Za-z0-9_]+)");

            foreach (Match m in fieldMatches)
            {
                string className = m.Groups[1].Value;
                string fieldName = m.Groups[2].Value;

                string scriptName = className + " (" + className + ".cs)";
                result.Add(scriptName + ":" + fieldName);
            }
        }

        return result;
    }

    private string FindInspectorScriptPath()
    {
        string[] guids = AssetDatabase.FindAssets("UnitInspectorUI t:Script");
        if (guids.Length == 0)
            return null;

        return AssetDatabase.GUIDToAssetPath(guids[0]);
    }

    private void ExportReport()
    {
        using (StreamWriter writer = new StreamWriter(ReportFile))
        {
            writer.WriteLine("=== Stats Checker Report ===");
            writer.WriteLine(DateTime.Now.ToString());
            writer.WriteLine();

            foreach (KeyValuePair<string, List<string>> kvp in missingStats)
            {
                writer.WriteLine(kvp.Key);
                foreach (string stat in kvp.Value)
                    writer.WriteLine(" - " + stat);
                writer.WriteLine();
            }

            writer.WriteLine("=== Ignored Stats ===");
            foreach (string s in ignoredStats)
                writer.WriteLine(s);

            writer.WriteLine();
            writer.WriteLine("=== Ignored Scripts ===");
            foreach (string s in ignoredScripts)
                writer.WriteLine(s);
        }

        AssetDatabase.Refresh();
        Debug.Log("Stats report exported to: " + ReportFile);
    }
}