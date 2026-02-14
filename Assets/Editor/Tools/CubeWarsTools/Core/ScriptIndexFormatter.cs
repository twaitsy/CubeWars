using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CubeWarsTools.Core
{
    public static class ScriptIndexFormatter
    {
        private static readonly string Timestamp =
            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        // ---------------------------
        // MARKDOWN FORMATTERS
        // ---------------------------

        public static string FormatMarkdown(
            Dictionary<string, Dictionary<string, List<string>>> systemGroups)
        {
            var sb = new StringBuilder();

            sb.AppendLine("# Cube Wars Script Index");
            sb.AppendLine($"Generated: {Timestamp}");
            sb.AppendLine();

            foreach (var group in systemGroups.OrderBy(g => g.Key))
            {
                sb.AppendLine($"## {group.Key}");
                sb.AppendLine();

                foreach (var folder in group.Value.OrderBy(f => f.Key))
                {
                    sb.AppendLine($"### {folder.Key}");
                    foreach (var file in folder.Value.OrderBy(f => f))
                        sb.AppendLine($"- {file}");

                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

        public static string FormatMarkdownForGroup(
            string groupName,
            Dictionary<string, List<string>> folders)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"# Cube Wars Script Index — {groupName}");
            sb.AppendLine($"Generated: {Timestamp}");
            sb.AppendLine();

            foreach (var folder in folders.OrderBy(f => f.Key))
            {
                sb.AppendLine($"## {folder.Key}");
                foreach (var file in folder.Value.OrderBy(f => f))
                    sb.AppendLine($"- {file}");

                sb.AppendLine();
            }

            return sb.ToString();
        }

        // ---------------------------
        // TEXT FORMATTERS
        // ---------------------------

        public static string FormatText(
            Dictionary<string, Dictionary<string, List<string>>> systemGroups)
        {
            var sb = new StringBuilder();

            sb.AppendLine("=== CUBE WARS SCRIPT INDEX ===");
            sb.AppendLine($"Generated: {Timestamp}");
            sb.AppendLine();

            foreach (var group in systemGroups.OrderBy(g => g.Key))
            {
                sb.AppendLine(group.Key.ToUpper());
                sb.AppendLine();

                foreach (var folder in group.Value.OrderBy(f => f.Key))
                {
                    sb.AppendLine(folder.Key);
                    foreach (var file in folder.Value.OrderBy(f => f))
                        sb.AppendLine($"    {file}");

                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

        public static string FormatTextForGroup(
            string groupName,
            Dictionary<string, List<string>> folders)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"=== CUBE WARS SCRIPT INDEX — {groupName.ToUpper()} ===");
            sb.AppendLine($"Generated: {Timestamp}");
            sb.AppendLine();

            foreach (var folder in folders.OrderBy(f => f.Key))
            {
                sb.AppendLine(folder.Key);
                foreach (var file in folder.Value.OrderBy(f => f))
                    sb.AppendLine($"    {file}");

                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}