using System.Collections.Generic;
using UnityEngine;

public class HousingRegistry : MonoBehaviour
{
    public static HousingRegistry Instance;

    readonly List<House> houses = new();
    readonly Dictionary<Civilian, House> assignedHousing = new();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void RegisterHouse(House house)
    {
        if (house != null && !houses.Contains(house))
            houses.Add(house);
    }

    public void UnregisterHouse(House house)
    {
        if (house == null)
            return;

        houses.Remove(house);

        var toClear = new List<Civilian>();
        foreach (var kv in assignedHousing)
            if (kv.Value == house)
                toClear.Add(kv.Key);

        for (int i = 0; i < toClear.Count; i++)
            assignedHousing.Remove(toClear[i]);
    }

    public bool TryGetAssignedHouse(Civilian civilian, out House house)
    {
        return assignedHousing.TryGetValue(civilian, out house) && house != null && house.IsAlive;
    }

    public bool TryAssignHouse(Civilian civilian, int teamID, Vector3 fromPosition, out House house)
    {
        house = null;
        if (civilian == null)
            return false;

        if (TryGetAssignedHouse(civilian, out house) && house.CanAcceptResident(civilian))
            return true;

        house = FindClosestAvailableHouse(teamID, civilian, fromPosition);
        if (house == null || !house.TryAddResident(civilian))
            return false;

        assignedHousing[civilian] = house;
        return true;
    }

    public House FindClosestAvailableHouse(int teamID, Civilian civilian, Vector3 fromPosition)
    {
        House best = null;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < houses.Count; i++)
        {
            House house = houses[i];
            if (house == null || !house.IsAlive || house.teamID != teamID)
                continue;

            if (!house.CanAcceptResident(civilian))
                continue;

            float sqrDistance = (house.transform.position - fromPosition).sqrMagnitude;
            if (sqrDistance < bestDistance)
            {
                bestDistance = sqrDistance;
                best = house;
            }
        }

        return best;
    }

    public void ClearAssignment(Civilian civilian)
    {
        if (civilian == null)
            return;

        if (assignedHousing.TryGetValue(civilian, out House house) && house != null)
            house.RemoveResident(civilian);

        assignedHousing.Remove(civilian);
    }
}
