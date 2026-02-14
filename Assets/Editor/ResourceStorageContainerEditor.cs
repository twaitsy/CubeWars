using UnityEditor;

[CustomEditor(typeof(ResourceStorageContainer))]
public class ResourceStorageContainerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
    }
}
