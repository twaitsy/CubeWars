using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CubeWarsTools.Analysis;

namespace CubeWarsTools.Browser
{
    public class ScriptBrowserWindow : EditorWindow
    {
        private Vector2 folderScroll;
        private Vector2 scriptScroll;
        private Vector2 analysisScroll;

        private string searchTerm = "";
        private string selectedFolder = "";
        private string selectedScript = "";

        private Dictionary<string, List<string>> folderToScripts;
        private Dictionary<string, ScriptAnalysisResult> scriptAnalysis;
        private Dictionary<string, DependencyAnalysisResult> dependencyAnalysis;

        [MenuItem("Tools/CubeWars/Analysis/Open Script Browser")]
        public static void Open()
        {
            var window = GetWindow<ScriptBrowserWindow>("Script Browser");
            window.minSize = new Vector2(1100, 600);
            window.RefreshData();
        }

        private void RefreshData()
        {
            ScriptBrowserData.Load(out folderToScripts, out scriptAnalysis, out dependencyAnalysis);
        }

        private void OnGUI()
        {
            DrawSearchBar();

            EditorGUILayout.BeginHorizontal();

            DrawFolderPanel();
            DrawScriptPanel();
            DrawAnalysisPanel();

            EditorGUILayout.EndHorizontal();
        }

        // ---------------------------------------------------------
        // SEARCH BAR
        // ---------------------------------------------------------

        private void DrawSearchBar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            GUILayout.Label("Search:", GUILayout.Width(50));
            string newSearch = GUILayout.TextField(searchTerm, EditorStyles.toolbarTextField);

            if (newSearch != searchTerm)
            {
                searchTerm = newSearch;
                selectedScript = "";
            }

            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(70)))
                RefreshData();

            EditorGUILayout.EndHorizontal();
        }

        // ---------------------------------------------------------
        // LEFT PANEL — FOLDER TREE
        // ---------------------------------------------------------

        private void DrawFolderPanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(250));
            EditorGUILayout.LabelField("Folders", EditorStyles.boldLabel);

            folderScroll = EditorGUILayout.BeginScrollView(folderScroll);

            foreach (var folder in folderToScripts.Keys.OrderBy(f => f))
            {
                bool isSelected = folder == selectedFolder;

                GUIStyle style = isSelected ? EditorStyles.whiteLabel : EditorStyles.label;

                if (GUILayout.Button(folder, style))
                {
                    selectedFolder = folder;
                    selectedScript = "";
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        // ---------------------------------------------------------
        // MIDDLE PANEL — SCRIPT LIST
        // ---------------------------------------------------------

        private void DrawScriptPanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(300));
            EditorGUILayout.LabelField("Scripts", EditorStyles.boldLabel);

            scriptScroll = EditorGUILayout.BeginScrollView(scriptScroll);

            if (!string.IsNullOrEmpty(selectedFolder) && folderToScripts.ContainsKey(selectedFolder))
            {
                IEnumerable<string> scripts = folderToScripts[selectedFolder];

                if (!string.IsNullOrEmpty(searchTerm))
                    scripts = scripts.Where(s => s.ToLower().Contains(searchTerm.ToLower()));

                foreach (var script in scripts.OrderBy(s => s))
                {
                    bool isSelected = script == selectedScript;
                    GUIStyle style = isSelected ? EditorStyles.whiteLabel : EditorStyles.label;

                    if (GUILayout.Button(script, style))
                        selectedScript = script;
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        // ---------------------------------------------------------
        // RIGHT PANEL — ANALYSIS SUMMARY
        // ---------------------------------------------------------

        private void DrawAnalysisPanel()
        {
            EditorGUILayout.BeginVertical();

            EditorGUILayout.LabelField("Analysis", EditorStyles.boldLabel);

            analysisScroll = EditorGUILayout.BeginScrollView(analysisScroll);

            if (!string.IsNullOrEmpty(selectedScript))
            {
                string fullPath = ScriptBrowserData.GetFullScriptPath(selectedFolder, selectedScript);

                if (scriptAnalysis.ContainsKey(fullPath))
                {
                    DrawAnalysisSummary(scriptAnalysis[fullPath], dependencyAnalysis[fullPath]);

                    GUILayout.Space(10);

                    if (GUILayout.Button("Open Script"))
                        ScriptBrowserData.OpenScript(fullPath);

                    if (GUILayout.Button("Open Analysis File"))
                        ScriptBrowserData.OpenAnalysisFile(fullPath);
                }
                else
                {
                    GUILayout.Label("No analysis found.");
                }
            }
            else
            {
                GUILayout.Label("Select a script to view analysis.");
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawAnalysisSummary(ScriptAnalysisResult script, DependencyAnalysisResult deps)
        {
            DrawList("Classes", script.ClassNames);
            DrawList("Base Classes", script.BaseClasses);
            DrawList("Interfaces", script.Interfaces);
            DrawList("Methods", script.Methods);
            DrawList("Properties", script.Properties);
            DrawList("Events", script.Events);
            DrawList("Fields", script.Fields);
            DrawList("Serialized Fields", script.SerializedFields);
            DrawList("Attributes", script.Attributes);

            GUILayout.Space(10);

            DrawList("Using Namespaces", deps.UsingNamespaces);
            DrawList("Type References", deps.TypeReferences);
            DrawList("GetComponent<T>()", deps.GetComponentTypes);
            DrawList("RequireComponent", deps.RequireComponentTypes);
            DrawList("Event Subscriptions", deps.EventSubscriptions);
            DrawList("Attribute Types", deps.AttributeTypes);
        }

        private void DrawList(string title, List<string> items)
        {
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);

            if (items == null || items.Count == 0)
            {
                GUILayout.Label("- None");
                return;
            }

            foreach (var item in items.Distinct().OrderBy(i => i))
                GUILayout.Label("- " + item);

            GUILayout.Space(5);
        }
    }
}