using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class CivilianStateMachineController : MonoBehaviour
{
    private readonly Dictionary<Civilian.State, Action> stateHandlers = new();

    public void Configure(IReadOnlyDictionary<Civilian.State, Action> handlers)
    {
        stateHandlers.Clear();
        if (handlers == null)
            return;

        foreach (var handler in handlers)
        {
            if (handler.Value != null)
                stateHandlers[handler.Key] = handler.Value;
        }
    }

    public void Tick(Civilian.State state)
    {
        if (stateHandlers.TryGetValue(state, out var handler))
            handler?.Invoke();
    }
}
