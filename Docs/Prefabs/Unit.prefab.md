# Unit.prefab
- **Path:** `Assets/Prefabs/Unit.prefab`
- **Gameplay role:** Generic combat unit produced by military buildings.
- **Referenced by scripts:** `Unit`, `UnitCombatController`, command/selection systems, inspector unit/combat sections.
- **Required components (minimum):** `Unit` (auto-enforces `NavMeshAgent` + `UnitCombatController`), command controller, selectable/collider.
- **Notes:** Team assignment and combat stance controls depend on expected unit components.
