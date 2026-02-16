using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CubeWars/Database/Needs")]
public class NeedsDatabase : ScriptableObject
{
    [SerializeField]
    private List<NeedDefinition> needs = new();

    private Dictionary<string, NeedDefinition> _byId;

    void OnEnable()
    {
        BuildLookup();
    }

    void BuildLookup()
    {
        _byId = new Dictionary<string, NeedDefinition>();

        foreach (var need in needs)
        {
            if (need == null || string.IsNullOrWhiteSpace(need.id))
                continue;

            var key = need.id.Trim().ToLowerInvariant();
            _byId[key] = need;
        }
    }

    public bool TryGetById(string id, out NeedDefinition def)
    {
        def = null;
        if (string.IsNullOrWhiteSpace(id) || _byId == null)
            return false;

        var key = id.Trim().ToLowerInvariant();
        return _byId.TryGetValue(key, out def);
    }

    public IReadOnlyList<NeedDefinition> AllNeeds => needs;
}