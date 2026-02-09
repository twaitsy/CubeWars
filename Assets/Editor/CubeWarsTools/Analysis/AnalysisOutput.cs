using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace CubeWarsTools.Analysis
{
    /// <summary>
    /// Writes analysis results to disk.
    /// Mirrors the folder structure of Assets/Scripts under Assets/Docs/Analysis.
    /// Generates:
    /// - Per-script analysis markdown files
    /// - A master analysis index
    /// </summary>
    public static class AnalysisOutput
    {
        private static readonly string ScriptsRoot = "Assets/Scripts";
        private static readonly string AnalysisRoot = "Assets/Docs/Analysis";

        /// <summary>
        /// Writes all analysis results to disk.
        /// </summary>
        public static void WriteAllAnalysis(
            Dictionary<string, ScriptAnalysisResult> scriptAnalysis,
            Dictionary<string, DependencyAnalysisResult> dependencyAnalysis)
        {
            EnsureRootFolder();

            foreach (var kvp in scriptAnalysis)
            {
                string scriptPath = kvp.Key;
                ScriptAnalysisResult scriptData = kvp.Value;
                DependencyAnalysisResult depData = dependencyAnalysis.ContainsKey(scriptPath)
                    ? dependencyAnalysis[scriptPath]
                    : new DependencyAnalysisResult();

                WriteSingleAnalysis(scriptPath, scriptData, depData);
            }

            WriteMasterIndex(scriptAnalysis.Keys.ToList());
            AssetDatabase.Refresh();
        }

        // ---------------------------------------------------------
        // WRITE SINGLE ANALYSIS FILE
        // ---------------------------------------------------------

        private static void WriteSingleAnalysis(
            string scriptPath,
            ScriptAnalysisResult script,
            DependencyAnalysisResult deps)
        {
            string relative = scriptPath.Replace(ScriptsRoot, "").TrimStart('/');
            string folder = Path.GetDirectoryName(relative).Replace("\\", "/");
            string fileName = Path.GetFileNameWithoutExtension(scriptPath) + "_Analysis.md";

            string outputDir = Path.Combine(AnalysisRoot, folder).Replace("\\", "/");
            Directory.CreateDirectory(outputDir);

            string outputPath = Path.Combine(outputDir, fileName);

            string md = BuildMarkdown(script, deps);
            File.WriteAllText(outputPath, md);
        }

        // ---------------------------------------------------------
        // BUILD MARKDOWN CONTENT
        // ---------------------------------------------------------

        private static string BuildMarkdown(
            ScriptAnalysisResult script,
            DependencyAnalysisResult deps)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"# Analysis — {Path.GetFileName(script.ScriptPath)}");
            sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();

            sb.AppendLine("## Namespace");
            sb.AppendLine(script.Namespace);
            sb.AppendLine();

            sb.AppendLine("## Classes");
            AppendList(sb, script.ClassNames);

            sb.AppendLine("## Base Classes");
            AppendList(sb, script.BaseClasses);

            sb.AppendLine("## Interfaces");
            AppendList(sb, script.Interfaces);

            sb.AppendLine("## Methods");
            AppendList(sb, script.Methods);

            sb.AppendLine("## Properties");
            AppendList(sb, script.Properties);

            sb.AppendLine("## Events");
            AppendList(sb, script.Events);

            sb.AppendLine("## Fields");
            AppendList(sb, script.Fields);

            sb.AppendLine("## Serialized Fields");
            AppendList(sb, script.SerializedFields);

            sb.AppendLine("## Attributes");
            AppendList(sb, script.Attributes);

            sb.AppendLine("## Dependencies");
            sb.AppendLine();

            sb.AppendLine("### Using Namespaces");
            AppendList(sb, deps.UsingNamespaces);

            sb.AppendLine("### Type References");
            AppendList(sb, deps.TypeReferences);

            sb.AppendLine("### GetComponent<T>()");
            AppendList(sb, deps.GetComponentTypes);

            sb.AppendLine("### RequireComponent");
            AppendList(sb, deps.RequireComponentTypes);

            sb.AppendLine("### Event Subscriptions");
            AppendList(sb, deps.EventSubscriptions);

            sb.AppendLine("### Attribute Types");
            AppendList(sb, deps.AttributeTypes);

            return sb.ToString();
        }

        private static void AppendList(StringBuilder sb, List<string> items)
        {
            if (items == null || items.Count == 0)
            {
                sb.AppendLine("- None");
                sb.AppendLine();
                return;
            }

            foreach (var item in items.Distinct().OrderBy(i => i))
                sb.AppendLine($"- {item}");

            sb.AppendLine();
        }

        // ---------------------------------------------------------
        // MASTER INDEX
        // ---------------------------------------------------------

        private static void WriteMasterIndex(List<string> scriptPaths)
        {
            string indexPath = Path.Combine(AnalysisRoot, "Analysis_Index.md");
            var sb = new StringBuilder();

            sb.AppendLine("# Cube Wars — Analysis Index");
            sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();

            foreach (var path in scriptPaths.OrderBy(p => p))
            {
                string relative = path.Replace(ScriptsRoot, "").TrimStart('/');
                string folder = Path.GetDirectoryName(relative).Replace("\\", "/");
                string scriptName = Path.GetFileNameWithoutExtension(path);

                string analysisPath = $"{folder}/{scriptName}_Analysis.md";

                sb.AppendLine($"- [{scriptName}]({analysisPath})");
            }

            File.WriteAllText(indexPath, sb.ToString());
        }

        // ---------------------------------------------------------
        // FOLDER SETUP
        // ---------------------------------------------------------

        private static void EnsureRootFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Docs"))
                AssetDatabase.CreateFolder("Assets", "Docs");

            if (!AssetDatabase.IsValidFolder(AnalysisRoot))
                AssetDatabase.CreateFolder("Assets/Docs", "Analysis");
        }
    }
}