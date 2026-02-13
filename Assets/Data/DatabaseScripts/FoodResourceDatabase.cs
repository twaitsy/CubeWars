using UnityEngine;

[System.Serializable]
public struct FoodResourceEntry
{
    public ResourceType type;
    [Min(1)] public int hungerRestore;
    [Tooltip("Higher value means civilians prefer this food when available.")]
    public int ranking;
}

[CreateAssetMenu(menuName = "CubeWars/Food Resource Database", fileName = "FoodResourceDatabase")]
public class FoodResourceDatabase : ScriptableObject
{
    public FoodResourceEntry[] foods = new FoodResourceEntry[]
    {
        new FoodResourceEntry { type = ResourceType.Food, hungerRestore = 2, ranking = 10 },
        new FoodResourceEntry { type = ResourceType.Flour, hungerRestore = 1, ranking = 5 },
        new FoodResourceEntry { type = ResourceType.Bread, hungerRestore = 4, ranking = 30 }
    };

    static readonly FoodResourceEntry[] fallbackFoods = new FoodResourceEntry[]
    {
        new FoodResourceEntry { type = ResourceType.Food, hungerRestore = 2, ranking = 10 },
        new FoodResourceEntry { type = ResourceType.Flour, hungerRestore = 1, ranking = 5 },
        new FoodResourceEntry { type = ResourceType.Bread, hungerRestore = 4, ranking = 30 }
    };

    static FoodResourceEntry[] Entries(FoodResourceDatabase db)
    {
        if (db != null && db.foods != null && db.foods.Length > 0)
            return db.foods;

        return fallbackFoods;
    }

    public static bool IsFood(FoodResourceDatabase db, ResourceType type)
    {
        var entries = Entries(db);
        for (int i = 0; i < entries.Length; i++)
        {
            if (entries[i].type == type)
                return true;
        }

        return false;
    }

    public static int GetHungerRestore(FoodResourceDatabase db, ResourceType type)
    {
        var entries = Entries(db);
        for (int i = 0; i < entries.Length; i++)
        {
            if (entries[i].type == type)
                return Mathf.Max(1, entries[i].hungerRestore);
        }

        return 1;
    }

    public static int GetRanking(FoodResourceDatabase db, ResourceType type)
    {
        var entries = Entries(db);
        for (int i = 0; i < entries.Length; i++)
        {
            if (entries[i].type == type)
                return entries[i].ranking;
        }

        return 0;
    }
}
