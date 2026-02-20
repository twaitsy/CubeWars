using System.Collections.Generic;

public static class CivilianRegistry
{
    private static readonly HashSet<Civilian> Civilians = new();

    public static IReadOnlyCollection<Civilian> All => Civilians;

    public static List<Civilian> GetAll()
    {
        return new List<Civilian>(Civilians);
    }

    public static void Register(Civilian civilian)
    {
        if (civilian == null)
            return;

        Civilians.Add(civilian);
    }

    public static void Unregister(Civilian civilian)
    {
        if (civilian == null)
            return;

        Civilians.Remove(civilian);
    }
}
