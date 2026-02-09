using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CubeWarsTools.Analysis
{
    /// <summary>
    /// Full Phase 2 parser.
    /// Extracts classes, methods, fields, properties, events, attributes, and namespaces.
    /// Designed to handle mixed formatting styles (AI-generated, human-written, etc.).
    /// </summary>
    public static class ClassMethodExtractor
    {
        // -------------------------
        // REGEX PATTERNS
        // -------------------------

        private static readonly Regex NamespaceRegex =
            new Regex(@"namespace\s+([A-Za-z0-9_.]+)", RegexOptions.Compiled);

        private static readonly Regex ClassRegex =
            new Regex(@"class\s+([A-Za-z0-9_]+)(?:\s*:\s*([A-Za-z0-9_<>,\s]+))?",
                      RegexOptions.Compiled);

        private static readonly Regex InterfaceRegex =
            new Regex(@"interface\s+([A-Za-z0-9_]+)", RegexOptions.Compiled);

        private static readonly Regex MethodRegex =
            new Regex(@"(?:public|private|protected|internal)\s+(?:static\s+)?(?:async\s+)?[A-Za-z0-9_<>,\[\]]+\s+([A-Za-z0-9_]+)\s*\(",
                      RegexOptions.Compiled);

        private static readonly Regex PropertyRegex =
            new Regex(@"(?:public|private|protected|internal)\s+[A-Za-z0-9_<>,\[\]]+\s+([A-Za-z0-9_]+)\s*\{\s*(?:get|set)",
                      RegexOptions.Compiled);

        private static readonly Regex EventRegex =
            new Regex(@"event\s+[A-Za-z0-9_<>,\[\]]+\s+([A-Za-z0-9_]+)",
                      RegexOptions.Compiled);

        private static readonly Regex FieldRegex =
            new Regex(@"(?:public|private|protected|internal)\s+(?:static\s+)?[A-Za-z0-9_<>,\[\]]+\s+([A-Za-z0-9_]+)\s*;",
                      RegexOptions.Compiled);

        private static readonly Regex SerializedFieldRegex =
            new Regex(@"\[SerializeField\][\s\r\n]*?(?:public|private|protected|internal)?\s*(?:static\s+)?[A-Za-z0-9_<>,\[\]]+\s+([A-Za-z0-9_]+)",
                      RegexOptions.Compiled);

        private static readonly Regex AttributeRegex =
            new Regex(@"\[([A-Za-z0-9_]+)(?:\(.+?\))?\]",
                      RegexOptions.Compiled);

        // -------------------------
        // MAIN ENTRY POINT
        // -------------------------

        public static ScriptAnalysisResult AnalyzeScript(string scriptPath)
        {
            if (!File.Exists(scriptPath))
            {
                Debug.LogWarning($"ClassMethodExtractor: File not found: {scriptPath}");
                return new ScriptAnalysisResult();
            }

            string code = File.ReadAllText(scriptPath);

            ScriptAnalysisResult result = new ScriptAnalysisResult();
            result.ScriptPath = scriptPath;
            result.Namespace = ExtractSingle(NamespaceRegex, code);
            result.ClassNames = ExtractMatches(ClassRegex, code);
            result.Interfaces = ExtractMatches(InterfaceRegex, code);
            result.Methods = ExtractMatches(MethodRegex, code);
            result.Properties = ExtractMatches(PropertyRegex, code);
            result.Events = ExtractMatches(EventRegex, code);
            result.Fields = ExtractMatches(FieldRegex, code);
            result.SerializedFields = ExtractMatches(SerializedFieldRegex, code);
            result.Attributes = ExtractMatches(AttributeRegex, code);
            result.BaseClasses = ExtractBaseClasses(code);

            return result;
        }

        // -------------------------
        // HELPERS
        // -------------------------

        private static string ExtractSingle(Regex regex, string input)
        {
            Match match = regex.Match(input);
            if (match.Success && match.Groups.Count > 1)
                return match.Groups[1].Value;
            return string.Empty;
        }

        private static List<string> ExtractMatches(Regex regex, string input)
        {
            List<string> list = new List<string>();
            MatchCollection matches = regex.Matches(input);

            foreach (Match match in matches)
            {
                if (match.Groups.Count > 1)
                    list.Add(match.Groups[1].Value);
            }

            return list;
        }

        private static List<string> ExtractBaseClasses(string code)
        {
            List<string> list = new List<string>();
            MatchCollection matches = ClassRegex.Matches(code);

            foreach (Match match in matches)
            {
                if (match.Groups.Count > 2 && !string.IsNullOrWhiteSpace(match.Groups[2].Value))
                {
                    string inheritance = match.Groups[2].Value;
                    string[] parts = inheritance.Split(',');

                    foreach (string p in parts)
                        list.Add(p.Trim());
                }
            }

            return list;
        }
    }

    /// <summary>
    /// Data container for script analysis results.
    /// </summary>
    public class ScriptAnalysisResult
    {
        public string ScriptPath;

        public string Namespace;

        public List<string> ClassNames = new List<string>();
        public List<string> BaseClasses = new List<string>();
        public List<string> Interfaces = new List<string>();

        public List<string> Methods = new List<string>();
        public List<string> Properties = new List<string>();
        public List<string> Events = new List<string>();

        public List<string> Fields = new List<string>();
        public List<string> SerializedFields = new List<string>();

        public List<string> Attributes = new List<string>();
    }
}