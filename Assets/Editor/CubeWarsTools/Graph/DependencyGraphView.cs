using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using CubeWarsTools.Analysis;

namespace CubeWarsTools.Graph
{
    public class DependencyGraphView : GraphView
    {
        private readonly string ScriptsRoot = "Assets/Scripts";
        private readonly string AnalysisRoot = "Assets/Docs/Analysis";

        private Dictionary<string, ScriptAnalysisResult> scriptAnalysis;
        private Dictionary<string, DependencyAnalysisResult> dependencyAnalysis;

        // Avoids hiding GraphView.nodes
        private Dictionary<string, DependencyNode> nodeLookup =
            new Dictionary<string, DependencyNode>();

        public DependencyGraphView()
        {
            style.flexGrow = 1;

            Insert(0, new GridBackground());

            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            BuildGraph();
        }

        // ---------------------------------------------------------
        // BUILD GRAPH
        // ---------------------------------------------------------

        public void BuildGraph()
        {
            ClearGraph();

            LoadAnalysisData();
            CreateNodes();
            CreateEdges();

            RunAutoLayout();
        }

        private void ClearGraph()
        {
            graphElements.ForEach(RemoveElement);
            nodeLookup.Clear();
        }

        private void LoadAnalysisData()
        {
            scriptAnalysis = new Dictionary<string, ScriptAnalysisResult>();
            dependencyAnalysis = new Dictionary<string, DependencyAnalysisResult>();

            List<string> scripts = Directory.GetFiles(ScriptsRoot, "*.cs", SearchOption.AllDirectories)
                                            .Select(p => p.Replace("\\", "/"))
                                            .ToList();

            foreach (string scriptPath in scripts)
            {
                string relative = scriptPath.Replace(ScriptsRoot, "").TrimStart('/');
                string folder = Path.GetDirectoryName(relative).Replace("\\", "/");
                string scriptName = Path.GetFileNameWithoutExtension(scriptPath);

                string analysisPath = Path.Combine(AnalysisRoot, folder, scriptName + "_Analysis.md")
                                      .Replace("\\", "/");

                if (File.Exists(analysisPath))
                {
                    ScriptAnalysisResult s = ParseScriptAnalysis(analysisPath);
                    DependencyAnalysisResult d = ParseDependencyAnalysis(analysisPath);

                    scriptAnalysis[scriptPath] = s;
                    dependencyAnalysis[scriptPath] = d;
                }
            }
        }

        // ---------------------------------------------------------
        // NODE CREATION (with delayed scatter)
        // ---------------------------------------------------------

        private void CreateNodes()
        {
            int index = 0;

            foreach (var kvp in scriptAnalysis)
            {
                string scriptPath = kvp.Key;
                ScriptAnalysisResult data = kvp.Value;

                var node = new DependencyNode(scriptPath, data);
                nodeLookup[scriptPath] = node;

                AddElement(node);

                // Delay position assignment until after GraphView layout
                int capturedIndex = index;
                node.schedule.Execute(() =>
                {
                    float angle = capturedIndex * 0.5f;
                    float radius = 300f + (capturedIndex * 5f);

                    Vector2 pos = new Vector2(
                        Mathf.Cos(angle) * radius,
                        Mathf.Sin(angle) * radius
                    );

                    node.SetPosition(new Rect(pos, new Vector2(220, 140)));
                });

                index++;
            }
        }

        // ---------------------------------------------------------
        // EDGE CREATION
        // ---------------------------------------------------------

        private void CreateEdges()
        {
            foreach (var kvp in dependencyAnalysis)
            {
                string scriptPath = kvp.Key;
                DependencyAnalysisResult deps = kvp.Value;

                if (!nodeLookup.ContainsKey(scriptPath))
                    continue;

                DependencyNode fromNode = nodeLookup[scriptPath];

                foreach (string type in deps.TypeReferences)
                    TryCreateEdge(fromNode, type, Color.cyan);

                foreach (string type in deps.GetComponentTypes)
                    TryCreateEdge(fromNode, type, Color.green);

                foreach (string type in deps.RequireComponentTypes)
                    TryCreateEdge(fromNode, type, Color.yellow);
            }
        }

        private void TryCreateEdge(DependencyNode fromNode, string typeName, Color color)
        {
            string targetPath = nodeLookup.Keys
                .FirstOrDefault(p => Path.GetFileNameWithoutExtension(p) == typeName);

            if (string.IsNullOrEmpty(targetPath))
                return;

            DependencyNode toNode = nodeLookup[targetPath];

            var edge = new DependencyEdge(fromNode.output, toNode.input, color);
            AddElement(edge);
        }

        // ---------------------------------------------------------
        // AUTO LAYOUT (force-directed)
        // ---------------------------------------------------------

        public void RunAutoLayout()
        {
            const float repulsion = 5000f;
            const float attraction = 0.02f;
            const float damping = 0.85f;

            Dictionary<DependencyNode, Vector2> velocity =
                nodeLookup.Values.ToDictionary(n => n, n => Vector2.zero);

            for (int iteration = 0; iteration < 80; iteration++)
            {
                foreach (var a in nodeLookup.Values)
                {
                    foreach (var b in nodeLookup.Values)
                    {
                        if (a == b) continue;

                        Vector2 delta = a.GetPosition().position - b.GetPosition().position;
                        float dist = Mathf.Max(50f, delta.magnitude);

                        Vector2 force = delta.normalized * (repulsion / (dist * dist));
                        velocity[a] += force;
                    }
                }

                foreach (var edge in edges.ToList())
                {
                    if (edge is DependencyEdge depEdge)
                    {
                        var a = depEdge.fromNode;
                        var b = depEdge.toNode;

                        Vector2 delta = b.GetPosition().position - a.GetPosition().position;
                        Vector2 force = delta * attraction;

                        velocity[a] += force;
                        velocity[b] -= force;
                    }
                }

                foreach (var node in nodeLookup.Values)
                {
                    Vector2 pos = node.GetPosition().position;
                    pos += velocity[node] * 0.02f;
                    velocity[node] *= damping;

                    node.SetPosition(new Rect(pos, node.GetPosition().size));
                }
            }
        }

        // ---------------------------------------------------------
        // SCATTER NODES (manual)
        // ---------------------------------------------------------

        public void ScatterNodes()
        {
            int index = 0;

            foreach (var node in nodeLookup.Values)
            {
                float angle = index * 0.5f;
                float radius = 300f + (index * 5f);

                Vector2 pos = new Vector2(
                    Mathf.Cos(angle) * radius,
                    Mathf.Sin(angle) * radius
                );

                node.SetPosition(new Rect(pos, node.GetPosition().size));
                index++;
            }
        }

        // ---------------------------------------------------------
        // PARSING
        // ---------------------------------------------------------

        private ScriptAnalysisResult ParseScriptAnalysis(string path)
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

        private DependencyAnalysisResult ParseDependencyAnalysis(string path)
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
    }
}