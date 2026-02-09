using UnityEngine;
using UnityEditor;
using System.IO;

namespace CubeWarsTools.Core
{
    public static class ScriptIndexOpener
    {
        private static readonly string MarkdownPath = "Assets/Docs/ScriptIndex.md";
        private static readonly string TextPath = "Assets/Docs/ScriptIndex.txt";

        [MenuItem("Tools/CubeWars/Open Script Index")]
        public static void OpenScriptIndex()
        {
            // Prefer Markdown
            if (File.Exists(MarkdownPath))
            {
                OpenFile(MarkdownPath);
                return;
            }

            // Fallback to text
            if (File.Exists(TextPath))
            {
                OpenFile(TextPath);
                return;
            }

            Debug.LogWarning("No Script Index found. Generate it first via Tools → CubeWars → Generate Script Index.");
        }

        private static void OpenFile(string path)
        {
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);

            if (asset != null)
            {
                Selection.activeObject = asset;
                EditorGUIUtility.PingObject(asset);
                Debug.Log($"Opened Script Index: {path}");
            }
            else
            {
                Debug.LogError($"Failed to open Script Index at: {path}");
            }
        }
    }
}