using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UnusedScriptScannerWindow : EditorWindow
{
    private enum ViewMode { Table, List }
    private ViewMode viewMode = ViewMode.Table;

    private Vector2 scroll;
    private bool scanCompleted = false;

    private List<ScriptInfo> allScripts = new List<ScriptInfo>();
    private HashSet<string> usedScripts = new HashSet<string>();
    private List<ScriptInfo> unusedScripts = new List<ScriptInfo>();

    private const string ScriptsRoot = "Assets/Scripts/";

    private class ScriptInfo
    {
        public string name;
        public string path;
        public MonoScript script;
        public string reason;
    }

    [MenuItem("Tools/CubeWars/Unused Script Scanner")]
    public static void Open()
    {
        var window = GetWindow<UnusedScriptScannerWindow>();
        window.titleContent = new GUIContent("Unused Script Scanner");
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
        usedScripts.Clear();
        allScripts.Clear();
        unusedScripts.Clear();

        try
        {
            ScanAllScripts();
            ScanScenes();
            ScanPrefabs();
            ScanScriptableObjects();
            ScanResources();
            ScanAddComponentCalls();
            ScanReflectionUsage();
            ScanInheritance();
            ScanEditorTooling();

            IdentifyUnusedScripts();
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        scanCompleted = true;
        Repaint();
    }

    // ---------------------------------------------------------
    // SCAN: Collect only scripts inside Assets/Scripts/
    // ---------------------------------------------------------
    private void ScanAllScripts()
    {
        EditorUtility.DisplayProgressBar("Scanning Scripts", "Collecting scripts from Assets/Scripts...", 0.05f);

        string[] scriptGuids = AssetDatabase.FindAssets("t:MonoScript");

        foreach (string guid in scriptGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            // Only include scripts inside Assets/Scripts/
            if (!path.StartsWith(ScriptsRoot, StringComparison.OrdinalIgnoreCase))
                continue;

            MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
            if (script == null) continue;

            allScripts.Add(new ScriptInfo
            {
                name = script.name,
                path = path,
                script = script
            });
        }
    }

    // ---------------------------------------------------------
    // SCAN: Scenes
    // ---------------------------------------------------------
    private void ScanScenes()
    {
        EditorUtility.DisplayProgressBar("Scanning Scenes", "Loading scenes...", 0.15f);

        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene");

        SceneSetup[] originalSetup = EditorSceneManager.GetSceneManagerSetup();

        foreach (string guid in sceneGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            CollectScriptsFromScene(scene);
        }

        EditorSceneManager.RestoreSceneManagerSetup(originalSetup);
    }

    private void CollectScriptsFromScene(Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (var comp in root.GetComponentsInChildren<Component>(true))
            {
                if (comp == null) continue;

                MonoBehaviour mb = comp as MonoBehaviour;
                if (mb == null) continue;

                MonoScript script = MonoScript.FromMonoBehaviour(mb);
                if (script != null && script != null)
                {
                    string path = AssetDatabase.GetAssetPath(script);
                    if (path.StartsWith(ScriptsRoot))
                        usedScripts.Add(script.name);
                }
            }
        }
    }

    // ---------------------------------------------------------
    // SCAN: Prefabs
    // ---------------------------------------------------------
    private void ScanPrefabs()
    {
        EditorUtility.DisplayProgressBar("Scanning Prefabs", "Loading prefabs...", 0.30f);

        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");

        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null) continue;

            foreach (var comp in prefab.GetComponentsInChildren<Component>(true))
            {
                if (comp == null) continue;

                MonoBehaviour mb = comp as MonoBehaviour;
                if (mb == null) continue;

                MonoScript script = MonoScript.FromMonoBehaviour(mb);
                if (script != null)
                {
                    string scriptPath = AssetDatabase.GetAssetPath(script);
                    if (scriptPath.StartsWith(ScriptsRoot))
                        usedScripts.Add(script.name);
                }
            }
        }
    }

    // ---------------------------------------------------------
    // SCAN: ScriptableObjects
    // ---------------------------------------------------------
    private void ScanScriptableObjects()
    {
        EditorUtility.DisplayProgressBar("Scanning ScriptableObjects", "Loading assets...", 0.45f);

        string[] soGuids = AssetDatabase.FindAssets("t:ScriptableObject");

        foreach (string guid in soGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ScriptableObject so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);

            if (so == null) continue;

            SerializedObject soObj = new SerializedObject(so);
            SerializedProperty prop = soObj.GetIterator();

            while (prop.NextVisible(true))
            {
                if (prop.propertyType == SerializedPropertyType.ObjectReference)
                {
                    UnityEngine.Object obj = prop.objectReferenceValue;
                    if (obj is MonoScript ms)
                    {
                        string scriptPath = AssetDatabase.GetAssetPath(ms);
                        if (scriptPath.StartsWith(ScriptsRoot))
                            usedScripts.Add(ms.name);
                    }
                }
            }
        }
    }

    // ---------------------------------------------------------
    // SCAN: Resources
    // ---------------------------------------------------------
    private void ScanResources()
    {
        EditorUtility.DisplayProgressBar("Scanning Resources", "Loading assets...", 0.55f);

        UnityEngine.Object[] resources = Resources.LoadAll("", typeof(UnityEngine.Object));

        foreach (var obj in resources)
        {
            if (obj is GameObject go)
            {
                foreach (var comp in go.GetComponentsInChildren<Component>(true))
                {
                    if (comp == null) continue;

                    MonoBehaviour mb = comp as MonoBehaviour;
                    if (mb == null) continue;

                    MonoScript script = MonoScript.FromMonoBehaviour(mb);
                    if (script != null)
                    {
                        string scriptPath = AssetDatabase.GetAssetPath(script);
                        if (scriptPath.StartsWith(ScriptsRoot))
                            usedScripts.Add(script.name);
                    }
                }
            }
        }
    }

    // ---------------------------------------------------------
    // SCAN: AddComponent<T>()
    // ---------------------------------------------------------
    private void ScanAddComponentCalls()
    {
        EditorUtility.DisplayProgressBar("Scanning Code", "Detecting AddComponent<T>()...", 0.65f);

        Regex regex = new Regex(@"AddComponent<(\w+)>");

        foreach (var script in allScripts)
        {
            string code = script.script.text;
            MatchCollection matches = regex.Matches(code);

            foreach (Match match in matches)
                usedScripts.Add(match.Groups[1].Value);
        }
    }

    // ---------------------------------------------------------
    // SCAN: Reflection
    // ---------------------------------------------------------
    private void ScanReflectionUsage()
    {
        EditorUtility.DisplayProgressBar("Scanning Code", "Detecting reflection usage...", 0.75f);

        Regex regex = new Regex(@"GetType\(""(\w+)""");

        foreach (var script in allScripts)
        {
            string code = script.script.text;
            MatchCollection matches = regex.Matches(code);

            foreach (Match match in matches)
                usedScripts.Add(match.Groups[1].Value);
        }
    }

    // ---------------------------------------------------------
    // SCAN: Inheritance
    // ---------------------------------------------------------
    private void ScanInheritance()
    {
        EditorUtility.DisplayProgressBar("Scanning Code", "Detecting inheritance...", 0.85f);

        foreach (var script in allScripts)
        {
            Type type = script.script.GetClass();
            if (type == null) continue;

            if (usedScripts.Contains(type.Name))
            {
                Type baseType = type.BaseType;
                while (baseType != null && baseType != typeof(MonoBehaviour))
                {
                    usedScripts.Add(baseType.Name);
                    baseType = baseType.BaseType;
                }
            }
        }
    }

    // ---------------------------------------------------------
    // SCAN: Editor Tooling
    // ---------------------------------------------------------
    private void ScanEditorTooling()
    {
        EditorUtility.DisplayProgressBar("Scanning Editor Scripts", "Detecting CustomEditor...", 0.90f);

        Regex regex = new Regex(@"CustomEditor\s*\(\s*typeof\((\w+)\)");

        foreach (var script in allScripts)
        {
            string code = script.script.text;
            MatchCollection matches = regex.Matches(code);

            foreach (Match match in matches)
                usedScripts.Add(match.Groups[1].Value);
        }
    }

    // ---------------------------------------------------------
    // FINAL: Identify unused scripts
    // ---------------------------------------------------------
    private void IdentifyUnusedScripts()
    {
        EditorUtility.DisplayProgressBar("Finalizing", "Identifying unused scripts...", 0.98f);

        foreach (var script in allScripts)
        {
            if (!usedScripts.Contains(script.name))
            {
                script.reason = "Not referenced anywhere in project";
                unusedScripts.Add(script);
            }
        }
    }

    // ---------------------------------------------------------
    // UI
    // ---------------------------------------------------------
    private void OnGUI()
    {
        GUILayout.Space(10);

        if (!scanCompleted)
        {
            GUILayout.Label("Scanning project...", EditorStyles.boldLabel);
            return;
        }

        GUILayout.BeginHorizontal();
        if (GUILayout.Toggle(viewMode == ViewMode.Table, "Table View", EditorStyles.toolbarButton))
            viewMode = ViewMode.Table;
        if (GUILayout.Toggle(viewMode == ViewMode.List, "List View", EditorStyles.toolbarButton))
            viewMode = ViewMode.List;
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        scroll = GUILayout.BeginScrollView(scroll);

        if (viewMode == ViewMode.Table)
            DrawTableView();
        else
            DrawListView();

        GUILayout.EndScrollView();
    }

    private void DrawTableView()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Script", GUILayout.Width(200));
        GUILayout.Label("Path", GUILayout.Width(450));
        GUILayout.Label("Reason", GUILayout.Width(200));
        GUILayout.Label("Actions", GUILayout.Width(100));
        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        foreach (var script in unusedScripts)
        {
            GUILayout.BeginHorizontal();

            GUILayout.Label(script.name, GUILayout.Width(200));
            GUILayout.Label(script.path, GUILayout.Width(450));
            GUILayout.Label(script.reason, GUILayout.Width(200));

            if (GUILayout.Button("Ping", GUILayout.Width(50)))
                EditorGUIUtility.PingObject(script.script);

            if (GUILayout.Button("Open", GUILayout.Width(50)))
                AssetDatabase.OpenAsset(script.script);

            GUILayout.EndHorizontal();
        }
    }

    private void DrawListView()
    {
        foreach (var script in unusedScripts)
        {
            GUILayout.BeginVertical("box");
            GUILayout.Label(script.name, EditorStyles.boldLabel);
            GUILayout.Label(script.path);
            GUILayout.Label("Reason: " + script.reason);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Ping", GUILayout.Width(60)))
                EditorGUIUtility.PingObject(script.script);
            if (GUILayout.Button("Open", GUILayout.Width(60)))
                AssetDatabase.OpenAsset(script.script);
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            GUILayout.Space(5);
        }
    }
}