using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using CubeWarsTools.Analysis;

namespace CubeWarsTools.Core
{
    /// <summary>
    /// Integrates Phase 1 + Phase 2.
    /// Generates:
    /// - Script Index (Markdown + Text)
    /// - Full Analysis (per script)
    /// - Mirrored folder structure under Assets/Docs/Analysis
    /// </summary>
    public static class ScriptIndexWithAnalysis
    {
        private static readonly string ScriptsRoot = "Assets/Scripts";

        [MenuItem("Tools/CubeWars/Documentation/Generate Script Index With Analysis")]
        public static void GenerateWithAnalysis()
        {
            Debug.Log("Cube Wars: Starting full index + analysis generation...");

            // 1. Generate the normal script index
            ScriptIndexGenerator.Generate();

            // 2. Collect all script paths
            List<string> scripts = new List<string>(
                Directory.GetFiles(ScriptsRoot, "*.cs", SearchOption.AllDirectories)
            );

            // 3. Run analysis on each script
            Dictionary<string, ScriptAnalysisResult> scriptAnalysis =
                new Dictionary<string, ScriptAnalysisResult>();

            Dictionary<string, DependencyAnalysisResult> dependencyAnalysis =
                new Dictionary<string, DependencyAnalysisResult>();

            foreach (string scriptPath in scripts)
            {
                string normalized = scriptPath.Replace("\\", "/");

                ScriptAnalysisResult s = ClassMethodExtractor.AnalyzeScript(normalized);
                DependencyAnalysisResult d = DependencyScanner.AnalyzeDependencies(normalized);

                scriptAnalysis[normalized] = s;
                dependencyAnalysis[normalized] = d;
            }

            // 4. Write analysis output
            AnalysisOutput.WriteAllAnalysis(scriptAnalysis, dependencyAnalysis);

            Debug.Log("Cube Wars: Full index + analysis generation complete.");
        }
    }
}