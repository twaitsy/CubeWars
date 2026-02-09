using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using CubeWarsTools.Analysis;
using System.IO;

namespace CubeWarsTools.Graph
{
    /// <summary>
    /// A single script node in the dependency graph.
    /// Clean technical style with:
    /// - Title (script name)
    /// - Subtitle (namespace)
    /// - Input port
    /// - Output port
    /// - Click actions
    /// </summary>
    public class DependencyNode : Node
    {
        public string ScriptPath { get; private set; }
        public ScriptAnalysisResult Analysis { get; private set; }

        public Port input;
        public Port output;

        public DependencyNode(string scriptPath, ScriptAnalysisResult analysis)
        {
            ScriptPath = scriptPath;
            Analysis = analysis;

            title = Path.GetFileNameWithoutExtension(scriptPath);

            // Clean technical style
            style.width = 220;
            style.minHeight = 120;
            style.borderBottomWidth = 1;
            style.borderTopWidth = 1;
            style.borderLeftWidth = 1;
            style.borderRightWidth = 1;
            style.borderBottomColor = new Color(0.2f, 0.2f, 0.2f);
            style.borderTopColor = new Color(0.2f, 0.2f, 0.2f);
            style.borderLeftColor = new Color(0.2f, 0.2f, 0.2f);
            style.borderRightColor = new Color(0.2f, 0.2f, 0.2f);
            style.backgroundColor = new Color(0.18f, 0.18f, 0.18f);

            // Subtitle (namespace)
            if (!string.IsNullOrEmpty(analysis.Namespace))
            {
                var nsLabel = new Label(analysis.Namespace)
                {
                    style =
                    {
                        unityFontStyleAndWeight = FontStyle.Italic,
                        fontSize = 11,
                        color = new Color(0.75f, 0.75f, 0.75f),
                        marginLeft = 4,
                        marginBottom = 4
                    }
                };
                mainContainer.Add(nsLabel);
            }

            // Ports
            input = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
            input.portName = "In";
            inputContainer.Add(input);

            output = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));
            output.portName = "Out";
            outputContainer.Add(output);

            // Buttons
            AddOpenButtons();

            RefreshExpandedState();
            RefreshPorts();
        }

        private void AddOpenButtons()
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.marginTop = 6;
            row.style.marginBottom = 4;

            var openScript = new Button(() =>
            {
                Object asset = AssetDatabase.LoadAssetAtPath<Object>(ScriptPath);
                if (asset != null)
                {
                    Selection.activeObject = asset;
                    EditorGUIUtility.PingObject(asset);
                }
            })
            {
                text = "Open Script"
            };
            openScript.style.flexGrow = 1;

            var openAnalysis = new Button(() =>
            {
                string relative = ScriptPath.Replace("Assets/Scripts", "").TrimStart('/');
                string folder = Path.GetDirectoryName(relative).Replace("\\", "/");
                string scriptName = Path.GetFileNameWithoutExtension(ScriptPath);

                string analysisPath = $"Assets/Docs/Analysis/{folder}/{scriptName}_Analysis.md";

                Object asset = AssetDatabase.LoadAssetAtPath<Object>(analysisPath);
                if (asset != null)
                {
                    Selection.activeObject = asset;
                    EditorGUIUtility.PingObject(asset);
                }
            })
            {
                text = "Analysis"
            };
            openAnalysis.style.flexGrow = 1;

            row.Add(openScript);
            row.Add(openAnalysis);

            mainContainer.Add(row);
        }
    }
}