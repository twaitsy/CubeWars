using UnityEngine;

public class ToolController : MonoBehaviour
{
    [SerializeField] private ToolDefinition[] startingTools;

    public void SetStartingTools(ToolDefinition[] tools)
    {
        startingTools = tools;
    }
}