using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ResourceStorageContainer))]
public class ResourceStorageContainerEditor : Editor
{
    SerializedProperty teamIdProp;
    SerializedProperty resourceFlowProp;

    void OnEnable()
    {
        teamIdProp = serializedObject.FindProperty("teamID");
        resourceFlowProp = serializedObject.FindProperty("resourceFlow");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(teamIdProp);
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Per-Resource Hauler Permissions", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Configure if each resource can be picked up (Supply), dropped off (Receive), both, or disabled for this storage.", MessageType.Info);

        foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
            DrawResourceFlowRow(type);

        serializedObject.ApplyModifiedProperties();
    }

    void DrawResourceFlowRow(ResourceType type)
    {
        int index = FindOrCreateEntry(type);
        if (index < 0) return;

        SerializedProperty entry = resourceFlowProp.GetArrayElementAtIndex(index);
        SerializedProperty modeProp = entry.FindPropertyRelative("flowMode");

        string label = type + " Flow (Hauler Pickup/Dropoff)";
        modeProp.enumValueIndex = (int)(ResourceStorageContainer.ResourceFlowMode)
            EditorGUILayout.EnumPopup(new GUIContent(label), (ResourceStorageContainer.ResourceFlowMode)modeProp.enumValueIndex);
    }

    int FindOrCreateEntry(ResourceType type)
    {
        for (int i = 0; i < resourceFlowProp.arraySize; i++)
        {
            SerializedProperty entry = resourceFlowProp.GetArrayElementAtIndex(i);
            SerializedProperty typeProp = entry.FindPropertyRelative("type");
            if (typeProp.enumValueIndex == (int)type)
                return i;
        }

        int newIndex = resourceFlowProp.arraySize;
        resourceFlowProp.InsertArrayElementAtIndex(newIndex);
        SerializedProperty newEntry = resourceFlowProp.GetArrayElementAtIndex(newIndex);
        newEntry.FindPropertyRelative("type").enumValueIndex = (int)type;
        newEntry.FindPropertyRelative("flowMode").enumValueIndex = (int)ResourceStorageContainer.ResourceFlowMode.ReceiveAndSupply;
        return newIndex;
    }
}
