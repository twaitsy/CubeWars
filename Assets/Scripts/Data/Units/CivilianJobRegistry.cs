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
        public CivilianRole legacyRole;
        public CivilianWorkDomain domain;
        public bool supportsCraftingAssignment;
    }

    public static JobProfile GetProfile(CivilianJobType jobType)
    {
        switch (jobType)
        {
            case CivilianJobType.Gatherer:
                return New(jobType, CivilianRole.Gatherer, CivilianWorkDomain.Gathering);
            case CivilianJobType.Builder:
                return New(jobType, CivilianRole.Builder, CivilianWorkDomain.Building);
            case CivilianJobType.Hauler:
                return New(jobType, CivilianRole.Hauler, CivilianWorkDomain.Hauling);
            case CivilianJobType.Crafter:
            case CivilianJobType.Farmer:
            case CivilianJobType.Technician:
            case CivilianJobType.Scientist:
            case CivilianJobType.Engineer:
            case CivilianJobType.Blacksmith:
            case CivilianJobType.Carpenter:
            case CivilianJobType.Cook:
                return New(jobType, CivilianRole.Crafter, CivilianWorkDomain.Crafting, true);
            case CivilianJobType.Idle:
                return New(jobType, CivilianRole.Idle, CivilianWorkDomain.Idle);
            default:
                return New(CivilianJobType.Generalist, CivilianRole.Gatherer, CivilianWorkDomain.Gathering, true);
        }
    }

    public static CivilianJobType ToJobType(CivilianRole legacyRole)
    {
        switch (legacyRole)
        {
            case CivilianRole.Builder: return CivilianJobType.Builder;
            case CivilianRole.Hauler: return CivilianJobType.Hauler;
            case CivilianRole.Idle: return CivilianJobType.Idle;
            case CivilianRole.Crafter: return CivilianJobType.Crafter;
            case CivilianRole.Farmer: return CivilianJobType.Farmer;
            case CivilianRole.Technician: return CivilianJobType.Technician;
            case CivilianRole.Scientist: return CivilianJobType.Scientist;
            case CivilianRole.Engineer: return CivilianJobType.Engineer;
            case CivilianRole.Blacksmith: return CivilianJobType.Blacksmith;
            case CivilianRole.Carpenter: return CivilianJobType.Carpenter;
            case CivilianRole.Cook: return CivilianJobType.Cook;
            default: return CivilianJobType.Gatherer;
        }
    }

    static JobProfile New(CivilianJobType jobType, CivilianRole legacyRole, CivilianWorkDomain domain, bool supportsCraftingAssignment = false)
    {
        return new JobProfile
        {
            jobType = jobType,
            legacyRole = legacyRole,
            domain = domain,
            supportsCraftingAssignment = supportsCraftingAssignment
        };
    }
}
