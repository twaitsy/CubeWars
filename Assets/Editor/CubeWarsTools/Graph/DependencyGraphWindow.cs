using UnityEditor;
using UnityEngine;

namespace CubeWarsTools.Graph
{
    public class DependencyGraphWindow : EditorWindow
    {
        private DependencyGraphView graphView;

        [MenuItem("Tools/CubeWars/Dependency Graph")]
        public static void Open()
        {
            var window = GetWindow<DependencyGraphWindow>();
            window.titleContent = new GUIContent("Dependency Graph");
            window.minSize = new Vector2(1200, 700);
            window.Show();
        }

        private void OnEnable()
        {
            ConstructGraphView();
        }

        private void OnDisable()
        {
            if (graphView != null)
                rootVisualElement.Remove(graphView);
        }

        private void ConstructGraphView()
        {
            graphView = new DependencyGraphView();

            // Modern replacement for StretchToParentSize()
            graphView.style.flexGrow = 1;

            rootVisualElement.Add(graphView);
        }

        private void OnGUI()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("Refresh Graph", EditorStyles.toolbarButton))
                graphView.BuildGraph();

            if (GUILayout.Button("Auto‑Layout", EditorStyles.toolbarButton))
                graphView.RunAutoLayout();

            if (GUILayout.Button("Scatter", EditorStyles.toolbarButton))
                graphView.ScatterNodes();

            GUILayout.EndHorizontal();
        }
    }
}