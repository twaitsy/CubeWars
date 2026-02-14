using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace CubeWarsTools.Graph
{
    public class DependencyEdge : Edge
    {
        public DependencyNode fromNode;
        public DependencyNode toNode;

        public Color edgeColor;

        public DependencyEdge(Port output, Port input, Color color)
        {
            this.output = output;
            this.input = input;

            fromNode = output.node as DependencyNode;
            toNode = input.node as DependencyNode;

            edgeColor = color;

            tooltip = $"Dependency: {fromNode.title} → {toNode.title}";

            capabilities &= ~Capabilities.Selectable;
            capabilities &= ~Capabilities.Deletable;
            capabilities &= ~Capabilities.Movable;

            output.Connect(this);
            input.Connect(this);

            schedule.Execute(() =>
            {
                if (edgeControl != null)
                {
                    edgeControl.inputColor = edgeColor;
                    edgeControl.outputColor = edgeColor;
                }
            });
        }
    }
}