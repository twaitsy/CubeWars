// =============================================================
// CivilianRole.cs
//
// DEPENDENCIES:
// - Civilian: uses this to decide behaviour/state machine
// - JobManager: counts civilians per role
// - TaskBoardUI: displays counts per role, including Idle
//
// NOTES FOR FUTURE MAINTENANCE:
// - If you add new roles, update Civilian.SetRole(), TaskBoardUI, and any AI/job logic.
// =============================================================

public enum CivilianRole
{
    Gatherer,
    Builder,
    Hauler,
    Crafter,
    Farmer,
    Technician,
    Scientist,
    Engineer,
    Blacksmith,
    Carpenter,
    Cook,
    Idle
}