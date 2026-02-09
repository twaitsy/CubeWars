using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CubeWarsTools.Analysis;

namespace CubeWarsTools.Browser
{
    /// <summary>
    /// Loads and caches:
    /// - Script paths
    /// - Folder structure
    /// - Analysis results
    /// Provides helper methods for the Script Browser Window.
    /// </summary>
    public static class ScriptBrowserData
    {
        private static readonly string ScriptsRoot = "Assets/Scripts";
        private static readonly string AnalysisRoot = "Assets/Docs/Analysis";

        /// <summary>
        /// Loads all folder/script mappings and analysis data.
        /// </summary>
        public static void Load(
            out Dictionary<string, List<string>> folderToScripts,
            out Dictionary<string, ScriptAnalysisResult> scriptAnalysis,
            out Dictionary<string, DependencyAnalysisResult> dependencyAnalysis)
        {
            folderToScripts = new Dictionary<string, List<string>>();
            scriptAnalysis = new Dictionary<string, ScriptAnalysisResult>();
            dependencyAnalysis = new Dictionary<string, DependencyAnalysisResult>();

            // 1. Collect all scripts
            List<string> scripts = Directory.GetFiles(ScriptsRoot, "*.cs", SearchOption.AllDirectories)
                                            .Select(p => p.Replace("\\", "/"))
                                            .ToList();

            // 2. Build folder → script mapping
            foreach (string scriptPath in scripts)
            {
                string relative = scriptPath.Replace(ScriptsRoot, "").TrimStart('/');
                string folder = Path.GetDirectoryName(relative).Replace("\\", "/");
                string file = Path.GetFileName(scriptPath);

                if (!folderToScripts.ContainsKey(folder))
                    folderToScripts[folder] = new List<string>();

                folderToScripts[folder].Add(file);
            }

            // 3. Load analysis files
            foreach (string scriptPath in scripts)
            {
                string relative = scriptPath.Replace(ScriptsRoot, "").TrimStart('/');
                string folder = Path.GetDirectoryName(relative).Replace("\\", "/");
                string scriptName = Path.GetFileNameWithoutExtension(scriptPath);

                string analysisPath = Path.Combine(AnalysisRoot, folder, scriptName + "_Analysis.md")
                                      .Replace("\\", "/");

                if (File.Exists(analysisPath))
                {
                    // Load the analysis file and parse it into summary objects
                    ScriptAnalysisResult s = ParseScriptAnalysis(analysisPath);
                    DependencyAnalysisResult d = ParseDependencyAnalysis(analysisPath);

                    scriptAnalysis[scriptPath] = s;
                    dependencyAnalysis[scriptPath] = d;
                }
            }
        }

        // ---------------------------------------------------------
        // PARSING ANALYSIS FILES
        // ---------------------------------------------------------

        private static ScriptAnalysisResult ParseScriptAnalysis(string path)
        {
            ScriptAnalysisResult result = new ScriptAnalysisResult();
            result.ScriptPath = path;

            string[] lines = File.ReadAllLines(path);

            List<string> currentList = null;

            foreach (string line in lines)
            {
                if (line.StartsWith("## Classes")) currentList = result.ClassNames;
                else if (line.StartsWith("## Base Classes")) currentList = result.BaseClasses;
                else if (line.StartsWith("## Interfaces")) currentList = result.Interfaces;
                else if (line.StartsWith("## Methods")) currentList = result.Methods;
                else if (line.StartsWith("## Properties")) currentList = result.Properties;
                else if (line.StartsWith("## Events")) currentList = result.Events;
                else if (line.StartsWith("## Fields")) currentList = result.Fields;
                else if (line.StartsWith("## Serialized Fields")) currentList = result.SerializedFields;
                else if (line.StartsWith("## Attributes")) currentList = result.Attributes;
                else if (line.StartsWith("## Namespace"))
                {
                    // Next line is namespace
                    int idx = System.Array.IndexOf(lines, line);
                    if (idx + 1 < lines.Length)
                        result.Namespace = lines[idx + 1].Trim();
                }
                else if (line.StartsWith("- ") && currentList != null)
                {
                    string item = line.Substring(2).Trim();
                    if (item != "None")
                        currentList.Add(item);
                }
            }

            return result;
        }

        private static DependencyAnalysisResult ParseDependencyAnalysis(string path)
        {
            DependencyAnalysisResult result = new DependencyAnalysisResult();
            result.ScriptPath = path;

            string[] lines = File.ReadAllLines(path);

            List<string> currentList = null;

            foreach (string line in lines)
            {
                if (line.StartsWith("### Using Namespaces")) currentList = result.UsingNamespaces;
                else if (line.StartsWith("### Type References")) currentList = result.TypeReferences;
                else if (line.StartsWith("### GetComponent")) currentList = result.GetComponentTypes;
                else if (line.StartsWith("### RequireComponent")) currentList = result.RequireComponentTypes;
                else if (line.StartsWith("### Event Subscriptions")) currentList = result.EventSubscriptions;
                else if (line.StartsWith("### Attribute Types")) currentList = result.AttributeTypes;
                else if (line.StartsWith("- ") && currentList != null)
                {
                    string item = line.Substring(2).Trim();
                    if (item != "None")
                        currentList.Add(item);
                }
            }

            return result;
        }

        // ---------------------------------------------------------
        // HELPERS
        // ---------------------------------------------------------

        public static string GetFullScriptPath(string folder, string scriptName)
        {
            return Path.Combine(ScriptsRoot, folder, scriptName).Replace("\\", "/");
        }

        public static void OpenScript(string fullPath)
        {
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(fullPath);
            if (asset != null)
            {
                Selection.activeObject = asset;
                EditorGUIUtility.PingObject(asset);
            }
        }

        public static void OpenAnalysisFile(string fullScriptPath)
        {
            string relative = fullScriptPath.Replace(ScriptsRoot, "").TrimStart('/');
            string folder = Path.GetDirectoryName(relative).Replace("\\", "/");
            string scriptName = Path.GetFileNameWithoutExtension(fullScriptPath);

            string analysisPath = Path.Combine(AnalysisRoot, folder, scriptName + "_Analysis.md")
                                  .Replace("\\", "/");

            Object asset = AssetDatabase.LoadAssetAtPath<Object>(analysisPath);
            if (asset != null)
            {
                Selection.activeObject = asset;
                EditorGUIUtility.PingObject(asset);
            }
        }
    }
}