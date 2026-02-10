# Barracks.prefab
- **Path:** `Assets/Prefabs/Barracks.prefab`
- **Gameplay role:** Produces combat units through the production queue.
- **Referenced by scripts:** `UnitInspectorUI` (queue/training controls), `Barracks` runtime logic, placement and construction systems.
- **Required components (minimum):** `Barracks` (inherits `Building`), `UnitProductionQueue` dependencies, collider.
- **Notes:** Must remain selectable so inspector controls can trigger training and queue cancellation.
