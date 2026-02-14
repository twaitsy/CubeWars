using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace CubeWarsTools.Core
{
    public static class ScriptIndexGenerator
    {
        private static readonly string ScriptsRoot = "Assets/Scripts";
        private static readonly string DocsRoot = EditorToolsPaths.ScriptIndex;

        private static readonly string[] SystemGroups = new[]
        {
            "Core",
            "Systems",
            "Runtime",
            "Data",
            "Managers",
            "Utilities",
            "Documents",
            "UI"
        };

        [MenuItem("Tools/CubeWars/Documentation/Generate Script Index")]
        public static void Generate()
        {
            EnsureDocsFolder();

            // 1. Scan all scripts
            var allScripts = Directory.GetFiles(ScriptsRoot, "*.cs", SearchOption.AllDirectories)
                                      .Select(path => path.Replace("\\", "/"))
                                      .ToList();

            // 2. Group by folder
            var folderGroups = GroupByFolder(allScripts);

            // 3. Group folders into system categories
            var systemGroups = GroupBySystemCategory(folderGroups);

            // 4. Format output
            string markdown = ScriptIndexFormatter.FormatMarkdown(systemGroups);
            string text = ScriptIndexFormatter.FormatText(systemGroups);

            // 5. Write master files
            File.WriteAllText(Path.Combine(DocsRoot, "ScriptIndex.md"), markdown);
            File.WriteAllText(Path.Combine(DocsRoot, "ScriptIndex.txt"), text);

            // 6. Write system‑specific files
            WriteSystemSpecificIndexes(systemGroups);

            AssetDatabase.Refresh();
            Debug.Log("Cube Wars Script Index generated successfully.");
        }

        private static Dictionary<string, List<string>> GroupByFolder(List<string> scripts)
        {
            var result = new Dictionary<string, List<string>>();

            foreach (var scriptPath in scripts)
            {
                string folder = Path.GetDirectoryName(scriptPath).Replace("\\", "/");
                string file = Path.GetFileName(scriptPath);

                if (!result.ContainsKey(folder))
                    result[folder] = new List<string>();

                result[folder].Add(file);
            }

            return result;
        }

        private static Dictionary<string, Dictionary<string, List<string>>> GroupBySystemCategory(
            Dictionary<string, List<string>> folderGroups)
        {
            var result = new Dictionary<string, Dictionary<string, List<string>>>();

            // Initialize groups
            foreach (var group in SystemGroups)
                result[group] = new Dictionary<string, List<string>>();

            result["Ungrouped"] = new Dictionary<string, List<string>>();

            // Assign folders to groups
            foreach (var kvp in folderGroups)
            {
                string folder = kvp.Key;
                string assignedGroup = "Ungrouped";

                foreach (var group in SystemGroups)
                {
                    if (folder.Contains($"/{group}"))
                    {
                        assignedGroup = group;
                        break;
                    }
                }

                if (!result[assignedGroup].ContainsKey(folder))
                    result[assignedGroup][folder] = new List<string>();

                result[assignedGroup][folder].AddRange(kvp.Value);
            }

            return result;
        }

        private static void WriteSystemSpecificIndexes(
            Dictionary<string, Dictionary<string, List<string>>> systemGroups)
        {
            foreach (var group in systemGroups)
            {
                string groupName = group.Key;
                string mdPath = Path.Combine(DocsRoot, $"Index_{groupName}.md");
                string txtPath = Path.Combine(DocsRoot, $"Index_{groupName}.txt");

                string md = ScriptIndexFormatter.FormatMarkdownForGroup(groupName, group.Value);
                string txt = ScriptIndexFormatter.FormatTextForGroup(groupName, group.Value);

                File.WriteAllText(mdPath, md);
                File.WriteAllText(txtPath, txt);
            }
        }

        private static void EnsureDocsFolder()
        {
            EditorToolsPaths.EnsureFolder(DocsRoot);
        }
    }
}