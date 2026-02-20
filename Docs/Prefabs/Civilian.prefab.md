# Civilian.prefab
- **Path:** `Assets/Prefabs/Civilian.prefab`
- **Gameplay role:** Worker unit for gathering, hauling, and construction tasks.
- **Referenced by scripts:** `Civilian`, `JobManager`, `UnitInspectorUI` civilian role controls, spawner systems.
- **Required components (minimum):** `Civilian` (auto-enforces Health/Movement/Carrying/Gathering/Needs/Housing/ConstructionWorkerControl/CivilianStateMachineController/ToolController/JobController/TrainingController/UpgradeController), movement/selection components, collider.
- **Needs integration:** `NeedsController` reads `food/rest/water` rates from `NeedsDatabase` and state-priority actions from `NeedsActionDatabase` via `GameDatabaseLoader`.
- **Behavior notes:** civilians now reduce movement speed at high tiredness and can be interrupted into need states (`SeekingFoodStorage`, `SeekingHouse`) by need-action rules.
- **Notes:** Inspector role buttons rely on this prefab exposing `Civilian` behavior.
