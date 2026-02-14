using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace CubeWarsTools.ScriptDocumentation
{
    public static class ScriptDocumentationExporter
    {
        private const string ScriptsRoot = "Assets/Scripts";
        private static readonly string OutputFolder = EditorToolsPaths.Exports;
        private static readonly string OutputFile = EditorToolsPaths.Exports + "/ScriptDatabase.md";

        private class ScriptInfo
        {
            public string Name;
            public string Path;
            public string RelativePath;
            public string FolderGroup;          // Core, Data, Runtime, Systems, etc.
            public string SystemClassification; // Buildings, AI, Combat, etc.
            public string BaseClass;
            public List<string> Interfaces = new List<string>();
            public string Code;

            public List<string> SerializedFields = new List<string>();
            public List<string> PublicMethods = new List<string>();
            public List<string> PrivateMethods = new List<string>();
            public List<string> Properties = new List<string>();
            public List<string> Events = new List<string>();
            public List<string> Coroutines = new List<string>();

            public List<string> Comments = new List<string>();
            public List<string> XmlDocs = new List<string>();
            public List<string> Regions = new List<string>();

            public HashSet<string> Keywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            public List<string> UsesScripts = new List<string>();
            public List<string> UsedByScripts = new List<string>();

            public string Summary;
        }

        [MenuItem("Tools/CubeWars/Documentation/Export Script Database")]
        public static void Generate()
        {
            try
            {
                EditorUtility.DisplayProgressBar("Script Documentation", "Scanning scripts...", 0.1f);

                EnsureOutputFolder();

                var scripts = LoadScripts();
                EditorUtility.DisplayProgressBar("Script Documentation", "Parsing scripts...", 0.3f);

                foreach (var script in scripts)
                {
                    ParseStructure(script);
                    ExtractCommentsAndRegions(script);
                    InferSystemClassification(script);
                    GenerateKeywords(script);
                }

                EditorUtility.DisplayProgressBar("Script Documentation", "Building cross references...", 0.5f);
                BuildCrossReferences(scripts);

                EditorUtility.DisplayProgressBar("Script Documentation", "Generating summaries...", 0.6f);
                foreach (var script in scripts)
                {
                    script.Summary = GenerateSummary(script);
                }

                EditorUtility.DisplayProgressBar("Script Documentation", "Writing Markdown...", 0.8f);
                var markdown = BuildMarkdownDocument(scripts);

                File.WriteAllText(OutputFile, markdown, Encoding.UTF8);
                AssetDatabase.Refresh();

                EditorUtility.ClearProgressBar();
                Debug.Log($"[ScriptDocumentationExporter] Documentation generated at: {OutputFile}");
            }
            catch (Exception ex)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError("[ScriptDocumentationExporter] Error: " + ex);
            }
        }

        private static void EnsureOutputFolder()
        {
            EditorToolsPaths.EnsureFolder(OutputFolder);
        }

        private static List<ScriptInfo> LoadScripts()
        {
            var result = new List<ScriptInfo>();

            if (!Directory.Exists(ScriptsRoot))
            {
                Debug.LogWarning($"[ScriptDocumentationExporter] Scripts root not found: {ScriptsRoot}");
                return result;
            }

            var files = Directory.GetFiles(ScriptsRoot, "*.cs", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var assetPath = file.Replace("\\", "/");
                var relative = assetPath;

                var code = File.ReadAllText(assetPath);

                var name = Path.GetFileNameWithoutExtension(assetPath);

                var group = InferFolderGroup(relative);

                result.Add(new ScriptInfo
                {
                    Name = name,
                    Path = assetPath,
                    RelativePath = relative,
                    FolderGroup = group,
                    Code = code
                });
            }

            return result;
        }

        private static string InferFolderGroup(string path)
        {
            // Expecting: Assets/Scripts/<Group>/...
            var parts = path.Split('/');
            if (parts.Length < 3) return "Ungrouped";
            if (!string.Equals(parts[0], "Assets", StringComparison.OrdinalIgnoreCase)) return "Ungrouped";
            if (!string.Equals(parts[1], "Scripts", StringComparison.OrdinalIgnoreCase)) return "Ungrouped";

            var group = parts[2];

            // Normalize to known groups
            switch (group.ToLowerInvariant())
            {
                case "core": return "Core";
                case "data": return "Data";
                case "documents": return "Documents";
                case "runtime": return "Runtime";
                case "systems": return "Systems";
                case "ui": return "UI";
                case "utilities": return "Utilities";
                default: return "Ungrouped";
            }
        }

        private static void ParseStructure(ScriptInfo script)
        {
            var code = script.Code;

            // Base class and interfaces
            var classMatch = Regex.Match(code, @"class\s+(\w+)\s*(?::\s*([^{\n]+))?", RegexOptions.Multiline);
            if (classMatch.Success)
            {
                var inheritance = classMatch.Groups[2].Value;
                if (!string.IsNullOrEmpty(inheritance))
                {
                    var parts = inheritance.Split(',')
                        .Select(p => p.Trim())
                        .Where(p => !string.IsNullOrEmpty(p))
                        .ToList();

                    if (parts.Count > 0)
                    {
                        script.BaseClass = parts[0];
                        if (parts.Count > 1)
                        {
                            for (int i = 1; i < parts.Count; i++)
                                script.Interfaces.Add(parts[i]);
                        }
                    }
                }
            }

            // Serialized fields
            foreach (Match m in Regex.Matches(code, @"\[SerializeField\][^\n]*\n\s*(.+?);"))
            {
                var line = m.Groups[1].Value.Trim();
                script.SerializedFields.Add(line);
            }

            // Properties
            foreach (Match m in Regex.Matches(code, @"\b(\w+)\s+(\w+)\s*\{\s*get;"))
            {
                script.Properties.Add(m.Value.Trim());
            }

            // Events
            foreach (Match m in Regex.Matches(code, @"event\s+[\w<>\[\]]+\s+\w+;"))
            {
                script.Events.Add(m.Value.Trim());
            }

            // Methods (public/private, including coroutines)
            foreach (Match m in Regex.Matches(code, @"\b(public|private|protected|internal)\s+([\w<>\[\]]+\s+)?(\w+)\s*\(([^)]*)\)\s*\{", RegexOptions.Multiline))
            {
                var access = m.Groups[1].Value;
                var returnAndName = m.Groups[0].Value.Trim();

                var methodName = m.Groups[3].Value;
                var returnType = m.Groups[2].Value.Trim();

                var isCoroutine = returnType.Contains("IEnumerator");

                // Extract first few lines of body + representative line
                var bodyExcerpt = ExtractMethodExcerpt(code, m.Index);

                if (isCoroutine)
                {
                    script.Coroutines.Add(bodyExcerpt);
                }
                else if (access == "public" || access == "protected")
                {
                    script.PublicMethods.Add(bodyExcerpt);
                }
                else
                {
                    script.PrivateMethods.Add(bodyExcerpt);
                }
            }
        }

        private static string ExtractMethodExcerpt(string code, int startIndex)
        {
            // Find opening brace
            var braceIndex = code.IndexOf('{', startIndex);
            if (braceIndex < 0) return string.Empty;

            int depth = 0;
            int i = braceIndex;
            var lines = new List<string>();
            var currentLine = new StringBuilder();

            bool started = false;
            string representativeLine = null;

            for (; i < code.Length; i++)
            {
                char c = code[i];
                currentLine.Append(c);

                if (c == '{')
                {
                    depth++;
                    started = true;
                }
                else if (c == '}')
                {
                    depth--;
                    if (depth <= 0)
                    {
                        // End of method
                        lines.Add(currentLine.ToString());
                        break;
                    }
                }

                if (c == '\n')
                {
                    var line = currentLine.ToString();
                    lines.Add(line);
                    currentLine.Length = 0;

                    // Try to pick a representative line (contains keywords or meaningful identifiers)
                    if (representativeLine == null)
                    {
                        if (IsRepresentativeLine(line))
                            representativeLine = line.TrimEnd();
                    }

                    // Limit to first ~8 lines
                    if (lines.Count >= 8 && depth <= 1)
                        break;
                }
            }

            if (representativeLine != null && !lines.Contains(representativeLine))
            {
                lines.Add("// Representative: " + representativeLine.Trim());
            }

            return string.Join("", lines).Trim();
        }

        private static bool IsRepresentativeLine(string line)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed)) return false;
            if (trimmed.StartsWith("//")) return false;
            if (trimmed.StartsWith("/*")) return false;
            if (trimmed.StartsWith("{") || trimmed.StartsWith("}")) return false;
            if (trimmed.Length < 10) return false;

            // Heuristic: contains method calls, conditions, or assignments
            if (trimmed.Contains("(") && trimmed.Contains(")")) return true;
            if (trimmed.Contains("=")) return true;
            if (trimmed.Contains("if ") || trimmed.Contains("for ") || trimmed.Contains("while ")) return true;

            return false;
        }

        private static void ExtractCommentsAndRegions(ScriptInfo script)
        {
            var code = script.Code;

            foreach (Match m in Regex.Matches(code, @"//(.*)"))
            {
                var comment = m.Groups[1].Value.Trim();
                if (!string.IsNullOrEmpty(comment))
                    script.Comments.Add(comment);
            }

            foreach (Match m in Regex.Matches(code, @"///(.*)"))
            {
                var xml = m.Groups[1].Value.Trim();
                if (!string.IsNullOrEmpty(xml))
                    script.XmlDocs.Add(xml);
            }

            foreach (Match m in Regex.Matches(code, @"#region\s+(.*)"))
            {
                var region = m.Groups[1].Value.Trim();
                if (!string.IsNullOrEmpty(region))
                    script.Regions.Add(region);
            }
        }

        private static void InferSystemClassification(ScriptInfo script)
        {
            // Very simple heuristic based on path and name
            var pathLower = script.Path.ToLowerInvariant();
            var nameLower = script.Name.ToLowerInvariant();

            string system = null;

            if (pathLower.Contains("/buildings/") || nameLower.Contains("building") || nameLower.Contains("barracks") || nameLower.Contains("turret") || nameLower.Contains("factory") || nameLower.Contains("headquarters"))
                system = "Buildings";
            else if (pathLower.Contains("/ai/") || nameLower.StartsWith("ai"))
                system = "AI";
            else if (pathLower.Contains("/combat/") || nameLower.Contains("projectile") || nameLower.Contains("weapon") || nameLower.Contains("attack"))
                system = "Combat";
            else if (pathLower.Contains("/units/") || nameLower.Contains("unit") || nameLower.Contains("civilian"))
                system = "Units";
            else if (pathLower.Contains("/ui/") || nameLower.Contains("ui") || nameLower.Contains("menu") || nameLower.Contains("hud"))
                system = "UI";
            else if (pathLower.Contains("/managers/") || nameLower.Contains("manager"))
                system = "Managers";
            else if (pathLower.Contains("/resources/") || nameLower.Contains("resource"))
                system = "Resources";
            else if (pathLower.Contains("/utilities/") || pathLower.Contains("/helpers/") || nameLower.Contains("util"))
                system = "Utilities";
            else if (pathLower.Contains("/data/"))
                system = "Data";
            else if (pathLower.Contains("/core/"))
                system = "Core";
            else
                system = "Unclassified";

            script.SystemClassification = system;
        }

        private static void GenerateKeywords(ScriptInfo script)
        {
            void AddWords(string text)
            {
                if (string.IsNullOrEmpty(text)) return;
                var words = Regex.Split(text, @"[^A-Za-z0-9_]+");
                foreach (var w in words)
                {
                    var word = w.Trim();
                    if (word.Length < 3) continue;
                    script.Keywords.Add(word);
                }
            }

            AddWords(script.Name);
            AddWords(script.BaseClass);
            foreach (var i in script.Interfaces) AddWords(i);
            foreach (var f in script.SerializedFields) AddWords(f);
            foreach (var m in script.PublicMethods) AddWords(m);
            foreach (var m in script.PrivateMethods) AddWords(m);
            foreach (var p in script.Properties) AddWords(p);
            foreach (var e in script.Events) AddWords(e);
            foreach (var c in script.Comments) AddWords(c);
            foreach (var x in script.XmlDocs) AddWords(x);
            foreach (var r in script.Regions) AddWords(r);
        }

        private static void BuildCrossReferences(List<ScriptInfo> scripts)
        {
            var nameToScript = scripts.ToDictionary(s => s.Name, s => s, StringComparer.OrdinalIgnoreCase);

            foreach (var script in scripts)
            {
                var code = script.Code;

                foreach (var other in scripts)
                {
                    if (other == script) continue;

                    // Simple heuristic: if code mentions other script name as a word
                    var pattern = @"\b" + Regex.Escape(other.Name) + @"\b";
                    if (Regex.IsMatch(code, pattern))
                    {
                        if (!script.UsesScripts.Contains(other.Name))
                            script.UsesScripts.Add(other.Name);
                    }
                }
            }

            // Reverse mapping
            foreach (var script in scripts)
            {
                foreach (var used in script.UsesScripts)
                {
                    if (nameToScript.TryGetValue(used, out var target))
                    {
                        if (!target.UsedByScripts.Contains(script.Name))
                            target.UsedByScripts.Add(script.Name);
                    }
                }
            }
        }

        private static string GenerateSummary(ScriptInfo script)
        {
            var sb = new StringBuilder();

            // Base classification sentence
            sb.Append(script.Name);
            sb.Append(" appears to be a ");

            if (!string.IsNullOrEmpty(script.SystemClassification) && script.SystemClassification != "Unclassified")
            {
                sb.Append(script.SystemClassification.ToLowerInvariant());
                sb.Append(" script");
            }
            else
            {
                sb.Append("script");
            }

            if (!string.IsNullOrEmpty(script.BaseClass))
            {
                sb.Append(" derived from ");
                sb.Append(script.BaseClass);
            }

            sb.Append(".");

            // Mention key responsibilities based on keywords
            var keyWords = script.Keywords
                .Where(k => k.Length > 4)
                .Take(8)
                .ToList();

            if (keyWords.Count > 0)
            {
                sb.Append(" It appears to be related to: ");
                sb.Append(string.Join(", ", keyWords));
                sb.Append(".");
            }

            // Mention overlaps
            var overlaps = new List<string>();
            if (script.SystemClassification == "Buildings")
            {
                if (script.Name.IndexOf("factory", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    script.Name.IndexOf("barracks", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    overlaps.Add("other production buildings such as Barracks, VehicleFactory, or WeaponsFactory");
                }
                if (script.Name.IndexOf("turret", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    overlaps.Add("other defensive buildings such as Turret or DefenseTurret");
                }
            }

            if (overlaps.Count > 0)
            {
                sb.Append(" It may share responsibilities with ");
                sb.Append(string.Join(" and ", overlaps));
                sb.Append(".");
            }

            return sb.ToString();
        }

        private static string BuildMarkdownDocument(List<ScriptInfo> scripts)
        {
            var sb = new StringBuilder();

            sb.AppendLine("# CUBE WARS SCRIPT DATABASE");
            sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();
            sb.AppendLine("## Table of Contents");

            var groups = scripts
                .GroupBy(s => s.FolderGroup)
                .OrderBy(g => g.Key);

            foreach (var group in groups)
            {
                var anchor = ToAnchor(group.Key);
                sb.AppendLine($"- [{group.Key}](#{anchor})");
            }

            sb.AppendLine();

            foreach (var group in groups)
            {
                sb.AppendLine($"# {group.Key}");
                sb.AppendLine();

                var ordered = group
                    .OrderBy(s => s.RelativePath, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                foreach (var script in ordered)
                {
                    AppendScriptSection(sb, script);
                }
            }

            return sb.ToString();
        }

        private static void AppendScriptSection(StringBuilder sb, ScriptInfo script)
        {
            sb.AppendLine($"## {script.RelativePath}");
            sb.AppendLine($"**Script:** {script.Name}");
            sb.AppendLine($"**System Classification:** {script.SystemClassification}");
            sb.AppendLine($"**Base Class:** {(string.IsNullOrEmpty(script.BaseClass) ? "-" : script.BaseClass)}");
            sb.AppendLine($"**Interfaces:** {(script.Interfaces.Count == 0 ? "-" : string.Join(", ", script.Interfaces))}");
            sb.AppendLine();

            if (!string.IsNullOrEmpty(script.Summary))
            {
                sb.AppendLine("### Summary");
                sb.AppendLine(script.Summary);
                sb.AppendLine();
            }

            if (script.SerializedFields.Count > 0)
            {
                sb.AppendLine("### Serialized Fields");
                sb.AppendLine("```csharp");
                foreach (var f in script.SerializedFields)
                    sb.AppendLine(f);
                sb.AppendLine("```");
                sb.AppendLine();
            }

            if (script.Properties.Count > 0)
            {
                sb.AppendLine("### Properties");
                sb.AppendLine("```csharp");
                foreach (var p in script.Properties)
                    sb.AppendLine(p);
                sb.AppendLine("```");
                sb.AppendLine();
            }

            if (script.Events.Count > 0)
            {
                sb.AppendLine("### Events");
                sb.AppendLine("```csharp");
                foreach (var e in script.Events)
                    sb.AppendLine(e);
                sb.AppendLine("```");
                sb.AppendLine();
            }

            if (script.PublicMethods.Count > 0)
            {
                sb.AppendLine("### Public Methods (excerpts)");
                sb.AppendLine("```csharp");
                foreach (var m in script.PublicMethods)
                {
                    sb.AppendLine(m);
                    sb.AppendLine();
                }
                sb.AppendLine("```");
                sb.AppendLine();
            }

            if (script.PrivateMethods.Count > 0)
            {
                sb.AppendLine("### Private Methods (excerpts)");
                sb.AppendLine("```csharp");
                foreach (var m in script.PrivateMethods)
                {
                    sb.AppendLine(m);
                    sb.AppendLine();
                }
                sb.AppendLine("```");
                sb.AppendLine();
            }

            if (script.Coroutines.Count > 0)
            {
                sb.AppendLine("### Coroutines (excerpts)");
                sb.AppendLine("```csharp");
                foreach (var c in script.Coroutines)
                {
                    sb.AppendLine(c);
                    sb.AppendLine();
                }
                sb.AppendLine("```");
                sb.AppendLine();
            }

            if (script.Comments.Count > 0 || script.XmlDocs.Count > 0)
            {
                sb.AppendLine("### Comments & Documentation");
                foreach (var c in script.Comments.Take(10))
                    sb.AppendLine($"- // {c}");
                foreach (var x in script.XmlDocs.Take(10))
                    sb.AppendLine($"- /// {x}");
                sb.AppendLine();
            }

            if (script.Keywords.Count > 0)
            {
                sb.AppendLine("### Keywords");
                sb.AppendLine(string.Join(", ", script.Keywords.OrderBy(k => k)));
                sb.AppendLine();
            }

            if (script.UsesScripts.Count > 0 || script.UsedByScripts.Count > 0)
            {
                sb.AppendLine("### Cross References");
                if (script.UsesScripts.Count > 0)
                    sb.AppendLine("**Uses:** " + string.Join(", ", script.UsesScripts.OrderBy(n => n)));
                else
                    sb.AppendLine("**Uses:** -");

                if (script.UsedByScripts.Count > 0)
                    sb.AppendLine("**Used By:** " + string.Join(", ", script.UsedByScripts.OrderBy(n => n)));
                else
                    sb.AppendLine("**Used By:** -");

                sb.AppendLine();
            }

            // Blank line separation between scripts
            sb.AppendLine();
        }

        private static string ToAnchor(string heading)
        {
            if (string.IsNullOrEmpty(heading)) return "";
            var lower = heading.Trim().ToLowerInvariant();
            lower = Regex.Replace(lower, @"[^\w\- ]+", "");
            lower = lower.Replace(' ', '-');
            return lower;
        }
    }
}