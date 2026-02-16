using UnityEngine;

public abstract class NeedBehaviour : ScriptableObject
{
    public abstract void Tick(NeedsController controller, NeedInstance need);
}