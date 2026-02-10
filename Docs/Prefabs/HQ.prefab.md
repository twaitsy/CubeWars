# HQ.prefab
- **Path:** `Assets/Prefabs/HQ.prefab`
- **Gameplay role:** Core base structure and anchor for team build-grid generation.
- **Referenced by scripts:** `BuildGridManager` (HQ discovery for grid creation), `Headquarters`/`Building` systems, `UnitInspectorUI` diplomacy section.
- **Required components (minimum):** `Headquarters`, `Building`, collider, visual renderers.
- **Notes:** Destroying HQ impacts team expansion options because grid generation is centered around existing HQ instances.
