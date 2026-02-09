using UnityEngine;

public class UnitNameAssigner : MonoBehaviour
{
    public static readonly string[] NamePool =
    {
        "Alex", "Jules", "Mara", "Nova", "Orin", "Kade", "Sera", "Rhea", "Voss", "Iris",
        "Dax", "Nia", "Theo", "Lena", "Zane", "Milo", "Skye", "Aria", "Rune", "Pax"
    };

    private static int nextId = 1;

    [SerializeField] private string unitClass = "Worker";

    void Awake()
    {
        AssignName();
    }

    public void AssignName()
    {
        if (NamePool.Length == 0) return;

        int idx = Random.Range(0, NamePool.Length);
        int id = nextId++;
        gameObject.name = $"{NamePool[idx]} {unitClass} #{id}";
    }
}
