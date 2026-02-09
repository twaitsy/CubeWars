using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CubeWarsTools.Analysis
{
    /// <summary>
    /// Medium-depth dependency scanner.
    /// Detects:
    /// - using statements
    /// - type references in fields, properties, methods
    /// - serialized dependencies
    /// - GetComponent<T>()
    /// - RequireComponent attributes
    /// - Event subscriptions
    /// - Attribute-based dependencies
    /// </summary>
    public static class DependencyScanner
    {
        // -------------------------
        // REGEX PATTERNS
        // -------------------------

        private static readonly Regex UsingRegex =
            new Regex(@"using\s+([A-Za-z0-9_.]+)\s*;", RegexOptions.Compiled);

        private static readonly Regex TypeReferenceRegex =
            new Regex(@"[A-Za-z0-9_<>]+\s+[A-Za-z0-9_]+\s*(?:=|;|\()", RegexOptions.Compiled);

        private static readonly Regex GetComponentRegex =
            new Regex(@"GetComponent<([A-Za-z0-9_<>]+)>", RegexOptions.Compiled);

        private static readonly Regex RequireComponentRegex =
            new Regex(@"\[RequireComponent\s*\(\s*typeof\(([^)]+)\)\s*\)\]",
                      RegexOptions.Compiled);

        private static readonly Regex EventSubscriptionRegex =
            new Regex(@"([A-Za-z0-9_]+)\s*\+=\s*", RegexOptions.Compiled);

        private static readonly Regex AttributeTypeRegex =
            new Regex(@"\[([A-Za-z0-9_]+)(?:\(.+?\))?\]", RegexOptions.Compiled);

        // -------------------------
        // MAIN ENTRY POINT
        // -------------------------

        public static DependencyAnalysisResult AnalyzeDependencies(string scriptPath)
        {
            if (!File.Exists(scriptPath))
            {
                Debug.LogWarning($"DependencyScanner: File not found: {scriptPath}");
                return new DependencyAnalysisResult();
            }

            string code = File.ReadAllText(scriptPath);

            DependencyAnalysisResult result = new DependencyAnalysisResult();
            result.ScriptPath = scriptPath;
            result.UsingNamespaces = ExtractMatches(UsingRegex, code);
            result.TypeReferences = ExtractTypeReferences(code);
            result.GetComponentTypes = ExtractMatches(GetComponentRegex, code);
            result.RequireComponentTypes = ExtractMatches(RequireComponentRegex, code);
            result.EventSubscriptions = ExtractMatches(EventSubscriptionRegex, code);
            result.AttributeTypes = ExtractMatches(AttributeTypeRegex, code);

            return result;
        }

        // -------------------------
        // HELPERS
        // -------------------------

        private static List<string> ExtractMatches(Regex regex, string input)
        {
            List<string> list = new List<string>();
            MatchCollection matches = regex.Matches(input);

            foreach (Match match in matches)
            {
                if (match.Groups.Count > 1)
                    list.Add(match.Groups[1].Value.Trim());
            }

            return list;
        }

        private static List<string> ExtractTypeReferences(string code)
        {
            List<string> list = new List<string>();
            MatchCollection matches = TypeReferenceRegex.Matches(code);

            foreach (Match match in matches)
            {
                string segment = match.Value.Trim();

                string[] parts = segment.Split(
                    new[] { ' ', '\t', '\r', '\n' },
                    StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length > 0)
                {
                    string type = parts[0].Trim();
                    if (!string.IsNullOrWhiteSpace(type))
                        list.Add(type);
                }
            }

            return list;
        }
    }

    /// <summary>
    /// Data container for dependency analysis results.
    /// </summary>
    public class DependencyAnalysisResult
    {
        public string ScriptPath;

        public List<string> UsingNamespaces = new List<string>();
        public List<string> TypeReferences = new List<string>();
        public List<string> GetComponentTypes = new List<string>();
        public List<string> RequireComponentTypes = new List<string>();
        public List<string> EventSubscriptions = new List<string>();
        public List<string> AttributeTypes = new List<string>();
    }
}