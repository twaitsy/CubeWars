using UnityEngine;

public static class UnitNamePool
{
    static readonly string[] firstNames =
    {
        "Alex", "Riley", "Jordan", "Casey", "Sam", "Taylor", "Morgan", "Avery", "Rowan", "Kai"
    };

    static readonly string[] lastNames =
    {
        "Stone", "Rivers", "Vale", "Hart", "Quill", "Bennett", "Sloan", "Warden", "Keene", "Dawson"
    };

    public static string GetRandomDisplayName()
    {
        string first = firstNames[Random.Range(0, firstNames.Length)];
        string last = lastNames[Random.Range(0, lastNames.Length)];
        return first + " " + last;
    }
}
