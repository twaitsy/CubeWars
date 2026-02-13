# Analysis — BuildMenuUI.cs
Generated: 2026-02-14

## Purpose
`BuildMenuUI` provides the IMGUI-based build catalog UI and now supports both legacy `BuildItemDefinition` lists and the newer detailed `BuildingDefinition` format from `BuildingsDatabase`.

## Key Runtime Flows
- Toggle and visibility control via `SetVisible`, `ToggleVisible`, and `Update` keybind checks.
- Catalog build path:
  1. Resolve `BuildingsDatabase`.
  2. Prefer detailed `buildings` entries.
  3. Fallback to legacy `items` entries.
  4. Final fallback to Resources discovery.
- Detailed entries are converted at runtime into temporary `BuildItemDefinition` instances so existing placement logic remains unchanged.

## Important Methods
- `DiscoverFromDatabase`:
  - Builds runtime entries from `BuildingsDatabase`.
  - Maintains mapping from generated build items back to their source `BuildingDefinition`.
- `BuildRuntimeEntries`:
  - Prioritizes `buildings` data.
  - Falls back to `items` only when no valid detailed entries exist.
- `CreateBuildItemFromDetailed`:
  - Copies display/prefab/build-time metadata.
  - Maps `BuildingCategory` to `AIBuildingPriority`.
  - Converts detailed construction costs into `ResourceCost[]`.
- `BuildDetailsString`:
  - Displays additional details in row text (time, HP, workers, storage).
- `ClearRuntimeGeneratedItems`:
  - Destroys temporary ScriptableObjects on rebuild/destroy to avoid leaks.

## Dependencies
- `BuildingsDatabase`, `BuildingDefinition`, `BuildItemDefinition`
- `BuildPlacementManager`
- `TeamStorageManager`, `TeamResources`
- `GameDatabase`

## Notes
- Resource conversion relies on resource IDs/display names matching `ResourceType` enum tokens (normalizing spaces, `_`, and `-`).
- Detailed-format rows enrich UI text without changing placement APIs.
