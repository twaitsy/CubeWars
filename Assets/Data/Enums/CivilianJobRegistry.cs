using UnityEngine;

public enum CivilianWorkDomain
{
    Idle,
    Gathering,
    Building,
    Hauling,
    Crafting
}

public static class CivilianJobRegistry
{
    public struct JobProfile
    {
        public CivilianJobType jobType;
        public CivilianWorkDomain domain;
        public bool supportsCraftingAssignment;
    }

    public static JobProfile GetProfile(CivilianJobType jobType)
    {
        switch (jobType)
        {
            case CivilianJobType.Gatherer:
                return New(jobType, CivilianWorkDomain.Gathering);
            case CivilianJobType.Builder:
                return New(jobType, CivilianWorkDomain.Building);
            case CivilianJobType.Hauler:
                return New(jobType, CivilianWorkDomain.Hauling);
            case CivilianJobType.Crafter:
            case CivilianJobType.Farmer:
            case CivilianJobType.Technician:
            case CivilianJobType.Scientist:
            case CivilianJobType.Engineer:
            case CivilianJobType.Blacksmith:
            case CivilianJobType.Carpenter:
            case CivilianJobType.Cook:
                return New(jobType, CivilianWorkDomain.Crafting, true);
            case CivilianJobType.Idle:
                return New(jobType, CivilianWorkDomain.Idle);
            default:
                return New(CivilianJobType.Generalist, CivilianWorkDomain.Gathering, true);
        }
    }

    static JobProfile New(CivilianJobType jobType, CivilianWorkDomain domain, bool supportsCraftingAssignment = false)
    {
        return new JobProfile
        {
            jobType = jobType,
            domain = domain,
            supportsCraftingAssignment = supportsCraftingAssignment
        };
    }
}
