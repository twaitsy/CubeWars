using System.Collections.Generic;
using UnityEngine;

public class NeedsController : MonoBehaviour
{
    [SerializeField] private List<NeedDefinition> startingNeeds = new();
    private readonly List<NeedInstance> activeNeeds = new();

    public void SetNeeds(List<NeedDefinition> defs)
    {
        startingNeeds = defs;
        InitializeNeeds();
    }

    private void InitializeNeeds()
    {
        activeNeeds.Clear();

        foreach (var def in startingNeeds)
        {
            if (def == null) continue;

            activeNeeds.Add(new NeedInstance
            {
                definition = def,
                currentValue = def.maxValue
            });
        }
    }

    void Update()
    {
        float dt = Time.deltaTime;

        foreach (var need in activeNeeds)
        {
            need.Tick(dt);

            if (need.definition.behaviour != null)
                need.definition.behaviour.Tick(this, need);
        }
    }
}